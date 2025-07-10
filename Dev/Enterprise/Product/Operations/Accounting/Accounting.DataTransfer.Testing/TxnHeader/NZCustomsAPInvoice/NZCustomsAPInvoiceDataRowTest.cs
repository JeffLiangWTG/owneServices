using CargoWise.Types;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class NZCustomsAPInvoiceDataRowTest : FlatFileDataRowTest
	{
		public void TestAllProperties()
		{
			NZCustomsAPInvoiceDataRow row = new NZCustomsAPInvoiceDataRow(RawData);
			AssertEquals("Field Count", 17, row.FieldCount);
			AssertEquals("Job Number", "STMEL5858468", row.JobNumber);
			AssertEquals("Net Charges Excl GST", new ZDecimal(51.72), row.NetChargesExclGST);
			AssertEquals("GST", new ZDecimal(306.30 + 7.76), row.GST);
			AssertEquals("Total Incl GST", new ZDecimal(365.78), row.TotalInclGST);
		}

		readonly ZString RawData = @"CBROK,40605883C,40218666G,ABILIQUIP LIMITED,13/JUN/2022,4168237201,INV,,STMEL5858468,51.72,0.00,7.76,306.30,365.78,,30/JUN/2022,20/07/2022";
	}
}
