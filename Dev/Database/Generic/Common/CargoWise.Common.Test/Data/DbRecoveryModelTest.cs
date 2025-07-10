using System;
using System.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace CargoWise.Common.Testing.Data
{
	class DbRecoveryModelTest : TestCase
	{
		public void TestFullDbRecoveryModel()
		{
			AssertEquals(1, DbRecoveryModel.Full.Code);
			AssertEquals("FULL", DbRecoveryModel.Full.Name);
			AssertEquals("FULL", DbRecoveryModel.Full.ToString());
		}

		public void TestBulkLoggedDbRecoveryModel()
		{
			AssertEquals(2, DbRecoveryModel.BulkLogged.Code);
			AssertEquals("BULK_LOGGED", DbRecoveryModel.BulkLogged.Name);
			AssertEquals("BULK_LOGGED", DbRecoveryModel.BulkLogged.ToString());
		}

		public void TestSimpleDbRecoveryModel()
		{
			AssertEquals(3, DbRecoveryModel.Simple.Code);
			AssertEquals("SIMPLE", DbRecoveryModel.Simple.Name);
			AssertEquals("SIMPLE", DbRecoveryModel.Simple.ToString());
		}

		public void TestListOfAllDbRecoveryModels()
		{
			var all = DbRecoveryModel.All.ToList();
			AssertContainsExactElementsInAnyOrder(new[] { DbRecoveryModel.Full, DbRecoveryModel.Simple, DbRecoveryModel.BulkLogged }, all);
		}

		public void TestGetByCode()
		{
			AssertEquals(DbRecoveryModel.Full, DbRecoveryModel.Get(1));
			AssertEquals(DbRecoveryModel.BulkLogged, DbRecoveryModel.Get(2));
			AssertEquals(DbRecoveryModel.Simple, DbRecoveryModel.Get(3));
			var ex = AssertExceptionThrown<ArgumentOutOfRangeException>(() => DbRecoveryModel.Get(5));
			AssertEquals("code", ex.ParamName);
		}

		public void TestGetByName()
		{
			AssertEquals(DbRecoveryModel.Full, DbRecoveryModel.Get("full"));
			AssertEquals(DbRecoveryModel.Full, DbRecoveryModel.Get("FULL"));
			AssertEquals(DbRecoveryModel.Simple, DbRecoveryModel.Get("simple"));
			AssertEquals(DbRecoveryModel.Simple, DbRecoveryModel.Get("SIMPLE"));
			AssertEquals(DbRecoveryModel.BulkLogged, DbRecoveryModel.Get("bulk_logged"));
			AssertEquals(DbRecoveryModel.BulkLogged, DbRecoveryModel.Get("BULK_LOGGED"));
			var ex = AssertExceptionThrown<ArgumentOutOfRangeException>(() => DbRecoveryModel.Get("invalid"));
			AssertEquals("name", ex.ParamName);
		}
	}
}
