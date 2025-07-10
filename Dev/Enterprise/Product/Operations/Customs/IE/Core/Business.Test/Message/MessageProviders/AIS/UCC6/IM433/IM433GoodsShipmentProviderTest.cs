using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class IM433GoodsShipmentProviderTest : DataProviderTestCase<IM433GoodsShipmentProvider>
	{
		public void TestIM433GoodsShipment()
		{
			Assert("Should implement IM433GoodsShipment", Provider is IM433GoodsShipment);
		}

		public void TestPreviousDocuments()
		{
			SetUpTestData();
			entryInstruction.PreviousDocuments.AddNew();
			entryInstruction.PreviousDocuments.AddNew();
			var previousDocuments = Provider.PreviousDocuments;
			AssertEquals("Count", 2, previousDocuments.Count);
			Assert("Should be IReadOnlyCollection<ICcQualifierDocument>", previousDocuments is IReadOnlyCollection<ICcQualifierDocument>);
		}

		public void TestConsignment()
		{
			var consignment = Provider.Consignment;
			Assert("Should be MConsignment03Provider", consignment is MConsignment03Provider);
			AssertSame("Cached", consignment, Provider.Consignment);
		}

		public void TestGoodsShipmentItems()
		{
			SetUpTestData();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var goodsShipmentItems = Provider.GoodsShipmentItems;
			AssertEquals(2, goodsShipmentItems.Count);
			var goodsShipmentItem1 = goodsShipmentItems.First();
			CombineAssertions(() =>
			{
				AssertType<IM433GoodsShipmentItemProvider>(goodsShipmentItem1);
			});
		}

		protected override IM433GoodsShipmentProvider GetProvider()
		{
			SetUpTestData();
			return new IM433GoodsShipmentProvider(entryHeaderWrapper);
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
