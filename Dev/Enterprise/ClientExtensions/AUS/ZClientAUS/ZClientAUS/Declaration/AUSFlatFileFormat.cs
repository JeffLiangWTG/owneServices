
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.AUS
{
	public class AUSFlatFileFormat : FixedWidthFlatFileFormat
	{
		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.Txt; }
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			FlatFileDataRow extractedDataRow;
			if (rawRow.Trim().IsEmpty || rawRow.Trim().StartsWith(AUSConstants.EndLine) || rawRow.Trim() == AUSConstants.EOFIndicator)
			{
				extractedDataRow = new FlatFileDataRow(1);
			}
			else
			{
				extractedDataRow = new FlatFileDataRow(AUSConstants.RowFieldCount);
				extractedDataRow[AUSConstants.InvoiceNumber] = GetValue(AUSConstants.InvoiceNumberStartIndex, AUSConstants.InvoiceNumberLength, rawRow).Trim();
				extractedDataRow[AUSConstants.PartNumber] = GetValue(AUSConstants.PartNumberStartIndex, AUSConstants.PartNumberLength, rawRow).Trim();
				extractedDataRow[AUSConstants.PartDescription] = GetValue(AUSConstants.PartDescriptionStartIndex, AUSConstants.PartDescriptionLength, rawRow).Trim();
				extractedDataRow[AUSConstants.Quantity] = GetValue(AUSConstants.QuantityStartIndex, AUSConstants.QuantityLength, rawRow).Trim();
				extractedDataRow[AUSConstants.LinePrice] = GetValue(AUSConstants.LinePriceStartIndex, AUSConstants.LinePriceLength, rawRow).Trim();
				extractedDataRow[AUSConstants.Classification] = GetValue(AUSConstants.ClassificationStartIndex, AUSConstants.ClassificationLength, rawRow).Trim();
				extractedDataRow[AUSConstants.Origin] = GetValue(AUSConstants.OriginStartIndex, AUSConstants.OriginLength, rawRow).Trim();
			}

			return extractedDataRow;
		}

		#region Export Not Used

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			return ZString.Empty;
		}

		public override FileExtensionType FileExtensionForExport
		{
			get
			{
				return new FileExtensionType();
			}
		}

		#endregion
	}
}
