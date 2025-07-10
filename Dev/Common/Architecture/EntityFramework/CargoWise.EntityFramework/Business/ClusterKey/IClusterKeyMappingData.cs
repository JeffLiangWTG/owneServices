namespace CargoWise.EntityFramework
{
	public interface IClusterKeyMappingData
	{
		string ParentTableName { get; }

		string ParentFkColumnName { get; }

		string SecondaryParentTableName { get; }

		string SecondaryFkColumnName { get; }

		string TableName { get; }

		bool CanBeTopmostTable { get; }
	}
}
