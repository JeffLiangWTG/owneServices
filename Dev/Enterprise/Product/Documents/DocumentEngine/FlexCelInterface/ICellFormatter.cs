
namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public interface ICellFormatter
	{
		string Format(ExcelWorkSheet cellSource, int row, int column);
	}
}
