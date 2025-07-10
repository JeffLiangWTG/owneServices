using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(GetApplicableTariffAdditionalCodes))]
	class GetApplicableTariffAdditionalCodesTest : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		public void TestGetApplicableTariffAdditionalCodes()
		{
			SetupTestData();
			CombineAssertions(() =>
			{
				AssertEquals("Filter on everything: Only SEP additional codes applicable on 2024-6-30 for country AU and datagrouping FR should be returned.", tariffAdditionalCodePK1, TestConnection.ExecuteScalar($"SELECT ZY2_PK FROM {ScriptToTest.Name} ('{tariffPK}', 'SEP', '2024-6-30', 'AU', 'FR');"));
				AssertEquals("Filter on date: Only SEP additional codes applicable on 1900-6-30 for country AU and datagrouping FR should be returned.", tariffAdditionalCodePK2, TestConnection.ExecuteScalar($"SELECT ZY2_PK FROM {ScriptToTest.Name} ('{tariffPK}', 'SEP', '1900-6-30', 'AU', 'FR');"));
				AssertEquals("Filter on category: Only SIP additional codes applicable on 2024-6-30 for country AU and datagrouping FR should be returned.", tariffAdditionalCodePK3, TestConnection.ExecuteScalar($"SELECT ZY2_PK FROM {ScriptToTest.Name} ('{tariffPK}', 'SIP', '2024-6-30', 'AU', 'FR');"));
				AssertEquals("Filter on Tradegroup: Only SEP additional codes applicable on 2024-6-30 for country LV and datagrouping FR should be returned.", tariffAdditionalCodePK4, TestConnection.ExecuteScalar($"SELECT ZY2_PK FROM {ScriptToTest.Name} ('{tariffPK}', 'SEP', '2024-6-30', 'LV', 'FR');"));
				AssertEquals("Filter on datagrouping: Only SEP additional codes applicable on 2024-6-30 for country ER and datagrouping IT should be returned.", tariffAdditionalCodePK5, TestConnection.ExecuteScalar($"SELECT ZY2_PK FROM {ScriptToTest.Name} ('{tariffPK}', 'SEP', '2024-6-30', 'ER', 'IT');"));
				AssertEquals("Make sure no applicable code not returned: No SIP additional codes applicable on 2024-6-30 for country ER and datagrouping IT should be returned because no related applicability exists in RefDB.", null, TestConnection.ExecuteScalar($"SELECT ZY2_PK FROM {ScriptToTest.Name} ('{tariffPK}', 'SIP', '2024-6-30', 'ER', 'IT');"));
			});
		}

		void SetupTestData()
		{
			var startDate = new DateTime(1900, 1, 1);
			var endDate = new DateTime(2024, 12, 30);
			var parentPK = TestDataCreator.CreateRefDatabaseRefDataGrouping("EUN", "EU");
			TestDataCreator.CreateRefDatabaseRefDataGrouping("FR", "France", parentPK);
			TestDataCreator.CreateRefDatabaseRefDataGrouping("IT", "Italy", parentPK);

			var tariffTypePK = TestDataCreator.CreateRefDbEntZZRefCusTariffType("IMP", "IMPORT", "EUN", "EUN");
			tariffPK = TestDataCreator.CreateRefDbEntZZRefCusTariff(tariffTypePK, "TAR261120", "tariff test", "EUN", startDate, endDate);

			var frTradeGroupPK1 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("FR01", "FR01", "EUN", startDate, endDate);
			var frTradeGroupPK2 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("FR02", "FR02", "EUN", startDate, endDate);
			var itTradeGroupPK1 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("IT01", "Italy", "EUN", startDate, endDate);

			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(frTradeGroupPK1, "AU", "Australia", startDate, endDate);
			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(frTradeGroupPK2, "LV", "Latvia", startDate, endDate);
			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(itTradeGroupPK1, "ER", "Eritrea", startDate, endDate);

			TestDataCreator.CreateRefDbEntZZRefCusTariffAdditionalCodeCategory("SEP", "Subdivision statistique à l'exportation", "FR");
			TestDataCreator.CreateRefDbEntZZRefCusTariffAdditionalCodeCategory("SIP", "Subdivision statistique à l'importation", "FR");
			TestDataCreator.CreateRefDbEntZZRefCusTariffAdditionalCodeCategory("SEP", "Subdivision statistique à l'exportation", "IT");
			TestDataCreator.CreateRefDbEntZZRefCusTariffAdditionalCodeCategory("SIP", "Subdivision statistique à l'importation", "IT");

			tariffAdditionalCodePK1 = TestDataCreator.CreateRefDbEntZZRefCusTariffAdditionalCode(tariffPK, Guid.Empty, "FRSEP1", "SEP/AU/FR/Effective in 2024", "SEP", false, "FR");
			tariffAdditionalCodePK2 = TestDataCreator.CreateRefDbEntZZRefCusTariffAdditionalCode(tariffPK, Guid.Empty, "FRSEP2", "SEP/AU/FR/Effective in 1900", "SEP", false, "FR");
			tariffAdditionalCodePK3 = TestDataCreator.CreateRefDbEntZZRefCusTariffAdditionalCode(tariffPK, Guid.Empty, "FRSIP1", "SIP/AU/FR/Effective in 2024", "SIP", false, "FR");
			tariffAdditionalCodePK4 = TestDataCreator.CreateRefDbEntZZRefCusTariffAdditionalCode(tariffPK, Guid.Empty, "FRSEP3", "SEP/LV/FR/Effective in 2024", "SEP", false, "FR");
			tariffAdditionalCodePK5 = TestDataCreator.CreateRefDbEntZZRefCusTariffAdditionalCode(tariffPK, Guid.Empty, "ITSEP1", "SEP/ER/IT/Effective in 2024", "SEP", false, "IT");
			TestDataCreator.CreateRefDbEntZZRefCusTariffAdditionalCode(tariffPK, Guid.Empty, "ITSIP1", "SIP/ER/IT/No applicability", "SIP", false, "IT");

			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, Guid.Empty, tariffAdditionalCodePK1, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), frTradeGroupPK1, string.Empty, string.Empty, Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, Guid.Empty, tariffAdditionalCodePK2, new DateTime(1900, 1, 1), new DateTime(1900, 12, 31), frTradeGroupPK1, string.Empty, string.Empty, Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, Guid.Empty, tariffAdditionalCodePK3, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), frTradeGroupPK1, string.Empty, string.Empty, Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, Guid.Empty, tariffAdditionalCodePK4, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), frTradeGroupPK2, string.Empty, string.Empty, Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, Guid.Empty, tariffAdditionalCodePK5, new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), itTradeGroupPK1, string.Empty, string.Empty, Guid.Empty);
		}

		Guid tariffPK;
		Guid tariffAdditionalCodePK1;
		Guid tariffAdditionalCodePK2;
		Guid tariffAdditionalCodePK3;
		Guid tariffAdditionalCodePK4;
		Guid tariffAdditionalCodePK5;
	}
}
