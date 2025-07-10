using System.Drawing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.DocBuilder.Styling.Testing
{
	sealed class PrimaryBodyCellStylizerTest : CellStylizerTest<PrimaryBodyCellStylizer>
	{
		public override void TestCanStylize()
		{
			var stylizer = new TemplateStylizer(WorkSheet.ParentExcelInterface, DocumentsDataRegistry.Instance.DocBuilderTheme.Value.SelectedTheme);

			var cellStylizerForNormalCell = new PrimaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForNormalCell.CanStylize", false, cellStylizerForNormalCell.CanStylizeForTesting(WorkSheet, 0, 0));

			var cellStylizerFordocumentHeading = new PrimaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerFordocumentHeading.CanStylize", false, cellStylizerFordocumentHeading.CanStylizeForTesting(WorkSheet, 4, 2));

			var cellStylizerForPageNumberHeading = new PrimaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForPageNumberHeading.CanStylize", false, cellStylizerForPageNumberHeading.CanStylizeForTesting(WorkSheet, 4, 3));

			var cellStylizerForPrimaryHeading = new PrimaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForPrimaryHeading.CanStylize", false, cellStylizerForPrimaryHeading.CanStylizeForTesting(WorkSheet, 6, 2));

			var cellStylizerForPrimaryBody = new PrimaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForPrimaryBody.CanStylize", true, cellStylizerForPrimaryBody.CanStylizeForTesting(WorkSheet, 6, 3));

			var cellStylizerForSecondaryHeading = new PrimaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForSecondaryHeading.CanStylize", false, cellStylizerForSecondaryHeading.CanStylizeForTesting(WorkSheet, 8, 2));

			var cellStylizerForSecondaryBody = new PrimaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForSecondaryBody.CanStylize", false, cellStylizerForSecondaryBody.CanStylizeForTesting(WorkSheet, 9, 2));
		}

		public override void TestStylize()
		{
			var theme = new DocBuilderTheme("Test", false);
			theme.PrimaryBody.Color1 = Color.Red;
			theme.PrimaryBody.Color2 = Color.Blue;
			theme.PrimaryBody.Font = new Font("Courier New", 18, FontStyle.Bold | FontStyle.Italic);
			theme.PrimaryBody.FontColor = Color.Green;

			var templateStylizer = new TemplateStylizer(WorkSheet.ParentExcelInterface, theme);
			var cellStylizer = new PrimaryBodyCellStylizer(templateStylizer);
			var cellManager = templateStylizer.GetCellManager(WorkSheet, 6, 3);
			cellStylizer.Stylize(cellManager);

			AssertEquals("BackgroundPattern", FillPatternStyle.None, cellManager.BackgroundPattern);
			AssertEquals("Borders.Top", Color.Blue.ToArgb(), cellManager.Borders.Top.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
			AssertEquals("Borders.Left", Color.Blue.ToArgb(), cellManager.Borders.Left.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
			AssertEquals("Borders.Bottom", Color.Blue.ToArgb(), cellManager.Borders.Bottom.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
			AssertEquals("Borders.Right", Color.Blue.ToArgb(), cellManager.Borders.Right.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
		}

		public void TestStyliseLastColumnToStylise_ShouldNotGoBeyond()
		{
			var theme = new DocBuilderTheme("Test", false);
			theme.PrimaryBody.Color1 = Color.Red;
			theme.PrimaryBody.Color2 = Color.Blue;
			theme.PrimaryBody.Font = new Font("Courier New", 18, FontStyle.Bold | FontStyle.Italic);
			theme.PrimaryBody.FontColor = Color.Green;

			var templateStylizer = new TemplateStylizer(WorkSheet.ParentExcelInterface, theme);
			var cellStylizer = new PrimaryBodyCellStylizer(templateStylizer);
			var manager = templateStylizer.GetCellManager(WorkSheet, 6, 71);

			AssertNoExceptionThrown(() => cellStylizer.Stylize(manager));
		}

		public void TestStyliseToEdgeOfWorksheet_ShouldNotGoBeyond()
		{
			var theme = new DocBuilderTheme("Test", false);
			theme.PrimaryBody.Color1 = Color.Red;
			theme.PrimaryBody.Color2 = Color.Blue;
			theme.PrimaryBody.Font = new Font("Courier New", 18, FontStyle.Bold | FontStyle.Italic);
			theme.PrimaryBody.FontColor = Color.Green;

			var templateStylizer = new TemplateStylizer(WorkSheet.ParentExcelInterface, theme);
			var cellStylizer = new PrimaryBodyCellStylizer(templateStylizer);
			var manager = templateStylizer.GetCellManager(WorkSheet, 10, 71);

			AssertNoExceptionThrown(() => cellStylizer.Stylize(manager));
		}
	}
}
