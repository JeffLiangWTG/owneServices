using System.Data;
using System.IO;

namespace Enterprise.ZArchitecture.Environment
{
	public struct BurnCDSettingsStruct
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public BurnCDSettingsStruct(byte[] registryValue)
		{
			if (registryValue != null)
			{
				MemoryStream xmlStream = new MemoryStream(registryValue);
				DataSet data = new DataSet();
				data.ReadXml(xmlStream, XmlReadMode.Auto);

				DeleteSourceDir = bool.Parse((string)data.Tables[0].Rows[0]["DeleteSourceDir"]);
				BurnDriveLetter = char.Parse((string)data.Tables[0].Rows[0]["BurnDriveLetter"]);
				WriteSpeedTimes = int.Parse((string)data.Tables[0].Rows[0]["WriteSpeedTimes"]);
			}
			else
			{
				DeleteSourceDir = true;
				BurnDriveLetter = '\0';
				WriteSpeedTimes = 0;
			}
		}

		public bool DeleteSourceDir;
		public char BurnDriveLetter;
		public int WriteSpeedTimes;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public byte[] GetValue()
		{
			DataTable table = new DataTable();

			DataColumn deleteSourceDirColumn = new DataColumn("DeleteSourceDir", typeof(bool));
			DataColumn burnDriveLetterColumn = new DataColumn("BurnDriveLetter", typeof(char));
			DataColumn writeSpeedTimesColumn = new DataColumn("WriteSpeedTimes", typeof(int));

			table.Columns.Add(deleteSourceDirColumn);
			table.Columns.Add(burnDriveLetterColumn);
			table.Columns.Add(writeSpeedTimesColumn);

			DataRow row = table.NewRow();
			row[deleteSourceDirColumn] = DeleteSourceDir;
			row[burnDriveLetterColumn] = BurnDriveLetter;
			row[writeSpeedTimesColumn] = WriteSpeedTimes;

			table.Rows.Add(row);
			DataSet data = new DataSet();
			data.Tables.Add(table);

			MemoryStream xmlStream = new MemoryStream();
			data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);
			return xmlStream.ToArray();
		}
	}
}
