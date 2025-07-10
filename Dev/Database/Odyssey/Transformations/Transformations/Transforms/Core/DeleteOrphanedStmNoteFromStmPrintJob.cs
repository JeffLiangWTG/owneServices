namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	class DeleteOrphanedStmNoteFromStmPrintJob : DeleteOrphanedStmNoteRecords
	{
		protected override string ParentTableName => "StmPrintJob";
		protected override string ParentPKColumnName => "SP_PK";

		public override string UserDescription => "Delete Orphaned StmNote From StmPrintJob";
	}
}
