using System.Drawing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.DocBuilder.Styling.Testing
{
	sealed class PrimaryHeadingCellStylizerTest : CellStylizerTest<PrimaryHeadingCellStylizer>
	{
		public override void TestCanStylize()
		{
			var stylizer = new TemplateStylizer(WorkSheet.ParentExcelInterface, DocumentsDataRegistry.Instance.DocBuilderTheme.Value.SelectedTheme);

			var cellStylizerForNormalCell = new PrimaryHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForNormalCell.CanStylize", false, cellStylizerForNormalCell.CanStylizeForTesting(WorkSheet, 0, 0));

			var cellStylizerFordocumentHeading = new PrimaryHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerFordocumentHeading.CanStylize", false, cellStylizerFordocumentHeading.CanStylizeForTesting(WorkSheet, 4, 2));

			var cellStylizerForPageNumberHeading = new PrimaryHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForPageNumberHeading.CanStylize", false, cellStylizerForPageNumberHeading.CanStylizeForTesting(WorkSheet, 4, 3));

			var cellStylizerForPrimaryHeading = new PrimaryHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForPrimaryHeading.CanStylize", true, cellStylizerForPrimaryHeading.CanStylizeForTesting(WorkSheet, 6, 2));

			var cellStylizerForPrimaryBody = new PrimaryHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForPrimaryBody.CanStylize", false, cellStylizerForPrimaryBody.CanStylizeForTesting(WorkSheet, 6, 3));

			var cellStylizerForSecondaryHeading = new PrimaryHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForSecondaryHeading.CanStylize", false, cellStylizerForSecondaryHeading.CanStylizeForTesting(WorkSheet, 8, 2));

			var cellStylizerForSecondaryBody = new PrimaryHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForSecondaryBody.CanStylize", false, cellStylizerForSecondaryBody.CanStylizeForTesting(WorkSheet, 9, 2));
		}

		public override void TestStylize()
		{
			var theme = new DocBuilderTheme("Test", false);
			theme.PrimaryHeading.Color1 = Color.Red;
			theme.PrimaryHeading.Color2 = Color.Blue;
			theme.PrimaryHeading.Font = new Font("Courier New", 18, FontStyle.Bold | FontStyle.Italic);
			theme.PrimaryHeading.FontColor = Color.Green;

			var templateStylizer = new TemplateStylizer(WorkSheet.ParentExcelInterface, theme);
			var cellStylizer = new PrimaryHeadingCellStylizer(templateStylizer);
			var cellManager = templateStylizer.GetCellManager(WorkSheet, 6, 2);
			cellStylizer.Stylize(cellManager);

			AssertEquals("BackgroundColor", Color.Red.ToArgb(), cellManager.BackgroundColor.ToArgb());
			AssertEquals("BackgroundPattern", FillPatternStyle.Solid, cellManager.BackgroundPattern);
			AssertEquals("Borders.Top", Color.Blue.ToArgb(), cellManager.Borders.Top.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
			AssertEquals("Borders.Left", Color.Blue.ToArgb(), cellManager.Borders.Left.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
			AssertEquals("Borders.Bottom", Color.Blue.ToArgb(), cellManager.Borders.Bottom.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
			AssertEquals("Borders.Right", Color.Blue.ToArgb(), cellManager.Borders.Right.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
			AssertEquals("FontName", "Courier New", cellManager.FontName);
			AssertEquals("FontSize", 18f, cellManager.FontSize);
			AssertEquals("FontStyle", FontStyle.Bold | FontStyle.Italic, cellManager.FontStyle);
			AssertEquals("FontColor", Color.Green.ToArgb(), cellManager.FontColor.ToArgb());
		}
	}
}
