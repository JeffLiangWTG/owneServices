using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[UseSnapshotProtection]
	sealed class EventManagerNonTransactionedTest : TestCase
	{
		public void TestCallWithNoTransaction()
		{
			AssertEquals(true, DataUtils.ObjectExists(Db.Connection, "TG_CP_INS_StmALogQueue"));

			Db.Connection.ExecuteNonQuery(@"
CREATE TRIGGER TG_TestTrigger
	ON StmALog
	AFTER INSERT

AS
BEGIN
	DECLARE @isSuspended int;
	SET @isSuspended = (SELECT Result FROM dbo.IsTriggerSuspended('TG_CP_INS_StmALogQueue')) -- This has an error if not in a transaction.
END
"
				);

			AssertNoExceptionThrown(() =>
			{
				new EventManager().AddAuditLogEvent("ADD", "TableName", Guid.NewGuid(), DateTime.Now);
			});
		}
	}
}
