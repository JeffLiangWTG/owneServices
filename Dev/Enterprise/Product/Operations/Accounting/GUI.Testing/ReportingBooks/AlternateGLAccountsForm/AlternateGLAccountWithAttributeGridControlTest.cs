using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.GUI.Testing
{
	public class AlternateGLAccountWithAttributeGridControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			var creator = new TestObjectCreator(Factory);
			var gLHeader = creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);

			var chart = creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			creator.CreateAccAlternateChartFormat(chart, 1, "X", "2", ".");
			creator.CreateAccAlternateGLAccountDissection(gLHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, true);

			var alternateGLAccounts = new AlternateGLAccounts(Factory);

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.New;
				form.OnShown_ForTestOnly(new EventArgs());
				form.Show();

				alternateGLAccounts.CreateMultipleAlternateGLAccount = true;
				alternateGLAccounts.ChartPK = chart.PK;
				alternateGLAccounts.ParentGLAccountPK = gLHeader.PK;

				var alternateAccountGroupBox = form.Controls.Find("alternateAccountGroupBox", true);
				AssertEquals(1, alternateAccountGroupBox.Length);
				AssertEquals(1, alternateAccountGroupBox[0].Controls.Find("alternateGLAccountWithAttributeGridControl", false).Length);
				var alternateGLAccountWithAttributeGridControl = alternateAccountGroupBox[0].Controls.Find("alternateGLAccountWithAttributeGridControl", false);
				var grid = alternateGLAccountWithAttributeGridControl[0].Controls.Find("attributeGrid", true).First() as ZGrid;
				Assert(grid.Columns.Contains("OrganizationCode"));
				Assert(grid.Columns.Contains("OCGDescription"));
				Assert(grid.Columns.Contains("TICDescription"));
				Assert(grid.Columns.Contains("LFEDescription"));
				Assert(grid.Columns.Contains("LFODescription"));
				Assert(grid.Columns.Contains("SPRDescription"));
				Assert(grid.Columns.Contains("AlternateGLAccountNum"));
				Assert(grid.Columns.Contains("AlternateGLAccount+AccountNumWithSeparator"));
				Assert(grid.Columns.Contains("DebitCredit"));
				Assert(grid.Columns.Contains("Description"));
				Assert(grid.Columns.Contains("ReportSection"));
				Assert(grid.Columns.Contains("AlternateGLAccount+PrefixedAccountNum"));
				Assert(grid.Columns.Contains("PercentNum"));
				Assert(grid.Columns.Contains("ConsolidationNum"));
				Assert(grid.Columns.Contains("AlternateNum"));
				Assert(grid.Columns.Contains("PrintSequence"));
			}
		}
	}
}
