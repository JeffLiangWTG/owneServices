using System.Data;
using System.IO;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class TestDataGridLayoutManager : DataGridLayoutManager
	{
		public MemoryStream SerialiseAndGetCurrentLayoutStream(ZGrid grid)
		{
			var result = new MemoryStream();

			new DataGridLayoutDataSetSerialiser().SerialiseAndGetCurrentLayoutAsDataset(grid).WriteXml(result, XmlWriteMode.IgnoreSchema);
			result.Position = 0;

			return result;
		}

		public void LoadLayout(ZGrid grid, MemoryStream columnSettings)
		{
			LoadLayoutCore(grid, new DataGridLayoutDataSetSerialiser().GetColumnSettingDataSet(columnSettings));
		}
	}
}
