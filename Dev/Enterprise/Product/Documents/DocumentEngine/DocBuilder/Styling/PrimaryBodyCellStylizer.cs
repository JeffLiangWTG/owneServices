using System.Drawing;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.DocBuilder.Styling
{
	class PrimaryBodyCellStylizer : RegionCellStylizer
	{
		public PrimaryBodyCellStylizer(TemplateStylizer templateStylizer)
			: base(templateStylizer)
		{
			themeBorderBrightness = TemplateStylizer.Theme.PrimaryBody.Color2.GetBrightness();
		}

		readonly float themeBorderBrightness;

		internal override Color RegionIdentifyingBackgroundColor
		{
			get { return Color.FromArgb(204, 204, 255); }
		}

		internal override float ThemeBorderBrightness
		{
			get { return themeBorderBrightness; }
		}

		protected override void StylizeCore(StylizerCellManager cellManager)
		{
			cellManager.BackgroundColor = TemplateStylizer.Theme.PrimaryBody.Color1;
			cellManager.BackgroundPattern = FillPatternStyle.None;
			cellManager.Borders.Bottom.Color = TemplateStylizer.Theme.PrimaryBody.Color2;
			cellManager.Borders.Top.Color = TemplateStylizer.Theme.PrimaryBody.Color2;
			cellManager.Borders.Left.Color = TemplateStylizer.Theme.PrimaryBody.Color2;
			cellManager.Borders.Right.Color = TemplateStylizer.Theme.PrimaryBody.Color2;

			cellManager.ShouldApply = true;
		}
	}
}
