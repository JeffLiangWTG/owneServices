using System;
using System.Data;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Public.Test
{
	internal static class StmALogGeneratorForTest
	{
		public static Guid NewLog(Guid parent, string table, string eventCode, bool isCancelled)
		{
			const string sql = @"
INSERT INTO dbo.StmALog (SL_PK, SL_SE_NKEvent, SL_IsCancelled, SL_Parent, SL_Table, SL_EventTime)
VALUES (@PK, @EventCode, @IsCancelled, @Parent, @Table, @EventTime)
";

			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@EventCode", SqlDbType.Char, 3, eventCode);
				command.AddParameter("@IsCancelled", SqlDbType.Char, 1, isCancelled ? "Y" : "N");
				command.AddParameter("@Parent", SqlDbType.UniqueIdentifier, parent);
				command.AddParameter("@EventTime", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@Table", SqlDbType.VarChar, 35, table);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid NewLog(string eventCode)
		{
			return NewLog(Guid.NewGuid(), "DummyBizO", eventCode, false);
		}
	}
}

