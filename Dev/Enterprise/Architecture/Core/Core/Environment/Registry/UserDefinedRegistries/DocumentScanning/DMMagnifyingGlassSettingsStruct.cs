using System.Data;
using System.IO;

namespace Enterprise.ZArchitecture.Environment
{
	public struct DMMagnifyingGlassSettingsStruct
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public DMMagnifyingGlassSettingsStruct(byte[] registryValue)
		{
			if (registryValue != null)
			{
				MemoryStream xmlStream = new MemoryStream(registryValue);
				DataSet data = new DataSet();
				data.ReadXml(xmlStream, XmlReadMode.Auto);

				MagnificationPercentage = int.Parse((string)data.Tables[0].Rows[0]["MagnificationPercentage"]);
			}
			else
			{
				MagnificationPercentage = 0;
			}
		}

		public int MagnificationPercentage;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]

		public byte[] GetValue()
		{
			DataTable table = new DataTable();

			DataColumn magnificationPercentageColumn = new DataColumn("MagnificationPercentage", typeof(int));

			table.Columns.Add(magnificationPercentageColumn);

			DataRow row = table.NewRow();
			row[magnificationPercentageColumn] = MagnificationPercentage;

			table.Rows.Add(row);
			DataSet data = new DataSet();
			data.Tables.Add(table);

			MemoryStream xmlStream = new MemoryStream();
			data.WriteXml(xmlStream, XmlWriteMode.IgnoreSchema);

			return xmlStream.ToArray();
		}
	}
}
