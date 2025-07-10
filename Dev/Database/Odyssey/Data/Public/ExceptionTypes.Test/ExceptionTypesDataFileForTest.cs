using System.IO;

namespace Enterprise.DbUpgrader.Data.Testing
{
	class ExceptionTypesDataFileForTest : ExceptionTypesDataFile
	{
		public ExceptionTypesDataFileForTest() : base(DataFileRelativePath)
		{
		}

		const string DataFileRelativePath = @"ExceptionTypes.Testing\ExceptionTypes.xml";

		public override string DefaultDataFileBasePath
		{
			get
			{
				return Path.Combine(TestFileConstants.BaseSourcePath, @"Database\Odyssey\Data\Public\");
			}
		}
	}
}
