namespace Enterprise.DbUpgrader.Transformations
{
	using CargoWise.Schema;

	interface ITransformFkToParentIdAndTableCodeInfo
	{
		string OldFkColumnName { get; }
		SchemaGuidColumn NewParentIDColumn { get; }
		SchemaStringColumn NewParentTableCodeColumn { get; }
	}
}