using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentItemDatesPlacesProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentItemDatesPlacesProvider>
	{
		public void TestIGoodsShipmentItemTypeDatesPlaces()
		{
			Assert("Should implement IGoodsShipmentItemTypeDatesPlaces", Provider is IGoodsShipmentItemTypeDatesPlaces);
		}

		public void TestCountryDestination()
		{
			SetUpTestData();
			invoiceLine.ZG_CountryOfDestination = "FR";
			AssertEquals("FR", Provider.CountryDestination);
		}

		public void TestCountryDispatch()
		{
			SetUpTestData();
			invoiceLine.ZG_CountryOfDispatch = "GB";
			AssertEquals("GB", Provider.CountryDispatch);
		}

		public void TestCountryOrigin()
		{
			SetUpTestData();
			invoiceLine.JI_CountryOfOrigin = "AU";
			AssertEquals("AU", Provider.CountryOrigin);
		}

		public void TestCountryPreferentialOrigin()
		{
			SetUpTestData();
			invoiceLine.ZG_CountryOfSupply = "IE";
			AssertEquals("IE", Provider.CountryPreferentialOrigin);
		}

		protected override IM413AndIM415GoodsShipmentItemDatesPlacesProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentItemDatesPlacesProvider(invoiceLine);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
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

		JobDeclaration declaration;
		CusEntryInstruction instruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
	}
}
