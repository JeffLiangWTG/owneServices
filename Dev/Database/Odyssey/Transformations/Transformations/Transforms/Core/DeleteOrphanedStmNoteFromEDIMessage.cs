namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	class DeleteOrphanedStmNoteFromEDIMessage : DeleteOrphanedStmNoteRecords
	{
		protected override string ParentTableName => "EDIMessage";
		protected override string ParentPKColumnName => "EM_PK";

		public override string UserDescription => "Delete Orphaned StmNote From EDIMessage";
	}
}
