using System.Collections.Generic;

namespace Enterprise.ZArchitecture.GUI
{
	public class PanelLayoutColumn
	{
		readonly List<PanelLayoutRow> rows = new List<PanelLayoutRow>();

		public IReadOnlyList<PanelLayoutRow> Rows => rows;

		internal void Add(PanelLayoutRow row)
		{
			rows.Add(row);
		}
	}
}
