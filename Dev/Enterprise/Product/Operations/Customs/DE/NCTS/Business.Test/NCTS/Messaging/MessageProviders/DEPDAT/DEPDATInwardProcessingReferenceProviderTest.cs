using CargoWise.Customs.DE.MessageContracts.NCTS;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DEPDATInwardProcessingReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<IDEPDATInwardProcessingReference>
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

		public void TestGoodsRelatedData()
		{
			AssertEquals("complement", Provider.GoodsRelatedData);
		}

		protected override void SetUp()
		{
			base.SetUp();
			previousProcedure = Factory.NewWithValidTestData<NctsPreviousDocument>();
			previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._9DEY;
			previousProcedure.Status = true;
			previousProcedure.CSI_ReferenceNumber = "23DE12334566";
			previousProcedure.CSI_LineNo = 15;
			previousProcedure.CSI_Description = "complement";
		}

		protected override IDEPDATInwardProcessingReference GetProvider() => DEPDATInwardProcessingReferenceProvider.NewOrNull(previousProcedure);

		NctsPreviousDocument previousProcedure;
	}
}
