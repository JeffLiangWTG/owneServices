using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(GetRateSelectionCriteriaInfo))]
	class GetRateSelectionCriteriaInfoTest : DbCreateScriptTest
	{
		public void TestGetRateSelectionCriteriaInfo()
		{
			var tariffPk1 = Guid.NewGuid();
			var tariffPk2 = Guid.NewGuid();
			var tariffPk3 = Guid.NewGuid();

			var prepareTestDataSql = @"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = newid();
				DECLARE @RateTypePK UNIQUEIDENTIFIER = newid();
				DECLARE @ParentDataGroupingPk UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupPK1 UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupPK2 UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupCountryPK1 UNIQUEIDENTIFIER = newid();
				DECLARE @tradeGroupCountryPK2 UNIQUEIDENTIFIER = newid();
				DECLARE @rateCodePk1 UNIQUEIDENTIFIER = newid();
				DECLARE @rateCodePk2 UNIQUEIDENTIFIER = newid();
				DECLARE @preferencePK1 UNIQUEIDENTIFIER = newid();
				DECLARE @preferencePK2 UNIQUEIDENTIFIER = newid();
				DECLARE @preferencePK3 UNIQUEIDENTIFIER = newid();
				DECLARE @ratePk1 UNIQUEIDENTIFIER = newid();
				DECLARE @ratePk2 UNIQUEIDENTIFIER = newid();
				DECLARE @ratePk3 UNIQUEIDENTIFIER = newid();
				DECLARE @ratePk4 UNIQUEIDENTIFIER = newid();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (@ParentDataGroupingPk, 'EUN', 'European Union');
				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (newid(), 'DE', 'European Union', @ParentDataGroupingPk);
				INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES (@TariffTypePK, 'TT1', 'TariffTypeOne', 'EUN');

				INSERT INTO RefDatabase_RefLanguageType (ZX6_PK, ZX6_Language, ZX6_Description)
				VALUES (NEWID(), 'DE', 'German')

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode)
				VALUES
				(@tariffPk1, @TariffTypePK, 'TC1', 'ZZ Tariff1', 'EUN', '2020-01-01', '2079-06-06', ''),
				(@tariffPk3, @TariffTypePK, 'TC3', 'ZZ Tariff3', 'EUN', '2020-01-01', '2079-06-06', '')

				INSERT INTO dbo.CusRefTariffVersion (CRT_PK, CRT_Version, CRT_Description, CRT_EffectiveDate, CRT_RN_NKCountryCode, CRT_SystemCreateTimeUtc, CRT_SystemCreateUser, CRT_SystemLastEditTimeUtc, CRT_SystemLastEditUser)
				VALUES (NEWID(), 'HS2021', 'VERSION DESC', '2021-01-01', 'DE', GetUtcDate(), '~BP', GetUtcDate(), '~BP');

				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_CRT_NKTariffVersion, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES (@tariffPk2, 'TT1', 'TC2', 'Cus Tariff2', '2021-01-01', '2079-06-06', 'TFF', 'DE', 'HS2021', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePK, 'DTY', 'RateTypeOne', 'EUN', '');

				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES(@rateCodePk1, 'RC1', @RateTypePK, 'ZZ RateCode1')

				INSERT INTO [dbo].[CusRefRateCode] ([CR7_PK], [CR7_RateCode], [CR7_RateType], [CR7_Description], [CR7_RN_NKCountryCode], CR7_SystemCreateTimeUtc, CR7_SystemCreateUser, CR7_SystemLastEditTimeUtc, CR7_SystemLastEditUser)
				VALUES (@rateCodePk2, 'RC2', 'DTY', 'Cus RateCode2', 'DE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO RefDatabase_RefCusPreference(ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
				VALUES
				(@preferencePK1, 'PR1', 'ZZ Preference1', 'EUN'),
				(@preferencePK3, 'PR3', 'ZZ Preference3', 'EUN')

				INSERT INTO RefDatabase_RefCusPreferenceLanguage(ZX9_PK, ZX9_ZX6_NKLanguage, ZX9_ZZS_Preference, ZX9_Description)
				VALUES (NEWID(), 'DE', @preferencePK1, 'ZZ Preference1 Translated')

				INSERT INTO dbo.CusRefPreference(CR8_PK,CR8_Preference,CR8_Description,CR8_RN_NKCountryCode, CR8_SystemCreateTimeUtc, CR8_SystemCreateUser, CR8_SystemLastEditTimeUtc, CR8_SystemLastEditUser)
				VALUES(@preferencePK2, 'PR2', 'Cus Preference2', 'DE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
				(@ratePk1, @rateCodePk1, @tariffPk1, @preferencePK1, 'EUN', 'ZZ RateFormula1', '2021-01-01', '2029-12-31'),
				(@ratePk3, @rateCodePk1, @tariffPk3, @preferencePK1, 'EUN', 'ZZ RateFormula3', '2021-01-01', '2029-12-31'),
				(@ratePk4, @rateCodePk1, @tariffPk1, @preferencePK3, 'EUN', 'ZZ RateFormula4', '2021-01-01', '2029-12-31')

				INSERT INTO dbo.CusRefRate (CR2_PK, CR2_CR7_RateCode, CR2_CR1_TARIFF, CR2_CR8_Preference, CR2_RateFormula, CR2_StartDate, CR2_EndDate, CR2_SystemCreateTimeUtc, CR2_SystemCreateUser, CR2_SystemLastEditTimeUtc, CR2_SystemLastEditUser)
				VALUES (@ratePk2, @rateCodePk2, @tariffPk2, @preferencePK2, 'Cus RateFormula2', '2021-01-01', '2030-07-16', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES (@tradeGroupPK1, 'TG1', 'ZZ TG1', '2020-07-01', '2030-07-16', 'EUN')

				INSERT INTO RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode)
				VALUES (@tradeGroupCountryPK1, @tradeGroupPK1, '2020-07-01', '2030-07-16', 'ZZ tradeGroupCountry1', 'CA')

				INSERT INTO dbo.CusRefTradeGroup (CR9_PK, CR9_TradeGroup, CR9_Description, CR9_StartDate, CR9_EndDate, CR9_RN_NKCountryCode, CR9_SystemCreateTimeUtc, CR9_SystemCreateUser, CR9_SystemLastEditTimeUtc, CR9_SystemLastEditUser)
				VALUES (@tradeGroupPK2, 'TG2', 'Cus TG2', '2020-07-01', '2030-07-16', 'DE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTradeGroupCountry (CRA_PK, CRA_CR9_TradeGroup, CRA_StartDate, CRA_EndDate, CRA_Description, CRA_RN_NKTradeGroupCountryCode, CRA_SystemCreateTimeUtc, CRA_SystemCreateUser, CRA_SystemLastEditTimeUtc, CRA_SystemLastEditUser)
				VALUES (@tradeGroupCountryPK2, @tradeGroupPK2, '2020-07-01', '2030-07-16', 'Cus tradeGroupCountry2', 'CA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZA_SecondTradeGroup)
				VALUES
				(newid(), @ratePk1, @tradeGroupPK1, '1900-01-01', '2079-06-06', 'ADD', '1', null),
				(newid(), @ratePk3, @tradeGroupPK1, '1900-01-01', '2079-06-06', 'A3', '3', @tradeGroupPK1),
				(newid(), @ratePk4, @tradeGroupPK1, '1900-01-01', '2079-06-06', 'ADD', '1', null)

				INSERT INTO dbo.CusRefApplicability (CR4_PK, CR4_CR2_Rate, CR4_CR9_TradeGroup, CR4_StartDate, CR4_EndDate, CR4_OrderNumber, CR4_SystemCreateTimeUtc, CR4_SystemCreateUser, CR4_SystemLastEditTimeUtc, CR4_SystemLastEditUser)
				VALUES (newid(), @ratePk2, @tradeGroupPK2, '1900-01-01', '2079-06-06', '2', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

			using (var cmd = TestConnection.Command(prepareTestDataSql))
			{
				cmd.AddParameter("@tariffPk1", SqlDbType.UniqueIdentifier, tariffPk1);
				cmd.AddParameter("@tariffPk2", SqlDbType.UniqueIdentifier, tariffPk2);
				cmd.AddParameter("@tariffPk3", SqlDbType.UniqueIdentifier, tariffPk3);
				cmd.ExecuteNonQuery();
			}

			var criteriaSetPk1 = Guid.NewGuid();
			var criteriaSetPk2 = Guid.NewGuid();
			var criteriaSetPk3 = Guid.NewGuid();
			var criteriaSetPk4 = Guid.NewGuid();
			var criteriaSetPk5 = Guid.NewGuid();

			var sql = $@"
				DECLARE @RateCriteriaTvp AS TVP_RateSelectionCriteria_V2;

				INSERT INTO @RateCriteriaTvp (CriteriaId, TariffPK, TradeGroupCountry, DataGrouping, EffectiveDate, Preference, AdditionalCodesXml, OrderNumber, RateType, RateCode)
				VALUES
					(@criteriaSetPk1, @tariffPk1, 'CA', 'EUN', '2021-06-06 23:59:00.000', 'PR1', N'<v>ADD</v>', '1', 'DTY', 'RC1'),
					(@criteriaSetPk2, @tariffPk2, 'CA', 'DE', '2021-06-06 23:59:00.000', 'PR2', N'<v></v>', '2', 'DTY', 'RC2'),
					(@criteriaSetPk3, @tariffPk3, 'CA', 'EUN', '2021-06-06 23:59:00.000', 'PR1', N'<v>A3</v>', '3', 'DTY', 'RC1'),
					(@criteriaSetPk4, @tariffPk3, 'XX', 'EUN', '2021-06-06 23:59:00.000', 'PR1', N'<v>A3</v>', '3', 'DTY', 'RC1'),
					(@criteriaSetPk5, @tariffPk1, 'CA', 'EUN', '2021-06-06 23:59:00.000', 'PR3', N'<v>ADD</v>', '1', 'DTY', 'RC1');

				EXEC {ScriptToTest.Name} @RateCriteriaTvp, 'DE';";

			var resultList = new List<(string orderNumber, string additionalCode, string tradeGroup, string tradeGroupDescription, string preference, string preferenceDescription, string rateType, string rateCode, string secondTradeGroup, string translatedPreferenceDescription)>();

			using (var cmd = TestConnection.Command(sql))
			{
				cmd.AddParameter("@tariffPk1", SqlDbType.UniqueIdentifier, tariffPk1);
				cmd.AddParameter("@tariffPk2", SqlDbType.UniqueIdentifier, tariffPk2);
				cmd.AddParameter("@tariffPk3", SqlDbType.UniqueIdentifier, tariffPk3);
				cmd.AddParameter("@criteriaSetPk1", SqlDbType.UniqueIdentifier, criteriaSetPk1);
				cmd.AddParameter("@criteriaSetPk2", SqlDbType.UniqueIdentifier, criteriaSetPk2);
				cmd.AddParameter("@criteriaSetPk3", SqlDbType.UniqueIdentifier, criteriaSetPk3);
				cmd.AddParameter("@criteriaSetPk4", SqlDbType.UniqueIdentifier, criteriaSetPk4);
				cmd.AddParameter("@criteriaSetPk5", SqlDbType.UniqueIdentifier, criteriaSetPk5);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						resultList.Add((reader["ZZT_OrderNumber"].ToString(), reader["ZZT_AdditionalCode"].ToString(),
							reader["ZZA_TradeGroup"].ToString(), reader["ZZA_Description"].ToString(),
							reader["ZZS_Preference"].ToString(), reader["ZZS_Description"].ToString(),
							reader["ZY1_RateType"].ToString(), reader["ZY1_RateCode"].ToString(), reader["SecondTradeGroup"].ToString(), reader["TranslatedPreferenceDescription"].ToString()));
					}
				}
			}

			CombineAssertions(() =>
			{
				AssertEquals("Selected Criteria count", 4, resultList.Count);
				AssertCollectionContains("criteriaSet1", ("1", "ADD", "TG1", "ZZ TG1", "PR1", "ZZ Preference1", "DTY", "RC1", "", "ZZ Preference1 Translated"), resultList);
				AssertCollectionContains("criteriaSet2", ("2", "", "TG2", "Cus TG2", "PR2", "Cus Preference2", "DTY", "RC2", "", ""), resultList);
				AssertCollectionContains("criteriaSet3", ("3", "A3", "TG1", "ZZ TG1", "PR1", "ZZ Preference1", "DTY", "RC1", "TG1", "ZZ Preference1 Translated"), resultList);
				AssertCollectionContains("criteriaSet5", ("1", "ADD", "TG1", "ZZ TG1", "PR3", "ZZ Preference3", "DTY", "RC1", "", ""), resultList);
			});
		}
	}
}
