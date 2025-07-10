using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations
{
	sealed class SourceTableForTesting : SourceTable
	{
		public SourceTableForTesting(string originalDb, string originalSchema, string originalName, SourceColumn[] columns, string whereClause)
			: base(originalDb, originalSchema, originalName, columns, whereClause)
		{
		}

		public SourceTableForTesting(string originalDb, string originalName, SourceColumn[] columns, string whereClause)
			: base(originalDb, originalName, columns, whereClause)
		{
		}

		public string DataCopyStorageDb_Exposed
		{
			get { return DataCopyStorageDb; }
		}

		public string OriginalFullyQualifiedName_Exposed
		{
			get { return OriginalFullyQualifiedName; }
		}

		public string FullName_Exposed
		{
			get { return FullName; }
		}
	}
}
