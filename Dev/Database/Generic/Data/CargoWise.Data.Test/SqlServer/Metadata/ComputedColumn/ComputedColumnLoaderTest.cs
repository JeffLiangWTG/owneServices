using System.Diagnostics;
using CargoWise.Data.SqlServer.Metadata.ComputedColumn;
using CargoWise.Database.Shared;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class ComputedColumnLoaderTest : TransactionedTestCase
	{
		public void TestLoadTop1()
		{
			Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.DummyBizo ADD Z0_Computed AS 'A' PERSISTED");
			var info = ComputedColumnLoader.LoadTop1(Db.Connection, "dbo", "DummyBizo", "Z0_Computed");

			AssertNotNull(info);

			var expected = ComputedColumnInfo.Builder.New("dbo", "DummyBizo")
							.Name("Z0_Computed")
							.ComputedExpression("'A'")
							.Persisted()
							.GetInfo();

			AssertEquals(expected, info);
		}

		public void TestToDatabaseFormat()
		{
			var info = ComputedColumnLoader.ToDatabaseFormat(Db.Connection,
				ComputedColumnInfo.Builder.New("dbo", "DummyBizo")
					.Name("Z0_Computed")
					.ComputedExpression("CURRENT_TIMESTAMP")
					.GetInfo());

			AssertEquals("getdate()", info.ColumnExpression);
		}

		[DeveloperOnlyTest]
		public void TestToDatabaseFormat_Performance()
		{
			var stopwatch = Stopwatch.StartNew();
			var info = ComputedColumnLoader.ToDatabaseFormat(Db.Connection,
				ComputedColumnInfo.Builder.New("dbo", "DummyBizo")
					.Name("Z0_Computed")
					.ComputedExpression("CURRENT_TIMESTAMP")
					.GetInfo());
			stopwatch.Start();
			for (var i = 0; i < 1000; i++)
			{
				info = ComputedColumnLoader.ToDatabaseFormat(Db.Connection, info);
			}
			AssertLessThan(stopwatch.ElapsedMilliseconds, 15000);
		}
	}
}
