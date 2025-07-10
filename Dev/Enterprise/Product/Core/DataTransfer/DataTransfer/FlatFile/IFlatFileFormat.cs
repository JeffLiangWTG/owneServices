
using CargoWise.Types;

namespace Enterprise.DataTransfer.Business
{
	public interface IFlatFileFormat
	{
		ZString ConvertToLine(FlatFileDataRow row);
		FlatFileDataRow ConvertToRow(ZString flatFileLine);
		FileExtensionType FileExtensionForExport { get; }
		FileExtensionType FileExtensionForImport { get; }
	}
}
