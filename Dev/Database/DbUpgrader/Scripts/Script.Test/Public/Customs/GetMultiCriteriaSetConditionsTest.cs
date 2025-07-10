using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(GetMultiCriteriaSetConditions))]
	class GetMultiCriteriaSetConditionsTest : DbCreateScriptTest
	{
		public void TestGetMultiCriteriaSetConditions()
		{
			SetupTestData();

			var criteriaSetPk1 = new Guid("00000000-0000-0000-0001-000000000000");
			var criteriaSetPk2 = new Guid("00000000-0000-0000-0002-000000000000");
			var criteriaSetPk3 = new Guid("00000000-0000-0000-0003-000000000000");

			var sql = $@"
				DECLARE @ConditionCriteriaTvp AS TVP_ConditionSelectionCriteria_V2;
				DECLARE @AdditionalCodesTvp AS TVP_AdditionalCodes;
				DECLARE @SecondTradeGroupTvp AS TVP_SecondTradeGroup;

				INSERT INTO @AdditionalCodesTvp (Id, CriteriaId, AdditionalCode)
					VALUES
					(NEWID(), '{criteriaSetPk1}', ''),
					(NEWID(), '{criteriaSetPk2}', 'AC1'),
					(NEWID(), '{criteriaSetPk2}', 'AC2');

				INSERT INTO @SecondTradeGroupTvp (Id, CriteriaId, SecondTradeGroup)
				VALUES (newid(), newid(), '')

				INSERT INTO @ConditionCriteriaTvp (CriteriaId, TariffPK, TradeGroupCountry, DataGrouping, EffectiveDate, IsImport, IsExport, ConditionClass, ConditionType, Preference, OrderNumber)
					VALUES
						('{criteriaSetPk1}', '{tariffPk}', 'FR', 'EUN', '2021-01-02 00:00:00.000', 1, 0, 'VAT', '4002', '085', '004'),
						('{criteriaSetPk2}', '{tariffPk}', 'FR', 'EUN', '2021-01-02 00:00:00.000',  1, 0, 'VAT', '4002', '085', '005'),
						('{criteriaSetPk3}', '{tariffPk}', 'FR', 'EUN', '2021-01-02 00:00:00.000', 1, 0, 'CTRL', '4001', '', '001');
				EXEC {ScriptToTest.Name} @ConditionCriteriaTvp, @AdditionalCodesTvp, @SecondTradeGroupTvp;";

			var resultList = new List<(Guid CriteriaId, Guid ConditionPk)>();

			TestConnection.ExecuteReader(
				sql,
				reader =>
				{
					var criteriaId = (Guid)reader["CriteriaId"];
					var conditionPk = (Guid)reader["ConditionPk"];
					resultList.Add((criteriaId, conditionPk));
				}
			);

			CombineAssertions(() =>
			{
				AssertEquals("Selected Condition count", 3, resultList.Count);
				resultList.Sort();

				AssertEquals("1st Condition CriteriaId", criteriaSetPk1, resultList[0].CriteriaId);
				AssertEquals("1st Condition ConditionPk", conditionPK3, resultList[0].ConditionPk);

				AssertEquals("2nd Condition CriteriaId", criteriaSetPk2, resultList[1].CriteriaId);
				AssertEquals("2nd Condition ConditionPk", conditionPK3, resultList[1].ConditionPk);

				AssertEquals("3rd Condition CriteriaId", criteriaSetPk3, resultList[2].CriteriaId);
				AssertEquals("3rd Condition ConditionPk", conditionPK1, resultList[2].ConditionPk);
			});
		}

		public void TestGetMultiCriteriaSetConditions_SecondTradeGroup()
		{
			SetupTestData();

			const string sql = @"
				DECLARE @ConditionCriteriaTvp AS TVP_ConditionSelectionCriteria_V2;
				DECLARE @AdditionalCodesTvp AS TVP_AdditionalCodes;
				DECLARE @SecondTradeGroupTvp AS TVP_SecondTradeGroup;

				INSERT INTO @AdditionalCodesTvp (Id, CriteriaId, AdditionalCode)
				VALUES
					(NEWID(), @criteriaSetPk1, 'AC1'),
					(NEWID(), @criteriaSetPk2, 'AC1'),
					(NEWID(), @criteriaSetPk3, '');

				INSERT INTO @SecondTradeGroupTvp (Id, CriteriaId, SecondTradeGroup)
				VALUES
					(newid(), @criteriaSetPk1, '1011'),
					(newid(), @criteriaSetPk1, 'IT'),
					(newid(), @criteriaSetPk2, 'IT'),
					(newid(), @criteriaSetPk2, 'XX'),
					(newid(), @criteriaSetPk3, 'XX');

				INSERT INTO @ConditionCriteriaTvp (CriteriaId, TariffPK, TradeGroupCountry, DataGrouping, EffectiveDate, IsImport, IsExport, ConditionClass, ConditionType, Preference, OrderNumber)
					VALUES
						(@criteriaSetPk1, @tariffPk, 'FR', 'EUN', '2021-01-02 00:00:00.000', 1, 0, 'CLASS', '4003', '085', '001'),
						(@criteriaSetPk2, @tariffPk, 'FR', 'EUN', '2021-01-02 00:00:00.000',  1, 0, 'CLASS', '4003', '085', '001'),
						(@criteriaSetPk3, @tariffPk, 'FR', 'EUN', '2021-01-02 00:00:00.000', 1, 0, 'CTRL', '4001', '085', '001');
				EXEC GetMultiCriteriaSetConditions @ConditionCriteriaTvp, @AdditionalCodesTvp, @SecondTradeGroupTvp;";

			var criteriaSetPk1 = Guid.NewGuid();
			var criteriaSetPk2 = Guid.NewGuid();
			var criteriaSetPk3 = Guid.NewGuid();
			var resultList = new List<(Guid CriteriaId, Guid ConditionPk)>();
			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@tariffPk", SqlDbType.UniqueIdentifier, tariffPk);
				cmd.AddParameter("@criteriaSetPk1", SqlDbType.UniqueIdentifier, criteriaSetPk1);
				cmd.AddParameter("@criteriaSetPk2", SqlDbType.UniqueIdentifier, criteriaSetPk2);
				cmd.AddParameter("@criteriaSetPk3", SqlDbType.UniqueIdentifier, criteriaSetPk3);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var criteriaId = (Guid)reader["CriteriaId"];
						var conditionPk = (Guid)reader["ConditionPk"];
						resultList.Add((criteriaId, conditionPk));
					}
				}
			}

			CombineAssertions(() =>
			{
				AssertEquals("Selected Condition count", 4, resultList.Count);
				AssertCollectionContains("Multi secondTradeGroups: 1011 matched", (criteriaSetPk1, conditionPK4), resultList);
				AssertCollectionContains("Multi secondTradeGroups: IT matched", (criteriaSetPk1, conditionPK5), resultList);
				AssertCollectionContains("Multi secondTradeGroups: IT matched", (criteriaSetPk2, conditionPK5), resultList);
				AssertCollectionContains("No matter what the SecondTradeGroup is as ZZT_ZZA_SecondTradeGroup is null", (criteriaSetPk3, conditionPK1), resultList);
			});
		}

		void SetupTestData()
		{
			startDate = new DateTime(2021, 1, 1);
			endDate = new DateTime(2021, 12, 31, 23, 59, 59, 999);
			var parentPk = TestDataCreator.CreateRefDatabaseRefDataGrouping("EUN", "EU");
			TestDataCreator.CreateRefDatabaseRefDataGrouping("FR", "France", parentPk);
			TestDataCreator.CreateRefDatabaseRefDataGrouping("IT", "Italy", parentPk);

			tariffTypePK = TestDataCreator.CreateRefDbEntZZRefCusTariffType("IMP", "IMPORT", "EUN", "EUN");
			tariffPk = TestDataCreator.CreateRefDbEntZZRefCusTariff(tariffTypePK, "TAR261120", "tariff test", "EUN", startDate, endDate);

			tradeGroupPk1 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("FR", "France", "EUN", startDate, endDate);
			tradeGroupPk2 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("1011", "1011", "EUN", startDate, endDate);
			tradeGroupPk3 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("IT", "Italy", "EUN", startDate, endDate);

			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(tradeGroupPk1, "FR", "France", startDate, endDate);
			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(tradeGroupPk2, "FR", "France", startDate, endDate);
			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(tradeGroupPk3, "IT", "Italy", startDate, endDate);

			conditionTypePK1 = TestDataCreator.CreateRefDbEntZZRefCusConditionType("CTRL", "4001", "test condition type 1", "EUN");
			conditionTypePK2 = TestDataCreator.CreateRefDbEntZZRefCusConditionType("VAT", "4002", "test condition type 2", "EUN");
			var conditionTypePK3 = TestDataCreator.CreateRefDbEntZZRefCusConditionType("CLASS", "4003", "test condition type 3", "EUN");

			preferencePK1 = TestDataCreator.CreateRefDbEntZZRefCusPreference("085", "preference 1", "EUN");
			preferencePK2 = TestDataCreator.CreateRefDbEntZZRefCusPreference("086", "preference 2", "EUN");

			conditionPK1 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK1, tariffPk, startDate, endDate, 1, 0, Guid.Empty, "EUN");
			conditionPK2 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK1, tariffPk, startDate, endDate, 1, 0, preferencePK2, "EUN");
			conditionPK3 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK2, tariffPk, startDate, endDate, 1, 0, preferencePK1, "EUN");
			conditionPK4 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK3, tariffPk, startDate, endDate, 1, 0, preferencePK1, "EUN");
			conditionPK5 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK3, tariffPk, startDate, endDate, 1, 0, preferencePK1, "EUN");

			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK1, startDate, endDate, tradeGroupPk1, "", "001", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK2, startDate, endDate, tradeGroupPk2, "", "002", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK2, startDate, endDate, tradeGroupPk3, "", "003", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK3, startDate, endDate, tradeGroupPk1, "", "004", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK3, startDate, endDate, tradeGroupPk1, "AC1", "005", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK4, startDate, endDate, tradeGroupPk1, "", "001", tradeGroupPk2);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK5, startDate, endDate, tradeGroupPk1, "AC1", "001", tradeGroupPk3);
		}

		DateTime startDate, endDate;
		Guid tariffTypePK, tariffPk;
		Guid tradeGroupPk1, tradeGroupPk2, tradeGroupPk3;
		Guid conditionTypePK1, conditionTypePK2;
		Guid preferencePK1, preferencePK2;
		Guid conditionPK1, conditionPK2, conditionPK3, conditionPK4, conditionPK5;
	}
}
