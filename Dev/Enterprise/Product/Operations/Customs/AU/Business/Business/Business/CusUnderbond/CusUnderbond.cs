using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CusUnderbond.Schema.C4_SendersMessageReference)]
	[UniversalDataContext(DataContextType.UnderBond)]
	[SystemDefinedValues]
	public class CusUnderbond :
		Customs.Business.CusUnderbond,
		ICMRMessageRespondee,
		ICMRControlMessageRespondee,
		ICusUnderbondDependentCollectionParent,
		IJobInvoicingPlugIn,
		IWorkflowProvider,
		IJobHeaderParent,
		Customs.Business.IMessageManageableBizObj,
		Integration.Customs.AU.ICusUnderbond,
		IEDocsProvider,
		IDocumentSupportable,
		IDataExportCSVFileNameProvider,
		Customs.Business.ISynchroniserReadOnlyMembersProvider,
		IEDIMessageCollectionProvider,
		ITransitWarehouseSyncDataParent
	{
		public abstract new class Schema : Customs.Business.CusUnderbond.Schema
		{
			public const string LinkedObjectName = "LinkedObjectName";
			public const string AU_OutturnResponsiblePartyID = "AU_OutturnResponsiblePartyID";

			public const int AU_OutturnResponsiblePartyIDMaxLength = 15;
		}

		public CusUnderbond(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			Calculator = new CusUnderbondStatusCalculator(this);
			OutturnStatusCalculator = new CusUnderbondOutturnStatusCalculator(this);
		}

		public static CusUnderbond New(BusinessObjectFactory factory)
		{
			return factory.New<CusUnderbond>();
		}

		public static bool IsOutturnAccepted(ZString outturnStatus)
		{
			return (outturnStatus == CMRBaseStatuses.Codes.OriginalAccepted || outturnStatus == CMRBaseStatuses.Codes.AmendmentAccepted);
		}

		public EDIMessageCollection CargoMessages
		{
			get { return FilteredMessageCollection(new ZString[] { CMRMessage.CMRMessageTypes.CARST, CMRMessage.CMRMessageTypes.UBMREQR }); }
		}

		public EDIMessageCollection OutturnMessages
		{
			get { return FilteredMessageCollection(new ZString[] { CMRMessage.CMRMessageTypes.AIROUT }); }
		}

		EDIMessageCollection FilteredMessageCollection(ZString[] messageTypes)
		{
			var messagesFilter = new ZQuery(EDIMessageSchema.EM_MessageType, messageTypes);
			var filteredMessages = GetNewEDIMessageCollection();
			filteredMessages.Load(messagesFilter);
			return filteredMessages;
		}

		protected override TypeLoaderCollection GetParentLoaders()
		{
			var result = base.GetParentLoaders();
			result.Add(new TypeLoader(typeof(CusHAWBBase)));
			result.Add(new TypeLoader(typeof(CusMAWBBase)));
			result.Add(new TypeLoader(typeof(CusPartShip)));
			result.Add(new TypeLoader(typeof(CusSCAContainer)));
			result.Add(new TypeLoader(typeof(CusSCAPivot)));
			result.Add(new TypeLoader(typeof(CusSeaManOBLDetail)));
			result.Add(new TypeLoader(typeof(CusSCADepotHouse)));
			result.Add(new TypeLoader(typeof(CusSCADepotContainer)));
			result.Add(new WrapperTypeLoader<CFSLoadListConsol, CFSLoadListConsolWrapper>());
			result.Add(new WrapperTypeLoader<CFSContainer, CFSContainerWrapper>());
			//Result.Add(new WrapperTypeLoader(typeof(TallyC), typeof(CFSTallyContainerWrapper)));
			result.Add(new WrapperTypeLoader<CFSShipment, CFSShipmentWrapper>());
			return result;
		}
		#region New Properties

		public override ZString ApprovalStatus
		{
			get { return Factory.GetCachedValue<CMRUnderbondStatuses>().GetDescriptionFromCode(C4_Status); }
		}

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return 0; }
		}

		public new CusUnderbondCusOutturnCollection Outturns
		{
			get { return (CusUnderbondCusOutturnCollection)base.Outturns; }
		}

		protected override Customs.Business.CusUnderbondCusOutturnCollection GetNewOutturnCollection()
		{
			var typeOfElements = IsDepot ? typeof(DepotCusOutturn) : typeof(CusOutturn);
			return new CusUnderbondCusOutturnCollection(this, typeOfElements);
		}

		public CusOutturnHeader OutturnHeader
		{
			get { return IsAirCargo ? AirOutturnHeader : SeaOutturnHeader; }
		}

		CusOutturnHeader AirOutturnHeader
		{
			get
			{
				if (fAirOutturnHeader == null)
				{
					fAirOutturnHeader = Factory.Load<CusOutturnHeader>(C4_C6);
					if (fAirOutturnHeader == null)
					{
						fAirOutturnHeader = Factory.New<CusOutturnHeader>();
					}
				}
				return fAirOutturnHeader;
			}
		}
		CusOutturnHeader fAirOutturnHeader;

		CusOutturnHeader SeaOutturnHeader
		{
			get
			{
				CusOutturnHeader result = null;
				var oceanBill = OceanBill;
				if (oceanBill != null && !oceanBill.CB_Voyage.IsEmpty && !oceanBill.CB_LloydsIMO.IsEmpty && !C4_DestinationPremiseID.IsEmpty)
				{
					var query = new ZQuery(CusOutturnHeaderSchema.C6_OutturningPremiseID, C4_DestinationPremiseID);
					query.AddToFilter(CusOutturnHeaderSchema.C6_LloydsIMO, oceanBill.CB_LloydsIMO);
					query.AddToFilter(CusOutturnHeaderSchema.C6_VoyageNum, oceanBill.CB_Voyage);
					result = Factory.LoadTop1<CusOutturnHeader>(query);
				}
				return result;
			}
		}

		public ZBool IsSeaOutturned(ZString housebill)
		{
			var result = false;
			var containerNumber = ContainerNumber;
			var outturnHeader = OutturnHeader;
			if (outturnHeader != null && !containerNumber.IsEmpty)
			{
				result = outturnHeader.Outturns.Cast<CusOutturn>().Any(x => x.C5_ContainerNumber == containerNumber && x.C5_HouseBill == housebill && !x.C5_CargoUnpackDate.IsEmpty);
			}
			return result;
		}

		public bool IsStandAloneUnderbond
		{
			get { return C4_ParentID.IsEmpty; }
		}

		public bool IsChangingUniqueIdentifier
		{
			get { return C4_MAWBInfo.HasChanges && (OutturnStatus.Code != CMRBaseStatuses.Codes.NotSent && OutturnStatus.Code != CMRBaseStatuses.Codes.WithdrawalAccepted && OutturnStatus.Code != CMRBaseStatuses.Codes.OriginalRejected); }
		}

		public bool IsChangingCustomsIdentifier
		{
			get
			{
				return (C4_FlightNoInfo.HasChanges || C4_ArrivalDateInfo.HasChanges || C4_OuturnedInfo.HasChanges) &&
					   (OutturnStatus.Code != CMRBaseStatuses.Codes.NotSent && OutturnStatus.Code != CMRBaseStatuses.Codes.WithdrawalAccepted && OutturnStatus.Code != CMRBaseStatuses.Codes.OriginalRejected);
			}
		}

		protected override CusEntryNumStatus OutturnStatusCore
		{
			get
			{
				var result = base.OutturnStatusCore;
				if (C4_ParentTableCode == CusMAWBSchema.Constants.Prefix)
				{
					result.Status.StatusChanged += StatusChangedForCusMAWBOutturn;
				}
				return result;
			}
		}

		void StatusChangedForCusMAWBOutturn(object sender, StatusChangedEventArgs e)
		{
			if (IsOutturnAccepted(e.NewStatus) && LinkedObject is CusMAWB)
			{
				foreach (CusOutturn line in Outturns)
				{
					var house = line.Parent as CusHAWB;
					if (house != null)
					{
						house.OutturnCargo(line);
						if (line.C5_OutturnResultType != CMROutturnResultType.Codes.ShortLanded || line.C5_PackagesOutturned > 0)
						{
							if (house.CS_CargoReceivedDate.IsEmpty)
							{
								var cargoReceivedAtDepotLog = house.CargoReceivedAtDepotLogs.MostRecentLog;
								if (cargoReceivedAtDepotLog != null)
								{
									house.CS_CargoReceivedDate = cargoReceivedAtDepotLog.SL_EventTime;
								}
							}
						}
						else if (!house.CS_CargoReceivedDate.IsEmpty)
						{
							house.CS_CargoReceivedDate = ZDateTime.Empty;
						}
					}
				}
			}
		}

		public ZString UnderbondBySeaVesselID
		{
			get { return C4_UnderbondBySeaLloydsIMONum; }
		}

		[BusinessObjectTestExclude()]
		[MaxLength(25)]
		public ZString LinkedObjectName
		{
			get { return base.LinkedObjectStringRepresentation; }
			set
			{
				UpdateLinkedObjectByName(value);
				LinkedObjectNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LinkedObjectNameInfo
		{
			get { return GetZPropertyInfo(Schema.LinkedObjectName); }
		}

		public bool IsAirCargo
		{
			get { return C4_ParentTableCode == CusHAWBSchema.Constants.Prefix || C4_ParentTableCode == CusMAWBSchema.Constants.Prefix; }
		}

		void UpdateLinkedObjectByName(ZString linkedObjectName)
		{
			ICusUnderbondUnionCollectionParent underbondParent = null;
			if (LinkedObject != null)
			{
				if (MAWB != null)
				{
					underbondParent = MAWB;
				}
				else if (OceanBill != null)
				{
					underbondParent = OceanBill;
				}
			}

			ICusUnderbondDependentCollectionParent result = null;
			if (underbondParent != null)
			{
				foreach (var provider in underbondParent.GetAllPossibleCollectionProviders())
				{
					if (provider.UnderbondHumanReadableName.ToString() == linkedObjectName)
					{
						result = provider;
						break;
					}
				}
			}
			LinkedObject = result;
		}

		public ZString ShortDescription
		{
			get
			{
				var result = ZString.Empty;
				if (OceanBill != null)
				{
					result = OceanBill.ShortDescription;
				}
				else
				{
					var currentObject = LinkedObject as BusinessObject;
					if (currentObject != null)
					{
						result = CMRRespondeeWrapper.GetWrapper(currentObject).ShortDescription;
					}
				}
				return result;
			}
		}

		public bool HasAlreadyBeenSent { get; set; }

		[MaxLength(Schema.AU_OutturnResponsiblePartyIDMaxLength)]
		public ZString AU_OutturnResponsiblePartyID
		{
			get
			{
				var result = this.GetSystemDefinedValue<ZString>(Schema.AU_OutturnResponsiblePartyID);
				if (result.IsEmpty)
				{
					result = C4_Calculated_ResponsiblePartyID;
				}
				return result;
			}
			set
			{
				CheckMaximumLength(AU_OutturnResponsiblePartyIDInfo, value);
				this.SetSystemDefinedValue(Schema.AU_OutturnResponsiblePartyID, value);
				AU_OutturnResponsiblePartyIDInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					var validation = base.Validation as CusUnderbondValidation;
					if (validation != null)
					{
						validation.ValidateAU_OutturnResponsiblePartyID();
					}
				}
			}
		}

		public ZPropertyInfo AU_OutturnResponsiblePartyIDInfo
		{
			get { return GetZPropertyInfo(Schema.AU_OutturnResponsiblePartyID); }
		}

		#endregion

		#region Related Business objects

		public CusOutturnHeader Header => Factory.Load<CusOutturnHeader>(C4_C6);

		public CusHAWB HAWBLinked
		{
			get { return LinkedObject as CusHAWB; }
		}

		public CusMAWB MAWB
		{
			get { return LinkedObject as CusMAWB ?? HAWBLinked?.MAWB; }
		}

		public CusSCAPivot PivotLinked
		{
			get { return LinkedObject as CusSCAPivot; }
		}

		public CusSCAContainer ContainerLinked
		{
			get { return LinkedObject as CusSCAContainer; }
		}

		public ZString ContainerNumber
		{
			get { return ContainerLinked != null ? ContainerLinked.CN_ContainerNumber : ZString.Empty; }
		}

		public CusSCAHouse SCAHouseLinked
		{
			get { return LinkedObject as CusSCAHouse; }
		}

		public CusSCAOceanBill OceanBill
		{
			get
			{
				CusSCAOceanBill result = null;
				if (PivotLinked != null)
				{
					result = PivotLinked.OceanBill;
				}
				else if (ContainerLinked != null)
				{
					result = ContainerLinked.OceanBill;
				}
				else if (SCAHouseLinked != null)
				{
					result = SCAHouseLinked.OceanBill;
				}
				return result;
			}
		}

		#endregion

		#region BusinessObject Overrides

		public override void Delete()
		{
			JobHeader.DeleteAllJobs(this);
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete &&
					UnderbondStatus.Code != CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived &&
					UnderbondStatus.Code != CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived &&
					CMRStatusHelper.CanDelete(UnderbondStatus.Code);
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result;
				if (!CanDelete)
				{
					result = ResString.GetMultilingualString("3A6222B0-6767-4A28-8B03-29FA048EE496", "You cannot delete this Underbond because there are messages associated with it.");
				}
				else
				{
					result = base.ReasonForNotAbleToDelete;
				}
				return result;
			}
		}
		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void SetDefaultValuesFromParent()
		{
			base.SetDefaultValuesFromParent();

			var linkedObjectAsDefaultProvider = LinkedObject as IUnderbondDefaultValueProvider;
			if (linkedObjectAsDefaultProvider != null)
			{
				linkedObjectAsDefaultProvider.SetUnderbondDefaultValues(this);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			C4_ApplicationCode = Enterprise.Customs.Business.CusUnderbondApplicationCodeList.Codes.AUUnderbond;
			C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Road;
			C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			C4_IsMoveFromDischarge = true;

			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			if (currentCompany.OrgProxy != null)
			{
				if (!currentCompany.OrgProxy.PrimaryRegistrationNumber.Number.IsEmpty)
				{
					C4_ResponsiblePartyID = currentCompany.OrgProxy.PrimaryRegistrationNumber.Number.Left(C4_ResponsiblePartyIDInfo.MaxLength);
				}
				C4_DestinationPremiseID = currentCompany.OrgProxy.MainAddress.LocalControlledPremisesID;
			}
		}

		#endregion

		public override bool VoyageAndVesselDetailsVisible
		{
			get { return C4_ModeOfMovement == CMRUnderbondModeOfMovement.Codes.SeaInternationalVessel; }
		}

		public bool EqualsByAirKeyFields(CusUnderbond underbond)
		{
			return IsAirCargo
				&& underbond.IsAirCargo
				&& !C4_FlightNo.IsEmpty
				&& C4_FlightNo == underbond.C4_FlightNo
				&& !C4_ArrivalDate.IsEmpty
				&& C4_ArrivalDate == underbond.C4_ArrivalDate
				&& !C4_DestinationPremiseID.IsEmpty
				&& C4_DestinationPremiseID == underbond.C4_DestinationPremiseID
				&& !C4_Outurned.IsEmpty
				&& C4_Outurned == underbond.C4_Outurned;
		}

		public void ResetRejectedOutturnMessageStatus()
		{
			foreach (CusOutturn outturn in Outturns)
			{
				if (outturn.C5_MessageStatus == CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected)
				{
					outturn.C5_MessageStatus = ZString.Empty; // reset any previously received line rejections.
				}
			}
		}

		#region Can Do Outturn

		protected override bool GetCanDoOutturn()
		{
			return !(IsDeleted || C4_DestinationPremiseID.IsEmpty) && (
				IsUnderbondDestinedToOrgProxyOfCurrentCompany() ||
				IsUnderbondDestinedToOrgProxyOfCurrentCompanyBranches() ||
				IsUnderbondDestinedToCFSOrg());
		}

		bool IsUnderbondDestinedToOrgProxyOfCurrentCompany()
		{
			return IsPremiseIDForOrganisation(C4_DestinationPremiseID, GlbCompany.GetCurrentCompany(Factory).OrgProxy);
		}

		bool IsUnderbondDestinedToOrgProxyOfCurrentCompanyBranches()
		{
			return IsPremiseIDForBranch(C4_DestinationPremiseID);
		}

		bool IsUnderbondDestinedToCFSOrg()
		{
			var result = false;
			var cfsOrg = (OrgHeader)((IOrgAddress)MAWB?.UnpackDepotAddress)?.Header;
			if (cfsOrg != null)
			{
				var currentCompanyOrgProxy = GlbCompany.GetCurrentCompany(Factory).GC_OH_OrgProxy;
				var cfsOrgIsLinkedToCurrentCompanyOrgProxy = cfsOrg.ServiceRelatedParties.Cast<OrgRelatedParty>()
					.Any(x => x.PR_OH_RelatedParty == currentCompanyOrgProxy);
				var cfsOrgHasCCPMatchingDestinationCode = IsPremiseIDForOrganisation(C4_DestinationPremiseID, cfsOrg);
				result = cfsOrgIsLinkedToCurrentCompanyOrgProxy && cfsOrgHasCCPMatchingDestinationCode;
			}
			return result;
		}

		bool IsPremiseIDForBranch(ZString premiseID)
		{
			var result = false;

			var currentBranch = GlbBranch.GetCurrentBranch(Factory);
			var isRespPartyID = FreightDataRegistry.Instance.OuturnResponsiblePartyIDOverride.GetValueWithoutFallback(Guid.Empty, currentBranch.PK.ToGuid(), Guid.Empty).ContainsCode(premiseID);

			var currentCompany = GlbCompany.GetCurrentCompany(Factory);

			foreach (var branch in currentCompany.Branches)
			{
				if (IsPremiseIDForOrganisation(premiseID, branch.OrgProxy) || isRespPartyID)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		bool IsPremiseIDForOrganisation(ZString premiseID, OrgHeader organisation)
		{
			var result = false;
			if (organisation != null)
			{
				foreach (OrgAddress address in organisation.Addresses)
				{
					if (address.LocalControlledPremisesID.ToUpper() == premiseID.ToUpper())
					{
						result = true;
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region Calculated Fields

		public bool HasSplitMessageOriginalRejectedLog
		{
			get { return CusUnderbondOutturnLogManager.HasSplitMessageOriginalRejectedLog; }
		}

		public bool HasSplitMessageFailedLog
		{
			get { return CusUnderbondOutturnLogManager.HasSplitMessageFailedLog; }
		}

		public bool HasNonExistantLineAtCustomsLog
		{
			get { return CusUnderbondOutturnLogManager.HasNonExistantLineAtCustoms; }
		}

		public CusUnderbondOutturnLogManager CusUnderbondOutturnLogManager
		{
			get
			{
				if (cusUnderbondOutturnLogManager == null)
				{
					cusUnderbondOutturnLogManager = new CusUnderbondOutturnLogManager(this);
				}
				return cusUnderbondOutturnLogManager;
			}
		}
		CusUnderbondOutturnLogManager cusUnderbondOutturnLogManager;
		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return IsDepot && (MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name));
		}
		#endregion

		protected override bool GetIsUnderbondForSeaShipment()
		{
			var linkedObject = this.LinkedObject;
			return linkedObject is CusSCAContainer || linkedObject is CusSCAPivot || linkedObject is CusSeaManOBLDetail
				|| linkedObject is CFSContainerWrapper || linkedObject is CFSShipmentWrapper;
		}

		protected override bool GetIsLastMessageDateSupported()
		{
			var linkedObject = this.LinkedObject;
			return linkedObject is CusMAWB;
		}

		protected override bool TranshipmentPortVisibleCore()
		{
			return C4_MovementReason == CMRUnderbondRequestCodes.Codes.Transshipment;
		}

		ZString ICMRControlMessageRespondee.UpdateStatusWhenControlMessageSyntaxError(EDIMessage incomingMessage, EDIMessage outgoingMessage)
		{
			var logText = ZString.Empty;
			var newStatus = ZString.Empty;

			if (outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Original)
			{
				newStatus = CMRBaseStatuses.Codes.OriginalRejected;
			}
			else if (outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Change ||
					outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Amendment ||
					outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.ReplaceHeader)
			{
				newStatus = CMRBaseStatuses.Codes.AmendmentRejected;
			}
			else if (outgoingMessage.EM_MessageSubType == CMRMessage.MessageSubTypes.Withdraw)
			{
				newStatus = CMRBaseStatuses.Codes.WithdrawalRejected;
			}

			if (newStatus != ZString.Empty)
			{
				if (outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.UBMREQ)
				{
					UnderbondStatus.Code = newStatus;
					logText = "Underbond to: " + newStatus;
				}
				else if (outgoingMessage.EM_MessageType == CMRMessage.CMRMessageTypes.AIROUT)
				{
					OutturnStatus.Code = newStatus;
					logText = "Air Outturn to: " + newStatus;
				}
			}
			return logText;
		}

		#region Property Overrides

		public override ZString C4_MAWB
		{
			get { return base.C4_MAWB; }
			set
			{
				var hasChanges = value != C4_MAWB;
				base.C4_MAWB = value;
				if (hasChanges && !IsCopying)
				{
					Outturns.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString C4_MovementReason
		{
			get { return base.C4_MovementReason; }
			set
			{
				var hasChanged = base.C4_MovementReason != value;
				base.C4_MovementReason = value;
				if (hasChanged && !IsCopying)
				{
					var defaultValue = ZString.Empty;
					if (value == CMRUnderbondRequestCodes.Codes.Transshipment
						&& LinkedObject != null && LinkedObject.UsesTranshipmentPortOnUnderbond
						&& !LinkedObject.DefaultTranshipmentPort.StartsWith(Core.Constants.CountryCodes.Australia))
					{
						defaultValue = LinkedObject.DefaultTranshipmentPort;
					}
					C4_RL_NKTranshipDestPort = defaultValue;
				}
			}
		}

		protected override ZString DefaultUnderbondStatus
		{
			get { return CMRBaseStatuses.Codes.NotSent; }
		}

		protected override ZString DefaultOutturnStatus
		{
			get { return CMRBaseStatuses.Codes.NotSent; }
		}

		public override ZGuid C4_OA_DestinationAddress
		{
			get { return base.C4_OA_DestinationAddress; }
			set
			{
				base.C4_OA_DestinationAddress = value;
				FireCanDoOutturnChangedEvent();
			}
		}

		public override ZString C4_DestinationPremiseID
		{
			get
			{
				return base.C4_DestinationPremiseID;
			}
			set
			{
				base.C4_DestinationPremiseID = value;
				FireCanDoOutturnChangedEvent();
			}
		}

		#region UnderbondBySea

		public void SetUnderbondBySeaVessel(RefVessel vessel)
		{
			C4_UnderbondBySeaVessel = vessel.RV_Code;
			C4_UnderbondBySeaLloydsIMONum = vessel.RV_LloydsNumber;
		}

		public override ZString C4_UnderbondBySeaVessel
		{
			get { return base.C4_UnderbondBySeaVessel; }
			set
			{
				base.C4_UnderbondBySeaVessel = value;
				UpdateUnderbondBySeaLloydsIMONumFromVessel();
			}
		}

		public override ZString C4_UnderbondBySeaLloydsIMONum
		{
			get { return base.C4_UnderbondBySeaLloydsIMONum; }
			set
			{
				base.C4_UnderbondBySeaLloydsIMONum = value;
				UpdateUnderbondBySeaVesselFromLloyds();
			}
		}

		void UpdateUnderbondBySeaLloydsIMONumFromVessel()
		{
			if (C4_UnderbondBySeaLloydsIMONum.IsEmpty && !C4_UnderbondBySeaVessel.IsEmpty)
			{
				var vesselsFromName = RefVessel.LookupVesselByName(C4_UnderbondBySeaVessel, Factory);
				if (vesselsFromName.Length == 1)
				{
					base.C4_UnderbondBySeaLloydsIMONum = vesselsFromName[0].RV_LloydsNumber;
				}
			}
		}

		void UpdateUnderbondBySeaVesselFromLloyds()
		{
			if (C4_UnderbondBySeaVessel.IsEmpty && !C4_UnderbondBySeaLloydsIMONum.IsEmpty)
			{
				var vesselFromLloyds = RefVessel.LookupVesselByLloyds(C4_UnderbondBySeaLloydsIMONum, Factory);
				if (vesselFromLloyds != null)
				{
					base.C4_UnderbondBySeaVessel = vesselFromLloyds.RV_Code;
				}
			}
		}

		#endregion

		public override ZString C4_ResponsiblePartyID
		{
			get
			{
				return base.C4_ResponsiblePartyID;
			}
			set
			{
				base.C4_ResponsiblePartyID = value.Replace(" ", "");
			}
		}

		public bool IsDepot => C4_ParentTableCode == JobShipmentSchema.Constants.Prefix || C4_ParentTableCode == JobContainerSchema.Constants.Prefix;

		public bool AcknowledgedByCustoms
		{
			get
			{
				return !C4_MessageStatus.IsEmpty
					&& C4_MessageStatus != CMRBaseStatuses.Codes.NotSent
					&& C4_MessageStatus != CMRBaseStatuses.Codes.OriginalRejected
					&& C4_MessageStatus != CMRBaseStatuses.Codes.WithdrawalAccepted;
			}
		}

		public override ZString C4_ParentTableCode
		{
			get
			{
				return base.C4_ParentTableCode;
			}
			set
			{
				var hasChanges = value != C4_ParentTableCode;
				base.C4_ParentTableCode = value;
				if (hasChanges)
				{
					Outturns.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid C4_ParentID
		{
			get
			{
				return base.C4_ParentID;
			}
			set
			{
				var hasChanges = C4_ParentID != value;
				base.C4_ParentID = value;
				if (hasChanges)
				{
					Outturns.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		public readonly CusUnderbondStatusCalculator Calculator;
		public readonly CusUnderbondOutturnStatusCalculator OutturnStatusCalculator;

		#region ICusUnderbondDependentCollectionParent Members

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines
		{
			get { return null; }
		}

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds
		{
			get { return null; }
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get { return true; }
		}

		ZString IOutturnableLine.UnderbondHumanReadableName
		{
			get { return "Stand alone"; }
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return false; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber
		{
			get { return C4_SendersMessageReference; }
		}

		#endregion

		#region IJobHeaderParent Members

		void IJobHeaderParent.SetJobNumberFieldOnSaving()
		{
			PopulateC4_SendersMessageReferenceIfNeeded();
		}

		void IJobHeaderParent.OnJobCreating(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobCreated(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleting(JobHeader job)
		{
		}

		void IJobHeaderParent.OnJobDeleted(JobHeader job)
		{
		}

		bool IJobHeaderParent.AllowInvoiceDeletion
		{
			get { return true; }
		}

		#endregion

		#region IJobInvoicingPlugIn Members

		CusUnderbondInvoicingSupporter fInvoicingSupporter;
		public IJobInvoicingSupporter InvoicingSupporter
		{
			get { return fInvoicingSupporter ?? (fInvoicingSupporter = new CusUnderbondInvoicingSupporter(this)); }
		}

		#endregion

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return JobInvoicingConsumerTypes.CusUnderbond.Code; }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		[ChildEditable(true)]
		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ProcessTaskCollection<CusUnderbondProcessTask, CusUnderbond>(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection<CusUnderbondProcessTask, CusUnderbond> workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var result = this.GetJobRelatedTemplateSelectionCriteria();
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, C4_FlightNo.SubstringSafe(0, 2), ZString.Empty);
			return result;
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get { return new DocManagerInfo(this, Core.Constants.DocManagerCodes.CusUnderbond); }
		}

		#endregion

		#region IEDocsProvider Members

		public EDocsProviderSupporter GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return new CusUnderbondDocumentSupporter(this); }
		}

		#endregion

		#region Implementation

		protected override ZString CountryCode => Core.Constants.CountryCodes.Australia;

		protected override Customs.Business.CusUnderbondValidation GetNewValidation()
		{
			if (IsDepot)
			{
				return new DepotCusUnderbondValidation(this);
			}
			else if (LinkedObject is CTOCusMAWB)
			{
				return base.GetNewValidation();
			}
			else
			{
				return new CusUnderbondValidation(this);
			}
		}

		public new CusUnderbondLookups Lookups
		{
			get { return base.Lookups as CusUnderbondLookups; }
		}

		protected override Customs.Business.CusUnderbondLookups GetNewLookups()
		{
			return new CusUnderbondLookups(this);
		}

		#endregion

		#region IMessageManageableBizObj Members

		Customs.Business.IMessageManager Customs.Business.IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			return new CusUnderbondMessageManager(this);
		}

		bool Customs.Business.IMessageManageableBizObj.IsInAStatusAmendmentSendable
		{
			get { return true; }
		}

		Customs.Business.ContinueWithDetection Customs.Business.IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			return Enterprise.Customs.Business.ContinueWithDetection.Yes;
		}

		#endregion

		#region IDataExportCSVFileNameProvider

		ZString IDataExportCSVFileNameProvider.FileNameSuffix
		{
			get
			{
				var builder = new ZStringBuilder();
				builder.Append(C4_SendersMessageReference);
				builder.Append(LinkedObjectStringRepresentation);
				return builder.ToStringWithDelimiterBetweenAppends("_");
			}
		}

		#endregion
	}
}
