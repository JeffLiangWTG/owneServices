using System.Data;
using System.IO;

namespace Enterprise.ZArchitecture.Environment
{
	public struct DMScanningFileOutputOptionSettingsStruct
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public DMScanningFileOutputOptionSettingsStruct(byte[] registryValue)
		{
			if (registryValue != null)
			{
				MemoryStream xmlStream = new MemoryStream(registryValue);
				DataSet data = new DataSet();
				data.ReadXml(xmlStream, XmlReadMode.Auto);

				OutputOption = ((string)data.Tables[0].Rows[0]["OutputOption"]);
				AutoAllocate = bool.Parse((string)data.Tables[0].Rows[0]["AutoAllocate"]);
			}
			else
			{
				OutputOption = "Automatic";
				AutoAllocate = true;
			}
		}

		public string OutputOption;
		public bool AutoAllocate;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public byte[] GetValue()
		{
			DataTable table = new DataTable();

			DataColumn outputOptionColumn = new DataColumn("OutputOption", typeof(string));
			DataColumn autoAllocateColumn = new DataColumn("AutoAllocate", typeof(bool));

			table.Columns.Add(outputOptionColumn);
			table.Columns.Add(autoAllocateColumn);

			DataRow row = table.NewRow();
			row[outputOptionColumn] = OutputOption;
			row[autoAllocateColumn] = AutoAllocate;

			table.Rows.Add(row);
			DataSet data = new DataSet();
			data.Tables.Add(table);

			MemoryStream xmlStream = new MemoryStream();
			data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);
			return xmlStream.ToArray();
		}
	}
}
