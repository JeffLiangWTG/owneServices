using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business
{
	public class AutoDetectFlatFileFormat : DelimitedFlatFileFormat
	{
		protected override char Delimiter
		{
			get { return (autoDetectedDelimiter == '\0') ? ',' : autoDetectedDelimiter; }
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			if (autoDetectedDelimiter == '\0')
			{
				autoDetectedDelimiter = OCsvLine.AutoDetectDelimiter(rawRow);
			}
			return base.ConvertToRow(rawRow);
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.All; }
		}

		char autoDetectedDelimiter;
	}
}
