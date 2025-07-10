using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(GetMultiCriteriaSetRates))]
	class GetMultiCriteriaSetRatesTest : DbCreateScriptTest
	{
		public void TestGetMultiCriteriaSetRates()
		{
			var tariffPk = Guid.NewGuid();
			var rateCodePk1 = Guid.NewGuid();
			var rateCodePk2 = Guid.NewGuid();
			var ratePk1 = new Guid("00000000-0000-0000-0000-000000000001");
			var ratePk2 = new Guid("00000000-0000-0000-0000-000000000002");

			var prepareTestDataSql = $@"
				DECLARE @TarifTypePk UNIQUEIDENTIFIER = newid();
				DECLARE @RateTypePk UNIQUEIDENTIFIER = newid();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (newid(), 'EUN', 'European Union');
				INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES (@TarifTypePk, 'TT1', 'TariffTypeOne', 'EUN');
				INSERT RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_ZZI_TariffType, ZZ1_ZZF_NKTaxOrFeeCode) VALUES ('{tariffPk}', 'TC1', 'TariffCodeOne', 'EUN', @TarifTypePk, '');
				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePk, 'RT1', 'RateTypeOne', 'EUN', '');
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_Description, ZY1_ZZR_RateType) VALUES
					('{rateCodePk1}', 'RC1', 'RateCodeOne', @RateTypePk),
					('{rateCodePk2}', 'RC2', 'RateCodeTwo', @RateTypePk);
				INSERT RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_RateFormula, ZZ2_ZZ1_Tariff) VALUES
					('{ratePk1}', '{rateCodePk1}', 'RateFormula1', '{tariffPk}'),
					('{ratePk2}', '{rateCodePk2}', 'RateFormula2', '{tariffPk}');
				INSERT RefDatabase_RefCusApplicability (ZZT_PK, ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_EndDate) VALUES
					(newid(), '{ratePk1}', '1900-01-01', '2079-06-06 23:59:00.000'),
					(newid(), '{ratePk2}', '1900-01-01', '2000-12-31 23:59:00.000'),
					(newid(), '{ratePk2}', '2000-01-01', '2079-06-06 23:59:00.000');
			";
			TestConnection.ExecuteNonQuery(prepareTestDataSql);

			var criteriaSetPk1 = new Guid("00000000-0000-0000-0001-000000000000");
			var criteriaSetPk2 = new Guid("00000000-0000-0000-0002-000000000000");

			var sql = $@"
				DECLARE @RateCriteriaTvp AS TVP_RateSelectionCriteria_V2;
				DECLARE @SecondTradeGroupTvp AS TVP_SecondTradeGroup;

				INSERT INTO @RateCriteriaTvp (CriteriaId, TariffPK, TradeGroupCountry, DataGrouping, EffectiveDate, Preference, AdditionalCodesXml, OrderNumber, RateType, RateCode)
				VALUES
						('{criteriaSetPk1}', '{tariffPk}', 'XX', 'EUN', '2079-06-06 23:59:00.000', '', N'<v></v>', N'', 'RT1', ''   ),
						('{criteriaSetPk2}', '{tariffPk}', 'XX', 'EUN', '2079-06-06 23:59:00.000', '', N'<v></v>', N'', ''   , 'RC1');

				INSERT INTO @SecondTradeGroupTvp (Id, CriteriaId, SecondTradeGroup)
				VALUES (newid(), newid(), '')

				EXEC {ScriptToTest.Name} @RateCriteriaTvp, @SecondTradeGroupTvp;";

			var resultList = new List<(Guid CriteriaId, Guid RatePk)>();

			TestConnection.ExecuteReader(
				sql,
				reader =>
				{
					var criteriaId = (Guid)reader["CriteriaId"];
					var ratePk = (Guid)reader["RatePk"];
					resultList.Add((criteriaId, ratePk));
				}
			);

			AssertEquals("Selected Rate count", 3, resultList.Count);
			resultList.Sort();

			CombineAssertions(() =>
			{
				AssertEquals("1st Rate CriteriaId", criteriaSetPk1, resultList[0].CriteriaId);
				AssertEquals("1st Rate RatePk", ratePk1, resultList[0].RatePk);

				AssertEquals("2nd Rate CriteriaId", criteriaSetPk1, resultList[1].CriteriaId);
				AssertEquals("2nd Rate RatePk", ratePk2, resultList[1].RatePk);

				AssertEquals("3rd Rate CriteriaId", criteriaSetPk2, resultList[2].CriteriaId);
				AssertEquals("3rd Rate RatePk", ratePk1, resultList[2].RatePk);
			});
		}

		public void TestGetMultiCriteriaSetRates_SecondTradeGroup()
		{
			var tariffPk1 = Guid.NewGuid();
			var tariffPk2 = Guid.NewGuid();
			var rateCodePk1 = Guid.NewGuid();
			var ratePk1 = Guid.NewGuid();
			var ratePk2 = Guid.NewGuid();

			var prepareTestDataSql = $@"
				DECLARE @TarifTypePk UNIQUEIDENTIFIER = newid();
				DECLARE @RateTypePk UNIQUEIDENTIFIER = newid();
				DECLARE @ParentDataGroupingPk UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupPK1 UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupPK2 UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupCountryPK1 UNIQUEIDENTIFIER = newid();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (@ParentDataGroupingPk, 'EUN', 'European Union');
				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (newid(), 'FR', 'France', @ParentDataGroupingPk);
				INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES (@TarifTypePk, 'TT1', 'TariffTypeOne', 'EUN');

				INSERT RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_ZZI_TariffType, ZZ1_ZZF_NKTaxOrFeeCode)
				VALUES
					(@tariffPk1, 'TC1', 'TariffCode1', 'EUN', @TarifTypePk, ''),
					(@tariffPk2, 'TC2', 'TariffCode2', 'EUN', @TarifTypePk, '')

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePk, 'RT1', 'RateTypeOne', 'EUN', '');
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_Description, ZY1_ZZR_RateType)
				VALUES (@rateCodePk1, 'RC1', 'RateCodeOne', @RateTypePk)

				INSERT RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_RateFormula, ZZ2_ZZ1_Tariff, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
					(@ratePk1, @rateCodePk1, 'RateFormula1', @tariffPk1, '2021-01-01', '2079-06-06'),
					(@ratePk2, @rateCodePk1, 'RateFormula2', @tariffPk2, '2021-01-01', '2079-06-06');

				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES
					(@tradeGroupPK1, 'TG1', 'ZZ TG1', '2021-01-01', '2079-06-06', 'EUN'),
					(@tradeGroupPK2, 'DPDOM', 'SecondTradeGroup', '2021-01-01', '2079-06-06', 'EUN')

				INSERT INTO RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode)
				VALUES (@tradeGroupCountryPK1, @tradeGroupPK1, '2021-01-01', '2079-06-06', 'ZZ tradeGroupCountry1', 'CA')

				INSERT RefDatabase_RefCusApplicability (ZZT_PK, ZZT_ZZ2_Rate, ZZT_StartDate, ZZT_EndDate, ZZT_ZZA_TradeGroup, ZZT_ZZA_SecondTradeGroup)
				VALUES
					(newid(), @ratePk1, '2021-01-01', '2079-06-06 23:59:00.000', @tradeGroupPK1, null),
					(newid(), @ratePk2, '2021-01-01', '2079-06-06 23:59:00.000', @tradeGroupPK1, @tradeGroupPK2);
			";
			using (var cmd = TestConnection.Command(prepareTestDataSql))
			{
				cmd.AddParameter("@tariffPk1", SqlDbType.UniqueIdentifier, tariffPk1);
				cmd.AddParameter("@tariffPk2", SqlDbType.UniqueIdentifier, tariffPk2);
				cmd.AddParameter("@rateCodePk1", SqlDbType.UniqueIdentifier, rateCodePk1);
				cmd.AddParameter("@ratePk1", SqlDbType.UniqueIdentifier, ratePk1);
				cmd.AddParameter("@ratePk2", SqlDbType.UniqueIdentifier, ratePk2);
				cmd.ExecuteNonQuery();
			}

			var criteriaSetPk1 = Guid.NewGuid();
			var criteriaSetPk2 = Guid.NewGuid();
			var criteriaSetPk3 = Guid.NewGuid();
			var criteriaSetPk4 = Guid.NewGuid();

			var sql = $@"
				DECLARE @RateCriteriaTvp AS TVP_RateSelectionCriteria_V2;
				DECLARE @SecondTradeGroupTvp AS TVP_SecondTradeGroup;

				INSERT INTO @RateCriteriaTvp (CriteriaId, TariffPK, TradeGroupCountry, DataGrouping, EffectiveDate, Preference, AdditionalCodesXml, OrderNumber, RateType, RateCode)
				VALUES
					(@criteriaSetPk1, @tariffPk1, 'CA', 'EUN', '2021-06-06', '', N'<v></v>', N'', '', ''),
					(@criteriaSetPk2, @tariffPk1, 'XX', 'EUN', '2021-06-06', '', N'<v></v>', N'', '', ''),
					(@criteriaSetPk3, @tariffPk2, 'CA', 'EUN', '2021-06-06', '', N'<v></v>', N'', '', ''),
					(@criteriaSetPk4, @tariffPk2, 'CA', 'EUN', '2021-06-06', '', N'<v></v>', N'', '', '');

				INSERT INTO @SecondTradeGroupTvp (Id, CriteriaId, SecondTradeGroup)
				VALUES
					(newid(), @criteriaSetPk1, 'XX'),
					(newid(), @criteriaSetPk3, 'DPDOM'),
					(newid(), @criteriaSetPk3, 'OTH'),
					(newid(), @criteriaSetPk4, 'XX');

				EXEC {ScriptToTest.Name} @RateCriteriaTvp, @SecondTradeGroupTvp;";

			var resultList = new List<(Guid CriteriaId, Guid RatePk)>();

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@tariffPk1", SqlDbType.UniqueIdentifier, tariffPk1);
				cmd.AddParameter("@tariffPk2", SqlDbType.UniqueIdentifier, tariffPk2);
				cmd.AddParameter("@criteriaSetPk1", SqlDbType.UniqueIdentifier, criteriaSetPk1);
				cmd.AddParameter("@criteriaSetPk2", SqlDbType.UniqueIdentifier, criteriaSetPk2);
				cmd.AddParameter("@criteriaSetPk3", SqlDbType.UniqueIdentifier, criteriaSetPk3);
				cmd.AddParameter("@criteriaSetPk4", SqlDbType.UniqueIdentifier, criteriaSetPk4);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var criteriaId = (Guid)reader["CriteriaId"];
						var ratePk = (Guid)reader["RatePk"];
						resultList.Add((criteriaId, ratePk));
					}
				}
			}

			CombineAssertions("criteriaSetPk4 Not match SecondTradeGroup and criteriaSetPk2 Not match TradeGroupCountry", () =>
			{
				AssertEquals("Selected Rate count", 2, resultList.Count);
				AssertCollectionContains((criteriaSetPk1, ratePk1), resultList);
				AssertCollectionContains((criteriaSetPk3, ratePk2), resultList);
			});
		}
	}
}

