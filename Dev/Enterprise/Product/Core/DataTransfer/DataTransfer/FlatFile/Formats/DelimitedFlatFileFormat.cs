using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business
{
	public abstract class DelimitedFlatFileFormat : FlatFileFormat
	{
		#region Export

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			ZStringBuilder builder = new ZStringBuilder();

			for (int i = 0; i < row.FieldCount; i++)
			{
				ZString fieldValue = row.GetField(i);
				AppendFieldValue(builder, fieldValue);
				if (!IsEndOfLine(row, i))
				{
					builder.Append(Delimiter.ToString());
				}
			}

			return builder.ToString();
		}

		protected bool IsEndOfLine(FlatFileDataRow row, int index)
		{
			return (index == row.FieldCount - 1);
		}

		void AppendFieldValue(ZStringBuilder builder, ZString fieldValue)
		{
			if (IncludeQuotesWhenExporting)
			{
				builder.Append('\"' + fieldValue + '\"');
			}
			else
			{
				builder.Append(fieldValue);
			}
		}

		#endregion

		#region Import

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			var csvLine = new OCsvLine(rawRow, Delimiter);
			var result = new FlatFileDataRow(csvLine.FieldValues);
			return result;
		}

		#endregion

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.Txt; }
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.Txt; }
		}

		protected abstract char Delimiter { get; }

		protected virtual bool IncludeQuotesWhenExporting
		{
			get { return false; }
		}
	}
}
