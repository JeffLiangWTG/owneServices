using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class NZCustomsAPInvoiceDataRow : FlatFileDataRow
	{
		public NZCustomsAPInvoiceDataRow(ZString rawData)
			: this(new OCsvLine(rawData))
		{
		}

		protected NZCustomsAPInvoiceDataRow(OCsvLine csvLine)
			: base(csvLine.FieldValues)
		{
			JobNumber = GetFieldAndTrim(Schema.JobNumber);

			NetChargesExclGST = 0;
			NetChargesExclGST += GetFieldAsZDecimal(Schema.FeesNLevies);
			NetChargesExclGST += GetFieldAsZDecimal(Schema.Duty);

			GST = 0;
			GST += GetFieldAsZDecimal(Schema.GstOnFeesNLevies);
			GST += GetFieldAsZDecimal(Schema.GstOnImports);

			TotalInclGST = GetFieldAsZDecimal(Schema.TotalInclGST);
		}

		public static class Schema
		{
			public const int JobNumber = 8;
			public const int FeesNLevies = 9;
			public const int Duty = 10;
			public const int GstOnFeesNLevies = 11;
			public const int GstOnImports = 12;
			public const int TotalInclGST = 13;
		}

		public ZString JobNumber
		{
			get;
			set;
		}

		public ZDecimal NetChargesExclGST
		{
			get;
			set;
		}

		public ZDecimal GST
		{
			get;
			set;
		}

		public ZDecimal TotalInclGST
		{
			get;
			set;
		}
	}
}
