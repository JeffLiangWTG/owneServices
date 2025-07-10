using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script;
using NUnit.Framework;

namespace CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(GetCategoryDetailsFromRegistry))]
	class GetCategoryDetailsFromRegistryTest : BiCreateScriptTest
	{
		public void TestGetCategoryDetailsFromRegistryBaseTestDefaultValue()
		{
			var result = GetCategoryDetailsFromRegistryTuple("BSH");
			AssertEquals("Result should have 81 rows", 81, result.Length);
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("D01", "货币资金"),
				Tuple.Create("D02", "△交易性金融资产"),
				Tuple.Create("D03", "＃短期投资"),
				Tuple.Create("D04", "应收票据"),
				Tuple.Create("D05", "应收账款"),
				Tuple.Create("D06", "预付款项"),
				Tuple.Create("D07", "应收股利"),
				Tuple.Create("D08", "应收利息"),
				Tuple.Create("D09", "其他应收款"),
				Tuple.Create("D10", "存货"),
				Tuple.Create("D11", "其中：原材料"),
				Tuple.Create("D12", "库存商品（产成品）"),
				Tuple.Create("D13", "一年内到期的非流动资产"),
				Tuple.Create("D14", "其他流动资产"),
				Tuple.Create("D20", "△可供出售金融资产"),
				Tuple.Create("D21", "△持有至到期投资"),
				Tuple.Create("D22", "＃长期债权投资"),
				Tuple.Create("D23", "△长期应收款"),
				Tuple.Create("D24", "长期股权投资"),
				Tuple.Create("D25", "＃股权分置流通权"),
				Tuple.Create("D26", "△投资性房地产"),
				Tuple.Create("D27", "固定资产原价"),
				Tuple.Create("D28", "减：累计折旧"),
				Tuple.Create("D30", "减：固定资产减值准备"),
				Tuple.Create("D32", "在建工程"),
				Tuple.Create("D33", "工程物资"),
				Tuple.Create("D34", "固定资产清理"),
				Tuple.Create("D35", "△生产性生物资产"),
				Tuple.Create("D36", "△油气资产"),
				Tuple.Create("D37", "无形资产"),
				Tuple.Create("D38", "其中：土地使用权"),
				Tuple.Create("D39", "△开发支出"),
				Tuple.Create("D40", "△商誉"),
				Tuple.Create("D41", "＃*合并价差"),
				Tuple.Create("D42", "长期待摊费用（递延资产）"),
				Tuple.Create("D43", "△递延所得税资产"),
				Tuple.Create("D44", "＃递延税款借项"),
				Tuple.Create("D45", "其他非流动资产（其他长期资产）"),
				Tuple.Create("D46", "其中：特准储备物资"),
				Tuple.Create("H01", "短期借款"),
				Tuple.Create("H02", "△交易性金融负债"),
				Tuple.Create("H03", "＃应付权证"),
				Tuple.Create("H04", "应付票据"),
				Tuple.Create("H05", "应付账款"),
				Tuple.Create("H06", "预收款项"),
				Tuple.Create("H07", "应付职工薪酬"),
				Tuple.Create("H08", "其中：应付工资"),
				Tuple.Create("H09", "应付福利费"),
				Tuple.Create("H10", "应交税费"),
				Tuple.Create("H11", "其中：应交税金"),
				Tuple.Create("H12", "应付利息"),
				Tuple.Create("H13", "应付股利（应付利润）"),
				Tuple.Create("H14", "其他应付款"),
				Tuple.Create("H15", "一年内到期的非流动负债"),
				Tuple.Create("H16", "其他流动负债"),
				Tuple.Create("H30", "长期借款"),
				Tuple.Create("H31", "应付债券"),
				Tuple.Create("H32", "长期应付款"),
				Tuple.Create("H33", "专项应付款"),
				Tuple.Create("H34", "预计负债"),
				Tuple.Create("H35", "△递延所得税负债"),
				Tuple.Create("H36", "＃递延税款贷项"),
				Tuple.Create("H37", "其他非流动负债"),
				Tuple.Create("H38", "其中：特准储备基金"),
				Tuple.Create("H41", "国家资本"),
				Tuple.Create("H42", "集体资本"),
				Tuple.Create("H43", "法人资本"),
				Tuple.Create("H44", "其中：国有法人资本"),
				Tuple.Create("H45", "集体法人资本"),
				Tuple.Create("H46", "个人资本"),
				Tuple.Create("H47", "外商资本"),
				Tuple.Create("H48", "资本公积"),
				Tuple.Create("H49", "△减：库存股"),
				Tuple.Create("H50", "盈余公积"),
				Tuple.Create("H51", "△一般风险准备"),
				Tuple.Create("H52", "＃*未确认的投资损失（以“-”号填列"),
				Tuple.Create("H53", "未分配利润"),
				Tuple.Create("H54", "其中：现金股利"),
				Tuple.Create("H55", "外币报表折算差额"),
				Tuple.Create("H57", "*少数股东权益"),
				Tuple.Create("H59", "＃减：未处理资产损失")
			}, result);

			result = GetCategoryDetailsFromRegistryTuple("P&L");
			AssertEquals("Result should have 32 rows", 32, result.Length);
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("D02", "其中：主营业务收入"),
				Tuple.Create("D03", "其他业务收入"),
				Tuple.Create("D05", "其中：主营业务成本"),
				Tuple.Create("D06", "其他业务成本"),
				Tuple.Create("D07", "营业税金及附加"),
				Tuple.Create("D08", "销售费用"),
				Tuple.Create("D09", "管理费用"),
				Tuple.Create("D10", "其中：业务招待费"),
				Tuple.Create("D11", "研究与开发费"),
				Tuple.Create("D12", "财务费用"),
				Tuple.Create("D13", "其中：利息支出"),
				Tuple.Create("D14", "利息收入"),
				Tuple.Create("D15", "汇兑净损失（汇兑净收益以“－”号填列）"),
				Tuple.Create("D16", "△资产减值损失"),
				Tuple.Create("D17", "其他"),
				Tuple.Create("D18", "△加：公允价值变动收益（损失以“－”号填列）"),
				Tuple.Create("D19", "投资收益（损失以“－”号填列）"),
				Tuple.Create("D20", "其中：对联营企业和合营企业的投资收益"),
				Tuple.Create("H22", "加：营业外收入"),
				Tuple.Create("H23", "其中：非流动资产处置利得"),
				Tuple.Create("H24", "非货币性资产交换利得（非货币性交易收益）"),
				Tuple.Create("H25", "政府补助（补贴收入）"),
				Tuple.Create("H26", "债务重组利得"),
				Tuple.Create("H27", "减：营业外支出"),
				Tuple.Create("H28", "其中：非流动资产处置损失"),
				Tuple.Create("H29", "非货币性资产交换损失（非货币性交易损失）"),
				Tuple.Create("H30", "债务重组损失"),
				Tuple.Create("H32", "减：所得税费用"),
				Tuple.Create("H33", "加：＃* 未确认的投资损失"),
				Tuple.Create("H35", "减：* 少数股东损益"),
				Tuple.Create("H38", "基本每股收益"),
				Tuple.Create("H39", "稀释每股收益")
			}, result);

			result = GetCategoryDetailsFromRegistryTuple("PLM");
			AssertEquals("Result should have 14 rows", 14, result.Length);
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("D01", "一、主营业务收入"),
				Tuple.Create("D04", "主营业务成本"),
				Tuple.Create("D05", "主营业务税金及附加"),
				Tuple.Create("D11", "其他业务利润"),
				Tuple.Create("D14", "营业费用"),
				Tuple.Create("D15", "管理费用"),
				Tuple.Create("D16", "财务费用"),
				Tuple.Create("D19", "投资收益"),
				Tuple.Create("D22", "补贴收入"),
				Tuple.Create("D23", "营业外收入"),
				Tuple.Create("D25", "减：营业外支出"),
				Tuple.Create("D28", "所得税"),
				Tuple.Create("D29", "少数股东损益(合并报表填列)"),
				Tuple.Create("D30", "未确认的投资损失(合并报表填列)")
			}, result);

			result = GetCategoryDetailsFromRegistryTuple("PLA");
			AssertEquals("Result should have 12 rows", 12, result.Length);
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("A02", "未分配利润"),
				Tuple.Create("A03", "利润分配--盈余公积转入"),
				Tuple.Create("A04", "利润分配--提取法定盈余公积"),
				Tuple.Create("A05", "利润分配--提取法定公益金"),
				Tuple.Create("A06", "利润分配--提取职工奖励及福利基金"),
				Tuple.Create("A07", "利润分配--提取储备基金"),
				Tuple.Create("A08", "利润分配--提取企业发展基金"),
				Tuple.Create("A09", "利润分配--利润归还投资"),
				Tuple.Create("A10", "利润分配--应付优先股股利"),
				Tuple.Create("A11", "利润分配--提取任意盈余公积"),
				Tuple.Create("A12", "利润分配--应付普通股股利"),
				Tuple.Create("A13", "利润分配--转作股本的普通股股利")
			}, result);
		}

		public void TestGetCategoryDetailsFromRegistryBaseTestOverrideValue()
		{
			PrepareData();
			var result = GetCategoryDetailsFromRegistryTuple("BSH");
			AssertEquals("Result should have rows", 1, result.Length);
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("D01", "货币资金"),
			}, result);

			result = GetCategoryDetailsFromRegistryTuple("P&L");
			AssertEquals("Result should have rows", 1, result.Length);
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("D02", "其中：主营业务收入"),
			}, result);

			result = GetCategoryDetailsFromRegistryTuple("PLM");
			AssertEquals("Result should have rows", 1, result.Length);
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("D01", "一、主营业务收入"),
			}, result);

			result = GetCategoryDetailsFromRegistryTuple("PLA");
			AssertEquals("Result should have rows", 1, result.Length);
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("A02", "未分配利润"),
			}, result);
		}

		Tuple<string, string>[] GetCategoryDetailsFromRegistryTuple(string reportType)
		{
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[dbo].GetCategoryDetailsFromRegistry('{reportType}')");
			return dataTable.Rows.Cast<DataRow>().Select(row => Tuple.Create(row["Category"].ToString(), row["CategoryDescription"].ToString())).ToArray();
		}

		void PrepareData()
		{
			string insertRegistryValues = $@"DECLARE @registryRawValue nvarchar(MAX)
Set @registryRawValue = N'<ArrayOfComplianceReportType xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><ComplianceReportType><ReportType>BSH</ReportType><ReportTypeDescription>Balance Sheet</ReportTypeDescription><ArrayOfComplianceReportsSetupCategory><ComplianceReportsSetupCategory><Category>D01</Category><CategoryDescription>货币资金</CategoryDescription><Sequence>2</Sequence><AllowDuplicate>N</AllowDuplicate></ComplianceReportsSetupCategory></ArrayOfComplianceReportsSetupCategory></ComplianceReportType><ComplianceReportType><ReportType>P&amp;L</ReportType><ReportTypeDescription>Profit And Loss for Year</ReportTypeDescription><ArrayOfComplianceReportsSetupCategory><ComplianceReportsSetupCategory><Category>D02</Category><CategoryDescription>其中：主营业务收入</CategoryDescription><Sequence>1</Sequence><AllowDuplicate>N</AllowDuplicate></ComplianceReportsSetupCategory></ArrayOfComplianceReportsSetupCategory></ComplianceReportType><ComplianceReportType><ReportType>PLM</ReportType><ReportTypeDescription>Profit And Loss for Monthly</ReportTypeDescription><ArrayOfComplianceReportsSetupCategory><ComplianceReportsSetupCategory><Category>D01</Category><CategoryDescription>一、主营业务收入</CategoryDescription><Sequence>1</Sequence><AllowDuplicate>N</AllowDuplicate></ComplianceReportsSetupCategory></ArrayOfComplianceReportsSetupCategory></ComplianceReportType><ComplianceReportType><ReportType>PLA</ReportType><ReportTypeDescription>Profit and Loss Appropriation</ReportTypeDescription><ArrayOfComplianceReportsSetupCategory><ComplianceReportsSetupCategory><Category>A02</Category><CategoryDescription>未分配利润</CategoryDescription><Sequence>2</Sequence><AllowDuplicate>N</AllowDuplicate></ComplianceReportsSetupCategory></ArrayOfComplianceReportsSetupCategory></ComplianceReportType></ArrayOfComplianceReportType>'
INSERT INTO [{ScriptDbName}].Finance.BAS__ComplianceReportsSetUpScnData
(ComplianceReportsSetUpScnDataID, ComplianceReportsSetUpScnDataKey, ComplianceReportsSetUpScnDataValue)
Values
(newID(), '1', CONVERT(varbinary(MAX), @registryRawValue))";
			TestConnection.ExecuteNonQuery(insertRegistryValues);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}

