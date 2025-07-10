using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class NZCustomsAPInvoiceFileFormat : CsvFlatFileFormat
	{
		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			return new NZCustomsAPInvoiceDataRow(rawRow);
		}
	}
}
