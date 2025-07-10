using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class IM432HeaderProviderTest : DataProviderTestCase<IM432HeaderProvider>
	{
		public void TestInterfaces()
		{
			Assert("Should implement IIM432Header", Provider is IIM432Header);
			Assert("Should implement IIM432Operation", Provider is IIM432Operation);
		}

		public void TestImportOperation()
		{
			AssertSame(Provider, Provider.ImportOperation);
		}

		public void TestLRN()
		{
			SetUpTestData();
			entryHeader.CH_BGMReference = "LRN001";
			AssertEquals("LRN001", Provider.LRN);
		}

		public void TestMRN()
		{
			SetUpTestData();
			entryHeader.MovementReferenceNumberSetter("12MRN345ABCDE678R9");
			AssertEquals("12MRN345ABCDE678R9", Provider.MRN);
		}

		public void TestGoodsShipment()
		{
			SetUpTestData();
			IIM432Header provider = Provider;
			var goodsShipment = provider.GoodsShipment;
			AssertType<IM432GoodsShipmentProvider>(goodsShipment);
			AssertSame("Cached", goodsShipment, provider.GoodsShipment);
		}

		protected override IM432HeaderProvider GetProvider()
		{
			SetUpTestData();
			return new IM432HeaderProvider(new AISMessageSendingAction(entryHeader));
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
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
	}
}
