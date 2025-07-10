using System.Data;
using System.Drawing;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.Visualisation
{
	public class VisualiserComponentGrid : VisualiserComponent
	{
		public VisualiserComponentGrid(Point location, Size size, VisualiserDataSet dS, string bindToName)
			: this(location, size, dS, bindToName, false)
		{
		}

		public VisualiserComponentGrid(Point location, Size size, VisualiserDataSet dS, string bindToName, bool autoHeight)
			: base(location, size)
		{
			this.DS = dS;
			this.BindToName = bindToName;
			this.AutoHeight = autoHeight;
		}

		public readonly VisualiserDataSet DS;
		public readonly string BindToName;
		public readonly bool AutoHeight;

#if DEBUG
		public override string GetControlDescriptionForTesting()
		{
			ZStringBuilder columns = new ZStringBuilder();
			DataTable table = DS.Tables[BindToName];
			if (table != null)
			{
				foreach (DataColumn column in table.Columns)
				{
					columns.Append(column.ColumnName);
				}
			}
			else
			{
				columns.Append("Table Not Found in VisualiserDataSet.");
			}
			return base.GetControlDescriptionForTesting()
					+ "Grid Bound To: " + BindToName + "\r\n"
					+ "Columns: " + columns.ToStringWithDelimiterBetweenAppends(", ") + "\r\n";
		}
#endif
	}
}
