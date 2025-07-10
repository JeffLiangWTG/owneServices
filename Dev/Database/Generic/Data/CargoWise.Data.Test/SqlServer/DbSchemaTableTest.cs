using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Data.SqlServer.Testing
{
	sealed class DbSchemaTableTest : TestCase
	{
		public void TestHashing()
		{
			var t1 = new DbSchemaTable("odysey.dbo.orgheader");
			var t2 = new DbSchemaTable("Odysey.Dbo.Orgheader");
			var t3 = new DbSchemaTable("odysey.dbo.orgheader");
			var t4 = new DbSchemaTable("odysey.dbo.orgheaderx");

			var hs = new HashSet<DbSchemaTable>(new[] { t1, t2, t3, t4 });
			AssertEquals(2, hs.Count);
			Assert(hs.Contains(t4));
		}

		public void TestEmptySchema()
		{
			var t = new DbSchemaTable("odysey..orgheader");
			AssertEquals("dbo", t.SchemaName);
			AssertEquals("[odysey].[dbo].[orgheader]", t.ToString());

			t = new DbSchemaTable("odysey", string.Empty, "orgheader");
			AssertEquals("dbo", t.SchemaName);
			AssertEquals("[odysey].[dbo].[orgheader]", t.ToString());

			t = new DbSchemaTable("odysey", null, "orgheader");
			AssertEquals("dbo", t.SchemaName);
			AssertEquals("[odysey].[dbo].[orgheader]", t.ToString());
		}
	}
}
