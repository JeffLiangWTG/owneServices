using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class StmSystemDefinedFieldDataFile : EmbeddedDataFile
	{
		public StmSystemDefinedFieldDataFile() : base(DataFileRelativePath, StmSystemDefinedFieldSchema.Constants.TableName)
		{
		}

		internal StmSystemDefinedFieldDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, StmSystemDefinedFieldSchema.Constants.TableName)
		{
		}

		protected override string SelectQuery
		{
			get { return string.Format("SELECT * FROM {0} order by 1", StmSystemDefinedFieldSchema.Constants.TableName); }
		}

		public override string ResourceRelativeName => "StmSystemDefinedField.StmSystemDefinedField.xml";

		#region Implementation

		const string DataFileRelativePath = @"Public\StmSystemDefinedField\StmSystemDefinedField.xml";

		#endregion
	}
}
