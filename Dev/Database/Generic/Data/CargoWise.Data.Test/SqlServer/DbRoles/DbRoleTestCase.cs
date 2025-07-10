using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public abstract class DbRoleTestCase<T> : TestCase
		where T : DbRole
	{
		protected sealed override void SetUp()
		{
			base.SetUp();

			TestConnection = Db.NewAdminConnection();
			DbRole = GetTestRole();

			var allSchemasSql = string.Format(CultureInfo.InvariantCulture, @"
SELECT 
	name
FROM
	sys.schemas
WHERE
	name not in ('sys', 'guest', 'INFORMATION_SCHEMA') AND name not like 'db[_]%'
");

			var allSchemaNames = new List<string>();
			TestConnection.ExecuteReader(allSchemasSql, reader => allSchemaNames.Add(reader.GetString(0).Trim()));

			AllSchemaNames = allSchemaNames.ToArray();
		}

		protected sealed override void TearDown()
		{
			if (CleanUpTestDbRole)
			{
				SqlSecurityUtils.DbRole.Drop(TestConnection, DbRole.Name);
			}
			TestConnection.Dispose();
			TestConnection = null;

			base.TearDown();
		}

		protected abstract bool CleanUpTestDbRole { get; }
		protected abstract T GetTestRole();

		protected AdminConnection TestConnection { get; private set; }
		protected T DbRole { get; private set; }
		protected string[] AllSchemaNames { get; private set; }
	}
}
