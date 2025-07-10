using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class CommonJobComInvoiceHeaderValidationBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestShouldCheckMissingPreviousDocuments()
		{
			var parent = Factory.New<JobComInvoiceHeader>();
			var validation = new CommonJobComInvoiceHeaderValidationForTest(parent);
			AssertEquals("ShouldCheckMissingPreviousDocuments", false, validation.ShouldCheckMissingPreviousDocuments);
		}

		class CommonJobComInvoiceHeaderValidationForTest : CommonJobComInvoiceHeaderValidation
		{
			public CommonJobComInvoiceHeaderValidationForTest(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
			{
			}

			public new bool ShouldCheckMissingPreviousDocuments => base.ShouldCheckMissingPreviousDocuments;
		}
	}
}
