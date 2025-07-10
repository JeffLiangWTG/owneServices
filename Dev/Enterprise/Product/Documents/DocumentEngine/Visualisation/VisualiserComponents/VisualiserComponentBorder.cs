using System.Drawing;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.Visualisation
{
	public class VisualiserComponentBorder : VisualiserComponent
	{
		public VisualiserComponentBorder(Point location, Size size, CellFormat cellFormat)
			: base(location, size)
		{
			CellFormat = cellFormat;
		}

		public readonly CellFormat CellFormat;

		public bool HasBorders
		{
			get { return !CellFormat.Borders.IsEmpty; }
		}
	}
}
