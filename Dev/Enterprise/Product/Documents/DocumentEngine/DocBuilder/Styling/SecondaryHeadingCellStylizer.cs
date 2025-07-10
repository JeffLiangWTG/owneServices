using System.Drawing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.DocBuilder.Styling
{
	class SecondaryHeadingCellStylizer : RegionCellStylizer
	{
		public SecondaryHeadingCellStylizer(TemplateStylizer templateStylizer)
			: base(templateStylizer)
		{
			themeBorderBrightness = TemplateStylizer.Theme.SecondaryHeading.Color2.GetBrightness();
		}

		readonly float themeBorderBrightness;

		internal override Color RegionIdentifyingBackgroundColor
		{
			get { return DocBuilderTheme.DefaultSecondaryColor; }
		}

		internal override float ThemeBorderBrightness
		{
			get { return themeBorderBrightness; }
		}

		protected override void StylizeCore(StylizerCellManager cellManager)
		{
			cellManager.BackgroundColor = TemplateStylizer.Theme.SecondaryHeading.Color1;
			cellManager.BackgroundPattern = FillPatternStyle.Solid;
			cellManager.Borders.Bottom.Color = TemplateStylizer.Theme.SecondaryHeading.Color2;
			cellManager.Borders.Top.Color = TemplateStylizer.Theme.SecondaryHeading.Color2;
			cellManager.Borders.Left.Color = TemplateStylizer.Theme.SecondaryHeading.Color2;
			cellManager.Borders.Right.Color = TemplateStylizer.Theme.SecondaryHeading.Color2;

			cellManager.FontName = TemplateStylizer.Theme.SecondaryHeading.Font.FontFamily.Name;
			cellManager.FontSize = TemplateStylizer.Theme.SecondaryHeading.Font.Size;
			cellManager.FontStyle = TemplateStylizer.Theme.SecondaryHeading.Font.Style;
			cellManager.FontColor = TemplateStylizer.Theme.SecondaryHeading.FontColor;

			cellManager.ShouldApply = true;
		}
	}
}
