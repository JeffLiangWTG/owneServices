using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using RefCusCodeList = Enterprise.Customs.Business.UniversalReferenceConstants.RefCusCodeList;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class TemporaryStoragePackedItem : AsycudaPackedItem
		, IHugeSequenceNumberLine
		, ITariffFormatProvider
		, Integration.Customs.ICusSupportingInfoTypeSupporter
		, Integration.Customs.ICusCodeDataTypeSupporter
		, ICusReferenceTypeSupporter
		, ISupplementaryCodeSupporter
		, ITariffDescriptionSynchronizerSupporter
	{
		public TemporaryStoragePackedItem(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => Res.GetString("777D2CCE-E768-4703-AFE2-E0D94609FAE5", "Storage Packed Item");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : ManifestBase.AsycudaPackedItem.Schema
		{
			public const string API_FormattedTariff = "API_FormattedTariff";
			public const int API_GrossWeightDecimalPlaces = 2;
			public const int API_NetWeightDecimalPlaces = 3;

			public new const int API_CustomsUQMaxLength = 4;
			public new const int API_CustomsUQ2MaxLength = 4;
			public new const int API_CustomsUQ3MaxLength = 4;
		}

		public static class ChargeType
		{
			public static readonly ZString Duty = "DTY";
			public static readonly ZString AntiDumpingDuty = "ADD";
			public static readonly ZString CountervailingDuty = "CVD";
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			API_RX_NKGoodsValueCurrency = "EUR";
			API_GrossWeightUQ = Core.Constants.Weight.Kilograms;
		}

		public new TemporaryStorageBill Bill => (TemporaryStorageBill)base.Bill;

		public new TemporaryStoragePack Pack => (TemporaryStoragePack)base.Pack;

		public ZString CountryCode => Bill?.Header?.AMA_RN_NKCountry ?? ZString.Empty;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return property.IsReadOnly || ReadOnly || (Bill?.Header?.IsNoEditAllowedCustomsStatus() ?? false) || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		[ReadOnly(true)]
		[ResourceStringData("TemporaryStoragePackedItem.API_LineNo", Caption = "Sequence Number", ShortCaption = "Seq.")]
		public override ZInt API_LineNo
		{
			get { return base.API_LineNo; }
			set
			{
				var oldValue = API_LineNo;
				base.API_LineNo = HugeSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					Bill?.PackedItems.SequenceNumberCalculator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}

		ZGuid ISequenceNumberLine.FKToHeader => API_ABL_Bill;

		ZInt ISequenceNumberLine<ZInt>.SequenceNumber
		{
			get
			{
				return API_LineNo;
			}
			set
			{
				API_LineNo = value;
			}
		}

		[RelatedBusinessObject("Bill")]
		public override ZGuid API_ABL_Bill
		{
			get => base.API_ABL_Bill;
			set
			{
				var oldValue = base.API_ABL_Bill;
				base.API_ABL_Bill = value;
				if (!IsCopying && oldValue != API_ABL_Bill)
				{
					if (!API_ABL_Bill.IsValid)
					{
						DetachedFromBill(oldValue);
					}
					else
					{
						AttachedToBill();
					}
				}
			}
		}

		void DetachedFromBill(ZGuid oldValue)
		{
			var bill = Factory.Load<TemporaryStorageBill>(oldValue);
			if (bill != null)
			{
				bill.PackedItems.SequenceNumberCalculator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
			}
		}

		void AttachedToBill()
		{
			var bill = Bill;
			if (bill != null)
			{
				bill.PackedItems.SequenceNumberCalculator.RecalculateWhenAdded(this);
			}
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePackedItemLookups.TariffList))]
		public override ZString API_Tariff
		{
			get
			{
				return base.API_Tariff;
			}
			set
			{
				base.API_Tariff = TariffFormatter.Format(value).Left(API_TariffInfo.MaxLength);
				Bill.PackedItems.MarkAsNeedingValidation();
				SetDefaultTariffUnitOfMeasures();
				SetDefaultSecondAndThirdUnitOfMeasures();
				SetDefaultTariffUnitOfMeasuresFromRatesView();
			}
		}

		void SetDefaultTariffUnitOfMeasures()
		{
			API_CustomsUQ = Tariff?.GetSpecificUOM(Constants.UnitOfMeasureTypes.AdditionalUOMType) ?? ZString.Empty;
		}

		void SetDefaultSecondAndThirdUnitOfMeasures()
		{
			if (UniversalTariff is TariffView tariff)
			{
				var tradeGroupCountry = API_RN_NKGoodsOrigin.IsEmpty ? (ZString)Core.Constants.CountryCodes.EuropeanUnion : API_RN_NKGoodsOrigin;
				var tariffUoms = tariff.UnitsOfMeasure
					.Where(uom => uom.ZZ8_Type.In(new ZString[] { Constants.UnitOfMeasureTypes.CustomsUOM3Type, Constants.UnitOfMeasureTypes.CustomsUOM4Type, Constants.UnitOfMeasureTypes.CustomsUOM5Type }))
					.Where(uom => uom.CusTradeGroup?.TradeGroupCountries.Any(country => country.ZZB_RN_NKTradeGroupCountryCode == tradeGroupCountry) ?? true)
					.OrderBy(x => x.ZZ8_Type).ThenBy(x => x.ZZ8_UOM).Select(x => x.ZZ8_UOM).ToArray();

				var tariffUomsAlreadySet = tariff.UnitsOfMeasure
					.Where(uom => uom.ZZ8_Type.In(new ZString[] { Constants.UnitOfMeasureTypes.StatisticalUOMType, Constants.UnitOfMeasureTypes.AdditionalUOMType }))
					.Where(uom => uom.CusTradeGroup?.TradeGroupCountries.Any(country => country.ZZB_RN_NKTradeGroupCountryCode == tradeGroupCountry) ?? true)
					.OrderBy(x => x.ZZ8_Type).ThenBy(x => x.ZZ8_UOM).Select(x => x.ZZ8_UOM).ToArray();

				var rateUoms = HighestDuty?.UnitsOfMeasure.Select(x => x.ZXG_UOM).Where(x => !RateCalcUnitOfMeasureAggregator.IsConvertableFrom(x, tariffUomsAlreadySet.Union(tariffUoms), CountryCode, Factory)).OrderBy(x => x) ?? Enumerable.Empty<ZString>();
				var distinctUOMs = new Queue<ZString>(tariffUoms.Union(rateUoms.Where(x => !tariffUoms.Contains(x))).Distinct());

				var secondUnit = ZString.Empty;
				var thirdUnit = ZString.Empty;
				if (distinctUOMs.Count > 0)
				{
					secondUnit = distinctUOMs.First();
					thirdUnit = distinctUOMs.FirstOrDefault(u => u != secondUnit);
				}

				API_CustomsUQ2Info.Value = secondUnit.SubstringSafe(0, API_CustomsUQ2Info.MaxLength);
				API_CustomsUQ3Info.Value = thirdUnit.SubstringSafe(0, API_CustomsUQ3Info.MaxLength);
			}
		}

		void SetDefaultTariffUnitOfMeasuresFromRatesView()
		{
			UOMDefaulter.DefaultUOMIfApplicable(HighestDuty, this);
			UOMDefaulter.DefaultUOMIfApplicable(HighestAntiDumpingDuty, this);
			UOMDefaulter.DefaultUOMIfApplicable(HighestCountervailingDuty, this);
		}

		[ReadOnlyMember(nameof(IsFormattedTariffReadOnly))]
		[ResourceStringData("TemporaryStoragePackedItem.API_FormattedTariff", Caption = "Tariff")]
		public ZString API_FormattedTariff
		{
			get { return TariffFormatter.DisplayFormat(API_Tariff); }
			set
			{
				var oldValue = API_Tariff;
				var shouldSynchronizeDescription = TariffDescriptionSynchronizer.ShouldSynchronizeDescription;

				API_Tariff = value;
				if (!IsCopying && oldValue != API_Tariff && shouldSynchronizeDescription)
				{
					TariffDescriptionSynchronizer.SynchronizeDescription();
				}
			}
		}

		public ZPropertyInfo API_FormattedTariffInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.API_FormattedTariff, x => API_TariffInfo); }
		}

		TariffDescriptionSynchronizer TariffDescriptionSynchronizer => tariffDescriptionSynchronizer ??= new TariffDescriptionSynchronizer(this);
		TariffDescriptionSynchronizer tariffDescriptionSynchronizer;

		#region ITariffDescriptionSyncronizerSupporter

		ZString ITariffDescriptionSynchronizerSupporter.CurrentTariffDescription
		{
			get => API_GoodsDescription;
			set => API_GoodsDescription = value;
		}

		ZString ITariffDescriptionSynchronizerSupporter.OfficialCustomsTariffDescription => (UniversalTariff ?? Tariff)?.FullTariffDescription(ZDateTime.Today, includeSectionHeadings: false, includeChapterHeading: false, useTariffPreferredLanguage: true) ?? ZString.Empty;

		#endregion

		TariffFormatter TariffFormatter
		{
			get { return tariffFormatter ?? (tariffFormatter = TariffFormatter.New(CountryCode)); }
		}
		TariffFormatter tariffFormatter;

		ITariffFormatter ITariffFormatProvider.TariffFormatter => TariffFormatter;

		public TariffView UniversalTariff
		{
			get
			{
				TariffView universalTariff = null;
				if (!API_Tariff.IsEmpty)
				{
					universalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(GetDataGroupingForUniversalTariff, Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff, API_Tariff, ValuationDate);
				}
				return universalTariff;
			}
		}

		public TariffView Tariff
		{
			get
			{
				var tariff = API_Tariff;
				return !tariff.IsEmpty ? new TariffView.Loader(Factory).LoadMostRecentCachedTariff(
					dataGroupingCode: GetDataGroupingForUniversalTariff,
					tariffType: (ZString)Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff,
					tariffCode: API_Tariff,
					valuationDate: ValuationDate,
					tariffCodeComparisonOperator: SQLComparisonOperator.StartsWith
				) : null;
			}
		}

		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePackedItemLookups.CountryOfOriginList))]
		[ResourceStringData("89D10D69-587E-4983-8BD7-1FE7E73457CB", Caption = "Origin Country/Region", MediumCaption = "Origin Ctry./Rgn.", ShortCaption = "Origin", FullDescription = "Country/Region of Origin")]
		public override ZString API_RN_NKGoodsOrigin
		{
			get => base.API_RN_NKGoodsOrigin;
			set
			{
				base.API_RN_NKGoodsOrigin = value;
				SetDefaultTariffUnitOfMeasures();
				SetDefaultSecondAndThirdUnitOfMeasures();
				SetDefaultTariffUnitOfMeasuresFromRatesView();
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("34F92151-BAC9-435A-8D99-8B4BB70FDF38", Caption = "Supplementary Codes", MediumCaption = "Sup. Codes", ShortCaption = "Sup. Codes", FullDescription = "Additional Supplementary Codes")]
		public ZString API_Supplements => AdditionalSupplementaryCodes.AsString;

		public ZPropertyInfo API_SupplementsInfo => GetZPropertyInfo(nameof(API_Supplements));

		[ChildEditable(true)]
		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePackedItemLookups.AdditionalCodeList))]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public SupplementaryCodeCollection AdditionalSupplementaryCodes
		{
			get
			{
				if (additionalSupplementaryCodes == null)
				{
					additionalSupplementaryCodes = SupplementaryCodeCollection.New(API_SupplementsInfo);
					RegisterEditableChildObject(additionalSupplementaryCodes);
					_ = additionalSupplementaryCodes.SubscribeToChildrenChanges(RefreshAllDutyAmountsFromTariffRates, new[] { CusCodeDataSchema.Constants.CY_Code });
				}

				return additionalSupplementaryCodes;
			}
		}
		SupplementaryCodeCollection additionalSupplementaryCodes;

		ZString ISupplementaryCodeSupporter.SupplementaryCodesFieldType => nameof(FieldType.TextDropEdit);

		ResourceStringData ISupplementaryCodeSupporter.SupplementaryCodeCaption => null;

		IEnumerable<BaseSupplementaryCode> ISupplementaryCodeSupporter.SupplementaryCodes => SupplementaryCodes;

		ICusCodeDataCollection<BaseSupplementaryCode> ISupplementaryCodeSupporter.AdditionalSupplementaryCodes => AdditionalSupplementaryCodes;

		void CalculateDutyAmountsForType(ZString chargeType, RateView rateView)
		{
			var newDutiesAndTaxes = new List<DutiesAndTaxesData>();
			AddDutiesAndFeesForType(newDutiesAndTaxes, rateView);
			UpdateDutiesAndTaxesForType(chargeType, newDutiesAndTaxes);
		}

		void AddDutiesAndFeesForType(List<DutiesAndTaxesData> newDutiesAndTaxes, RateView rateView)
		{
			if (rateView != null)
			{
				var calculationResult = CreateGoodsItemDutyCalculator().Calculate(rateView);
				foreach (var intermediateResult in calculationResult.IntermediateResults)
				{
					newDutiesAndTaxes.Add(new DutiesAndTaxesData(intermediateResult.BaseValue, intermediateResult.MethodOfCalculation, intermediateResult.Amount, intermediateResult.Rate));
				}
			}
		}

		void UpdateDutiesAndTaxesForType(ZString chargeType, List<DutiesAndTaxesData> newDutiesAndTaxes)
		{
			var existingDutiesAndTaxes = DutiesAndTaxes.Where(x => x.AET_ChargeType == chargeType).ToList();
			var newDutiesAndTaxesCount = newDutiesAndTaxes.Count;
			var existingDutiesAndTaxesCount = existingDutiesAndTaxes.Count;
			var maxCount = Math.Max(newDutiesAndTaxesCount, existingDutiesAndTaxesCount);
			for (var i = 0; i < maxCount; ++i)
			{
				if (i >= existingDutiesAndTaxesCount)
				{
					existingDutiesAndTaxes.Add(DutiesAndTaxes.AddNew());
				}
				if (i < newDutiesAndTaxesCount)
				{
					existingDutiesAndTaxes[i].AET_ChargeType = chargeType;
					existingDutiesAndTaxes[i].AET_BaseValue = newDutiesAndTaxes[i].BaseValue;
					existingDutiesAndTaxes[i].AET_MethodOfCalculation = newDutiesAndTaxes[i].MethodOfCalculation.Left(CusInBondFee.Schema.BFE_MethodOfCalculationMaxLength);
					existingDutiesAndTaxes[i].AET_ChargeAmount = newDutiesAndTaxes[i].Amount;
					existingDutiesAndTaxes[i].AET_Rate = newDutiesAndTaxes[i].Rate;
				}
				else
				{
					existingDutiesAndTaxes[i].Delete();
				}
			}
		}

		protected internal void RefreshAllDutyAmountsFromTariffRates()
		{
			RefreshDutyAmountsFromTariffRates(refreshDuty: true, refreshADD: true, refreshCVD: true);
			Bill?.Header?.ApportionedAmountToGuaranteeLiabilityAmount();
		}

		public bool CalculateAndRefreshAllDutyAmountFromTariffRates => CalculateAndRefreshAllDutyAmountFromTariffRatesCore;

		protected virtual bool IsFormattedTariffReadOnly => false;

		protected virtual bool CalculateAndRefreshAllDutyAmountFromTariffRatesCore => false;

		internal protected void RefreshDutyAmountsFromTariffRates(bool refreshDuty = false, bool refreshADD = false, bool refreshCVD = false)
		{
			if (CalculateAndRefreshAllDutyAmountFromTariffRates)
			{
				if (refreshDuty)
				{
					CalculateDutyAmountsForType(ChargeType.Duty, HighestDuty);
				}
				if (refreshADD)
				{
					CalculateDutyAmountsForType(ChargeType.AntiDumpingDuty, HighestAntiDumpingDuty);
				}
				if (refreshCVD)
				{
					CalculateDutyAmountsForType(ChargeType.CountervailingDuty, HighestCountervailingDuty);
				}
			}
		}

		GoodsItemDutyCalculator CreateGoodsItemDutyCalculator() => new(this);

		public RateView HighestDuty
		{
			get
			{
				var tradeGroupCountry = API_RN_NKGoodsOrigin.IsEmpty ? (ZString)Core.Constants.CountryCodes.EuropeanUnion : API_RN_NKGoodsOrigin;
				var supplementaryCodesForCachedValue = ZString.Join("_", SupplementaryCodes.Select(x => x.CY_Code).ToArray());
				var effectiveDate = ValuationDate;
				var dataGroupingCode = DataGroupingCode;
				return Factory.GetCachedValue(FormattableString.Invariant($"EU.Business.CusTempStorage.HighestDuty_{tradeGroupCountry}_{dataGroupingCode}_{EffectiveTariffForCalculateFields}_{effectiveDate}_{supplementaryCodesForCachedValue}_{API_GoodsValue}"), () =>
				{
					var primaryPreferenceCode = "100";
					var rateType = Constants.RateTypes.Duty;
					var criteria = new SpecificRateSelectionCriteria(tradeGroupCountry, dataGroupingCode, primaryPreferenceCode, "", new HashSet<ZString>(SupplementaryCodes.Select(x => x.CY_Code)), effectiveDate, rateType, "");

					RateView result = GetApplicableRateFromCriteria(criteria);

					if (result is null)
					{
						var addCode = UniversalTariff?.GetAdditionalCodesForDuty(tradeGroupCountry, effectiveDate, dataGroupingCode, primaryPreferenceCode).OrderByDescending(x => x).FirstOrDefault() ?? ZString.Empty;
						var criteria2 = new SpecificRateSelectionCriteria(tradeGroupCountry, dataGroupingCode, primaryPreferenceCode, "", new HashSet<ZString>() { addCode }, effectiveDate, rateType, "");
						result = GetApplicableRateFromCriteria(criteria2);
					}

					return result;
				});
			}
		}

		protected RateView HighestAntiDumpingDuty
		{
			get
			{
				var tradeGroupCountry = API_RN_NKGoodsOrigin.IsEmpty ? (ZString)Core.Constants.CountryCodes.EuropeanUnion : API_RN_NKGoodsOrigin;
				var addAdditionalCodesForCachedValue = ZString.Join("_", ADDAdditionalCodes.ToArray());
				var effectiveDate = ValuationDate;
				var rateType = Constants.RateTypes.AntiDumping;
				var primaryPreferenceCode = "100";
				var dataGroupingCode = DataGroupingCode;
				return Factory.GetCachedValue(FormattableString.Invariant($"EU.Business.CusTempStorage.HighestAntiDumpingDuty_{tradeGroupCountry}_{dataGroupingCode}_{EffectiveTariffForCalculateFields}_{effectiveDate}_{addAdditionalCodesForCachedValue}"), () =>
				{
					var criteria = new SpecificRateSelectionCriteria(tradeGroupCountry, dataGroupingCode, primaryPreferenceCode, "", new HashSet<ZString>(ADDAdditionalCodes), effectiveDate, rateType, "");
					return GetApplicableRateFromCriteria(criteria);
				});
			}
		}

		public IEnumerable<ZString> ADDAdditionalCodes
		{
			get
			{
				var additionalCodesForAntiDumping = UniversalTariff?.GetAdditionalCodesForAntiDumping(API_RN_NKGoodsOrigin, ValuationDate) ?? Array.Empty<ZString>();
				return GetAdditionalCodes(additionalCodesForAntiDumping);
			}
		}

		protected RateView HighestCountervailingDuty
		{
			get
			{
				var tradeGroupCountry = API_RN_NKGoodsOrigin.IsEmpty ? (ZString)Core.Constants.CountryCodes.EuropeanUnion : API_RN_NKGoodsOrigin;
				var cvdAdditionalCodesForCachedValue = ZString.Join("_", CVDAdditionalCodes.ToArray());
				var effectiveDate = ValuationDate;
				var rateType = Constants.RateTypes.Countervailing;
				var primaryPreferenceCode = "100";
				var dataGroupingCode = DataGroupingCode;
				return Factory.GetCachedValue(FormattableString.Invariant($"EU.Business.CusTempStorage.HighestCountervailingDuty_{tradeGroupCountry}_{dataGroupingCode}_{EffectiveTariffForCalculateFields}_{effectiveDate}_{cvdAdditionalCodesForCachedValue}"), () =>
				{
					var criteria = new SpecificRateSelectionCriteria(tradeGroupCountry, dataGroupingCode, primaryPreferenceCode, "", new HashSet<ZString>(CVDAdditionalCodes), effectiveDate, rateType, "");
					return GetApplicableRateFromCriteria(criteria);
				});
			}
		}

		public IEnumerable<ZString> CVDAdditionalCodes
		{
			get
			{
				var additionalCodesForCountervailing = UniversalTariff?.GetAdditionalCodesForCountervailing(API_RN_NKGoodsOrigin, ValuationDate) ?? Array.Empty<ZString>();
				return GetAdditionalCodes(additionalCodesForCountervailing);
			}
		}

		IEnumerable<ZString> GetAdditionalCodes(IEnumerable<ZString> additionalCodes)
		{
			var supplementaryCodes = SupplementaryCodes.Select(x => x.CY_Code);

			if (additionalCodes.Any(x => supplementaryCodes.Contains(x)))
			{
				return supplementaryCodes;
			}
			else
			{
				var worstCaseAdditionalCodes = EUUniversalLookupsHelper.GetWorstCaseAdditionalCodes(Factory, DataGroupingCode, ValuationDate).GetAllCodesZString();
				if (additionalCodes.Any(x => worstCaseAdditionalCodes.Contains(x)))
				{
					return worstCaseAdditionalCodes;
				}
			}
			return Array.Empty<ZString>();
		}

		protected RateView GetApplicableRateFromCriteria(SpecificRateSelectionCriteria criteria)
		{
			var applicableRates = UniversalTariff?.GetApplicableRates(criteria);

			RateView result = null;

			if (applicableRates != null)
			{
				var applicableRatesCount = applicableRates.Count();
				if (applicableRatesCount == 1)
				{
					result = applicableRates.First();
				}
				else if (applicableRatesCount > 1)
				{
					result = applicableRates.Where(x => x.RateApplicabilities.Any(y => y.ZZT_AdditionalCode != ""))
											.OrderByDescending(x => x.ZZ2_StartDate)
											.ThenBy(x => x.PreferenceCode)
											.FirstOrDefault();
					if (result is null)
					{
						var calculator = new GoodsItemDutyCalculator(this);
						result = applicableRates.MaxBy(p => calculator.Calculate(p).ResultAmount);
					}
				}
			}

			return result;
		}

		public ZString DataGroupingCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public ZString CustomsFirstUnitQtyKilograms => API_CustomsUQ.IsEmpty ? (ZString)RefCusCodeList.CustomsUq.Weight.Kilogram : API_CustomsUQ;

		TariffView ICusCodeDataWithOrderSupporter.Tariff => Tariff;

		IZZRateSelectionCriteria ICusCodeDataWithOrderSupporter.RateSelectionCriteria => AllApplicableRatesSelectionCriteria;

		public ZString GetCountryCodeFromAdditionalCode(ZString additionalCode) => ZString.Empty;

		public ZString GetCountryCodeForCodeProvider() => Bill?.Header?.AMA_RN_NKCountry ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		void ICusCodeDataWithOrderSupporter.OnCodesChanged()
		{
		}

		CodeDescriptionPairList ICusCodeDataWithOrderSupporter.CachedListOfAdditionalCodeDescriptions => Lookups.CachedListOfAdditionalCodeDescriptions;

		public IEnumerable<SupplementaryCode> SupplementaryCodes => AdditionalSupplementaryCodes.OfType<SupplementaryCode>();

		[MeasureUnit(nameof(API_RX_NKGoodsValueCurrency), MeasureUnitType.Unknown)]
		[ResourceStringData("45DC9821-3707-48B1-B7D0-F86AE2F7487D", Caption = "Monetary Value", MediumCaption = "Mon. Value", ShortCaption = "Mon. Value", FullDescription = "Item's Monetary Value")]
		public override ZDecimal API_GoodsValue { get => base.API_GoodsValue; set { base.API_GoodsValue = value; } }

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePackedItemLookups.Currencies))]
		public override ZString API_RX_NKGoodsValueCurrency
		{
			get => base.API_RX_NKGoodsValueCurrency;
			set => base.API_RX_NKGoodsValueCurrency = value.ToUpper();
		}

		[MeasureUnit(nameof(API_CustomsQty), MeasureUnitType.Unknown)]
		[ResourceStringData("D4DE9118-4069-4D8D-8A87-B51719FD4789", Caption = "Supplementary Quantity", MediumCaption = "Supplementary Qty", ShortCaption = "Sup. Qty", FullDescription = "Supplementary Additional Quantity for Liability Amount Calculation")]
		public override ZDecimal API_CustomsQty { get => base.API_CustomsQty; set { base.API_CustomsQty = value; } }

		[MaxLength(Schema.API_CustomsUQMaxLength)]
		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePackedItemLookups.CustomsUnitOfQuantityList))]
		[ResourceStringData("763E4417-D091-4718-9468-5C1D68539C9D", Caption = "Supplementary Unit Quantity", MediumCaption = "Supplementary Unit Qty", ShortCaption = "Sup. Unit Qty", FullDescription = "Supplementary Additional Unit for Liability Amount Calculation")]
		public override ZString API_CustomsUQ { get => base.API_CustomsUQ; set => base.API_CustomsUQ = value.ToUpper(); }

		[MeasureUnit(nameof(API_CustomsUQ2), MeasureUnitType.Unknown)]
		[ResourceStringData("33918A3E-F643-432C-9DCA-14FA34C58157", Caption = "Second Quantity", MediumCaption = "Second Qty", ShortCaption = "Second Qty", FullDescription = "Second Additional Quantity for Liability Amount Calculation")]
		public override ZDecimal API_CustomsQty2 { get => base.API_CustomsQty2; set { base.API_CustomsQty2 = value; } }

		[MaxLength(Schema.API_CustomsUQ2MaxLength)]
		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePackedItemLookups.CustomsUnitOfQuantityList))]
		public override ZString API_CustomsUQ2 { get => base.API_CustomsUQ2; set => base.API_CustomsUQ2 = value; }

		[MeasureUnit(nameof(API_CustomsUQ3), MeasureUnitType.Unknown)]
		[ResourceStringData("5F3BE53E-BF27-4B24-9EE2-F1C60E441BA4", Caption = "Third Quantity", MediumCaption = "Third Qty", ShortCaption = "Third Qty", FullDescription = "Third Additional Quantity for Liability Amount Calculation")]
		public override ZDecimal API_CustomsQty3 { get => base.API_CustomsQty3; set { base.API_CustomsQty3 = value; } }

		[MaxLength(Schema.API_CustomsUQ3MaxLength)]
		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePackedItemLookups.CustomsUnitOfQuantityList))]
		public override ZString API_CustomsUQ3 { get => base.API_CustomsUQ3; set => base.API_CustomsUQ3 = value; }

		[ResourceStringData("60F3E1FB-CA8F-4DA9-A56B-E7D2FBC704C6", Caption = "Liability Amount", MediumCaption = "Liability Amount", ShortCaption = "Lia. Amount", FullDescription = "Calculated Liability Amount")]
		public virtual ZDecimal LiabilityAmount => Factory.GetCached(ref liabilityAmountCached, GetLiabilityAmount);
		CachedProperty<ZDecimal> liabilityAmountCached;
		public ZPropertyInfo LiabilityAmountInfo => GetZPropertyInfo(nameof(LiabilityAmount));

		ZDecimal GetLiabilityAmount() => DutiesAndTaxes.Cast<TemporaryStorageDutyAndTax>().Sum(x => x.AET_ChargeAmount) + API_TaxAmount;

		public IZZRateSelectionCriteria AllApplicableRatesSelectionCriteria => Factory.GetValue(ref allApplicableRatesSelectionCriteria, GetAllApplicableRatesSelectionCriteriaCore);
		CachedProperty<IZZRateSelectionCriteria> allApplicableRatesSelectionCriteria;

		public ZString EffectiveCountryOfOrigin
		{
			get { return API_RN_NKGoodsOrigin.IsEmpty ? (ZString)Core.Constants.CountryCodes.EuropeanUnion : API_RN_NKGoodsOrigin; }
		}

		protected IZZRateSelectionCriteria GetAllApplicableRatesSelectionCriteriaCore() => new SpecificRateSelectionCriteria(EffectiveCountryOfOrigin, GetDataGroupingForUniversalTariff, "", "", new HashSet<ZString>() { }, ValuationDate, "", "");

		public IZZConditionSelectionCriteria ConditionSelectionCriteria => Factory.GetValue(ref conditionSelectionCriteria, GetConditionSelectionCriteriaCore);
		CachedProperty<IZZConditionSelectionCriteria> conditionSelectionCriteria;
		protected IZZConditionSelectionCriteria GetConditionSelectionCriteriaCore() => new ZZConditionSelectionCriteria<TemporaryStoragePackedItem>(this);

		protected ZString EffectiveTariffForCalculateFields => API_Tariff;

		public ZDateTime ValuationDate => ZDateTime.Today;

		public ZString GetDataGroupingForUniversalTariff => Bill?.Header?.DataGrouping ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		protected virtual int API_GrossWeightDecimalPlaces => Schema.API_GrossWeightDecimalPlaces;

		[ResourceStringData("TemporaryStoragePackedItem.API_GoodsDescription", Caption = "Goods Description", ShortCaption = "Goods desc.")]
		public override ZString API_GoodsDescription { get => base.API_GoodsDescription; set => base.API_GoodsDescription = value; }

		[ResourceStringData("TemporaryStoragePackedItem.API_GrossWeight", Caption = "Gross Weight")]
		[DecimalPlaces(nameof(API_GrossWeightDecimalPlaces))]
		public override ZDecimal API_GrossWeight { get => base.API_GrossWeight; set => base.API_GrossWeight = value; }

		[ResourceStringData("TemporaryStoragePackedItem.API_GrossWeightUQ", Caption = "Gross Weight UQ", ShortCaption = "UG")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePackedItemLookups.GrossWeightUQList))]
		public override ZString API_GrossWeightUQ { get => base.API_GrossWeightUQ; set => base.API_GrossWeightUQ = value; }

		[ResourceStringData("TemporaryStoragePackedItem.API_NetWeight", Caption = "Net Weight")]
		[DecimalPlaces(Schema.API_NetWeightDecimalPlaces)]
		public override ZDecimal API_NetWeight { get => base.API_NetWeight; set => base.API_NetWeight = value; }

		[ResourceStringData("TemporaryStoragePackedItem.API_NetWeightUQ", Caption = "Net Weight UQ", ShortCaption = "UN")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePackedItemLookups.NetWeightUQList))]
		public override ZString API_NetWeightUQ { get => base.API_NetWeightUQ; set => base.API_NetWeightUQ = value; }

		[ResourceStringData("TemporaryStoragePackedItem.API_ChemicalSubstanceCode", Caption = "Customs Union and Statistics (CUS) Code", MediumCaption = "CUS Code", ShortCaption = "CUS", FullDescription = "The Customs Union and Statistics (CUS) Code is the identifier assigned within the European Customs Inventory of Chemical Substances (ECICS) to mainly chemical substances and preparations.")]
		[List(nameof(Lookups) + "." + nameof(TemporaryStoragePackedItemLookups.ChemicalSubstanceCodeList))]
		public override ZString API_ChemicalSubstanceCode { get => base.API_ChemicalSubstanceCode; set => base.API_ChemicalSubstanceCode = value; }

		public new TemporaryStoragePackedItemLookups Lookups => (TemporaryStoragePackedItemLookups)base.Lookups;

		protected override AsycudaPackedItemLookups GetNewLookups() => new TemporaryStoragePackedItemLookups(this);

		protected override AsycudaPackedItemValidation GetNewValidation() => new TemporaryStoragePackedItemValidation(this);

		[ChildEditable]
		public ITemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> AdditionalInfos => fAdditionalInfos ?? (fAdditionalInfos = GetAdditionalInfos());
		ITemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> fAdditionalInfos;

		ITemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> GetAdditionalInfos()
		{
			var result = CreateNewAdditionalInfoCollection();
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected virtual ITemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo> CreateNewAdditionalInfoCollection()
		{
			return new TemporaryStorageAdditionalInfoCollection<TemporaryStorageAdditionalInfo>(this);
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public ITemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage> TemporaryStorageLinkPackages
		{
			get
			{
				if (temporaryStorageLinkPackages == null)
				{
					temporaryStorageLinkPackages = GetNewTemporaryStorageLinkPackagesCore();
					temporaryStorageLinkPackages.Load();
					RegisterEditableChildObject(temporaryStorageLinkPackages);
				}

				return temporaryStorageLinkPackages;
			}
		}
		ITemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage> temporaryStorageLinkPackages;

		protected virtual ITemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage> GetNewTemporaryStorageLinkPackagesCore()
		{
			return new TemporaryStorageLinkPackageCollection<TemporaryStorageLinkPackage>(this);
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public AsycudaPackPackedItemPivotCollection PackagesPivot
		{
			get
			{
				if (fPackagesPivot == null)
				{
					fPackagesPivot = GetNewPackagesPivotCore();
					fPackagesPivot.Load();
					fPackagesPivot.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(fPackagesPivot);
				}
				return fPackagesPivot;
			}
		}
		AsycudaPackPackedItemPivotCollection fPackagesPivot;

		protected virtual AsycudaPackPackedItemPivotCollection GetNewPackagesPivotCore()
		{
			return new AsycudaPackPackedItemPivotCollection<TemporaryStoragePackedItem, TemporaryStoragePack>(this);
		}

		public AsycudaPackPackedItemPivot ToggleLinkageWithPackage(TemporaryStoragePack package, bool value)
		{
			return ToggleLinkageWithPackageCore(this, package, value);
		}

		AsycudaPackPackedItemPivot ToggleLinkageWithPackageCore(TemporaryStoragePackedItem packedItem, TemporaryStoragePack package, bool value)
		{
			AsycudaPackPackedItemPivot result = null;
			var basePackagePivotCollection = ((packedItem != null && package != null) ? PackagesPivot : null);
			if (basePackagePivotCollection != null)
			{
				if (value)
				{
					result = basePackagePivotCollection.AddPivotFor(package);
				}
				else if (basePackagePivotCollection.Cast<AsycudaPackPackedItemPivot>().Any(p => p.APP_APA_Pack == package.PK))
				{
					basePackagePivotCollection.DeletePivotFor(package);
				}
			}

			return result;
		}

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

			return result;
		}

		protected virtual ITemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument> CreateNewSupportingDocumentCollection() => new TemporaryStorageSupportingDocumentCollection<TemporaryStorageSupportingDocument>(this);

		#endregion SupportingDocuments

		public TemporaryStorageDutyAndTaxCollection DutiesAndTaxes
		{
			get
			{
				if (dutiesAndTaxes == null)
				{
					dutiesAndTaxes = new TemporaryStorageDutyAndTaxCollection(this);
					dutiesAndTaxes.Load();
				}

				return dutiesAndTaxes;
			}
		}
		TemporaryStorageDutyAndTaxCollection dutiesAndTaxes;

		protected override Type GetAsycudaTaxTypeCore() => typeof(TemporaryStorageDutyAndTax);

		#region ICusCodeDataTypeSupporter Implementation

		IDictionary<ZString, Type> Integration.Customs.ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ EU.Business.CusCodeDataTypeList.Codes.SupplementaryCode, typeof(SupplementaryCode) }
			};
		}

		#endregion

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

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

		IDictionary<ZString, Type> ICusReferenceTypeSupporter.GetCusReferenceTypes() => new Dictionary<ZString, Type>
		{
			{ CusReferenceTypeList.Codes.SupplyChainActor, SupplyChainActorType },
		};

		protected virtual Type SupplyChainActorType => typeof(CusSupplyChainActorReference);

		[ChildEditable]
		public ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> SupplyChainActors
		{
			get
			{
				if (supplyChainActors == null)
				{
					supplyChainActors = CreateNewSupplyChainActors();
					supplyChainActors.Load();
					RegisterEditableChildObject(supplyChainActors);
				}
				return supplyChainActors;
			}
		}

		protected virtual ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> CreateNewSupplyChainActors() => new CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>(this);

		public Guid RegistryCompanyPK => GlbCompany.CurrentCompany.PK.ToGuid();

		public Guid RegistryBranchPK => GlbBranch.CurrentBranch.PK.ToGuid();

		ICusSupplyChainActorReferenceCollection<CusSupplyChainActorReference> supplyChainActors;

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
				DutiesAndTaxes.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		#region Cloning and copying

		protected override bool SupportsCloneCore() => true;

		protected virtual TemporaryStorageHeaderCloneStrategy GetTemporaryStorageHeaderCloneStrategy(BusinessObject bizObjToClone) => new(bizObjToClone);

		internal ITemporaryStoragePackedItemValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, () => Bill?.Header?.Configuration.PackedItemConfiguration.GetValidationDecider());
		CachedValue<ITemporaryStoragePackedItemValidationDecider> validationDeciderCached;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newPackedItem = (TemporaryStoragePackedItem)base.CloneInternal(args);

			SupportingDocuments.ForEach(supDoc => newPackedItem.SupportingDocuments.Add(Clone(supDoc)));
			PreviousDocuments.ForEach(preDoc => newPackedItem.PreviousDocuments.Add(Clone(preDoc)));
			AdditionalInfos.ForEach(addInfo => newPackedItem.AdditionalInfos.Add(Clone(addInfo)));
			SupplyChainActors.ForEach(addInfo => newPackedItem.SupplyChainActors.Add(Clone(addInfo)));

			return newPackedItem;

			BusinessObject Clone(BusinessObject bizObjToClone)
				=> GetTemporaryStorageHeaderCloneStrategy(bizObjToClone).Clone();
		}

		#endregion

		protected override IValueSetStrategy GetValueSetStrategy() => new TemporaryStoragePackedItemValueSetStrategy(this);

		protected readonly struct DutiesAndTaxesData
		{
			public DutiesAndTaxesData(ZDecimal baseValue, ZString methodOfCalculation, ZDecimal amount, ZDecimal rate)
			{
				BaseValue = baseValue;
				MethodOfCalculation = methodOfCalculation;
				Amount = amount.Round(2);
				Rate = rate * (methodOfCalculation == "%" ? 100 : 1);
			}

			public ZDecimal BaseValue { get; }
			public ZString MethodOfCalculation { get; }
			public ZDecimal Amount { get; }
			public ZDecimal Rate { get; }
		}
	}
}
