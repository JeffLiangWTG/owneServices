using System.Drawing;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.DocBuilder.Styling
{
	class DocumentHeadingCellStylizer : RegionCellStylizer
	{
		public DocumentHeadingCellStylizer(TemplateStylizer templateStylizer)
			: base(templateStylizer)
		{
			themeBorderBrightness = TemplateStylizer.Theme.DocumentHeading.Color2.GetBrightness();
		}

		readonly float themeBorderBrightness;

		internal override Color RegionIdentifyingBackgroundColor
		{
			get { return Color.FromArgb(0, 51, 102); }
		}

		internal override float ThemeBorderBrightness
		{
			get { return themeBorderBrightness; }
		}

		protected override void StylizeCore(StylizerCellManager cellManager)
		{
			cellManager.BackgroundColor = TemplateStylizer.Theme.DocumentHeading.Color1;
			cellManager.BackgroundPattern = FillPatternStyle.Solid;
			cellManager.Borders.Bottom.Color = TemplateStylizer.Theme.DocumentHeading.Color2;
			cellManager.Borders.Top.Color = TemplateStylizer.Theme.DocumentHeading.Color2;
			cellManager.Borders.Left.Color = TemplateStylizer.Theme.DocumentHeading.Color2;
			cellManager.Borders.Right.Color = TemplateStylizer.Theme.DocumentHeading.Color2;

			cellManager.FontName = TemplateStylizer.Theme.DocumentHeading.Font.FontFamily.Name;
			cellManager.FontSize = TemplateStylizer.Theme.DocumentHeading.Font.Size;
			cellManager.FontStyle = TemplateStylizer.Theme.DocumentHeading.Font.Style;
			cellManager.FontColor = TemplateStylizer.Theme.DocumentHeading.FontColor;

			cellManager.ShouldApply = true;
		}
	}
}
