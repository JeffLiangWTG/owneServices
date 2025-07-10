using System;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class APDraftInvoiceLinkColumnStyle : ZTextBoxColumnStyle
	{
		public APDraftInvoiceLinkColumnStyle(APDraftInvoiceLinkColumnStyleInfo columnInfo) : base(columnInfo)
		{
		}

#if !WINZOR
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int paintingRowNum, Brush backBrush, Brush foreBrush, bool alignedToRight)
		{
			if (!parentZGrid.IsSelected(paintingRowNum))
			{
				backBrush = BrushProvider.FromColor(SystemDataRegistry.Instance.ColorTheme.GridReadOnlyColor);
				g.FillRectangle(backBrush, bounds);
			}

			var customCellTextStyleConfig = GetCustomCellTextStyleConfiguration();

			foreBrush = new SolidBrush(customCellTextStyleConfig.TextColor);
			var cellStyle = customCellTextStyleConfig.IsUnderlined ? FontStyle.Underline : 0;
			var cellFont = new Font(DataGridTableStyle.DataGrid.Font, cellStyle);
			var cellText = ColumnTextAtRow(source, paintingRowNum);

			PaintText(g, bounds, source, paintingRowNum, cellText, cellFont, backBrush, foreBrush, rightToLeft: false);
		}
#endif

#if WINZOR
		protected override string GetCellStyleString(CurrencyManager source, int rowNum)
		{
			var cellStyleString = base.GetCellStyleString(source, rowNum);

			var customCellTextStyleConfig = GetCustomCellTextStyleConfiguration();
			cellStyleString += $"color: {ColorTranslator.ToHtml(customCellTextStyleConfig.TextColor)};";
			cellStyleString += customCellTextStyleConfig.IsUnderlined ? $"text-decoration: underline;" : "";
			cellStyleString += $"cursor: pointer;";

			return cellStyleString;
		}
#endif

		CellTextStyleConfiguration GetCustomCellTextStyleConfiguration()
		{
			return new CellTextStyleConfiguration
			{
				TextColor = Color.Blue,
				IsUnderlined = true
			};
		}

		class CellTextStyleConfiguration
		{
			public Color TextColor { get; set; }
			public bool IsUnderlined { get; set; }
		}
	}

	public class APDraftInvoiceLinkColumnStyleInfo : ZTextBoxColumnStyleInfo
	{
		public override Type ColumnStyleType
		{
			get { return typeof(APDraftInvoiceLinkColumnStyle); }
		}
	}
}
