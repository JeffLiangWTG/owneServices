namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class DataFileForCharCaseComparisonTest : EmbeddedDataFile
	{
		public DataFileForCharCaseComparisonTest()
			: base(TestFileConstants.TestDataFileRelativeResourcePath, "StmTemplate")
		{
		}
	}
}
