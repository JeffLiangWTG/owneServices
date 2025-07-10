using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	internal abstract class SagInvoiceHeaderDataRowTest : TestCase
	{
		public virtual void TestProperties()
		{
			SagInvoiceHeaderDataRow row = NewSagInvoiceHeaderDataRow;
			AssertEquals("Row should have 12 files", 12, row.FieldCount);
			row.AccountNumber = new ZString("mwcbnxjajhyxzjarczbeatqngyjimshyxrxpopdxutxzsiebnhydmqdsfpvpnaoraazimkdnvdkhvquxuiveudmgoejzuojuegzsjwocekhigvwpuxvfekwwnstibdthtpcpqcoxttzjwdashuewtepujdeqcgaarapqshysixlkknzjsbahzwnozifmyyvnsyswojzavmnwtejweuxbgtbumaqkblukbyslvdvibalnjqvuzjidaugzxxjntpy");
			row.SettlementDueDate = new ZDateTime(2006, 9, 11);
			row.OsGoodsValue = new ZDecimal(76.2203303520662);
			row.LocalControlValue = new ZDecimal(8.32222868144616);
			row.ExchangeCurrencyRate = new ZDecimal(53974.5778562383);
			row.ReciprocalExchangeCurrencyRate = new ZDecimal(89244.8356790677);
			row.LedgerSource = new ZInt(100451544);
			row.TransactionType = new ZInt(281043633);
			row.InvoiceDate = new ZDateTime(2003, 7, 15);
			row.LocalTaxValue = new ZDecimal(2.4931955256002);
			AssertEquals("AccountNumber", new ZString("mwcbnxjajhyxzjarczbeatqngyjimshyxrxpopdxutxzsiebnhydmqdsfpvpnaoraazimkdnvdkhvquxuiveudmgoejzuojuegzsjwocekhigvwpuxvfekwwnstibdthtpcpqcoxttzjwdashuewtepujdeqcgaarapqshysixlkknzjsbahzwnozifmyyvnsyswojzavmnwtejweuxbgtbumaqkblukbyslvdvibalnjqvuzjidaugzxxjntpy"), row.GetField(SagInvoiceHeaderDataRow.Schema.AccountNumber));
			AssertEquals("SettlementDueDate", new ZDateTime(2006, 9, 11), row.GetFieldAsZDateTime(SagInvoiceHeaderDataRow.Schema.SettlementDueDate, ELGConstants.DataDateFormat));
			AssertEquals("OsGoodsValue", new ZDecimal(76.22), row.GetFieldAsZDecimal(SagInvoiceHeaderDataRow.Schema.OsGoodsValue, 2));
			AssertEquals("LocalControlValue", new ZDecimal(8.32), row.GetFieldAsZDecimal(SagInvoiceHeaderDataRow.Schema.LocalControlValue, 2));
			AssertEquals("ExchangeCurrencyRate", new ZDecimal(53974.577856), row.GetFieldAsZDecimal(SagInvoiceHeaderDataRow.Schema.ExchangeCurrencyRate, 6));
			AssertEquals("ReciprocalExchangeCurrencyRate", new ZDecimal(89244.835679), row.GetFieldAsZDecimal(SagInvoiceHeaderDataRow.Schema.ReciprocalExchangeCurrencyRate, 6));
			AssertEquals("LedgerSource", new ZInt(100451544), row.GetFieldAsZInt(SagInvoiceHeaderDataRow.Schema.LedgerSource));
			AssertEquals("TransactionType", new ZInt(281043633), row.GetFieldAsZInt(SagInvoiceHeaderDataRow.Schema.TransactionType));
			AssertEquals("PostedDate", new ZDateTime(2003, 7, 15), row.GetFieldAsZDateTime(SagInvoiceHeaderDataRow.Schema.InvoiceDate, ELGConstants.DataDateFormat));
			AssertEquals("LocalTaxValue", new ZDecimal(2.49), row.GetFieldAsZDecimal(SagInvoiceHeaderDataRow.Schema.LocalTaxValue));
		}

		protected abstract SagInvoiceHeaderDataRow NewSagInvoiceHeaderDataRow { get; }
	}
}
