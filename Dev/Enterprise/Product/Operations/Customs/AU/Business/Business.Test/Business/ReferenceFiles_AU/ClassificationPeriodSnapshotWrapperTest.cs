using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ClassificationPeriodSnapshotWrapper))]
	sealed class ClassificationPeriodSnapshotWrapperTest : TestCaseWithFactory
	{
		public void TestWrappedProperties()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Universal.Constants.TariffTypes.Import);
				var testImpTariffType = helper.CreateNewOrGetExistingTariffType(AUConstants.RefDataGroupCodes.AustraliaTest, Universal.Constants.TariffTypes.Import);
				var impTariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, impTariffType.PK, "1234567890", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
				helper.CreateTariffUOM(impTariff1, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");
				helper.CreateTariffUOM(impTariff1, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "T");
				var impTariff2 = helper.LoadOrCreateNewTariff(AUConstants.RefDataGroupCodes.AustraliaTest, impTariffType.PK, "9876543210", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST");
				helper.CreateTariffUOM(impTariff2, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "LA");
				helper.CreateTariffUOM(impTariff2, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "L");

				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					var wrapper = ClassificationPeriodSnapshotWrapper.Load(Factory, "12345678", "90", ZDateTime.Today);
					AssertEquals("QuantityUnit", "KG", wrapper.QuantityUnit);
					AssertEquals("SecondQuantityUnit", "T", wrapper.SecondQuantityUnit);
				}

				using (AUCustomsDataRegistry.Instance.UseCMRTariffTestData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					var wrapper = ClassificationPeriodSnapshotWrapper.Load(Factory, "98765432", "10", ZDateTime.Today);
					AssertEquals("QuantityUnit", "LA", wrapper.QuantityUnit);
					AssertEquals("SecondQuantityUnit", "L", wrapper.SecondQuantityUnit);
				}
			}
		}

		public void TestWrappedProperties_Old()
		{
			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var statClassification = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
				statClassification.SC_TariffClassificationNumber = "98765432";
				statClassification.SC_StatisticalClassificationCode = "10";
				statClassification.SC_QuantityUnit = "LA";
				statClassification.SC_SecondQuantityUnit = "L";
				statClassification.SC_StartDate = ZDateTime.MinSmallDateTimeValue;

				var wrapper = ClassificationPeriodSnapshotWrapper.Load(Factory, "98765432", "10", ZDateTime.Today);
				AssertEquals("QuantityUnit", "LA", wrapper.QuantityUnit);
				AssertEquals("SecondQuantityUnit", "L", wrapper.SecondQuantityUnit);
			}
		}
	}
}
