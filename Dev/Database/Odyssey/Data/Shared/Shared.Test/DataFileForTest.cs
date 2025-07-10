using System.Reflection;

namespace Enterprise.DbUpgrader.Data.Testing
{
	sealed class DataFileForTest : EmbeddedDataFile
	{
		public DataFileForTest() : base(TestFileConstants.TestDataFileRelativeResourcePath, "StmMenuTemplatePivot", "StmTemplate", "StmMenuItem")
		{
		}

		public DataFileForTest(string fileRelativePath, params string[] tableNames) : base(fileRelativePath, tableNames)
		{
		}

		protected override Assembly ResourceAssembly
		{
			get { return GetType().Assembly; }
		}
	}
}
