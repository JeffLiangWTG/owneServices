using CargoWise.Customs.DE.MessageContracts.NCTS;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DEPDATCustomsWarehousingReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<IDEPDATCustomsWarehousingReference>
	{
		public void TestAccessViaATLAS()
		{
			AssertEquals(true, Provider.AccessViaATLAS);
		}

		public void TestMRN() => CombineAssertions(() =>
		{
			AssertEquals("23DE12334566", Provider.MRN);
			AssertNull(nameof(Provider.RegistrationNumber), Provider.RegistrationNumber);
		});

		public void TestRegistrationNumber() => CombineAssertions(() =>
		{
			previousProcedure.CSI_ReferenceNumber = "23NL12334566";

			AssertEquals("23NL12334566", Provider.RegistrationNumber);
			AssertNull(nameof(Provider.MRN), Provider.MRN);
		});

		public void TestGoodsItemNumber()
		{
			AssertEquals(15, Provider.GoodsItemNumber);
		}

		public void TestUsualTreatment()
		{
			AssertEquals(true, Provider.UsualTreatment);
		}

		public void TestComplement()
		{
			AssertEquals("complement", Provider.Complement);
		}

		public void TestHarmonizedSystemSubheadingCode()
		{
			AssertEquals("123456", Provider.HarmonizedSystemSubheadingCode);
		}

		public void TestCombinedNomenclatureCode()
		{
			AssertEquals("78", Provider.CombinedNomenclatureCode);
		}

		public void TestTaricCode()
		{
			AssertEquals("90", Provider.TaricCode);
		}

		public void TestNationalAdditionalCode()
		{
			AssertEquals("5", Provider.NationalAdditionalCode);
		}

		public void TestGoodsReduction() => CombineAssertions(() =>
		{
			AssertEquals("LTR", Provider.GoodsReduction.MeasurementUnit);
			AssertEquals(string.Empty, Provider.GoodsReduction.Qualifier);
			AssertEquals("22.123", Provider.GoodsReduction.Quantity.ToString());
		});

		public void TestGoodsReductionAfterTreatment() => CombineAssertions(() =>
		{
			AssertEquals("KGM", Provider.GoodsReductionAfterTreatment.MeasurementUnit);
			AssertEquals("A", Provider.GoodsReductionAfterTreatment.Qualifier);
			AssertEquals("18", Provider.GoodsReductionAfterTreatment.Quantity.ToString());
		});

		public void TestGoodsReductionAfterTreatment_NoUsualProcessing()
		{
			previousProcedure.UsualProcessingFlag = false;
			AssertNull(nameof(Provider.GoodsReductionAfterTreatment), Provider.GoodsReductionAfterTreatment);
		}

		protected override void SetUp()
		{
			base.SetUp();
			previousProcedure = Factory.NewWithValidTestData<NctsPreviousDocument>();
			previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEZ;
			previousProcedure.CSI_ReferenceNumber = "23DE12334566";
			previousProcedure.CSI_LineNo = 15;
			previousProcedure.Status = true;
			previousProcedure.UsualProcessingFlag = true;
			previousProcedure.CSI_Description = "complement";
			previousProcedure.CSI_Tariff = "12345678905";
			previousProcedure.CSI_Quantity = 17.9999;
			previousProcedure.CSI_UnitOfQuantity = "KGMA";
			previousProcedure.CSI_Quantity2 = 22.12345;
			previousProcedure.CSI_UnitOfQuantity2 = "LTR";
		}

		protected override IDEPDATCustomsWarehousingReference GetProvider() => DEPDATCustomsWarehousingReferenceProvider.NewOrNull(previousProcedure);

		NctsPreviousDocument previousProcedure;
	}
}
