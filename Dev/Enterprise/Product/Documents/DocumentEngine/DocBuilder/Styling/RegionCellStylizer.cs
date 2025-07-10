using System.Drawing;

namespace Enterprise.DocumentEngine.DocBuilder.Styling
{
	abstract class RegionCellStylizer
	{
		internal RegionCellStylizer(TemplateStylizer templateStylizer)
		{
			this.TemplateStylizer = templateStylizer;
		}

		internal readonly TemplateStylizer TemplateStylizer;

		protected abstract void StylizeCore(StylizerCellManager cellFormat);
		internal abstract float ThemeBorderBrightness { get; }
		internal abstract Color RegionIdentifyingBackgroundColor { get; }

		internal void Stylize(StylizerCellManager cellManager)
		{
			StylizeCore(cellManager);
			ColorSurroundingBorders(cellManager);
		}

		internal bool CanStylize(StylizerCellManager cellFormat)
		{
			return cellFormat.BackgroundColor.ToArgb().Equals(RegionIdentifyingBackgroundColor.ToArgb());
		}

		void ColorSurroundingBorders(StylizerCellManager cellManager)
		{
			var workSheet = cellManager.WorkSheet;
			var row = cellManager.Row;
			var column = cellManager.Column;

			if (column < TemplateStylizer.MaximumColumnsToStylize && row + 1 < workSheet.RowCount)
			{
				var bottomCellManager = TemplateStylizer.GetCellManager(workSheet, row + 1, column);
				bottomCellManager.Borders.Top.Color = cellManager.Borders.Bottom.Color;
				bottomCellManager.ShouldApply = true;

				if (row > 0)
				{
					var topCellManager = TemplateStylizer.GetCellManager(workSheet, row - 1, column);
					topCellManager.Borders.Bottom.Color = cellManager.Borders.Top.Color;
					topCellManager.ShouldApply = true;
				}
			}
			if (column + 1 < TemplateStylizer.MaximumColumnsToStylize)
			{
				var rightCellManager = TemplateStylizer.GetCellManager(workSheet, row, column + 1);
				rightCellManager.Borders.Left.Color = cellManager.Borders.Right.Color;
				rightCellManager.ShouldApply = true;

				if (column > 0)
				{
					var leftCellManager = TemplateStylizer.GetCellManager(workSheet, row, column - 1);
					leftCellManager.Borders.Right.Color = cellManager.Borders.Left.Color;
					leftCellManager.ShouldApply = true;
				}
			}
		}
	}
}
