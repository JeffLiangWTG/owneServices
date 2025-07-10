using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.JP.AFR.Business
{
	[SingleObjectAroundARow]
	[UniversalDataContext(DataContextType.AFRBill)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class JPAFRBills : AutoJPAFRBills,
		INotificationForwardingPartySequenceNumberHeader,
		Integration.Customs.JP.AFR.IJPAFRBills,
		IUNDGSubstancePivotParent,
		IDocAddresses,
		ICanDelete,
		ISynchroniserReadOnlyMembersProvider,
		ICusCodeDataTypeSupporter,
		ICusAddInfoTypeSupporter,
		IWorkflowTriggerEventSource,
		ISailingSynchronisationTarget<BillOfLading>,
		IUNDGDataItemProvider
	{
		public JPAFRBills(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new class Schema : AutoJPAFRBills.Schema
		{
			public const string JPB_ReleaseStatusDescription = "JPB_ReleaseStatusDescription";
			public const string JPB_MessageStatusDescription = "JPB_MessageStatusDescription";
			public const string JPB_Calc_ArrivalBondedAreaCode = "JPB_Calc_ArrivalBondedAreaCode";
			public const string JPB_Calc_EFDT = "JPB_Calc_EFDT";
			public const string JPB_Calc_ESDT = "JPB_Calc_ESDT";
			public const string JPB_Calc_GoodsValue = "JPB_Calc_GoodsValue";
			public const string JPB_Calc_RX_NKGoodsValueCurrency = "JPB_Calc_RX_NKGoodsValueCurrency";
			public const string JPB_Calc_TemporaryLandingDuration = "JPB_Calc_TemporaryLandingDuration";
			public const string JPB_Calc_TemporaryLandingReason = "JPB_Calc_TemporaryLandingReason";
			public const string JPB_Calc_TransportMode = "JPB_Calc_TransportMode";
			public const string JPB_Calc_GeneralCustomsTransitApprovalNumber = "JPB_Calc_GeneralCustomsTransitApprovalNumber";

			public const string JPB_NoOfAFRContainers = "JPB_NoOfAFRContainers";
			public const string JPB_NoOfSailingContainers = "JPB_NoOfSailingContainers";
		}
		#endregion

		#region Calculated InBondDetails Properties

		#region JPB_Calc_ArrivalBondedAreaCode
		[ResourceStringData("JPAFRBills|JPB_Calc_ArrivalBondedAreaCode", Caption = "Arrival Place")]
		[MaxLength(JPAFRInBondDetails.Schema.JPI_ArrivalBondedAreaCodeMaxLength)]
		public ZString JPB_Calc_ArrivalBondedAreaCode
		{
			get
			{
				var inBondDetails = InBondDetails;
				return inBondDetails == null ? ZString.Empty : inBondDetails.JPI_ArrivalBondedAreaCode;
			}
			set
			{
				value = value.TrimEnd(' ', '\t');
				CheckMaximumLength(JPB_Calc_ArrivalBondedAreaCodeInfo, value);
				var inBondDetails = InBondDetails;
				var oldValue = inBondDetails == null ? ZString.Empty : inBondDetails.JPI_ArrivalBondedAreaCode;
				if (!value.IsEmpty && inBondDetails == null)
				{
					inBondDetails = CreateNewInBondDetails();
				}
				if (inBondDetails != null)
				{
					inBondDetails.JPI_ArrivalBondedAreaCode = value;
				}
				JPB_Calc_ArrivalBondedAreaCodeInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJPB_Calc_ArrivalBondedAreaCode();
				}
			}
		}

		public ZPropertyInfo JPB_Calc_ArrivalBondedAreaCodeInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_Calc_ArrivalBondedAreaCode); }
		}
		#endregion

		#region JPB_Calc_ESDT
		[ResourceStringData("JPAFRBills|JPB_Calc_ESDT", Caption = "Estimated Start Date", ShortCaption = "Trans. ETD", FullDescription = "Estimated Start Date of Transportation.")]
		public ZDateTime JPB_Calc_ESDT
		{
			get
			{
				var inBondDetails = InBondDetails;
				return inBondDetails == null ? ZDateTime.Empty : inBondDetails.JPI_ESDT;
			}
			set
			{
				var inBondDetails = InBondDetails;
				var oldValue = inBondDetails == null ? ZDateTime.Empty : inBondDetails.JPI_ESDT;
				if (!value.IsEmpty && inBondDetails == null)
				{
					inBondDetails = CreateNewInBondDetails();
				}
				if (inBondDetails != null)
				{
					inBondDetails.JPI_ESDT = value;
				}
				PopulateTemporaryLandingDurationIfPossible(value, this.JPB_Calc_EFDT);
				JPB_Calc_ESDTInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJPB_Calc_ESDT();
				}
			}
		}

		public ZPropertyInfo JPB_Calc_ESDTInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_Calc_ESDT); }
		}
		#endregion

		#region JPB_Calc_EFDT
		[ResourceStringData("JPAFRBills|JPB_Calc_EFDT", Caption = "Estimated Finish Date", ShortCaption = "Trans. ETA", FullDescription = "Estimated Finish Date of Transportation.")]
		public ZDateTime JPB_Calc_EFDT
		{
			get
			{
				var inBondDetails = InBondDetails;
				return inBondDetails == null ? ZDateTime.Empty : inBondDetails.JPI_EFDT;
			}
			set
			{
				var inBondDetails = InBondDetails;
				var oldValue = inBondDetails == null ? ZDateTime.Empty : inBondDetails.JPI_EFDT;
				if (!value.IsEmpty && inBondDetails == null)
				{
					inBondDetails = CreateNewInBondDetails();
				}
				if (inBondDetails != null)
				{
					inBondDetails.JPI_EFDT = value;
				}
				PopulateTemporaryLandingDurationIfPossible(this.JPB_Calc_ESDT, value);
				JPB_Calc_EFDTInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJPB_Calc_EFDT();
				}
			}
		}

		public ZPropertyInfo JPB_Calc_EFDTInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_Calc_EFDT); }
		}
		#endregion

		#region JPB_Calc_GoodsValue
		[ResourceStringData("JPAFRBills|JPB_Calc_GoodsValue", Caption = "Goods Value", ShortCaption = "Value")]
		public ZDecimal JPB_Calc_GoodsValue
		{
			get
			{
				var inBondDetails = InBondDetails;
				return inBondDetails == null ? ZDecimal.Zero : inBondDetails.JPI_GoodsValue;
			}
			set
			{
				var inBondDetails = InBondDetails;
				var oldValue = inBondDetails == null ? ZDecimal.Zero : inBondDetails.JPI_GoodsValue;
				if (!value.IsEmpty && inBondDetails == null)
				{
					inBondDetails = CreateNewInBondDetails();
				}
				if (inBondDetails != null)
				{
					inBondDetails.JPI_GoodsValue = value;
				}
				JPB_Calc_GoodsValueInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJPB_Calc_GoodsValue();
				}
			}
		}

		public ZPropertyInfo JPB_Calc_GoodsValueInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_Calc_GoodsValue); }
		}
		#endregion

		#region JPB_Calc_RX_NKGoodsValueCurrency
		[List(nameof(Lookups) + "." + nameof(JPAFRBillsLookups.GoodsValueCurrencies))]
		[ResourceStringData("JPAFRBills|JPB_Calc_RX_NKGoodsValueCurrency", Caption = "Goods Value Currency", ShortCaption = "Value  Curr.")]
		[MaxLength(JPAFRInBondDetails.Schema.JPI_RX_NKGoodsValueCurrencyMaxLength)]
		public ZString JPB_Calc_RX_NKGoodsValueCurrency
		{
			get
			{
				var inBondDetails = InBondDetails;
				return inBondDetails == null ? ZString.Empty : inBondDetails.JPI_RX_NKGoodsValueCurrency;
			}
			set
			{
				value = value.TrimEnd(' ', '\t');
				CheckMaximumLength(JPB_Calc_RX_NKGoodsValueCurrencyInfo, value);
				var inBondDetails = InBondDetails;
				var oldValue = inBondDetails == null ? ZString.Empty : inBondDetails.JPI_RX_NKGoodsValueCurrency;
				if (!value.IsEmpty && inBondDetails == null)
				{
					inBondDetails = CreateNewInBondDetails();
				}
				if (inBondDetails != null)
				{
					inBondDetails.JPI_RX_NKGoodsValueCurrency = value;
				}
				JPB_Calc_RX_NKGoodsValueCurrencyInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJPB_Calc_RX_NKGoodsValueCurrency();
				}
			}
		}

		public ZPropertyInfo JPB_Calc_RX_NKGoodsValueCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_Calc_RX_NKGoodsValueCurrency); }
		}
		#endregion

		#region JPB_Calc_TemporaryLandingDuration
		[ResourceStringData("JPAFRBills|JPB_Calc_TemporaryLandingDuration", Caption = "Transhipment Duration Days", ShortCaption = "Trans. Duration Days", FullDescription = "The number of days from the start date of discharging.")]
		public ZInt JPB_Calc_TemporaryLandingDuration
		{
			get
			{
				var inBondDetails = InBondDetails;
				return inBondDetails == null ? ZInt.Zero : inBondDetails.JPI_TemporaryLandingDuration;
			}
			set
			{
				var inBondDetails = InBondDetails;
				var oldValue = inBondDetails == null ? ZInt.Zero : inBondDetails.JPI_TemporaryLandingDuration;
				if (!value.IsEmpty && inBondDetails == null)
				{
					inBondDetails = CreateNewInBondDetails();
				}
				if (inBondDetails != null)
				{
					inBondDetails.JPI_TemporaryLandingDuration = value;
				}
				JPB_Calc_TemporaryLandingDurationInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJPB_Calc_TemporaryLandingDuration();
				}
			}
		}

		public ZPropertyInfo JPB_Calc_TemporaryLandingDurationInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_Calc_TemporaryLandingDuration); }
		}
		#endregion

		#region JPB_Calc_TemporaryLandingReason
		[List(nameof(Lookups) + "." + nameof(JPAFRBillsLookups.TemporaryLandingReasonCodeList))]
		[ResourceStringData("JPAFRBills|JPB_Calc_TemporaryLandingReason", Caption = "Transhipment Reason", ShortCaption = "Trans. Reason")]
		[MaxLength(JPAFRInBondDetails.Schema.JPI_TemporaryLandingReasonMaxLength)]
		public ZString JPB_Calc_TemporaryLandingReason
		{
			get
			{
				var inBondDetails = InBondDetails;
				return inBondDetails == null ? ZString.Empty : inBondDetails.JPI_TemporaryLandingReason;
			}
			set
			{
				value = value.TrimEnd(' ', '\t');
				CheckMaximumLength(JPB_Calc_TemporaryLandingReasonInfo, value);
				var inBondDetails = InBondDetails;
				var oldValue = inBondDetails == null ? ZString.Empty : inBondDetails.JPI_TemporaryLandingReason;
				if (!value.IsEmpty && inBondDetails == null)
				{
					inBondDetails = CreateNewInBondDetails();
				}
				if (inBondDetails != null)
				{
					inBondDetails.JPI_TemporaryLandingReason = value;
				}
				JPB_Calc_TemporaryLandingReasonInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJPB_Calc_TemporaryLandingReason();
				}
			}
		}

		public ZPropertyInfo JPB_Calc_TemporaryLandingReasonInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_Calc_TemporaryLandingReason); }
		}
		#endregion

		#region JPB_Calc_TransportMode
		[List(nameof(Lookups) + "." + nameof(JPAFRBillsLookups.TransportModeList))]
		[ResourceStringData("JPAFRBills|JPB_Calc_TransportMode", Caption = "Transhipment Transport Mode", ShortCaption = "Trans. Mode")]
		[MaxLength(JPAFRInBondDetails.Schema.JPI_TransportModeMaxLength)]
		public ZString JPB_Calc_TransportMode
		{
			get
			{
				var inBondDetails = InBondDetails;
				return inBondDetails == null ? ZString.Empty : inBondDetails.JPI_TransportMode;
			}
			set
			{
				value = value.TrimEnd(' ', '\t');
				CheckMaximumLength(JPB_Calc_TransportModeInfo, value);
				var inBondDetails = InBondDetails;
				var oldValue = inBondDetails == null ? ZString.Empty : inBondDetails.JPI_TransportMode;
				if (!value.IsEmpty && inBondDetails == null)
				{
					inBondDetails = CreateNewInBondDetails();
				}
				if (inBondDetails != null)
				{
					inBondDetails.JPI_TransportMode = value;
				}
				JPB_Calc_TransportModeInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJPB_Calc_TransportMode();
				}
			}
		}

		public ZPropertyInfo JPB_Calc_TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_Calc_TransportMode); }
		}
		#endregion

		#region JPB_Calc_GeneralCustomsTransitApprovalNumber

		[ResourceStringData("JPAFRBills|JPB_Calc_GeneralCustomsTransitApprovalNumber", Caption = "General Customs Transit Approval Number", ShortCaption = "Transit Approval Number")]
		[MaxLength(JPAFRInBondDetails.Schema.JPI_GeneralCustomsTransitApprovalNumberMaxLength)]
		public ZString JPB_Calc_GeneralCustomsTransitApprovalNumber
		{
			get
			{
				var inBondDetails = InBondDetails;
				return inBondDetails == null ? ZString.Empty : inBondDetails.JPI_GeneralCustomsTransitApprovalNumber;
			}
			set
			{
				value = value.TrimEnd(' ', '\t');
				CheckMaximumLength(JPB_Calc_GeneralCustomsTransitApprovalNumberInfo, value);
				var inBondDetails = InBondDetails;
				var oldValue = inBondDetails == null ? ZString.Empty : inBondDetails.JPI_GeneralCustomsTransitApprovalNumber;
				if (!value.IsEmpty && inBondDetails == null)
				{
					inBondDetails = CreateNewInBondDetails();
				}
				if (inBondDetails != null)
				{
					inBondDetails.JPI_GeneralCustomsTransitApprovalNumber = value;
				}
				JPB_Calc_GeneralCustomsTransitApprovalNumberInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateJPB_Calc_GeneralCustomsTransitApprovalNumber();
				}
			}
		}

		public ZPropertyInfo JPB_Calc_GeneralCustomsTransitApprovalNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_Calc_GeneralCustomsTransitApprovalNumber); }
		}
		#endregion

		void PopulateTemporaryLandingDurationIfPossible(ZDateTime eSDT, ZDateTime eFDT)
		{
			if (eSDT.IsValid && eFDT.IsValid && eFDT >= eSDT)
			{
				this.JPB_Calc_TemporaryLandingDuration = (ZInt)((eFDT - eSDT).TotalDays + 1);
			}
		}

		#endregion

		public bool IsBillAlreadyRegistered
		{
			get { return AFRBillCustomsStatusList.IsRegisteredType(JPB_ReleaseStatus); }
		}

		public bool IsRiskAssessmentReceivedForBill
		{
			get { return AFRBillCustomsStatusList.IsRiskAssessmentReceivedType(JPB_ReleaseStatus); }
		}

		public bool IsMessagingInProgress
		{
			get { return MessageStatusList.IsMessagingInProgressType(JPB_MessageStatus); }
		}

		public bool ShouldSynchronise
		{
			get
			{
				var header = Header;
				return header != null && header.ShouldSynchroniseWithConsol;
			}
		}

		#region Override Properties
		[RelatedBusinessObject("Header")]
		public override ZGuid JPB_JPH_Header
		{
			get { return base.JPB_JPH_Header; }
			set { base.JPB_JPH_Header = value; }
		}

		public JPAFRHeader Header
		{
			get { return Factory.Load<JPAFRHeader>(JPB_JPH_Header); }
		}

		[DecimalPlaces(2)]
		public override ZDecimal JPB_FreightValue
		{
			get { return base.JPB_FreightValue; }
			set { base.JPB_FreightValue = value; }
		}

		[ReadOnly(true)]
		public override ZString JPB_ReleaseStatus
		{
			get { return base.JPB_ReleaseStatus; }
			set { base.JPB_ReleaseStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.JPAFRBills|JPB_ReleaseStatusDescription", Caption = "Release Status Description", ShortCaption = "Rel. Status Desc.")]
		public ZString JPB_ReleaseStatusDescription
		{
			get { return Lookups.BillCustomsStatusList.GetDescriptionFromCode(JPB_ReleaseStatus); }
		}

		public ZPropertyInfo JPB_ReleaseStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_ReleaseStatusDescription); }
		}

		[ReadOnly(true)]
		public override ZString JPB_MessageStatus
		{
			get { return base.JPB_MessageStatus; }
			set { base.JPB_MessageStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.JPAFRBills|JPB_MessageStatusDescription", Caption = "Message Status Description", ShortCaption = "Msg. Status Desc.")]
		public ZString JPB_MessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(JPB_MessageStatus); }
		}

		public ZPropertyInfo JPB_MessageStatusDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_MessageStatusDescription); }
		}

		[List(nameof(Lookups) + "." + nameof(JPAFRBillsLookups.ManifestUnitList))]
		public override ZString JPB_ManifestUQ
		{
			get { return base.JPB_ManifestUQ; }
			set { base.JPB_ManifestUQ = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JPAFRBillsLookups.WeightUnitList))]
		[MeasureUnit(Schema.JPB_GrossWeight, MeasureUnitType.Weight)]
		public override ZString JPB_GrossWeightUQ
		{
			get { return base.JPB_GrossWeightUQ; }
			set { base.JPB_GrossWeightUQ = value; }
		}

		[MeasureUnit(Schema.JPB_GrossWeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal JPB_GrossWeight
		{
			get { return base.JPB_GrossWeight; }
			set { base.JPB_GrossWeight = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JPAFRBillsLookups.VolumeUnitList))]
		[MeasureUnit(Schema.JPB_Volume, MeasureUnitType.Volume)]
		public override ZString JPB_VolumeUQ
		{
			get { return base.JPB_VolumeUQ; }
			set { base.JPB_VolumeUQ = value; }
		}

		[MeasureUnit(Schema.JPB_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal JPB_Volume
		{
			get { return base.JPB_Volume; }
			set { base.JPB_Volume = value; }
		}

		[RelatedBusinessObject("SpecialCargoCode")]
		[List(nameof(Lookups) + "." + nameof(JPAFRBillsLookups.SpecialCargoCodes))]
		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.JPAFRBills|JPB_SpecialCargoCode", Caption = "Special Cargo Code", ShortCaption = "SPC. Cargo Code")]
		public override ZString JPB_SpecialCargoCode
		{
			get { return base.JPB_SpecialCargoCode; }
			set { base.JPB_SpecialCargoCode = value; }
		}

		public override UNDGSubstance Substance
		{
			get
			{
				return UNDGSubstancePivotCollection.DefaultSubstance;
			}
		}

		public ZZRefCusCodeListCombined SpecialCargoCode =>
			ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, JPB_SpecialCargoCode, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanSpecialCargoCode, ZDateTime.Today);

		#endregion

		#region New Properties

		#region JPB_NoOfAFRContainers

		public ZInt JPB_NoOfAFRContainers
		{
			get { return this.Containers.Count; }
		}

		public ZPropertyInfo JPB_NoOfAFRContainersInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_NoOfAFRContainers); }
		}

		#endregion

		#region JPB_NoOfSailingContainers

		public ZInt JPB_NoOfSailingContainers
		{
			get
			{
				if (!fJPB_NoOfSailingContainers.HasValue)
				{
					var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)this).Source;
					fJPB_NoOfSailingContainers = sailingBill != null ? sailingBill.RealContainers.Count : 0;
				}
				return fJPB_NoOfSailingContainers.Value;
			}
		}
		ZInt? fJPB_NoOfSailingContainers;

		public ZPropertyInfo JPB_NoOfSailingContainersInfo
		{
			get { return GetZPropertyInfo(Schema.JPB_NoOfSailingContainers); }
		}

		#endregion

		#endregion

		#region Related Objects

		public JobDocAddress Consignor
		{
			get
			{
				if (consignor == null || consignor.IsDeleted)
				{
					consignor = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.ConsignorDocumentaryAddress));
				}

				return consignor;
			}
		}
		JobDocAddress consignor;

		public JobDocAddress Consignee
		{
			get
			{
				if (consignee == null || consignee.IsDeleted)
				{
					consignee = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.ConsigneeAddress));
				}

				return consignee;
			}
		}
		JobDocAddress consignee;

		public JobDocAddress NotifyParty1
		{
			get
			{
				if (notifyParty1 == null || notifyParty1.IsDeleted)
				{
					notifyParty1 = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.NotifyParty));
				}

				return notifyParty1;
			}
		}
		JobDocAddress notifyParty1;

		public JobDocAddress NotifyParty2
		{
			get
			{
				if (notifyParty2 == null || notifyParty2.IsDeleted)
				{
					notifyParty2 = DocAddresses.FindOrCreateWithRequirement(GetDocAddressRequirement(DocAddressType.NotifyParty2));
				}

				return notifyParty2;
			}
		}
		JobDocAddress notifyParty2;

		[ChildEditable]
		public JPAFRContainerCollection Containers
		{
			get
			{
				if (containers == null)
				{
					containers = new JPAFRContainerCollection(this);
					RegisterEditableChildObject(containers);
				}
				return containers;
			}
		}
		JPAFRContainerCollection containers;

		public JPAFRInBondDetails InBondDetails
		{
			get
			{
				if (inBondDetails == null || inBondDetails.IsDeleted)
				{
					if (inBondDetails != null)
					{
						UnRegisterEditableChildObject(inBondDetails);
					}
					var query = new ZQuery(JPAFRInBondDetailsSchema.JPI_JPB_Bill, PK);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					inBondDetails = Factory.LoadTop1<JPAFRInBondDetails>(query);
					RegisterEditableChildObject(inBondDetails);
				}
				return inBondDetails;
			}
		}
		JPAFRInBondDetails inBondDetails;

		[ChildEditable]
		public CusCodeDataWithSequenceNumberLineCollection<OtherRelevantLaw> OtherRelevantLaws
		{
			get
			{
				if (otherRelevantLaws == null)
				{
					otherRelevantLaws = new CusCodeDataWithSequenceNumberLineCollection<OtherRelevantLaw>(this, OtherRelevantLaw.ORLType);
					RegisterEditableChildObject(otherRelevantLaws);
				}
				return otherRelevantLaws;
			}
		}
		CusCodeDataWithSequenceNumberLineCollection<OtherRelevantLaw> otherRelevantLaws;

		[ChildEditable]
		public NotificationForwardingPartyCollection NotificationForwardingParties
		{
			get
			{
				if (notificationForwardingParties == null)
				{
					notificationForwardingParties = new NotificationForwardingPartyCollection(this);
					RegisterEditableChildObject(notificationForwardingParties);
				}
				return notificationForwardingParties;
			}
		}
		NotificationForwardingPartyCollection notificationForwardingParties;

		public BLLFunctionInfo BLLFunctionInfo
		{
			get
			{
				if (bllFunctionInfo == null)
				{
					var query = new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction);
					query.AddToFilter(CusAddInfoSchema.B7_ParentID, PK);
					bllFunctionInfo = Factory.LoadTop1<BLLFunctionInfo>(query);
				}
				return bllFunctionInfo;
			}
		}
		BLLFunctionInfo bllFunctionInfo;

		#endregion

		#region Override Methods

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				Factory.Load<JPAFRInBondDetails>(new ZQuery(JPAFRInBondDetailsSchema.JPI_JPB_Bill, PK)).DeleteAll();
				OtherRelevantLaws.DeleteAll();
				NotificationForwardingParties.DeleteAll();
				Containers.DeleteAll();
				DocAddresses.RemoveAndDeleteAll();
				UNDGSubstancePivotCollection.DeleteAll();
				UNDGs.DeleteAll();
			}
			base.Delete();
			InBondDetailInitiator = null;
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (IsInDatabase && JPB_MessageStatusInfo.HasChanges)
			{
				Logs.AddNew(Events.MessageStatusChange, ZString.Format("{0} - {1}", JPB_MessageStatus, new MessageStatusList().GetDescriptionFromCode(JPB_MessageStatus)));
			}
			if (IsInDatabase && JPB_ReleaseStatusInfo.HasChanges)
			{
				Logs.AddNew(Events.StatusChange, ZString.Format("Status Change:{0} -> {1}", JPB_ReleaseStatusInfo.OriginalValue, JPB_ReleaseStatus));
			}
			if (!IsInDatabase || IsDeleting || JPB_DGInfo.HasChanges)
			{
				Logs.CreateOrRecreateEventLog(Events.DangerousGoodsChanged, EstimateActual.Actual, ZDateTimeOffset.Now);
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

		#region Implementation

		internal ShortSequenceNumberGenerator OtherRelevantLawSequenceNumberGenerator
		{
			get { return otherRelevantLawSequenceNumberGenerator ?? (otherRelevantLawSequenceNumberGenerator = new ShortSequenceNumberGenerator(() => new TypedEnumerable<IShortSequenceNumberLine>(OtherRelevantLaws))); }
		}
		ShortSequenceNumberGenerator otherRelevantLawSequenceNumberGenerator;

		JPAFRInBondDetails CreateNewInBondDetails()
		{
			if (inBondDetails != null)
			{
				UnRegisterEditableChildObject(inBondDetails);
				inBondDetails = null;
			}
			InBondDetailInitiator.OnDisposing += InBondDetailInitiator_OnDisposing;
			Factory.Saved += Factory_Saved;
			if (!InBondMutex.IsLocked && InBondMutex.Lock())
			{
				inBondDetails = Factory.New<JPAFRInBondDetails>();
				inBondDetails.JPI_JPB_Bill = PK;
				RegisterEditableChildObject(InBondDetails);
			}
			else
			{
				InBondDetailInitiator.NotifyUserOfAnInvalidOperation(InBondMutexLockText);
			}
			return inBondDetails;
		}

		void InBondDetailInitiator_OnDisposing(object sender, EventArgs e)
		{
			if (InBondMutex.HasLock)
			{
				InBondMutex.Unlock();
			}
			Factory.Saved -= Factory_Saved;
			if (inBondDetailInitiator != null)
			{
				inBondDetailInitiator.OnDisposing -= InBondDetailInitiator_OnDisposing;
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				InBondDetailInitiator_OnDisposing(this, EventArgs.Empty);
				factory.Saved -= Factory_Saved;
			}
		}

		public static string InBondMutexLockText
		{
			get { return Res.GetString("JPAFRBills|InBondMutexLockText", "Someone else is already in the process of creating InBond Details for this AFR.\r\nYou should be able to access the InBond Details when the person has saved the record. Please try later."); }
		}

		ZGlobalMutex InBondMutex
		{
			get { return inBondMutex ?? (inBondMutex = new ZGlobalMutex(MutexIDs.AFRJobBeingCreated, PK.ToString())); }
		}
		ZGlobalMutex inBondMutex;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JPB_GrossWeightUQ = WeightUnitCodeList.Codes.Kilogram;
			JPB_VolumeUQ = VolumeUnitCodeList.Codes.CubicMeter;
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = "Bill Of Lading";

				if (!JPB_BillNumber.IsEmpty)
				{
					result += " " + JPB_BillNumber;
				}

				return result;
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(JPAFRBills bill)
				: base(bill)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();
				Factory.AddFetchHint(JPAFRContainerSchema.JPC_JPB_Bill, BusinessObject.PK);
				Factory.AddFetchHint(JPAFRInBondDetailsSchema.JPI_JPB_Bill, BusinessObject.PK);
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(
					tableSchema: JobDocAddressSchema.Instance,
					mainQuery: new ZQuery(JobDocAddressSchema.E2_ParentTableCode, BusinessObject.TablePrefix),
					secondQuery: new ZQuery(JobDocAddressSchema.E2_ParentID, BusinessObject.PK));
				Factory.AddFetchHint(
					tableSchema: UNDGDataItemSchema.Instance,
					mainQuery: new ZQuery(UNDGDataItemSchema.DI_ParentTableCode, BusinessObject.TablePrefix),
					secondQuery: new ZQuery(UNDGDataItemSchema.DI_ParentID, BusinessObject.PK));
			}
		}

		#endregion

		#region INotificationForwardingPartySequenceNumberHeader Members

		ShortSequenceNumberGenerator INotificationForwardingPartySequenceNumberHeader.SequenceNumberGenerator
		{
			get { return notificationFonrwardingPartySequenceNumberGenerator ?? (notificationFonrwardingPartySequenceNumberGenerator = new ShortSequenceNumberGenerator(() => new TypedEnumerable<IShortSequenceNumberLine>(NotificationForwardingParties))); }
		}
		ShortSequenceNumberGenerator notificationFonrwardingPartySequenceNumberGenerator;

		#endregion

		#region InBond Detail Initiator

		public IInBondDetailInitiator InBondDetailInitiator
		{
			get
			{
				if (inBondDetailInitiator == null)
				{
					if (Header != null && Header.InBondDetailInitiator != null)
					{
						inBondDetailInitiator = Header.InBondDetailInitiator;
					}
					else
					{
						throw new ApplicationException("You can't perform this action that results in InBond data being created because you have not hooked up a IInBondDetailInitiator to the Bill");
					}
				}
				return inBondDetailInitiator;
			}
			set { inBondDetailInitiator = value; }
		}
		IInBondDetailInitiator inBondDetailInitiator;

		public bool HasInBondDetailInitiator
		{
			get { return inBondDetailInitiator != null; }
		}

		#endregion

		#region IDocAddresses Members

		[ChildEditable]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					fDocAddresses.Sort(JobDocAddress.Schema.E2_AddressSequence);
					RegisterEditableChildObject(fDocAddresses);
				}
				return fDocAddresses;
			}
		}
		internal JobDocAddressDependentCollection fDocAddresses;

		JobDocAddressDependentCollection IDocAddresses.DocAddresses
		{
			get { return DocAddresses; }
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return GetDocAddressRequirement(addressType);
		}

		void AddressMandatoryCheck(JobDocAddressValidation addressValidation)
		{
			var parent = addressValidation.Parent;
			parent.OrganisationPKInfo.AddMessageError(ValidationConstants.Bill.AddressIsMandatory);
		}

		JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType)
		{
			JobDocAddressRequirement result = null;
			switch (addressType)
			{
				case DocAddressType.ConsignorDocumentaryAddress:
				case DocAddressType.ConsigneeAddress:
				case DocAddressType.NotifyParty:
				case DocAddressType.NotifyParty2:
					return GetJobDocAddressRequirement(addressType);
			}

			return result;
		}

		JobDocAddressRequirement GetJobDocAddressRequirement(DocAddressType addressType)
		{
			JobDocAddressRequirement result;
			if (!PartiesJobDocAddressRequirements.TryGetValue(addressType, out result))
			{
				result = new JobDocAddressRequirement(addressType);
				result.DefaultMax = 1;
				switch (addressType)
				{
					case DocAddressType.ConsignorDocumentaryAddress:
					case DocAddressType.ConsigneeAddress:
					case DocAddressType.NotifyParty:
						result.IsMandatory = true;
						result.ValidateOrganisationPKUponMandatoryRequirement = AddressMandatoryCheck;
						break;
				}
				PartiesJobDocAddressRequirements.Add(addressType, result);
			}
			return result;
		}

		Dictionary<DocAddressType, JobDocAddressRequirement> PartiesJobDocAddressRequirements
		{
			get { return fPartiesJobDocAddressRequirements ?? (fPartiesJobDocAddressRequirements = new Dictionary<DocAddressType, JobDocAddressRequirement>()); }
		}
		Dictionary<DocAddressType, JobDocAddressRequirement> fPartiesJobDocAddressRequirements;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					DocAddressType.ConsignorDocumentaryAddress,
					DocAddressType.ConsigneeAddress,
					DocAddressType.NotifyParty,
					DocAddressType.NotifyParty2
				};
			}
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new JPAFRBillsJobDocAddressValidation(addressToValidate);
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return !(docAddress == consignor ||
				docAddress == consignee ||
				docAddress == notifyParty1 ||
				docAddress == notifyParty2);
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region ICanDelete Members
		bool ICanDelete.CanDelete
		{
			get { return !ShouldSynchronise && !IsBillAlreadyRegistered && !IsMessagingInProgress; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get
			{
				if (IsMessagingInProgress)
				{
					return ResString.GetMultilingualString("JPAFRBill|C359D065-B85E-4849-8FE9-2ED3F3614CEC", "This Bill Of Lading is awaiting message response from Customs.\r\nPlease wait till the message response arrives and try again.");
				}
				else if (IsBillAlreadyRegistered)
				{
					return ResString.GetMultilingualString("JPAFRBill|865123AB-3D8D-48E7-B48C-92559B2C6AAB", "This Bill Of Lading is already registered with Customs.\r\nYou need to delete it from Customs file by sending Delete AFR amendment before deleting it here. (AFR > Amendment Manifest > Send with action ‘Delete’)");
				}
				else
				{
					return ResString.GetMultilingualString("JPAFRBill|286F17FA-AF3A-4081-8B6D-74CF40FFCD7A", "Bill Of Lading values are copied from the Consol. If you want to delete this record, please do it in the Consol, or you may tick 'Override Freight Defaults'.");
				}
			}
		}

		#endregion

		#region VOCC Section

		public bool IsShippingLineEntry
		{
			get
			{
				var header = this.Header;
				return header != null && header.JPH_IsShippingLineEntry;
			}
		}

		public ZString DischargePortCode
		{
			get
			{
				var header = this.Header;
				return header != null ? header.JPH_RL_NKDischarge : ZString.Empty;
			}
		}

		#endregion

		#region ISailingSynchronisationTarget<BillOfLading> Members

		bool ISailingSynchronisationTarget<BillOfLading>.IsMatched(BillOfLading sailingTarget)
		{
			return IsMatched(sailingTarget);
		}

		bool IsMatched(BillOfLading sailingTarget)
		{
			return sailingTarget != null && GetBillNumber(sailingTarget) == this.JPB_BillNumber;
		}

		void ISailingSynchronisationTarget<BillOfLading>.Set(BillOfLading sailingTarget)
		{
			using (this.GetValidationSuspender())
			{
				this.JPB_BillNumber = GetBillNumber(sailingTarget);
			}
		}

		BillOfLading ISailingSynchronisationTarget<BillOfLading>.Source
		{
			get { return SailingSynchronisationSource; }
		}

		BillOfLading fSailingSynchronisationSource;

		BillOfLading SailingSynchronisationSource
		{
			get
			{
				if (fSailingSynchronisationSource != null && IsMatched(fSailingSynchronisationSource) && !fSailingSynchronisationSource.IsCancelled)
				{
					return fSailingSynchronisationSource;
				}
				fSailingSynchronisationSource = Header.BillsOfLading.FirstOrDefault(x => IsMatched(x) && !x.IsCancelled);
				return fSailingSynchronisationSource;
			}
		}

		void ISailingSynchronisationTarget<BillOfLading>.Synchronise()
		{
			var sailingBill = SailingSynchronisationSource;
			if (sailingBill != null && sailingBill.IsContainerised)
			{
				using (this.GetValidationSuspender())
				{
					PopulatePortInformation(sailingBill);
					PopulateJobDocAddresses(sailingBill);
					PopulateQuantities(sailingBill);
					PopulateGoodsInfo(sailingBill);
					this.Containers.Synchronise(sailingBill.RealContainers.Cast<BillOfLadingContainer>());
				}
			}
		}

		void PopulatePortInformation(BillOfLading sailingBill)
		{
			this.JPB_RL_NKOrigin = sailingBill.JS_RL_NKOrigin;
			this.JPB_RL_NKDelivery = sailingBill.JS_RL_NKDestination;
			this.JPB_RL_NKFinalDestination = sailingBill.JS_RL_NKDestination;
		}

		void PopulateJobDocAddresses(BillOfLading sailingBill)
		{
			SailingBillImportHelper.SynchroniseJobDocAddressIfNotEmpty(this.Consignor, sailingBill.ConsignorDocumentaryAddress);
			var consigneeDocumentaryAddress = sailingBill.ConsigneeDocumentaryAddress;
			SailingBillImportHelper.SynchroniseJobDocAddressIfNotEmpty(this.Consignee, consigneeDocumentaryAddress);
			var notifyPartyDocumentaryAddress = sailingBill.NotifyPartyDocumentaryAddress;
			if (notifyPartyDocumentaryAddress != null && !notifyPartyDocumentaryAddress.IsEmpty)
			{
				SailingBillImportHelper.SynchroniseJobDocAddressIfNotEmpty(this.NotifyParty1, sailingBill.NotifyPartyDocumentaryAddress);
			}
			else
			{
				SailingBillImportHelper.SynchroniseJobDocAddressIfNotEmpty(this.NotifyParty1, consigneeDocumentaryAddress);
			}
			SailingBillImportHelper.SynchroniseJobDocAddressIfNotEmpty(this.NotifyParty2, sailingBill.NotifyParty2DocumentaryAddress);
		}

		void PopulateQuantities(BillOfLading sailingBill)
		{
			var sailingPackLines = sailingBill.RealContainers.Cast<BillOfLadingContainer>().SelectMany(x => x.PackLines).Cast<BillOfLadingPackLine>().ToArray();
			this.JPB_ManifestUQ = sailingPackLines.Select(x => x.JL_F3_NKPackType).FirstOrDefault(x => !x.IsEmpty);
			this.JPB_ManifestQty = sailingPackLines.Sum(x => x.JL_PackageCount);
			var sourceVolumeUQ = sailingPackLines.Select(x => x.JL_ActualVolumeUQ).FirstOrDefault(x => !x.IsEmpty);
			if (!sourceVolumeUQ.IsEmpty)
			{
				this.JPB_Volume = JPAFRBillsynchroniser.ConvertVolumeIfNecessary(sailingPackLines.Sum(x => new ZVolume(x.JL_ActualVolume, x.JL_ActualVolumeUQ).ConvertTo(sourceVolumeUQ)), sourceVolumeUQ);
			}
			this.JPB_VolumeUQ = JPAFRBillsynchroniser.TranslateVolumeUQ(sourceVolumeUQ);
			var sourceWeightUQ = sailingPackLines.Select(x => x.JL_ActualWeightUQ).FirstOrDefault(x => !x.IsEmpty);
			if (!sourceWeightUQ.IsEmpty)
			{
				this.JPB_GrossWeight = JPAFRBillsynchroniser.ConvertWeightIfNecessary(sailingPackLines.Sum(x => new ZWeight(x.JL_ActualWeight, x.JL_ActualWeightUQ).ConvertTo(sourceWeightUQ)), sourceWeightUQ);
			}
			this.JPB_GrossWeightUQ = JPAFRBillsynchroniser.TranslateWeightUQ(sourceWeightUQ);
		}

		void PopulateGoodsInfo(BillOfLading sailingBill)
		{
			this.JPB_MarksAndNumbers = sailingBill.JS_MarksAndNumbers.KeepAlphanumericCharacters().Left(AutoJPAFRBills.Schema.JPB_MarksAndNumbersMaxLength);
			this.JPB_GoodsDescription = sailingBill.JS_GoodsDescription;
			var packlinesInSortOrder = sailingBill.OuterPackLines.OfType<Freight.Business.PackLine>().OrderByDescending(x => JPAFRBillsynchroniser.GetPacklineOrderKey(x)).ToArray();
			var mostRelevantPackline = packlinesInSortOrder.FirstOrDefault(x => !x.JL_HarmonisedCode.IsEmpty) ?? packlinesInSortOrder.FirstOrDefault();
			this.JPB_Tariff = mostRelevantPackline == null ? ZString.Empty : mostRelevantPackline.JL_HarmonisedCode.Replace(".", "").Left(6);
			this.JPB_RN_NKGoodsOrigin = mostRelevantPackline == null ? ZString.Empty : mostRelevantPackline.JL_RN_NKOrigin;
			this.JPB_DG = JPAFRBillsynchroniser.GetUNDGSubstanceFromPacklineList(packlinesInSortOrder);
		}

		string GetBillNumber(BillOfLading bill)
		{
			var header = this.Header;
			var registryCompanyPK = header == null ? Guid.Empty : header.RegistryCompanyPK;
			var carrierCode = header == null ? ZString.Empty : header.JPH_CarrierCode;
			return bill.JS_HouseBill.Trim().GetTargetOceanBillNumber(registryCompanyPK, carrierCode);
		}

		#endregion

		#region IWorkflowTriggerEventSource Members

		public IGlbCompany JobHeaderCompany
		{
			get { return Header?.Company; }
		}

		public IReadOnlyList<IWorkflowProviderCore> ParentWorkflowProviders
		{
			get
			{
				var list = new List<IWorkflowProviderCore>();
				var parent = this.Header;
				if (parent != null)
				{
					list.Add(parent);
				}
				return list;
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(OtherRelevantLaw.ORLType, typeof(OtherRelevantLaw));
			result.Add(NotificationForwardingParty.NFPType, typeof(NotificationForwardingParty));
			return result;
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction, typeof(BLLFunctionInfo));
			return result;
		}

		#endregion

		internal void RefreshSailingStatistics()
		{
			fJPB_NoOfSailingContainers = null;
			JPB_NoOfSailingContainersInfo.RefreshBinding();
		}

		[RelatedBusinessObject("UNDGSubstance")]
		[List(nameof(Lookups) + "." + nameof(JPAFRBillsLookups.UNDGSubstances))]
		public override ZGuid JPB_DG
		{
			get => base.JPB_DG;
			set
			{
				if (base.JPB_DG != value)
				{
					var substance = Factory.Load<UNDGSubstance>(value);
					UNDGSubstancePivotCollection.UpdateDefaultPivot(substance);

					base.JPB_DG = value;

					foreach (var extraUNDG in UNDGs)
					{
						extraUNDG.MarkAsNeedingValidation();
					}
				}
			}
		}

		public override ZString JPB_DG_NKSubstance
		{
			get => base.JPB_DG_NKSubstance;
			set
			{
				if (base.JPB_DG_NKSubstance != value)
				{
					var substance = UNDGSubstanceLoader.LoadSubstances(Factory, value.SubstringSafe(0, 4), value.SubstringSafe(4, 2), UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO).FirstOrDefault();
					UNDGSubstancePivotCollection.UpdateDefaultPivot(substance);

					base.JPB_DG_NKSubstance = value;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.JP.AFR.Business.JPAFRBills|JPB_IMOClass", Caption = "IMO Class")]
		public ZString JPB_IMOClass => UNDGSubstance?.DG_Class ?? ZString.Empty;

		[ChildEditable(true)]
		public UNDGSubstancePivotCollection UNDGSubstancePivotCollection
		{
			get
			{
				if (undgSubstancePivotCollection == null)
				{
					undgSubstancePivotCollection = new UNDGSubstancePivotCollection(Factory, this, JPAFRBillsSchema.Constants.Prefix);
					RegisterEditableChildObject(undgSubstancePivotCollection);
				}

				return undgSubstancePivotCollection;
			}
		}

		UNDGSubstancePivotCollection undgSubstancePivotCollection;

		public UNDGSubstance UNDGSubstance
		{
			get { return (UNDGSubstance)Factory.Load(typeof(UNDGSubstance), JPB_DG); }
		}

		#region IUNDGDataItemProvider

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (undgs == null)
				{
					undgs = new UNDGDataItemCollection(this);
					undgs.EnableMaxCountValidation(4, string.Empty, false);

					RegisterEditableChildObject(undgs);
				}

				return undgs;
			}
		}
		UNDGDataItemCollection undgs;

		MasterFiles.Business.UNDGDataItemCollection IUNDGDataItemProvider.UNDGs => UNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => false;

		#endregion
	}
}
