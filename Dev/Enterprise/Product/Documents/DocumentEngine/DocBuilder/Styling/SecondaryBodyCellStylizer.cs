using System.Drawing;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.DocBuilder.Styling
{
	class SecondaryBodyCellStylizer : RegionCellStylizer
	{
		public SecondaryBodyCellStylizer(TemplateStylizer templateStylizer)
			: base(templateStylizer)
		{
			themeBorderBrightness = TemplateStylizer.Theme.SecondaryBody.Color2.GetBrightness();
		}

		readonly float themeBorderBrightness;

		internal override Color RegionIdentifyingBackgroundColor
		{
			get { return Color.FromArgb(255, 204, 153); }
		}

		internal override float ThemeBorderBrightness
		{
			get { return themeBorderBrightness; }
		}

		protected override void StylizeCore(StylizerCellManager cellManager)
		{
			cellManager.BackgroundColor = TemplateStylizer.Theme.SecondaryBody.Color1;
			cellManager.BackgroundPattern = FillPatternStyle.None;
			cellManager.Borders.Bottom.Color = TemplateStylizer.Theme.SecondaryBody.Color2;
			cellManager.Borders.Top.Color = TemplateStylizer.Theme.SecondaryBody.Color2;
			cellManager.Borders.Left.Color = TemplateStylizer.Theme.SecondaryBody.Color2;
			cellManager.Borders.Right.Color = TemplateStylizer.Theme.SecondaryBody.Color2;

			cellManager.ShouldApply = true;
		}
	}
}
