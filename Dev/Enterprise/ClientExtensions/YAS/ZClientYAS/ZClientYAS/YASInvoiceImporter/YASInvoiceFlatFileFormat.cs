using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.YAS.YASInvoiceImporter
{
	public class YASInvoiceFlatFileFormat : FlatFileFormat
	{
		#region Overrides

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			return new ZString();
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			FlatFileDataRow result = null;
			if (rawRow.Length > InvoiceConstants.Length.InvoiceNo)
			{
				result = new FlatFileDataRow(InvoiceConstants.NoOfFields);
				result.SetField(InvoiceConstants.FixedFieldPosition.InvoiceNo, rawRow.SubstringSafe(InvoiceConstants.Position.InvoiceNo, InvoiceConstants.Length.InvoiceNo).Trim());
				result.SetField(InvoiceConstants.FixedFieldPosition.PartNo, rawRow.SubstringSafe(InvoiceConstants.Position.PartNo, InvoiceConstants.Length.PartNo).Trim());
				result.SetField(InvoiceConstants.FixedFieldPosition.PartDescription, rawRow.SubstringSafe(InvoiceConstants.Position.PartDescription, InvoiceConstants.Length.PartDescription).Trim());
				result.SetField(InvoiceConstants.FixedFieldPosition.Qty, rawRow.SubstringSafe(InvoiceConstants.Position.Qty, InvoiceConstants.Length.Qty).Trim());
				result.SetField(InvoiceConstants.FixedFieldPosition.Value, rawRow.SubstringSafe(InvoiceConstants.Position.Value, InvoiceConstants.Length.Value).Trim());
			}
			else
			{
				result = new FlatFileDataRow(0);
			}

			return result;
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return new FileExtensionType(); }
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		protected override string GetClientSpecificFileExtension()
		{
			return InvoiceConstants.FileExtension;
		}

		#endregion
	}
}
