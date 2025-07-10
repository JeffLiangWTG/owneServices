using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415HeaderProviderTest : DataProviderTestCase<IM413AndIM415HeaderProvider>
	{
		public void TestIIM415Header()
		{
			Assert("Should implement IIM415Header", Provider is IIM413AndIM415Header);
		}

		public void TestMessageDeclarationType()
		{
			SetUpTestData();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			AssertEquals("MessageDeclarationType", "H1", Provider.MessageDeclarationType);
		}

		public void TestHasRequestedProcedure71()
		{
			SetUpTestData();
			invoiceLine.JI_Procedure = "210000";
			AssertEquals("HasRequestedProcedure71", false, Provider.HasRequestedProcedure71);

			invoiceLine.JI_Procedure = "710000";
			AssertEquals("HasRequestedProcedure71", true, Provider.HasRequestedProcedure71);
		}

		public void TestDeclaration()
		{
			SetUpTestData();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			AssertEquals("MessageType", "H1", Provider.Declaration.MessageType);
		}

		public void TestGoodsShipment()
		{
			var goodsShipment = Provider.GoodsShipment;
			AssertType<IM413AndIM415GoodsShipmentProvider>(goodsShipment);
			AssertSame("Cached", goodsShipment, Provider.GoodsShipment);
		}

		protected override IM413AndIM415HeaderProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415HeaderProvider(new AISUCC5MessageSendingAction(entryHeader));
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
				entryHeader.MovementReferenceNumberSetter("MRN2343234242");
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
