using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	public class IM483DeclarationProviderTest : DataProviderTestCase<IM483DeclarationTypeProvider>
	{
		public void TestMRN()
		{
			SetUpTestData();
			entryHeader.MovementReferenceNumberSetter("12MRN345ABCDE678R9");
			AssertEquals("12MRN345ABCDE678R9", Provider.MRN);
		}

		public void TestLRN()
		{
			SetUpTestData();
			entryHeader.CH_BGMReference = "LRN001";
			AssertEquals("LRN", "LRN001", Provider.LRN);
		}

		protected override IM483DeclarationTypeProvider GetProvider()
		{
			SetUpTestData();
			return new IM483DeclarationTypeProvider(entryHeader);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
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
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
