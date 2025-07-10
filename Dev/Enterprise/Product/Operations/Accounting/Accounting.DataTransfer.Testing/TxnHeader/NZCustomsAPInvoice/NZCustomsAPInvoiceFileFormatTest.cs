using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class NZCustomsAPInvoiceFileFormatTest : CsvFlatFileFormatTest
	{
		public new void TestConvertToRow()
		{
			NZCustomsAPInvoiceFileFormat format = new NZCustomsAPInvoiceFileFormat();
			FlatFileDataRow resultRow = format.ConvertToRow(RawData);
			Assert("Row type", resultRow is NZCustomsAPInvoiceDataRow);
		}

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new NZCustomsAPInvoiceFileFormat();
		}

		readonly ZString RawData = @"40067555E,40160682D,2007-04-12,1119626101,1,S00004635,30.00,15.00,45.00,SAGGI MOTION PICTURES";
	}
}
