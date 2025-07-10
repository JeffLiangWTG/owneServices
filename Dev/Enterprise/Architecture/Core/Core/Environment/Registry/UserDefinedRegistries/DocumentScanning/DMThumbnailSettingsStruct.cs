using System.Data;
using System.IO;

namespace Enterprise.ZArchitecture.Environment
{
	public struct DMThumbnailSettingsStruct
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public DMThumbnailSettingsStruct(byte[] registryValue)
		{
			if (registryValue != null)
			{
				MemoryStream xmlStream = new MemoryStream(registryValue);
				DataSet data = new DataSet();
				data.ReadXml(xmlStream, XmlReadMode.Auto);

				NumberOfThumbnails = int.Parse((string)data.Tables[0].Rows[0]["NumberOfThumbnails"]);
				ThumbNailViewActive = bool.Parse((string)data.Tables[0].Rows[0]["ThumbNailViewActive"]);
			}
			else
			{
				NumberOfThumbnails = 0;
				ThumbNailViewActive = false;
			}
		}

		public int NumberOfThumbnails;
		public bool ThumbNailViewActive;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public byte[] GetValue()
		{
			DataTable table = new DataTable();

			DataColumn numberOfThumbnailsColumn = new DataColumn("NumberOfThumbnails", typeof(int));
			DataColumn thumbNailViewActiveColumn = new DataColumn("ThumbNailViewActive", typeof(bool));

			table.Columns.Add(numberOfThumbnailsColumn);
			table.Columns.Add(thumbNailViewActiveColumn);

			DataRow row = table.NewRow();
			row[numberOfThumbnailsColumn] = NumberOfThumbnails;
			row[thumbNailViewActiveColumn] = ThumbNailViewActive;

			table.Rows.Add(row);
			DataSet data = new DataSet();
			data.Tables.Add(table);

			MemoryStream xmlStream = new MemoryStream();
			data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);
			return xmlStream.ToArray();
		}
	}
}
