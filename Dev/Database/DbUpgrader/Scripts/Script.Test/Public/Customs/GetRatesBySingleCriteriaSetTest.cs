using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(GetRatesBySingleCriteriaSet))]
	class GetRatesBySingleCriteriaSetTest : DbCreateScriptTest
	{
		public void TestGetRatesBySingleCriteriaSet_SecondTradeGroup()
		{
			var prepareTestDataSql = $@"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID();
				DECLARE @RateTypePK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ParentDataGroupingPk UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupPK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupPK2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupCountryPK1 UNIQUEIDENTIFIER = NEWID();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (@ParentDataGroupingPk, 'EUN', 'European Union');
				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), 'FR', 'France', @ParentDataGroupingPk);
				INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES (@TariffTypePK, 'TT1', 'TariffTypeOne', 'EUN');

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode)
				VALUES
				('{tariffPk1}', @TariffTypePK, 'TC1', 'ZZ Tariff1', 'EUN', '2021-01-01', '2079-06-06', ''),
				('{tariffPk2}', @TariffTypePK, 'TC2', 'ZZ Tariff2', 'EUN', '2021-01-01', '2079-06-06', '')

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePK, 'DTY', 'RateTypeOne', 'EUN', '');
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description) VALUES('{rateCodePk1}', 'RC1', @RateTypePK, 'ZZ RateCode1')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
					('{ratePk1}', '{rateCodePk1}', '{tariffPk1}', null, 'EUN', 'ZZ RateFormula1', '2021-01-01', '2079-06-06'),
					('{ratePk2}', '{rateCodePk1}', '{tariffPk2}', null, 'EUN', 'ZZ RateFormula2', '2021-01-01', '2079-06-06')

				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES
					(@tradeGroupPK1, 'TG1', 'ZZ TG1', '2021-01-01', '2079-06-06', 'EUN'),
					(@tradeGroupPK2, 'DPDOM', 'SecondTradeGroup', '2021-01-01', '2079-06-06', 'EUN')

				INSERT INTO RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode)
				VALUES (@tradeGroupCountryPK1, @tradeGroupPK1, '2021-01-01', '2079-06-06', 'ZZ tradeGroupCountry1', 'CA')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZA_SecondTradeGroup)
				VALUES
					(NEWID(), '{ratePk1}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '', null),
					(NEWID(), '{ratePk2}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '', @tradeGroupPK2)
			";

			TestConnection.ExecuteNonQuery(prepareTestDataSql);

			CombineAssertions(() =>
			{
				AssertEquals("ZZT_ZZA_SecondTradeGroup is null and Empty secondTradeGroup", ratePk1, GetRatesBySingleCriteria(tariffPk1, "CA", "", "EUN", "", "", "", "2021-06-06 23:59:00.000", "", ""));
				AssertEquals("ZZT_ZZA_SecondTradeGroup is null and Non-Empty secondTradeGroup ignored", ratePk1, GetRatesBySingleCriteria(tariffPk1, "CA", "TT", "EUN", "", "", "", "2021-06-06 23:59:00.000", "", ""));
				AssertEquals("Match secondTradeGroup", ratePk2, GetRatesBySingleCriteria(tariffPk2, "CA", "DPDOM", "EUN", "", "", "", "2021-06-06 23:59:00.000", "", ""));
				AssertEquals("No Rate - Empty secondTradeGroup", Guid.Empty, GetRatesBySingleCriteria(tariffPk2, "CA", "", "EUN", "", "", "", "2021-06-06 23:59:00.000", "", ""));
				AssertEquals("No Rate - Not Match secondTradeGroup", Guid.Empty, GetRatesBySingleCriteria(tariffPk2, "CA", "TT", "EUN", "", "", "", "2021-06-06 23:59:00.000", "", ""));
				AssertEquals("No Rate - Not match TradeGroup", Guid.Empty, GetRatesBySingleCriteria(tariffPk2, "US", "DPDOM", "EUN", "", "", "", "2021-06-06 23:59:00.000", "", ""));
			});
		}

		public void TestGetRatesBySingleCriteriaSet()
		{
			var prepareTestDataSql = $@"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID();
				DECLARE @RateTypePK UNIQUEIDENTIFIER = NEWID();
				DECLARE @ParentDataGroupingPk UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupPK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupPK2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupCountryPK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupCountryPK2 UNIQUEIDENTIFIER = NEWID();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (@ParentDataGroupingPk, 'EUN', 'European Union');
				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), 'DE', 'German', @ParentDataGroupingPk);
				INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES (@TariffTypePK, 'TT1', 'TariffTypeOne', 'EUN');

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode) 
				VALUES ('{tariffPk1}', @TariffTypePK, 'TC1', 'ZZ Tariff1', 'EUN', '2021-01-01', '2079-06-06', '')

				INSERT INTO dbo.CusRefTariffVersion (CRT_PK, CRT_Version, CRT_Description, CRT_EffectiveDate, CRT_RN_NKCountryCode, CRT_SystemCreateTimeUtc, CRT_SystemCreateUser, CRT_SystemLastEditTimeUtc, CRT_SystemLastEditUser)
				VALUES (NEWID(), 'HS2021', 'VERSION DESC', '2021-01-01', 'DE', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				INSERT INTO dbo.CusRefTariff (CR1_PK, CR1_ZZI_NKTariffType, CR1_TariffCode, CR1_Description, CR1_StartDate, CR1_EndDate, CR1_ZZF_NKTaxOrFeeCode, CR1_RN_NKCountryCode, CR1_CRT_NKTariffVersion, CR1_SystemCreateTimeUtc, CR1_SystemCreateUser, CR1_SystemLastEditTimeUtc, CR1_SystemLastEditUser)
				VALUES ('{tariffPk2}', 'TT1', 'TC2', 'Cus Tariff2', '2021-01-01', '2079-06-06', 'TFF', 'DE', 'HS2021', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePK, 'DTY', 'RateTypeOne', 'EUN', '');

				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description)
				VALUES('{rateCodePk1}', 'RC1', @RateTypePK, 'ZZ RateCode1')

				INSERT INTO [dbo].[CusRefRateCode] ([CR7_PK], [CR7_RateCode], [CR7_RateType], [CR7_Description], [CR7_RN_NKCountryCode], CR7_SystemCreateTimeUtc, CR7_SystemCreateUser, CR7_SystemLastEditTimeUtc, CR7_SystemLastEditUser)
				VALUES ('{rateCodePk2}', 'RC2', 'DTY', 'Cus RateCode2', 'DE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO RefDatabase_RefCusPreference(ZZS_PK, ZZS_Preference, ZZS_Description, ZZS_ZZZ_NKDataGrouping)
				VALUES('{preferencePK1}', 'PR1', 'ZZ Preference1', 'EUN')

				INSERT INTO dbo.CusRefPreference(CR8_PK,CR8_Preference,CR8_Description,CR8_RN_NKCountryCode, CR8_SystemCreateTimeUtc, CR8_SystemCreateUser, CR8_SystemLastEditTimeUtc, CR8_SystemLastEditUser)
				VALUES('{preferencePK2}', 'PR2', 'Cus Preference2', 'DE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk1}', '{rateCodePk1}', '{tariffPk1}', '{preferencePK1}', 'EUN', 'ZZ RateFormula1', '2021-01-01', '2029-12-31')

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES ('{ratePk2}', '{rateCodePk1}', '{tariffPk1}', null, 'EUN', 'ZZ RateFormula1', '2030-01-01', '2079-06-06')

				INSERT INTO dbo.CusRefRate (CR2_PK, CR2_CR7_RateCode, CR2_CR1_TARIFF, CR2_CR8_Preference, CR2_RateFormula, CR2_StartDate, CR2_EndDate, CR2_SystemCreateTimeUtc, CR2_SystemCreateUser, CR2_SystemLastEditTimeUtc, CR2_SystemLastEditUser)
				VALUES ('{ratePk3}', '{rateCodePk2}', '{tariffPk2}', '{preferencePK2}', 'Cus RateFormula2', '2021-01-01', '2029-12-31', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefRate (CR2_PK, CR2_CR7_RateCode, CR2_CR1_TARIFF, CR2_CR8_Preference, CR2_RateFormula, CR2_StartDate, CR2_EndDate, CR2_SystemCreateTimeUtc, CR2_SystemCreateUser, CR2_SystemLastEditTimeUtc, CR2_SystemLastEditUser)
				VALUES ('{ratePk4}', '{rateCodePk2}', '{tariffPk2}', null, 'Cus RateFormula2', '2030-01-01', '2079-06-06', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES (@tradeGroupPK1, 'TG1', 'ZZ TG1', '2021-01-01', '2079-06-06', 'EUN')

				INSERT INTO RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode)
				VALUES (@tradeGroupCountryPK1, @tradeGroupPK1, '2021-01-01', '2079-06-06', 'ZZ tradeGroupCountry1', 'CA')

				INSERT INTO dbo.CusRefTradeGroup (CR9_PK, CR9_TradeGroup, CR9_Description, CR9_StartDate, CR9_EndDate, CR9_RN_NKCountryCode, CR9_SystemCreateTimeUtc, CR9_SystemCreateUser, CR9_SystemLastEditTimeUtc, CR9_SystemLastEditUser)
				VALUES (@tradeGroupPK2, 'TG2', 'Cus TG2', '2021-01-01', '2079-06-06', 'DE', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefTradeGroupCountry (CRA_PK, CRA_CR9_TradeGroup, CRA_StartDate, CRA_EndDate, CRA_Description, CRA_RN_NKTradeGroupCountryCode, CRA_SystemCreateTimeUtc, CRA_SystemCreateUser, CRA_SystemLastEditTimeUtc, CRA_SystemLastEditUser)
				VALUES (@tradeGroupCountryPK2, @tradeGroupPK2, '2021-01-01', '2079-06-06', 'Cus tradeGroupCountry2', 'CA', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (NEWID(), '{ratePk1}', @tradeGroupPK1, '2021-01-01', '2079-06-06', 'ADD', '1')

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber)
				VALUES (NEWID(), '{ratePk2}', @tradeGroupPK1, '2021-01-01', '2079-06-06', 'ADD', '')

				INSERT INTO dbo.CusRefApplicability (CR4_PK, CR4_CR2_Rate, CR4_CR9_TradeGroup, CR4_StartDate, CR4_EndDate, CR4_OrderNumber, CR4_SystemCreateTimeUtc, CR4_SystemCreateUser, CR4_SystemLastEditTimeUtc, CR4_SystemLastEditUser)
				VALUES (NEWID(), '{ratePk3}', @tradeGroupPK2, '2021-01-01', '2079-06-06', '2', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				INSERT INTO dbo.CusRefApplicability (CR4_PK, CR4_CR2_Rate, CR4_CR9_TradeGroup, CR4_StartDate, CR4_EndDate, CR4_OrderNumber, CR4_SystemCreateTimeUtc, CR4_SystemCreateUser, CR4_SystemLastEditTimeUtc, CR4_SystemLastEditUser)
				VALUES (NEWID(), '{ratePk4}', @tradeGroupPK2, '2021-01-01', '2079-06-06', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
			TestConnection.ExecuteNonQuery(prepareTestDataSql);

			CombineAssertions(() =>
			{
				AssertEquals("ZZ RatePk", ratePk1, GetRatesBySingleCriteria(tariffPk1, "CA", "", "EUN", "PR1", "<v>ADD</v>", "1", "2021 -06-06 23:59:00.000", "DTY", "RC1"));
				AssertEquals("ZZ RatePk - empty rate type", ratePk1, GetRatesBySingleCriteria(tariffPk1, "CA", "", "EUN", "PR1", "<v>ADD</v>", "1", "2021-06-06 23:59:00.000", "", "RC1"));
				AssertEquals("ZZ RatePk - empty rate code", ratePk1, GetRatesBySingleCriteria(tariffPk1, "CA", "", "EUN", "PR1", "<v>ADD</v>", "1", "2021-06-06 23:59:00.000", "DTY", ""));
				AssertEquals("ZZ RatePk - empty orderNumber", ratePk2, GetRatesBySingleCriteria(tariffPk1, "CA", "", "EUN", "PR1", "<v>ADD</v>", "", "2031-06-06 23:59:00.000", "DTY", "RC1"));
				AssertEquals("ZZ RatePk - empty preference and orderNumber", ratePk2, GetRatesBySingleCriteria(tariffPk1, "CA", "", "EUN", "", "<v>ADD</v>", "", "2031-06-06 23:59:00.000", "DTY", "RC1"));

				AssertEquals("Cus RatePk", ratePk3, GetRatesBySingleCriteria(tariffPk2, "CA", "", "DE", "PR2", "<v></v>", "2", "2021-06-06 23:59:00.000", "DTY", "RC2"));
				AssertEquals("Cus RatePk - empty rate type", ratePk3, GetRatesBySingleCriteria(tariffPk2, "CA", "", "DE", "PR2", "<v></v>", "2", "2021-06-06 23:59:00.000", "", "RC2"));
				AssertEquals("Cus RatePk - empty rate code", ratePk3, GetRatesBySingleCriteria(tariffPk2, "CA", "", "DE", "PR2", "<v></v>", "2", "2021-06-06 23:59:00.000", "DTY", ""));
				AssertEquals("Cus RatePk - empty orderNumber", ratePk4, GetRatesBySingleCriteria(tariffPk2, "CA", "", "DE", "PR2", "<v></v>", "", "2031-06-06 23:59:00.000", "DTY", "RC2"));
				AssertEquals("Cus RatePk - empty preference and orderNumber", ratePk4, GetRatesBySingleCriteria(tariffPk2, "CA", "", "DE", "", "<v></v>", "", "2031-06-06 23:59:00.000", "DTY", "RC2"));
			});
		}

		public void TestGetRatesBySingleCriteriaSet_DutyAffectingAdditionalCodes()
		{
			var prepareTestDataSql = $@"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID();
				DECLARE @RateTypePK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @RateTypePK2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @ParentDataGroupingPk UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupPK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupCountryPK1 UNIQUEIDENTIFIER = NEWID();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (@ParentDataGroupingPk, 'EUN', 'European Union');
				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), 'GB', 'United Kingdom', @ParentDataGroupingPk);
				INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES (@TariffTypePK, 'TT1', 'TariffTypeOne', 'EUN');

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode)
				VALUES
					('{tariffPk1}', @TariffTypePK, 'TC1', 'ZZ Tariff1', 'EUN', '2021-01-01', '2079-06-06', '');

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePK1, 'DTY', 'RateTypeOne', 'EUN', '');
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description) VALUES('{rateCodePk1}', 'RC1', @RateTypePK1, 'ZZ RateCode2');

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePK2, 'DTX', 'RateTypeTwo', 'EUN', '');
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description) VALUES('{rateCodePk2}', 'RC2', @RateTypePK2, 'ZZ RateCode1');

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
					('{ratePk1}', '{rateCodePk1}', '{tariffPk1}', null, 'EUN', 'ZZ RateFormula1', '2021-01-01', '2079-06-06');
				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
					('{ratePk2}', '{rateCodePk1}', '{tariffPk1}', null, 'EUN', 'ZZ RateFormula2', '2021-01-01', '2079-06-06');
				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
					('{ratePk3}', '{rateCodePk2}', '{tariffPk1}', null, 'EUN', 'ZZ RateFormula3', '2021-01-01', '2079-06-06');

				
				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES
					(@tradeGroupPK1, 'TG1', 'ZZ TG1', '2021-01-01', '2079-06-06', 'EUN');

				INSERT INTO RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode)
				VALUES (@tradeGroupCountryPK1, @tradeGroupPK1, '2021-01-01', '2079-06-06', 'ZZ tradeGroupCountry1', 'GB');

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZA_SecondTradeGroup)
				VALUES
					(NEWID(), '{ratePk1}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '', null),
					(NEWID(), '{ratePk1}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '1234', '', null),
					(NEWID(), '{ratePk2}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '4567', '', null),
					(NEWID(), '{ratePk3}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '', null);
			";

			TestConnection.ExecuteNonQuery(prepareTestDataSql);

			CombineAssertions(() =>
			{
				var ratesFound = GetRatesBySingleCriteria_All(tariffPk1, "GB", "", "EUN", "", "", "", "2021-06-06 23:59:00.000", "DTY", "");
				AssertEquals("Single rate - no additional codes", 1, ratesFound.Count());
				AssertEquals("Rate with no additional code applicability", ratePk1, ratesFound.FirstOrDefault());
				ratesFound = GetRatesBySingleCriteria_All(tariffPk1, "GB", "", "EUN", "", "<v>1234</v>", "", "2021-06-06 23:59:00.000", "DTY", "");
				AssertEquals("Single rate - matching additional code", 1, ratesFound.Count());
				AssertEquals("Rate1 with matching additional code", ratePk1, ratesFound.FirstOrDefault());
				ratesFound = GetRatesBySingleCriteria_All(tariffPk1, "GB", "", "EUN", "", "<v>4567</v>", "", "2021-06-06 23:59:00.000", "DTY", "");
				AssertEquals("Single rate - matching additional code", 1, ratesFound.Count());
				AssertEquals("Rate2 with matching additional code2", ratePk2, ratesFound.FirstOrDefault());
				ratesFound = GetRatesBySingleCriteria_All(tariffPk1, "GB", "", "EUN", "", "", "", "2021-06-06 23:59:00.000", "DTX", "");
				AssertEquals("Single rate - no additional code, not DTY", 1, ratesFound.Count());
				AssertEquals("Rate with correct type no additional code applicability", ratePk3, ratesFound.FirstOrDefault());
			});
		}

		public void TestGetRatesBySingleCriteriaSet_RatesShouldBeLoadedSeparatedForEachRateType()
		{
			var prepareTestDataSql = $@"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID();
				DECLARE @RateTypePK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @RateTypePK2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @ParentDataGroupingPk UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupPK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupCountryPK1 UNIQUEIDENTIFIER = NEWID();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (@ParentDataGroupingPk, 'EUN', 'European Union');
				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (NEWID(), 'FR', 'France', @ParentDataGroupingPk);
				INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES (@TariffTypePK, 'TT1', 'TariffTypeOne', 'EUN');

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode)
				VALUES
					('{tariffPk1}', @TariffTypePK, 'TC1', 'ZZ Tariff1', 'EUN', '2021-01-01', '2079-06-06', '');

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePK1, 'DTY', 'RateTypeOne', 'EUN', '');
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description) VALUES('{rateCodePk1}', 'RC1', @RateTypePK1, 'ZZ RateCode2');

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_CustomsValueFormula) VALUES (@RateTypePK2, 'DTX', 'RateTypeTwo', 'EUN', '');
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description) VALUES('{rateCodePk2}', 'RC2', @RateTypePK2, 'ZZ RateCode1');

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
					('{ratePk1}', '{rateCodePk1}', '{tariffPk1}', null, 'EUN', 'ZZ RateFormula1', '2021-01-01', '2079-06-06');
				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
					('{ratePk3}', '{rateCodePk2}', '{tariffPk1}', null, 'EUN', 'ZZ RateFormula3', '2021-01-01', '2079-06-06');

				
				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES
					(@tradeGroupPK1, 'TG1', 'ZZ TG1', '2021-01-01', '2079-06-06', 'EUN');

				INSERT INTO RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode)
				VALUES (@tradeGroupCountryPK1, @tradeGroupPK1, '2021-01-01', '2079-06-06', 'ZZ tradeGroupCountry1', 'FR');

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZA_SecondTradeGroup)
				VALUES
					(NEWID(), '{ratePk1}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '1234', '', null),
					(NEWID(), '{ratePk3}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '', null);
			";

			TestConnection.ExecuteNonQuery(prepareTestDataSql);

			CombineAssertions(() =>
			{
				var ratesFound = GetRatesBySingleCriteria_All(tariffPk1, "FR", "", "EUN", "", "<v>1234</v>", "", "2021-06-06 23:59:00.000", "", "");
				AssertContainsExactElementsInAnyOrder("Two rate should be loaded, which are RatePK1 of DTY type with 1234 additional code, and RatePK3 of DTX type with empty additional code.", new[] { ratePk1, ratePk3 }, ratesFound.ToArray());
			});
		}

		public void TestGetRatesBySingleCriteriaSet_ImportExport()
		{
			var prepareTestDataSql = $@"
				DECLARE @TariffTypePK UNIQUEIDENTIFIER = NEWID();
				DECLARE @RateTypePK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @RateTypePK2 UNIQUEIDENTIFIER = NEWID();
				DECLARE @RateTypePK3 UNIQUEIDENTIFIER = NEWID();
				DECLARE @ParentDataGroupingPk UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupPK1 UNIQUEIDENTIFIER = NEWID();
				DECLARE @tradeGroupCountryPK1 UNIQUEIDENTIFIER = NEWID();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES (@ParentDataGroupingPk, 'EUN', 'European Union');
				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description, ZZZ_ZZZ_Grouping) VALUES (newid(), 'GB', 'United Kingdom', @ParentDataGroupingPk);
				INSERT RefDatabase_RefCusTariffType (ZZI_PK, ZZI_TariffType, ZZI_Description, ZZI_ZZZ_NKDataGrouping) VALUES (@TariffTypePK, 'TT1', 'TariffTypeOne', 'EUN');

				INSERT INTO RefDatabase_RefCusTariff (ZZ1_PK, ZZ1_ZZI_TariffType, ZZ1_TariffCode, ZZ1_Description, ZZ1_ZZZ_NKDataGrouping, ZZ1_StartDate, ZZ1_EndDate, ZZ1_ZZF_NKTaxOrFeeCode)
				VALUES
					('{tariffPk1}', @TariffTypePK, 'TC1', 'ZZ Tariff1', 'EUN', '2021-01-01', '2079-06-06', '');

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_IsExport, ZZR_CustomsValueFormula) VALUES (@RateTypePK1, 'DTY', 'RateTypeOne', 'EUN', 0, '');
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description) VALUES('{rateCodePk1}', 'RC1', @RateTypePK1, 'ZZ RateCode1');

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_IsExport, ZZR_CustomsValueFormula) VALUES (@RateTypePK2, 'DTX', 'RateTypeTwo', 'EUN', 0, '');
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description) VALUES('{rateCodePk2}', 'RC2', @RateTypePK2, 'ZZ RateCode2');

				INSERT RefDatabase_RefCusRateType (ZZR_PK, ZZR_RateType, ZZR_Description, ZZR_ZZZ_NKDataGrouping, ZZR_IsExport, ZZR_CustomsValueFormula) VALUES (@RateTypePK3, 'DTZ', 'RateTypeThree', 'EUN', 1, '');
				INSERT RefDatabase_RefCusRateCode (ZY1_PK, ZY1_RateCode, ZY1_ZZR_RateType, ZY1_Description) VALUES('{rateCodePk3}', 'RC3', @RateTypePK3, 'ZZ RateCode3');

				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
					('{ratePk1}', '{rateCodePk1}', '{tariffPk1}', null, 'EUN', 'ZZ RateFormula1', '2020-01-01', '2020-12-12');
				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
					('{ratePk2}', '{rateCodePk2}', '{tariffPk1}', null, 'EUN', 'ZZ RateFormula2', '2021-01-01', '2079-06-06');
				INSERT INTO RefDatabase_RefCusRate (ZZ2_PK, ZZ2_ZY1_RateCode, ZZ2_ZZ1_Tariff, ZZ2_ZZS_Preference, ZZ2_ZZZ_NKDataGrouping, ZZ2_RateFormula, ZZ2_StartDate, ZZ2_EndDate)
				VALUES
					('{ratePk3}', '{rateCodePk3}', '{tariffPk1}', null, 'EUN', 'ZZ RateFormula3', '2021-01-01', '2079-06-06');

				
				INSERT INTO RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_StartDate, ZZA_EndDate, ZZA_ZZZ_NKDataGrouping)
				VALUES
					(@tradeGroupPK1, 'TG1', 'ZZ TG1', '2020-01-01', '2079-06-06', 'EUN');

				INSERT INTO RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_StartDate, ZZB_EndDate, ZZB_Description, ZZB_RN_NKTradeGroupCountryCode)
				VALUES (@tradeGroupCountryPK1, @tradeGroupPK1, '2020-01-01', '2079-06-06', 'ZZ tradeGroupCountry1', 'GB');

				INSERT INTO RefDatabase_RefCusApplicability(ZZT_PK, ZZT_ZZ2_Rate, ZZT_ZZA_TradeGroup, ZZT_StartDate, ZZT_EndDate, ZZT_AdditionalCode, ZZT_OrderNumber, ZZT_ZZA_SecondTradeGroup)
				VALUES
					(NEWID(), '{ratePk1}', @tradeGroupPK1, '2020-01-01', '2020-12-12', '', '', null),
					(NEWID(), '{ratePk2}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '', null),
					(NEWID(), '{ratePk3}', @tradeGroupPK1, '2021-01-01', '2079-06-06', '', '', null);
			";

			TestConnection.ExecuteNonQuery(prepareTestDataSql);

			CombineAssertions(() =>
			{
				AssertEquals("ZZ RatePk1", ratePk1, GetRatesBySingleCriteria(tariffPk1, "GB", "", "EUN", "", "", "", "2020-06-06 23:59:00.000", "", "", 0));
				AssertEquals("ZZ RatePk2", ratePk2, GetRatesBySingleCriteria(tariffPk1, "GB", "", "EUN", "", "", "", "2021-06-06 23:59:00.000", "", "", 1));
				AssertEquals("ZZ RatePk3", ratePk3, GetRatesBySingleCriteria(tariffPk1, "GB", "", "EUN", "", "", "", "2021-06-06 23:59:00.000", "", "", 2));
				AssertContainsExactElementsInAnyOrder("ZZ Rate for both directions", new Guid[] { ratePk2, ratePk3 }, GetRatesBySingleCriteria_All(tariffPk1, "GB", "", "EUN", "", "", "", "2021-06-06 23:59:00.000", "", "", 0));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			tariffPk1 = Guid.NewGuid();
			tariffPk2 = Guid.NewGuid();
			rateCodePk1 = Guid.NewGuid();
			rateCodePk2 = Guid.NewGuid();
			rateCodePk3 = Guid.NewGuid();
			preferencePK1 = Guid.NewGuid();
			preferencePK2 = Guid.NewGuid();
			ratePk1 = new Guid("00000000-0000-0000-0000-000000000001");
			ratePk2 = new Guid("00000000-0000-0000-0000-000000000002");
			ratePk3 = new Guid("00000000-0000-0000-0000-000000000003");
			ratePk4 = new Guid("00000000-0000-0000-0000-000000000004");
		}

		protected override bool RequiresSchemaBinding => false;

		IEnumerable<Guid> GetRatesBySingleCriteria_All(Guid tariffPk, string tradeGroupCountry, string secondTradeGroup, string dataGrouping, string preference, string additionalCodesXml, string orderNumber, string effectiveDate, string rateType, string rateCode, int direction = 0)
		{
			var sql = $"SELECT ZZ2_PK FROM {ScriptToTest.Name} ('{tariffPk}', '{tradeGroupCountry}', '{secondTradeGroup}', '{dataGrouping}', '{preference}', N'{additionalCodesXml}', '{orderNumber}', '{effectiveDate}', '{rateType}', '{rateCode}', {direction});";
			var result = TestConnection.ExecuteScalar(sql);
			var list = new List<Guid>();
			TestConnection.ExecuteReader(sql, reader =>
			{
				list.Add((Guid)reader["ZZ2_PK"]);
			});
			return list;
		}

		Guid GetRatesBySingleCriteria(Guid tariffPk, string tradeGroupCountry, string secondTradeGroup, string dataGrouping, string preference, string additionalCodesXml, string orderNumber, string effectiveDate, string rateType, string rateCode, int direction = 0)
		{
			var sql = $"SELECT ZZ2_PK FROM {ScriptToTest.Name} ('{tariffPk}', '{tradeGroupCountry}', '{secondTradeGroup}', '{dataGrouping}', '{preference}', N'{additionalCodesXml}', '{orderNumber}', '{effectiveDate}', '{rateType}', '{rateCode}', {direction});";
			var result = TestConnection.ExecuteScalar(sql);
			return result == null ? Guid.Empty : (Guid)result;
		}

		Guid tariffPk1, tariffPk2, rateCodePk1, rateCodePk2, rateCodePk3, preferencePK1, preferencePK2, ratePk1, ratePk2, ratePk3, ratePk4;
	}
}
