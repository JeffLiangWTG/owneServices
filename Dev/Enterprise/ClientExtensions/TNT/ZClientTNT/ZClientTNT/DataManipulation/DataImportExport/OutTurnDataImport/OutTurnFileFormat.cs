using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TNT.OutTurnDataImport
{
	public class OutTurnFileFormat : PipeDelimitedFlatFileFormat
	{
		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			FlatFileDataRow dataRow = base.ConvertToRow(rawRow);
			OutTurnFlatFileDataRow result = new OutTurnFlatFileDataRow();
			int i = 0;
			for (; i < Math.Min(dataRow.FieldCount, result.FieldCount); i++)
			{
				result[i] = dataRow[i];
			}

			while (++i < result.FieldCount)
			{
				result[i] = "";
			}

			return result;
		}

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			throw new NotSupportedException("ConvertToLine");
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		protected override string GetClientSpecificFileExtension()
		{
			return TNTConstants.OuturnFileExtension;
		}
	}
}
