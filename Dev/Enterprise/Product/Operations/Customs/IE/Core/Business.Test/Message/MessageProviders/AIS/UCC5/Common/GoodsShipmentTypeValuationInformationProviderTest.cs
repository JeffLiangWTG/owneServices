using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class GoodsShipmentTypeValuationInformationProviderTest : DataProviderTestCase<GoodsShipmentTypeValuationInformationProvider>
	{
		public void TestDeliveryTerms()
		{
			SetUpTestData();
			invoice.JZ_IncoTerm = "AIS";
			invoice.ZG_AgreedPlaceCode = "AB";
			invoice.JZ_IncoTermPlace = "WAD";

			CombineAssertions(() =>
			{
				AssertEquals("IncotermCode", "AIS", Provider.DeliveryTerms.IncotermCode);
				AssertNull("UNLOCODE", Provider.DeliveryTerms.UNLOCODE);
				AssertEquals("CountryCode", "AB", Provider.DeliveryTerms.CountryCode);
				AssertEquals("Place", "WAD", Provider.DeliveryTerms.Place);
			});
		}

		public void TestAdditionsDeductions()
		{
			SetUpTestData();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = "CT1";
			charge.J7_Amount = 10m;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
			var charge2 = invoiceLine2.Charges.AddNew();
			charge2.J7_ChargeType = "CT1";
			charge2.J7_Amount = 20m;

			var apportionedCharge1 = invoiceLine.ApportionedCharges.AddNew();
			apportionedCharge1.J7_ChargeType = "CT1";
			apportionedCharge1.J7_Amount = 1m;
			apportionedCharge1.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			var apportionedCharge2 = invoiceLine2.ApportionedCharges.AddNew();
			apportionedCharge2.J7_ChargeType = "CT2";
			apportionedCharge2.J7_Amount = 2m;
			apportionedCharge2.J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;

			CombineAssertions(() =>
			{
				AssertEquals("AdditionsDeductions", 2, Provider.AdditionsDeductions.Count);
				AssertContainsExactElementsInAnyOrder("There should be 2 AdditionsDeductions elements as both Charges and Apportioned Charges should be merged by charge type.", new string[] { "CT1|31", "CT2|2" }, Provider.AdditionsDeductions.Select(x => x.Code + "|" + x.Amount.ToString()));
			});

			var zeroCharge = invoice.Charges.AddNew();
			zeroCharge.J7_ChargeType = AISChargeCodeList.Codes._1X;
			zeroCharge.J7_Amount = 0m;
			var provider = GetProvider();

			AssertEquals("InvoiceHeader zero charge for 1X", 3, provider.AdditionsDeductions.Count);
		}

		protected override GoodsShipmentTypeValuationInformationProvider GetProvider()
		{
			SetUpTestData();
			return new GoodsShipmentTypeValuationInformationProvider(new EntryHeaderWrapper(entryHeader));
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
