using System.Data;
using CargoWise.Data;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZSqlDataAccessorTest : TestCase
	{
		class DummySqlDataAccessor : ZSqlDataAccessor
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			public DummySqlDataAccessor(DataSet data, ZSqlConnectionInfo connectionInfo)
				: base(data, connectionInfo)
			{
			}

			public void TestLoaderSaver()
			{
				Assert("Sql saver", Saver is ZSqlSaver);
				Assert("Sql loader", Loader is ZSqlLoader);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			public new DataSet Data
			{
				get
				{
					return base.Data;
				}
			}

			public new ZSqlConnectionInfo ConnectionInfo
			{
				get
				{
					return base.ConnectionInfo;
				}
			}
		}

		[ExpectNoExceptions()]
		public void TestLoaderSaver()
		{
			DummySqlDataAccessor accessor = new DummySqlDataAccessor(TestData, TestConnectionInfo);
			accessor.TestLoaderSaver();
		}

		public void TestDataAndConnectionInfo()
		{
			DummySqlDataAccessor accessor = new DummySqlDataAccessor(TestData, TestConnectionInfo);
			AssertEquals("Dataset", TestData, accessor.Data);
			AssertEquals("ConnectionInfo", TestConnectionInfo, accessor.ConnectionInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		readonly DataSet TestData = new DataSet();
		readonly ZSqlConnectionInfo TestConnectionInfo = new ZSqlConnectionInfo(Db.Connection, "");
	}
}
