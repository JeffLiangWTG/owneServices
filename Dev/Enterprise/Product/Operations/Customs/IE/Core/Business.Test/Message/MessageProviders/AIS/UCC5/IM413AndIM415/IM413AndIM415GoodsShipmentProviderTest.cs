using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using NonPersistentCusContainer = Enterprise.Customs.Business.NonPersistentCusContainer;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentProvider>
	{
		public void TestIIM415GoodsShipmentType()
		{
			Assert("Should implement IIM415GoodsShipmentType", Provider is IIM413AndIM415GoodsShipmentType);
		}

		public void TestGoodsShipmentTypeDocumentsAuthorisations()
		{
			SetUpTestData();
			var goodsShipmentDocumentAuthorisations = Provider.GoodsShipmentTypeDocumentsAuthorisations;
			AssertType<IM413AndIM415GoodsShipmentTypeDocumentsAuthorisationsProvider>(goodsShipmentDocumentAuthorisations);
			AssertSame("Cached", goodsShipmentDocumentAuthorisations, Provider.GoodsShipmentTypeDocumentsAuthorisations);
		}

		public void TestParties()
		{
			SetUpTestData();
			var goodsShipmentParties = Provider.Parties;
			AssertType<IM413AndIM415GoodsShipmentTypePartiesProvider>(goodsShipmentParties);
			AssertSame("Cached", goodsShipmentParties, Provider.Parties);
		}

		public void TestTransportInformation()
		{
			AssertType<IM413AndIM415GoodsShipmentTransportInformationProvider>(Provider.TransportInformation);
		}

		public void TestTransactionNature()
		{
			SetUpTestData();
			invoiceHeader.JZ_ValuationCode = "C";
			AssertEquals("C", Provider.TransactionNature);

			invoiceHeader.JZ_ValuationCode = "1";
			AssertEquals("1", Provider.TransactionNature);
		}

		public void TestValuationInformation()
		{
			SetUpTestData();
			var valuationInformation = Provider.ValuationInformation;
			AssertSame("Cached", valuationInformation, Provider.ValuationInformation);
		}

		public void TestDatesPlaces()
		{
			AssertType<IM413AndIM415GoodsShipmentTypeDatesPlacesProvider>(Provider.DatesPlaces);
		}

		public void TestGoodsShipmentItem()
		{
			SetUpTestData();
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			var goodsShipmentItem = Provider.GoodsShipmentItem;
			AssertType<IM413AndIM415GoodsShipmentItemProvider[]>(goodsShipmentItem);
			AssertSame("Cached", goodsShipmentItem, Provider.GoodsShipmentItem);
			AssertEquals("Precondition", 2, entryHeader.MergedLines.Count);
			AssertEquals("Count", 2, goodsShipmentItem.Count);
		}

		public void TestTransportInformation_ContainerIDAggregation()
		{
			SetUpTestData();
			var declaration = instruction.JobDeclaration;
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CNT001";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CNT002";
			declaration.CusContainers.AddNew().CO_ContainerNumber = "CNT003";

			entryLine.CL_LineNumber = 1;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			invoiceLine2.JI_CL = entryLine2.PK;

			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Single(pack => pack.ContainerNumber.Contains("CNT001")).IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Single(pack => pack.ContainerNumber.Contains("CNT003")).IsForInvoiceLine = true;

			invoiceLine2.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Single(pack => pack.ContainerNumber.Contains("CNT002")).IsForInvoiceLine = true;
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().Single(pack => pack.ContainerNumber.Contains("CNT003")).IsForInvoiceLine = true;

			var headerContainerIDs = Provider.TransportInformation.ContainerIdentificationNumbers;

			CombineAssertions("ContainerIds: with aggregation among Header and Items.", () =>
			{
				AssertEquals("1 Container ID in Header level", 1, headerContainerIDs.Count);
				AssertEquals("Header level container number: the container linked to both the lines.", "CNT003", headerContainerIDs.Single());

				var item1 = Provider.GoodsShipmentItem.First(item => item.GoodsItemNumber == "1");
				AssertEquals("1 Container ID in Item 1", 1, item1.ContainerIdentificationNumbers.Count);
				AssertEquals("Item level container number: the container associated with line 1.", "CNT001", item1.ContainerIdentificationNumbers.Single());

				var item2 = Provider.GoodsShipmentItem.First(item => item.GoodsItemNumber == "2");
				AssertEquals("1 Container ID in Item 2", 1, item2.ContainerIdentificationNumbers.Count);
				AssertEquals("Item level container number: the container associated with line 2.", "CNT002", item2.ContainerIdentificationNumbers.Single());
			});
		}

		protected override IM413AndIM415GoodsShipmentProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentProvider(new EntryHeaderWrapper(entryHeader));
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceHeader = declaration.Invoices.AddNew();
				invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = instruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		CusEntryInstruction instruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
