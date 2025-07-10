using System.Drawing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.DocBuilder.Styling.Testing
{
	sealed class DocumentHeadingCellStylizerTest : CellStylizerTest<DocumentHeadingCellStylizer>
	{
		public override void TestCanStylize()
		{
			var stylizer = new TemplateStylizer(WorkSheet.ParentExcelInterface, DocumentsDataRegistry.Instance.DocBuilderTheme.Value.SelectedTheme);

			var cellStylizerForNormalCell = new DocumentHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForNormalCell.CanStylize", false, cellStylizerForNormalCell.CanStylizeForTesting(WorkSheet, 0, 0));

			var cellStylizerFordocumentHeading = new DocumentHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerFordocumentHeading.CanStylize", true, cellStylizerFordocumentHeading.CanStylizeForTesting(WorkSheet, 4, 2));

			var cellStylizerForPageNumberHeading = new DocumentHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForPageNumberHeading.CanStylize", false, cellStylizerForPageNumberHeading.CanStylizeForTesting(WorkSheet, 4, 3));

			var cellStylizerForPrimaryHeading = new DocumentHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForPrimaryHeading.CanStylize", false, cellStylizerForPrimaryHeading.CanStylizeForTesting(WorkSheet, 6, 2));

			var cellStylizerForPrimaryBody = new DocumentHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForPrimaryBody.CanStylize", false, cellStylizerForPrimaryBody.CanStylizeForTesting(WorkSheet, 6, 3));

			var cellStylizerForSecondaryHeading = new DocumentHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForSecondaryHeading.CanStylize", false, cellStylizerForSecondaryHeading.CanStylizeForTesting(WorkSheet, 8, 2));

			var cellStylizerForSecondaryBody = new DocumentHeadingCellStylizer(stylizer);
			AssertEquals("cellStylizerForSecondaryBody.CanStylize", false, cellStylizerForSecondaryBody.CanStylizeForTesting(WorkSheet, 9, 2));
		}

		public override void TestStylize()
		{
			var theme = new DocBuilderTheme("Test", false);
			theme.DocumentHeading.Color1 = Color.Red;
			theme.DocumentHeading.Color2 = Color.Blue;
			theme.DocumentHeading.Font = new Font("Courier New", 18, FontStyle.Bold | FontStyle.Italic);
			theme.DocumentHeading.FontColor = Color.Green;

			var templateStylizer = new TemplateStylizer(WorkSheet.ParentExcelInterface, theme);
			var cellStylizer = new DocumentHeadingCellStylizer(templateStylizer);
			var cellManager = templateStylizer.GetCellManager(WorkSheet, 4, 2);
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
