using System;

using CargoWise.Types;

using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.HEN.Nissan
{
	public class NissanInvoiceFlatFileFormat : FixedWidthFlatFileFormat
	{
		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.All; }
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			FlatFileDataRow extractedDataRow;

			ZString lineType = GetValue(NissanConstants.LineTypeStartIndex, NissanConstants.LineTypeLength, rawRow).Trim();

			switch (lineType)
			{
				case NissanConstants.InvoiceHeaderLineType:
					extractedDataRow = ConvertToInvoiceHeaderRow(rawRow);
					break;
				case NissanConstants.InvoiceLineLineType:
					extractedDataRow = ConvertToInvoiceLineRow(rawRow);
					break;
				default:
					extractedDataRow = new FlatFileDataRow(1);
					break;
			}

			return extractedDataRow;
		}

		FlatFileDataRow ConvertToInvoiceHeaderRow(ZString dataRow)
		{
			FlatFileDataRow result = new FlatFileDataRow(NissanConstants.InvoiceHeader.FieldCount);
			result[NissanConstants.LineTypePosition] = GetValue(NissanConstants.LineTypeStartIndex, NissanConstants.LineTypeLength, dataRow).Trim();
			result[NissanConstants.InvoiceHeader.InvoiceNo] = GetValue(NissanConstants.InvoiceHeader.StartIndex.InvoiceNo, NissanConstants.InvoiceHeader.Length.InvoiceNo, dataRow).Trim();
			result[NissanConstants.InvoiceHeader.Currency] = GetValue(NissanConstants.InvoiceHeader.StartIndex.Currency, NissanConstants.InvoiceHeader.Length.Currency, dataRow).Trim();
			result[NissanConstants.InvoiceHeader.TotalAmount] = GetValue(NissanConstants.InvoiceHeader.StartIndex.TotalAmount, NissanConstants.InvoiceHeader.Length.TotalAmount, dataRow).Trim();
			result[NissanConstants.InvoiceHeader.GrossWeight] = GetValue(NissanConstants.InvoiceHeader.StartIndex.GrossWeight, NissanConstants.InvoiceHeader.Length.GrossWeight, dataRow).Trim();

			return result;
		}

		FlatFileDataRow ConvertToInvoiceLineRow(ZString dataRow)
		{
			FlatFileDataRow result = new FlatFileDataRow(NissanConstants.InvoiceLine.FieldCount);
			result[NissanConstants.LineTypePosition] = GetValue(NissanConstants.LineTypeStartIndex, NissanConstants.LineTypeLength, dataRow).Trim();
			result[NissanConstants.InvoiceLine.ProductCode] = GetValue(NissanConstants.InvoiceLine.StartIndex.ProductCode, NissanConstants.InvoiceLine.Length.ProductCode, dataRow).Trim();
			result[NissanConstants.InvoiceLine.InvoiceQty] = GetValue(NissanConstants.InvoiceLine.StartIndex.InvoiceQty, NissanConstants.InvoiceLine.Length.InvoiceQty, dataRow).Trim();
			result[NissanConstants.InvoiceLine.InvoicePrice] = GetValue(NissanConstants.InvoiceLine.StartIndex.InvoicePrice, NissanConstants.InvoiceLine.Length.InvoicePrice, dataRow).Trim();
			result[NissanConstants.InvoiceLine.GrossWeight] = GetValue(NissanConstants.InvoiceLine.StartIndex.GrossWeight, NissanConstants.InvoiceLine.Length.GrossWeight, dataRow).Trim();
			result[NissanConstants.InvoiceLine.OrderNo] = GetValue(NissanConstants.InvoiceLine.StartIndex.OrderNo, NissanConstants.InvoiceLine.Length.OrderNo, dataRow).Trim();
			result[NissanConstants.InvoiceLine.OrderLineNo] = GetValue(NissanConstants.InvoiceLine.StartIndex.OrderLineNo, NissanConstants.InvoiceLine.Length.OrderLineNo, dataRow).Trim();
			result[NissanConstants.InvoiceLine.GoodsOrigin] = GetValue(NissanConstants.InvoiceLine.StartIndex.GoodsOrigin, NissanConstants.InvoiceLine.Length.GoodsOrigin, dataRow).Trim();

			return result;
		}

		#region Export not in use

		public override FileExtensionType FileExtensionForExport
		{
			get { throw new NotImplementedException("Export not in use"); }
		}

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			throw new NotImplementedException("Export not in use");
		}

		#endregion
	}
}
