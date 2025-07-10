using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.ComplianceRisk.GUI
{
	public class LegalBooksColumnStyle : ZTextBoxColumnStyle
	{
		public LegalBooksColumnStyle(LegalBooksColumnStyleInfo columnInfo) : base(columnInfo)
		{
		}

#if !WINZOR
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignedToRight)
		{
			foreBrush = new SolidBrush(Color.Blue);

			var cellText = ColumnTextAtRow(source, paintingRowNum);
			var cellFont = new Font(DataGridTableStyle.DataGrid.Font, FontStyle.Underline);

			PaintText(g, bounds, source, paintingRowNum, cellText, cellFont, backBrush, foreBrush, false);
		}
#endif
	}

	public class LegalBooksColumnStyleInfo : ZTextBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(LegalBooksColumnStyle); }
		}
	}
}
