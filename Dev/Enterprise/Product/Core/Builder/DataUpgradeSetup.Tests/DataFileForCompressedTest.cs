using Enterprise.DbUpgrader.Data;
using Enterprise.DbUpgrader.Data.Testing;

namespace Enterprise.Builder.DataUpgradeSetup.Testing
{
	public class DataFileForCompressedTest : EmbeddedDataFile
	{
		public DataFileForCompressedTest()
				: base(FilePathHelperTest.CompressedTestDataFilePath, "StmMenuTemplatePivot", "StmTemplate", "StmMenuItem")
		{
		}

		public override string FileResourceName
		{
			get { return "Enterprise.DbUpgrader.Data.Shared.Test.TestFiles.TestDataFile.xml.gz"; }
		}

		protected override System.Reflection.Assembly ResourceAssembly
		{
			get { return typeof(TestFileConstants).Assembly; }
		}

		public override string DefaultDataFileBasePath
		{
			get
			{
				return "";
			}
		}
	}
}
