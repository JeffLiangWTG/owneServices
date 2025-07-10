using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NonPersistentCusContainer = Enterprise.Customs.Business.NonPersistentCusContainer;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	class IM432GoodsShipmentProviderTest : DataProviderTestCase<IM432GoodsShipmentProvider>
	{
		public void TestMRN()
		{
			entryHeader.MovementReferenceNumberSetter("MRNIE24ROS1232134");
			AssertEquals("MRNIE24ROS1232134", Provider.MRN);
		}

		public void TestDocumentsAuthorisations_WithAggregation()
		{
			entryInstruction.PreviousDocuments.AddNew().CSI_Code = "C1";
			entryInstruction.PreviousDocuments.AddNew().CSI_Code = "C2";

			entryLine.CL_LineNumber = 1;
			invoiceLine.PreviousDocuments.AddNew().CSI_Code = "C11";
			invoiceLine.PreviousDocuments.AddNew().CSI_Code = "C12";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			invoiceLine2.PreviousDocuments.AddNew().CSI_Code = "C11";
			invoiceLine2.PreviousDocuments.AddNew().CSI_Code = "C22";
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;

			CombineAssertions("DocumentsAuthorisations With Aggregation", () =>
			{
				AssertEquals("Header should have 3 items.", 3, Provider.DocumentsAuthorisations.Count);
				AssertEquals("Header should have C1(from header)", true, Provider.DocumentsAuthorisations.Any(doc => doc.PreviousDocumentType == "C1"));
				AssertEquals("Header should have C2(from header)", true, Provider.DocumentsAuthorisations.Any(doc => doc.PreviousDocumentType == "C2"));
				AssertEquals("Header should have C11(from item)", true, Provider.DocumentsAuthorisations.Any(doc => doc.PreviousDocumentType == "C11"));

				var item1 = Provider.GoodsShipmentItems.First(item => item.GoodsItemNumber == "1");
				AssertEquals("Item 1 D.A. should have 1 items.", 1, item1.DocumentsAuthorisations.Count);
				AssertEquals("Item 1 D.A. should have C11.", "C12", item1.DocumentsAuthorisations.First().PreviousDocumentType);

				var item2 = Provider.GoodsShipmentItems.First(item => item.GoodsItemNumber == "2");
				AssertEquals("Item 2 D.A. should have 2 items.", 1, item2.DocumentsAuthorisations.Count);
				AssertEquals("Item 2 D.A. should have C22.", "C22", item2.DocumentsAuthorisations.First().PreviousDocumentType);
			});
		}

		public void TestGoodsLocation()
		{
			AssertType<GoodsLocationProvider>("Type of GoodsLocation", Provider.GoodsLocation);
		}

		public void TestContainerIds()
		{
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CNT001";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CNT002";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CNT003";

			entryLine.CL_LineNumber = 1;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Single(pack => pack.ContainerNumber.Contains("CNT001")).IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Single(pack => pack.ContainerNumber.Contains("CNT003")).IsForInvoiceLine = true;

			invoiceLine2.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Single(pack => pack.ContainerNumber.Contains("CNT002")).IsForInvoiceLine = true;
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Single(pack => pack.ContainerNumber.Contains("CNT003")).IsForInvoiceLine = true;

			CombineAssertions("ContainerIds: with aggregation among Header and Items.", () =>
			{
				AssertEquals("1 Container ID in Header level", 1, Provider.ContainerIds.Count);
				AssertEquals("Header level container number: the container linked to both the lines.", "CNT003", Provider.ContainerIds.Single());

				var item1 = Provider.GoodsShipmentItems.First(item => item.GoodsItemNumber == "1");
				AssertEquals("1 Container ID in Item 1", 1, item1.ContainerId.Count);
				AssertEquals("Item level container number: the container associated with line 1.", "CNT001", item1.ContainerId.Single());

				var item2 = Provider.GoodsShipmentItems.First(item => item.GoodsItemNumber == "2");
				AssertEquals("1 Container ID in Item 2", 1, item2.ContainerId.Count);
				AssertEquals("Item level container number: the container associated with line 2.", "CNT002", item2.ContainerId.Single());
			});
		}

		public void TestGoodsShipmentItems()
		{
			AssertEquals("GoodsShipmentItem count 1", 1, Provider.GoodsShipmentItems.Count);
			AssertType<IM432GoodsShipmentItemProvider>("GoodsShipmentItem type", Provider.GoodsShipmentItems.Single());
		}

		protected override void SetUp() => SetUpTestDataIfNeeded();

		protected override IM432GoodsShipmentProvider GetProvider() => new IM432GoodsShipmentProvider(new EntryHeaderWrapper(entryHeader));

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
