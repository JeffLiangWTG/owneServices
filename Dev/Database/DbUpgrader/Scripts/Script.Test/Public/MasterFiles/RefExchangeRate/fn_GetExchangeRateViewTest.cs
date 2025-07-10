using System;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.RefExchangeRate;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.Customs.KR.Testing.KRTestDataCreator;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.RefExchangeRate.Testing
{
	[TestedType(typeof(fn_GetExchangeRateView))]
	class fn_GetExchangeRateViewTest : DbCreateScriptTest
	{
		public void Testfn_GetExchangeRateView()
		{
			CreateRefExchangeRate(companyPK, "USD", 0.1m, "CUE", DateTime.Today.AddDays(1), DateTime.Today.AddDays(2));

			using (var command = TestConnection.Command(string.Format("SELECT * FROM fn_GetExchangeRateView('USD', 'CUE', GETDATE(), '{0}')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert(!reader.Read());
				}
			}

			CreateRefExchangeRate(companyPK, "USD", 0.1m, "CUE", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

			using (var command = TestConnection.Command(string.Format("SELECT * FROM fn_GetExchangeRateView('USD', 'CUE', GETDATE(), '{0}')", companyPK)))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(0.1m, reader[0]);
				}
			}
		}

		Guid companyPK;

		protected override void SetUp()
		{
			companyPK = TestDataCreator.CreateCompany("KC1", "KR", "KRW");
		}
	}
}
