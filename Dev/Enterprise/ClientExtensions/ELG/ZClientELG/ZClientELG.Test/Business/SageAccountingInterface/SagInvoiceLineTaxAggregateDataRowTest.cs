using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	public class SagInvoiceLineTaxAggregateDataRowTest : TestCase
	{
		public void TestProperties()
		{
			SagInvoiceLineTaxAggregateDataRow row = new SagInvoiceLineTaxAggregateDataRow();
			AssertEquals("Row should have 5 files", 5, row.FieldCount);
			row.TaxRateIndicator = new ZString("p");
			row.GoodsValue = new ZDecimal(98.2977341386945);
			row.DiscountValue = new ZDecimal(7.01118205069154);
			row.DiscountPercentage = new ZDecimal(66.3141062326329);
			row.TaxValue = new ZDecimal(572.050298830518);
			AssertEquals("TaxRateIndicator", new ZString("p"), row.GetField(SagInvoiceLineTaxAggregateDataRow.Schema.TaxRateIndicator));
			AssertEquals("GoodsValue", new ZDecimal(98.30), row.GetFieldAsZDecimal(SagInvoiceLineTaxAggregateDataRow.Schema.GoodsValue));
			AssertEquals("DiscountValue", new ZDecimal(7.01), row.GetFieldAsZDecimal(SagInvoiceLineTaxAggregateDataRow.Schema.DiscountValue));
			AssertEquals("DiscountPercentage", new ZDecimal(66.31), row.GetFieldAsZDecimal(SagInvoiceLineTaxAggregateDataRow.Schema.DiscountPercentage));
			AssertEquals("TaxValue", new ZDecimal(572.05), row.GetFieldAsZDecimal(SagInvoiceLineTaxAggregateDataRow.Schema.TaxValue));
		}
	}
}
