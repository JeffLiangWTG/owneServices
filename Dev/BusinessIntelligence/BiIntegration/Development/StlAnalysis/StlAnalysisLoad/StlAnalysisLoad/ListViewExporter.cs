namespace Enterprise.StlAnalysis.Load
{
	using System.Text;
	using System.Windows.Forms;
	using Enterprise.ZArchitecture.GUI;

	class ListViewExporter
	{
		public void CopyToClipboard(ListView listViewToExport)
		{
			var contents = new StringBuilder();

			for (int i = 0; i < listViewToExport.Items.Count; i++)
			{
				var item = listViewToExport.Items[i];

				for (int j = 0; j < item.SubItems.Count; j++)
				{
					contents.Append(item.SubItems[j].Text.Trim().Replace('\t', ' '));
					contents.Append('\t');
				}

				contents.AppendLine();
			}

			SafeClipboard.SetText(contents.ToString()); // This is a standalone tool.
		}

		public static int CalculateColumnWidth(ListView listView, int columnIndex)
		{
			const int verticalBarWidth = 21;
			int result = listView.Width - verticalBarWidth;

			for (int i = 0; i < listView.Columns.Count; i++)
			{
				if (i != columnIndex)
				{
					result -= listView.Columns[i].Width;
				}
			}

			return result;
		}
	}
}
