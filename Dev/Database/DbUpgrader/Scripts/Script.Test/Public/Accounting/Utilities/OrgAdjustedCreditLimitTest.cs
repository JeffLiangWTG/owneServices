using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Utilities;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Utilities.Testing
{
	[TestedType(typeof(OrgAdjustedCreditLimit))]
	class OrgAdjustedCreditLimitTest : DbCreateScriptTest
	{
		public void TestWithNoExpiryDate()
		{
			AssertValues(100M, 50M, DBNull.Value, 100M);
		}

		public void TestExpired()
		{
			AssertValues(100M, 50M, DateTime.UtcNow.AddDays(-1), 100M);
		}

		public void TestNotExpired()
		{
			AssertValues(100M, 50M, DateTime.UtcNow.AddDays(1), 150M);
		}

		public void TestNoIncrease()
		{
			AssertValues(100M, 0M, DateTime.UtcNow.AddDays(1), 100M);
		}

		void AssertValues(decimal arCreditLimit, decimal arTemporaryCreditLimitIncrease, object arTemporaryCreditLimitIncreaseExpiry, decimal expectedResult)
		{
			var testHelper = new TestDbHelperBase(Db.Connection);
			var result = (decimal)testHelper.RunSQL(new
			{
				ARCreditLimit = arCreditLimit,
				ARTemporaryCreditLimitIncrease = arTemporaryCreditLimitIncrease,
				ARTemporaryCreditLimitIncreaseExpiry = arTemporaryCreditLimitIncreaseExpiry
			},
			"select AdjustedCreditLimit from dbo.OrgAdjustedCreditLimit(@ARCreditLimit, @ARTemporaryCreditLimitIncrease, @ARTemporaryCreditLimitIncreaseExpiry)",
				 CommandType.Text, TestDbHelperBase.SQLExecutionTypes.ExecuteScalar);
			AssertEquals(expectedResult, result);
		}
	}
}

