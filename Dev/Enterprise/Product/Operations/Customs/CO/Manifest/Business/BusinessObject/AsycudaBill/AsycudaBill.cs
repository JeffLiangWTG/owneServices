using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using AddressType = Enterprise.ZArchitecture.Business.AddressType;

namespace Enterprise.Customs.CO.Manifest.Business
{
	public class AsycudaBill : ASYCUDA.Business.AsycudaBill, Integration.Customs.ASYCUDA.COManifest.IAsycudaBill
	{
		public AsycudaBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : ASYCUDA.Business.AsycudaBill.Schema
		{
			public const string TravelDocumentType = "TravelDocumentType";
			public const int TravelDocumentTypeMaxLength = 2;
			public const string CargoDisposition = "CargoDisposition";
			public const int CargoDispositionMaxLength = 3;
			public const string Multimodal = "Multimodal";
			public const string CarriersLiability = "CarriersLiability";
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;
		protected override ZString GetCountryCode() => Core.Constants.CountryCodes.Colombia;
		public new AsycudaBillLookups Lookups => (AsycudaBillLookups)base.Lookups;
		protected override ManifestBase.AsycudaBillLookups GetNewLookups() => new AsycudaBillLookups(this);
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForRegularBill() => new AsycudaBillValidationForRegularBill(this);
		AsycudaBillValidationForRegularBill RegularBillValidation => Validation as AsycudaBillValidationForRegularBill;
		protected override ManifestBase.AsycudaBillValidation GetNewValidationForMasterChild() => new AsycudaBillValidationForMasterChild(this);
		public new ZString GetCountryCodeForCustomsRegNo(OrgAddress address) => base.GetCountryCodeForCustomsRegNo(address);
		protected override Type GetPackTypeCore() => typeof(AsycudaPack);
		public new AsycudaPackCollection Packs => (AsycudaPackCollection)base.Packs;
		protected override ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => new AsycudaPackCollection(this);

		#region TravelDocumentType

		[MaxLength(Schema.TravelDocumentTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.TravelDocumentTypes))]
		[ResourceStringData("AsycudaBill.TravelDocumentType", Caption = "Travel Document Type", ShortCaption = "Travel Doc. Type")]
		public ZString TravelDocumentType
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.TravelDocumentType);
			set
			{
				var oldValue = TravelDocumentType;
				CheckMaximumLength(TravelDocumentTypeInfo, value);
				this.SetSystemDefinedValue(Schema.TravelDocumentType, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateTravelDocumentType();
				}
				TravelDocumentTypeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TravelDocumentTypeInfo => GetZPropertyInfo(Schema.TravelDocumentType);

		#endregion

		#region CargoDisposition

		[MaxLength(Schema.CargoDispositionMaxLength)]
		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.CargoDispositionList))]
		[ResourceStringData("AsycudaBill.CargoDisposition", Caption = "Cargo Disposition")]
		public ZString CargoDisposition
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.CargoDisposition);
			set
			{
				var oldValue = CargoDisposition;
				CheckMaximumLength(CargoDispositionInfo, value);
				this.SetSystemDefinedValue(Schema.CargoDisposition, value);
				if (!IsValidationSuspended)
				{
					RegularBillValidation.ValidateCargoDisposition();
				}
				CargoDispositionInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CargoDispositionInfo => GetZPropertyInfo(Schema.CargoDisposition);

		#endregion

		#region Shipper

		public override ZString[] ShipperRegNoTypes() => RegNoTypes(ABL_RN_NKShipperCountry);

		#endregion

		#region Consignee

		public override ZString[] ConsigneeRegNoTypes() => RegNoTypes(ABL_RN_NKConsigneeCountry);

		#endregion

		ZString[] RegNoTypes(ZString countryCode)
		{
			var regNoTypes = new List<ZString> { ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, ColombiaOrgCusCodeInfo.OrgCusCodes.CID, OrgCusCode.CodeTypes.PassportID };
			if (countryCode != Core.Constants.CountryCodes.Colombia)
			{
				regNoTypes.Add(ColombiaOrgCusCodeInfo.OrgCusCodes.FID);
			}
			return regNoTypes.ToArray();
		}

		#region Multimodal

		[ResourceStringData("AsycudaBill.Multimodal", Caption = "Multimodal")]
		public ZBool Multimodal
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.Multimodal);
			set
			{
				var oldValue = Multimodal;
				this.SetSystemDefinedValue(Schema.Multimodal, value);
				MultimodalInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo MultimodalInfo => GetZPropertyInfo(Schema.Multimodal);

		#endregion

		#region CarriersLiability

		[ResourceStringData("AsycudaBill.CarriersLiability", Caption = "Carrier's Liability")]
		public ZBool CarriersLiability
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.CarriersLiability);
			set
			{
				var oldValue = CarriersLiability;
				this.SetSystemDefinedValue(Schema.CarriersLiability, value);
				CarriersLiabilityInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CarriersLiabilityInfo => GetZPropertyInfo(Schema.CarriersLiability);

		#endregion

		#region GoodsLocation
		protected override ZAddress GetNewABL_OA_GoodsLocation_ZAddress()
		{
			var result = base.GetNewABL_OA_GoodsLocation_ZAddress();
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}
		#endregion

		[ResourceStringData("AsycudaBill.ABL_FreightValue", Caption = "Freight Value")]
		public override ZDecimal ABL_FreightValue { get => base.ABL_FreightValue; set => base.ABL_FreightValue = value; }

		[ResourceStringData("AsycudaBill.ABL_GoodsValue", Caption = "Goods Value FOB")]
		public override ZDecimal ABL_GoodsValue { get => base.ABL_GoodsValue; set => base.ABL_GoodsValue = value; }

		ZDecimal ConvertUsingCustomsRate(ZDateTime dateForRate, ZDecimal amount, ZString originalCurrencyCode, ZString destinationCurrencyCode, GlbCompany company)
		{
			var result = ZDecimal.Zero;
			if (originalCurrencyCode == destinationCurrencyCode)
			{
				result = amount;
			}
			else if (dateForRate.IsValid)
			{
				var originalCurrency = RefCurrency.LoadFromCurrencyCode(Factory, originalCurrencyCode);
				if (originalCurrency != null)
				{
					var destinationCurrency = RefCurrency.LoadFromCurrencyCode(Factory, destinationCurrencyCode);
					if (destinationCurrency != null && company != null)
					{
						var originalAmount = new Money(amount, originalCurrency);
						var cC = new RefCurrencyCurrencyConverter(company, Factory, dateForRate, ZArchitecture.Core.ExchangeRateType.Customs, 0);
						result = cC.ConvertRounded(originalAmount, destinationCurrency).Amount;
					}
				}
			}
			return result;
		}

		public ZDecimal GoodsValuesWithUSD => Factory.GetValue(ref goodsValuesWithUSDCached, () => ConvertUsingCustomsRate(Header.AMA_MasterBillIssueDate, ABL_GoodsValue, ABL_RX_NKGoodsValueCurrency, Core.Constants.CurrencyCodes.UnitedStates, Header.Branch?.Company));
		CachedProperty<ZDecimal> goodsValuesWithUSDCached;

		public ZDecimal FreightValuesWithUSD => Factory.GetValue(ref freightValuesWithUSDCached, () => ConvertUsingCustomsRate(Header.AMA_MasterBillIssueDate, ABL_FreightValue, ABL_RX_NKFreightValueCurrency, Core.Constants.CurrencyCodes.UnitedStates, Header.Branch?.Company));
		CachedProperty<ZDecimal> freightValuesWithUSDCached;

		[ResourceStringData("AsycudaBill.ABL_BillIssueDate", Caption = "Issue Date")]
		public override ZDate ABL_BillIssueDate { get => base.ABL_BillIssueDate; set => base.ABL_BillIssueDate = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaBillLookups.ContainerModeList))]
		[ResourceStringData("AsycudaBill.ABL_ContainerMode", Caption = "Container Mode")]
		public override ZString ABL_ContainerMode { get => base.ABL_ContainerMode; set => base.ABL_ContainerMode = value; }

		protected override bool IsManifestUQNeedToConvertCore => false;

		public override bool CanDelete => true;

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			MultilingualString result = (NoResString)string.Empty;

			return Header.AMA_MessageStatus == MessageStatusCodeList.Codes.Accepted || Header.AMA_MessageStatus == MessageStatusCodeList.Codes.Sent || Header.AMA_MessageStatus == MessageStatusCodeList.Codes.Cancel
				? ResString.GetMultilingualString("FD1ABFA4-82AD-4E67-97FF-C6EB716A6A2D", "This Bill is already sent to Customs.")
				: result;
		}

		protected override ZBool ShouldSynchronisePaymentType() => ZBool.True;
	}
}
