using System;
using System.Collections.Generic;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(GetMultiCriteriaSetTariffAdditionalCodes))]
	class GetMultiCriteriaSetTariffAdditionalCodesTest : DbCreateScriptTest
	{
		public void TestGetMultiCriteriaSetTariffAdditionalCodes()
		{
			SetupTestData();

			var criteriaSetPk1 = new Guid("00000000-0000-0000-0001-000000000000");
			var criteriaSetPk2 = new Guid("00000000-0000-0000-0002-000000000000");
			var criteriaSetPk3 = new Guid("00000000-0000-0000-0003-000000000000");
			var criteriaSetPk4 = new Guid("00000000-0000-0000-0004-000000000000");
			var criteriaSetPk5 = new Guid("00000000-0000-0000-0005-000000000000");
			var criteriaSetPk6 = new Guid("00000000-0000-0000-0006-000000000000");

			var sql = $@"
				DECLARE @TariffAdditionalCodeCriteriaTvp AS TVP_TariffAdditionalCodeSelectionCriteria;

				INSERT INTO @TariffAdditionalCodeCriteriaTvp (CriteriaId, TariffPK, Category, EffectiveDate, TradeGroupCountry, DataGrouping)
					VALUES
						('{criteriaSetPk1}', '{tariffPK}', 'SEP', '2024-06-01 00:00:00.000', 'AU', 'FR'),
						('{criteriaSetPk2}', '{tariffPK}', 'SEP', '1900-06-01 00:00:00.000', 'AU', 'FR'),
						('{criteriaSetPk3}', '{tariffPK}', 'SIP', '2024-06-01 00:00:00.000', 'AU', 'FR'),
						('{criteriaSetPk4}', '{tariffPK}', 'SEP', '2024-06-01 00:00:00.000', 'LV', 'FR'),
						('{criteriaSetPk5}', '{tariffPK}', 'SEP', '2024-06-01 00:00:00.000', 'ER', 'IT'),
						('{criteriaSetPk6}', '{tariffPK}', 'SIP', '2024-06-01 00:00:00.000', 'ER', 'IT')
				EXEC {ScriptToTest.Name} @TariffAdditionalCodeCriteriaTvp;";

			var resultList = new List<(Guid CriteriaId, Guid ConditionPk)>();

			TestConnection.ExecuteReader(
				sql,
				reader =>
				{
					var criteriaId = (Guid)reader["CriteriaId"];
					var tariffAdditionalCodePK = (Guid)reader["TariffAdditionalCodePk"];
					resultList.Add((criteriaId, tariffAdditionalCodePK));
				}
			);

			CombineAssertions(() =>
			{
				AssertEquals("Selected Condition count is only 5  because no tariff additional code matches 6th criteria.", 5, resultList.Count);
				resultList.Sort();

				AssertEquals("1st Condition CriteriaId", criteriaSetPk1, resultList[0].CriteriaId);
				AssertEquals("1st Condition TariffAdditionalCodePK", tariffAdditionalCodePK1, resultList[0].ConditionPk);

				AssertEquals("2nd Condition CriteriaId", criteriaSetPk2, resultList[1].CriteriaId);
				AssertEquals("2nd Condition TariffAdditionalCodePK", tariffAdditionalCodePK2, resultList[1].ConditionPk);

				AssertEquals("3rd Condition CriteriaId", criteriaSetPk3, resultList[2].CriteriaId);
				AssertEquals("3rd Condition TariffAdditionalCodePK", tariffAdditionalCodePK3, resultList[2].ConditionPk);

				AssertEquals("4th Condition CriteriaId", criteriaSetPk4, resultList[3].CriteriaId);
				AssertEquals("4th Condition TariffAdditionalCodePK", tariffAdditionalCodePK4, resultList[3].ConditionPk);

				AssertEquals("5th Condition CriteriaId", criteriaSetPk5, resultList[4].CriteriaId);
				AssertEquals("5th Condition TariffAdditionalCodePK", tariffAdditionalCodePK5, resultList[4].ConditionPk);
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
