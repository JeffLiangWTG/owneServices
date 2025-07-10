using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Chief.GenericMessagingHarness;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class CustomsExportConsolIntegrationWrapper : NonPersistentBusinessObject, IObsoleteValidation, Integration.Customs.GB.GBChief.IChiefExportConsolIntegrationWrapper
	{
		public static class Schema
		{
			public const string Messages = "Messages";
			public const string HasExportEntryForMessaging = "HasExportEntryForMessaging";
		}

		public CustomsExportConsolIntegrationWrapper(ForwardingConsol consol, ISendsMessagesToCustoms sendsMessagesToCustoms)
			: base(consol.Factory)
		{
			this.ForwardingConsol = consol;
			this.sendsMessagesToCustoms = sendsMessagesToCustoms;

			ForwardingConsol.JK_RL_NKLoadPortInfo.ValueChanged += JK_RL_NKLoadPortInfo_ValueChanged;
			ForwardingConsol.JK_MasterBillNumInfo.ValueChanged += JK_MasterBillNumInfo_ValueChanged;
			ForwardingConsol.JK_OA_SendingForwarderAddressInfo.ValueChanged += JK_OA_SendingForwarderAddressInfo_ValueChanged;
		}

		void JK_OA_SendingForwarderAddressInfo_ValueChanged(object sender, EventArgs e)
		{
			MawbExportHelper.CalculateMUCR(false);
		}

		void JK_MasterBillNumInfo_ValueChanged(object sender, EventArgs e)
		{
			MawbExportHelper.CalculateMUCR(false);
		}

		void JK_RL_NKLoadPortInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ForwardingConsol.IsExport())
			{
				MawbExportHelper.DefaultBadgeCodeFromLoadPort();
			}

			MawbExportHelper.CalculateMUCR(false);
		}

		/// <summary>
		/// For Spring
		/// </summary>
		public CustomsExportConsolIntegrationWrapper()
		{
		}

		public ForwardingConsol ForwardingConsol
		{
			get;
			private set;
		}

		public ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollection Messages
		{
			get { return messages ?? (messages = new ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollection(ForwardingConsol)); }
		}

		public void HandleConsolAddedToOrRemovedFromShipment(CollectionCountChangedEventArgs args, ForwardingShipment shipment)
		{
			if (CanSendMessages)
			{
				PerformEACMessaging(shipment, args, AssociateShipmentsDeclarationToConsol, DisAssociateShipmentsDeclarationFromConsol);
				RecordAuditDetailsOnConsol();
				MawbExportHelper.CalculateCTStatusIfNeeded();
			}
		}

		public void HandleShipmentAddedToOrRemovedFromConsol(CollectionCountChangedEventArgs args)
		{
			if (CanSendMessages)
			{
				var shipment = args.BizObject as ForwardingShipment;
				PerformEACMessaging(shipment, args, AssociateShipmentsDeclarationToConsol, DisAssociateShipmentsDeclarationFromConsol);
				RecordAuditDetailsOnConsol();
				MawbExportHelper.CalculateCTStatusIfNeeded();
			}
		}

		protected bool CanSendMessages
		{
			get
			{
				return ForwardingConsol != null
					&& ForwardingConsol.IsExport()
					&& !ForwardingConsol.IsDomestic()
					&& ForwardingConsol.JK_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)
					&& GBCustomsDataRegistry.Instance.GB_CustomsModuleEnabledForShipmentsAndConsols.Value;
			}
		}

		void RecordAuditDetailsOnConsol()
		{
			ForwardingConsol.JK_SystemLastEditTimeUtc = ZDateTime.Now; // Allows us to compare the last edit date against the last query message date
			ForwardingConsol.JK_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
		}

		void PerformEACMessaging(IConsolOrShipment childObject, CollectionCountChangedEventArgs args, Action<CollectionCountChangedEventArgs, IConsolOrShipment> associateToConsol, Action<CollectionCountChangedEventArgs, IConsolOrShipment> disassociateFromConsol)
		{
			if (childObject.IsInDatabase)
			{
				if (ImportExportHelper.IsBranchCountry(ForwardingConsol.JK_RL_NKLoadPort))
				{
					var shipment = childObject as ForwardingShipment;
					if (shipment != null && shipment.JS_CommunityTransitStatus == ExportCommunityTransitStatusList.Codes.C)
					{
						// C-status does not need an EAC
						ShowWarning("D6518863-EE52-4962-8ABF-B7B952D69D5D", shipment.HumanReadableName + " - no EAC message will be sent for this C-status shipment", "C-Status shipment");
						return;
					}
					if (!MawbExportHelper.ME_MasterUCR.IsEmpty && !MawbExportHelper.ME_Profile.IsEmpty)
					{
						if (args.ItemAdded)
						{
							var consolNotKnownToBeClosed = MawbExportHelper.ME_ChiefConsolIsClosed ? "" : "\r\nWarning: the master consol is not known to be closed.";
							if (ShowYesNo("7CE67EFC-0990-4F5F-95D4-DCDB5C905E2A", GetSendEachPopupMessage(childObject) + consolNotKnownToBeClosed, "Associate at Customs?"))
							{
								associateToConsol(args, childObject);
							}
						}
						else if (args.ItemRemoved)
						{
							disassociateFromConsol(args, childObject);
						}
					}
					else
					{
						ShowWarning("64C19DA4-2EE6-47B6-9501-F52EFB33D7EA", @"There is insufficient data to automatically notify Customs of this attachment/detachment.
To enable automatic notifications to Customs, ensure that a MAWB number is present and
that a CCS-UK profile is selected on the 'Electronic Messaging' tab.", "Customs Notifications");
					}
				}
			}
		}

		string GetSendEachPopupMessage(IConsolOrShipment childObject)
		{
			var childObjectNeedsDucrs = (childObject is ForwardingShipment ? "the (D)UCR(s) of " : "");
			return string.Format("Do you wish to send an EAC message/s to associate {0}{1} to consolidation {2}?", childObjectNeedsDucrs, childObject.HumanReadableName, MawbExportHelper.ME_MasterUCR);
		}

		public void AnticipateArrivalOnChief()
		{
			if (ContinueWithSending())
			{
				SendMessageOnConsol(new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master));
			}
		}

		public void ArriveGoodsOnChief()
		{
			if (ContinueWithSending())
			{
				SendMessageOnConsol(new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master));
			}
		}

		public void DepartGoodsOnChief()
		{
			if (ContinueWithSending())
			{
				SendMessageOnConsol(new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master));
			}
		}

		public void QueryMasterDEC()
		{
			if (ContinueWithSending())
			{
				SendMessageOnConsol(new GbDes242MessageFunction.QueryMasterDEC());
			}
		}

		public void CloseMasterUcrOnChiefDirectlyOnConsol()
		{
			if (ContinueWithSending())
			{
				SendMessageOnConsol(new GbDes242MessageFunction.MucrClose());
			}
		}

		bool ContinueWithSending()
		{
			bool continueWithSend = true;
			if (IsProfileForCDS)
			{
				continueWithSend = ShowYesNo("FE1F39D6-AAEE-4F52-92D8-67CFA5808879", ProfileMismatchMessage, "Profile Mismatch");
			}
			return continueWithSend;
		}

		ZString ProfileMismatchMessage => string.Format("You have asked to create a CHIEF message, but profile/badge {0} is set to use CDS for this consol's load port.\r\n\r\n" +
			"This is based on the configuration in the registry.\r\n\r\n " +
			"It is highly likely that generating this message will result in it being undeliverable, because the means of delivery is also based on registry settings.\r\n\r\n" +
			"Would you like to create the message anyway?", MawbExportHelper.ME_Profile);

		void SendMessageOnConsol(GbDes242MessageFunction how)
		{
			var consolSender = GetConsolMessgeSender();
			consolSender.SendToRecipient(this, sendsMessagesToCustoms, how, true);
			MawbExportHelper.Messages.Load();
			Messages.Refresh();
			Messages.RefreshBinding();
		}

		internal ConsolMessageSender GetConsolMessgeSender()
		{
			if (IsProfileForCDS)
			{
				return (ConsolMessageSender)Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.GB.GBCDS.ICDSConsolMessageSender>(), this);
			}
			else
			{
				return new ConsolMessageSender();
			}
		}

		public void CloseMasterUcrOnChiefUsingExistingEntry()
		{
			if (ContinueWithSending())
			{
				if (HasExportEntryForMessaging)
				{
					var messageSender = (IDeclarationMessageSender)Activator.CreateInstance(ObjectFactory.GetType<Integration.Customs.GB.IDeclarationMessageSenderChooser>());
					messageSender.Send(decForSending, sendsMessagesToCustoms, new GbDes242MessageFunction.MucrClose());
					Messages.Refresh();
					Messages.RefreshBinding();
				}
			}
		}

		internal bool HasExportEntryForMessaging
		{
			get
			{
				FindGbDeclarationWithActiveEntriesOnAnyOfConsolsShipmentsToUseForSending();
				if (decForSending == null)
				{
					ShowError("EC92C817-5AE7-4428-849E-A524F0AB149A", "There are no shipments on this consolidation that have an attached declaration that can be used for messaging with Customs.\r\nEnsure shipments have a declaration and that entries have been generated.", "No Shipments");
				}
				return decForSending != null;
			}
		}

		internal JobDeclaration GbDeclarationWithActiveEntriesOnAnyOfConsolsShipmentsToUseForSending
		{
			get
			{
				FindGbDeclarationWithActiveEntriesOnAnyOfConsolsShipmentsToUseForSending();
				return decForSending;
			}
		}

		void FindGbDeclarationWithActiveEntriesOnAnyOfConsolsShipmentsToUseForSending()
		{
			var shipment = (from ForwardingShipment ship in ForwardingConsol.Shipments
							where ship.JS_RL_NKOrigin.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)
							orderby ship.JS_SystemCreateTimeUtc
							select ship
							).FirstOrDefault();
			if (shipment != null)
			{
				decForSending = (from JobDeclaration dec in shipment.Declarations.OfType<JobDeclaration>()
								 where dec.IsExport && dec.ActiveEntryHeaders.Count > 0 && dec.CountryCode == Core.Constants.CountryCodes.UnitedKingdom
								 orderby dec.JE_SystemCreateTimeUtc
								 select dec
								 ).FirstOrDefault();
			}
		}

		void AssociateShipmentsDeclarationToConsol(CollectionCountChangedEventArgs args, IConsolOrShipment shipment)
		{
			var newMucrToSetOnFullDeclaration = MawbExportHelper.ME_ChiefConsolIsClosed ? null : MawbExportHelper.ME_MasterUCR;  // if consol is closed then a declaration should not be given a mucr otherwise you might see "E1010 CONSOLIDATION STATUS PROHIBITS THIS ACTION"
			SendEacMessage<GbDes242MessageFunction.MucrAssociate>(args, newMucrToSetOnFullDeclaration, (ForwardingShipment)shipment);
		}

		void SendEacMessage<TypeOfEacMessageAsOrDis>(CollectionCountChangedEventArgs args, string newMucrToSetOnFullDeclaration, ForwardingShipment childShipment)
			where TypeOfEacMessageAsOrDis : GbDes242MessageFunction.MasterUcrWithChildUcrFunction, new()
		{
			FindGbDeclarationFromShipment(childShipment);
			if (decForSending != null && !decForSending.JE_UCR.IsEmpty)
			{
				if (decForSending.ActiveEntryHeaders.Count == 0)
				{
					var errorMessageNoEntryHeaderFound = args.BizObject.HumanReadableName + " has no GB export declaration with an entry header.";
					var error = errorMessageNoEntryHeaderFound + " An EAC message could not be sent.\r\nYou should close without saving to undo your attach/detach operation.";
					ShowError("4270343A-FB7D-44C5-AA1C-588631213986", error, "Send EAC Message Error");
				}
				else
				{
					foreach (Customs.Business.CusEntryHeader ceh in decForSending.ActiveEntryHeaders)
					{
						if (newMucrToSetOnFullDeclaration != null)
						{
							decForSending.JE_MasterUCR = newMucrToSetOnFullDeclaration;
						}
						SendMessageOnConsol(new TypeOfEacMessageAsOrDis() { ChildUCRToBeAddedToMasterUCR = ceh.CH_BGMReference });
					}
				}
			}
			else
			{
				var mainWarningAboutNoFullDeclarationFound = args.BizObject.HumanReadableName + " has no GB export declaration with a DUCR.";
				var externalUcrs = (from CusEntryNumber cen in childShipment.Numbers where cen.CE_EntryType == CusEntryNumberTypes.Standard.UniqueConsignementReference select cen.CE_EntryNum).ToArray();
				if (externalUcrs.Length == 0)
				{
					ShowError("319514D9-3F2E-4C29-9122-C41059E0189A", mainWarningAboutNoFullDeclarationFound + " No external UCR exists on the shipment. An EAC message could not be sent.\r\nYou should close without saving to undo your attach/detach operation.", "No Export Declaration");
				}
				else
				{
					var allExternalDucrsAsString = string.Join(System.Environment.NewLine, externalUcrs);
					if (ShowYesNo("6371914F-6E6A-41BE-B730-B91B21C17E11", mainWarningAboutNoFullDeclarationFound + " Do you wish to use the external UCR(s)\r\n" + allExternalDucrsAsString + "?", "Use External UCR?"))
					{
						foreach (var oneExternalUcr in externalUcrs)
						{
							SendMessageOnConsol(new TypeOfEacMessageAsOrDis() { ChildUCRToBeAddedToMasterUCR = oneExternalUcr });
						}
					}
					else
					{
						ShowInformation("C976A3CC-D29E-4C7B-AD9B-73B7C3E6AF45", "No EAC message was sent.\r\nYou should close without saving to undo your attach/detach operation.", "Message Not Sent");
					}
				}
			}
		}

		void DisAssociateShipmentsDeclarationFromConsol(CollectionCountChangedEventArgs args, IConsolOrShipment childShipment)
		{
			var newMucrToSetOnFullDeclaration = "";
			SendEacMessage<GbDes242MessageFunction.MucrDisAssociate>(args, newMucrToSetOnFullDeclaration, (ForwardingShipment)childShipment);
		}

		void FindGbDeclarationFromShipment(ForwardingShipment shipment)
		{
			if (shipment != null)
			{
				decForSending = (from JobDeclaration dec in shipment.Declarations.OfType<JobDeclaration>()
								 where dec.IsExport && dec.CountryCode == Core.Constants.CountryCodes.UnitedKingdom
								 orderby dec.JE_SystemCreateTimeUtc
								 select dec
									 ).FirstOrDefault();
			}
		}

		public MawbExportAddInfo MawbExportHelper
		{
			get
			{
				if (MawbExportAddInfosCollectionOfOneItem.Count == 0)
				{
					MawbExportAddInfosCollectionOfOneItem.AddNew();

					if (MawbExportAddInfosCollectionOfOneItem[0].Data.Consol.IsExport())
					{
						MawbExportAddInfosCollectionOfOneItem[0].Data.Initialize();
					}
				}

				return MawbExportAddInfosCollectionOfOneItem[0].Data;
			}
		}

		public void CalculateMasterUCR(bool updateOnlyWhenEmpty = true)
		{
			if ((MawbExportHelper.ME_MasterUCR.IsEmpty && updateOnlyWhenEmpty) || !updateOnlyWhenEmpty)
			{
				MawbExportHelper.CalculateMUCR(updateOnlyWhenEmpty);
			}
		}

		ChiefRelatedConsolCollection relatedConsols;
		[ChildEditable(true)]
		public ChiefRelatedConsolCollection RelatedConsols
		{
			get
			{
				if (relatedConsols == null)
				{
					relatedConsols = new ChiefRelatedConsolCollection(ForwardingConsol);
					RegisterEditableChildObject(relatedConsols);
					relatedConsols.CollectionCountChange -= new CollectionCountChangedEventHandler(RelatedConsolsCollection_CountChangedProperly);
					relatedConsols.CollectionCountChange += new CollectionCountChangedEventHandler(RelatedConsolsCollection_CountChangedProperly);
				}
				return relatedConsols;
			}
		}

		void RelatedConsolsCollection_CountChangedProperly(object sender, CollectionCountChangedEventArgs args)
		{
			var childConsol = (IConsolOrShipment)args.BizObject;
			PerformEACMessaging(childConsol, args, SendAssociateMessageForConsolToConsol, SendDisassociateMessageForConsolToConsol);
		}

		void SendDisassociateMessageForConsolToConsol(CollectionCountChangedEventArgs argsForChildConsol, IConsolOrShipment childConsol)
		{
			SendEacForConsolToConsol(new GbDes242MessageFunction.MucrDisAssociate(), (ForwardingConsol)childConsol);
		}

		void SendAssociateMessageForConsolToConsol(CollectionCountChangedEventArgs argsForChildConsol, IConsolOrShipment childConsol)
		{
			SendEacForConsolToConsol(new GbDes242MessageFunction.MucrAssociate(), (ForwardingConsol)childConsol);
		}

		void SendEacForConsolToConsol(GbDes242MessageFunction.MasterUcrWithChildUcrFunction messageFunction, ForwardingConsol childConsol)
		{
			messageFunction.ChildUCRToBeAddedToMasterUCR = MasterUCRCalculator.CalculateAirExports(childConsol.JK_MasterBillNum);
			SendMessageOnConsol(messageFunction);
		}

		UnRelatedConsolsToAddCollection unRelatedConsolsToAdd;
		public UnRelatedConsolsToAddCollection UnRelatedConsolsToAdd
		{
			get { return unRelatedConsolsToAdd ?? (unRelatedConsolsToAdd = new UnRelatedConsolsToAddCollection(ForwardingConsol, RelatedConsols)); }
		}

		[ChildEditable(true)]
		CusAddInfoCollection<MawbExportAddInfo> MawbExportAddInfosCollectionOfOneItem
		{
			get
			{
				if (mawbExportAddInfosCollectionOfOneItem == null)
				{
					mawbExportAddInfosCollectionOfOneItem = new CusAddInfoCollection<MawbExportAddInfo>(ForwardingConsol);
					mawbExportAddInfosCollectionOfOneItem.Load();
					RegisterEditableChildObject(mawbExportAddInfosCollectionOfOneItem);
				}
				return mawbExportAddInfosCollectionOfOneItem;
			}
		}

		public ZString GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol()
		{
			return GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol(ForwardingConsol);
		}

		public ZString GetReasonWhyCannotPrintAirlineDeliveryScheduleForConsol(Integration.Forwarding.IForwardingConsol iForwardingConsol)
		{
			this.ForwardingConsol = (ForwardingConsol)iForwardingConsol;
			var resultReason = string.Empty;
			if (!ForwardingConsol.IsExport() || ForwardingConsol.IsDomestic() || !ForwardingConsol.JK_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.UnitedKingdom))
			{
				resultReason = "The consol is not an international export from GB";
			}
			else
			{
				var soeMaster = new ExportStyleOfEntries().GetDescriptionFromCode(MawbExportHelper.ME_ChiefMasterStyleOfEntry);
				if (ForwardingConsol.SendingForwarder == null || ForwardingConsol.ReceivingForwarder == null)
				{
					resultReason = "The ADS cannot be produced. The sending or receiving agent is not set.";
				}
				else if (!ForwardingConsol.IsExport())
				{
					resultReason = "ADS is only for exports";
				}
				else if (IsRegularAgentOperator)
				{
					resultReason = CheckClosedConsolForAnyReasonWhyAdsIsNotAllowedForRegularAgent(resultReason);
				}
				else if (IsDesignatedExportPlaceAgentOperator)
				{
					if (soeMaster != ExportStyleOfEntries.Descriptions.PermittedToProgress)
					{
						resultReason = "The ADS cannot be produced. The DEP consol does not have permission to progress.  Current SoE is: " + soeMaster + ".";
					}
					else
					{
						// Check all declarations for P2P or C status
						var declarationsNotP2PAndNotCStatus = new ZStringBuilder();
						var allDeclarations = new List<JobDeclaration>();
						foreach (ForwardingShipment shipment in ForwardingConsol.Shipments)
						{
							allDeclarations.AddRange(from JobDeclaration dec in shipment.Declarations.OfType<JobDeclaration>() where dec.CountryCode == Core.Constants.CountryCodes.UnitedKingdom && dec.IsExport select dec);
						}

						foreach (JobDeclaration dec in allDeclarations)
						{
							if (dec != null)
							{
								if (dec.ZG_StyleOfEntrySOE != ExportStyleOfEntries.Codes.PermittedToProgress && dec.ZG_CTStatusID != ExportCommunityTransitStatusList.Codes.C)
								{
									declarationsNotP2PAndNotCStatus.AppendLine(string.Format("	Declaration {0}, SoE {1}, CT status {3}, entry status {2}", dec.JE_DeclarationReference, dec.ZG_StyleOfEntrySOE, dec.JE_EntryStatus, dec.ZG_CTStatusID));
								}
							}
							if (declarationsNotP2PAndNotCStatus.Length != 0)
							{
								declarationsNotP2PAndNotCStatus.Prepend("DEP operator: the following declarations on this consol do not have P2P and are not C-status: \r\n\r\n");
								resultReason = declarationsNotP2PAndNotCStatus.ToString();
							}
						}
					}
				}
				else  // CCSUK messaging is on but not known if we're a simple or DEP agent.  User has not selected a profile
				{
					resultReason = "Select a valid profile/PIMA from the dropdown on the 'Electronic Messaging' tab first";
				}
			}
			return resultReason;
		}

		string CheckClosedConsolForAnyReasonWhyAdsIsNotAllowedForRegularAgent(string resultReason)
		{
			if (!MawbExportHelper.ME_ChiefConsolIsClosed)
			{
				resultReason = "The ADS cannot be produced. The agent consol is not closed at Customs.";
			}
			else
			{
				if (MawbExportHelper.ME_Queried.IsEmpty)
				{
					resultReason = "The DUCRs on this consol are not synchronised with Customs, or the last synchronisation request reported problems.  Send a DEC message and act upon its response.";
				}
				else if (ForwardingConsol.JK_SystemLastEditTimeUtc > MawbExportHelper.ME_Queried)
				{
					resultReason = "The consol has been modified since the last DEC query message was processed. Send a DEC message";
				}
				else if ((from ForwardingShipment s in ForwardingConsol.Shipments where s.JS_SystemLastEditTimeUtc > MawbExportHelper.ME_Queried select s).Any())
				{
					resultReason = "The consol is potentially out of synch with Customs. At least one shipment has been modified since the last query. Send a DEC message";
				}
				else
				{
					var duffShipments = new ZStringBuilder();
					foreach (ForwardingShipment shipment in ForwardingConsol.Shipments)
					{
						if (shipment.JS_CommunityTransitStatus != ExportCommunityTransitStatusList.Codes.C)
						{
							var gbLodgedExportDeclarations = (from BaseJobDeclaration dec in shipment.Declarations where dec.CountryCode == Core.Constants.CountryCodes.UnitedKingdom && dec.IsExport && !dec.DeclarationNumber.IsEmpty select dec);
							if (!gbLodgedExportDeclarations.Any())
							{
								var externalDucrs = (from CusEntryNumber cen in shipment.Numbers orderby cen.CE_EntryNum where cen.CE_EntryType == CusEntryNumberTypes.Standard.UniqueConsignementReference select cen);
								if (!externalDucrs.Any())
								{
									duffShipments.Append(shipment.HumanReadableName);
								}
							}
						}
					}
					if (duffShipments.Length > 0)
					{
						resultReason = "At least one shipment has no internal or external DUCRs and is not C-status. Jobs:\r\n" + duffShipments.ToStringWithNewLineBetweenAppends();
					}
				}
			}
			return resultReason;
		}

		bool IsDesignatedExportPlaceAgentOperator
		{
			get
			{
				var credentialsSetting = GetCredentialForPima();
				return credentialsSetting != null && credentialsSetting.IsDEPOperator;
			}
		}

		public bool CredentialIsDEP => IsExportConsol && IsDesignatedExportPlaceAgentOperator;

		public bool CredentialIsLoader => IsLoader;

		bool IsLoader
		{
			get
			{
				var credentialsSetting = GetCredentialForPima();
				return credentialsSetting != null && credentialsSetting.IsMaritimeLoader;
			}
		}

		bool IsRegularAgentOperator
		{
			get
			{
				var credentialsSetting = GetCredentialForPima();
				return credentialsSetting != null && (!credentialsSetting.IsDEPOperator && credentialsSetting.IsCcskAgent);
			}
		}

		bool IsExportConsol => ForwardingConsol?.IsExport() ?? ZBool.False;

		public bool IsChiefCcsukEnabled => IsExportConsol &&
				ForwardingConsol.JK_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.UnitedKingdom, StringComparison.Ordinal) &&
				(ForwardingConsol.IsAir || !GBCustomsDataRegistry.Instance.ChiefExportConsolIntegrationShowForAirOnly.Value);

		public bool IsCDSFunctionalityEnabled => GlbStaff.CurrentUser.GS_IsDeveloper
			|| ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(FunctionalityTypes.DeclarationApplicationCode, CountryCodes.UnitedKingdom, ZDateTime.Now);

		public bool IsProfileForCDS => IsProfileForCDSCore;
		protected virtual bool IsProfileForCDSCore => MawbExportHelper.Lookups.ProfilesForCDS.ContainsCode(MawbExportHelper.ME_Profile);

		public bool IsProfileForCCSUK => IsProfileForCCSUKCore;
		protected virtual bool IsProfileForCCSUKCore
		{
			get
			{
				var result = false;

				var profile = MawbExportHelper.ME_Profile;
				if (!profile.IsEmpty)
				{
					var badge = profile.Length == 3 ? profile : profile.Right(3);
					var badgeSetting = BadgeCodeGetter.InstanceCachedFor(this.MawbExportHelper).GetFromBadgeCode(badge, BadgeDirectionList.Codes.EXP);
					if (badgeSetting != null)
					{
						result = badgeSetting.CSPCode == GatewayList.Codes.CCSUKviaNTMsgGW;
					}
				}

				return result;
			}
		}

		public CredentialsSetting GetCredentialForPima()
		{
			var credentials = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			return credentials.Cast<CredentialsSetting>().FirstOrDefault(cred => cred.BadgeCode == MawbExportHelper.ME_Profile);
		}

		void ShowWarning(string msgId, string message, string caption) => ShowYesNo(msgId, message, caption, MessageStyle.Warning);
		void ShowError(string msgId, string message, string caption) => ShowYesNo(msgId, message, caption, MessageStyle.Error);
		void ShowInformation(string msgId, string message, string caption) => ShowYesNo(msgId, message, caption, MessageStyle.Information);
		bool ShowYesNo(string msgId, string message, string caption) => ShowYesNoCancel(msgId, message, caption);

		protected bool ShowYesNoCancel(string msgId, string message, string caption, MessageStyle messageStyle = MessageStyle.Question)
		{
			if (!supressedMessages.Contains(msgId))
			{
				var result = sendsMessagesToCustoms?.YesNoCancelQuery(message + YesNoCancelQuestionInfo, caption, messageStyle) ?? YesNoCancel.Yes;

				if (result == YesNoCancel.Yes)
				{
					supressedMessages.Add(msgId);
				}

				return result != YesNoCancel.Cancel;
			}
			else
			{
				return true;
			}
		}

		protected void ShowYesNo(string msgId, string message, string caption, MessageStyle messageStyle = MessageStyle.Question)
		{
			if (!supressedMessages.Contains(msgId))
			{
				var result = sendsMessagesToCustoms?.YesNoQuery(message + YesNoQuestionInfo, caption, messageStyle) ?? true;

				if (result)
				{
					supressedMessages.Add(msgId);
				}
			}
		}

		readonly HashSet<string> supressedMessages = new HashSet<string>();

		public const string YesNoCancelQuestionInfo = "\r\nPress Yes to send and stop asking for this consolidation\r\nPress No to send and keep asking for this consolidation\r\nPress Cancel not to send and keep asking for this consolidation";
		public const string YesNoQuestionInfo = "\r\nPress Yes to acknowledge and stop notifying me of this issue for this consolidation\r\nPress No to acknowledge and keep notifying me of this issue for this consolidation";

		CusAddInfoCollection<MawbExportAddInfo> mawbExportAddInfosCollectionOfOneItem;
		JobDeclaration decForSending;
		ChiefExportDes242MessagesOnConsolsShipmentsDeclarationsCollection messages;
		readonly ISendsMessagesToCustoms sendsMessagesToCustoms;
	}
}
