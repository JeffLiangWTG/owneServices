using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.StmNums;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.StmNums.Testing
{
	[TestedType(typeof(TG_StmNums_Insert))]
	class TG_StmNums_InsertTest : DbCreateScriptTest
	{
		public void TestInsert()
		{
			var ownerPK = Guid.NewGuid();
			var stmNumsPK = helper.InsertView(ownerPK, "OrgOwned_SSC", 1, 6000, 0);

			AssertEquals(true, helper.ExistsInTable(ownerPK, "OrgOwned_SSC", 1, 6000));
			AssertEquals(true, helper.ExistsInView(stmNumsPK));
		}

		public void TestInsertsLongName()
		{
			// Arrange
			var ownerPK = Guid.NewGuid();
			var name = "OrgOwned_SSC" + string.Join(string.Empty, Enumerable.Range(1, 244).Select(i => i % 10).Select(i => i.ToString()));

			// Act
			helper.InsertView(ownerPK, name, 1, 6000, 0);

			// Assert
			AssertEquals(true, helper.ExistsInTable(ownerPK, name, 1, 6000));
		}

		public void TestDuplicateFountainThrowsError()
		{
			var ownerPK = Guid.NewGuid();
			helper.InsertView(ownerPK, "OrgOwned_SSC", 1, 6000, 0);

			var ex = AssertExceptionThrown<SqlException>(() => helper.InsertView(ownerPK, "OrgOwned_SSC", 1, 6000, 0));
			AssertEquals(@"Fountain Already Existed.", ex.Message);
		}

		#region Implementation

		ViewStmNumsTestHelper helper;

		protected override void SetUp()
		{
			base.SetUp();
			helper = new ViewStmNumsTestHelper(TestConnection);
		}
		#endregion
	}

	[UseSnapshotProtection]
	class TG_StmNums_InsertSnapshotTest : TestCase
	{
		public void TestSameTransactionCreatedHeaderRuinsGetNext()
		{
			// Arrange
			var ownerPk = Guid.Empty;
			const string name = "OrgOwned_SSC";
			const int minValue = 1;
			const int maxValue = 6000;
			const int canRollover = 0;

			using (Db.Connection.BeginTransactionWithManager())
			{
				var helper = new ViewStmNumsTestHelper(Db.Connection);
				helper.InsertView(ownerPk, name, minValue, maxValue, canRollover);

				using (var command = Db.Connection.Command("FountainGetNexts"))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.AddParameter("@Name ", SqlDbType.VarChar, name);
					command.AddParameter("@Owner", SqlDbType.UniqueIdentifier, ownerPk);
					command.AddParameter("@Amount", SqlDbType.BigInt, 3);
					command.AddParameter("@MinValue", SqlDbType.BigInt, minValue);
					command.AddParameter("@MaxValue", SqlDbType.BigInt, maxValue);
					command.AddParameter("@CanRollover", SqlDbType.Bit, canRollover);
					command.AddParameter("@CallInSameTransaction", SqlDbType.Bit, false);

					// Act
					// Assert
					AssertNoExceptionThrown(() => command.ExecuteNonQuery());
				}
			}
		}
	}
}

