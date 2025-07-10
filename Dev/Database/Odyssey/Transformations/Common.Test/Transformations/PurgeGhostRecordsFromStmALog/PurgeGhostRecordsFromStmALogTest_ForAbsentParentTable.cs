using System;
using CargoWise.Data.Testing;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	[UseSnapshotProtection(skipTransaction: true)] // we want to test it outside transactions - that's how online transformations run
	class PurgeGhostRecordsFromStmALogTest_ForAbsentParentTable : PurgeGhostRecordsFromStmALogTestCase
	{
		protected override PurgeGhostRecordsFromStmALog GetNewTestTransformationInstance() => new PurgeProcessTasksFromStmALogForTest_WithAbsentParentTable();
		protected override string GetParentTableName() => "ProcessTasks";
		protected override string GetParentTableColumnNames() => "P9_PK, P9_ParentTableCode, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser";
		protected override string GetValuesToInsertIntoParentTable(Guid pk, int i) => $"'{pk}', 'JE', GetUtcDate(), '~BP', GetUtcDate(), '~BP'";
	}

	class PurgeProcessTasksFromStmALogForTest_WithAbsentParentTable : PurgeGhostRecordsFromStmALog
	{
		public override string UserDescription => "Test transformation";
		public override bool ParentTableExists => false;
		protected override string ParentTableName => "ProcessTasks";
	}
}
