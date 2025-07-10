using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageManagers;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAHouse : BaseCusSCAHouse, IWorkflowTriggerEventSource, ISupplementaryCargoReport, IMessageManagerEventHandler
	{
		public const string NonContaineriseID = Core.Constants.ContainerModes.NonContainerised;

		public CusSCAHouse(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public static new readonly TypeDecider TypeDecider = new CusSCAHouseTypeDecider();

		#region new properties

		public bool ShouldSynchronizeWithShipment
		{
			get { return !CA_OverrideFreightDefaults && (CA_ShipmentStatus.IsEmpty || CA_ShipmentStatus == SupplementaryCargoReportJobStatusList.Codes.Cancelled); }
		}

		public CusSCAHouseSynchroniser ShipmentSynchroniser
		{
			get
			{
				if (fShipmentSynchroniser == null)
				{
					if (Shipment == null)
					{
						throw new NotSupportedException("You can't synchronise with a Shipment when you don't have a Shipment.");
					}

					fShipmentSynchroniser = new CusSCAHouseSynchroniser(this, Shipment);
				}
				return fShipmentSynchroniser;
			}
		}
		CusSCAHouseSynchroniser fShipmentSynchroniser;

		public void EnableAndSynchronise(bool forceSynch = false)
		{
			if (Shipment != null)
			{
				using (GetValidationSuspender())
				{
					ShipmentSynchroniser.SetEnabled(ShouldSynchronizeWithShipment, ShipmentSynchroniser.DetectEnabled);
					if (forceSynch)
					{
						ShipmentSynchroniser.Synchronise(forceSynch);
					}
					else
					{
						ShipmentSynchroniser.Synchronise();
					}
				}
			}
			SetReadOnlyWhereRequired();
		}

		public bool IsAttachedToShipment
		{
			get { return Shipment != null; }
		}

		public ZString MessageStatusDescription
		{
			get { return new MessageStatusList(MessageTypeList.Descriptions.SupplementaryCargoReport).GetDescriptionFromCode(CA_MessageStatus); }
		}

		public ZPropertyInfo MessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(MessageStatusDescription)); }
		}

		public ZString ShipmentStatusDescription
		{
			get { return new SupplementaryCargoReportJobStatusList().GetDescriptionFromCode(CA_ShipmentStatus); }
		}

		public ZPropertyInfo ShipmentStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ShipmentStatusDescription)); }
		}

		public CusEntryNumber SRNCusEntryNum
		{
			get
			{
				if (sRNCusEntryNum == null)
				{
					var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, this.PK);
					filter.AddToFilter(CusEntryNumSchema.CE_EntryType, CanadaAdditionalReferenceNumberTypes.Codes.CCN);
					sRNCusEntryNum = Factory.LoadTop1<CusEntryNumber>(filter);
					if (sRNCusEntryNum == null)
					{
						sRNCusEntryNum = CusEntryNumber.New(this, CanadaAdditionalReferenceNumberTypes.Codes.CCN, Core.Constants.CountryCodes.Canada);
					}
					RegisterEditableChildObject(sRNCusEntryNum);
					sRNCusEntryNum.CE_ParentIDInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					sRNCusEntryNum.CE_ParentTableInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					sRNCusEntryNum.CE_RN_NKCountryCodeInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					sRNCusEntryNum.CE_CategoryInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					sRNCusEntryNum.CE_EntryNumInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					sRNCusEntryNum.CE_EntryTypeInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
				}
				return sRNCusEntryNum;
			}
		}
		CusEntryNumber sRNCusEntryNum;

		#endregion

		#region overrides

		public new CusSCAHouseValidation Validation
		{
			get { return (CusSCAHouseValidation)base.Validation; }
		}

		protected override Customs.Business.CusSCAHouseValidation GetNewValidation()
		{
			return new CusSCAHouseValidation(this);
		}

		protected override Enterprise.Messaging.Business.EDIMessageCollection CreateEDIMessageCollection()
		{
			return new EDIMessageCollection(this);
		}

		public bool isCalledFromConsolPlugin;
		public new CusSCAOceanBill OceanBill
		{
			get
			{
				var result = (CusSCAOceanBill)base.OceanBill;
				if (!isCalledFromConsolPlugin && result != null && !result.IsDeleted && IsAttachedToShipment && !result.IsRegisteredEditableChildObject(this))
				{
					result.RegisterEditableChildObject(this);
				}
				return result;
			}
		}

		protected override IEnumerable<BaseCusSCAPivot> GetCusSCAPivotCollection() => PackLines;

		[ChildEditable()]
		public CusSCAPivotCollectionForHouse PackLines
		{
			get
			{
				if (packLines == null)
				{
					packLines = new CusSCAPivotCollectionForHouse(this);
					packLines.SetUNDGsReadOnly(ShouldSynchronizeWithShipment);
					RegisterEditableChildObject(packLines);
				}
				return packLines;
			}
		}
		CusSCAPivotCollectionForHouse packLines;

		protected override Customs.Business.CusSCAHouseLookups GetNewLookups()
		{
			return new CusSCAHouseLookups(this);
		}

		public new CusSCAHouseLookups Lookups
		{
			get { return (CusSCAHouseLookups)base.Lookups; }
		}

		public override CargoAddressType ConsigneeAddressType
		{
			get { return CargoAddressType.All; }
		}

		public override CargoAddressType ConsignorAddressType
		{
			get { return CargoAddressType.All; }
		}

		public override ZString CA_NotifyName
		{
			get { return base.CA_NotifyName; }
			set
			{
				var oldValue = base.CA_NotifyName;
				base.CA_NotifyName = value;
				if ((!IsCopying && !CA_NotifyName.IsEmpty && CA_NotifyName != oldValue))
				{
					this.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CA_DeliveryName
		{
			get { return base.CA_DeliveryName; }
			set
			{
				var oldValue = base.CA_DeliveryName;
				base.CA_DeliveryName = value;
				if ((!IsCopying && !CA_DeliveryName.IsEmpty && CA_DeliveryName != oldValue))
				{
					this.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAHouseLookups.ConsigneeState_List_For_Country))]
		public override ZString CA_ConsigneeState
		{
			get { return base.CA_ConsigneeState; }
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAHouseLookups.ConsignorState_List_For_Country))]
		public override ZString CA_ConsignorState
		{
			get { return base.CA_ConsignorState; }
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAHouseLookups.NotifyState_List_For_Country))]
		public override ZString CA_NotifyState
		{
			get { return base.CA_NotifyState; }
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAHouseLookups.DeliveryState_List_For_Country))]
		public override ZString CA_DeliveryState
		{
			get { return base.CA_DeliveryState; }
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAHouseLookups.InTransitCodeList))]
		public override ZString CA_FROBTransitImportCode
		{
			get { return base.CA_FROBTransitImportCode; }
			set
			{
				var oldValue = base.CA_FROBTransitImportCode;
				if (!IsCopying && oldValue != value)
				{
					base.CA_FROBTransitImportCode = value;
					if (!IsSettingDefaults && OnApplicationTypeChanged != null)
					{
						OnApplicationTypeChanged(this, new ValueChangedEventArgs(oldValue, CA_FROBTransitImportCodeInfo));
					}
				}
			}
		}

		public event EventHandler<ValueChangedEventArgs> OnApplicationTypeChanged;
		internal bool IsSettingDefaults { get; set; }

		[ReadOnly(true)]
		public override ZString CA_MessageStatus
		{
			get { return base.CA_MessageStatus; }
		}

		[ReadOnly(true)]
		public override ZString CA_ShipmentStatus
		{
			get { return base.CA_ShipmentStatus; }
		}

		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZString CA_RL_NK_PortOfDestination
		{
			get { return base.CA_RL_NK_PortOfDestination; }
		}

		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZGuid CA_OH_Consignee
		{
			get { return base.CA_OH_Consignee; }
		}

		protected override bool ConsigneeAddressLocked
		{
			get { return base.ConsigneeAddressLocked || ShouldSynchronizeWithShipment; }
		}

		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZGuid CA_OH_Consignor
		{
			get { return base.CA_OH_Consignor; }
		}

		protected override bool ConsignorAddressLocked
		{
			get { return base.ConsignorAddressLocked || ShouldSynchronizeWithShipment; }
		}

		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZGuid CA_OH_Notify
		{
			get { return base.CA_OH_Notify; }
		}

		protected override bool NotifyAddressLocked
		{
			get { return base.NotifyAddressLocked || ShouldSynchronizeWithShipment; }
		}

		[ReadOnlyMember(ShouldSynchronizeWithShipmentConst)]
		public override ZGuid CA_OA_DeliveryAddress
		{
			get { return base.CA_OA_DeliveryAddress; }
		}

		protected override bool DeliveryAddressLocked
		{
			get { return base.DeliveryAddressLocked || ShouldSynchronizeWithShipment; }
		}
		protected override void OnOverrideFreightDefaultChanged()
		{
			base.OnOverrideFreightDefaultChanged();
			if (OceanBill != null)
			{
				OceanBill.EnableAndSynchronise();
			}

			EnableAndSynchronise();
		}

		public void SetReadOnlyWhereRequired()
		{
			PackLines.SetUNDGsReadOnly(ShouldSynchronizeWithShipment);
		}

		protected override void OnMessagesCountChanged()
		{
			base.OnMessagesCountChanged();
			EnableAndSynchronise();
			RefreshBindingIncludingChildren();
		}

		protected override bool StateProvinceIsSupported
		{
			get { return true; }
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			possibleLicencePostRequired = false;
			if (!IsDeleted && Messages.Count > 0)
			{
				var newStatus = new SupplementaryCargoReportStatusCalculator().CalculatedJobStatus(this);
				if (CA_ShipmentStatusInfo.OriginalValue.IsEmpty && !newStatus.IsEmpty)
				{
					possibleLicencePostRequired = true;
				}
				CA_ShipmentStatus = newStatus;
			}
		}
		bool possibleLicencePostRequired;

		public Enterprise.Messaging.Business.EDIMessage LastSentMessage
		{
			get
			{
				if (lastSentMessage == null)
				{
					lastSentMessage = new CachedProperty<Enterprise.Messaging.Business.EDIMessage>(Factory, delegate
					{
						return Messages.GetLastMessage(EDIMessage.ApplicationCodes.CAACI, MessageTypeList.Codes.SupplementaryCargoReport, EDIMessage.Direction.Transmit, EDIMessage.Status.Sent);
					});
				}
				return lastSentMessage.Value;
			}
		}
		CachedProperty<Enterprise.Messaging.Business.EDIMessage> lastSentMessage;

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded)
			{
				CA_MessageStatus = (ZString)CA_MessageStatusInfo.OriginalValue;
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
				var houseBranchPK = OceanBill != null ? OceanBill.Branch.PK : GlbBranch.CurrentBranch.PK;
				using (Env.SetTemporaryUserContext(loginUser.IsEmpty ? Env.CurrentUser.LoginName : loginUser.ToString(),
																						houseBranchPK.ToGuid(),
																						department.IsValid ? department.ToGuid() : Env.CurrentDepartment.PK))
				{
					var logger = ObjectFactory.Get<ILicenceConsumptionLogCreator>();
					logger.CreateLog(Env.Licence.ACIReportingPerTransaction, true);
				}
			}
		}

		public override void Delete()
		{
			PackLines.DeleteAll();
			OnDelete?.Invoke(this, EventArgs.Empty);

			base.Delete();
		}

		public event EventHandler OnDelete;

		#endregion

		#region ICAEDIFACTMessageAttachee Members

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
			get { return CA_HouseBill; }
		}

		ZString IEDIFACTMessageAttachee.MessageStatus
		{
			get { return CA_MessageStatus; }
			set { CA_MessageStatus = value; }
		}

		ZString IEDIFACTMessageAttachee.JobStatus
		{
			get { return CA_ShipmentStatus; }
			set { CA_ShipmentStatus = value; }
		}

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages
		{
			get { return Messages; }
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return false; }
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return true; }
		}

		#endregion

		#region ISupplementaryCargoReport Members
		ZString ISupplementaryCargoReport.DocumentMessageNumber
		{
			get { return CA_BGMReference; }
			set { CA_BGMReference = value; }
		}

		ZString ISupplementaryCargoReport.ServiceOption
		{
			get { return ServiceOptions.Codes.SupplementaryCargoReport; }
		}

		ZString ISupplementaryCargoReport.ModeOfTransport
		{
			get
			{
				if (OceanBill != null)
				{
					if (OceanBill.IsAir)
					{
						return CBSATransportTypeList.Descriptions.Air;
					}

					if (OceanBill.IsSea)
					{
						return CBSATransportTypeList.Descriptions.Sea;
					}

					if (OceanBill.IsRail)
					{
						return CBSATransportTypeList.Descriptions.Rail;
					}

					if (OceanBill.IsRoad)
					{
						return CBSATransportTypeList.Descriptions.Road;
					}
				}
				return "0";
			}
		}

		public ZString CarrierCode => Factory.CanadianCarrierCode();

		public ZPropertyInfo CarrierCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CarrierCode)); }
		}

		ZString ISupplementaryCargoReport.OriginalCargoControlNumber
		{
			get { return OriginalCCN; }
		}

		[BusinessObjectTestExclude()]
		[MaxLength(25)]
		public ZString OriginalCCN
		{
			get { return OceanBill != null ? OceanBill.OriginalCCN : ZString.Empty; }
			set
			{
				CheckMaximumLength(OriginalCCNInfo, value);
				if (OceanBill != null)
				{
					OceanBill.OriginalCCN = value;
				}
				OriginalCCNInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OriginalCCNInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalCCN)); }
		}

		ZString ISupplementaryCargoReport.UniqueConsignmentReference
		{
			get { return ZString.Empty; }
		}

		[ReadOnlyMember(nameof(IsAttachedToShipment))]
		[MaxLength(25)]
		public ZString SupplementaryReferenceNumber
		{
			get { return SRNCusEntryNum.CE_EntryNum; }
			set
			{
				CheckMaximumLength(SupplementaryReferenceNumberInfo, value);
				SRNCusEntryNum.CE_EntryNum = value;
				SupplementaryReferenceNumberInfo.RefreshBinding();
				MarkAsNeedingValidation();
			}
		}

		public ZPropertyInfo SupplementaryReferenceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(SupplementaryReferenceNumber)); }
		}

		ZString ISupplementaryCargoReport.DestinationCountryCode
		{
			get { return CA_RL_NK_PortOfDestination.Substring(0, 2); }
		}

		ZString ISupplementaryCargoReport.DestinationCityName
		{
			get
			{
				var result = ZString.Empty;
				if (_PortOfDestination != null)
				{
					result = _PortOfDestination.RL_PortName;
					ZInt i = result.IndexOf("/");
					if (i > 0)
					{
						result = result.SubstringSafe(i + 1);
					}
				}
				return result;
			}
		}

		public ZString BillOfLading
		{
			get { return OceanBill != null ? OceanBill.CB_OceanBill : ZString.Empty; }
		}

		ZString ISupplementaryCargoReport.DestinationPortName
		{
			get { return CA_CargoFacilityLocation; }
		}

		ZString ISupplementaryCargoReport.CustomsProcedureCode
		{
			get { return CA_FROBTransitImportCode; }
		}

		ZString ISupplementaryCargoReport.SpecialInstructions
		{
			get { return CA_SpecialInstructions; }
		}

		ZString ISupplementaryCargoReport.Authentication
		{
			get { return CA_AuthenticationCode; }
		}

		IAddressForACI ISupplementaryCargoReport.Consignee
		{
			get
			{
				return new AddressForACI(CA_ConsigneeName,
					CA_ConsigneeAddress1, CA_ConsigneeAddress2, CA_ConsigneeSuburb, CA_ConsigneeState, CA_ConsigneePostcode, CA_RN_NKConsigneeCountryCode,
					CA_ConsigneePhone, CA_ConsigneeContactName);
			}
		}

		IAddressForACI ISupplementaryCargoReport.Consignor
		{
			get
			{
				return new AddressForACI(CA_ConsignorName,
					CA_ConsignorAddress1, CA_ConsignorAddress2, CA_ConsignorSuburb, CA_ConsignorState, CA_ConsignorPostcode, CA_RN_NKConsignorCountryCode,
					CA_ConsignorPhone, CA_ConsignorContactName);
			}
		}

		IAddressForACI ISupplementaryCargoReport.NotifyParty
		{
			get
			{
				return new AddressForACI(CA_NotifyName,
					CA_NotifyAddress1, CA_NotifyAddress2, CA_NotifySuburb, CA_NotifyState, CA_NotifyPostcode, CA_RN_NKNotifyCountryCode,
					CA_NotifyPhone, CA_NotifyContactName);
			}
		}

		IAddressForACI ISupplementaryCargoReport.DeliveryParty
		{
			get
			{
				return new AddressForACI(CA_DeliveryName,
					CA_DeliveryAddress1, CA_DeliveryAddress2, CA_DeliverySuburb, CA_DeliveryState, CA_DeliveryPostcode, CA_RN_NKDeliveryCountryCode,
					CA_DeliveryPhone, CA_DeliveryContactName);
			}
		}

		public IEnumerable<ISCRContainer> Containers
		{
			get
			{
				if (OceanBill != null && !OceanBill.IsAir)
				{
					var containerNums = new List<ZString>();
					containerNums.Add(NonContaineriseID);
					foreach (var pivot in PackLines)
					{
						if (pivot.Container != null && !containerNums.Contains(pivot.Container.CN_ContainerNumber))
						{
							containerNums.Add(pivot.Container.CN_ContainerNumber);
							yield return pivot.Container;
						}
					}
				}
			}
		}

		public IEnumerable<ISCRLine> GoodsLines
		{
			get
			{
				foreach (var packLine in PackLines)
				{
					if (packLine.CN_ContainerMode != Core.Constants.ContainerModes.Empty)
					{
						yield return packLine;
					}
				}
			}
		}

		#endregion

		#region Implementation of IMessageManagerEventHandler

		void IMessageManagerEventHandler.OnMessageQueuedForSending()
		{
			if (IsAttachedToShipment)
			{
				StatusLogManager.AddCustomsCommencedEvent(Shipment.Logs, ApplicationCodeList.Codes.CAACI);
			}
		}

		#endregion

		const string ShouldSynchronizeWithShipmentConst = "ShouldSynchronizeWithShipment";

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (IsInDatabase)
			{
				if (CA_MessageStatusInfo.HasChanges)
				{
					Logs.AddNew(Events.MessageStatusChange, ZString.Format("{0} - {1}", CA_MessageStatus, MessageStatusDescription));
				}

				if (CA_ShipmentStatusInfo.HasChanges)
				{
					Logs.AddNew(Events.StatusChange, ZString.Format("{0} - {1}", CA_ShipmentStatus, ShipmentStatusDescription));
				}
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("7365108A-55E6-4570-8D3E-36B420FF7938", "ACI Supplementary");
				var parameters = new ZStringBuilder();
				parameters.AppendIfNotEmpty(Res.GetString("89205E4B-C04E-4DAD-BC61-67030F081994", "HBL") + ": ", CA_HouseBill);
				parameters.AppendIfNotEmpty(Res.GetString("F8B91116-4694-470A-8188-56CB85279222", "MHB") + ": ", CA_MasterHouseBill);
				if (!parameters.IsEmpty)
				{
					result += " (" + parameters.ToStringWithDelimiterBetweenAppends(" ") + ")";
				}
				return result;
			}
		}

		#region IWorkflowTriggerEventSource

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return OceanBill?.Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var consol = Consol;
				return consol != null ? new IWorkflowProviderCore[] { consol } : Array.Empty<IWorkflowProviderCore>();
			}
		}
		#endregion // IWorkflowTriggerEventSource
	}
}
