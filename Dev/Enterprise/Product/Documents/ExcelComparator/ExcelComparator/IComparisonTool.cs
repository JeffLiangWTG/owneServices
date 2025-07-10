namespace Enterprise.ExcelComparator
{
	interface IComparisonTool
	{
		string Path { get; }
		bool IsInstalled();
		void RunComparison(string filePath1, string filePath2);
	}
}
