using System;
using System.Collections.Generic;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(GetConditionApplicabilitiesByCriteria))]
	class GetConditionApplicabilitiesByCriteriaTest : DbCreateScriptTest
	{
		public void TestConditionApplicabilitiesByCriteria_Case1()
		{
			SetupTestData();
			var sql = $@"
				DECLARE @ConditionCriteriaTvp AS TVP_ConditionSelectionCriteria_V2;
				DECLARE @SecondTradeGroupTvp AS TVP_SecondTradeGroup;
				DECLARE @criteriaPk UNIQUEIDENTIFIER = newid();

				INSERT INTO @ConditionCriteriaTvp (CriteriaId, TariffPK, TradeGroupCountry, DataGrouping, EffectiveDate, IsImport, IsExport)
				VALUES (@criteriaPk, '{tariffPk}', 'IQ', 'EUN', '{DateTime.Today:yyy-MM-dd}', 1, 0)

				INSERT INTO @SecondTradeGroupTvp (Id, CriteriaId, SecondTradeGroup)
				VALUES (newid(), @criteriaPk, '1011')

				EXEC {ScriptToTest.Name} @ConditionCriteriaTvp, @SecondTradeGroupTvp;";

			var list = ExecGetConditionApplicabilitiesByCriteria(sql);

			AssertContainsExactElementsInAnyOrder(new List<(string conditionClass, string conditionType, string orderNumber, string additionalCode, string tradeGroup, string preference, string secondTradeGroup)>
			{
				("CTRL", "4001", "001", "1001", "IQ", "085", "1011"),
				("CTRL", "4001", "002", "1002", "1011", "086", ""),
				("RATE", "4002", "004", "1004", "IQ", "085", "1011")
			}, list);
		}

		public void TestConditionApplicabilitiesByCriteria_Case2()
		{
			SetupTestData();
			var sql = $@"
				DECLARE @ConditionCriteriaTvp AS TVP_ConditionSelectionCriteria_V2;
				DECLARE @SecondTradeGroupTvp AS TVP_SecondTradeGroup;
				DECLARE @criteriaPk UNIQUEIDENTIFIER = newid();

				INSERT INTO @ConditionCriteriaTvp (CriteriaId, TariffPK, TradeGroupCountry, DataGrouping, EffectiveDate, IsImport, IsExport, ConditionClass)
				VALUES (@criteriaPk, '{tariffPk}', 'IQ', 'EUN', '{DateTime.Today:yyy-MM-dd}', 1, 0, 'CTRL')

				INSERT INTO @SecondTradeGroupTvp (Id, CriteriaId, SecondTradeGroup)
				VALUES (newid(), @criteriaPk, '1011')

				EXEC {ScriptToTest.Name} @ConditionCriteriaTvp, @SecondTradeGroupTvp;";

			var list = ExecGetConditionApplicabilitiesByCriteria(sql);

			AssertContainsExactElementsInAnyOrder(new List<(string conditionClass, string conditionType, string orderNumber, string additionalCode, string tradeGroup, string preference, string secondTradeGroup)>
			{
				("CTRL", "4001", "001", "1001", "IQ", "085", "1011"),
				("CTRL", "4001", "002", "1002", "1011", "086", "")
			}, list);
		}

		public void TestConditionApplicabilitiesByCriteria_Case3()
		{
			SetupTestData();
			var sql = $@"
				DECLARE @ConditionCriteriaTvp AS TVP_ConditionSelectionCriteria_V2;
				DECLARE @SecondTradeGroupTvp AS TVP_SecondTradeGroup;
				DECLARE @criteriaPk UNIQUEIDENTIFIER = newid();

				INSERT INTO @ConditionCriteriaTvp (CriteriaId, TariffPK, TradeGroupCountry, DataGrouping, EffectiveDate, IsImport, IsExport, ConditionType)
				VALUES (@criteriaPk, '{tariffPk}', 'IQ', 'EUN', '{DateTime.Today:yyy-MM-dd}', 1, 0, '4002')

				INSERT INTO @SecondTradeGroupTvp (Id, CriteriaId, SecondTradeGroup)
				VALUES (newid(), @criteriaPk, '1011')

				EXEC {ScriptToTest.Name} @ConditionCriteriaTvp, @SecondTradeGroupTvp;";

			var list = ExecGetConditionApplicabilitiesByCriteria(sql);

			AssertContainsExactElementsInAnyOrder(new List<(string conditionClass, string conditionType, string orderNumber, string additionalCode, string tradeGroup, string preference, string secondTradeGroup)>
			{
				("RATE", "4002", "004", "1004", "IQ", "085", "1011")
			}, list);
		}

		public void TestConditionApplicabilitiesByCriteria_Case4()
		{
			SetupTestData();
			var conditionPK4 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK2, tariffPk, startDate, endDate, 1, 0, Guid.Empty, "EUN");
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK4, startDate, endDate, tradeGroupPk1, "1005", "005", Guid.Empty);

			var sql = $@"
				DECLARE @ConditionCriteriaTvp AS TVP_ConditionSelectionCriteria_V2;
				DECLARE @SecondTradeGroupTvp AS TVP_SecondTradeGroup;
				DECLARE @criteriaPk UNIQUEIDENTIFIER = newid();

				INSERT INTO @ConditionCriteriaTvp (CriteriaId, TariffPK, TradeGroupCountry, DataGrouping, EffectiveDate, IsImport, IsExport, Preference)
				VALUES (@criteriaPk, '{tariffPk}', 'IQ', 'EUN', '{DateTime.Today:yyy-MM-dd}', 1, 0, '085')

				INSERT INTO @SecondTradeGroupTvp (Id, CriteriaId, SecondTradeGroup)
				VALUES (newid(), @criteriaPk, '1011')

				EXEC {ScriptToTest.Name} @ConditionCriteriaTvp, @SecondTradeGroupTvp;";

			var list = ExecGetConditionApplicabilitiesByCriteria(sql);

			AssertContainsExactElementsInAnyOrder(new List<(string conditionClass, string conditionType, string orderNumber, string additionalCode, string tradeGroup, string preference, string secondTradeGroup)>
			{
				("CTRL", "4001", "001", "1001", "IQ", "085", "1011"),
				("RATE", "4002", "004", "1004", "IQ", "085", "1011"),
				("RATE", "4002", "005", "1005", "IQ", "", "")
			}, list);
		}

		public void TestConditionApplicabilitiesByCriteria_Case5()
		{
			SetupTestData();
			var conditionPK4 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK2, tariffPk, startDate, endDate, 1, 0, Guid.Empty, "EUN");
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK4, startDate, endDate, tradeGroupPk1, "1005", "", Guid.Empty);

			var sql = $@"
				DECLARE @ConditionCriteriaTvp AS TVP_ConditionSelectionCriteria_V2;
				DECLARE @SecondTradeGroupTvp AS TVP_SecondTradeGroup;
				DECLARE @criteriaPk UNIQUEIDENTIFIER = newid();

				INSERT INTO @ConditionCriteriaTvp (CriteriaId, TariffPK, TradeGroupCountry, DataGrouping, EffectiveDate, IsImport, IsExport, OrderNumber)
				VALUES (@criteriaPk, '{tariffPk}', 'IQ', 'EUN', '{DateTime.Today:yyy-MM-dd}', 1, 0, '001')

				INSERT INTO @SecondTradeGroupTvp (Id, CriteriaId, SecondTradeGroup)
				VALUES (newid(), @criteriaPk, '1011')

				EXEC {ScriptToTest.Name} @ConditionCriteriaTvp, @SecondTradeGroupTvp;";

			var list = ExecGetConditionApplicabilitiesByCriteria(sql);

			AssertContainsExactElementsInAnyOrder(new List<(string conditionClass, string conditionType, string orderNumber, string additionalCode, string tradeGroup, string preference, string secondTradeGroup)>
			{
				("CTRL", "4001", "001", "1001", "IQ", "085", "1011"),
				("RATE", "4002", "", "1005", "IQ", "", "")
			}, list);
		}

		IEnumerable<(string conditionClass, string conditionType, string orderNumber, string additionalCode, string tradeGroup, string preference, string secondTradeGroup)> ExecGetConditionApplicabilitiesByCriteria(string sql)
		{
			var list = new List<(string conditionClass, string conditionType, string orderNumber, string additionalCode, string tradeGroup, string preference, string secondTradeGroup)>();
			TestConnection.ExecuteReader(sql, reader =>
			{
				list.Add(((string)reader["ZX2_ConditionClass"],
						(string)reader["ZX2_ConditionType"],
						(string)reader["ZZT_OrderNumber"],
						(string)reader["ZZT_AdditionalCode"],
						(string)reader["ZZA_TradeGroup"],
						(string)reader["ZZS_Preference"],
						reader["SecondTradeGroup"].ToString()
						));
			});
			return list;
		}

		void SetupTestData()
		{
			var year = DateTime.Now.Year;
			startDate = new DateTime(year, 1, 1);
			endDate = new DateTime(year, 12, 31, 23, 59, 59, 999);
			TestDataCreator.CreateRefDatabaseRefDataGrouping("EUN", "EU");
			TestDataCreator.CreateRefDatabaseRefDataGrouping("IQ", "Iraq");
			TestDataCreator.CreateRefDatabaseRefDataGrouping("AU", "Australia");

			var tariffTypePK = TestDataCreator.CreateRefDbEntZZRefCusTariffType("IMP", "IMPORT", "EUN", "EUN");
			tariffPk = TestDataCreator.CreateRefDbEntZZRefCusTariff(tariffTypePK, "TAR261120", "tariff test", "EUN", startDate, endDate);

			tradeGroupPk1 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("IQ", "Iraq", "EUN", startDate, endDate);
			tradeGroupPk2 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("1011", "1011", "EUN", startDate, endDate);
			tradeGroupPk3 = TestDataCreator.CreateRefDbEntZZRefCusTradeGroup("AU", "Australia", "EUN", startDate, endDate);

			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(tradeGroupPk1, "IQ", "Iraq", startDate, endDate);
			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(tradeGroupPk2, "IQ", "Iraq", startDate, endDate);
			TestDataCreator.CreateRefDbEntZZRefCusTradeGroupCountry(tradeGroupPk3, "AU", "Australia", startDate, endDate);

			conditionTypePK1 = TestDataCreator.CreateRefDbEntZZRefCusConditionType("CTRL", "4001", "test condition type 1", "EUN");
			conditionTypePK2 = TestDataCreator.CreateRefDbEntZZRefCusConditionType("RATE", "4002", "test condition type 2", "EUN");

			var preferencePK1 = TestDataCreator.CreateRefDbEntZZRefCusPreference("085", "preference 1", "EUN");
			var preferencePK2 = TestDataCreator.CreateRefDbEntZZRefCusPreference("086", "preference 2", "EUN");

			conditionPK1 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK1, tariffPk, startDate, endDate, 1, 0, preferencePK1, "EUN");
			conditionPK2 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK1, tariffPk, startDate, endDate, 1, 0, preferencePK2, "EUN");
			conditionPK3 = TestDataCreator.CreateRefDbEntZZRefCusCondition(conditionTypePK2, tariffPk, startDate, endDate, 1, 0, preferencePK1, "EUN");

			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK1, startDate, endDate, tradeGroupPk1, "1001", "001", tradeGroupPk2);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK2, startDate, endDate, tradeGroupPk2, "1002", "002", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK2, startDate, endDate, tradeGroupPk3, "1003", "003", Guid.Empty);
			TestDataCreator.CreateRefDbEntZZRefCusApplicability(Guid.Empty, conditionPK3, startDate, endDate, tradeGroupPk1, "1004", "004", tradeGroupPk2);
		}

		DateTime startDate;
		DateTime endDate;

		Guid tariffPk;

		Guid tradeGroupPk1;
		Guid tradeGroupPk2;
		Guid tradeGroupPk3;

		Guid conditionTypePK1;
		Guid conditionTypePK2;

		Guid conditionPK1;
		Guid conditionPK2;
		Guid conditionPK3;
	}
}
