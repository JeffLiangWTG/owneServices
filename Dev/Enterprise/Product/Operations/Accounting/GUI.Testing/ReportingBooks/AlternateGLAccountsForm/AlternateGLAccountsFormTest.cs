using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AlternateGLAccountsForm))]
	public class AlternateGLAccountsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			return new AlternateGLAccountsForm(alternateGLAccounts);
		}

		public void TestFormCaption()
		{
			using (var form = (AlternateGLAccountsForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();

				AssertEquals("Caption must be 'Alternate GL Account'", "Alternate GL Account", form.FormCaption);
			}
		}

		public void TestHasLogTab()
		{
			using (var form = (AlternateGLAccountsForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals(1, form.Controls.Find("formLogTabPage", true).Length);
			}
		}

		public void TestControls_NewForm()
		{
			var gLHeader = Creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			var chart = Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(chart, 1, "X", "2", ".");
			Creator.CreateAccAlternateGLAccountDissection(gLHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, true);

			var alternateGLAccounts = new AlternateGLAccounts(Factory);

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.Show();
				var chartAndAccountTypeGroupBox = form.Controls.Find("chartAndAccountTypeGroupBox", true);

				AssertEquals(1, chartAndAccountTypeGroupBox.Length);
				AssertEquals(1, chartAndAccountTypeGroupBox[0].Controls.Find("chartGuidFindBox", false).Length);
				AssertEquals(1, chartAndAccountTypeGroupBox[0].Controls.Find("accountTypeDropEdit", false).Length);

				var parentAccountGroupBox = form.Controls.Find("parentAccountGroupBox", true);
				AssertEquals(1, parentAccountGroupBox.Length);
				AssertEquals(1, parentAccountGroupBox[0].Controls.Find("cashFlowCategoryDropEdit", false).Length);
				AssertEquals(1, parentAccountGroupBox[0].Controls.Find("parentAccountGuidFindBox", false).Length);
				AssertEquals(1, parentAccountGroupBox[0].Controls.Find("unitsDropEdit", false).Length);

				var alternateAccountGroupBox = form.Controls.Find("alternateAccountGroupBox", true);
				AssertEquals(1, alternateAccountGroupBox.Length);
				AssertEquals(AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right, alternateAccountGroupBox[0].Anchor);
				AssertEquals(1, alternateAccountGroupBox[0].Controls.Find("singleAlternateGLAccountControl", false).Length);
				AssertEquals(true, alternateAccountGroupBox[0].Visible);

				alternateGLAccounts.CreateMultipleAlternateGLAccount = true;
				alternateGLAccounts.ChartPK = chart.PK;
				alternateGLAccounts.ParentGLAccountPK = gLHeader.PK;

				alternateAccountGroupBox = form.Controls.Find("alternateAccountGroupBox", true);
				AssertEquals(1, alternateAccountGroupBox.Length);

				var alternateGLAccountWithAttributeGridControl = alternateAccountGroupBox[0].Controls.Find("alternateGLAccountWithAttributeGridControl", false);
				AssertEquals(1, alternateGLAccountWithAttributeGridControl.Length);
				AssertEquals(DockStyle.Fill, alternateGLAccountWithAttributeGridControl[0].Dock);
			}
		}

		public void TestControls_EditForm_HasSeparateNumberingDissection()
		{
			AssertControls_EditForm(true);
		}

		public void TestControls_EditForm_HasNoSeparateNumberingDissection()
		{
			AssertControls_EditForm(false);
		}

		void AssertControls_EditForm(bool hasSeparateNumberingDissection)
		{
			var gLHeader = Creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			var chart = Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(chart, 1, "X", "2", ".");
			Creator.CreateAccAlternateGLAccountDissection(gLHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, true);

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			if (hasSeparateNumberingDissection)
			{
				var alternateGLAccountsWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
				alternateGLAccountsWithAttributeSet.AlternateGLAccount = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1100", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1);
				var attribute = alternateGLAccountsWithAttributeSet.Attributes.AddNew();
				attribute.AAA_Attribute = AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG;
			}

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				var editAlternateAccountTabControl = form.Controls.Find("editAlternateAccountTabControl", true);

				AssertEquals(1, editAlternateAccountTabControl.Length);
				AssertEquals(1, editAlternateAccountTabControl[0].Controls.Find("detailTabPage", false).Length);
				AssertEquals(1, editAlternateAccountTabControl[0].Controls.Find("relatedAlternateAccountsTabPage", false).Length);

				var detailTabPage = editAlternateAccountTabControl[0].Controls.Find("detailTabPage", true);
				AssertEquals(1, detailTabPage.Length);
				AssertEquals(1, detailTabPage[0].Controls.Find("attributesGroupBox", false).Length);
				AssertEquals(1, detailTabPage[0].Controls.Find("alternateAccountDetailGroupBox", false).Length);

				var attributesGroupBox = detailTabPage[0].Controls.Find("attributesGroupBox", false);
				var grid = attributesGroupBox[0].Controls.Find("alternateAccountAttributeGrid", false).First() as ZGrid;
				Assert(grid.Columns.Contains("AAA_Attribute"));
				Assert(grid.Columns.Contains("ValueDescription"));

				(editAlternateAccountTabControl[0] as ZTabControl).SelectTab("relatedAlternateAccountsTabPage");
				var relatedAlternateAccountsTabPage = editAlternateAccountTabControl[0].Controls.Find("relatedAlternateAccountsTabPage", true);
				AssertEquals(1, relatedAlternateAccountsTabPage.Length);
				AssertEquals(1, relatedAlternateAccountsTabPage[0].Controls.Find("alternateGLAccountWithAttributeGridInTabControl", false).Length);

				form.OnShown_ForTestOnly(new EventArgs());
				var alternateAccountGroupBox = form.Controls.Find("alternateAccountGroupBox", true);
				AssertEquals(1, alternateAccountGroupBox.Length);
				AssertEquals(!hasSeparateNumberingDissection, alternateAccountGroupBox[0].Visible);
			}
		}

		public void TestSetParentGLAccountPK()
		{
			SetUpForSetParentGLAccountPKAndChartPK();

			var debtorOrgCount = GetDebtorOrgCount();
			var oCGValueCount = AccountingMasterFilesConstants.OCGList.Count;
			var lFOValueCount = AccountingMasterFilesConstants.LFOList.Count;
			var lFEValueCount = AccountingMasterFilesConstants.LFEList.Count;
			var tICValueCount = AccountingMasterFilesConstants.TICList.Count;

			var alternateGLAccounts = new AlternateGLAccounts(Factory);

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.New;
				form.OnShown_ForTestOnly(new EventArgs());
				form.Show();
				alternateGLAccounts.ChartPK = Chart.PK;
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count, 1);

				alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.ParentGLAccountPK, GLHeader.PK);
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count, debtorOrgCount);
				Assert(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.ParentGLAccountPK == GLHeader.PK));

				alternateGLAccounts.ParentGLAccountPK = GLHeader2.PK;
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.ParentGLAccountPK, GLHeader2.PK);
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count, 1);
				Assert(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.ParentGLAccountPK == GLHeader2.PK));

				alternateGLAccounts.ChartPK = Chart2.PK;
				alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.ParentGLAccountPK, GLHeader.PK);
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count, oCGValueCount * lFOValueCount * lFEValueCount * tICValueCount);
				Assert(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.ParentGLAccountPK == GLHeader.PK));

				alternateGLAccounts.ParentGLAccountPK = GLHeader2.PK;
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.ParentGLAccountPK, GLHeader2.PK);
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count, 1);
				Assert(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.ParentGLAccountPK == GLHeader2.PK));
			}
		}

		public void TestSetParentGLAccountPKWithExistAlternateGLAccountWithoutAttribute()
		{
			SetUpForSetParentGLAccountPKAndChartPK();
			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			var alternateGLAccountWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = AlternateGLAccount;

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.Show();
				alternateGLAccounts.ParentGLAccountPK = GLHeader2.PK;
				AssertNotEquals(ZString.Empty, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSetParentGLAccountPKWithExistAlternateGLAccountWithAttribute()
		{
			SetUpForSetParentGLAccountPKAndChartPK();
			var attribute = Creator.CreateAccAlternateGlAccountAttribute(AlternateGLAccount, GLHeader.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, AccountingMasterFilesConstants.LFOCodes.FOR);
			Factory.Save();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			var alternateGLAccountWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = AlternateGLAccount;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet = alternateGLAccountWithAttributeSet;

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				alternateGLAccounts.ParentGLAccountPK = GLHeader2.PK;
				AssertEquals(@"Parent Account '10.00.1010' has dissection configuration with separate number configuration, clearing/changing the value will result in all saved Alternate Accounts be deleted.
Click 'Yes' to continue and system will delete all Alternate Accounts previously mapped against Parent Account '10.00.1010'.
Click 'No' to cancel the change and revert back to original value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(alternateGLAccounts.ParentGLAccountPK, GLHeader.PK);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				alternateGLAccounts.ParentGLAccountPK = GLHeader2.PK;
				AssertEquals(@"Parent Account '10.00.1010' has dissection configuration with separate number configuration, clearing/changing the value will result in all saved Alternate Accounts be deleted.
Click 'Yes' to continue and system will delete all Alternate Accounts previously mapped against Parent Account '10.00.1010'.
Click 'No' to cancel the change and revert back to original value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(alternateGLAccounts.ParentGLAccountPK, GLHeader2.PK);
				Assert(AlternateGLAccount.IsDeleted);
				Assert(attribute.IsDeleted);
				Assert(alternateGLAccounts.AlternateGLAccountsWithAttributeDeleted);
			}
		}

		public void TestSetParentGLAccountPK_SetAlternateGLAcocuntNonReadOnaly()
		{
			SetUpForSetParentGLAccountPKAndChartPK();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = Chart.PK;

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.New;
				form.OnShown_ForTestOnly(new EventArgs());
				form.Show();

				var alternateAccountGroupBox = form.Controls.Find("alternateAccountGroupBox", true);
				var singleAlternateGLAccountControl = alternateAccountGroupBox[0].Controls.Find("singleAlternateGLAccountControl", false)[0];
				AssertNotNull(singleAlternateGLAccountControl);
				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);

				alternateGLAccounts.ChartPK = Chart.PK;
				alternateGLAccounts.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				alternateGLAccounts.ParentGLAccountPK = GLHeader3.PK;

				alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1102";
				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);
				foreach (Control control in singleAlternateGLAccountControl.Controls.Find("singleAlternateGLAccountPanel", true)[0].Controls)
				{
					AssertEquals(true, control.Enabled);
				}

				alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = AlternateGLAccount.AGA_AccountNum;
				AssertEquals(true, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);
				foreach (Control control in singleAlternateGLAccountControl.Controls.Find("singleAlternateGLAccountPanel", true)[0].Controls)
				{
					if (control.Name != "accountNumberTextBox" && control.Name != "existAlternateAccountLabel")
					{
						AssertEquals(false, control.Enabled);
					}
					else
					{
						AssertEquals(true, control.Enabled);
					}
				}

				alternateGLAccounts.ParentGLAccountPK = ZGuid.Empty;

				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);
				foreach (Control control in singleAlternateGLAccountControl.Controls.Find("singleAlternateGLAccountPanel", true)[0].Controls)
				{
					AssertEquals(true, control.Enabled);
				}
			}
		}

		public void TestSetChartPK()
		{
			SetUpForSetParentGLAccountPKAndChartPK();

			var debtorOrgCount = GetDebtorOrgCount();
			var oCGValueCount = AccountingMasterFilesConstants.OCGList.Count;
			var lFOValueCount = AccountingMasterFilesConstants.LFOList.Count;
			var lFEValueCount = AccountingMasterFilesConstants.LFEList.Count;
			var tICValueCount = AccountingMasterFilesConstants.TICList.Count;

			var alternateGLAccounts = new AlternateGLAccounts(Factory);

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.New;
				form.OnShown_ForTestOnly(new EventArgs());
				form.Show();
				alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count, 1);

				alternateGLAccounts.ChartPK = Chart.PK;
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.ChartPK, Chart.PK);
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count, debtorOrgCount);
				Assert(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.Chart.PK == Chart.PK));

				alternateGLAccounts.ChartPK = Chart2.PK;
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.ChartPK, Chart2.PK);
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count, oCGValueCount * lFOValueCount * lFEValueCount * tICValueCount);
				Assert(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.Chart.PK == Chart2.PK));

				alternateGLAccounts.ParentGLAccountPK = GLHeader2.PK;
				alternateGLAccounts.ChartPK = Chart.PK;
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.ChartPK, Chart.PK);
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count, 1);
				Assert(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.Chart.PK == Chart.PK));

				alternateGLAccounts.ChartPK = Chart2.PK;
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.ChartPK, Chart2.PK);
				AssertEquals(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count, 1);
				Assert(alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.Chart.PK == Chart2.PK));
			}
		}

		public void TestDelete_AlternateGLAccountWithMultipleAttribute()
		{
			SetUpForSetParentGLAccountPKAndChartPK();
			Creator.CreateAccAlternateGlAccountAttribute(AlternateGLAccount, GLHeader.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, AccountingMasterFilesConstants.LFOCodes.FOR);

			var newAlternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "30.00.1100", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1);
			Creator.CreateAccAlternateGlAccountAttribute(newAlternateGLAccount, GLHeader.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, AccountingMasterFilesConstants.LFECodes.WEU);
			Factory.Save();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			var alternateGLAccountWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = AlternateGLAccount;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet = alternateGLAccountWithAttributeSet;

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				((IPostingButtonsProvider)form).CommandButtonPost.PerformClick();
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "The selected Alternate Account '10.00.1100' has related Alternate Accounts mapped to the same Parent Account '10.00.1010'.\r\nYou can view the list of related Alternate Accounts via the 'Related Alternate Accounts' tab.\r\nClick 'Yes' to continue and system will delete the selected Alternate Account '10.00.1100'.\r\nClick 'No' to cancel the action.");
				AssertEquals(false, alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.IsDeleted);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				((IPostingButtonsProvider)form).CommandButtonPost.PerformClick();
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "The selected Alternate Account '10.00.1100' has related Alternate Accounts mapped to the same Parent Account '10.00.1010'.\r\nYou can view the list of related Alternate Accounts via the 'Related Alternate Accounts' tab.\r\nClick 'Yes' to continue and system will delete the selected Alternate Account '10.00.1100'.\r\nClick 'No' to cancel the action.");
				AssertEquals(true, alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.IsDeleted);
			}
		}

		public void TestDelete_NoException_NewCreatedAccAlternateGLAccount()
		{
			var chart = Creator.CreateAlternateChart("ABC");

			var dissection = Creator.GLHeader1.AlternateGLAccountDissections.AddNew();
			dissection.ADC_AAC_AlternateChart = chart.PK;
			dissection.ADC_Attribute = "OCG";
			dissection.ADC_SeparateNumbering = true;
			Factory.Save();

			var account = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			var attribute = Creator.CreateAccAlternateGlAccountAttribute(account, Creator.GLHeader1.PK);
			Factory.Save();

			var account1 = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1010", "P&L", "CR", 1, "AS", 2);
			var attribute1 = Creator.CreateAccAlternateGlAccountAttribute(account1, Creator.GLHeader1.PK, 2);

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = chart.PK;
			alternateGLAccounts.ParentGLAccountPK = Creator.GLHeader1.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();

			var alternateGLAccountWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = account;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet = alternateGLAccountWithAttributeSet;

			alternateGLAccounts.ResetAlternateGLAccountsWithAttributeSet(alternateGLAccounts.ChartPK, alternateGLAccounts.ParentGLAccountPK);

			using (var form = new AlternateGLAccountsFormForTest(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(() => form.HandleApplyPostingButtonClickUnsafe_ForTestOnly());
			}
		}

		public void TestDelete_AlternateGLAccountWithManyParent()
		{
			SetUpForSetParentGLAccountPKAndChartPK();
			Creator.CreateAccAlternateGlAccountAttribute(AlternateGLAccount, GLHeader.PK, 1, null, null);
			Creator.CreateAccAlternateGlAccountAttribute(AlternateGLAccount, GLHeader2.PK, 1, null, null);

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			var alternateGLAccountWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = AlternateGLAccount;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet = alternateGLAccountWithAttributeSet;

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				((IPostingButtonsProvider)form).CommandButtonPost.PerformClick();
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "The selected Alternate Account '10.00.1100' is mapped to the following Parent Accounts:\r\n10.00.1010 - desc\r\n20.00.1010 - desc\r\n\r\nClick 'Yes' to continue and system will delete the Alternate Accounts and related mappings.\r\nClick 'No' to cancel the action.");
				AssertEquals(false, alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.IsDeleted);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				((IPostingButtonsProvider)form).CommandButtonPost.PerformClick();
				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "The selected Alternate Account '10.00.1100' is mapped to the following Parent Accounts:\r\n10.00.1010 - desc\r\n20.00.1010 - desc\r\n\r\nClick 'Yes' to continue and system will delete the Alternate Accounts and related mappings.\r\nClick 'No' to cancel the action.");
				AssertEquals(true, alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.IsDeleted);
			}
		}

		public void TestAlternateGLAccountNumChanged_NewForm()
		{
			SetUpForSetParentGLAccountPKAndChartPK();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = Chart.PK;

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.New;
				form.OnShown_ForTestOnly(new EventArgs());
				form.Show();

				var alternateAccountGroupBox = form.Controls.Find("alternateAccountGroupBox", true);
				var singleAlternateGLAccountControl = alternateAccountGroupBox[0].Controls.Find("singleAlternateGLAccountControl", false)[0];
				AssertNotNull(singleAlternateGLAccountControl);
				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);

				alternateGLAccounts.ChartPK = Chart.PK;
				alternateGLAccounts.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				alternateGLAccounts.ParentGLAccountPK = GLHeader3.PK;

				alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1102";
				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);
				foreach (Control control in singleAlternateGLAccountControl.Controls.Find("singleAlternateGLAccountPanel", true)[0].Controls)
				{
					AssertEquals(true, control.Enabled);
				}

				alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = AlternateGLAccount.AGA_AccountNum;
				AssertEquals(true, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);
				foreach (Control control in singleAlternateGLAccountControl.Controls.Find("singleAlternateGLAccountPanel", true)[0].Controls)
				{
					if (control.Name != "accountNumberTextBox" && control.Name != "existAlternateAccountLabel")
					{
						AssertEquals(false, control.Enabled);
					}
					else
					{
						AssertEquals(true, control.Enabled);
					}
				}

				Factory.Save();

				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);
				foreach (Control control in singleAlternateGLAccountControl.Controls.Find("singleAlternateGLAccountPanel", true)[0].Controls)
				{
					AssertEquals(true, control.Enabled);
				}
			}
		}

		public void TestAlternateGLAccountNumChanged_EditForm()
		{
			SetUpForSetParentGLAccountPKAndChartPK();

			var testObjectCreator = new TestObjectCreator(Factory);
			var alternateGLAccount = testObjectCreator.CreateAccAlternateGlAccount(Chart.PK, "20.00.1100", "BSH");
			var attribute = testObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, GLHeader2.PK, attribute: string.Empty);
			Factory.Save();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.ParentGLAccountPK = GLHeader2.PK;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = alternateGLAccount.AGA_AccountNum;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.Attributes.Add(attribute);
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.ParentGLAccountPK = GLHeader2.PK;

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.OnShown_ForTestOnly(new EventArgs());
				form.Show();

				var alternateAccountGroupBox = form.Controls.Find("alternateAccountGroupBox", true);
				var singleAlternateGLAccountControl = alternateAccountGroupBox[0].Controls.Find("singleAlternateGLAccountControl", false)[0];
				AssertNotNull(singleAlternateGLAccountControl);
				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = AlternateGLAccount.AGA_AccountNum;
				AssertEquals(@"Alternate Account '20.00.1100' is no longer mapped to any Parent Account with this update and will be deleted.
Click 'Yes' to continue. 
Click 'No' to cancel the change and revert back to the original value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum, "20.00.1100");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = AlternateGLAccount.AGA_AccountNum;
				AssertEquals(@"Alternate Account '20.00.1100' is no longer mapped to any Parent Account with this update and will be deleted.
Click 'Yes' to continue. 
Click 'No' to cancel the change and revert back to the original value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, alternateGLAccount.IsDeleted);
				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);

				foreach (Control control in singleAlternateGLAccountControl.Controls.Find("singleAlternateGLAccountPanel", true)[0].Controls)
				{
					AssertEquals(true, control.Enabled);
				}
			}
		}

		public void TestAlternateGLAccountNumChanged_NoParentAccount_NewForm()
		{
			SetUpForSetParentGLAccountPKAndChartPK();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.AccountType = "TTL";

			using (var form = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				form.DisplayMode = ODisplayMode.New;
				form.OnShown_ForTestOnly(new EventArgs());
				form.Show();

				var alternateAccountGroupBox = form.Controls.Find("alternateAccountGroupBox", true);
				var singleAlternateGLAccountControl = alternateAccountGroupBox[0].Controls.Find("singleAlternateGLAccountControl", false)[0];
				AssertNotNull(singleAlternateGLAccountControl);
				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);
				AssertEquals(true, alternateGLAccounts.ParentGLAccountPKInfo.ReadOnly);

				alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = AlternateGLAccount.AGA_AccountNum;

				AssertNotEquals(alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.PK, AlternateGLAccount.PK);
				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);
				foreach (Control control in singleAlternateGLAccountControl.Controls.Find("singleAlternateGLAccountPanel", true)[0].Controls)
				{
					AssertEquals(true, control.Enabled);
				}

				alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.1102";
				AssertEquals(false, singleAlternateGLAccountControl.GetControl<ZLabel>("existAlternateAccountLabel", true).Visible);

				foreach (Control control in singleAlternateGLAccountControl.Controls.Find("singleAlternateGLAccountPanel", true)[0].Controls)
				{
					AssertEquals(true, control.Enabled);
				}
			}
		}

		public void TestIdentifierForPersistingForm()
		{
			var chart = Creator.CreateAlternateChart("MGT", "Management Reporting");
			Creator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			var glHeader = Creator.CreateGLHeader("Test.aa");

			Factory.Save();

			var alternateGLAccountWithAttribute = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			var attribute = Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccountWithAttribute, glHeader.PK, 1, string.Empty, string.Empty);
			var alternateGLAccountWithoutAttribute = Creator.CreateAccAlternateGlAccount(chart.PK, "20.00.1000", Core.Constants.AccountType.Header, "DR", 1, "OV", 1);
			Factory.Save();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = chart.PK;
			alternateGLAccounts.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			alternateGLAccounts.ParentGLAccountPK = glHeader.PK;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.Attributes.Add(attribute);
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = alternateGLAccountWithAttribute.AGA_AccountNum;

			using (var testForm = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				AssertEquals("IdentifierForPersistingForm should be Attribute PK for Alternate GL Account with attribute.", attribute.PK, testForm.IdentifierForPersistingForm);
			}

			alternateGLAccounts = new AlternateGLAccounts(Factory);
			var alternateGLAccountsWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountsWithAttributeSet.AlternateGLAccount = alternateGLAccountWithoutAttribute;
			alternateGLAccountsWithAttributeSet.AlternateGLAccountNum = alternateGLAccountWithoutAttribute.AGA_AccountNum;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet = alternateGLAccountsWithAttributeSet;

			using (var testForm = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				AssertEquals("IdentifierForPersistingForm should be Alternate GL Account PK for Alternate GL Account without attribute.", alternateGLAccountWithoutAttribute.PK, testForm.IdentifierForPersistingForm);
			}
		}

		public void TestSaveToRecentItems()
		{
			var chart = Creator.CreateAlternateChart("MGT", "Management Reporting");
			Creator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			var glHeader = Creator.CreateGLHeader("Test.aa");
			var glHeader2 = Creator.CreateGLHeader("Test.bb");

			Factory.Save();

			var alternateGLAccountWithAttribute = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			var attribute = Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccountWithAttribute, glHeader.PK, 1, string.Empty, string.Empty);
			var attribute2 = Creator.CreateAccAlternateGlAccountAttribute(alternateGLAccountWithAttribute, glHeader2.PK, 1, string.Empty, string.Empty);

			Factory.Save();

			var stmLink = Factory.NewWithValidTestData<StmLink>();
			stmLink.STL_ModuleID = ModuleIDs.AlternateGLAccounts.Name;
			stmLink.STL_ItemDescription = "10.00.1000 - 10.00.1000 - Test.aa";
			stmLink.STL_ItemPK = attribute.PK;
			stmLink.STL_LinkType = "FLF";
			stmLink.STL_GS_NKUser = "~BP";
			stmLink.STL_GC_LogonCompany = GlbCompany.CurrentCompany.PK;
			stmLink.STL_LastUsedDateTimeUtc = ZDateTime.Now;
			Factory.Save();

			var stmLinkQuery = new ZQuery();
			stmLinkQuery.AddToFilter(StmLinkSchema.STL_ModuleID, ModuleIDs.AlternateGLAccounts.Name);
			var stmLinks = Factory.Load<StmLink>(stmLinkQuery);
			AssertEquals(1, stmLinks.Length);
			AssertEquals("10.00.1000 - 10.00.1000 - Test.aa", stmLinks[0].STL_ItemDescription);

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = chart.PK;
			alternateGLAccounts.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			alternateGLAccounts.ParentGLAccountPK = glHeader2.PK;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.Attributes.Add(attribute2);
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = alternateGLAccountWithAttribute.AGA_AccountNum;

			using (var testForm = new AlternateGLAccountsForm(alternateGLAccounts))
			{
				alternateGLAccountWithAttribute.AGA_Description = "Test";
				testForm.SaveToRecentItems_ForTestOnly();
				Factory.Save();

				stmLinks = Factory.Load<StmLink>(stmLinkQuery);
				AssertEquals(1, stmLinks.Length);
				AssertEquals("10.00.1000 - Test - Test.aa", stmLinks[0].STL_ItemDescription);
			}
		}

		int GetDebtorOrgCount()
		{
			var query = new ZDBOnlyQuery(typeof(OrgHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			var fromAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
			fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			subQuery.AddToFilter(fromAccountFilter);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return Factory.Load<OrgHeader>(query).Length;
		}

		void SetUpForSetParentGLAccountPKAndChartPK()
		{
			Chart = Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Chart2 = Creator.CreateAlternateChart("MG2", "Management Reporting", true, true, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(Chart, 1, "X", "2", ".");
			Creator.CreateAccAlternateChartFormat(Chart2, 1, "X", "2", ".");

			GLHeader = Creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			GLHeader.AG_CashFlowType = "XXX";
			GLHeader.AG_StatisticalUnits = "KG";

			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, true);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart2.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, true);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart2.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, true);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart2.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart2.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, true);

			GLHeader2 = Creator.CreateAccGLHeader("20.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, false);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, false);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, false);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, false);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, false);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, false);
			GLHeader3 = Creator.CreateAccGLHeader("30.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			Factory.Save();

			AlternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "10.00.1100", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader.PK.ToGuid());
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (AlternateGLAccountsForm)GetFormToBashCore())
			{
				AssertNotNull("Alternate GL Account form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		public new void TestAuditPlugIn()
		{
			using (var form = GetFormToBash())
			{
				var zform = form as ZForm;

				var bizObj = (zform.BusinessEntity as AlternateGLAccounts).FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount;
				AssertNotNull("Business Object of form with audit plug-in", bizObj);

				var expectedAuditTable = string.Format(CultureInfo.InvariantCulture,
					"[{0}].[{1}].[{2}]",
					CargoWise.Data.Db.AuditDatabaseName,
					bizObj.PKSchemaColumn.TableSchema.SqlSchemaName,
					bizObj.TableName);
				var auditTableIdSql = string.Format(CultureInfo.InvariantCulture, "SELECT OBJECT_ID(N'{0}', N'U')", expectedAuditTable);

				Assert(
					"Business Object audit table " + expectedAuditTable + " does not exist",
					((IDbConnected)Factory).Connection.ExecuteScalar(auditTableIdSql) != DBNull.Value);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
		}

		AccAlternateChart Chart;
		AccAlternateChart Chart2;
		AccGLHeader GLHeader;
		AccGLHeader GLHeader2;
		AccGLHeader GLHeader3;
		AccAlternateGLAccount AlternateGLAccount;
		TestObjectCreator Creator;

		class AlternateGLAccountsFormForTest : AlternateGLAccountsForm
		{
			public AlternateGLAccountsFormForTest(AlternateGLAccounts alternateGLAccounts) : base(alternateGLAccounts)
			{
			}

			public void HandleApplyPostingButtonClickUnsafe_ForTestOnly(bool closeOnSave = true)
			{
				HandleApplyPostingButtonClickUnsafe(closeOnSave);
			}
		}
	}
}
