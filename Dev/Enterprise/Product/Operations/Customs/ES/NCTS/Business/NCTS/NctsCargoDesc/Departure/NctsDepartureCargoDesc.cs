using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using static Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.ES.NCTS.Business
{
	[SystemDefinedValues]
	public class NctsDepartureCargoDesc : EU.NCTS.Business.NctsDepartureCargoDesc
		, Integration.Customs.ES.IDepartureCargoDesc
	{
		public NctsDepartureCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EU.NCTS.Business.NctsCommonCargoDesc.Schema
		{
			public const int ExciseCodeMaxLength = 3;
			public const int PVPCurrencyMaxLength = 3;
		}

		#region GenAddOn
		public static class GenAddOnColumnConstants
		{
			public const string ExciseCode = "ES_NCTS_ExciseCode";
			public const string PVPValue = "ES_NCTS_PVPValue";
			public const string PVPCurrency = "ES_NCTS_PVPCurrency";
		}
		#endregion

		public new NctsHeader Header => (NctsHeader)base.Header;

		public override ZString BY_HarmonisedTariff
		{
			get => base.BY_HarmonisedTariff;
			set
			{
				base.BY_HarmonisedTariff = value;
				if (!IsCopying)
				{
					if (BY_HarmonisedTariff.StartsWith("87", StringComparison.Ordinal) || BY_HarmonisedTariff.StartsWith("84", StringComparison.Ordinal))
					{
						IsVehicles = true;
					}
					DefaultExciseCodeIfNeeded();
				}
			}
		}

		void DefaultExciseCodeIfNeeded()
		{
			var list = ESDepartureCargoDescLookups.ExciseCodeList;
			if (list.Count == 1)
			{
				ExciseCode = list[0].Code;
			}
			else if (!list.ContainsCode(ExciseCode))
			{
				ExciseCode = ZString.Empty;
			}
		}

		void ModifyReleaseStatusInPredeclarationWhenParentIsBill()
		{
			if (BY_ParentTableCode == ZArchitecture.Schema.CusInBondBillSchema.Constants.Prefix)
			{
				Header?.ModifyReleaseStatusInPredeclaration();
			}
		}

		public override ZShort BY_LineNo
		{
			get => base.BY_LineNo;
			set
			{
				var oldValue = base.BY_LineNo;
				base.BY_LineNo = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZInt BY_DeclarationGoodsItemNumber
		{
			get => base.BY_DeclarationGoodsItemNumber;
			set
			{
				var oldValue = base.BY_DeclarationGoodsItemNumber;
				base.BY_DeclarationGoodsItemNumber = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZString BY_Type
		{
			get => base.BY_Type;
			set
			{
				var oldValue = base.BY_Type;
				base.BY_Type = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZString BY_RN_NKCountryOfOrigin
		{
			get => base.BY_RN_NKCountryOfOrigin;
			set
			{
				var oldValue = base.BY_RN_NKCountryOfOrigin;
				base.BY_RN_NKCountryOfOrigin = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZString BY_RN_NKCountryOfDestination
		{
			get => base.BY_RN_NKCountryOfDestination;
			set
			{
				var oldValue = base.BY_RN_NKCountryOfDestination;
				base.BY_RN_NKCountryOfDestination = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZString BY_CommercialReferenceNumber
		{
			get => base.BY_CommercialReferenceNumber;
			set
			{
				var oldValue = base.BY_CommercialReferenceNumber;
				base.BY_CommercialReferenceNumber = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZString BY_Description
		{
			get => base.BY_Description;
			set
			{
				var oldValue = base.BY_Description;
				base.BY_Description = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZString BY_CusC4Number
		{
			get => base.BY_CusC4Number;
			set
			{
				var oldValue = base.BY_CusC4Number;
				base.BY_CusC4Number = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZString BY_FormattedHarmonisedTariff
		{
			get => base.BY_FormattedHarmonisedTariff;
			set
			{
				var oldValue = base.BY_FormattedHarmonisedTariff;
				base.BY_FormattedHarmonisedTariff = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZDecimal BY_GrossWeight
		{
			get => base.BY_GrossWeight;
			set
			{
				var oldValue = base.BY_GrossWeight;
				base.BY_GrossWeight = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZDecimal BY_NetWeight
		{
			get => base.BY_NetWeight;
			set
			{
				var oldValue = base.BY_NetWeight;
				base.BY_NetWeight = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		[DecimalPlaces(3)]
		public override ZDecimal BY_CustomsSecondQuantity
		{
			get => base.BY_CustomsSecondQuantity;
			set
			{
				var oldValue = base.BY_CustomsSecondQuantity;
				base.BY_CustomsSecondQuantity = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		public override ZString BY_CustomsSecondUnitQty
		{
			get => base.BY_CustomsSecondUnitQty;
			set
			{
				var oldValue = base.BY_CustomsSecondUnitQty;
				base.BY_CustomsSecondUnitQty = value;

				if (!IsCopying && oldValue != value)
				{
					ModifyReleaseStatusInPredeclarationWhenParentIsBill();
				}
			}
		}

		protected override void ConsigneeJobDocAddressChanged(object sender, EventArgs e)
		{
			base.ConsigneeJobDocAddressChanged(sender, e);
			ModifyReleaseStatusInPredeclarationWhenParentIsBill();
		}

		public ZString TariffDescription
		{
			get
			{
				return ((IFindBoxListProvider)Lookups.Tariffs).DescriptionFromCode(BY_HarmonisedTariff);
			}
		}

		public ZBool IsVehicles
		{
			get => this.GetSystemDefinedValue<ZBool>(GenAddOnHelper.IsVehicles);
			set
			{
				var oldValue = IsVehicles;
				if (value != oldValue)
				{
					this.SetSystemDefinedValue(GenAddOnHelper.IsVehicles, value);
					if (!IsValidationSuspended && !IsPhase5)
					{
						((NctsDepartureCargoDescPhase4Validation)Validation).ValidateIsVehicles();
					}
					Packages.RemoveAndDeleteAll();
					IsVehiclesInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo IsVehiclesInfo => GetZPropertyInfo(nameof(IsVehicles));

		[MaxLength(Schema.ExciseCodeMaxLength)]
		[List(nameof(ESDepartureCargoDescLookups) + "." + nameof(NctsDepartureCargoDescPhase5Lookups.ExciseCodeList))]
		public ZString ExciseCode
		{
			get => this.GetSystemDefinedValue<ZString>(GenAddOnColumnConstants.ExciseCode);
			set
			{
				var oldValue = ExciseCode;
				if (value != oldValue)
				{
					CheckMaximumLength(ExciseCodeInfo, value);
					this.SetSystemDefinedValue(GenAddOnColumnConstants.ExciseCode, value);
					if (!IsValidationSuspended && IsPhase5)
					{
						((NctsDepartureCargoDescPhase5Validation)Validation).ValidateExciseCode();
					}
					ExciseCodeInfo.RefreshBinding(oldValue);
					EU.NCTS.Business.UOMDefaulter.DefaultUOMIfApplicable(CurrentExciseRate, this);
					UpdateAllFeesFromTariffRates();
				}
			}
		}

		public ZPropertyInfo ExciseCodeInfo => GetZPropertyInfo(nameof(ExciseCode));

		public ZDecimal PVPValue
		{
			get => this.GetSystemDefinedValue<ZDecimal>(GenAddOnColumnConstants.PVPValue);
			set
			{
				var oldValue = PVPValue;
				if (value != oldValue)
				{
					this.SetSystemDefinedValue(GenAddOnColumnConstants.PVPValue, value);
					if (!IsValidationSuspended && IsPhase5)
					{
						((NctsDepartureCargoDescPhase5Validation)Validation).ValidatePVPValue();
					}
					PVPValueInfo.RefreshBinding(oldValue);
					UpdateAllFeesFromTariffRates();
				}
			}
		}

		public ZPropertyInfo PVPValueInfo => GetZPropertyInfo(nameof(PVPValue));

		public bool IsPVPApplicable => Factory.GetValue(ref isPVPApplicableCached, () => IsPhase5 && (CurrentExciseRate?.ZZ2_RateFormula.Contains(ES.Business.UniversalReferenceConstants.ReservedRateFormulaValue.RetailPriceCode) ?? false));

		CachedProperty<bool> isPVPApplicableCached;

		public RateView CurrentExciseRate => Factory.GetValue(ref currentExciseRateCached, () => UniversalTariff?.ChildTariffs.Where(x => x.ZZH_RelatedTariffCode == ExciseCode).Select(x => x.RelatedTariffFrom).FirstOrDefault(x => x.ZZ1_ZZI_TariffTypeCode == exciseTariffTypeCodeByDepartureState)?.Rates.FirstOrDefault());

		CachedProperty<RateView> currentExciseRateCached;

		public RateView CurrentSpecialExciseRate => Factory.GetCachedValue("ESJobComInvoiceLine.CurrentSpecialExciseRate_" + ExciseCode, () =>
													{
														var result = UniversalTariff?.ChildTariffs.Where(x => x.ZZH_RelatedTariffCode == SpecialExciseCode).Select(x => x.RelatedTariffFrom).FirstOrDefault()?.Rates.FirstOrDefault();
														return result;
													});

		public ZString SpecialExciseCode
		{
			get
			{
				var specialExciseCode = ZString.Empty;

				if (ExciseCode.StartsWith("0") || ExciseCode.StartsWith("5"))
				{
					specialExciseCode = ExciseCode.StartsWith("0") ? "5" + ExciseCode.SubstringSafe(1) : "0" + ExciseCode.SubstringSafe(1);
				}

				return specialExciseCode;
			}
		}

		[MaxLength(Schema.PVPCurrencyMaxLength)]
		[List(nameof(ESDepartureCargoDescLookups) + "." + nameof(EU.NCTS.Business.IDepartureCargoDescLookups.Currencies))]
		public ZString PVPCurrency => Core.Constants.CurrencyCodes.EuropeanUnion;

		public ZPropertyInfo PVPCurrencyInfo => GetZPropertyInfo(nameof(PVPCurrency));

		protected override IEnumerable<RateView> ExciseRates
		{
			get
			{
				yield return CurrentExciseRate;
				yield return CurrentSpecialExciseRate;
			}
		}

		protected override List<HashSet<string>> GetUnitOfMeasureConversionSets()
		{
			var result = base.GetUnitOfMeasureConversionSets();
			result.Add(new HashSet<string> { CustomsUq.Number.NumberOfItems, ES.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.ThousandItems });
			return result;
		}

		protected override EU.NCTS.Business.GoodsItemDutyCalculator CreateGoodsItemDutyCalculator() => new GoodsItemDutyCalculator(this);

		public new ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CusSupplyChainActorReferences => (ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>)base.CusSupplyChainActorReferences;
		protected override ICusSupplyChainActorReferenceCollection<EU.NCTS.Business.CusSupplyChainActorReference> GetCusSupplyChainActorReferences() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);
		protected override Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

		public new EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument> PreviousDocuments => (EU.NCTS.Business.INctsPreviousDocumentCollection<NctsPreviousDocument>)base.PreviousDocuments;
		protected override EU.NCTS.Business.INctsPreviousDocumentCollection<EU.NCTS.Business.NctsPreviousDocument> GetPreviousDocuments() => new EU.NCTS.Business.NctsPreviousDocumentCollection<NctsPreviousDocument>(this);

		public new EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo> AdditionalInfos => (EU.NCTS.Business.INctsAdditionalInfoCollection<NctsAdditionalInfo>)base.AdditionalInfos;
		protected override EU.NCTS.Business.INctsAdditionalInfoCollection<EU.NCTS.Business.NctsAdditionalInfo> GetNctsAdditionalInfoCollection() => new EU.NCTS.Business.NctsAdditionalInfoCollection<NctsAdditionalInfo>(this);

		public new EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument> SupportingDocuments => (EU.NCTS.Business.INctsSupportingDocumentCollection<NctsSupportingDocument>)base.SupportingDocuments;
		protected override EU.NCTS.Business.INctsSupportingDocumentCollection<EU.NCTS.Business.NctsSupportingDocument> GetNewNctsSupportingDocumentCollection() => new EU.NCTS.Business.NctsSupportingDocumentCollection<NctsSupportingDocument>(this);

		protected override Type AdditionalInfoType => typeof(NctsAdditionalInfo);
		protected override Type PreviousDocumentType => typeof(NctsPreviousDocument);
		protected override Type SupportingDocumentType => typeof(NctsSupportingDocument);

		public new EU.NCTS.Business.INctsPackageCollection<NctsPackage, NctsDepartureCargoDesc> Packages => (NctsPackageCollection<NctsDepartureCargoDesc>)base.Packages;

		protected override EU.NCTS.Business.INctsPackageCollection<EU.NCTS.Business.NctsPackage, EU.NCTS.Business.NctsCommonCargoDesc> GetNctsPackageCollection() => new NctsPackageCollection<NctsDepartureCargoDesc>(this);

		protected override EU.NCTS.Business.NctsDepartureCargoDescPhase5Validation GetNewPhase5Validation() => new NctsDepartureCargoDescPhase5Validation(this);

		protected override EU.NCTS.Business.NctsDepartureCargoDescPhase4Validation GetNewPhase4Validation() => new NctsDepartureCargoDescPhase4Validation(this);

		protected override EU.NCTS.Business.NctsCommonCargoDescLookups GetNewPhase5Lookups() => new NctsDepartureCargoDescPhase5Lookups(this);

		public NctsDepartureCargoDescPhase5Lookups ESDepartureCargoDescLookups => new NctsDepartureCargoDescPhase5Lookups(this);

		protected override void CloneInternalPackages(BusinessObjectCloneArgs args, EU.NCTS.Business.NctsCommonCargoDesc newGoodsItem)
		{
			((NctsDepartureCargoDesc)newGoodsItem).IsVehicles = IsVehicles;
			base.CloneInternalPackages(args, newGoodsItem);
		}

		protected override void CloneInternalAdditionalInfos(BusinessObjectCloneArgs args, EU.NCTS.Business.NctsCommonCargoDesc newGoodsItem)
		{
			foreach (var additionalInfos in AdditionalInfos.OrderBy(x => x.CSI_SubType + x.CSI_LineNo))
			{
				var newAdditionalInfos = (NctsAdditionalInfo)new EU.NCTS.Business.NctsDeepCloneStrategy(additionalInfos, ((NctsDepartureCargoDesc)newGoodsItem).PK).Clone();
				((NctsDepartureCargoDesc)newGoodsItem).AdditionalInfos.Add(newAdditionalInfos);
			}
		}

		protected override void CloneInternalSupportingDocuments(BusinessObjectCloneArgs args, EU.NCTS.Business.NctsCommonCargoDesc newGoodsItem)
		{
			foreach (var supportingDocument in SupportingDocuments.OrderBy(x => x.CSI_LineNo))
			{
				var newSupportingDocument = (NctsSupportingDocument)new EU.NCTS.Business.NctsDeepCloneStrategy(supportingDocument, ((NctsDepartureCargoDesc)newGoodsItem).PK).Clone();
				((NctsDepartureCargoDesc)newGoodsItem).SupportingDocuments.Add(newSupportingDocument);
			}
		}

		public override void OnSaving()
		{
			if (BY_DeclarationGoodsItemNumber.IsEmpty && Header != null)
			{
				Header.ShouldResetGoodsItemNumbers = !Header.UpdatePreDeclaration;
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				Header.ShouldResetGoodsItemNumbers = !Header.UpdatePreDeclaration;
			}
			base.Delete();
		}

		protected override void SetDefaultTaxTypeCore()
		{
			if (!BY_HarmonisedTariff.IsEmpty && IsCustomOfficeCanaryIsland)
			{
				BY_ZZF_NKTaxType = igicTaxType;
			}
			else
			{
				base.SetDefaultTaxTypeCore();
			}
		}

		public bool IsCustomOfficeCanaryIsland
		{
			get
			{
				var header  = Header;
				var customsOfficesForDeparture = header != null ? (header.IsPhase5 ? header.CommonMovementHeader.CustomsOfficesForDeparture : header.CustomsOfficesForDeparture) : null;  
				var customOffice = customsOfficesForDeparture != null ? customsOfficesForDeparture.Where(x => x.CY_Code.Equals(OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)).Select(x => x.CY_Data).FirstOrDefault() : ZString.Empty;
				return customOffice.StartsWith(customOfficeCanaryIslandWith35) || customOffice.StartsWith(customOfficeCanaryIslandWith38);
			}
		}

		ZString exciseTariffTypeCodeByDepartureState => IsCustomOfficeCanaryIsland ? ES.Business.UniversalReferenceConstants.RefCusTariffType.CANEX : ES.Business.UniversalReferenceConstants.RefCusTariffType.ESEXC;

		protected override IEnumerable<RefCusTaxOrFee> GetEffectiveTaxes() => base.GetEffectiveTaxes().Where(x => !x.ZZF_Code.StartsWith(igicPrefix));

		const string customOfficeCanaryIslandWith35 = "ES0035";
		const string customOfficeCanaryIslandWith38 = "ES0038";
		const string igicTaxType = "IG1";
		const string igicPrefix = "IG";
	}
}
