// -----------------------------------------------------------------------
// <copyright file="IRenameColumnInfo.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Enterprise.DbUpgrader.Transformation.Common
{
	/// <summary>
	/// TODO: Update summary.
	/// </summary>
	public interface IRenameColumnTransformationInfo
	{
		string SchemaName { get; }
		string TableName { get; }
		string OldColumnName { get; }
		string NewColumnName { get; }
	}

	public class RenameColumnTransformationInfo : IRenameColumnTransformationInfo
	{
		public RenameColumnTransformationInfo(string schemaName, string tableName, string oldColumnName, string newColumnName)
		{
			SchemaName = schemaName;
			TableName = tableName;
			OldColumnName = oldColumnName;
			NewColumnName = newColumnName;
		}

		public RenameColumnTransformationInfo(string tableName, string oldColumnName, string newColumnName)
			: this(CargoWise.Data.Db.SqlDbOwnerSchema, tableName, oldColumnName, newColumnName)
		{
		}

		public string SchemaName { get; private set; }
		public string TableName { get; private set; }
		public string OldColumnName { get; private set; }
		public string NewColumnName { get; private set; }
	}
}
