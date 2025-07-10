using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.ClusterKey
{
	sealed class ClusterKeyMappingData : IClusterKeyMappingData
	{
		public ClusterKeyMappingData(string tableName, string parentTableName, string parentFkColumnName)
		{
			this.tableName = tableName;
			this.parentTableName = parentTableName;
			this.parentFkColumnName = parentFkColumnName;
		}
		readonly string parentTableName;
		readonly string parentFkColumnName;
		readonly string tableName;

		public void SetSecondaryParentInformation(string tableName, string fkColumnName)
		{
			this.secondaryParentTableName = tableName;
			this.secondaryFkColumnName = fkColumnName;
		}

		public void SetCanBeTopmostTableToTrue() => canBeTopmostTable = true;

		#region IClusterKeyMappingData

		string IClusterKeyMappingData.ParentTableName => parentTableName;

		string IClusterKeyMappingData.ParentFkColumnName => parentFkColumnName;

		string IClusterKeyMappingData.SecondaryParentTableName => secondaryParentTableName;

		string IClusterKeyMappingData.SecondaryFkColumnName => secondaryFkColumnName;

		string IClusterKeyMappingData.TableName => tableName;

		bool IClusterKeyMappingData.CanBeTopmostTable => canBeTopmostTable;

		#endregion

		string secondaryParentTableName;
		string secondaryFkColumnName;
		bool canBeTopmostTable;
	}
}
