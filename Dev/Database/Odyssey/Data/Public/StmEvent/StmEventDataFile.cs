using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class StmEventDataFile : EmbeddedDataFile
	{
		public StmEventDataFile() : base(DataFileRelativePath, StmEventSchema.Constants.TableName)
		{
		}

		internal StmEventDataFile(string customDataFileRelativePath) : base(customDataFileRelativePath, StmEventSchema.Constants.TableName)
		{
		}

		public override string ResourceRelativeName => "StmEvent.StmEvent.xml";

		#region Implementation

		const string DataFileRelativePath = @"Public\StmEvent\StmEvent.xml";

		#endregion
	}
}
