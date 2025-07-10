namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public class CellFormatterUsingFormattedValue : ICellFormatter
	{
		public string Format(ExcelWorkSheet cellSource, int row, int column)
		{
			var cell = cellSource.GetCell(row, column);
			return cell.FormattedValue;
		}
	}
}