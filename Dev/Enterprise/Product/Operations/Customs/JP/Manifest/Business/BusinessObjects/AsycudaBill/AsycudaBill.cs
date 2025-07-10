using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.JP.Manifest.Business
{
	[CodeProperty(AsycudaBill.Schema.ABL_BillNumber), DescriptionProperty(AsycudaBill.Schema.ABL_BillNumber)]
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill,
		Integration.Customs.ICusSupportingInfoTypeSupporter,
		ITariffFormatProvider,
		ICusOtherLawReferenceParent,
		ISupportMultipleResourceStringData
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new partial class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const int ABL_CountryOfOriginMaxLength = 2;
			public const int ABL_TariffMaxLength = 10;
			public const int TemporaryLandingReasonMaxLength = 3;
			public const int TemporaryLandingBondedTransportCodeMaxLength = 2;

			public new const int ABL_GoodsLocationMaxLength = 5;
			public new const int ABL_GoodsDescriptionMaxLength = 350;
			public new const int ABL_BillNumberMaxLength = 35;
			public new const int ABL_LocationInformationMaxLength = 35;
			public new const int ABL_MarksAndNumbersMaxLength = 140;
			public new const int ABL_RemarksMaxLength = 140;
		}

		public const string HCH01EndHAWB = "END";

		public bool IsDummySendingObjectForHCH01End => Factory is ReadOnlyBusinessObjectFactory && Header.IsHCH;

		public bool HasConsigneeRegNoCode => Factory.GetCached(ref hasConsigneeRegNoCodeProperty, () =>
			!ABL_OA_Consignee.IsEmpty
			&& !ABL_ConsigneeRegNoType.IsEmpty
			&& (!Consignee?.Header.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(ABL_ConsigneeRegNoType, Core.Constants.CountryCodes.Japan, Consignee.PK).IsEmpty ?? false));

		CachedProperty<bool> hasConsigneeRegNoCodeProperty;

		public bool HasShipperRegNoCode => Factory.GetCached(ref hasShipperRegNoCodeProperty, () =>
			!ABL_OA_Shipper.IsEmpty
			&& !ABL_ShipperRegNoType.IsEmpty
			&& (!Shipper?.Header.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(ABL_ShipperRegNoType, Core.Constants.CountryCodes.Japan, Shipper.PK).IsEmpty ?? false));

		CachedProperty<bool> hasShipperRegNoCodeProperty;

		public bool HasNotifyPartyRegNoCode => Factory.GetCached(ref hasNotifyPartyRegNoCodeProperty, () =>
			!ABL_OA_NotifyParty.IsEmpty
			&& !ABL_NotifyPartyRegNoType.IsEmpty
			&& (!NotifyParty?.Header.CustomsCodes.GetCustomsRegNoPremiseAddressOnly(ABL_NotifyPartyRegNoType, Core.Constants.CountryCodes.Japan, NotifyParty.PK).IsEmpty ?? false));

		CachedProperty<bool> hasNotifyPartyRegNoCodeProperty;

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => Header?.MultipleKeysToUse;

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		[MaxLength(Schema.ABL_CountryOfOriginMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CountryCollection))]
		[ResourceStringData("JPAsycudaBill.ABL_CountryOfOrigin", Caption = "Goods Origin")]
		public ZString ABL_CountryOfOrigin
		{
			get => this.GetSystemDefinedValue<ZString>(Constants.GenAddOnColumnFieldName.ABL_CountryOfOrigin);
			set
			{
				var oldValue = ABL_CountryOfOrigin;
				if (value != oldValue)
				{
					CheckMaximumLength(ABL_CountryOfOriginInfo, value);
					this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.ABL_CountryOfOrigin, value);
					RegularBillValidation.ValidateABL_CountryOfOrigin();

					ABL_CountryOfOriginInfo.RefreshBinding(oldValue);
				}
			}
		}

		public override ZGuid ABL_JS_Shipment
		{
			get => base.ABL_JS_Shipment;
			set
			{
				var oldValue = base.ABL_JS_Shipment;
				base.ABL_JS_Shipment = value;
				if (oldValue != value && !IsCopying)
				{
					DefaultPacksFromShipment();
				}
			}
		}

		void DefaultPacksFromShipment()
		{
			var shipment = Shipment;
			if (shipment != null && ABL_ManifestUQ.IsEmpty && ABL_ManifestQty.IsEmpty)
			{
				var refPack = AsycudaPackHelper.LoadRefPackForManifestBill(Factory, shipment.JS_F3_NKPackType, Core.Constants.CountryCodes.Japan);
				if (refPack != null)
				{
					ABL_ManifestQty = new ZDecimal(refPack.ConversionFactor * shipment.JS_OuterPacks).ToZInt();
					ABL_ManifestUQ = refPack.RP_CustomsPack;
				}
				else
				{
					ABL_ManifestQty = shipment.JS_OuterPacks;
				}
			}
		}

		public ZPropertyInfo ABL_CountryOfOriginInfo
		{
			get { return GetZPropertyInfo(Constants.GenAddOnColumnFieldName.ABL_CountryOfOrigin); }
		}

		[MaxLength(Schema.ABL_MarksAndNumbersMaxLength)]
		[ResourceStringData("20D8A799-B190-4DF2-BF99-F8D83EA11A57", Caption = "Marks")]
		public override ZString ABL_MarksAndNumbers
		{
			get => base.ABL_MarksAndNumbers;
			set => base.ABL_MarksAndNumbers = value;
		}

		[MaxLength(Schema.ABL_RemarksMaxLength)]
		public override ZString ABL_Remarks
		{
			get => base.ABL_Remarks;
			set => base.ABL_Remarks = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.Locations))]
		[MaxLength(Schema.ABL_GoodsLocationMaxLength)]
		[ResourceStringData("JPAsycudaBill.ABL_GoodsLocation", Caption = "Bonded Location")]
		[ResourceStringData("JPAsycudaBill.ABL_GoodsLocation|IsHCH01", Caption = "Into Bonded Warehouse", MediumCaption = "Into Warehouse", ShortCaption = "Into Whs.", MultipleKey = AsycudaManifestHeader.CaptionKeyAir + AsycudaManifestHeader.CaptionKeyImp)]
		public override ZString ABL_GoodsLocation
		{
			get => base.ABL_GoodsLocation;
			set
			{
				var oldValue = base.ABL_GoodsLocation;
				if (oldValue != value)
				{
					base.ABL_GoodsLocation = value;
					if (IsChildMasterBill)
					{
						Header?.ViaLocationCodeInfo.RefreshBinding();
					}
					Header?.MarkAsNeedingValidation();
				}
			}
		}

		[RelatedBusinessObject(nameof(SpecialCargoCode))]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.SpecialCargoCodes))]
		[ResourceStringData("JPAsycudaBill.ABL_SpecialCargoCode", Caption = "Special Cargo Code")]
		public override ZString ABL_SpecialCargoCode
		{
			get => base.ABL_SpecialCargoCode;
			set => base.ABL_SpecialCargoCode = value;
		}

		[ResourceStringData("JPAsycudaBill.SpecialCargoCodeDescription", Caption = "Special Cargo Code Description", MediumCaption = "SPC Description", ShortCaption = "SPC Desc.")]
		public ZString SpecialCargoCodeDescription => SpecialCargoCode?.ZZD_Description ?? ZString.Empty;

		public ZZRefCusCodeListCombined SpecialCargoCode => JPRefCusCodeListTypes.GetSpecialCargoCode(Factory, ABL_SpecialCargoCode);

		[ResourceStringData("JPAsycudaBill.ABL_RL_NKPortOfDischarge", Caption = "Discharge Port")]
		public override ZString ABL_RL_NKPortOfDischarge { get => base.ABL_RL_NKPortOfDischarge; set => base.ABL_RL_NKPortOfDischarge = value; }

		[MaxLength(Schema.ABL_LocationInformationMaxLength)]
		public override ZString ABL_LocationInformation { get => base.ABL_LocationInformation; set => base.ABL_LocationInformation = value; }

		[MaxLength(Schema.ABL_GoodsDescriptionMaxLength)]
		public override ZString ABL_GoodsDescription
		{
			get => base.ABL_GoodsDescription;
			set => base.ABL_GoodsDescription = value;
		}

		[MaxLength(Schema.ABL_BillNumberMaxLength)]
		[ResourceStringData("670DF6B5-8258-48D5-AD67-F826FAF8E5DE", Caption = "Bill Number")]
		[ResourceStringData("JPAsycudaBill.ABL_BillNumber|IsAir", Caption = "HAWB", MultipleKey = AsycudaManifestHeader.CaptionKeyAir + AsycudaManifestHeader.CaptionKeyImp)]
		[ResourceStringData("JPAsycudaBill.ABL_BillNumber|IsAir", Caption = "HAWB", MultipleKey = AsycudaManifestHeader.CaptionKeyAir + AsycudaManifestHeader.CaptionKeyExp)]
		[ResourceStringData("JPAsycudaBill.ABL_BillNumber|IsSea", Caption = "House B/L", MultipleKey = AsycudaManifestHeader.CaptionKeySea + AsycudaManifestHeader.CaptionKeyImp)]
		[ResourceStringData("JPAsycudaBill.ABL_BillNumber|IsSea", Caption = "House B/L", MultipleKey = AsycudaManifestHeader.CaptionKeySea + AsycudaManifestHeader.CaptionKeyExp)]
		public override ZString ABL_BillNumber
		{
			get => base.ABL_BillNumber;
			set => base.ABL_BillNumber = value;
		}

		public override ZString ABL_RL_NKFinalDestination
		{
			get => base.ABL_RL_NKFinalDestination;
			set
			{
				var previousValue = base.ABL_RL_NKFinalDestination;
				if (previousValue != value)
				{
					base.ABL_RL_NKFinalDestination = value;
					if (!IsCopying)
					{
						FinalDestinationIATACode = FinalDestination?.RL_IATA ?? ZString.Empty;
						if (!IsValidationSuspended)
						{
							Validation.ValidateABL_RL_NKFinalDestination();
							RegularBillValidation.ValidateFinalDestinationIATACode();
						}
					}
				}
			}
		}

		[ResourceStringData("JPAsycudaBill.FinalDestinationIATACode", Caption = "IATA Code")]
		public ZString FinalDestinationIATACode
		{
			get => isFinalDestinationIATACodeOverridden ? finalDestinationIATACode : FinalDestination?.RL_IATA ?? finalDestinationIATACode;
			set
			{
				if (value != finalDestinationIATACode)
				{
					SetNonPersistentPropertyValue(FinalDestinationIATACodeInfo, ref finalDestinationIATACode, value);
					if (!IsValidationSuspended)
					{
						RegularBillValidation.ValidateFinalDestinationIATACode();
					}
					if (!finalDestinationIATACode.IsEmpty)
					{
						var refUNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_IATA, value));
						if (refUNLOCO != null)
						{
							ABL_RL_NKFinalDestination = refUNLOCO.RL_Code;
							isFinalDestinationIATACodeOverridden = false;
						}
						else
						{
							isFinalDestinationIATACodeOverridden = true;
						}
					}
				}
			}
		}

		ZString finalDestinationIATACode;
		bool isFinalDestinationIATACodeOverridden;

		public ZPropertyInfo FinalDestinationIATACodeInfo => GetZPropertyInfo(nameof(FinalDestinationIATACode));

		public ZString ShipperAddress
		{
			get
			{
				var addressBuilder = new ZStringBuilder();
				addressBuilder.AppendIfNotEmpty($"{ABL_ShipperStreet1} {ABL_ShipperStreet2}".Trim());
				addressBuilder.AppendIfNotEmpty(ABL_ShipperCity);
				addressBuilder.AppendIfNotEmpty(ABL_ShipperState);
				addressBuilder.AppendIfNotEmpty(ABL_ShipperPostcode);
				addressBuilder.AppendIfNotEmpty(ABL_RN_NKShipperCountry);
				return addressBuilder.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		public ZString ConsigneeAddress
		{
			get
			{
				var addressBuilder = new ZStringBuilder();
				addressBuilder.AppendIfNotEmpty(ABL_ConsigneePostcode);
				addressBuilder.AppendIfNotEmpty(ABL_ConsigneeState);
				addressBuilder.AppendIfNotEmpty(ABL_ConsigneeCity);
				addressBuilder.AppendIfNotEmpty($"{ABL_ConsigneeStreet1} {ABL_ConsigneeStreet2}".Trim());
				return addressBuilder.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		public ZString NotifyPartyAddress
		{
			get
			{
				var addressBuilder = new ZStringBuilder();
				addressBuilder.AppendIfNotEmpty(ABL_NotifyPartyPostcode);
				addressBuilder.AppendIfNotEmpty(ABL_NotifyPartyState);
				addressBuilder.AppendIfNotEmpty(ABL_NotifyPartyCity);
				addressBuilder.AppendIfNotEmpty($"{ABL_NotifyPartyStreet1} {ABL_NotifyPartyStreet2}".Trim());
				return addressBuilder.ToStringWithDelimiterBetweenAppends(",");
			}
		}

		[LightValidationTestExempt]
		public override ZInt ABL_ClusterKey { get => base.ABL_ClusterKey; set => base.ABL_ClusterKey = value; }

		public override ZDecimal ABL_GrossWeight
		{
			get => base.ABL_GrossWeight;
			set
			{
				var oldValue = base.ABL_GrossWeight;
				if (oldValue != value)
				{
					base.ABL_GrossWeight = value;
					if (!IsValidationSuspended)
					{
						RegularBillValidation.ValidateCustomsWeight();
					}
				}
			}
		}

		[ResourceStringData("JPAsycudaBill.ABL_GrossWeightUQ", Caption = "Gross Weight Unit", MediumCaption = "Gross Wgt. Unit", ShortCaption = "UQ")]
		public override ZString ABL_GrossWeightUQ
		{
			get => base.ABL_GrossWeightUQ;
			set
			{
				var oldValue = base.ABL_GrossWeightUQ;
				if (oldValue != value)
				{
					base.ABL_GrossWeightUQ = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateABL_GrossWeight();
						RegularBillValidation.ValidateCustomsWeight();
					}
				}
			}
		}

		[ResourceStringData("JPAsycudaBill.CustomsWeightUQ", Caption = "Customs Weight Unit", MediumCaption = "Cus. Wgt. Unit", ShortCaption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.JPCustomsWeightUnitList))]
		public ZString CustomsWeightUQ => NACCSUnitConverter.ConvertToCustomsWeightUnit(ABL_GrossWeightUQ, ConvertManifestTypeToProcedureCodeForUnitConversion(Header.AMA_ManifestType));

		[ResourceStringData("JPAsycudaBill.CustomsWeight", Caption = "Customs Weight", ShortCaption = "Cus. Wgt.")]
		public ZDecimal CustomsWeight
		{
			get => NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(ABL_GrossWeight, ABL_GrossWeightUQ, ConvertManifestTypeToProcedureCodeForUnitConversion(Header.AMA_ManifestType));
			set
			{
				var grossWeightUQ = ABL_GrossWeightUQ;
				if (!grossWeightUQ.IsEmpty)
				{
					ABL_GrossWeight = Weight.ConvertSafe(value, NACCSUnitConverter.ConvertJPCustomsWeightUnitToCW1WeightUnit(CustomsWeightUQ), grossWeightUQ);
				}
				CustomsWeightInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomsWeightInfo => GetZPropertyInfo(nameof(CustomsWeight));

		[ResourceStringData("JPAsycudaBill.ABL_ConsigneeRegNo", Caption = "Consignee Registration No", ShortCaption = "Reg. No")]
		public override ZString ABL_ConsigneeRegNo
		{
			get => base.ABL_ConsigneeRegNo;
			set
			{
				var oldValue = base.ABL_ConsigneeRegNo;
				if (oldValue != value)
				{
					base.ABL_ConsigneeRegNo = value;

					if (!IsCopying && !IsValidationSuspended && ShouldShowConsigneeRegNoType)
					{
						Validation.ValidateABL_ConsigneeRegNoType();
					}
				}
			}
		}

		public override ZString ABL_MessageStatus
		{
			get => base.ABL_MessageStatus;
			set
			{
				var oldValue = base.ABL_MessageStatus;
				if (value != oldValue)
				{
					base.ABL_MessageStatus = value;

					if (!IsCopying)
					{
						Header.NeedToUpdateMessageStatus = true;
					}
				}
			}
		}

		[ResourceStringData("8E540669-5473-4B09-BA6F-F29EA7B779AD", Caption = "Message Status Description")]
		public ZString MessageStatusDescription => Lookups.MessageStatusList.GetDescriptionFromCode(ABL_MessageStatus);

		[ResourceStringData("JPAsycudaBill.TemporaryLandingStatusDescription", Caption = "Temporary Landing Status Description", ShortCaption = "TL Status Desc.")]
		public ZString TemporaryLandingStatusDescription => Factory.GetCachedValue<TemporaryLandingStatusCodeList>().GetDescriptionFromCode(TemporaryLandingStatus);

		public ZPropertyInfo TemporaryLandingStatusDescriptionInfo => GetZPropertyInfo(nameof(TemporaryLandingStatusDescription));

		[ReadOnly(false)]
		[ResourceStringData("JPAsycudaBill.ABL_ConsigneeRegNoType", Caption = "Code")]
		public override ZString ABL_ConsigneeRegNoType
		{
			get => base.ABL_ConsigneeRegNoType;
			set
			{
				var oldValue = base.ABL_ConsigneeRegNoType;
				if (oldValue != value)
				{
					base.ABL_ConsigneeRegNoType = value;

					if (!IsCopying && ConsigneeUseRealOrg)
					{
						ABL_ConsigneeRegNo = GetRegNoAndTypeForAddress(Consignee, value).RegNo;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CargoTypeList))]
		[ResourceStringData("47F07446-17B1-4613-8FDF-C11219953AF9", Caption = "Cargo Type")]
		public override ZString ABL_CargoType { get => base.ABL_CargoType; set => base.ABL_CargoType = value; }

		[ResourceStringData("697D2931-CC34-4C9E-9E15-661C7442AC67", Caption = "Customs Status")]
		public override ZString ABL_BillStatus
		{
			get => base.ABL_BillStatus;
			set
			{
				var oldValue = base.ABL_BillStatus;
				base.ABL_BillStatus = value;
				if (!IsCopying && oldValue != ABL_BillStatus)
				{
					Header.NeedToUpdateRegistrationStatus = true;
				}
			}
		}

		protected override bool ABL_BillStatus_ReadOnly => true;

		[ResourceStringData("1E2DFAE6-C06F-40FB-A4C8-2C0F0EF1A834", Caption = "Customs Status Desc.")]
		public override ZString ABL_BillStatusDescription => base.ABL_BillStatusDescription;

		[BusinessObjectTestExclude]
		[MaxLength(Schema.ABL_TariffMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.TariffCollection))]
		[ResourceStringData("58860F78-E892-41A3-B513-BFC1D0EAF1FE", Caption = "Statistical/HS Code", ShortCaption = "Stat./HS")]
		public ZString ABL_Tariff
		{
			get => TariffFormatter.DisplayFormat(GetPack()?.PackedItem.API_Tariff ?? ZString.Empty);
			set
			{
				if (ABL_Tariff != value)
				{
					var newTariff = TariffFormatter.Format(value).Left(Schema.ABL_TariffMaxLength);
					Pack.PackedItem.API_Tariff = newTariff;
					if (!IsValidationSuspended && Validation is AsycudaBillValidationForRegularBill validationForRegularBill)
					{
						validationForRegularBill.ValidateABL_Tariff();
					}
					ABL_TariffInfo.RefreshBinding();
				}
			}
		}

		[ResourceStringData("JPAsycudaBill.ABL_ShipperRegNo", Caption = "Shipper Registration No", ShortCaption = "Reg. No")]
		public override ZString ABL_ShipperRegNo
		{
			get => base.ABL_ShipperRegNo;
			set
			{
				var oldValue = base.ABL_ShipperRegNo;
				if (oldValue != value)
				{
					base.ABL_ShipperRegNo = value;

					if (!IsCopying && !IsValidationSuspended && ShouldShowShipperRegNoType)
					{
						Validation.ValidateABL_ShipperRegNoType();
					}
				}
			}
		}

		[ReadOnly(false)]
		[ResourceStringData("JPAsycudaBill.ABL_ShipperRegNoType", Caption = "Code")]
		public override ZString ABL_ShipperRegNoType
		{
			get => base.ABL_ShipperRegNoType;
			set
			{
				var oldValue = base.ABL_ShipperRegNoType;
				if (oldValue != value)
				{
					base.ABL_ShipperRegNoType = value;

					if (!IsCopying && ShipperUseRealOrg)
					{
						ABL_ShipperRegNo = GetRegNoAndTypeForAddress(Shipper, value).RegNo;
					}
				}
			}
		}

		[ResourceStringData("JPAsycudaBill.ABL_NotifyPartyRegNo", Caption = "Notify Party Registration No", ShortCaption = "Reg. No")]
		public override ZString ABL_NotifyPartyRegNo
		{
			get => base.ABL_NotifyPartyRegNo;
			set
			{
				var oldValue = base.ABL_NotifyPartyRegNo;
				if (oldValue != value)
				{
					base.ABL_NotifyPartyRegNo = value;

					if (!IsCopying && !IsValidationSuspended && ShouldShowNotifyPartyRegNoType)
					{
						Validation.ValidateABL_NotifyPartyRegNoType();
					}
				}
			}
		}

		[ReadOnly(false)]
		[ResourceStringData("JPAsycudaBill.ABL_NotifyPartyRegNoType", Caption = "Code")]
		public override ZString ABL_NotifyPartyRegNoType
		{
			get => base.ABL_NotifyPartyRegNoType;
			set
			{
				var oldValue = base.ABL_NotifyPartyRegNoType;
				if (oldValue != value)
				{
					base.ABL_NotifyPartyRegNoType = value;

					if (!IsCopying && NotifyPartyUseRealOrg)
					{
						ABL_NotifyPartyRegNo = GetRegNoAndTypeForAddress(NotifyParty, value).RegNo;
					}
				}
			}
		}

		public ZPropertyInfo ABL_TariffInfo => GetZPropertyInfo(nameof(ABL_Tariff));

		public ZBool ShouldShowRepresentativeHSCode => Factory.GetValue(ref shouldShowRepresentativeHSCodeCached, () =>
		{
			var result = false;
			var tariffCode = UniversalTariff?.ZZ1_TariffCode ?? ZString.Empty;
			var tariffCodeLength = tariffCode.Length;
			if (tariffCodeLength != 0 && tariffCodeLength != 4 && tariffCodeLength != 6)
			{
				result = true;
			}
			return result;
		});
		CachedProperty<ZBool> shouldShowRepresentativeHSCodeCached;

		[ResourceStringData("JPAsycudaBill.RepresentativeHSCode", Caption = "Representative HS Code", ShortCaption = "Rep. HS ", FullDescription = "Representative HS Code (HSN) to be sent in NVC messages")]
		public ZString ABL_RepresentativeHSCode => Factory.GetValue(ref representativeHSCodeCached, () =>
		{
			var result = ZString.Empty;
			if (ShouldShowRepresentativeHSCode)
			{
				var tariffCode = UniversalTariff.ZZ1_TariffCode;
				tariffCode = tariffCode.Length > 6 ? tariffCode.Left(6) : tariffCode.Left(4);
				result = TariffFormatter.DisplayFormat(tariffCode);
			}
			return result;
		});
		CachedProperty<ZString> representativeHSCodeCached;

		public ZPropertyInfo RepresentativeHSCodeInfo => GetZPropertyInfo(nameof(ABL_RepresentativeHSCode));

		public ZString TariffType => IsImport ? Universal.Constants.TariffTypes.Import : Universal.Constants.TariffTypes.Export;

		[ResourceStringData("531D024A-8AE5-4DF6-B876-84E33DFCE069", Caption = "Statistical/HS Description", ShortCaption = "Stat./HS Desc.")]
		public ZString TariffDescription => UniversalTariff?.ZZ1_Description ?? ZString.Empty;

		public ZPropertyInfo TariffDescriptionInfo => GetZPropertyInfo(nameof(TariffDescription));

		public TariffView UniversalTariff
		{
			get
			{
				TariffView universalTariff = null;
				if (!ABL_Tariff.IsEmpty)
				{
					var tariff = ((ITariffFormatProvider)this).TariffFormatter.Format(ABL_Tariff);
					universalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Japan, TariffType, tariff, ZDateTime.Today);
				}
				return universalTariff;
			}
		}

		[ResourceStringData("7CBE8C08-ED94-487B-A1A6-1310CFA6060E", Caption = "Net Weight")]
		public override ZDecimal ABL_NetWeight
		{
			get => base.ABL_NetWeight;
			set
			{
				var oldValue = base.ABL_NetWeight;
				if (oldValue != value)
				{
					base.ABL_NetWeight = value;
					if (!IsValidationSuspended && Validation is AsycudaBillValidationForRegularBill regularBillValidation)
					{
						regularBillValidation.ValidateCustomsNetWeight();
					}
				}
			}
		}

		[ResourceStringData("7603ECD7-914E-480F-9584-B387FBDF179E", Caption = "Net Weight UQ", ShortCaption = "UQ")]
		public override ZString ABL_NetWeightUQ
		{
			get => base.ABL_NetWeightUQ;
			set
			{
				var oldValue = base.ABL_NetWeightUQ;
				if (oldValue != value)
				{
					base.ABL_NetWeightUQ = value;
					if (!IsValidationSuspended)
					{
						RegularBillValidation.ValidateCustomsNetWeight();
					}
				}
			}
		}

		[ResourceStringData("JPAsycudaBill.CustomsNetWeightUQ", Caption = "Customs Net Weight Unit", MediumCaption = "Cus. Net Wgt. Unit", ShortCaption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.JPCustomsWeightUnitList))]
		public ZString CustomsNetWeightUQ => NACCSUnitConverter.ConvertToCustomsWeightUnit(ABL_NetWeightUQ, ConvertManifestTypeToProcedureCodeForUnitConversion(Header.AMA_ManifestType));

		[ResourceStringData("JPAsycudaBill.CustomsNetWeight", Caption = "Customs Net Weight", ShortCaption = "Cus. Net Wgt.")]
		public ZDecimal CustomsNetWeight
		{
			get => NACCSUnitConverter.CalculateWeightInCustomsWeightUnit(ABL_NetWeight, ABL_NetWeightUQ, ConvertManifestTypeToProcedureCodeForUnitConversion(Header.AMA_ManifestType));
			set
			{
				var netWeightUQ = ABL_NetWeightUQ;
				if (!netWeightUQ.IsEmpty)
				{
					ABL_NetWeight = Weight.ConvertSafe(value, NACCSUnitConverter.ConvertJPCustomsWeightUnitToCW1WeightUnit(CustomsNetWeightUQ), netWeightUQ);
				}
				CustomsNetWeightInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomsNetWeightInfo => GetZPropertyInfo(nameof(CustomsNetWeight));

		public override ZDecimal ABL_Volume
		{
			get => base.ABL_Volume;
			set
			{
				var oldValue = base.ABL_Volume;
				if (oldValue != value)
				{
					base.ABL_Volume = value;
					if (!IsValidationSuspended)
					{
						RegularBillValidation.ValidateCustomsVolume();
					}
				}
			}
		}

		public override ZString ABL_VolumeUQ
		{
			get => base.ABL_VolumeUQ;
			set
			{
				var oldValue = base.ABL_VolumeUQ;
				if (oldValue != value)
				{
					base.ABL_VolumeUQ = value;
					if (!IsValidationSuspended)
					{
						RegularBillValidation.ValidateCustomsVolume();
					}
				}
			}
		}

		[ResourceStringData("JPAsycudaBill.CustomsVolumeUQ", Caption = "Customs Volume Unit", MediumCaption = "Cus. Volume Unit", ShortCaption = "UQ")]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.JPCustomsVolumeUnitList))]
		public ZString CustomsVolumeUQ => NACCSUnitConverter.ConvertToCustomsVolumeUnit(ABL_VolumeUQ);

		[ResourceStringData("JPAsycudaBill.CustomsVolume", Caption = "Customs Volume", ShortCaption = "Cus. Volume")]
		public ZDecimal CustomsVolume
		{
			get => NACCSUnitConverter.CalculateVolumeInCustomsVolumeUnit(ABL_Volume, ABL_VolumeUQ, ConvertManifestTypeToProcedureCodeForUnitConversion(Header.AMA_ManifestType));
			set
			{
				var volumeUQ = ABL_VolumeUQ;
				if (!volumeUQ.IsEmpty)
				{
					ABL_Volume = Volume.ConvertSafe(value, NACCSUnitConverter.ConvertJPCustomsVolumeUnitToCW1VolumeUnit(CustomsVolumeUQ), volumeUQ);
				}
				CustomsVolumeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CustomsVolumeInfo => GetZPropertyInfo(nameof(CustomsVolume));

		[DecimalPlaces(nameof(FreightValueDecimalPlaces))]
		public override ZDecimal ABL_FreightValue { get => base.ABL_FreightValue; set => base.ABL_FreightValue = value; }

		[DecimalPlaces(nameof(TransportValueDecimalPlaces))]
		public override ZDecimal ABL_TransportValue { get => base.ABL_TransportValue; set => base.ABL_TransportValue = value; }

		int FreightValueDecimalPlaces => ABL_RX_NKFreightValueCurrency == CurrencyCodes.Japan ? 0 : 2;

		int TransportValueDecimalPlaces => ABL_RX_NKTransportValueCurrency == CurrencyCodes.Japan ? 0 : 2;

		public new ZBool CanConvertConsigneeToOrganization => ABL_OA_Consignee.IsEmpty && (!ConsigneeABLAddress.AreEmpty() || (ShouldShowConsigneeRegNoType && (!ABL_ConsigneeRegNo.IsEmpty || !ABL_ConsigneeRegNoType.IsEmpty)));

		public ZBool ShouldShowConsigneeRegNoType => IsHCH || IsNVC;

		public override ZBool ConsigneeRegNoReadOnly => ShouldShowConsigneeRegNoType ? ConsigneeUseRealOrg && HasConsigneeRegNoCode : base.ConsigneeRegNoReadOnly;

		public override ZString[] ConsigneeRegNoTypes()
		{
			return ShouldShowConsigneeRegNoType ? [.. CustomsRegNumTypeValidation.GetListByImportAndPartyType(IsImport, isShipper: false, isConsignee: true)] : base.ConsigneeRegNoTypes();
		}

		public new ZBool CanConvertShipperToOrganization => ABL_OA_Shipper.IsEmpty && (!ShipperABLAddress.AreEmpty() || (ShouldShowShipperRegNoType && (!ABL_ShipperRegNo.IsEmpty || !ABL_ShipperRegNoType.IsEmpty)));

		public ZBool ShouldShowShipperRegNoType => IsHCH || IsNVC;

		public override ZBool ShipperRegNoReadOnly => ShouldShowShipperRegNoType ? ShipperUseRealOrg && HasShipperRegNoCode : base.ShipperRegNoReadOnly;

		public override ZString[] ShipperRegNoTypes()
		{
			if (ShouldShowShipperRegNoType)
			{
				return IsNVC
					? [.. CustomsRegNumTypeValidation.NvcShipperList]
					: [.. CustomsRegNumTypeValidation.GetListByImportAndPartyType(IsImport, isShipper: true, isConsignee: false)];
			}
			return base.ShipperRegNoTypes();
		}

		public new ZBool CanConvertNotifyPartyToOrganization => !IsHCH && ABL_OA_NotifyParty.IsEmpty && (!NotifyPartyABLAddress.AreEmpty() || (ShouldShowNotifyPartyRegNoType && (!ABL_NotifyPartyRegNo.IsEmpty || !ABL_NotifyPartyRegNoType.IsEmpty)));

		public ZBool ShouldShowNotifyPartyRegNoType => IsNVC;

		public override ZBool NotifyPartyRegNoReadOnly => ShouldShowNotifyPartyRegNoType ? NotifyPartyUseRealOrg && HasNotifyPartyRegNoCode : base.NotifyPartyRegNoReadOnly;

		public override ZString[] NotifyPartyRegNoTypes()
		{
			return ShouldShowNotifyPartyRegNoType ? [.. CustomsRegNumTypeValidation.GetListByImportAndPartyType(IsImport, isShipper: false, isConsignee: false)] : base.NotifyPartyRegNoTypes();
		}

		protected override (ZString RegNumber, ZString RegNumberType) GetPartyOrgAddressRegNoAndType(OrgAddress org, ManifestBase.AsycudaBillAddress.AddressType addressType, ZString[] regNoTypes)
		{
			switch (addressType)
			{
				case ManifestBase.AsycudaBillAddress.AddressType.Consignee:
					return ShouldShowConsigneeRegNoType ? GetRegNoAndTypeForAddress(org, regNoTypes) : GetRegNoAndTypeForParty(org, regNoTypes);
				case ManifestBase.AsycudaBillAddress.AddressType.Shipper:
					return ShouldShowShipperRegNoType ? GetRegNoAndTypeForAddress(org, regNoTypes) : GetRegNoAndTypeForParty(org, regNoTypes);
				case ManifestBase.AsycudaBillAddress.AddressType.NotifyParty:
					return ShouldShowNotifyPartyRegNoType ? GetRegNoAndTypeForAddress(org, regNoTypes) : GetRegNoAndTypeForParty(org, regNoTypes);

				default:
					return GetRegNoAndTypeForParty(org, regNoTypes);
			}
		}

		#region Temporary Landing Registration Number

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		[ResourceStringData("773ED308-B2C0-4606-9BA6-4763C7AC613C", Caption = "Temporary Landing Number", ShortCaption = "TL No.")]
		public ZString TemporaryLandingNumber
		{
			get => TemporaryLandingCusEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (TemporaryLandingCusEntryNumber != null)
				{
					TemporaryLandingCusEntryNumber.CE_EntryNum = value;
				}
				else
				{
					CreateNewTemporaryLandingRegistrationNumber(value);
				}

				TemporaryLandingNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TemporaryLandingNumberInfo => GetZPropertyInfo(nameof(TemporaryLandingNumber));

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		[ResourceStringData("ECFB7F50-6A2A-4AC1-86D4-F449E84D8A2E", Caption = "Temporary Landing Status", ShortCaption = "TL Status")]
		public ZString TemporaryLandingStatus
		{
			get => TemporaryLandingCusEntryNumber?.CE_EntryStatus ?? ZString.Empty;
			set
			{
				if (TemporaryLandingCusEntryNumber != null)
				{
					TemporaryLandingCusEntryNumber.CE_EntryStatus = value.Left(Customs.Common.CusEntryNumber.Schema.CE_EntryStatusMaxLength);
				}

				TemporaryLandingStatusInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo TemporaryLandingStatusInfo => GetZPropertyInfo(nameof(TemporaryLandingStatus));

		CusEntryNumber TemporaryLandingCusEntryNumber
		{
			get
			{
				if (temporaryLandingCusEntryNumber == null || temporaryLandingCusEntryNumber.IsDeleted || temporaryLandingCusEntryNumber.CE_EntryType != CusEntryNumberTypes.JP.TemporaryLandingRegistrationNumber)
				{
					temporaryLandingCusEntryNumber = Customs.Common.CusEntryNumber.Load(this, CusEntryNumberTypes.JP.TemporaryLandingRegistrationNumber, CountryCode);

					if (temporaryLandingCusEntryNumber != null)
					{
						RegisterEditableChildObject(temporaryLandingCusEntryNumber);
					}
				}

				return temporaryLandingCusEntryNumber;
			}
		}
		CusEntryNumber temporaryLandingCusEntryNumber;

		void CreateNewTemporaryLandingRegistrationNumber(string entryNumber, bool isSystemGenerated = false)
		{
			Customs.Common.CusEntryNumber.New<CusEntryNumber>(this, CusEntryNumberTypes.JP.TemporaryLandingRegistrationNumber, CountryCode, entryNumber, isSystemGenerated);
		}

		#endregion

		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;

		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);

		public AsycudaBillValidationForRegularBill RegularBillValidation => (AsycudaBillValidationForRegularBill)base.Validation;

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);

		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);

		public override bool IsImport => Header?.IsImport ?? false;

		public override bool IsExport => Header?.IsExport ?? false;

		internal AsycudaPack GetPack() => Packs.Cast<AsycudaPack>().FirstOrDefault();

		AsycudaPack Pack
		{
			get
			{
				if (!IsDeleted && (pack == null || pack.IsDeleted))
				{
					if (pack != null)
					{
						UnRegisterEditableChildObject(pack);
						pack = null;
					}
					if (!IsDeleting)
					{
						LoadPack();
					}
				}
				return pack;
			}
		}
		AsycudaPack pack;

		void LoadPack()
		{
			pack = GetPack();
			pack = pack ??= Packs.AddNew();
			RegisterEditableChildObject(pack);
		}

		#region TemporaryLanding
		[MaxLength(Schema.TemporaryLandingReasonMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ReasonList))]
		[ResourceStringData("JPAsycudaBill.TemporaryLandingReason", Caption = "Reason")]
		public ZString TemporaryLandingReason
		{
			get => TemporaryLandingInfo.CSI_Code;
			set
			{
				var oldValue = TemporaryLandingInfo.CSI_Code;
				if (oldValue != value)
				{
					CheckMaximumLength(TemporaryLandingReasonInfo, value);
					TemporaryLandingInfo.CSI_Code = value;
					TemporaryLandingReasonInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TemporaryLandingReasonInfo => (temporaryLandingInfo?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(TemporaryLandingReason)) : GetWrappedZPropertyInfo(nameof(TemporaryLandingReason), x => temporaryLandingInfo.CSI_CodeInfo);

		[ResourceStringData("JPAsycudaBill.TemporaryLandingStartDate", Caption = "Start Date")]
		public ZDate TemporaryLandingStartDate
		{
			get => TemporaryLandingInfo.CSI_DateOfIssue.Date;
			set
			{
				var oldValue = TemporaryLandingInfo.CSI_DateOfIssue;
				if (oldValue != value)
				{
					TemporaryLandingInfo.CSI_DateOfIssue = value;
					DefaultTemporaryLandingPeriodDays(value, TemporaryLandingEndDate);
					TemporaryLandingStartDateInfo.RefreshBinding();
				}
			}
		}
		public ZPropertyInfo TemporaryLandingStartDateInfo => (temporaryLandingInfo?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(TemporaryLandingStartDate)) : GetWrappedZPropertyInfo(nameof(TemporaryLandingStartDate), x => temporaryLandingInfo.CSI_DateOfIssueInfo);

		[ResourceStringData("JPAsycudaBill.TemporaryLandingEndDate", Caption = "End Date")]
		public ZDate TemporaryLandingEndDate
		{
			get => TemporaryLandingInfo.CSI_DateOfExpiry.Date;
			set
			{
				var oldValue = TemporaryLandingInfo.CSI_DateOfExpiry;
				if (oldValue != value)
				{
					TemporaryLandingInfo.CSI_DateOfExpiry = value;
					DefaultTemporaryLandingPeriodDays(TemporaryLandingStartDate, value);
					TemporaryLandingEndDateInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TemporaryLandingEndDateInfo => (temporaryLandingInfo?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(TemporaryLandingEndDate)) : GetWrappedZPropertyInfo(nameof(TemporaryLandingEndDate), x => temporaryLandingInfo.CSI_DateOfExpiryInfo);

		[ResourceStringData("JPAsycudaBill.TemporaryLandingPeriodDays", Caption = "Period (Days)")]
		public ZInt TemporaryLandingPeriodDays
		{
			get => TemporaryLandingInfo.CSI_ItemNumber;
			set
			{
				var oldValue = TemporaryLandingInfo.CSI_ItemNumber;
				if (oldValue != value)
				{
					TemporaryLandingInfo.CSI_ItemNumber = value;
					TemporaryLandingPeriodDaysInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TemporaryLandingPeriodDaysInfo => (temporaryLandingInfo?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(TemporaryLandingPeriodDays)) : GetWrappedZPropertyInfo(nameof(TemporaryLandingPeriodDays), x => temporaryLandingInfo.CSI_ItemNumberInfo);

		void DefaultTemporaryLandingPeriodDays(ZDate startDate, ZDate endDate)
		{
			if (TemporaryLandingPeriodDays.IsEmpty && endDate.IsValid && endDate.IsValid && endDate >= startDate)
			{
				var calcResult = (int)(endDate - startDate).TotalDays + 1;
				if (calcResult < 100)
				{
					TemporaryLandingPeriodDays = calcResult;
				}
			}
		}

		[MaxLength(Schema.TemporaryLandingBondedTransportCodeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.BondedTransportList))]
		[ResourceStringData("JPAsycudaBill.TemporaryLandingBondedTransportCode", Caption = "Bonded Transport Code")]
		public ZString TemporaryLandingBondedTransportCode
		{
			get => TemporaryLandingInfo.CSI_ReferenceNumber;
			set
			{
				var oldValue = TemporaryLandingInfo.CSI_ReferenceNumber;
				if (oldValue != value)
				{
					CheckMaximumLength(TemporaryLandingBondedTransportCodeInfo, value);
					TemporaryLandingInfo.CSI_ReferenceNumber = value;
					TemporaryLandingBondedTransportCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo TemporaryLandingBondedTransportCodeInfo => (temporaryLandingInfo?.IsDeleted ?? true) ? GetZPropertyInfo(nameof(TemporaryLandingBondedTransportCode)) : GetWrappedZPropertyInfo(nameof(TemporaryLandingBondedTransportCode), x => temporaryLandingInfo.CSI_ReferenceNumberInfo);

		TemporaryLandingInfoCollection GetTemporaryLandingInfoCollection()
		{
			var result = new TemporaryLandingInfoCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		[ChildEditable(true)]
		public TemporaryLandingInfoCollection TemporaryLandingInfoCollection => temporaryLandingInfoCollection ??= GetTemporaryLandingInfoCollection();
		TemporaryLandingInfoCollection temporaryLandingInfoCollection;

		public TemporaryLandingInfo TemporaryLandingInfo
		{
			get
			{
				if (temporaryLandingInfo?.IsDeleted ?? true)
				{
					temporaryLandingInfo = TemporaryLandingInfoCollection.FirstOrDefault() ?? TemporaryLandingInfoCollection.AddNew();
				}
				return temporaryLandingInfo;
			}
		}
		TemporaryLandingInfo temporaryLandingInfo;
		#endregion

		#region ICusSupportingInfoTypeSupporter
		IDictionary<ZString, Type> Integration.Customs.ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes() => new Dictionary<ZString, Type>
		{
			{ CusSupportingInfoTypeList.Codes.ApprovalCertificate, typeof(TemporaryLandingInfo) },
		};

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}
		#endregion

		#region OtherLawsandRegulations
		[ChildEditable(true)]
		public CusOtherLawReferenceCollection<CusOtherLawReference> OtherLawsandRegulations
		{
			get
			{
				if (otherLawsandRegulations == null)
				{
					otherLawsandRegulations = new CusOtherLawReferenceCollection<CusOtherLawReference>(this);
					otherLawsandRegulations.Load();
					RegisterEditableChildObject(otherLawsandRegulations);
				}
				return otherLawsandRegulations;
			}
		}

		public ITariffFormatter TariffFormatter => new Common.TariffFormatter();

		CusOtherLawReferenceCollection<CusOtherLawReference> otherLawsandRegulations;
		#endregion

		internal bool IsTemporaryLanding => IsTemporaryLandingTransportation || !(TemporaryLandingReason.IsEmpty && TemporaryLandingPeriodDays.IsEmpty);

		internal bool IsTemporaryLandingTransportation => !(TemporaryLandingStartDate.IsEmpty && TemporaryLandingEndDate.IsEmpty && TemporaryLandingBondedTransportCode.IsEmpty && ABL_GoodsLocation.IsEmpty) || OtherLawsandRegulations.Any();

		public bool IsHDF => Factory.GetValue(ref isHDF, () => Header?.IsHDF ?? false);
		CachedProperty<bool> isHDF;

		public bool IsHCH => Factory.GetValue(ref isHCH, () => Header?.IsHCH ?? false);
		CachedProperty<bool> isHCH;

		public bool IsNVC => Factory.GetValue(ref isNVC, () => Header?.IsNVC ?? false);
		CachedProperty<bool> isNVC;

		ZBool ICusOtherLawReferenceParent.IsOtherLawReferenceRequired => IsNVC;

		ZString ICusOtherLawReferenceParent.MessageType => ZString.Empty;

		public IEnumerable<ZString> GetTariffAttributesByKey(ZString key) => Enumerable.Empty<ZString>();

		string ConvertManifestTypeToProcedureCodeForUnitConversion(string manifestType)
		{
			return manifestType switch
			{
				JPManifestTypeCodeList.Codes.HCH => JPProcedureCodeList.Codes.HCH01,
				JPManifestTypeCodeList.Codes.HDF => JPProcedureCodeList.Codes.HDF01,
				JPManifestTypeCodeList.Codes.NVC => JPProcedureCodeList.Codes.NVC01,
				_ => string.Empty,
			};
		}
	}
}
