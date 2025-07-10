using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	class SendARInvoiceProcessorCreatorTest : TestCaseWithFactory
	{
		public void TestCreateSendARInvoiceProcessor()
		{
			AssertNull("SendARInvoiceProcessorCreator should not create a Processor for a null provider", SendARInvoiceProcessorCreator.CreateSendARInvoiceProcessor(null));
			AssertNull("SendARInvoiceProcessorCreator should not create a Processor for an APInvoice", SendARInvoiceProcessorCreator.CreateSendARInvoiceProcessor(Factory.New<APInvoice>()));
			AssertNotNull("SendARInvoiceProcessorCreator should create a Processor for an ARInvoice", SendARInvoiceProcessorCreator.CreateSendARInvoiceProcessor(Factory.New<ARInvoice>()));
			AssertNotNull("SendARInvoiceProcessorCreator should create a Processor for an ARCreditNote", SendARInvoiceProcessorCreator.CreateSendARInvoiceProcessor(Factory.New<ARCreditNote>()));
			AssertNotNull("SendARInvoiceProcessorCreator should create a Processor for an ARAdjustmentNote", SendARInvoiceProcessorCreator.CreateSendARInvoiceProcessor(Factory.New<ARAdjustmentNote>()));
		}

		#region Implementation

		SendARInvoiceProcessorCreator SendARInvoiceProcessorCreator
		{
			get { return new SendARInvoiceProcessorCreator(); }
		}

		#endregion
	}
}