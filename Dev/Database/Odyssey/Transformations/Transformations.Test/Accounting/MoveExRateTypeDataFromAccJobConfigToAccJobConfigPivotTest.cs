using System;
using System.Data;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Accounting;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Accounting
{
	[TestedType(typeof(MoveExRateTypeDataFromAccJobConfigToAccJobConfigPivot))]
	class MoveExRateTypeDataFromAccJobConfigToAccJobConfigPivotTest : DataTransformationTestCase
	{
		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_WTG__Copy the Exchange Rate Type from AccJobConfig to AccJobConfigPivot_1] ON [dbo].[AccJobConfig] ([JCF_ConfigType]) WHERE ([JCF_ConfigType]='ERT') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void PrepareTestData()
		{
			var helper = new TestDbHelper(TestConnection);

			exRateConfig1 = helper.InsertAccJobConfig("ERT", ledger: "AR", jobType: "AIR", serviceDirection: "ALL", transportMode: "ALL", code: "BUY", code2: "TDR");
			exRateConfig2 = helper.InsertAccJobConfig("ERT", ledger: "AR", jobType: "SHP", serviceDirection: "ALL", transportMode: "ALL", code: "SEL", code2: "TDR");
			exRateConfig3 = helper.InsertAccJobConfig("ERT", ledger: "AP", jobType: "ALL", serviceDirection: "ALL", transportMode: "ALL", code: "CUS", code2: "TDR");
			exRateConfig4 = helper.InsertAccJobConfig("ERT", ledger: "", jobType: "ALL", serviceDirection: "ALL", transportMode: "ALL", code: "PER", code2: "TDR");

			using (DataTransformationHelper.SuspendTriggerIfExists("TG_AccJobConfigPivot_InsertUpdate", AccJobConfigPivotSchema.Constants.TableName))
			{
				configPivot1 = helper.InsertAccJobConfigPivot(exRateConfig1, code: "USD");
				configPivot2 = helper.InsertAccJobConfigPivot(exRateConfig1, code: "CNY");
				configPivot3 = helper.InsertAccJobConfigPivot(exRateConfig2, code: "VND");
				configPivot4 = helper.InsertAccJobConfigPivot(exRateConfig2, code: "JPY");
				configPivot5 = helper.InsertAccJobConfigPivot(exRateConfig2, code: "SGD");
			}
		}

		protected override void AssertTransformationResults()
		{
			var dataTableCurrencyConfig = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT JCT_PK, JCT_ExRateType, JCT_JCF_JobConfig, JCT_Code FROM dbo.AccJobConfigPivot");
			AssertEquals("With existing ExRate config that already have Currency Config, only update JCT_ExRateType and does not add any Currency Config", 2, dataTableCurrencyConfig.Select($"JCT_JCF_JobConfig='{exRateConfig1}'").Length);
			AssertEquals("With existing ExRate config that already have Currency Config, only update JCT_ExRateType and does not add any Currency Config", 3, dataTableCurrencyConfig.Select($"JCT_JCF_JobConfig='{exRateConfig2}'").Length);
			AssertExistingCurrencyConfig(dataTableCurrencyConfig, configPivot1, "USD", "BUY");
			AssertExistingCurrencyConfig(dataTableCurrencyConfig, configPivot2, "CNY", "BUY");
			AssertExistingCurrencyConfig(dataTableCurrencyConfig, configPivot3, "VND", "SEL");
			AssertExistingCurrencyConfig(dataTableCurrencyConfig, configPivot4, "JPY", "SEL");
			AssertExistingCurrencyConfig(dataTableCurrencyConfig, configPivot5, "SGD", "SEL");
			AssertCreatedCurrencyConfig(dataTableCurrencyConfig, exRateConfig3, "CUS");
			AssertCreatedCurrencyConfig(dataTableCurrencyConfig, exRateConfig4, "PER");
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new MoveExRateTypeDataFromAccJobConfigToAccJobConfigPivot();
		}

		void AssertExistingCurrencyConfig(DataTable table, Guid currencyConfigPk, string expectedCurrencyCode, string expectedExRateType)
		{
			var currencyConfig = table.Select($"JCT_PK = '{currencyConfigPk}'")[0];
			AssertEquals("Should still have the same currency code", expectedCurrencyCode, currencyConfig["JCT_Code"]);
			AssertEquals("Should have exRateType of respective ExRate config", expectedExRateType, currencyConfig["JCT_ExRateType"]);
		}

		void AssertCreatedCurrencyConfig(DataTable table, Guid currencyConfigPk, string expectedExRateType)
		{
			var currencyConfig = table.Select($"JCT_JCF_JobConfig='{currencyConfigPk}'");
			AssertEquals("When ExRate config have no currency config, only create one currency config", 1, currencyConfig.Length);

			AssertEquals("Should have empty currency code", string.Empty, currencyConfig[0]["JCT_Code"]);
			AssertEquals("Should have exRateType of respective ExRate config", expectedExRateType, currencyConfig[0]["JCT_ExRateType"]);
		}

		Guid exRateConfig1, exRateConfig2, exRateConfig3, exRateConfig4;
		Guid configPivot1, configPivot2, configPivot3, configPivot4, configPivot5;
	}
}
