using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415DeclarationTypeGoodsInformationProviderTest : DataProviderTestCase<IM413AndIM415DeclarationTypeGoodsInformationProvider>
	{
		public void TestGrossMass()
		{
			SetUpTestData();
			AssertEquals("GrossMass", 0m, Provider.GrossMass);
		}

		public void TestTotalPackageNumber()
		{
			SetUpTestData();
			declaration.JE_TotalNoOfPacks = 22;
			AssertEquals("TotalPackageNumber", "22", Provider.TotalPackageNumber);
		}

		protected override IM413AndIM415DeclarationTypeGoodsInformationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415DeclarationTypeGoodsInformationProvider(declaration);
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
