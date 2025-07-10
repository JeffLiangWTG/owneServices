using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class MainDatabaseTestListener : BaseTestListener
	{
		public override void BeforeEachTest(DateTime startTime)
		{
			base.BeforeEachTest(startTime);
			var dbName = Db.DatabaseName;
			var currentDbName = Db.Connection.CurrentDatabase;
			Assertion.Assert($"Current database [{currentDbName}] should be set to main [{dbName}] before each test", StringComparer.OrdinalIgnoreCase.Equals(dbName, currentDbName));
		}

		public override void AfterEachTest(DateTime endTime)
		{
			base.AfterEachTest(endTime);

			var dbName = Db.DatabaseName;
			var currentDbName = Db.Connection.CurrentDatabase;
			Assertion.Assert($"Current database [{currentDbName}] should be set to main [{dbName}] after each test", StringComparer.OrdinalIgnoreCase.Equals(dbName, currentDbName));
		}
	}
}
