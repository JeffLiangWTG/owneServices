using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AlternateGLAccountsFilterBusinessObject))]
	class AccAlternateGLAccountsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new AlternateGLAccountsFilterBusinessObject();
		}

		public void TestAlternateAccountFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Alternate Account"];
			AssertNotNull(filter);
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "10001000";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			filter.Property = "1000.AA.10";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "1000-AA-10";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = "10001000";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "-";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			filter.Property = ".";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "100";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			filter.Property = "10.";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			filter.Property = "AA";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = "10";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.Property = "10.";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "A";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "10";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			filter.Property = "10.";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.Property = "1000.";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = "10.";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			filter.Property = "10";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.Property = "-";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);
		}

		public void TestChartCodeFilter()
		{
			var filter = (ModuleGuidFilter)FilterBO["Chart Code"];
			AssertNotNull(filter);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = Chart.PK;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			filter.Property = Guid.NewGuid();
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);
		}

		public void TestDescriptionFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Description"];
			AssertNotNull(filter);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "10.00.1000";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			filter.Property = "10.00.1010";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			AssertEquals(filter.FilterColumn.MaxLength, filter.MaxLength);
		}

		public void TestParentAccountFilter()
		{
			var filter = (ModuleGuidFilter)FilterBO["Parent Account"];
			AssertNotNull(filter);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = Creator.GLHeader1.PK;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			filter.Property = Creator.GLHeader2.PK;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			var accountWithoutParent = Creator.CreateAccAlternateGlAccount(Chart.PK, "20.00.1010", Core.Constants.AccountType.Total, Core.Constants.DebitCredit.Credit, 1, AccGLHeader.Constants.SectionTypes.Codes.Assets, 3, "20.00.1010");
			Factory.Save();

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
			Assert(TestCollection.Cast<AlternateGLAccountCombineParentAccount>().All(x => !x.AlternateGLAccountAttributes.Any()));

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filter.Property = Guid.Empty;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);
			Assert(TestCollection.Cast<AlternateGLAccountCombineParentAccount>().All(x => x.AlternateGLAccountAttributes.Any()));
		}

		public void TestGlobalStatusFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Chart Global Status"];
			AssertNotNull(filter);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "GLOBAL";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 0 record", 0, TestCollection.Count);

			filter.Property = "NOT GLOBAL";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			filter.Property = "ALL";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			AssertEquals(10, filter.MaxLength);
		}

		public void TestAccountTypeFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Account Type"];
			AssertNotNull(filter);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "BSH";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			filter.Property = "P&L";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "ALL";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			AssertEquals(AccAlternateGLAccountSchema.AGA_AccountType.MaxLength, filter.MaxLength);
		}

		public void TestReportSectionFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Report Section"];
			AssertNotNull(filter);
			filter.IsActive = true;
			filter.Property = "OV";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			filter.Property = "AS";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			AssertEquals(AccAlternateGLAccountSchema.AGA_ReportSection.MaxLength, filter.MaxLength);
		}

		public void TestCashFlowTypeFilter()
		{
			var filter = (ModuleTextFilter)FilterBO["Cash Flow Type"];
			AssertNotNull(filter);
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "CSH";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 2 record", 2, TestCollection.Count);

			filter.Property = CashFlowCodeLists.Codes.NON;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);

			filter.Property = "ALL";
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 3 record", 3, TestCollection.Count);

			AssertEquals(AccGLHeaderSchema.AG_CashFlowType.MaxLength, filter.MaxLength);
		}

		public void TestAuditFilter()
		{
			var userFilter = (ModuleNkFilter)FilterBO["Creating User"];
			AssertNotNull(userFilter);
			userFilter = (ModuleNkFilter)FilterBO["Last Edit User"];
			AssertNotNull(userFilter);
			var timeFilter = (ModuleDateFilter)FilterBO["Created Time"];
			AssertNotNull(timeFilter);
			timeFilter = (ModuleDateFilter)FilterBO["Last Edit Time"];
			AssertNotNull(timeFilter);
		}

		public void TestPercentNumberFilter()
		{
			var percentNumber = Creator.CreateAccAlternateGlAccount(Chart.PK, "30.00.1000", Core.Constants.AccountType.Total, Core.Constants.DebitCredit.Debit, 1, "", 1, "30.00.1000");
			var alternateAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "30.00.2000", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1, "30.00.2000");
			alternateAccount.AGA_AGA_PercentNum = percentNumber.PK;
			Factory.Save();

			var filter = (AlternateGLAccountModuleFilter)FilterBO["Percent Number"];
			AssertNotNull(filter);
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = alternateAccount.AGA_AGA_PercentNum;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
			AssertEquals("PercentNumber Filter does not supports Filters Match operator", false, filter.SupportsFiltersMatchComparisonOperator);
		}

		public void TestConsolidateNumberFilter()
		{
			var consolidateNumber = Creator.CreateAccAlternateGlAccount(Chart.PK, "30.00.1000", Core.Constants.AccountType.Consolidation, Core.Constants.DebitCredit.Debit, 1, "", 1, "30.00.1000");
			var alternateAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "30.00.2000", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1, "30.00.2000");
			alternateAccount.AGA_AGA_ConsolidationNum = consolidateNumber.PK;
			Factory.Save();

			var filter = (AlternateGLAccountModuleFilter)FilterBO["Consolidate Number"];
			AssertNotNull(filter);
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = alternateAccount.AGA_AGA_ConsolidationNum;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
			AssertEquals("ConsolidateNumber Filter does not supports Filters Match operator", false, filter.SupportsFiltersMatchComparisonOperator);
		}

		public void TestAlternateNumberFilter()
		{
			var alternateNumber = Creator.CreateAccAlternateGlAccount(Chart.PK, "30.00.1000", Core.Constants.AccountType.Alternate, Core.Constants.DebitCredit.Debit, 1, "", 1, "30.00.1000");
			var alternateAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "30.00.2000", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1, "30.00.2000");
			alternateAccount.AGA_AGA_AlternateNum = alternateNumber.PK;
			Factory.Save();

			var filter = (AlternateGLAccountModuleFilter)FilterBO["Alternate Number"];
			AssertNotNull(filter);
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = alternateAccount.AGA_AGA_AlternateNum;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
			AssertEquals("AlternateNumber Filter does not supports Filters Match operator", false, filter.SupportsFiltersMatchComparisonOperator);
		}

		public void TestTotalReferenceNumberFilter()
		{
			var totalReferenceNumber = Creator.CreateAccAlternateGlAccount(Chart.PK, "30.00.1000", Core.Constants.AccountType.Total, Core.Constants.DebitCredit.Debit, 1, "", 1, "30.00.1000");
			var alternateAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "30.00.2000", Core.Constants.AccountType.Header, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1, "30.00.2000");
			alternateAccount.AGA_AGA_HeaderDependsOnTotal = totalReferenceNumber.PK;
			Factory.Save();

			var filter = (AlternateGLAccountModuleFilter)FilterBO["Total Reference Number"];
			AssertNotNull(filter);
			AssertEquals(FilterCategories.NumbersAndReferences, filter.Category);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = alternateAccount.AGA_AGA_HeaderDependsOnTotal;
			TestCollection.SetFilterHelper(FilterBO.AlternateGLAccountFilter);
			TestCollection.Load();
			AssertEquals("Expecting collection 1 record", 1, TestCollection.Count);
			AssertEquals("TotalReferenceNumber Filter does not supports Filters Match operator", false, filter.SupportsFiltersMatchComparisonOperator);
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var exclusions = new List<Tuple<string, string>>();
			exclusions.Add(TableFilter(AccAlternateGLAccountSchema.Constants.TableName, "Percent Number"));
			exclusions.Add(TableFilter(AccAlternateGLAccountSchema.Constants.TableName, "Consolidate Number"));
			exclusions.Add(TableFilter(AccAlternateGLAccountSchema.Constants.TableName, "Alternate Number"));
			exclusions.Add(TableFilter(AccAlternateGLAccountSchema.Constants.TableName, "Total Reference Number"));
			exclusions.Add(TableFilter(AccAlternateChartSchema.Constants.TableName, "Chart Global Status"));

			return exclusions;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var exclusions = new List<Tuple<string, string>>();
			exclusions.Add(TableFilter(AccAlternateGLAccountSchema.Constants.TableName, "Percent Number"));
			exclusions.Add(TableFilter(AccAlternateGLAccountSchema.Constants.TableName, "Consolidate Number"));
			exclusions.Add(TableFilter(AccAlternateGLAccountSchema.Constants.TableName, "Alternate Number"));
			exclusions.Add(TableFilter(AccAlternateGLAccountSchema.Constants.TableName, "Total Reference Number"));

			return exclusions;
		}

		#region Override

		protected override void SetUp()
		{
			base.SetUp();
			Chart = Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(Chart, 1, "9999", "test", ".");
			Creator.CreateAccAlternateChartFormat(Chart, 2, "99", "test2", ".");
			Creator.CreateAccAlternateChartFormat(Chart, 3, "99", "test3", ".");
			Factory.Save();

			var chart2 = Creator.CreateAlternateChart("MG2", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(chart2, 1, "9999", "test", ".");
			Creator.CreateAccAlternateChartFormat(chart2, 2, "XX", "test2", ".");
			Creator.CreateAccAlternateChartFormat(chart2, 3, "99", "test3", ".");
			Factory.Save();

			var account = Creator.CreateAccAlternateGlAccount(Chart.PK, "10001000", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1, "10.00.1000");
			var account1 = Creator.CreateAccAlternateGlAccount(chart2.PK, "1000AA10", Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.DebitCredit.Credit, 1, AccGLHeader.Constants.SectionTypes.Codes.Assets, 2, "10.00.1010");
			Creator.GLHeader1.AG_CashFlowType = CashFlowCodeLists.Codes.CSH;
			Creator.GLHeader2.AG_CashFlowType = CashFlowCodeLists.Codes.NON;
			Creator.CreateAccAlternateGlAccountAttribute(account, Creator.GLHeader1.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");
			Creator.CreateAccAlternateGlAccountAttribute(account1, Creator.GLHeader1.PK, 2, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");
			Creator.CreateAccAlternateGlAccountAttribute(account, Creator.GLHeader2.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");

			Factory.Save();
			FilterBO = (AlternateGLAccountsFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		#endregion

		AccAlternateChart Chart;

		AlternateGLAccountCombineParentAccountCollection TestCollection
		{
			get { return testCollection ?? (testCollection = new AlternateGLAccountCombineParentAccountCollection(Factory)); }
		}

		AlternateGLAccountCombineParentAccountCollection testCollection;

		AlternateGLAccountsFilterBusinessObject FilterBO;

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));

		TestObjectCreator creator;
	}
}
