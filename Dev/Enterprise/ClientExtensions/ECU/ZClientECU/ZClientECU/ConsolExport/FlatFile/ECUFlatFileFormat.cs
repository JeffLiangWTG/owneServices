using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.ECU.ConsolExport
{
	public class ECUFlatFileFormat : FlatFileFormat
	{
		public ECUFlatFileFormat()
		{ }

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		protected override string GetClientSpecificFileExtension()
		{
			return "ECU";
		}

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			ZStringBuilder builder = new ZStringBuilder();

			for (int i = 0; i < row.FieldCount; i++)
			{
				ZString fieldValue = row.GetField(i);
				builder.Append(fieldValue);
			}

			return builder.ToString();
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			throw new NotImplementedException("Import is not supported.");
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { throw new NotImplementedException("Import is not supported."); }
		}
	}
}
