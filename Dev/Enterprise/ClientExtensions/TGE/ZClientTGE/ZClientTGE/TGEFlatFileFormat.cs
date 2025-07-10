using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.TGE.PMS
{
	public class TGEFlatFileFormat : TabDelimitedFlatFileFormat
	{
		public override FileExtensionType FileExtensionForImport
		{
			get { return FileExtensionType.Txt; }
		}

		public override FlatFileDataRow ConvertToRow(ZString rawRow)
		{
			FlatFileDataRow dataRow = base.ConvertToRow(rawRow);
			ConsolAndShipmentRecord result = new ConsolAndShipmentRecord(NumberFields);

			for (int i = 0; i < dataRow.FieldCount; i++)
			{
				result[i] = dataRow[i];
			}

			for (int j = dataRow.FieldCount; j < NumberFields; j++)
			{
				result[j] = ZString.Empty;
			}
			return result;
		}

		const int NumberFields = 74;
	}
}


