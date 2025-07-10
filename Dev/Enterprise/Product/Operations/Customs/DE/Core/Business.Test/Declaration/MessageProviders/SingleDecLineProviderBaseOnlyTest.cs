using Enterprise.Customs.DE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(SingleDecLineProvider))]
	sealed class SingleDecLineProviderBaseOnlyTest : ImportDecLineProviderAbstractTest<SingleDecLineProvider>
	{
		public void TestInvoiceAmount()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine.JI_LinePrice = 24532.85;
			invoiceLine2.JI_LinePrice = 1111.11;
			AssertEquals(25643.96m, Provider.InvoiceAmount);
		}

		public void TestForeignTradeStatisticsAmount()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine.JI_CustomsSecondQuantity = 10.58;
			invoiceLine.JI_CustomsSecondUnitQty = "KGM1";
			invoiceLine2.JI_CustomsSecondQuantity = 22.11;
			invoiceLine2.JI_CustomsSecondUnitQty = "KGM1";

			CombineAssertions(() =>
			{
				var amount = Provider.ForeignTradeStatisticsAmount;
				AssertEquals("Cached", amount, Provider.ForeignTradeStatisticsAmount);
				AssertEquals(32.69m, amount.Quantity);
				AssertEquals("KGM", amount.MeasurementUnit);
				AssertEquals("1", amount.Qualifier);
			});
		}

		public void TestForeignTradeStatisticsAmountFirst0()
		{
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			invoiceLine.JI_CustomsSecondQuantity = 0;
			invoiceLine.JI_CustomsSecondUnitQty = "KGM1";
			invoiceLine2.JI_CustomsSecondQuantity = 22.11;
			invoiceLine2.JI_CustomsSecondUnitQty = "KGM1";

			var amount = Provider.ForeignTradeStatisticsAmount;
			AssertNotNull(amount);
			AssertEquals(22.11m, amount.Quantity);
		}

		public void TestForeignTradeStatisticsAmount_Empty()
		{
			invoiceLine.JI_CustomsSecondQuantity = 0;
			invoiceLine.JI_CustomsSecondUnitQty = "KGM1";

			AssertNull(Provider.ForeignTradeStatisticsAmount);
		}

		protected override SingleDecLineProvider GetProvider() => new SingleDecLineProviderBaseForTest(entryLine);

		sealed class SingleDecLineProviderBaseForTest : SingleDecLineProvider
		{
			public SingleDecLineProviderBaseForTest(CusEntryLine entryLine)
				: base(entryLine)
			{
			}
		}
	}
}
