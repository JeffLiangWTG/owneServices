using System;
using System.Drawing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.DocBuilder.Styling
{
	class PrimaryHeadingCellStylizer : RegionCellStylizer
	{
		public PrimaryHeadingCellStylizer(TemplateStylizer templateStylizer)
			: base(templateStylizer)
		{
			themeBorderBrightness = TemplateStylizer.Theme.PrimaryHeading.Color2.GetBrightness();
		}

		readonly float themeBorderBrightness;

		internal override Color RegionIdentifyingBackgroundColor
		{
			get { return DocBuilderTheme.DefaultPrimaryColor; }
		}

		internal override float ThemeBorderBrightness
		{
			get { return themeBorderBrightness; }
		}

		protected override void StylizeCore(StylizerCellManager cellManager)
		{
			ThrowIfNull(cellManager, nameof(cellManager));
			ThrowIfNull(cellManager.Borders, nameof(cellManager.Borders));
			ThrowIfNull(cellManager.Borders.Bottom, nameof(cellManager.Borders.Bottom));
			ThrowIfNull(cellManager.Borders.Top, nameof(cellManager.Borders.Top));
			ThrowIfNull(cellManager.Borders.Left, nameof(cellManager.Borders.Left));
			ThrowIfNull(cellManager.Borders.Right, nameof(cellManager.Borders.Right));
			ThrowIfNull(TemplateStylizer, nameof(TemplateStylizer));
			ThrowIfNull(TemplateStylizer.Theme, nameof(TemplateStylizer.Theme));

			cellManager.BackgroundColor = TemplateStylizer.Theme.PrimaryHeading.Color1;
			cellManager.BackgroundPattern = FillPatternStyle.Solid;
			cellManager.Borders.Bottom.Color = TemplateStylizer.Theme.PrimaryHeading.Color2;
			cellManager.Borders.Top.Color = TemplateStylizer.Theme.PrimaryHeading.Color2;
			cellManager.Borders.Left.Color = TemplateStylizer.Theme.PrimaryHeading.Color2;
			cellManager.Borders.Right.Color = TemplateStylizer.Theme.PrimaryHeading.Color2;

			cellManager.FontName = TemplateStylizer.Theme.PrimaryHeading.Font.FontFamily.Name;
			cellManager.FontSize = TemplateStylizer.Theme.PrimaryHeading.Font.Size;
			cellManager.FontStyle = TemplateStylizer.Theme.PrimaryHeading.Font.Style;
			cellManager.FontColor = TemplateStylizer.Theme.PrimaryHeading.FontColor;

			cellManager.ShouldApply = true;

			void ThrowIfNull(object obj, string objectName)
			{
				_ = obj ?? throw new ArgumentNullException(objectName);
			}
		}
	}
}
