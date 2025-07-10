using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class RefTariffSynchronizeProcessorTest : TestCaseWithFactory
	{
		[TestDate(2024, 05, 25)]
		public void TestProcess1()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRate.Codes.EXS, rateType1.PK);
			var rateType2 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Duty);
			var rateCode2 = universalHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRate.Codes.DTY, rateType2.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);
			var preference2 = universalHelper.CreatePreferenceForCountry("02", "Preference 02", Core.Constants.CountryCodes.Canada);

			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", ZDateTime.Today, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff1, UOMTypeList.Codes.CU1, CustomsUnitOfMeasureList.Codes.MetricTon);
			universalHelper.CreateRate(tariff1, rateCode1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.MaxSmallDateTime, "0.4*VFD", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.Today, ZDateTime.MaxSmallDateTime, "MAX(1.65*VFD,2.94*[KGM])", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			universalHelper.CreateRate(tariff1, rateCode2.PK, ZDateTime.Today, ZDateTime.MaxSmallDateTime, "15.90*[TNE] + 0.07*VFD", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);

			var tariff2 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000002", ZDateTime.Today, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateTariffUOM(tariff2, UOMTypeList.Codes.CU1, CustomsUnitOfMeasureList.Codes.Kilogram);
			universalHelper.CreateRate(tariff2, rateCode1.PK, ZDateTime.Today, ZDateTime.MaxSmallDateTime, "0.4*VFD", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			universalHelper.CreateRate(tariff2, rateCode2.PK, ZDateTime.Today, ZDateTime.MaxSmallDateTime, "0.08*VFD", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			universalHelper.CreateRate(tariff2, rateCode2.PK, ZDateTime.Today, ZDateTime.MaxSmallDateTime, "0", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);

			var tariff3 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000003", ZDateTime.Today, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateRate(tariff3, rateCode1.PK, ZDateTime.Today, ZDateTime.MaxSmallDateTime, "0.4*VFD", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			universalHelper.CreateRate(tariff3, rateCode2.PK, ZDateTime.Today, ZDateTime.MaxSmallDateTime, "MAX(1.65*VFD,2.94*[KGM])", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			universalHelper.CreateRate(tariff3, rateCode2.PK, ZDateTime.Today.AddDays(-1), ZDateTime.MaxSmallDateTime, "MIN(9.48*[KGM], MAX(0.05*VFD,4.74*[KGM]))", preference2.PK, dataGrouping: Core.Constants.CountryCodes.Canada);
			Factory.Save();

			var logger = new SimpleLogger();
			var processor = new RefTariffSynchronizeProcessor(logger);
			processor.Process();
			AssertEquals(@"Start Tariff Synchronization processing
Successfully synchronized 3 Tariff.
", logger.ToString());

			var date = new ZDateTime(2024, 10, 04);

			var classHeaders = Factory.Load<CACClassHeader>(new ZQuery { OrderBy = CACClassHeader.Schema.ZA_ClassificationNumber });
			AssertEquals("CACClassHeaders count", 3, classHeaders.Length);
			AssertClassHeader(classHeaders[0], "00000001", "XXX", date, ZDateTime.MaxSmallDateTime, false, false, false, false);
			AssertEquals("CACRateHeaders(ExciseDutyRate) count", 1, classHeaders[0].ExciseDutyRates.Count);
			AssertRateHeader(classHeaders[0].ExciseDutyRates[0], date, ZDateTime.MaxSmallDateTime, false, ZString.Empty, false);
			AssertEquals("ExciseDutyRates.Rates count", 1, classHeaders[0].ExciseDutyRates[0].Rates.Count);
			AssertRate(classHeaders[0].ExciseDutyRates[0].Rates[0], ZString.Empty, false, false);
			AssertEquals("ExciseDutyRates.Rates.RateLines count", 1, classHeaders[0].ExciseDutyRates[0].Rates[0].RateLines.Count);
			AssertRateLine(classHeaders[0].ExciseDutyRates[0].Rates[0].RateLines[0], RateTypes.Codes.AdValorem, 40m, 0m, 0m);
			AssertEquals("CACRateHeaders(ClassificationRate) count", 1, classHeaders[0].ClassRates.Count);
			AssertRateHeader(classHeaders[0].ClassRates[0], date, ZDateTime.MaxSmallDateTime, false, CustomsUnitOfMeasureList.Codes.MetricTon, false);
			AssertEquals("ClassRates.Rates count", 2, classHeaders[0].ClassRates[0].Rates.Count);
			var rate1 = classHeaders[0].ClassRates[0].Rates.Cast<CACRate>().FirstOrDefault(r => r.ZC_TreatmentCode == "01");
			AssertRate(rate1, "01", false, false);
			AssertEquals("rate1.RateLines count", 2, rate1.RateLines.Count);
			AssertRateLine(rate1.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.AdValorem), RateTypes.Codes.AdValorem, 165m, 0m, 0m);
			AssertRateLine(rate1.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.Specific), RateTypes.Codes.Specific, 0m, 2.94m, 0m);
			var rate2 = classHeaders[0].ClassRates[0].Rates.Cast<CACRate>().FirstOrDefault(r => r.ZC_TreatmentCode == "02");
			AssertRate(rate2, "02", false, false);
			AssertEquals("rate2.RateLines count", 2, rate2.RateLines.Count);
			AssertRateLine(rate2.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.AdValorem), RateTypes.Codes.AdValorem, 7m, 0m, 0m);
			AssertRateLine(rate2.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.Specific), RateTypes.Codes.Specific, 15.9m, 0m, 0m);

			AssertClassHeader(classHeaders[1], "00000002", "XXX", date, ZDateTime.MaxSmallDateTime, false, false, false, false);
			AssertEquals("CACRateHeaders(ExciseDutyRate) count", 1, classHeaders[1].ExciseDutyRates.Count);
			AssertRateHeader(classHeaders[1].ExciseDutyRates[0], date, ZDateTime.MaxSmallDateTime, false, ZString.Empty, false);
			AssertEquals("ExciseDutyRates.Rates count", 1, classHeaders[1].ExciseDutyRates[0].Rates.Count);
			AssertRate(classHeaders[1].ExciseDutyRates[0].Rates[0], ZString.Empty, false, false);
			AssertEquals("ExciseDutyRates.Rates.RateLines count", 1, classHeaders[1].ExciseDutyRates[0].Rates[0].RateLines.Count);
			AssertRateLine(classHeaders[1].ExciseDutyRates[0].Rates[0].RateLines[0], RateTypes.Codes.AdValorem, 40m, 0m, 0m);
			AssertEquals("CACRateHeaders(ClassificationRate) count", 1, classHeaders[1].ClassRates.Count);
			AssertRateHeader(classHeaders[1].ClassRates[0], date, ZDateTime.MaxSmallDateTime, false, ZString.Empty, false);
			AssertEquals("ClassRates.Rates count", 2, classHeaders[1].ClassRates[0].Rates.Count);
			var rate3 = classHeaders[1].ClassRates[0].Rates.Cast<CACRate>().FirstOrDefault(r => r.ZC_TreatmentCode == "01");
			AssertRate(rate3, "01", false, false);
			AssertEquals("rate3.RateLines count", 1, rate3.RateLines.Count);
			AssertRateLine(rate3.RateLines[0], RateTypes.Codes.AdValorem, 8m, 0m, 0m);
			var rate4 = classHeaders[1].ClassRates[0].Rates.Cast<CACRate>().FirstOrDefault(r => r.ZC_TreatmentCode == "02");
			AssertRate(rate4, "02", true, true);

			AssertClassHeader(classHeaders[2], "00000003", "XXX", date, ZDateTime.MaxSmallDateTime, false, false, false, false);
			AssertEquals("CACRateHeaders(ExciseDutyRate) count", 1, classHeaders[2].ExciseDutyRates.Count);
			AssertRateHeader(classHeaders[2].ExciseDutyRates[0], date, ZDateTime.MaxSmallDateTime, false, ZString.Empty, false);
			AssertEquals("ExciseDutyRates.Rates count", 1, classHeaders[2].ExciseDutyRates[0].Rates.Count);
			AssertRate(classHeaders[2].ExciseDutyRates[0].Rates[0], ZString.Empty, false, false);
			AssertEquals("ExciseDutyRates.Rates.RateLines count", 1, classHeaders[2].ExciseDutyRates[0].Rates[0].RateLines.Count);
			AssertRateLine(classHeaders[2].ExciseDutyRates[0].Rates[0].RateLines[0], RateTypes.Codes.AdValorem, 40m, 0m, 0m);
			AssertEquals("CACRateHeaders(ClassificationRate) count", 1, classHeaders[2].ClassRates.Count);
			var rateHeader = classHeaders[2].ClassRates[0];
			AssertRateHeader(rateHeader, date, ZDateTime.MaxSmallDateTime, false, ZString.Empty, false);
			AssertEquals("rateHeader1.Rates count", 2, rateHeader.Rates.Count);
			rate1 = rateHeader.Rates.Cast<CACRate>().FirstOrDefault(r => r.ZC_TreatmentCode == "01");
			AssertRate(rate1, "01", false, false);
			AssertEquals("RateLines count", 2, rate1.RateLines.Count);
			AssertRateLine(rate1.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.AdValorem), RateTypes.Codes.AdValorem, 165m, 0m, 0m);
			AssertRateLine(rate1.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.Specific), RateTypes.Codes.Specific, 0m, 2.94m, 0m);
			rate2 = rateHeader.Rates.Cast<CACRate>().FirstOrDefault(r => r.ZC_TreatmentCode == "02");
			AssertRate(rate2, "02", false, false);
			AssertEquals("RateLines count", 3, rate2.RateLines.Count);
			AssertRateLine(rate2.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateType == RateTypes.Codes.AdValorem), RateTypes.Codes.AdValorem, 5m, 0m, 0m);
			AssertRateLine(rate2.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateMin != 0m), RateTypes.Codes.Specific, 0m, 4.74m, 0m);
			AssertRateLine(rate2.RateLines.Cast<CACRateLine>().FirstOrDefault(r => r.ZR_DutyRateMax != 0m), RateTypes.Codes.Specific, 0m, 0m, 9.48m);
		}

		[TestDate(2024, 05, 25)]
		public void TestProcess2()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRate.Codes.EXS, rateType1.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);

			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", ZDateTime.Today, ZDateTime.MaxSmallDateTime);
			universalHelper.CreateRate(tariff1, rateCode1.PK, ZDateTime.Today.AddDays(-5), ZDateTime.MaxSmallDateTime, "0.4*VFD", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);

			CreateCACClassHeader(Factory, "00000001", ZDateTime.Today, ZDateTime.Today.AddDays(10));
			CreateCACClassHeader(Factory, "00000001", ZDateTime.Today.AddDays(10), ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var logger = new SimpleLogger();
			var processor = new RefTariffSynchronizeProcessor(logger);
			processor.Process();
			AssertEquals(@"Start Tariff Synchronization processing
Successfully synchronized 1 Tariff.
", logger.ToString());
			var date = new ZDateTime(2024, 10, 04);
			var classHeaders = Factory.Load<CACClassHeader>(new ZQuery { OrderBy = CACClassHeader.Schema.ZA_ExpiryDate });
			AssertEquals(classHeaders.Length, 3);
			AssertEquals(classHeaders[0].ZA_ExpiryDate, ZDateTime.Today.AddDays(10));
			AssertEquals(classHeaders[1].ZA_ExpiryDate, date.AddDays(-1));
			AssertEquals(classHeaders[2].ZA_EffectiveDate, date);
			AssertEquals(classHeaders[2].ZA_ExpiryDate, ZDateTime.MaxSmallDateTime);
		}

		[TestDate(2024, 10, 10)]
		public void TestProcess3()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRate.Codes.EXS, rateType1.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);

			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", new ZDateTime(2024, 10, 09), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateRate(tariff1, rateCode1.PK, new ZDateTime(2024, 10, 09), ZDateTime.MaxSmallDateTime, "0.4*VFD", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);

			CreateCACClassHeader(Factory, "00000001", new ZDateTime(2024, 10, 04), new ZDateTime(2024, 10, 14));
			Factory.Save();
			var logger = new SimpleLogger();
			var processor = new RefTariffSynchronizeProcessor(logger);
			processor.Process();
			AssertEquals(@"Start Tariff Synchronization processing
Successfully synchronized 1 Tariff.
", logger.ToString());

			var classHeaders = Factory.Load<CACClassHeader>(new ZQuery { OrderBy = CACClassHeader.Schema.ZA_ExpiryDate });
			AssertEquals(classHeaders.Length, 2);
			AssertEquals(classHeaders[0].ZA_ExpiryDate, new ZDateTime(2024, 10, 08));
			AssertEquals(classHeaders[1].ZA_EffectiveDate, new ZDateTime(2024, 10, 09));
			AssertEquals(classHeaders[1].ZA_ExpiryDate, ZDateTime.MaxSmallDateTime);
		}

		[TestDate(2024, 10, 10)]
		public void TestProcess4()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			var rateType1 = universalHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Canada, Universal.Constants.RateTypes.Excise);
			var rateCode1 = universalHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRate.Codes.EXS, rateType1.PK);
			var preference1 = universalHelper.CreatePreferenceForCountry("01", "Preference 01", Core.Constants.CountryCodes.Canada);

			var tariff1 = universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", new ZDateTime(2024, 10, 04), ZDateTime.MaxSmallDateTime);
			universalHelper.CreateRate(tariff1, rateCode1.PK, new ZDateTime(2024, 10, 09), ZDateTime.MaxSmallDateTime, "0.4*VFD", preference1.PK, dataGrouping: Core.Constants.CountryCodes.Canada);

			var classHeader = CreateCACClassHeader(Factory, "00000001", new ZDateTime(2024, 10, 04), ZDateTime.MaxSmallDateTime);
			CreateCACRateHeader(Factory, classHeader.PK, CACRateHeader.RateType.ExciseDutyRate, new ZDateTime(2024, 10, 04), ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var logger = new SimpleLogger();
			var processor = new RefTariffSynchronizeProcessor(logger);
			processor.Process();
			AssertEquals(@"Start Tariff Synchronization processing
Successfully synchronized 1 Tariff.
", logger.ToString());
			var classHeaders = Factory.Load<CACClassHeader>(new ZQuery { OrderBy = CACClassHeader.Schema.ZA_ExpiryDate });
			AssertEquals(1, classHeaders.Length);
			AssertEquals(classHeaders[0].ZA_ExpiryDate, ZDateTime.MaxSmallDateTime);
			AssertEquals(classHeaders[0].ExciseDutyRates.Count, 2);
			var rate1 = classHeaders[0].ExciseDutyRates.Cast<CACRateHeader>().FirstOrDefault(r => r.ZB_EffectiveDate == new ZDateTime(2024, 10, 04));
			AssertEquals(rate1.ZB_ExpiryDate, new ZDateTime(2024, 10, 08));
			var rate2 = classHeaders[0].ExciseDutyRates.Cast<CACRateHeader>().FirstOrDefault(r => r.ZB_EffectiveDate == new ZDateTime(2024, 10, 09));
			AssertEquals(rate2.ZB_ExpiryDate, ZDateTime.MaxSmallDateTime);
		}

		[TestDate(2025, 01, 03)]
		public void TestProcess5()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", new ZDateTime(2024, 10, 04), ZDateTime.MaxSmallDateTime);
			universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000002", new ZDateTime(2025, 10, 04), ZDateTime.MaxSmallDateTime);
			universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000003", new ZDateTime(2024, 10, 04), new ZDateTime(2024, 11, 04));
			Factory.Save();
			var logger = new SimpleLogger();
			var processor = new RefTariffSynchronizeProcessor(logger);
			processor.Process();
			AssertEquals(@"Start Tariff Synchronization processing
Successfully synchronized 1 Tariff.
", logger.ToString());
		}

		[TestDate(2025, 01, 03)]
		public void TestProcessWillCheckLastHarmonizedTariffCount()
		{
			var universalHelper = new UniversalReferenceTestDataHelper(Factory);
			var harmonizedTariffType = universalHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem);
			universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000001", new ZDateTime(2024, 10, 04), ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var logger = new SimpleLogger();
			var processor = new RefTariffSynchronizeProcessor(logger);
			processor.Process();
			AssertEquals(@"Start Tariff Synchronization processing
Successfully synchronized 1 Tariff.
", logger.ToString());

			logger = new SimpleLogger();
			processor = new RefTariffSynchronizeProcessor(logger);
			processor.Process();
			AssertEquals(@"Global Tariff count has not changed, Tariff Synchronization skip.
", logger.ToString());

			universalHelper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Canada, harmonizedTariffType.PK, "00000002", new ZDateTime(2024, 10, 04), ZDateTime.MaxSmallDateTime);
			Factory.Save();
			logger = new SimpleLogger();
			processor = new RefTariffSynchronizeProcessor(logger);
			processor.Process();
			AssertEquals(@"Start Tariff Synchronization processing
Successfully synchronized 2 Tariff.
", logger.ToString());
		}

		CACClassHeader CreateCACClassHeader(BusinessObjectFactory factory, ZString tariffNum, ZDateTime effectiveTime, ZDateTime expiryDate)
		{
			var classHeader = factory.New<CACClassHeader>();
			classHeader.ZA_ClassificationNumber = tariffNum;
			classHeader.ZA_AreaCode = "900";
			classHeader.ZA_EffectiveDate = effectiveTime;
			classHeader.ZA_ExpiryDate = expiryDate;
			classHeader.ZA_ExchangeDateDeterminationFlag = false;
			classHeader.ZA_InactiveInd = false;
			classHeader.ZA_PermitInd = false;
			classHeader.ZA_QuotaInd = false;
			return classHeader;
		}

		CACRateHeader CreateCACRateHeader(BusinessObjectFactory factory,ZGuid classHeaderPK, string type, ZDateTime effectiveDate, ZDateTime expiryDate)
		{
			var rateHeader = factory.New<CACRateHeader>();
			rateHeader.ZB_ZA_ClassHeader = classHeaderPK;
			rateHeader.ZB_RateType = type;
			rateHeader.ZB_EffectiveDate = effectiveDate;
			rateHeader.ZB_ExpiryDate = expiryDate;
			return rateHeader;
		}

		void AssertClassHeader(CACClassHeader classHeader, string classNum, string areaCode, ZDateTime effectiveDate, ZDateTime expiryDate, bool exDateDet, bool inactive, bool permit, bool quota)
		{
			AssertEquals("ZA_ClassificationNumber", classNum, classHeader.ZA_ClassificationNumber);
			AssertEquals("ZA_AreaCode", areaCode, classHeader.ZA_AreaCode);
			AssertEquals("ZA_EffectiveDate", effectiveDate, classHeader.ZA_EffectiveDate);
			AssertEquals("ZA_ExpiryDate", expiryDate, classHeader.ZA_ExpiryDate);
			AssertEquals("ZA_ExchangeDateDeterminationFlag", exDateDet, classHeader.ZA_ExchangeDateDeterminationFlag);
			AssertEquals("ZA_InactiveInd", inactive, classHeader.ZA_InactiveInd);
			AssertEquals("ZA_PermitInd", permit, classHeader.ZA_PermitInd);
			AssertEquals("ZA_QuotaInd", quota, classHeader.ZA_QuotaInd);
		}

		void AssertRateHeader(CACRateHeader classRateHeader, ZDateTime effectiveDate, ZDateTime expiryDate, bool free, string uom, bool inactive)
		{
			AssertEquals("ZB_EffectiveDate", effectiveDate, classRateHeader.ZB_EffectiveDate);
			AssertEquals("ZB_ExpiryDate", expiryDate, classRateHeader.ZB_ExpiryDate);
			AssertEquals("ZB_FreeInd", free, classRateHeader.ZB_FreeInd);
			AssertEquals("ZB_UnitOfMeasure", uom, classRateHeader.ZB_UnitOfMeasure);
			AssertEquals("ZB_Inactive", inactive, classRateHeader.ZB_Inactive);
		}

		void AssertRate(CACRate rate, string treatmentCode, bool isFree, bool assertHasFreeLineOnly)
		{
			AssertEquals("ZC_TreatmentCode", treatmentCode, rate.ZC_TreatmentCode);
			AssertEquals("ZC_FreeInd", isFree, rate.ZC_FreeInd);
			AssertEquals("ZC_Inactive", false, rate.ZC_Inactive);

			if (assertHasFreeLineOnly)
			{
				AssertEquals("RateLines count", 1, rate.RateLines.Count);
				AssertRateLine(rate.RateLines[0], RateTypes.Codes.Free, 0m, 0m, 0m);
			}
		}

		void AssertRateLine(CACRateLine line, string type, ZDecimal regular, ZDecimal min, ZDecimal max)
		{
			AssertEquals("ZR_DutyRateType", type, line.ZR_DutyRateType);
			AssertEquals("ZR_DutyRateRegular", regular, line.ZR_DutyRateRegular);
			AssertEquals("ZR_DutyRateMin", min, line.ZR_DutyRateMin);
			AssertEquals("ZR_DutyRateMax", max, line.ZR_DutyRateMax);
		}
	}
}
