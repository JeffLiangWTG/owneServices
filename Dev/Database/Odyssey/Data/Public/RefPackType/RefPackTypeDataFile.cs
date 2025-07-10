using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class RefPackTypeDataFile : EmbeddedDataFile
	{
		public RefPackTypeDataFile() : base(DataFileRelativePath, RefPackTypeSchema.Constants.TableName)
		{
		}

		internal RefPackTypeDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, RefPackTypeSchema.Constants.TableName)
		{
		}

		public override string ResourceRelativeName => "RefPackType.RefPackType.xml";

		#region Implementation

		const string DataFileRelativePath = @"Public\RefPackType\RefPackType.xml";

		#endregion
	}
}
