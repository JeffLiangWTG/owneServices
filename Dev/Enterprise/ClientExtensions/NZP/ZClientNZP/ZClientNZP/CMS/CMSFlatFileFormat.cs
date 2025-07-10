using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
namespace Enterprise.Client.NZP.CMS
{
	public class CMSFlatFileFormat : PipeDelimitedFlatFileFormat
	{
		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			return base.ConvertToLine(row) + "|";
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			throw new NotSupportedException("This is not supported becuase the file is not currently imported");
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.None; }
		}

		protected override string GetClientSpecificFileExtension()
		{
			return "lst";
		}
	}
}
