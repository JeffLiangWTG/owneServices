using CargoWise.Data;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public interface IRenameTableTransformationInfo
	{
		string SchemaName { get; }
		string OldTableName { get; }
		string NewTableName { get; }
	}

	public class RenameTableTransformationInfo : IRenameTableTransformationInfo
	{
		public RenameTableTransformationInfo(string oldTableName, string newTableName)
		{
			this.SchemaName = Db.SqlDbOwnerSchema;
			this.OldTableName = oldTableName;
			this.NewTableName = newTableName;
		}

		public RenameTableTransformationInfo(string schemaName, string oldTableName, string newTableName)
		{
			this.SchemaName = schemaName;
			this.OldTableName = oldTableName;
			this.NewTableName = newTableName;
		}

		public string SchemaName { get; private set; }
		public string OldTableName { get; private set; }
		public string NewTableName { get; private set; }
	}
}
