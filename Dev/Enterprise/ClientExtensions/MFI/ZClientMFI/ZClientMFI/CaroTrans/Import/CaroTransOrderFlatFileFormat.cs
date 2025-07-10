using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransOrderFlatFileFormat : FlatFileFormat
	{
		public override CargoWise.Types.ZString ConvertToLine(FlatFileDataRow row)
		{
			throw new NotImplementedException("No export functionality");
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			var line = new OCsvLine(rawRow, ',');
			return new FlatFileDataRow(line.FieldValues);
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.ClientSpecific; }
		}

		protected override string GetClientSpecificFileExtension()
		{
			return Constants.CaroTransOrderClientSpecificFileExtension;
		}
	}
}
