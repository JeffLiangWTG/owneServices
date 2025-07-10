using System;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.RefExchangeRate;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.RefExchangeRate
{
	[TestedType(typeof(trgRefExchangeRate_Del))]
	class trgRefExchangeRate_Del_Test : DbCreateScriptTest
	{
		public void TestDelete()
		{
			var companyPK = TestDataCreator.CreateCompany("ZA1", "ZA", "ZAR");
			var exchangeRatePK = Guid.NewGuid();
			RefExchangeRateTest.InsertZZRefExchangeRate(TestConnection, exchangeRatePK, companyPK, "ZA", "CAD", "CUS", string.Empty, new DateTime(2016, 1, 2), new DateTime(2016, 1, 31), 0.5m, Guid.Empty);
			RefExchangeRateTest.InsertRefExchangeRateZZ(TestConnection, exchangeRatePK, "ZA", "CAD", "CUS", string.Empty, new DateTime(2015, 12, 1), new DateTime(2016, 1, 1), 0.6m);

			using (var command = TestConnection.Command(@"DELETE FROM dbo.RefExchangeRate WHERE RE_PK=@PK"))
			{
				command.AddParameterBasedOnDbColumn("@PK", exchangeRatePK, RefExchangeRateSchema.PK);
				command.ExecuteNonQuery();
			}
			using (var command = TestConnection.Command(@"SELECT COUNT(*) FROM dbo.ZZRefExchangeRate WHERE RE_PK=@PK"))
			{
				command.AddParameterBasedOnDbColumn("@PK", exchangeRatePK, ZZRefExchangeRateSchema.PK);
				AssertEquals("ZZRefExchangeRate should be deleted", 0, (int)command.ExecuteScalar());
			}
			using (var command = TestConnection.Command(@"SELECT COUNT(*) FROM RefDatabase_RefExchangeRateZZ WHERE ZZN_PK=@PK"))
			{
				command.AddParameterBasedOnDbColumn("@PK", exchangeRatePK, RefExchangeRateZZSchema.PK);
				AssertEquals("RefExchangeRateZZ should NOT be deleted", 1, (int)command.ExecuteScalar());
			}
		}
	}
}

