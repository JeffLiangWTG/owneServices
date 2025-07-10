using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	[SystemDefinedValues]
	[DependentBusinessObject(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.Bills))]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class TemporaryStorageBill : ManifestBase.AsycudaBill, Integration.Customs.ICusSupportingInfoTypeSupporter, ICusReferenceTypeSupporter, ITemporaryStorageBill
	{
		public new static readonly TemporaryStorageBillTypeDecider TypeDecider = new TemporaryStorageBillTypeDecider();

		public TemporaryStorageBill(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : ManifestBase.AsycudaBill.Schema
		{
			public const string TypeOfBillDocument = nameof(TemporaryStorageBill.TypeOfBillDocument);
			public const int TypeOfBillDocumentMaxLength = 4;
			public const string ConsignorOrgPK = nameof(TemporaryStorageBill.ConsignorOrgPK);
			public const string ConsigneeOrgPK = nameof(TemporaryStorageBill.ConsigneeOrgPK);
			public const string NotifyPartyOrgPK = nameof(TemporaryStorageBill.NotifyPartyOrgPK);
			public const int TypeOfPersonMaxLength = 1;
			public const int CityMaximumLength = 35;
			public const int NameMaximumLength = 70;
			public const string HasNoMasterBill = nameof(TemporaryStorageBill.HasNoMasterBill);
		}

		bool IsPropertyReadOnlyUnderAmendableFieldsEditAllowedCustomsStatus(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(ABL_BillNumber):
				case nameof(TypeOfBillDocument):
					return true;
				default:
					return false;
			}
		}

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			var result = ReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);

			if (!result)
			{
				var header = Header;
				if (header != null)
				{
					if (header.IsAmendableFieldsEditAllowedCustomsStatus())
					{
						result = IsPropertyReadOnlyUnderAmendableFieldsEditAllowedCustomsStatus(property.Name);
					}
					else
					{
						result = header.IsNoEditAllowedCustomsStatus();
					}
				}
			}

			return result;
		}

		public enum AddressType
		{
			None = 0,
			Consignee = 1,
			Shipper = 2,
			NotifyParty = 3,
			FreightForwarder = 4
		}

		[ChildEditable]
		public ITemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> AdditionalInfos => fAdditionalInfos ?? (fAdditionalInfos = GetAdditionalInfos());
		ITemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> fAdditionalInfos;

		ITemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> GetAdditionalInfos()
		{
			var result = CreateNewAdditionalInfoCollection();
			result.Load();
			RegisterEditableChildObject(result);
			if (IsDeconsolidationAndIsMasterBill)
			{
				result.SetReadOnlyIncludingChildren(true);
			}
			return result;
		}

		protected virtual ITemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> CreateNewAdditionalInfoCollection()
		{
			return new TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>(this);
		}

		[ChildEditable]
		public ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> SupplyChainActors
		{
			get
			{
				if (supplyChainActors == null)
				{
					supplyChainActors = new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
					supplyChainActors.Load();
					RegisterEditableChildObject(supplyChainActors);
					if (IsSupplyChainActorReferenceCollectionReadOnly)
					{
						supplyChainActors.SetCountedReadOnlyIncludingChildren(true);
					}
				}
				return supplyChainActors;
			}
		}
		ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> supplyChainActors;

		protected virtual bool IsSupplyChainActorReferenceCollectionReadOnly => IsDeconsolidationAndIsMasterBill;

		[ResourceStringData("BB2B8CAE-2793-4D60-8FA7-398384011E81", Caption = "Is Master?", MediumCaption = "Is Master?", ShortCaption = "Is Master?")]
		public ZBool ABL_Calc_IsMaster => ABL_BolType == ChildBolCode;

		public const string ChildBolCode = "BOL";

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.BillTypeList))]
		[ResourceStringData("8d0ca4e9-4d14-4ce2-9cf0-a0c59d1a5419", Caption = "Type of Bill Document", MediumCaption = "Type", ShortCaption = "Type")]
		[MaxLength(Schema.TypeOfBillDocumentMaxLength)]
		public virtual ZString TypeOfBillDocument
		{
			get => this.GetSystemDefinedValue<ZString>(Customs.Business.GenAddOnHelper.TypeOfBillDocument);
			set
			{
				BusinessObject.CheckMaximumLength(TypeOfBillDocumentInfo, value);
				this.SetSystemDefinedValue(Customs.Business.GenAddOnHelper.TypeOfBillDocument, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTypeOfBillDocument();
				}
				TypeOfBillDocumentInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TypeOfBillDocumentInfo => GetZPropertyInfo(nameof(TypeOfBillDocument));

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.BillKindList))]
		[ResourceStringData("9db4d994-0f8e-4387-a8ed-79d6d986f396", Caption = "Kind of Bill", MediumCaption = "Kind", ShortCaption = "Kind")]
		public override ZString ABL_BolType
		{
			get => base.ABL_BolType;
			set
			{
				bool hasChanged = ABL_BolType != value;
				base.ABL_BolType = value;

				if (hasChanged && !IsCopying)
				{
					Header?.Bills.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("1ee0cbd9-60dc-4c60-9e00-8bcfe2ca76c9", Caption = "Bill Number", MediumCaption = "Bill #", ShortCaption = "Bill")]
		public override ZString ABL_BillNumber
		{
			get => base.ABL_BillNumber;
			set
			{
				bool hasChanged = ABL_BillNumber != value;
				base.ABL_BillNumber = value;

				if (hasChanged && !IsCopying)
				{
					Header?.Bills.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("bc57cb96-dea5-4eda-987f-a4aa255f3d49", Caption = "UCR Number", MediumCaption = "UCR #", ShortCaption = "UCR")]
		[ReadOnlyMember(nameof(IsUCRNumberReadOnly))]
		public override ZString ABL_UCRNumber
		{
			get => base.ABL_UCRNumber;
			set => base.ABL_UCRNumber = value;
		}

		protected virtual bool IsUCRNumberReadOnly => IsDeconsolidationAndIsMasterBill;

		[ResourceStringData("8a26c90d-7082-46a4-a168-218b85f55fb1", Caption = "Gross Mass", MediumCaption = "Gross Mass", ShortCaption = "Gross Mass")]
		[ReadOnlyMember(nameof(IsDeconsolidationAndIsMasterBill))]
		public override ZDecimal ABL_GrossWeight
		{
			get => base.ABL_GrossWeight;
			set => base.ABL_GrossWeight = value;
		}

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.GrossWeightUnitList))]
		[ResourceStringData("af7cb955-744e-4306-b92c-0b6d2e93d728", Caption = "Gross Mass Unit", MediumCaption = "Unit", ShortCaption = "Unit")]
		[ReadOnlyMember(nameof(IsDeconsolidationAndIsMasterBill))]
		public override ZString ABL_GrossWeightUQ
		{
			get => base.ABL_GrossWeightUQ;
			set => base.ABL_GrossWeightUQ = value;
		}

		public override ZGuid ABL_AMA
		{
			get => base.ABL_AMA;
			set
			{
				var hasChanged = ABL_AMA != value;
				base.ABL_AMA = value;

				if (hasChanged && !IsCopying)
				{
					Header?.Containers.MarkAsNeedingValidation();
				}
			}
		}

		public override ZInt ABL_ClusterKey
		{
			get => base.ABL_ClusterKey;
			set
			{
				var hasChanged = ABL_ClusterKey != value;
				base.ABL_ClusterKey = value;

				if (hasChanged && !IsCopying)
				{
					Header?.Containers.MarkAsNeedingValidation();
				}
			}
		}

		public ZDecimal GrossWeightInKilogramsSafe => new ZWeight(ABL_GrossWeight, ABL_GrossWeightUQ).InKilogramsSafe;

		public ZPropertyInfo HasNoMasterBillInfo => GetZPropertyInfo(nameof(HasNoMasterBill));

		public ZBool HasNoMasterBill
		{
			get { return this.GetSystemDefinedValue<ZBool>(Schema.HasNoMasterBill); }
			set
			{
				ZBool oldValue = HasNoMasterBill;
				if (oldValue != value)
				{
					this.SetSystemDefinedValue(Schema.HasNoMasterBill, AddOnColumnDataType.Codes.Boolean, value);
				}
				HasNoMasterBillInfo.RefreshBinding(oldValue);
			}
		}

		#region Shipper

		#region ConsignorOrgPK

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.ConsignorOrganizationList))]
		[ResourceStringData("d8a4dfcf-a8cb-490c-ba0f-4be2d0b32f94", Caption = "Consignor Code")]
		[ReadOnlyMember(nameof(IsDeconsolidationAndIsMasterBill))]
		public ZGuid ConsignorOrgPK
		{
			get => ABL_OA_Shipper_ZAddress.OrgPK;
			set
			{
				ABL_OA_Shipper_ZAddress.OrgPK = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateConsignorOrgPK();
				}
				ConsignorOrgPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsignorOrgPKInfo => GetWrappedZPropertyInfo(Schema.ConsignorOrgPK, x => ABL_OA_Shipper_ZAddress.OrgPKInfo);

		#endregion

		protected override ZAddress GetNewABL_OA_Shipper_ZAddress()
		{
			var result = base.GetNewABL_OA_Shipper_ZAddress();
			result.DefaultAddressType = ZArchitecture.Business.AddressType.PIC;
			return result;
		}

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.ConsignorOrganizationList))]
		[ResourceStringData("86761de9-9287-4f2c-8502-e98595f17b04", Caption = "Consignor Address")]
		[ReadOnlyMember(nameof(IsDeconsolidationAndIsMasterBill))]
		public override ZGuid ABL_OA_Shipper
		{
			get => base.ABL_OA_Shipper;
			set
			{
				var oldValue = ABL_OA_Shipper;
				base.ABL_OA_Shipper = value;
				if (!IsCopying && oldValue != ABL_OA_Shipper)
				{
					ClearValueIfNeeded(!ABL_OA_Shipper.IsEmpty, ABL_ShipperNameInfo, ABL_ShipperStreet1Info, ABL_ShipperStreet2Info, ABL_ShipperCityInfo, ABL_ShipperStateInfo, ABL_ShipperPostcodeInfo, ABL_RN_NKShipperCountryInfo, ABL_ShipperRegNoInfo, ABL_ShipperRegNoTypeInfo, ABL_ShipperPhoneInfo);
					DefaultPartyOrgAddressDetails(Shipper, AddressType.Shipper);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateConsignorOrgPK();
				}
			}
		}
		protected bool ShipperUseRealOrg => !ABL_OA_Shipper.IsEmpty;

		[ReadOnlyMember(nameof(ShipperReadOnly))]
		[MaxLength(Schema.NameMaximumLength)]
		public override ZString ABL_ShipperName { get => base.ABL_ShipperName; set => base.ABL_ShipperName = value; }

		[ReadOnlyMember(nameof(ShipperReadOnly))]
		public override ZString ABL_ShipperStreet1 { get => base.ABL_ShipperStreet1; set => base.ABL_ShipperStreet1 = value; }

		[ReadOnlyMember(nameof(ShipperReadOnly))]
		public override ZString ABL_ShipperStreet2 { get => base.ABL_ShipperStreet2; set => base.ABL_ShipperStreet2 = value; }

		[ReadOnlyMember(nameof(ShipperReadOnly))]
		[MaxLength(Schema.CityMaximumLength)]
		public override ZString ABL_ShipperCity { get => base.ABL_ShipperCity; set => base.ABL_ShipperCity = value; }

		[ReadOnlyMember(nameof(ShipperReadOnly))]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.ShipperState_List))]
		public override ZString ABL_ShipperState { get => base.ABL_ShipperState; set => base.ABL_ShipperState = value; }

		[ReadOnlyMember(nameof(ShipperReadOnly))]
		public override ZString ABL_ShipperPostcode { get => base.ABL_ShipperPostcode; set => base.ABL_ShipperPostcode = value; }

		[ReadOnlyMember(nameof(ShipperReadOnly))]
		public override ZString ABL_ShipperPhone { get => base.ABL_ShipperPhone; set => base.ABL_ShipperPhone = value; }

		[ReadOnlyMember(nameof(ShipperReadOnly))]
		public override ZString ABL_RN_NKShipperCountry { get => base.ABL_RN_NKShipperCountry; set => base.ABL_RN_NKShipperCountry = value; }

		[ReadOnlyMember(nameof(ShipperReadOnly))]
		public override ZString ABL_ShipperRegNo { get => base.ABL_ShipperRegNo; set => base.ABL_ShipperRegNo = value; }

		[ReadOnlyMember(nameof(ShipperReadOnly))]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.TypeOfPersonList))]
		[ResourceStringData("D422E157-1C90-452F-B5F7-9CC28C3F8035", Caption = "Type of Person", MediumCaption = "Type of Person", ShortCaption = "Type")]
		[MaxLength(Schema.TypeOfPersonMaxLength)]
		public override ZString ABL_ShipperRegNoType
		{
			get => base.ABL_ShipperRegNoType;
			set => base.ABL_ShipperRegNoType = value;
		}

		public bool ShipperReadOnly => ShipperUseRealOrg || IsDeconsolidationAndIsMasterBill;
		#endregion

		#region Consignee

		#region ConsigneeOrgPK

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.ConsigneeOrganizationList))]
		[ResourceStringData("786916dc-970c-4bf0-aae4-d7489f3ed804", Caption = "Consignee Code")]
		[ReadOnlyMember(nameof(IsDeconsolidationAndIsMasterBill))]
		public ZGuid ConsigneeOrgPK
		{
			get => ABL_OA_Consignee_ZAddress.OrgPK;
			set
			{
				ABL_OA_Consignee_ZAddress.OrgPK = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateConsigneeOrgPK();
				}
				ConsigneeOrgPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneeOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeOrgPK, x => ABL_OA_Consignee_ZAddress.OrgPKInfo); }
		}

		#endregion

		protected override ZAddress GetNewABL_OA_Consignee_ZAddress()
		{
			var result = base.GetNewABL_OA_Consignee_ZAddress();
			result.DefaultAddressType = ZArchitecture.Business.AddressType.DLV;
			return result;
		}

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.ConsigneeOrganizationList))]
		[ResourceStringData("3886020c-da2e-4431-bd6e-9db45251322b", Caption = "Consignee Address")]
		[ReadOnlyMember(nameof(IsDeconsolidationAndIsMasterBill))]
		public override ZGuid ABL_OA_Consignee
		{
			get => base.ABL_OA_Consignee;
			set
			{
				var oldValue = ABL_OA_Consignee;
				base.ABL_OA_Consignee = value;
				if (!IsCopying && oldValue != ABL_OA_Consignee)
				{
					ClearValueIfNeeded(!ABL_OA_Consignee.IsEmpty, ABL_ConsigneeNameInfo, ABL_ConsigneeStreet1Info, ABL_ConsigneeStreet2Info, ABL_ConsigneeCityInfo, ABL_ConsigneeStateInfo, ABL_ConsigneePostcodeInfo, ABL_RN_NKConsigneeCountryInfo, ABL_ConsigneeRegNoInfo, ABL_ConsigneeRegNoTypeInfo, ABL_ConsigneePhoneInfo);
					DefaultPartyOrgAddressDetails(Consignee, AddressType.Consignee);
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateConsigneeOrgPK();
				}
			}
		}

		protected bool ConsigneeUseRealOrg => !ABL_OA_Consignee.IsEmpty;

		[ReadOnlyMember(nameof(ConsigneeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.TypeOfPersonList))]
		[ResourceStringData("D422E157-1C90-452F-B5F7-9CC28C3F8035", Caption = "Type of Person", MediumCaption = "Type of Person", ShortCaption = "Type")]
		[MaxLength(Schema.TypeOfPersonMaxLength)]
		public override ZString ABL_ConsigneeRegNoType { get => base.ABL_ConsigneeRegNoType; set => base.ABL_ConsigneeRegNoType = value; }

		[ReadOnlyMember(nameof(ConsigneeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.ConsigneeState_List))]
		public override ZString ABL_ConsigneeState { get => base.ABL_ConsigneeState; set => base.ABL_ConsigneeState = value; }

		[ReadOnlyMember(nameof(ConsigneeReadOnly))]
		[MaxLength(Schema.NameMaximumLength)]
		public override ZString ABL_ConsigneeName { get => base.ABL_ConsigneeName; set => base.ABL_ConsigneeName = value; }

		[ReadOnlyMember(nameof(ConsigneeReadOnly))]
		public override ZString ABL_ConsigneeStreet1 { get => base.ABL_ConsigneeStreet1; set => base.ABL_ConsigneeStreet1 = value; }

		[ReadOnlyMember(nameof(ConsigneeReadOnly))]
		public override ZString ABL_ConsigneeStreet2 { get => base.ABL_ConsigneeStreet2; set => base.ABL_ConsigneeStreet2 = value; }

		[ReadOnlyMember(nameof(ConsigneeReadOnly))]
		[MaxLength(Schema.CityMaximumLength)]
		public override ZString ABL_ConsigneeCity { get => base.ABL_ConsigneeCity; set => base.ABL_ConsigneeCity = value; }

		[ReadOnlyMember(nameof(ConsigneeReadOnly))]
		public override ZString ABL_ConsigneePostcode { get => base.ABL_ConsigneePostcode; set => base.ABL_ConsigneePostcode = value; }

		[ReadOnlyMember(nameof(ConsigneeReadOnly))]
		public override ZString ABL_ConsigneePhone { get => base.ABL_ConsigneePhone; set => base.ABL_ConsigneePhone = value; }

		[ReadOnlyMember(nameof(ConsigneeReadOnly))]
		public override ZString ABL_RN_NKConsigneeCountry { get => base.ABL_RN_NKConsigneeCountry; set => base.ABL_RN_NKConsigneeCountry = value; }

		[ReadOnlyMember(nameof(ConsigneeReadOnly))]
		public override ZString ABL_ConsigneeRegNo { get => base.ABL_ConsigneeRegNo; set => base.ABL_ConsigneeRegNo = value; }

		public bool ConsigneeReadOnly => ConsigneeUseRealOrg || IsDeconsolidationAndIsMasterBill;
		#endregion

		#region Notify Party

		#region NotifyPartyOrgPK

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.NotifyPartyOrganizationList))]
		[ResourceStringData("0DEE0D3D-39D9-4677-94D1-FD7F274D57BB", Caption = "Notify Party")]
		[ReadOnlyMember(nameof(IsDeconsolidationAndIsMasterBill))]
		public ZGuid NotifyPartyOrgPK
		{
			get => ABL_OA_NotifyParty_ZAddress.OrgPK;
			set
			{
				ABL_OA_NotifyParty_ZAddress.OrgPK = value;
				NotifyPartyOrgPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NotifyPartyOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.NotifyPartyOrgPK, x => ABL_OA_NotifyParty_ZAddress.OrgPKInfo); }
		}

		#endregion

		protected override ZAddress GetNewABL_OA_NotifyParty_ZAddress()
		{
			var result = base.GetNewABL_OA_NotifyParty_ZAddress();
			result.DefaultAddressType = ZArchitecture.Business.AddressType.OFC;
			return result;
		}

		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.NotifyPartyOrganizationList))]
		[ResourceStringData("0b3abbdd-f9a4-45a8-866f-b9d6286312bc", Caption = "Notify Party", MediumCaption = "Notify")]
		[ReadOnlyMember(nameof(IsDeconsolidationAndIsMasterBill))]
		public override ZGuid ABL_OA_NotifyParty
		{
			get => base.ABL_OA_NotifyParty;
			set
			{
				var oldValue = ABL_OA_NotifyParty;
				base.ABL_OA_NotifyParty = value;
				if (!IsCopying && oldValue != ABL_OA_NotifyParty)
				{
					ClearValueIfNeeded(!ABL_OA_NotifyParty.IsEmpty, ABL_NotifyPartyNameInfo, ABL_NotifyPartyStreet1Info, ABL_NotifyPartyStreet2Info, ABL_NotifyPartyCityInfo, ABL_NotifyPartyStateInfo, ABL_NotifyPartyPostcodeInfo, ABL_RN_NKNotifyPartyCountryInfo, ABL_NotifyPartyRegNoInfo, ABL_NotifyPartyRegNoTypeInfo, ABL_NotifyPartyPhoneInfo);
					DefaultPartyOrgAddressDetails(NotifyParty, AddressType.NotifyParty);
				}
			}
		}

		protected bool NotifyPartyUseRealOrg => !ABL_OA_NotifyParty.IsEmpty;

		[ReadOnlyMember(nameof(NotifyPartyReadOnly))]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.TypeOfPersonList))]
		[ResourceStringData("D422E157-1C90-452F-B5F7-9CC28C3F8035", Caption = "Type of Person", MediumCaption = "Type of Person", ShortCaption = "Type")]
		[MaxLength(Schema.TypeOfPersonMaxLength)]
		public override ZString ABL_NotifyPartyRegNoType { get => base.ABL_NotifyPartyRegNoType; set => base.ABL_NotifyPartyRegNoType = value; }

		[ReadOnlyMember(nameof(NotifyPartyReadOnly))]
		[List(nameof(Lookups) + "." + nameof(TemporaryStorageBillLookups.NotifyPartyState_List))]
		public override ZString ABL_NotifyPartyState { get => base.ABL_NotifyPartyState; set => base.ABL_NotifyPartyState = value; }

		[ReadOnlyMember(nameof(NotifyPartyReadOnly))]
		[MaxLength(Schema.NameMaximumLength)]
		public override ZString ABL_NotifyPartyName { get => base.ABL_NotifyPartyName; set => base.ABL_NotifyPartyName = value; }

		[ReadOnlyMember(nameof(NotifyPartyReadOnly))]
		public override ZString ABL_NotifyPartyStreet1 { get => base.ABL_NotifyPartyStreet1; set => base.ABL_NotifyPartyStreet1 = value; }

		[ReadOnlyMember(nameof(NotifyPartyReadOnly))]
		public override ZString ABL_NotifyPartyStreet2 { get => base.ABL_NotifyPartyStreet2; set => base.ABL_NotifyPartyStreet2 = value; }

		[ReadOnlyMember(nameof(NotifyPartyReadOnly))]
		[MaxLength(Schema.CityMaximumLength)]
		public override ZString ABL_NotifyPartyCity { get => base.ABL_NotifyPartyCity; set => base.ABL_NotifyPartyCity = value; }

		[ReadOnlyMember(nameof(NotifyPartyReadOnly))]
		public override ZString ABL_NotifyPartyPostcode { get => base.ABL_NotifyPartyPostcode; set => base.ABL_NotifyPartyPostcode = value; }

		[ReadOnlyMember(nameof(NotifyPartyReadOnly))]
		public override ZString ABL_NotifyPartyPhone { get => base.ABL_NotifyPartyPhone; set => base.ABL_NotifyPartyPhone = value; }

		[ReadOnlyMember(nameof(NotifyPartyReadOnly))]
		public override ZString ABL_RN_NKNotifyPartyCountry { get => base.ABL_RN_NKNotifyPartyCountry; set => base.ABL_RN_NKNotifyPartyCountry = value; }

		[ReadOnlyMember(nameof(NotifyPartyReadOnly))]
		public override ZString ABL_NotifyPartyRegNo { get => base.ABL_NotifyPartyRegNo; set => base.ABL_NotifyPartyRegNo = value; }

		public bool NotifyPartyReadOnly => NotifyPartyUseRealOrg || IsDeconsolidationAndIsMasterBill;

		public virtual ZString[] RegNoTypes()
		{
			return new ZString[] { OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU };
		}

		#endregion

		void ClearValueIfNeeded(bool shouldClear, ZPropertyInfo partyNameInfo, ZPropertyInfo partyStreet1Info, ZPropertyInfo partyStreet2Info, ZPropertyInfo partyCityInfo, ZPropertyInfo partyStateInfo, ZPropertyInfo partyPostcodeInfo, ZPropertyInfo partyCountryInfo, ZPropertyInfo regNoInfo, ZPropertyInfo regNoTypeInfo, ZPropertyInfo partyPhoneInfo = null)
		{
			if (shouldClear)
			{
				ClearValueIfNeeded(partyNameInfo);
				ClearValueIfNeeded(partyStreet1Info);
				ClearValueIfNeeded(partyStreet2Info);
				ClearValueIfNeeded(partyCityInfo);
				ClearValueIfNeeded(partyStateInfo);
				ClearValueIfNeeded(partyPostcodeInfo);
				ClearValueIfNeeded(partyCountryInfo);
				ClearValueIfNeeded(regNoInfo);
				ClearValueIfNeeded(regNoTypeInfo);
				if (partyPhoneInfo != null)
				{
					ClearValueIfNeeded(partyPhoneInfo);
				}
			}
		}

		void ClearValueIfNeeded(ZPropertyInfo info)
		{
			if (!info.Value.IsDefault)
			{
				info.Value = info.DefaultValue;
			}
		}

		void DefaultPartyOrgAddressDetails(OrgAddress org, AddressType type)
		{
			if (org != null)
			{
				switch (type)
				{
					case AddressType.Shipper:
						PopulateOrgAddressDetails(org, ABL_ShipperNameInfo, ABL_ShipperStreet1Info, ABL_ShipperStreet2Info, ABL_ShipperCityInfo, ABL_ShipperStateInfo, ABL_ShipperPostcodeInfo, ABL_ShipperPhoneInfo, ABL_RN_NKShipperCountryInfo, ABL_ShipperRegNoInfo, ABL_ShipperRegNoTypeInfo, RegNoTypes());
						break;
					case AddressType.Consignee:
						PopulateOrgAddressDetails(org, ABL_ConsigneeNameInfo, ABL_ConsigneeStreet1Info, ABL_ConsigneeStreet2Info, ABL_ConsigneeCityInfo, ABL_ConsigneeStateInfo, ABL_ConsigneePostcodeInfo, ABL_ConsigneePhoneInfo, ABL_RN_NKConsigneeCountryInfo, ABL_ConsigneeRegNoInfo, ABL_ConsigneeRegNoTypeInfo, RegNoTypes());
						break;
					case AddressType.NotifyParty:
						PopulateOrgAddressDetails(org, ABL_NotifyPartyNameInfo, ABL_NotifyPartyStreet1Info, ABL_NotifyPartyStreet2Info, ABL_NotifyPartyCityInfo, ABL_NotifyPartyStateInfo, ABL_NotifyPartyPostcodeInfo, ABL_NotifyPartyPhoneInfo, ABL_RN_NKNotifyPartyCountryInfo, ABL_NotifyPartyRegNoInfo, ABL_NotifyPartyRegNoTypeInfo, RegNoTypes());
						break;
				}
			}
		}

		void PopulateOrgAddressDetails(OrgAddress org, ZPropertyInfo nameInfo, ZPropertyInfo street1Info, ZPropertyInfo street2Info,
			ZPropertyInfo cityInfo, ZPropertyInfo stateInfo, ZPropertyInfo postCodeInfo, ZPropertyInfo phoneInfo, ZPropertyInfo countryInfo, ZPropertyInfo regNoInfo, ZPropertyInfo regNoTypeInfo, ZString[] regNoTypes)
		{
			if (org != null)
			{
				if (nameInfo != null)
				{
					nameInfo.Value = string.IsNullOrWhiteSpace(org.EffectiveCompanyName) ? ZString.Empty : org.EffectiveCompanyName.Trim().Left(Math.Min(70, nameInfo.MaxLength));
				}
				if (countryInfo != null)
				{
					countryInfo.Value = string.IsNullOrWhiteSpace(org.OA_RN_NKCountryCode) ? ZString.Empty : org.OA_RN_NKCountryCode.Trim().Left(countryInfo.MaxLength);
				}
				if (street1Info != null)
				{
					street1Info.Value = string.IsNullOrWhiteSpace(org.OA_Address1) ? ZString.Empty : org.OA_Address1.Trim().Left(street1Info.MaxLength);
				}
				if (street2Info != null)
				{
					street2Info.Value = string.IsNullOrWhiteSpace(org.Address2) ? ZString.Empty : org.Address2.Trim().Left(street2Info.MaxLength);
				}
				if (cityInfo != null)
				{
					cityInfo.Value = string.IsNullOrWhiteSpace(org.OA_City) ? ZString.Empty : org.OA_City.Trim().Left(Math.Min(35, cityInfo.MaxLength));
				}
				if (stateInfo != null)
				{
					stateInfo.Value = string.IsNullOrWhiteSpace(org.OA_State) ? ZString.Empty : org.OA_State.Trim().Left(stateInfo.MaxLength);
				}
				if (postCodeInfo != null)
				{
					postCodeInfo.Value = string.IsNullOrWhiteSpace(org.OA_PostCode) ? ZString.Empty : org.OA_PostCode.Trim().Left(postCodeInfo.MaxLength);
				}
				if (phoneInfo != null)
				{
					phoneInfo.Value = org.PhoneNumber.FormattedForBinding.Left(phoneInfo.MaxLength);
				}

				if (regNoInfo != null && regNoTypeInfo != null && Header != null)
				{
					var regNo = GetRegNoForParty(org, regNoTypes);
					regNoInfo.Value = regNo.Left(regNoInfo.MaxLength);
					switch (org.Header.OH_Category)
					{
						case OrgConstants.Category.NaturalPersonIndividual:
							regNoTypeInfo.Value = (ZString)"1";
							break;
						case OrgConstants.Category.Business:
						case OrgConstants.Category.Government:
						case OrgConstants.Category.NonGovernmentOrganisation:
							regNoTypeInfo.Value = (ZString)"2";
							break;
						default:
							break;
					}
				}
			}
		}

		public ZString GetRegNoForParty(OrgAddress address, ZString[] regNoTypes) => GetRegNoForPartyCore(address, regNoTypes);

		protected virtual ZString GetRegNoForPartyCore(OrgAddress address, ZString[] regNoTypes)
		{
			var regNo = ZString.Empty;
			foreach (var type in regNoTypes)
			{
				if (type == OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU)
				{
					regNo = address.Header.CustomsCodes.GetCustomsRegNo(type);
				}
				else
				{
					regNo = address.Header.CustomsCodes.GetCustomsRegNo(type, GetCountryCodeForCustomsRegNo(address));
				}
				if (!regNo.IsEmpty)
				{
					break;
				}
			}
			return regNo;
		}

		protected virtual ZString GetCountryCodeForCustomsRegNo(OrgAddress address) => Header.AMA_RN_NKCountry;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		}

		public new TemporaryStorageHeader Header => (TemporaryStorageHeader)base.Header;

		public new TemporaryStorageBillLookups Lookups => (TemporaryStorageBillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new TemporaryStorageBillLookups(this);

		public new TemporaryStorageBillValidation Validation => (TemporaryStorageBillValidation)base.Validation;

		protected override ManifestBase.AsycudaBillValidation GetNewValidation() => new TemporaryStorageBillValidation(this);

		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => GetCusSupportingInfoTypes();

		protected virtual IDictionary<ZString, Type> GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo, typeof(TemporaryStorageAdditionalInfo) },
				{ Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(TemporaryStorageSupportingDocument) },
				{ Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(TemporaryStoragePreviousDocument) }
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
		{
			{ CusReferenceTypeList.Codes.SupplyChainActor, SupplyChainActorType },
		};

		protected virtual Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

		[ChildEditable(true)]
		public ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> PreviousDocuments
		{
			get { return previousDocuments ?? (previousDocuments = GetPreviousDocuments()); }
		}
		ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> previousDocuments;

		ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> GetPreviousDocuments()
		{
			var result = CreateNewPreviousDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);
			if (IsDeconsolidationAndIsMasterBill)
			{
				result.SetReadOnlyIncludingChildren(true);
			}

			return result;
		}

		protected virtual ITemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument> CreateNewPreviousDocumentCollection() => new TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>(this);

		#region SupportingDocuments

		[ChildEditable(true)]
		public ITemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument> SupportingDocuments
		{
			get { return supportingDocuments ?? (supportingDocuments = GetSupportingDocuments()); }
		}
		ITemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument> supportingDocuments;

		ITemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument> GetSupportingDocuments()
		{
			var result = CreateNewSupportingDocumentCollection();
			result.Load();
			RegisterEditableChildObject(result);
			if (IsDeconsolidationAndIsMasterBill)
			{
				result.SetReadOnlyIncludingChildren(true);
			}

			return result;
		}

		protected virtual ITemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument> CreateNewSupportingDocumentCollection() => new TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument>(this);

		#endregion SupportingDocuments

		protected override Type GetPackTypeCore() => typeof(TemporaryStoragePack);

		public new TemporaryStoragePackCollection Packs => (TemporaryStoragePackCollection)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection()
		{
			var packCollection = new TemporaryStoragePackCollection(this);
			if (IsDeconsolidationAndIsMasterBill)
			{
				packCollection.SetReadOnlyIncludingChildren(true);
			}
			return packCollection;
		}

		protected override Type GetPackedItemTypeCore() => typeof(TemporaryStoragePackedItem);

		public new ITemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill> PackedItems
		{
			get
			{
				var result = (ITemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>)base.PackedItems;
				if (IsDeconsolidationAndIsMasterBill)
				{
					result.SetCountedReadOnlyIncludingChildren(true);
				}
				return result;
			}
		}

		protected override ManifestBase.IAsycudaBillPackedItemCollection<ManifestBase.AsycudaPackedItem, ManifestBase.AsycudaBill> CreateNewAsycudaBillPackedItemCollection() => new TemporaryStoragePackedItemCollection<TemporaryStoragePackedItem, TemporaryStorageBill>(this);

		public bool IsDeconsolidationAndIsMasterBill => Header != null && Header.IsDeconsolidation && ABL_Calc_IsMaster;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}
			base.Delete();
		}

		#region Cloning and copying

		internal ITemporaryStorageBillValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, () => Header?.Configuration.BillConfiguration.GetValidationDecider());
		CachedValue<ITemporaryStorageBillValidationDecider> validationDeciderCached;

		protected override bool SupportsCloneCore() => true;

		protected virtual TemporaryStorageHeaderCloneStrategy GetTemporaryStorageHeaderCloneStrategy(BusinessObject bizObjToClone) => new(bizObjToClone);

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newBill = (TemporaryStorageBill)base.CloneInternal(args);

			Packs.ForEach(pack => newBill.Packs.Add(Clone(pack)));
			PackedItems.ForEach(packedItem => newBill.PackedItems.Add(Clone(packedItem)));
			SupportingDocuments.ForEach(supDoc => newBill.SupportingDocuments.Add(Clone(supDoc)));
			PreviousDocuments.ForEach(preDoc => newBill.PreviousDocuments.Add(Clone(preDoc)));
			AdditionalInfos.ForEach(addInfo => newBill.AdditionalInfos.Add(Clone(addInfo)));
			SupplyChainActors.ForEach(addInfo => newBill.SupplyChainActors.Add(Clone(addInfo)));

			return newBill;

			BusinessObject Clone(BusinessObject bizObjToClone)
				=> GetTemporaryStorageHeaderCloneStrategy(bizObjToClone).Clone();
		}

		#endregion
	}
}
