using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.DE.Messaging.MessageSchema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	[DependentBusinessObject(typeof(CusTempStorageDec), nameof(CusTempStorageDec.CusTempStorageLines))]
	public abstract class CusTempStorageLine : EU.Business.CusTempStorage.CusTempStorageLine
	{
		protected CusTempStorageLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EU.Business.CusTempStorage.CusTempStorageLine.Schema
		{
			public new const int TSL_OwnerReferenceNumberMaxLength = 44;
			public new const int TSL_GoodsDescriptionMaxLength = 140;
			public const int MaxLineNoValue = 9999;
		}

		public new static readonly CusTempStorageLineTypeDecider TypeDecider = new CusTempStorageLineTypeDecider();

		public CusTempStorageJobHeader StorageHeader => Dec.StorageHeader;

		public new CusTempStorageDec Dec => (CusTempStorageDec)base.Dec;

		#region Custodian

		[ResourceStringData("82981B9F-589E-41BE-9FE8-15821C67F60E", Caption = "Custodian Address")]
		public override ZGuid TSL_OA_Custodian
		{
			get => base.TSL_OA_Custodian;
			set
			{
				var hasChanged = TSL_OA_Custodian != value;
				if (hasChanged)
				{
					base.TSL_OA_Custodian = value;
					if (!isSuspendSettingDefaultCustodianDetails)
					{
						var (eoriCode, branchCode) = GetEoriAndBranchCodeFromOrgAddress(Custodian);
						TSL_CustodianIdentifier = eoriCode;
						TSL_CustodianIdentifierBranchNo = branchCode;
					}
				}
			}
		}

		protected override ZAddress GetNewTSL_OA_Custodian_ZAddress()
		{
			var address = base.GetNewTSL_OA_Custodian_ZAddress();
			address.GetDefaultAddress = (header) => header?.MainAddress?.PK ?? ZGuid.Empty;
			return address;
		}

		[MaxLength(ATLASMessageSchema.EoriCodeMaxLength)]
		[ResourceStringData("586B21C3-9B27-4A93-93FA-64AA050DAFFE", Caption = "Custodian EORI")]
		public override ZString TSL_CustodianIdentifier
		{
			get => base.TSL_CustodianIdentifier;
			set
			{
				var hasChanged = TSL_CustodianIdentifier != value;
				if (hasChanged)
				{
					base.TSL_CustodianIdentifier = value;
					if (!isSuspendSettingDefaultCustodianDetails && Custodian == null)
					{
						var (orgAddressPK, branchCode) = GetOrgAddressPKAndBranchCodeFromEori(value);
						TSL_OA_Custodian = orgAddressPK;
						TSL_CustodianIdentifierBranchNo = branchCode;
					}
				}
			}
		}

		[MaxLength(ATLASMessageSchema.EoriBranchCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.CustodianIdentifierBranchNoList))]
		[ResourceStringData("193AB569-C8B1-477C-921E-AB04759548B1", Caption = "Custodian Branch")]
		public override ZString TSL_CustodianIdentifierBranchNo
		{
			get => base.TSL_CustodianIdentifierBranchNo;
			set
			{
				var hasChanged = TSL_CustodianIdentifierBranchNo != value;
				if (hasChanged)
				{
					base.TSL_CustodianIdentifierBranchNo = value;
					if (!isSuspendSettingDefaultCustodianDetails)
					{
						var eoriCode = TSL_CustodianIdentifier;
						var branchNo = TSL_CustodianIdentifierBranchNo;
						if (!eoriCode.IsEmpty && !branchNo.IsEmpty && Lookups.CustodianIdentifierBranchNoList.ContainsCode(branchNo))
						{
							TSL_OA_Custodian = GetOrgAddressFromEoriAndBranchCode(eoriCode, branchNo);
						}
					}
				}
			}
		}
		bool isSuspendSettingDefaultCustodianDetails;

		public IDisposable SuspendSettingDefaultCustodianDetails()
		{
			return new DisposableAction(() => isSuspendSettingDefaultCustodianDetails = true, () => isSuspendSettingDefaultCustodianDetails = false);
		}

		[ResourceStringData("5DB233AE-9CD1-41FE-9809-16E751CAD3F8", Caption = "Custodian Name")]
		public ZString CustodianName => Custodian?.EffectiveCompanyName ?? ZString.Empty;

		public ZPropertyInfo CustodianNameInfo => GetZPropertyInfo(nameof(CustodianName));

		[ResourceStringData("8a3f958f-8da9-40e5-ae7a-88bd785c35f6", Caption = "Disposal Entitled Trader Name")]
		public ZString GoodsOwnerName => GoodsOwner?.EffectiveCompanyName ?? ZString.Empty;

		public ZPropertyInfo GoodsOwnerNameInfo => GetZPropertyInfo(nameof(GoodsOwnerName));

		#endregion

		#region Disposal Entitled Trader

		public override ZGuid TSL_OA_GoodsOwner
		{
			get => base.TSL_OA_GoodsOwner;
			set
			{
				var hasChanged = TSL_OA_GoodsOwner != value;
				if (hasChanged)
				{
					base.TSL_OA_GoodsOwner = value;
					var (eoriCode, branchCode) = GetEoriAndBranchCodeFromOrgAddress(GoodsOwner);
					TSL_GoodsOwnerIdentifier = eoriCode;
					TSL_GoodsOwnerIdentifierBranchNo = branchCode;
				}
			}
		}

		protected override ZAddress GetNewTSL_OA_GoodsOwner_ZAddress()
		{
			var address = base.GetNewTSL_OA_GoodsOwner_ZAddress();
			address.GetDefaultAddress = (header) => header?.MainAddress?.PK ?? ZGuid.Empty;
			return address;
		}

		[MaxLength(ATLASMessageSchema.EoriCodeMaxLength)]
		[ResourceStringData("DFF06A1B-0BE8-4531-8936-70D27D416325", Caption = "Entitled Trader EORI")]
		public override ZString TSL_GoodsOwnerIdentifier
		{
			get => base.TSL_GoodsOwnerIdentifier;
			set
			{
				var hasChanged = TSL_GoodsOwnerIdentifier != value;
				if (hasChanged)
				{
					base.TSL_GoodsOwnerIdentifier = value;
					if (GoodsOwner == null)
					{
						var result = GetOrgAddressPKAndBranchCodeFromEori(value);
						TSL_OA_GoodsOwner = result.orgAddressPK;
						TSL_GoodsOwnerIdentifierBranchNo = result.branchCode;
					}
				}
			}
		}

		[MaxLength(ATLASMessageSchema.EoriBranchCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.GoodsOwnerIdentifierBranchNoList))]
		[ResourceStringData("2C11E94B-7D92-4C75-84D5-60B7895B3B96", Caption = "Entitled Trader Branch")]
		public override ZString TSL_GoodsOwnerIdentifierBranchNo
		{
			get => base.TSL_GoodsOwnerIdentifierBranchNo;
			set
			{
				var hasChanged = TSL_GoodsOwnerIdentifierBranchNo != value;
				if (hasChanged)
				{
					base.TSL_GoodsOwnerIdentifierBranchNo = value;
					var eoriCode = TSL_GoodsOwnerIdentifier;
					var branchNo = TSL_GoodsOwnerIdentifierBranchNo;
					if (!eoriCode.IsEmpty && !branchNo.IsEmpty && Lookups.GoodsOwnerIdentifierBranchNoList.ContainsCode(branchNo))
					{
						TSL_OA_GoodsOwner = GetOrgAddressFromEoriAndBranchCode(eoriCode, branchNo);
					}
				}
			}
		}

		#endregion

		#region Properties

		public override ZGuid TSL_STH
		{
			get => base.TSL_STH;
			set
			{
				var oldvalue = base.TSL_STH;
				base.TSL_STH = value;
				if (!IsCopying && oldvalue != value)
				{
					SetLineNumOnSettingTSL_STH();
				}
			}
		}

		[ReadOnlyMember(nameof(ReadOnlyTSL_LineNo))]
		[MaxLength(4)]
		[ResourceStringData("03C16EAB-7F1E-4206-88EB-3863D50A3A52", Caption = "Line Number", MediumCaption = "Line No.", ShortCaption = "Line#")]
		public override ZInt TSL_LineNo
		{
			get => base.TSL_LineNo;
			set
			{
				base.TSL_LineNo = value <= Schema.MaxLineNoValue ? value : (ZInt)Schema.MaxLineNoValue;
			}
		}

		protected virtual bool ReadOnlyTSL_LineNo => true;

		[ReadOnly(true)]
		[ResourceStringData("30868DE8-03FB-4055-A1BB-63534CB55547", Caption = "Customs Status")]
		public override ZString TSL_CustomsStatus { get => base.TSL_CustomsStatus; set => base.TSL_CustomsStatus = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.LocationOfGoodsList))]
		[ResourceStringData("c124c07f-e5ea-4fdc-b793-1de606c3f511", Caption = "Goods Location")]
		public override ZString TSL_LocationOfGoods { get => base.TSL_LocationOfGoods; set => base.TSL_LocationOfGoods = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.GoodsTypeList))]
		[ResourceStringData("f89978bc-6fec-4f77-b962-669496a989fc", Caption = "Goods Type")]
		public override ZString TSL_GoodsType { get => base.TSL_GoodsType; set => base.TSL_GoodsType = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.OwnerReferenceTypeList))]
		[ResourceStringData("d53fca36-c5c0-40b3-a85c-e81473c64dd4", ShortCaption = "Ref. Type", MediumCaption = "Owner Ref. Type", Caption = "Owner Reference Type")]
		public override ZString TSL_OwnerReferenceType { get => base.TSL_OwnerReferenceType; set => base.TSL_OwnerReferenceType = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.PackageTypeList))]
		[ResourceStringData("a2c07cf3-fc81-41b0-8d40-adf585ce30d8", Caption = "Package Type")]
		public override ZString TSL_PackageType { get => base.TSL_PackageType; set => base.TSL_PackageType = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.UnionStatusList))]
		[ResourceStringData("41b95051-624e-4069-8be8-0483c7a59d4e", Caption = "Union Status")]
		public override ZString TSL_UnionStatus { get => base.TSL_UnionStatus; set => base.TSL_UnionStatus = value; }

		[MaxLength(Schema.TSL_GoodsDescriptionMaxLength)]
		[ResourceStringData("718b0cb6-d96e-43f9-a579-6e620a90163c", Caption = "Description Of Goods", ShortCaption = "Goods Description")]
		public override ZString TSL_GoodsDescription { get => base.TSL_GoodsDescription; set => base.TSL_GoodsDescription = value; }

		[MaxLength(Schema.TSL_OwnerReferenceNumberMaxLength)]
		public override ZString TSL_OwnerReferenceNumber { get => base.TSL_OwnerReferenceNumber; set => base.TSL_OwnerReferenceNumber = value; }

		[ResourceStringData("ab3cae7a-dfec-4b6a-be55-475170379177", Caption = "Package Count")]
		public override ZInt TSL_PackageQty { get => base.TSL_PackageQty; set => base.TSL_PackageQty = value; }

		[DecimalPlaces(3)]
		[ResourceStringData("76d5950b-ffa1-4086-aaa6-af069ca910d6", Caption = "Gross Weight kg's")]
		public override ZDecimal TSL_GrossWeight { get => base.TSL_GrossWeight; set => base.TSL_GrossWeight = value; }

		[List(nameof(Lookups) + "." + nameof(CusTempStorageLineLookups.Countries))]
		[ResourceStringData("879c2112-88bb-43bd-bd1d-f6dd4c857bf0", Caption = "Country/Region Of Departure", ShortCaption = "Departure Ctry./Rgn.")]
		public override ZString TSL_RN_NKDepartureCountry { get => base.TSL_RN_NKDepartureCountry; set => base.TSL_RN_NKDepartureCountry = value; }

		[ResourceStringData("135d6ebc-97f7-4abb-abfe-3ec6efb5ba33", Caption = "Destination Place")]
		public override ZString TSL_DestinationPlace { get => base.TSL_DestinationPlace; set => base.TSL_DestinationPlace = value; }

		[ResourceStringData("4d9eafaf-7d6c-4213-8a40-e7c876667ddd", Caption = "Free-zone Flag")]
		public override ZBool TSL_IsFTZ { get => base.TSL_IsFTZ; set => base.TSL_IsFTZ = value; }

		public bool IsAWBDeclaration => Dec?.IsAWBDeclaration ?? false;

		public bool IsREGDeclaration => Dec?.IsREGDeclaration ?? false;

		public bool IsSINDeclaration => Dec?.IsSINDeclaration ?? false;

		public bool IsSingleCountPackgeType => PackageHelper.GetSingleCountPackageTypes(Factory).Contains(TSL_PackageType);

		public bool IsULD => TSL_OwnerReferenceType == OwnerReferenceTypeList.Codes.ULD;

		public bool PackageQtyShouldBeOne => IsSingleCountPackgeType || IsULD;

		#endregion

		#region Validation

		protected override EU.Business.CusTempStorage.CusTempStorageLineValidation GetNewValidation()
		{
			return new CusTempStorageLineValidation(this);
		}

		public new CusTempStorageLineValidation Validation => (CusTempStorageLineValidation)base.Validation;

		#endregion

		#region Lookups

		protected override EU.Business.CusTempStorage.CusTempStorageLineLookups GetNewLookups() => new CusTempStorageLineLookups(this);

		public new CusTempStorageLineLookups Lookups => (CusTempStorageLineLookups)base.Lookups;

		#endregion

		#region Overrides

		public override void Delete()
		{
			if (SequenceNumberEnabled && !IsDeleted)
			{
				Dec?.LineNumberGenerator?.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
			base.Delete();
		}

		public override bool CanDelete => base.CanDelete && (Dec?.STH_MessageStatus.IsEmpty ?? true);

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("b675d821-6df5-49a3-b727-5c97d6618215", "Line cannot be deleted as customs messaging has occurred");

		#endregion

		#region Implementation

		void SetLineNumOnSettingTSL_STH()
		{
			if (SetLineNumOnSettingTSL_STHEnabled)
			{
				if (SequenceNumberEnabled)
				{
					Dec?.LineNumberGenerator?.RecalculateWhenAdded(this);
				}
				else
				{
					var lines = Dec?.CusTempStorageLines.Cast<CusTempStorageLine>() ?? Enumerable.Empty<IHugeSequenceNumberLine>();
					TSL_LineNo = (lines.Any() ? lines.Max(x => x.SequenceNumber) : ZInt.Zero) + 1;
				}
			}
		}

		#region Eori and Branch

		(ZGuid orgAddressPK, ZString branchCode) GetOrgAddressPKAndBranchCodeFromEori(ZString eoriCode)
		{
			var branchCode = ZString.Empty;
			var orgAddressPK = ZGuid.Empty;
			if (EORIHelper.ValidEoriLength(eoriCode))
			{
				var orgHeader = Factory.GetOrgHeaderFromEoriCode(eoriCode);
				if (orgHeader != null)
				{
					var eoriBranchCodes = orgHeader.CustomsCodes.GetOrgCusCodesForCodeAndCountry(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany).Where(x => x.OK_CustomsRegNo.Length <= ATLASMessageSchema.EoriBranchCodeMaxLength).ToArray();
					if (eoriBranchCodes.Length == 1)
					{
						var branchCusCode = eoriBranchCodes[0];
						branchCode = branchCusCode.OK_CustomsRegNo;
						orgAddressPK = branchCusCode.OK_OA_PremisesAddress;
					}
				}
			}
			return (orgAddressPK, branchCode);
		}

		(ZString eoriCode, ZString branchCode) GetEoriAndBranchCodeFromOrgAddress(OrgAddress address)
		{
			var eoriCode = ZString.Empty;
			var branchCode = ZString.Empty;
			if (address != null)
			{
				eoriCode = address.Header?.GetEUEoriDetails() ?? ZString.Empty;
				var eoriBranchCode = address.CustomsCodes.GetOrgCusCodeObjectForCodeTypeAndCountry(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany);
				if (eoriBranchCode != null)
				{
					branchCode = eoriBranchCode.OK_CustomsRegNo;
				}
			}
			return (eoriCode, branchCode);
		}

		ZGuid GetOrgAddressFromEoriAndBranchCode(ZString eoriCode, ZString branchCode)
		{
			var result = ZGuid.Empty;
			if (!eoriCode.IsEmpty && !branchCode.IsEmpty && EORIHelper.ValidEoriLength(eoriCode))
			{
				var orgHeader = EORIHelper.GetOrgHeaderFromEoriCode(Factory, eoriCode);
				if (orgHeader != null)
				{
					var eoriBranchCodes = orgHeader.CustomsCodes.GetOrgCusCodesForCodeAndCountry(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany).Where(x => x.OK_CustomsRegNo == branchCode).ToArray();
					if (eoriBranchCodes.Length == 1)
					{
						result = eoriBranchCodes[0].OK_OA_PremisesAddress;
					}
				}
			}
			return result;
		}

		#endregion

		#endregion

	}
}
