using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class IM432GoodsShipmentProviderTest : DataProviderTestCase<IM432GoodsShipmentProvider>
	{
		public void TestIIM432GoodsShipment()
		{
			Assert("Should implement IIM432GoodsShipment", Provider is IIM432GoodsShipment);
		}

		public void TestPreviousDocuments()
		{
			SetUpTestData();
			entryInstruction.PreviousDocuments.AddNew();
			entryInstruction.PreviousDocuments.AddNew();
			var previousDocuments = Provider.PreviousDocuments;
			AssertEquals("Count", 2, previousDocuments.Count);
			Assert("Should be IReadOnlyCollection<DocumentProvider>", previousDocuments is IReadOnlyCollection<DocumentProvider>);
		}

		public void TestConsignment()
		{
			var consignment = Provider.Consignment;
			Assert("Should be MConsignment02Provider", consignment is MConsignment02Provider);
			AssertSame("Cached", consignment, Provider.Consignment);
		}

		protected override IM432GoodsShipmentProvider GetProvider()
		{
			SetUpTestData();
			return new IM432GoodsShipmentProvider(entryHeaderWrapper);
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
				entryHeaderWrapper = new EntryHeaderWrapper(entryHeader);
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		EntryHeaderWrapper entryHeaderWrapper;
	}
}
