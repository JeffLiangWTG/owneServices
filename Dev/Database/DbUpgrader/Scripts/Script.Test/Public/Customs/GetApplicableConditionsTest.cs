using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs
{
	[TestedType(typeof(GetApplicableConditions))]
	class GetApplicableConditionsTest : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;

		public void TestGetApplicableConditions()
		{
			SetupTestData();

			CombineAssertions(() =>
			{
				AssertEquals("conditionPK1 - isImport And no matter what the SecondTradeGroup is as ZZT_ZZA_SecondTradeGroup is null", conditionPK1, TestConnection.ExecuteScalar($"SELECT ZX1_PK FROM {ScriptToTest.Name} ('{tariffPk}', '085', 1, 0, 'CTRL', '4001', '2021-1-2', 'FR', '', '001', 'EUN', '1');"));
				AssertEquals("conditionPK2 - isExport", conditionPK2, TestConnection.ExecuteScalar($"SELECT ZX1_PK FROM {ScriptToTest.Name} ('{tariffPk}', '086', 0, 1, 'CTRL', '4001', '2021-1-2', 'FR', '', '002', 'EUN', '');"));
				AssertEquals("conditionPK3: Empty additional code", conditionPK3, TestConnection.ExecuteScalar($"SELECT ZX1_PK FROM {ScriptToTest.Name} ('{tariffPk}', '085', 1, 0, 'RATE', '4002', '2021-1-2', 'FR', '', '004', 'EUN', '');"));
				AssertEquals("conditionPK3: additional code 'AC1'", conditionPK3, TestConnection.ExecuteScalar($"SELECT ZX1_PK FROM {ScriptToTest.Name} ('{tariffPk}', '085', 1, 1, 'RATE', '4002', '2021-1-2', 'FR', 'AC1', '005', 'EUN', '');"));
				AssertEquals("conditionPK4: UNION ALL - match ZX1_ZZ1_Tariff IS NULL", conditionPK4, TestConnection.ExecuteScalar($"SELECT ZX1_PK FROM {ScriptToTest.Name} ('{tariffPk}', '085', 1, 1, 'RATE', '4002', '2021-1-2', 'FR', 'AC1', '006', 'EUN', '');"));
				AssertEquals("conditionPK5 - SecondTradeGroup matched", conditionPK5, TestConnection.ExecuteScalar($"SELECT ZX1_PK FROM {ScriptToTest.Name} ('{tariffPk}', '085', 1, 0, 'CLASS', '4003', '2021-1-2', 'FR', '', '001', 'EUN', '1011');"));
				AssertEquals("conditionPK5 - SecondTradeGroup not matched", null, TestConnection.ExecuteScalar($"SELECT ZX1_PK FROM {ScriptToTest.Name} ('{tariffPk}', '085', 1, 0, 'CLASS', '4003', '2021-1-2', 'FR', '', '001', 'EUN', '101');"));
			});
		}

		void SetupTestData()
		{
			var startDate = new DateTime(2021, 1, 1);
			var endDate = new DateTime(2021, 12, 31, 23, 59, 59, 999);
			var parentPk = TestDataCreator.CreateRefDatabaseRefDataGrouping("EUN", "EU");
			TestDataCreator.CreateRefDatabaseRefDataGrouping("FR", "France", parentPk);
			TestDataCreator.CreateRefDatabaseRefDataGrouping("IT", "Italy", parentPk);

			var tariffTypePK = TestDataCreator.CreateRefDbEntZZRefCusTariffType("IMP", "IMPORT", "EUN", "EUN");
			tariffPk = TestDataCreator.CreateRefDbEntZZRefCusTariff(tariffTypePK, "TAR261120", "tariff test", "EUN", startDate, endDate);

			var tradeGroupPk1 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("FR", "France", "EUN", startDate, endDate);
			var tradeGroupPk2 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("1011", "1011", "EUN", startDate, endDate);
			var tradeGroupPk3 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("IT", "Italy", "EUN", startDate, endDate);

			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(tradeGroupPk1, "FR", "France", startDate, endDate);
			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(tradeGroupPk2, "FR", "France", startDate, endDate);
			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(tradeGroupPk3, "IT", "Italy", startDate, endDate);

			var conditionTypePK1 = TestDataCreator.CreateRefDbEntZZRefCusConditionType("CTRL", "4001", "test condition type 1", "EUN");
			var conditionTypePK2 = TestDataCreator.CreateRefDbEntZZRefCusConditionType("RATE", "4002", "test condition type 2", "EUN");
			var conditionTypePK3 = TestDataCreator.CreateRefDbEntZZRefCusConditionType("CLASS", "4003", "test condition type 3", "EUN");

			var preferencePK1 = TestDataCreator.CreateRefDbEntZZRefCusPreference("085", "preference 1", "EUN");
			var preferencePK2 = TestDataCreator.CreateRefDbEntZZRefCusPreference("086", "preference 2", "EUN");

			conditionPK1 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK1, tariffPk, startDate, endDate, 1, 0, preferencePK1, "EUN");
			conditionPK2 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK1, tariffPk, startDate, endDate, 0, 1, preferencePK2, "EUN");
			conditionPK3 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK2, tariffPk, startDate, endDate, 1, 0, preferencePK1, "EUN");
			conditionPK4 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK2, Guid.Empty, startDate, endDate, 1, 0, preferencePK1, "EUN");
			conditionPK5 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK3, tariffPk, startDate, endDate, 1, 0, preferencePK1, "EUN");

			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK1, startDate, endDate, tradeGroupPk1, "", "001", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK2, startDate, endDate, tradeGroupPk2, "", "002", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK2, startDate, endDate, tradeGroupPk3, "", "003", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK3, startDate, endDate, tradeGroupPk1, "", "004", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK3, startDate, endDate, tradeGroupPk1, "AC1", "005", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK4, startDate, endDate, tradeGroupPk1, "AC1", "006", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK5, startDate, endDate, tradeGroupPk1, "", "001", tradeGroupPk2);
		}

		Guid tariffPk;
		Guid conditionPK1, conditionPK2, conditionPK3, conditionPK4, conditionPK5;
	}
}

