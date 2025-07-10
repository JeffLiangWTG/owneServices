using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM414HeaderProviderTest : DataProviderTestCase<IM414HeaderProvider>
	{
		public void TestIIM414Header()
		{
			Assert("Should implement IIM414Header", Provider is IIM414Header);
		}

		public void TestDeclaration()
		{
			var declarant = Provider.Declaration;
			AssertType<IM414DeclarationTypeProvider>(declarant);
			AssertSame("Cached", declarant, Provider.Declaration);
		}

		protected override IM414HeaderProvider GetProvider()
		{
			SetUpTestData();
			return new IM414HeaderProvider(new AISUCC5MessageSendingAction(entryHeader));
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
