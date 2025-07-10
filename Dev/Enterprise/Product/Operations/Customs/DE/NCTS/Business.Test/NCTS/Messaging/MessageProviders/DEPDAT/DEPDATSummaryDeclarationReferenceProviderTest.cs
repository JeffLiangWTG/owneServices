using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DEPDATSummaryDeclarationReferenceProviderTest : Customs.Business.Testing.DataProviderTestCase<IDEPDATSummaryDeclarationReference>
	{
		public void TestType()
		{
			previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
			AssertEquals(PreviousDocSubTypeList.Codes.AWB, Provider.Type);
		}

		public void TestReferenceNumber()
		{
			previousProcedure.CSI_ReferenceNumber = "REFNUMBER";
			AssertEquals("REFNUMBER", Provider.ReferenceNumber);
		}

		public void TestIdentificationNumber()
		{
			previousProcedure.CSI_ReferenceNumber2 = "REFNUMBER2";
			AssertEquals("REFNUMBER2", Provider.IdentificationNumber);
		}

		public void TestMRN()
		{
			CombineAssertions(() =>
			{
				previousProcedure.CSI_ReferenceNumber = "23DE1234567890";
				AssertEquals("MRN", "23DE1234567890", Provider.MRN);
				AssertNull("RegistrationNumber", Provider.RegistrationNumber);
			});
		}

		public void TestRegistrationNumber()
		{
			CombineAssertions(() =>
			{
				previousProcedure.CSI_ReferenceNumber = "AB23DE1234567890";
				AssertEquals("RegistrationNumber", "AB23DE1234567890", Provider.RegistrationNumber);
				AssertNull("MRN", Provider.MRN);
			});
		}

		public void TestGoodsItemNumber()
		{
			previousProcedure.CSI_LineNo = 15;
			AssertEquals(15, Provider.GoodsItemNumber);
		}

		public void TestNumberOfPackages()
		{
			previousProcedure.CSI_Quantity = 17;
			AssertEquals(17, Provider.NumberOfPackages);
		}

		public void TestIsIdentificationByKey()
		{
			CombineAssertions(() =>
			{
				previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
				AssertEquals("CSI_SubType AWB", true, Provider.IsIdentificationByKey);

				previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.ULD;
				AssertEquals("CSI_SubType ULD", true, Provider.IsIdentificationByKey);

				previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				AssertEquals("CSI_SubType REG", false, Provider.IsIdentificationByKey);
			});
		}

		public void TestIsIdentificationByRegistrationNumber()
		{
			CombineAssertions(() =>
			{
				previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.AWB;
				AssertEquals("CSI_SubType AWB", false, Provider.IsIdentificationByRegistrationNumber);

				previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.ULD;
				AssertEquals("CSI_SubType ULD", false, Provider.IsIdentificationByRegistrationNumber);

				previousProcedure.CSI_SubType = PreviousDocSubTypeList.Codes.REG;
				AssertEquals("CSI_SubType REG", true, Provider.IsIdentificationByRegistrationNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			previousProcedure = Factory.New<NctsPreviousDocument>();
			previousProcedure.CSI_Procedure = NctsPreviousProcedureList.Codes._N337;
		}

		protected override IDEPDATSummaryDeclarationReference GetProvider() => DEPDATSummaryDeclarationReferenceProvider.NewOrNull(previousProcedure);

		NctsPreviousDocument previousProcedure;
	}
}
