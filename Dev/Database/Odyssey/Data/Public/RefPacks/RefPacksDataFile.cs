using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class RefPacksDataFile : EmbeddedDataFile
	{
		public RefPacksDataFile()
			: base(DataFileRelativePath, new string[1] { RefPacksSchema.Constants.TableName })
		{
		}

		public override string ResourceRelativeName => "RefPacks.RefPacks.xml";

		#region Implementation

		const string DataFileRelativePath = @"Public\RefPacks\RefPacks.xml";

		#endregion
	}
}
