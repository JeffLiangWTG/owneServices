using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DutyDataWrapperTest : TestCaseWithFactory
	{
		public void TestReadOnly()
		{
			AssertEquals("Always false", false, new DutyDataWrapper(null).ReadOnly);
		}

		public void TestRandomLineDutyData()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Import);
				var impTariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "9876543210", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
				helper.CreateTariffUOM(impTariff, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");

				var importClassification = Factory.New<Classification>();
				importClassification.CC_TariffNum = "9876.54.32 10";
				importClassification.CC_ClassificationType = Classification.ClassificationType.IMP;
				importClassification.AddInfo.ZA_ORG = "US";
				importClassification.AddInfo.ZA_TreatmentCode_Hidden = "XXX";
				importClassification.AddInfo.ZA_TR2 = "2TR";
				importClassification.AddInfo.ZA_CL2 = "1234.56.78";

				importClassification.AddInfo.ZA_GSTE = "TTT";
				importClassification.AddInfo.ZA_LCTQ = "Y";
				importClassification.AddInfo.ZA_WETQ = "Y";
				importClassification.AddInfo.ZA_TreatmentCode_Hidden = "RRR";
				importClassification.AddInfo.ZA_UQ2 = "L";
				importClassification.AddInfo.ZA_RNO = "001";

				var wrapper = new DutyDataWrapper(importClassification);

				CombineAssertions(() =>
				{
					AssertEquals("Effective Duty Date", true, wrapper.RandomLineDutyData.EffectiveDutyDate.Date == ZDateTime.Today);
					AssertEquals("Stat Code", "10", wrapper.RandomLineDutyData.StatCode);
					AssertEquals("IsGSTExempt", false, wrapper.RandomLineDutyData.IsGSTExempt);
					AssertEquals("IsLCTPayable", true, wrapper.RandomLineDutyData.IsLCTPayable);
					AssertEquals("IsLCTExempt", true, wrapper.RandomLineDutyData.IsLCTExempt);
					AssertEquals("IsWETExempt", true, wrapper.RandomLineDutyData.IsWETExempt);
					AssertEquals("FirstTariffNumber", "98765432", wrapper.RandomLineDutyData.FirstTariffNumber);
					AssertEquals("FirstTreatmentCode", "RRR", wrapper.RandomLineDutyData.FirstTreatmentCode);
					AssertEquals("FirstUQ", "KG", wrapper.RandomLineDutyData.FirstUQ);
					AssertEquals("SecondUQ", "L", wrapper.RandomLineDutyData.SecondUQ);
					AssertEquals("Preference", "GEN", wrapper.RandomLineDutyData.Preference);
					AssertEquals("RateNumber", "001", wrapper.RandomLineDutyData.RateNumber);
					AssertEquals("SecondTariffNumber", "12345678", wrapper.RandomLineDutyData.SecondTariffNumber);
					AssertEquals("SecondTreatmentCode", "2TR", wrapper.RandomLineDutyData.SecondTreatmentCode);
					AssertEquals("TreatmentRateNumber", "", wrapper.RandomLineDutyData.TreatmentRateNumber);
				});
			}
		}

		public void TestRandomLineDutyData_Old()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var snapshot = CMRStatisticalClassificationPeriodSnapshot.Load(Factory, "2208.30.00 75", "", ZDateTime.Today);
				if (snapshot == null)
				{
					snapshot = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
					snapshot.SC_TariffClassificationNumber = "22083000";
					snapshot.SC_StatisticalClassificationCode = "75";
					snapshot.SC_QuantityUnit = "LA";
					snapshot.SC_StartDate = ZDateTime.Today.AddYears(-50);
					Factory.Save();
				}

				var importClassification = Factory.New<Classification>();
				importClassification.CC_TariffNum = snapshot.SC_TariffClassificationNumber + " " + snapshot.SC_StatisticalClassificationCode;
				importClassification.CC_ClassificationType = Classification.ClassificationType.IMP;
				importClassification.AddInfo.ZA_ORG = "US";
				importClassification.AddInfo.ZA_TreatmentCode_Hidden = "XXX";
				importClassification.AddInfo.ZA_TR2 = "2TR";
				importClassification.AddInfo.ZA_CL2 = "1234.56.78";

				importClassification.AddInfo.ZA_GSTE = "TTT";
				importClassification.AddInfo.ZA_LCTQ = "Y";
				importClassification.AddInfo.ZA_WETQ = "Y";
				importClassification.AddInfo.ZA_TreatmentCode_Hidden = "RRR";
				importClassification.AddInfo.ZA_UQ2 = "L";
				importClassification.AddInfo.ZA_RNO = "001";

				var wrapper = new DutyDataWrapper(importClassification);

				CombineAssertions(() =>
				{
					AssertEquals("Effective Duty Date", true, wrapper.RandomLineDutyData.EffectiveDutyDate.Date == ZDateTime.Today);
					AssertEquals("Stat Code", "75", wrapper.RandomLineDutyData.StatCode);
					AssertEquals("IsGSTExempt", false, wrapper.RandomLineDutyData.IsGSTExempt);
					AssertEquals("IsLCTPayable", true, wrapper.RandomLineDutyData.IsLCTPayable);
					AssertEquals("IsLCTExempt", true, wrapper.RandomLineDutyData.IsLCTExempt);
					AssertEquals("IsWETExempt", true, wrapper.RandomLineDutyData.IsWETExempt);
					AssertEquals("FirstTariffNumber", "22083000", wrapper.RandomLineDutyData.FirstTariffNumber);
					AssertEquals("FirstTreatmentCode", "RRR", wrapper.RandomLineDutyData.FirstTreatmentCode);
					AssertEquals("FirstUQ", "LA", wrapper.RandomLineDutyData.FirstUQ);
					AssertEquals("SecondUQ", "L", wrapper.RandomLineDutyData.SecondUQ);
					AssertEquals("Preference", "GEN", wrapper.RandomLineDutyData.Preference);
					AssertEquals("RateNumber", "001", wrapper.RandomLineDutyData.RateNumber);
					AssertEquals("SecondTariffNumber", "12345678", wrapper.RandomLineDutyData.SecondTariffNumber);
					AssertEquals("SecondTreatmentCode", "2TR", wrapper.RandomLineDutyData.SecondTreatmentCode);
					AssertEquals("TreatmentRateNumber", "", wrapper.RandomLineDutyData.TreatmentRateNumber);
				});
			}
		}
	}
}
