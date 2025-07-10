using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.D11B.Elements;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using FreightRegistry = Enterprise.Registry.Business.FreightDataRegistry;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[CodeProperty(CusCAeMHHouse.Schema.BW_HouseCCN), DescriptionProperty(CusCAeMHHouse.Schema.BW_HouseBill)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusCAeMHHouse : AutoCusCAeMHHouse,
		IDocAddresses,
		IEDIMessageCollectionProvider,
		IWorkflowTriggerEventSource,
		ISequenceNumberHeader,
		IACIHouseBillProvider,
		IMessageManagerEventHandler,
		ICanDelete,
		ISynchroniserReadOnlyMembersProvider,
		ITopLevelBizOProviderForJobDocAddress,
		IDocAddressesCaption,
		Integration.Customs.CA.ICusCAeMHHouse
	{
		public CusCAeMHHouse(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BW_MovementType = eMHMovementTypeList.Codes.Import;
		}

		public override ZDecimal BW_Weight
		{
			get { return base.BW_Weight; }
			set { base.BW_Weight = value > 0 ? (ZDecimal)Math.Max(1, value.Round(0)) : value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusCAeMHHouseLookups.MovementTypes))]
		public override ZString BW_MovementType
		{
			get { return base.BW_MovementType; }
			set { base.BW_MovementType = value; }
		}

		public ZString BW_MovementTypeDescription
		{
			get { return Lookups.MovementTypes.GetDescriptionFromCode(this.BW_MovementType); }
		}

		[List(nameof(Lookups) + "." + nameof(CusCAeMHHouseLookups.WeightUnits))]
		public override ZString BW_WeightUQ
		{
			get { return base.BW_WeightUQ; }
			set { base.BW_WeightUQ = value; }
		}

		public ZString BW_WeightUQDescription
		{
			get { return Lookups.WeightUnits.GetDescriptionFromCode(this.BW_WeightUQ); }
		}

		[List(nameof(Lookups) + "." + nameof(CusCAeMHHouseLookups.VolumnUnits))]
		public override ZString BW_VolumeUQ
		{
			get { return base.BW_VolumeUQ; }
			set { base.BW_VolumeUQ = value; }
		}

		public ZString BW_VolumeUQDesciption
		{
			get { return Lookups.VolumnUnits.GetDescriptionFromCode(this.BW_VolumeUQ); }
		}

		[List(nameof(Lookups) + "." + nameof(CusCAeMHHouseLookups.ReleasePorts))]
		public override ZString BW_CBSAReleasePort
		{
			get { return base.BW_CBSAReleasePort; }
			set { base.BW_CBSAReleasePort = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusCAeMHHouseLookups.ReleaseSubLocations))]
		public override ZString BW_CBSAReleaseSubLocation
		{
			get { return base.BW_CBSAReleaseSubLocation; }
			set { base.BW_CBSAReleaseSubLocation = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusCAeMHHouseLookups.AmendmentCodes))]
		public override ZString BW_AmendReasonCode
		{
			get { return base.BW_AmendReasonCode; }
			set { base.BW_AmendReasonCode = value; }
		}

		[RelatedBusinessObject(nameof(MasterBill))]
		public override ZGuid BW_BP_Master
		{
			get { return base.BW_BP_Master; }
			set { base.BW_BP_Master = value; }
		}

		public CusCAeMHMaster MasterBill
		{
			get { return Factory.Load<CusCAeMHMaster>(this.BW_BP_Master); }
		}

		[ReadOnlyMember(nameof(BW_HouseBill_ReadOnly))]
		public override ZString BW_HouseBill { get => base.BW_HouseBill; set => base.BW_HouseBill = value; }

		bool BW_HouseBill_ReadOnly
		{
			get
			{
				return DisableChangeHouseBillNumberAndCCN;
			}
		}

		[ReadOnlyMember(nameof(BW_HouseCCN_ReadOnly))]
		public override ZString BW_HouseCCN { get => base.BW_HouseCCN; set => base.BW_HouseCCN = value; }

		bool BW_HouseCCN_ReadOnly
		{
			get
			{
				return DisableChangeHouseBillNumberAndCCN;
			}
		}

		public bool DisableChangeHouseBillNumberAndCCN
		{
			get
			{
				return BW_MessageStatus != MessageStatusList.Codes.NotSent
					&& BW_MessageStatus != MessageStatusList.Codes.ClearDelete
					&& BW_MessageStatus != MessageStatusList.Codes.ErrorOriginal
					&& !BW_MessageStatus.IsEmpty;
			}
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusCAeMHHouseLookups.MessageStatuses))]
		public override ZString BW_MessageStatus
		{
			get { return base.BW_MessageStatus; }
			set { base.BW_MessageStatus = value; }
		}

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusCAeMHHouseLookups.CustomsStatuses))]
		public override ZString BW_CustomsStatus
		{
			get { return base.BW_CustomsStatus; }
			set { base.BW_CustomsStatus = value; }
		}

		[ReadOnly(true)]
		public override ZDateTime BW_RNSProcessingDate
		{
			get { return base.BW_RNSProcessingDate; }
			set { base.BW_RNSProcessingDate = value; }
		}

		public override ZBool BW_OverrideFreightDefaults
		{
			get => base.BW_OverrideFreightDefaults;
			set
			{
				base.BW_OverrideFreightDefaults = value;
				if (value)
				{
					DisposeSynchroniser();
				}
				else
				{
					EnableAndSynchronise();
				}
			}
		}

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region New Properties

		public Enterprise.Messaging.Business.EDIMessage LastSentMessage
		{
			get
			{
				if (lastSentMessage == null)
				{
					lastSentMessage = new CachedProperty<Enterprise.Messaging.Business.EDIMessage>(Factory, delegate
					{
						return Messages.GetLastMessage(EDIMessage.ApplicationCodes.CAACI, MessageTypeList.Codes.ACIHouseBill, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent);
					});
				}
				return lastSentMessage.Value;
			}
		}
		CachedProperty<Enterprise.Messaging.Business.EDIMessage> lastSentMessage;

		public ForwardingShipment Shipment
		{
			get { return Factory.Load<ForwardingShipment>(this.BW_ParentID); }
		}

		[ChildEditable(true)]
		public CusCAeMHHouseContainerPivotCollectionForHouse Pivots
		{
			get
			{
				if (fPivots == null)
				{
					fPivots = new CusCAeMHHouseContainerPivotCollectionForHouse(this);
					RegisterEditableChildObject(fPivots);
				}
				return fPivots;
			}
		}
		CusCAeMHHouseContainerPivotCollectionForHouse fPivots;

		[ChildEditable(true)]
		public CusCAeMHItemCollection Items
		{
			get
			{
				if (fItems == null)
				{
					fItems = new CusCAeMHItemCollection(this);
					RegisterEditableChildObject(fItems);
				}
				return fItems;
			}
		}
		CusCAeMHItemCollection fItems;

		[ChildEditable(true)]
		public CAeMHDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new CAeMHDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}
				return fDocAddresses;
			}
		}
		CAeMHDocAddressDependentCollection fDocAddresses;

		internal ShortSequenceNumberGenerator NotifyPartySequenceNumberGenerator
		{
			get { return fNotifyPartySequenceNumberGenerator ?? (fNotifyPartySequenceNumberGenerator = new ShortSequenceNumberGenerator(this, (x) => { return ((CAeMHDocAddress)x).E2_AddressType == DocAddressTypes.Codes.NotifyParty; })); }
		}
		ShortSequenceNumberGenerator fNotifyPartySequenceNumberGenerator;

		internal ShortSequenceNumberGenerator DeliveryAddressSequenceNumberGenerator
		{
			get { return fDeliveryAddressSequenceNumberGenerator ?? (fDeliveryAddressSequenceNumberGenerator = new ShortSequenceNumberGenerator(this, (x) => { return ((CAeMHDocAddress)x).E2_AddressType == DocAddressTypes.Codes.ConsigneePickupDeliveryAddress; })); }
		}
		ShortSequenceNumberGenerator fDeliveryAddressSequenceNumberGenerator;

		public bool IsPostArrival
		{
			get
			{
				if (isPostArrival == null)
				{
					isPostArrival = new CachedProperty<ZBool>(Factory, () =>
					{
						return MessagesForDisplay.Cast<EDIMessage>().Any(m => m.IsPostArrivalMessage());
					});
				}
				return isPostArrival.Value;
			}
		}
		CachedProperty<ZBool> isPostArrival;

		public int NumberOfUNDG
		{
			get
			{
				var num = 0;
				if (Items != null)
				{
					foreach (var item in Items)
					{
						if (item.UNDGs != null)
						{
							num += item.UNDGs.Count;
						}
					}
				}
				return num;
			}
		}
		public const int MaxPartyName = 70;
		public const int MaxContactName = 70;
		public const int MaxNumberOfUNDG = 9;
		public const int MaxCity = 35;
		public const int MaxState = 9;
		public const int MaxPostCode = 9;
		public const int MaxCountryCode = 2;

		public ZString FormattedLatestD4MessageStatus
		{
			get
			{
				var result = ZString.Empty;
				var statusCode = BW_D4MessageStatus;
				if (!statusCode.IsEmpty)
				{
					result = ZString.Format("{0} - {1}", statusCode, BW_D4MessageStatusDescription);
				}
				return result;
			}
		}

		public ZString BW_D4MessageStatusDescription
		{
			get
			{
				return CANoticeReasonCodesDescriptionHelper.GetD4NoticesDescriptionFromCode(Factory, BW_D4MessageStatus);
			}
		}

		#endregion

		#region Implementation

		public override void Delete()
		{
			Items.DeleteAll();
			Pivots.DeleteAll();
			DocAddresses.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (FreightRegistry.Instance.CanadaCargoControlNumberCustomization.Value == Enterprise.Core.Constants.ShipmentCCNCustomizationTypes.Code.NumberFountain)
			{
				if (BW_HouseCCN.IsEmpty)
				{
					int branchPrefix = FreightRegistry.Instance.CanadaCargoControlNumberBranchPrefix.Value;
					int branchPrefixDefault = FreightRegistry.Instance.CanadaCargoControlNumberBranchPrefix.DefaultValue;
					ZString fountainNumber = Env.NumberFountains.CanadaCargoControlNumber(GlbCompany.CurrentCompany.PK.ToGuid()).GetNextFormatted(Factory);

					if (branchPrefix != branchPrefixDefault)
					{
						int digits_in_branch_code = 2;
						BW_HouseCCN = ZString.Format("{0}{1}", branchPrefix.ToString(CultureInfo.CurrentCulture), fountainNumber.SubstringSafe(digits_in_branch_code));
					}
					else
					{
						BW_HouseCCN = fountainNumber;
					}
				}
			}
		}

		ACIEManifestForwaderStatusCalculator StatusCalculator
		{
			get { return statusCalculator ?? (statusCalculator = new ACIEManifestForwaderStatusCalculator(MessageTypeList.Descriptions.ACIHouseBill)); }
		}
		ACIEManifestForwaderStatusCalculator statusCalculator;

		public bool IsLodged
		{
			get { return StatusCalculator.IsLodged(BW_CustomsStatus); }
		}
		public bool IsAccepted
		{
			get { return StatusCalculator.IsAccepted(BW_CustomsStatus); }
		}

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateNumberPropertyIfRequired(BW_MessageReferenceInfo, x => GetNewMessageReference(x));

			possibleLicencePostRequired = false;
			if (!IsDeleted && Messages.Count > 0)
			{
				var newStatus = StatusCalculator.CalculatedJobStatus(this);
				if (BW_CustomsStatusInfo.OriginalValue.IsEmpty && !newStatus.IsEmpty)
				{
					possibleLicencePostRequired = true;
				}
				BW_CustomsStatus = newStatus;
			}
		}
		bool possibleLicencePostRequired;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				if (!this.IsInDatabase)
				{
					BW_MessageReference = ZString.Empty;
				}
				BW_MessageStatus = (ZString)BW_MessageStatusInfo.OriginalValue;
			}
			else if (possibleLicencePostRequired && LastSentMessage != null && !LastSentMessage.EM_IsTestMessage)
			{
				var loginUser = ZString.Empty;
				var userWhoQueuedThisRecord = LastSentMessage.UserWhoQueuedThisRecord;
				if (userWhoQueuedThisRecord != null)
				{
					loginUser = userWhoQueuedThisRecord.GS_LoginName;
				}

				var department = LastSentMessage.EM_GE;
				var houseBranchPK = MasterBill != null ? MasterBill.Branch.PK : GlbBranch.CurrentBranch.PK;
				using (Env.SetTemporaryUserContext(loginUser.IsEmpty ? Env.CurrentUser.LoginName : loginUser.ToString(),
																						houseBranchPK.ToGuid(),
																						department.IsValid ? department.ToGuid() : Env.CurrentDepartment.PK))
				{
					var logger = ObjectFactory.Get<ILicenceConsumptionLogCreator>();
					logger.CreateLog(Env.Licence.ACIeManifestReportingPerTransaction, true);
				}
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (!saveSucceeded)
			{
				foreach (EDIMessage message in Messages.ToArray())
				{
					if (message.IsTransmitMessage && !message.IsInDatabase && !message.IsDeleted)
					{
						message.Delete();
					}
				}
			}
		}

		ZString GetNewMessageReference(BusinessObjectFactory factory)
		{
			var result = ZString.Empty;
			var shipment = Shipment;
			if (shipment != null)
			{
				shipment.PopulateBillAndShipmentNumberIfNeeded();
				result = shipment.JS_UniqueConsignRef.Right(CusCAeMHHouse.Schema.BW_MessageReferenceMaxLength);
			}
			else
			{
				result = Env.NumberFountains.CAHouseBilleManifest.GetNextFormatted(factory);
			}
			return result;
		}

		#endregion

		#region IDocAddresses

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return true;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get { return DocAddresses; }
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return new CAeMHDocAddressRequirement(Factory, addressType);
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return new OrgHeaderCollection(Factory);
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new CusCAeMHHouseJobDocAddressValidation(addressToValidate);
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new[] {
					DocAddressType.ConsigneeDocumentaryAddress,
					DocAddressType.ConsignorDocumentaryAddress,
					DocAddressType.ConsigneePickupDeliveryAddress,
					DocAddressType.NotifyParty,
					DocAddressType.ImportBroker,
					DocAddressType.ReceivingForwarderAddress,
					DocAddressType.Carrier,
					DocAddressType.Warehouse
				};
			}
		}

		ZString IDocAddressesCaption.GetAddressCaption(JobDocAddress docAddress)
		{
			return CAeMHDocAddress.GetAddressCaption(docAddress.E2_AddressType);
		}

		#endregion

		#region IEDIMessageCollectionProvider

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get { return Messages; }
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		#endregion

		#region ISequenceNumberHeader

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines
		{
			get { return DocAddresses.Cast<ISequenceNumberLine>(); }
		}

		#endregion

		public const string JobIdentificationPrefix = "HBL-";

		#region IACIHouseBillProvider

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return true; }
		}

		BusinessObject IEDIFACTMessageAttachee.TopLevelBusinessObject
		{
			get { return this; }
		}

		void IEDIFACTMessageAttachee.AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			Messages.Add(message);
		}

		ZString IEDIFACTMessageAttachee.JobIdentification
		{
			get { return JobIdentificationPrefix + BW_MessageReference; }
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get { return BW_MessageStatus; }
			set { BW_MessageStatus = value; }
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get { return BW_CustomsStatus; }
			set { BW_CustomsStatus = value; }
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return MasterBill == null || !MasterBill.BP_IsActive; }
		}

		ZString IACIHouseBillProvider.HouseCCN
		{
			get { return BW_HouseCCN; }
		}

		ZString IACIHouseBillProvider.UCR
		{
			get { return BW_UCR; }
		}

		IEnumerable<ISecondaryNotifyParty> IACIHouseBillProvider.SecondaryNotifyParties
		{
			get
			{
				List<SnpObj> snpObjs = new List<SnpObj>();
				var snps = DocAddresses.FindDocAddressesByType(DocAddressType.ImportBroker);
				foreach (var snp in snps)
				{
					if (!snp.E2_GovRegNum.IsEmpty)
					{
						snpObjs.Add(new SnpObj(PartyFunctionCodeQualifierList.CustomsBroker, snp.E2_GovRegNum));
					}
				}
				snps = DocAddresses.FindDocAddressesByType(DocAddressType.ReceivingForwarderAddress);
				foreach (var snp in snps)
				{
					if (!snp.E2_GovRegNum.IsEmpty)
					{
						snpObjs.Add(new SnpObj(PartyFunctionCodeQualifierList.FreightForwarder, snp.E2_GovRegNum));
					}
				}
				snps = DocAddresses.FindDocAddressesByType(DocAddressType.Carrier);
				foreach (var snp in snps)
				{
					if (!snp.E2_GovRegNum.IsEmpty)
					{
						snpObjs.Add(new SnpObj(PartyFunctionCodeQualifierList.Carrier, snp.E2_GovRegNum));
					}
				}
				snps = DocAddresses.FindDocAddressesByType(DocAddressType.Warehouse);
				foreach (var snp in snps)
				{
					if (!snp.E2_GovRegNum.IsEmpty)
					{
						snpObjs.Add(new SnpObj(PartyFunctionCodeQualifierList.WarehouseKeeper, snp.E2_GovRegNum));
					}
				}
				return snpObjs;
			}
		}

		ZString IACIHouseBillProvider.MovementType
		{
			get { return BW_MovementType; }
		}

		ZString IACIHouseBillProvider.PrimaryCCN
		{
			get { return MasterBill == null ? ZString.Empty : MasterBill.BP_PrimaryCCN; }
		}

		ZString IACIHouseBillProvider.B2BComments
		{
			get { return BW_B2BComments; }
		}

		ZString IACIForwarderMessageProvider.AmendmentReason
		{
			get { return BW_AmendReasonCode; }
		}

		ZString IACIHouseBillProvider.TransportMode
		{
			get { return MasterBill == null ? string.Empty : MasterBill.Lookups.TransportModeList.GetDescriptionFromCode(MasterBill.BP_ModeOfTransport); }
		}

		ZBool IACIHouseBillProvider.IsConsolidatedCargo
		{
			get { return BW_IsMasterHouse; }
		}

		ZDecimal IACIHouseBillProvider.Volume
		{
			get { return BW_Volume; }
		}

		ZString IACIHouseBillProvider.VolumeUOM
		{
			get { return BW_VolumeUQ; }
		}

		ZString IACIHouseBillProvider.SpecialHandlingInstructions
		{
			get { return BW_HandlingInstructions; }
		}

		IJobDocAddress IACIHouseBillProvider.Consignee
		{
			get { return DocAddresses.FindByDocAddressType(DocAddressType.ConsigneeDocumentaryAddress); }
		}

		IJobDocAddress IACIHouseBillProvider.Shipper
		{
			get { return DocAddresses.FindByDocAddressType(DocAddressType.ConsignorDocumentaryAddress); }
		}

		IEnumerable<IJobDocAddress> IACIHouseBillProvider.DeliveryAddresses
		{
			get { return DocAddresses.FindDocAddressesByType(DocAddressType.ConsigneePickupDeliveryAddress); }
		}

		IEnumerable<IJobDocAddress> IACIHouseBillProvider.NotifyParties
		{
			get { return DocAddresses.FindDocAddressesByType(DocAddressType.NotifyParty); }
		}

		IJobDocAddress IACIHouseBillProvider.PlaceOfConsolidation => PlaceOfConsolidation;

		public JobDocAddress PlaceOfConsolidation
		{
			get
			{
				JobDocAddress result = null;
				if (BW_IsMasterHouse)
				{
					var housePOC = DocAddresses.FindByDocAddressType(DocAddressType.PlaceOfConsolidation);
					if (housePOC?.E2_Address1.IsEmpty ?? true)
					{
						result = MasterBill.PlaceOfConsolidation;
					}
					else
					{
						result = housePOC;
					}
				}
				return result != null && !result.E2_CompanyName.IsEmpty ? result : null;
			}
		}

		IJobDocAddress IACIHouseBillProvider.Consolidator => Consolidator;

		public JobDocAddress Consolidator
		{
			get
			{
				JobDocAddress result = null;
				if (BW_IsMasterHouse)
				{
					var houseCON = DocAddresses.FindByDocAddressType(DocAddressType.Consolidator);
					if (houseCON?.E2_Address1.IsEmpty ?? true)
					{
						result = MasterBill.Consolidator;
					}
					else
					{
						result = houseCON;
					}
				}
				return result != null && !result.E2_CompanyName.IsEmpty ? result : null;
			}
		}

		IOrgContact IACIHouseBillProvider.UNDGContact
		{
			get
			{
				foreach (var item in Items)
				{
					foreach (var undg in item.UNDGs)
					{
						if (undg.DGContact != null && !undg.DGContact.OC_ContactName.IsEmpty)
						{
							return undg.DGContact;
						}
					}
				}
				return null;
			}
		}

		ZString IACIHouseBillProvider.ReleasePortCode
		{
			get { return BW_CBSAReleasePort; }
		}

		ZString IACIHouseBillProvider.ReleaseSubLocationCode
		{
			get { return this.BW_CBSAReleaseSubLocation; }
		}

		ZString IACIHouseBillProvider.DischargePortCode
		{
			get { return MasterBill == null ? ZString.Empty : MasterBill.BP_CBSADischargePort; }
		}

		ZString IACIHouseBillProvider.DischargeSubLocationCode
		{
			get { return MasterBill == null ? ZString.Empty : MasterBill.BP_CBSADischargeSubLocation; }
		}

		ZString IACIHouseBillProvider.DGSpecialInstructions
		{
			get { return BW_DGSpecialInstructions; }
		}

		IEnumerable<IHouseBillContainer> IACIHouseBillProvider.Containers
		{
			get
			{
				return Pivots.Where(x => x.Container != null && x.Container.BQ_ContainerNumber != ZString.Empty && !x.Container.IsNonContainerized);
			}
		}

		IEnumerable<IHouseBillLine> IACIHouseBillProvider.Lines
		{
			get { return Items; }
		}

		ZDecimal IACIHouseBillProvider.TotalWeight
		{
			get { return BW_Weight; }
		}

		ZString IACIHouseBillProvider.TotalWeightUOM
		{
			get { return BW_WeightUQ; }
		}

		#endregion

		#region IMessageManagerEventHandler

		void IMessageManagerEventHandler.OnMessageQueuedForSending()
		{
			if (Shipment != null)
			{
				StatusLogManager.AddCustomsCommencedEvent(Shipment.Logs, ApplicationCodeList.Codes.CAACI);
			}
		}

		#endregion

		#region clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(CusCAeMHHouseSchema.Constants.BW_HouseBill);
			result.Add(CusCAeMHHouseSchema.Constants.BW_HouseCCN);
			result.Add(CusCAeMHHouseSchema.Constants.BW_OverrideFreightDefaults);
			result.Add(CusCAeMHHouseSchema.Constants.BW_CustomsStatus);
			result.Add(CusCAeMHHouseSchema.Constants.BW_MessageStatus);
			result.Add(CusCAeMHHouseSchema.Constants.BW_MessageReference);
			result.Add(CusCAeMHHouseSchema.Constants.BW_UCR);
			result.Add(CusCAeMHHouseSchema.Constants.BW_AmendReasonCode);
			result.Add(CusCAeMHHouseSchema.Constants.BW_BP_Master);
			result.Add(CusCAeMHHouseSchema.Constants.BW_ParentID);
			result.Add(CusCAeMHHouseSchema.Constants.BW_DGSpecialInstructions);
			result.Add(CusCAeMHHouseSchema.Constants.BW_HandlingInstructions);
			result.Add(CusCAeMHHouseSchema.Constants.BW_IsCloseReported);
			result.Add(CusCAeMHHouseSchema.Constants.TableName);
			return result;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var templateCopy = (CusCAeMHHouse)base.CloneInternal(args);
			foreach (var address in DocAddresses)
			{
				templateCopy.DocAddresses.Add(address.Clone());
			}
			foreach (var item in Items)
			{
				var itemCopy = (CusCAeMHItem)item.Clone();
				itemCopy.BX_BW_House = templateCopy.PK;
			}
			return templateCopy;
		}

		#endregion

		#region Synchronise

		internal bool ShouldSynchroniseWithShipment
		{
			get { return Shipment != null && !BW_OverrideFreightDefaults && BW_MessageStatus.IsEmpty; }
		}

		internal CusCAeMHHouseSynchroniser ShipmentSynchroniser
		{
			get { return fShipmentSynchroniser ?? (fShipmentSynchroniser = new CusCAeMHHouseSynchroniser(this, Shipment)); }
		}
		CusCAeMHHouseSynchroniser fShipmentSynchroniser;

		public void EnableAndSynchronise(bool forceSync = false)
		{
			if (Shipment != null)
			{
				using (GetValidationSuspender())
				{
					ShipmentSynchroniser.SetEnabled(ShouldSynchroniseWithShipment, ShipmentSynchroniser.DetectEnabled);
					if (forceSync)
					{
						ShipmentSynchroniser.Synchronise(forceSync);
					}
					else
					{
						ShipmentSynchroniser.Synchronise();
					}
				}
			}
		}

		void DisposeSynchroniser()
		{
			if (fShipmentSynchroniser != null)
			{
				fShipmentSynchroniser.SetEnabled(false, fShipmentSynchroniser.DetectEnabled);
				fShipmentSynchroniser.Dispose();
				fShipmentSynchroniser = null;
			}
		}

		public void HookShipmentSynchronisingEvent()
		{
			UnHookShipmentSynchronisingEvent();
			this.BW_OverrideFreightDefaultsInfo.ValueChanged += OnEventCausingReSynchronising;
			this.Messages.CountChanged += OnEventCausingReSynchronising;
		}

		public void UnHookShipmentSynchronisingEvent()
		{
			this.BW_OverrideFreightDefaultsInfo.ValueChanged -= OnEventCausingReSynchronising;
			this.Messages.CountChanged -= OnEventCausingReSynchronising;
		}

		void OnEventCausingReSynchronising(object sender, EventArgs e)
		{
			MasterBill.EnableAndSynchronise();
			EnableAndSynchronise();
		}

		[ReadOnlyMember(nameof(BW_DGSpecialInstructions_ReadOnly))]
		public override ZString BW_DGSpecialInstructions
		{
			get => base.BW_DGSpecialInstructions;
			set => base.BW_DGSpecialInstructions = value;
		}

		bool BW_DGSpecialInstructions_ReadOnly
		{
			get
			{
				return ShouldSynchroniseWithShipment;
			}
		}

		[ReadOnlyMember(nameof(BW_HandlingInstructions_ReadOnly))]
		public override ZString BW_HandlingInstructions
		{
			get => base.BW_HandlingInstructions;
			set => base.BW_HandlingInstructions = value;
		}

		bool BW_HandlingInstructions_ReadOnly
		{
			get
			{
				return ShouldSynchroniseWithShipment;
			}
		}

		#endregion

		#region bool flag

		public bool IsAttachedToAShipment
		{
			get
			{
				return Shipment != null && Shipment.Consols != null && Shipment.Consols.Count > 0;
			}
		}

		public bool IsCustomsStatusActive
		{
			get
			{
				return !string.IsNullOrWhiteSpace(BW_CustomsStatus)
				  && BW_CustomsStatus != EManifestForwarderJobStatusList.Codes.Cancelled;
			}
		}

		public bool IsMessageStatusAwaiting
		{
			get
			{
				return BW_MessageStatus == MessageStatusList.Codes.AwaitingChange ||
					BW_MessageStatus == MessageStatusList.Codes.AwaitingDelete ||
					BW_MessageStatus == MessageStatusList.Codes.AwaitingOriginal;
			}
		}

		public bool IsCloseReported
		{
			get
			{
				return BW_IsCloseReported;
			}
		}

		#endregion

		#region ICanDelete Members
		bool ICanDelete.CanDelete
		{
			get { return !IsAttachedToAShipment && !IsCustomsStatusActive && !IsMessageStatusAwaiting && !IsCloseReported && MessagesForDisplay.Count == 0; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get
			{
				if (IsAttachedToAShipment)
				{
					return ReasonForCannotDeleteAttachedToAShipment;
				}
				else if (IsCustomsStatusActive)
				{
					return ReasonForCannotDeleteCustomsStatusActive;
				}
				else if (IsMessageStatusAwaiting)
				{
					return ReasonForCannotDeleteMessageStatusAwaiting;
				}
				else if (MessagesForDisplay.Count > 0)
				{
					return ReasonForCannotDeleteMessagesAttached;
				}
				else
				{
					return ReasonForCannotDeleteCloseReported;
				}
			}
		}

		internal static MultilingualString ReasonForCannotDeleteAttachedToAShipment
		{
			get { return ResString.GetMultilingualString("CusCAeMHHouse|6ff74ab0-669e-4c0b-bbc5-ad6dcc878491", "This Bill Of Lading is attached to a shipment. \r\nPlease detach it in the consol and try again.."); }
		}

		internal static MultilingualString ReasonForCannotDeleteCustomsStatusActive
		{
			get { return ResString.GetMultilingualString("CusCAeMHHouse|e11de080-099c-470e-b1f8-0c790501ef94", "This Bill Of Lading is already registered with Customs.\r\nPlease send a withdrawal message and try again."); }
		}

		internal static MultilingualString ReasonForCannotDeleteMessageStatusAwaiting
		{
			get { return ResString.GetMultilingualString("CusCAeMHHouse|44851b40-10bd-4c4f-8b0b-d2c0009d4dde", "This Bill Of Lading is awaiting message response from Customs.\r\nPlease wait till the message response arrives and try again."); }
		}

		internal static MultilingualString ReasonForCannotDeleteCloseReported
		{
			get { return ResString.GetMultilingualString("CusCAeMHHouse|5c3c0486-e1f9-42ec-9de0-3499460ec909", "This Bill Of Lading is close reported.\r\nPlease send a withdrawal message and try again."); }
		}

		internal static MultilingualString ReasonForCannotDeleteMessagesAttached
		{
			get { return ResString.GetMultilingualString("CusCAeMHHouse|080955D8-08A3-48A3-A87D-D4566E9558C6", "This Bill Of Lading can not be deleted since it has attached messages."); }
		}

		#endregion

		#region Notice Messages

		public EDIMessageForDisplayCollection<EDIMessage> MessagesForDisplay
		{
			get
			{
				if (messagesForDisplay == null)
				{
					var query = new ZQuery();
					query.AddToFilter(EDIMessageQueryHelper.GetEDIMessageGenPivotQuery(new[] { PK }, new ZString[] { EDIMessageSubTypeList.Codes.XmlUniversalEvent, UniversalEventMessageTypes.Codes.D4Notices }));
					query.AddToFilter(Messages.CompleteFilter, JoinCondition.Or);

					messagesForDisplay = new EDIMessageForDisplayCollection<EDIMessage>(Factory, query);
					messagesForDisplay.Load();
					Messages.CountChanged += Messages_CountChanged;
				}

				return messagesForDisplay;
			}
		}
		EDIMessageForDisplayCollection<EDIMessage> messagesForDisplay;

		void Messages_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded && e.BizObject != null)
			{
				MessagesForDisplay.Add(e.BizObject);
				MessagesForDisplay.RefreshBinding();
			}
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (IsInDatabase)
			{
				if (BW_MessageStatusInfo.HasChanges)
				{
					Logs.AddNew(Events.MessageStatusChange, ZString.Format("{0} - {1} House", BW_MessageStatus, new MessageStatusList().GetDescriptionFromCode(BW_MessageStatus)));
				}
				if (BW_CustomsStatusInfo.HasChanges)
				{
					Logs.AddNew(Events.StatusChange, ZString.Format("{0} - {1} House", BW_CustomsStatus, new EManifestForwarderJobStatusList().GetDescriptionFromCode(BW_CustomsStatus)));
				}
			}
		}

		public BusinessObject GetTopBusinessObject()
		{
			return Factory.Load<CusCAeMHMaster>(new ZGuid(BW_BP_Master));
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("C6FC4FB9-F171-469D-A85F-9EB86A84B536", "eManifest House - {0}", BW_HouseCCN);
				return result;
			}
		}

		#region IWorkflowTriggerEventSource

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return MasterBill?.Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var shipment = Shipment;
				return shipment != null ? new IWorkflowProviderCore[] { shipment } : Array.Empty<IWorkflowProviderCore>();
			}
		}
		#endregion // IWorkflowTriggerEventSource
	}

	class SnpObj : ISecondaryNotifyParty
	{
		public SnpObj(PartyFunctionCodeQualifierList qualifer, ZString identifier)
		{
			SecondaryNotifyType = qualifer;
			Identifier = identifier;
		}

		public PartyFunctionCodeQualifierList SecondaryNotifyType { get; private set; }
		public ZString Identifier { get; private set; }
		public ZString NoticeType
		{
			get { return "MF"; }
		}
	}
}
