using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZSqlSaverFactoryTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestCreatesNewSaverWithRetry()
		{
			var data = new DataSet();
			var resolver = ObjectFactory.Get<IApplicationSchemaResolver>();

			var factory = new ZSqlSaverFactory();

			var saver1 = factory.GetSqlSaver(data, Db.Connection, resolver);
			AssertType<ZSqlSaverWithRetry>(saver1);

			var saver2 = factory.GetSqlSaver(data, Db.Connection, resolver);
			AssertType<ZSqlSaverWithRetry>(saver2);

			Assert("Should create a new instance on each method call.", !ReferenceEquals(saver1, saver2));
		}
	}
}
