using System;
using System.Data;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Public.Test
{
	internal static class ProcessTasksGeneratorForTest
	{
		public static Guid NewProcessTask(string type, bool isValid)
		{
			const string sql = @"
insert into dbo.ProcessTasks(P9_PK, P9_Type, P9_IsValid, P9_SystemCreateTimeUtc, P9_SystemCreateUser, P9_SystemLastEditTimeUtc, P9_SystemLastEditUser)
values(@PK, @Type, @IsValid, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@Type", SqlDbType.Char, 3, type);
				command.AddParameter("@IsValid", SqlDbType.Bit, isValid);

				command.ExecuteNonQuery();
			}

			return pk;
		}
	}
}

