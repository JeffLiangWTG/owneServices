using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	class GenerateARInvoiceToEdocsProcessorCreatorTest : TestCaseWithFactory
	{
		public void TestGenerateARInvoiceToEdocsProcessor()
		{
			AssertNull("GenerateARInvoiceToEdocsProcessorCreator should not create a Processor for a null provider", GenerateARInvoiceToEdocsProcessorCreator.GenerateARInvoiceToEdocsProcessor(null));
			AssertNull("GenerateARInvoiceToEdocsProcessorCreator should not create a Processor for an APInvoice", GenerateARInvoiceToEdocsProcessorCreator.GenerateARInvoiceToEdocsProcessor(Factory.New<APInvoice>()));
			AssertNotNull("GenerateARInvoiceToEdocsProcessorCreator should create a Processor for an ARInvoice", GenerateARInvoiceToEdocsProcessorCreator.GenerateARInvoiceToEdocsProcessor(Factory.New<ARInvoice>()));
			AssertNotNull("GenerateARInvoiceToEdocsProcessorCreator should create a Processor for an ARCreditNote", GenerateARInvoiceToEdocsProcessorCreator.GenerateARInvoiceToEdocsProcessor(Factory.New<ARCreditNote>()));
			AssertNotNull("GenerateARInvoiceToEdocsProcessorCreator should create a Processor for an ARAdjustmentNote", GenerateARInvoiceToEdocsProcessorCreator.GenerateARInvoiceToEdocsProcessor(Factory.New<ARAdjustmentNote>()));
		}

		#region Implementation

		GenerateARInvoiceToEdocsProcessorCreator GenerateARInvoiceToEdocsProcessorCreator
		{
			get { return new GenerateARInvoiceToEdocsProcessorCreator(); }
		}

		#endregion
	}
}