using System;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.MFI.CaroTrans
{
	public class CaroTransShipmentFlatFileFormat : FlatFileFormat
	{
		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			throw new NotImplementedException("No Import functionality");
		}

		public override ZString ConvertToLine(FlatFileDataRow row)
		{
			ZStringBuilder result = new ZStringBuilder();

			for (int i = 0; i < row.FieldCount; i++)
			{
				result.Append(row[i].PadRight(Constants.ShipmentRecord.FieldsLength[i]) + ",");
			}

			return result.ToString().TrimEnd(new char[] { ',' });
		}

		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.None; }
		}

		public override FileExtensionType FileExtensionForExport
		{
			get { return FileExtensionType.None; }
		}
	}
}
