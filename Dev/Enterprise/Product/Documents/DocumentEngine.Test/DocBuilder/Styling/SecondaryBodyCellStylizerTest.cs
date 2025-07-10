using System.Drawing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine.DocBuilder.Styling.Testing
{
	sealed class SecondaryBodyCellStylizerTest : CellStylizerTest<SecondaryBodyCellStylizer>
	{
		public override void TestCanStylize()
		{
			var stylizer = new TemplateStylizer(WorkSheet.ParentExcelInterface, DocumentsDataRegistry.Instance.DocBuilderTheme.Value.SelectedTheme);

			var cellStylizerForNormalCell = new SecondaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForNormalCell.CanStylize", false, cellStylizerForNormalCell.CanStylizeForTesting(WorkSheet, 0, 0));

			var cellStylizerFordocumentHeading = new SecondaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerFordocumentHeading.CanStylize", false, cellStylizerFordocumentHeading.CanStylizeForTesting(WorkSheet, 4, 2));

			var cellStylizerForPageNumberHeading = new SecondaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForPageNumberHeading.CanStylize", false, cellStylizerForPageNumberHeading.CanStylizeForTesting(WorkSheet, 4, 3));

			var cellStylizerForPrimaryHeading = new SecondaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForPrimaryHeading.CanStylize", false, cellStylizerForPrimaryHeading.CanStylizeForTesting(WorkSheet, 6, 2));

			var cellStylizerForPrimaryBody = new SecondaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForPrimaryBody.CanStylize", false, cellStylizerForPrimaryBody.CanStylizeForTesting(WorkSheet, 6, 3));

			var cellStylizerForSecondaryHeading = new SecondaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForSecondaryHeading.CanStylize", false, cellStylizerForSecondaryHeading.CanStylizeForTesting(WorkSheet, 8, 2));

			var cellStylizerForSecondaryBody = new SecondaryBodyCellStylizer(stylizer);
			AssertEquals("cellStylizerForSecondaryBody.CanStylize", true, cellStylizerForSecondaryBody.CanStylizeForTesting(WorkSheet, 9, 2));
		}

		public override void TestStylize()
		{
			var theme = new DocBuilderTheme("Test", false);
			theme.SecondaryBody.Color1 = Color.Red;
			theme.SecondaryBody.Color2 = Color.Blue;
			theme.SecondaryBody.Font = new Font("Courier New", 18, FontStyle.Bold | FontStyle.Italic);
			theme.SecondaryBody.FontColor = Color.Green;

			var templateStylizer = new TemplateStylizer(WorkSheet.ParentExcelInterface, theme);
			var cellStylizer = new SecondaryBodyCellStylizer(templateStylizer);
			var cellManager = templateStylizer.GetCellManager(WorkSheet, 9, 2);
			cellStylizer.Stylize(cellManager);

			AssertEquals("BackgroundPattern", FillPatternStyle.None, cellManager.BackgroundPattern);
			AssertEquals("Borders.Top", Color.Blue.ToArgb(), cellManager.Borders.Top.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
			AssertEquals("Borders.Left", Color.Blue.ToArgb(), cellManager.Borders.Left.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
			AssertEquals("Borders.Bottom", Color.Blue.ToArgb(), cellManager.Borders.Bottom.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
			AssertEquals("Borders.Right", Color.Blue.ToArgb(), cellManager.Borders.Right.Color.ToColor(WorkSheet.ParentExcelInterface.Xls).ToArgb());
		}
	}
}
