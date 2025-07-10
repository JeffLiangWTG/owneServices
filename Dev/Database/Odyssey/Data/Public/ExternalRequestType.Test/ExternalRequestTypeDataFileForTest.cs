using System.IO;

namespace Enterprise.DbUpgrader.Data.Testing
{
	class ExternalRequestTypeDataFileForTest : ExternalRequestTypeDataFile
	{
		public ExternalRequestTypeDataFileForTest() : base(DataFileRelativePath)
		{
		}

		const string DataFileRelativePath = @"ExternalRequestType.Testing\ExternalRequestType.xml";

		public override string DefaultDataFileBasePath
		{
			get
			{
				return Path.Combine(TestFileConstants.BaseSourcePath, @"Database\Odyssey\Data\Public\");
			}
		}
	}
}
