using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.Testing
{
	class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_Description()
		{
			invoiceLine.JI_Description = "123ABC";
			AssertNoWarning(invoiceLine.JI_DescriptionInfo, "The Description has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");

			invoiceLine.JI_Description = "123ABC– ";
			AssertHasWarning(invoiceLine.JI_DescriptionInfo, "The Description has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs.");
		}

		public void TestCheckJI_WeightUQ()
		{
			invoiceLine.GACPGAHeader.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;

			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilotonnes;
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.Validation.ValidateJI_WeightUQ();
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "Unit of Measure must be KGM if PGA - Global Affairs Canada is being reported.");

			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			invoiceLine.Validation.ValidateJI_WeightUQ();
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "Unit of Measure must be KGM if PGA - Global Affairs Canada is being reported.");

			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilotonnes;
			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			invoiceLine.Validation.ValidateJI_WeightUQ();
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightUQInfo, "Unit of Measure must be KGM if PGA - Global Affairs Canada is being reported.");
		}

		public void TestParent()
		{
			AssertEquals(invoiceLine.Validation.Parent, invoiceLine);
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
	}
}
