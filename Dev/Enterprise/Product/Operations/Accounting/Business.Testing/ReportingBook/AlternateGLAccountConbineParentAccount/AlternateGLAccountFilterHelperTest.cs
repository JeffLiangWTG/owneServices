using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AlternateGLAccountFilterHelper))]
	public class AlternateGLAccountFilterHelperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new AlternateGLAccountFilterHelper(Factory);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			FilterHelper = (AlternateGLAccountFilterHelper)CachedBusinessObject;

			var chart = Creator.CreateAlternateChart("MGT", "Management Reporting", true, true, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(chart, 1, "X", "2", ".");
			Factory.Save();

			var account = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1);
			var account1 = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1010", Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.DebitCredit.Credit, 1, AccGLHeader.Constants.SectionTypes.Codes.Assets, 2);

			var gLHeader1 = Creator.CreateAccGLHeader("Test.aa", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			var gLHeader2 = Creator.CreateAccGLHeader("Test.bb", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			Creator.CreateAccAlternateGlAccountAttribute(account, gLHeader1.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");
			Creator.CreateAccAlternateGlAccountAttribute(account1, gLHeader1.PK, 2, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");
			Creator.CreateAccAlternateGlAccountAttribute(account, gLHeader2.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");

			Factory.Save();
		}

		protected AlternateGLAccountFilterHelper FilterHelper;
		TestObjectCreator Creator;

		#region TestFilterHelper

		public void TestFilterHelper()
		{
			var nonCurrentCompanyChart = Creator.CreateAlternateChart("TRR", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			nonCurrentCompanyChart.AAC_GC_Company = Creator.NonCurrentCompany.PK;
			var currentCompanyChart = Creator.CreateAlternateChart("TCC", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Factory.Save();
			var nonCurrentCompanyAccount = Creator.CreateAccAlternateGlAccount(nonCurrentCompanyChart.PK, "10.00.1020", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit,
				1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1);
			var currentCompanyAccount = Creator.CreateAccAlternateGlAccount(currentCompanyChart.PK, "10.00.1030", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit,
				1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1);
			Factory.Save();

			var dynBizOs = new DynamicBusinessObjectCollection(Factory);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 4 AlternateGLAccounts", 4, dynBizOs.Count);
			Assert(!dynBizOs.Any(x => (ZGuid)x[AccAlternateChartSchema.AAC_GC_Company] == nonCurrentCompanyChart.AAC_GC_Company));

			var query = new ZQuery(AccAlternateGLAccountSchema.AGA_AccountNum, "10.00.1000");
			FilterHelper.SetOuterQuery(query);
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 2 AlternateGLAccounts", 2, dynBizOs.Count);

			FilterHelper.MaximumRows = 1;
			dynBizOs.Load(FilterHelper.PlainFilterWithoutParamValues, FilterHelper.FilterParameters);
			AssertEquals("There should be 1 AlternateGLAccounts", 1, dynBizOs.Count);
		}

		#endregion

		#endregion
	}
}
