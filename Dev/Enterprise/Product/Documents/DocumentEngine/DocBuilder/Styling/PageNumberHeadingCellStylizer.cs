using System.Drawing;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.DocBuilder.Styling
{
	class PageNumberHeadingCellStylizer : RegionCellStylizer
	{
		public PageNumberHeadingCellStylizer(TemplateStylizer templateStylizer)
			: base(templateStylizer)
		{
			themeBorderBrightness = TemplateStylizer.Theme.PageNumberHeading.Color2.GetBrightness();
		}

		readonly float themeBorderBrightness;

		internal override Color RegionIdentifyingBackgroundColor
		{
			get { return Color.FromArgb(255, 102, 0); }
		}

		internal override float ThemeBorderBrightness
		{
			get { return themeBorderBrightness; }
		}

		protected override void StylizeCore(StylizerCellManager cellManager)
		{
			cellManager.BackgroundColor = TemplateStylizer.Theme.PageNumberHeading.Color1;
			cellManager.BackgroundPattern = FillPatternStyle.Solid;
			cellManager.Borders.Bottom.Color = TemplateStylizer.Theme.PageNumberHeading.Color2;
			cellManager.Borders.Top.Color = TemplateStylizer.Theme.PageNumberHeading.Color2;
			cellManager.Borders.Left.Color = TemplateStylizer.Theme.PageNumberHeading.Color2;
			cellManager.Borders.Right.Color = TemplateStylizer.Theme.PageNumberHeading.Color2;

			cellManager.FontName = TemplateStylizer.Theme.PageNumberHeading.Font.FontFamily.Name;
			cellManager.FontSize = TemplateStylizer.Theme.PageNumberHeading.Font.Size;
			cellManager.FontStyle = TemplateStylizer.Theme.PageNumberHeading.Font.Style;
			cellManager.FontColor = TemplateStylizer.Theme.PageNumberHeading.FontColor;

			cellManager.ShouldApply = true;
		}
	}
}
