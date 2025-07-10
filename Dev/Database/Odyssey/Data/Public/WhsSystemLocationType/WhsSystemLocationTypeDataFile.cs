using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	internal class WhsSystemLocationTypeDataFile : EmbeddedDataFile
	{
		public WhsSystemLocationTypeDataFile() : base(DataFileRelativePath, new[] { WhsLocationTypeSchema.Constants.TableName })
		{
		}

		internal WhsSystemLocationTypeDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, new[] { WhsLocationTypeSchema.Constants.TableName })
		{
		}

		protected override string SelectQuery
		{
			get { return @"SELECT * FROM dbo.WhsLocationType WHERE WLT_IsSystem = 1 ORDER BY WLT_PK"; }
		}

		#region Implementation

		const string DataFileRelativePath = @"Public\WhsSystemLocationType\WhsSystemLocationType.xml";
		public override string ResourceRelativeName => "WhsSystemLocationType.WhsSystemLocationType.xml";

		#endregion
	}
}
