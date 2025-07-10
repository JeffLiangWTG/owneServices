using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.DocBuilder.Styling.Testing
{
	static class MethodExtensions
	{
		internal static bool CanStylizeForTesting(this RegionCellStylizer cellStylizer, ExcelWorkSheet workSheet, int rowIndex, int columnIndex)
		{
			var cellManager = new StylizerCellManager(workSheet, rowIndex, columnIndex);
			return cellStylizer.CanStylize(cellManager);
		}
	}
}
