using System;
using System.Data;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Public.Test
{
	internal static class GlbGeneratorForTests
	{
		public static Guid NewGroup(string code)
		{
			const string sql = @"
insert into dbo.GlbGroup(GG_PK, GG_Code, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser)
values(@pk, @code, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, code);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid NewStaff(string code, params string[] groups)
		{
			const string sql = @"
DECLARE @PerPk UNIQUEIDENTIFIER = newid()
INSERT INTO dbo.GlbPerson(PER_PK, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser) values (@PerPk, 'name', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

insert into dbo.GlbStaff(GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
values(@pk, @code, '<' + @code + '>', @PerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

			const string addSql = @"
insert into dbo.GlbGroupLink(GK_PK, GK_GS, GK_GG)
select
	newid(),
	@staffPK,
	GG_PK
from dbo.GlbGroup
where GG_Code = @code
";

			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, code);

				command.ExecuteNonQuery();
			}

			if (groups != null)
			{
				foreach (var group in groups)
				{
					using (var command = Db.Connection.Command(addSql))
					{
						command.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, pk);
						command.AddParameter("@code", SqlDbType.VarChar, group);

						command.ExecuteNonQuery();
					}
				}
			}

			return pk;
		}
	}
}

