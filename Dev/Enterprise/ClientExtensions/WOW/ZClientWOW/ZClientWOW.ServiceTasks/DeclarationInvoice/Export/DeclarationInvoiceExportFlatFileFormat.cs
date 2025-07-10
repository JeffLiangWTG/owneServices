using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.Export
{
	class DeclarationInvoiceExportFlatFileFormat : FixedWidthFlatFileFormat
	{
		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.Txt; }
		}

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			return row.ToString();
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			throw new NotImplementedException();
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.None; }
		}
	}
}
