using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM413AndIM415GoodsShipmentItemValuationInformationProviderTest : DataProviderTestCase<IM413AndIM415GoodsShipmentItemValuationInformationProvider>
	{
		public void TestIGoodsShipmentItemTypeValuationInformation()
		{
			Assert("Should implement IGoodsShipmentItemTypeValuationInformation", Provider is IGoodsShipmentItemTypeValuationInformation);
		}

		public void TestAdditionsDeductions()
		{
			SetUpTestData();

			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "AB";
			charge.J7_Amount = 10m;

			var charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = "AB";
			charge2.J7_Amount = 20m;

			var apportionedCharge = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge.J7_ChargeType = "AB";
			apportionedCharge.J7_Amount = 1m;
			apportionedCharge.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var apportionedCharge2 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge2.J7_ChargeType = "BC";
			apportionedCharge2.J7_Amount = 2m;
			apportionedCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var additionsDeductions = Provider.AdditionsDeductions;
			AssertSame("Cached", additionsDeductions, Provider.AdditionsDeductions);
			AssertContainsExactElementsInAnyOrder(new string[] { "AB|31", "BC|2" }, additionsDeductions.Select(x => x.Code + "|" + x.Amount.ToString()));
		}

		public void TestValuationIndicator()
		{
			SetUpTestData();
			invoiceLine.JI_RelatedIndicator = "Y";
			invoiceLine.ZG_RelatedIndicator3 = "Y";
			var provider = new IM413AndIM415GoodsShipmentItemValuationInformationProvider(entryLine, new EntryHeaderWrapper(entryHeader));
			var valuationIndicator = provider.ValuationIndicator;
			AssertEquals("1010", valuationIndicator);
			AssertSame("Cached", valuationIndicator, provider.ValuationIndicator);
		}

		public void TestItemAmount()
		{
			SetUpTestData();
			invoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";
			invoiceLine.JI_LinePrice = 1.23m;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			invoiceLine2.JI_LinePrice = 4.56m;
			var provider = new IM413AndIM415GoodsShipmentItemValuationInformationProvider(entryLine, new EntryHeaderWrapper(entryHeader));
			AssertEquals(5.79m, provider.ItemAmount);
			invoiceHeader.JZ_RX_NKInvoice_Currency = "USD";
			provider = new IM413AndIM415GoodsShipmentItemValuationInformationProvider(entryLine, new EntryHeaderWrapper(entryHeader));
			AssertEquals(5.79m, provider.ItemAmount);
		}

		public void TestValuationMethod()
		{
			SetUpTestData();
			invoiceLine.JI_ValuationCode = "1";
			AssertEquals("1", Provider.ValuationMethod);
		}

		public void TestPreference()
		{
			SetUpTestData();
			invoiceLine.JI_PrimaryPreference = "325";
			AssertEquals("325", Provider.Preference);
		}

		public void TestValue()
		{
			AssertNull(Provider.Value);
		}

		public void TestTransportCosts()
		{
			AssertNull(Provider.TransportCosts);
		}

		protected override IM413AndIM415GoodsShipmentItemValuationInformationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413AndIM415GoodsShipmentItemValuationInformationProvider(entryLine, new EntryHeaderWrapper(entryHeader));
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
