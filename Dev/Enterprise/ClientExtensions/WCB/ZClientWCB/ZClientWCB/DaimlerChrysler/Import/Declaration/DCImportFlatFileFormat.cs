using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using RecordType = Enterprise.Client.WCB.Constants.DaimlerChrysler.Import.RecordType;

namespace Enterprise.Client.WCB.DaimlerChrysler
{
	public class DCImportFlatFileFormat : FixedWidthFlatFileFormat
	{
		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			throw new NotImplementedException("No export functionality");
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			FlatFileDataRow result = null;

			if (rawRow.SubstringSafe(0, RecordType.Length) == DecInvoiceHeaderDataRow.InvoiceHeaderCode)
			{
				result = new DecInvoiceHeaderDataRow(rawRow);
			}
			else if (rawRow.SubstringSafe(0, RecordType.Length) == DecInvoiceLineDataRow.InvoiceLineCode)
			{
				result = new DecInvoiceLineDataRow(rawRow);
			}
			else if (rawRow.SubstringSafe(0, 6) == "***BOF" || rawRow.SubstringSafe(0, 6) == "***EOF")
			{
				result = new FlatFileDataRow(1);
				result[0] = rawRow;
			}
			return result;
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.Txt; }
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.Txt; }
		}
	}
}
