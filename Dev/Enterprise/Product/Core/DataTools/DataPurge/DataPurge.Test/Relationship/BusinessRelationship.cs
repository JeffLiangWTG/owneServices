using CargoWise.Schema;

namespace Enterprise.DataPurge
{
	public class BusinessRelationship
	{
		public BusinessRelationship(string parentTableName, SchemaColumn parentKeyColumnReferencedByChild, string childTableName, SchemaColumn childFkColumn, string justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords = null)
		{
			ParentKeyColumnReferencedByChild = parentKeyColumnReferencedByChild;
			ChildFkColumn = childFkColumn;
			ParentTableName = parentTableName;
			ChildTableName = childTableName;
			JustificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords = justificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords;
		}

		public string ParentTableName { get; init; }

		public SchemaColumn ParentKeyColumnReferencedByChild { get; init; }

		public string ChildTableName { get; init; }

		public SchemaColumn ChildFkColumn { get; init; }

		public string JustificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords { get; init; }

		public bool ForcingTest =>
			string.IsNullOrEmpty(JustificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords);

		public override string ToString()
		{
			var relation = $"[{ParentTableName}].[{ParentKeyColumnReferencedByChild.Name}] is referenced by [{ChildTableName}.{ChildFkColumn.Name}]";
			return ForcingTest ? relation : $"{relation}, the justification for whitelisting is \"{JustificationForWhitelisting_MightCauseDataPurgeFailureOrOrphanedRecords}\"";
		}
	}
}
