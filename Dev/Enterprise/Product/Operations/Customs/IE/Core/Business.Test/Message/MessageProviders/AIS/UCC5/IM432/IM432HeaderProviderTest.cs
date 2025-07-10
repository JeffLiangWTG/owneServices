using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM432HeaderProviderTest : DataProviderTestCase<IM432HeaderProvider>
	{
		public void TestDeclaration()
		{
			AssertType<IM432DeclarationProvider>("Type of Declaration", ((IIM432Header)Provider).Declaration);
		}

		public void TestGoodsShipment()
		{
			AssertType<IM432GoodsShipmentProvider>("Type of GoodsShipment", ((IIM432Header)Provider).GoodsShipment);
		}

		protected override void SetUp() => SetUpTestDataIfNeeded();

		protected override IM432HeaderProvider GetProvider() => new IM432HeaderProvider(new AISUCC5MessageSendingAction(entryHeader));

		void SetUpTestDataIfNeeded()
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
