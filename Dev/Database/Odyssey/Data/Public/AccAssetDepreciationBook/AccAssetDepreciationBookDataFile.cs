namespace Enterprise.DbUpgrader.Data
{
	public class AccAssetDepreciationBookDataFile : EmbeddedDataFile
	{
		public AccAssetDepreciationBookDataFile() : base(DataFileRelativePath, TableName)
		{
		}

		internal AccAssetDepreciationBookDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, TableName)
		{
		}

		protected override string SelectQuery =>
			$"SELECT * FROM dbo.{TableName} WHERE ADB_IsSystem = 1 ORDER BY ADB_PK;";

		#region Implementation

		const string DataFileRelativePath = @"Public\AccAssetDepreciationBook\AccAssetDepreciationBook.xml";
		public override string ResourceRelativeName => "AccAssetDepreciationBook.AccAssetDepreciationBook.xml";
		const string TableName = "AccAssetDepreciationBook";

		#endregion
	}
}
