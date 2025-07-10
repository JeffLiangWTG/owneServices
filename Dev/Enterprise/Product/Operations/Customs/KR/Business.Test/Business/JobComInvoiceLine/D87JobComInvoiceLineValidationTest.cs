using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class D87JobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public new void TestJI_NetWeightUQ()
		{
			invoiceLine.JI_NetWeight = 0;
			invoiceLine.Validation.ValidateJI_NetWeightUQ();
			AssertNoMessageErrors(invoiceLine.JI_NetWeightUQInfo);

			invoiceLine.JI_NetWeight = 1;
			invoiceLine.JI_NetWeightUQ = "";
			AssertNoMessageErrors(invoiceLine.JI_NetWeightUQInfo);

			invoiceLine.JI_NetWeightUQ = "99";
			AssertNoMessageErrors(invoiceLine.JI_NetWeightUQInfo);

			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageErrors(invoiceLine.JI_NetWeightUQInfo);
		}

		public new void TestJI_WeightUQ()
		{
			invoiceLine.JI_Weight = 0;
			invoiceLine.Validation.ValidateJI_WeightUQ();
			AssertNoMessageErrors(invoiceLine.JI_WeightUQInfo);

			invoiceLine.JI_Weight = 1;
			invoiceLine.JI_WeightUQ = "";
			AssertNoMessageErrors(invoiceLine.JI_WeightUQInfo);

			invoiceLine.JI_WeightUQ = "99";
			AssertNoMessageErrors(invoiceLine.JI_WeightUQInfo);

			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			AssertNoMessageErrors(invoiceLine.JI_WeightUQInfo);
		}

		public new void TestCheckJI_Description()
		{
			invoiceLine.Validation.ValidateJI_Description();
			AssertNoMessageErrors(invoiceLine.JI_DescriptionInfo);

			invoiceLine.JI_Description = "Test Model";
			AssertNoMessageErrors(invoiceLine.JI_DescriptionInfo);
		}

		public void TestNoMessageErrorsForUnusedFieldsOfD87()
		{
			invoiceLine.JI_LinePrice = 1;
			invoiceLine.RunPreSaveValidation();
			Assert(!invoiceLine.HasMessageErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;

			invoiceLine = declaration.InvoiceLines[0];
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
