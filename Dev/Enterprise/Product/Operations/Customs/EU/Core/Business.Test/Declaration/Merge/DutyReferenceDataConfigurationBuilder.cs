using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Internal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class DutyReferenceDataConfigurationBuilder
	{
		DutyReferenceDataConfigurationBuilder(BusinessObjectFactory factory)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			TariffTypes = new List<DutyTariffTypeConfigurationBuilder>();
			Preferences = new List<ZString>();
		}
		ZDateTime startDate;
		ZDateTime endDate;
		RefDataGrouping eunDataGrouping;
		CusRefTradeGroupView cusRefTradeGroupView;
		RefCusRateType dtyRateType;
		RefCusRateType addRateType;
		RefCusRateType cvdRateType;

		BusinessObjectFactory Factory { get; }
		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;

		public List<DutyTariffTypeConfigurationBuilder> TariffTypes { get; }
		public List<ZString> Preferences { get; }

		public DutyTariffTypeConfigurationBuilder AddTariffType(ZString typeCode)
		{
			var dutyTariffTypeConfigurationBuilder = DutyTariffTypeConfigurationBuilder.New(this, typeCode);
			TariffTypes.Add(dutyTariffTypeConfigurationBuilder);
			return dutyTariffTypeConfigurationBuilder;
		}

		public DutyReferenceDataConfigurationBuilder AddPreferences(params ZString[] preferences)
		{
			Preferences.AddRange(preferences.Where(x => !x.IsEmpty));
			return this;
		}

		public DutyReferenceDataConfigurationBuilder AddTaxOrFee(ZString taxCode, ZDecimal taxRate)
		{
			RefDataHelper.CreateTaxOrFee(taxCode, taxRate, eunDataGrouping.ZZZ_DataGrouping, startDate, endDate);
			return this;
		}

		DutyReferenceDataConfigurationBuilder Initalize()
		{
			startDate = ZDateTime.Today.AddYears(-1);
			endDate = ZDateTime.Today.AddYears(1);
			eunDataGrouping = RefDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			RefDataHelper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, parent: eunDataGrouping);
			cusRefTradeGroupView = RefDataHelper.CreateTradeGroup(eunDataGrouping.ZZZ_DataGrouping, "STANDARD", startDate, endDate);
			dtyRateType = RefDataHelper.CreateNewOrGetExistingRateType(eunDataGrouping.ZZZ_DataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty);
			addRateType = RefDataHelper.CreateNewOrGetExistingRateType(eunDataGrouping.ZZZ_DataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.AntiDumpingDuty);
			cvdRateType = RefDataHelper.CreateNewOrGetExistingRateType(eunDataGrouping.ZZZ_DataGrouping, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.CountervailingDuty);

			return this;
		}

		public void Configure()
		{
			var preferenceCollection = new List<CusRefPreferenceView>();
			foreach (var preferenceCode in Preferences)
			{
				var preference = RefDataHelper.CreatePreferenceForCountry(preferenceCode, "STANDARD", eunDataGrouping.ZZZ_DataGrouping);
				preferenceCollection.Add(preference);
			}

			foreach (var tariffType in TariffTypes)
			{
				var refCusTariffType = RefDataHelper.CreateNewOrGetExistingTariffType(eunDataGrouping.ZZZ_DataGrouping, tariffType.TypeCode);
				Factory.Save();
				foreach (var tariff in tariffType.Tariffs)
				{
					var refCusTariff = RefDataHelper.CreateTariff(eunDataGrouping.ZZZ_DataGrouping, refCusTariffType.PK, tariff.TariffCode, startDate, endDate, taxOrFeeCode: tariff.TaxOrFeeCode);
					Factory.Save();
					foreach (var rateCode in tariff.RateCodes)
					{
						var refCusRateCode = RefDataHelper.LoadOrCreateNewCusRateCode(Factory, rateCode.RateCode, GetRefCusRateType(rateCode.RateType).PK);
						var refCusRate = RefDataHelper.CreateRefCusRate(refCusTariff.PK, refCusRateCode.PK, startDate, endDate, rateCode.RateFormula, preferenceCollection.SingleOrDefault(x => x.ZZS_Preference == rateCode.Preference)?.PK ?? ZGuid.Empty);

						if (rateCode.AdditionalCodes.Any())
						{
							foreach (var additionalCode in rateCode.AdditionalCodes)
							{
								CreateCusApplicability(refCusRate, additionalCode);
							}
						}
						else
						{
							CreateCusApplicability(refCusRate, ZString.Empty);
						}
					}
				}
			}
		}

		void CreateCusApplicability(RefCusRate refCusRate, ZString additionalCode) => RefDataHelper.CreateCusApplicability(refCusRate.PK, cusRefTradeGroupView, startDate, endDate, additionalCode);

		RefCusRateType GetRefCusRateType(RateTypeEnum rateType)
		{
			switch (rateType)
			{
				case RateTypeEnum.Duty:
					return dtyRateType;
				case RateTypeEnum.CounterVailing:
					return cvdRateType;
				case RateTypeEnum.Antidumping:
					return addRateType;
				default:
					throw new ArgumentException($"No RefCusRateType associated to RateType: {rateType}");
			}
		}

		public static DutyReferenceDataConfigurationBuilder New(BusinessObjectFactory factory)
		{
			var dutyReferenceDataConfigurationBuilder = new DutyReferenceDataConfigurationBuilder(factory);
			dutyReferenceDataConfigurationBuilder.Initalize();
			return dutyReferenceDataConfigurationBuilder;
		}
	}

	public enum RateTypeEnum
	{
		Duty,
		CounterVailing,
		Antidumping,
	}
}
