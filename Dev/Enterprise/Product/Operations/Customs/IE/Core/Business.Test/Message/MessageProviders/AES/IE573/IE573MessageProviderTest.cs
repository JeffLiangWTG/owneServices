using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class IE573MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE573MessageProvider>
	{
		public void TestExportOperation()
		{
			AssertSame("ExportOperation", Provider, Provider.ExportOperation);
		}

		public void TestConsignment()
		{
			AssertType<IE570And573CommonConsignmentProvider>("Consignment", Provider.Consignment);
		}

		#region IIE570ExportOperation Members

		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter("MRN2343234242");
			AssertEquals("MRN", "MRN2343234242", Provider.MRN);
		}

		public void TestStoringFlag()
		{
			AssertEquals("StoringFlag", "0", Provider.StoringFlag);
		}

		#endregion

		protected override IE573MessageProvider GetProvider() => new IE573MessageProvider(entryHeader);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
