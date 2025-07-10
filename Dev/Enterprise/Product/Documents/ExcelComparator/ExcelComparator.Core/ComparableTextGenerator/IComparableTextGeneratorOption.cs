
namespace Enterprise.ExcelComparator
{
	public interface IComparableTextGeneratorOption
	{
		string RunOptionOnWorkSheetContents(string original, int workSheetNumber);
	}
}
