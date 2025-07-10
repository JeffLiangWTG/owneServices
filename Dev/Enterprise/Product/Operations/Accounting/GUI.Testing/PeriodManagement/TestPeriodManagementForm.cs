using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.Startup;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.GUI.PeriodManagement.Testing
{
	public class TestPeriodManagementForm : TestCaseWithFactory
	{
		public void TestReopenPeriodMenuAdding()
		{
			UsersAuthorizedToReopenClosedPeriodsCollection collection = new UsersAuthorizedToReopenClosedPeriodsCollection();
			collection.AddNew();
			collection[0].StaffPK = GlbStaff.CurrentUser.PK;
			AccountingConfigurationRegistry.Instance.UsersAuthorizedToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			using (PeriodManagementForm formForTest = new PeriodManagementForm())
			{
				MenuItem menuItem = formForTest.PeriodsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Reopen Period");
				AssertNotNull(menuItem);
			}
		}

		public void TestReopenPeriod()
		{
			UsersAuthorizedToReopenClosedPeriodsCollection collection = new UsersAuthorizedToReopenClosedPeriodsCollection();
			collection.AddNew();
			collection[0].StaffPK = Factory.NewWithValidTestData<GlbStaff>().PK;
			Factory.Save();
			AccountingConfigurationRegistry.Instance.UsersAuthorizedToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			using (var hostForm = new ZForm())
			using (PeriodManagementForm formForTest = new PeriodManagementForm())
			{
				hostForm.Controls.Add(formForTest);
				hostForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				formForTest.ReopenPeriod(null, null);
				AssertEquals("Last message", "Please select a period to reopen", UnitTestUserNotification.Instance.LastMessage.Text);

				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 1, 1);
				testNewYear.EndDate = new ZDateTime(1999, 12, 31);
				PeriodManager testManager = new PeriodManager(Factory);
				formForTest.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);
				Factory.Save();
				formForTest.PeriodManager_ForTestOnly.FinancialYear = 1999;
				formForTest.PeriodsGrid_ForTestOnly.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				formForTest.ReopenPeriod(null, null);
				AssertEquals("Last message", "Please select a period to reopen", UnitTestUserNotification.Instance.LastMessage.Text);

				formForTest.PeriodsGrid_ForTestOnly.UnSelectAll();
				formForTest.PeriodsGrid_ForTestOnly.Select(0);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				formForTest.ReopenPeriod(null, null);
				AssertEquals("Last message", "Nothing to reopen!", UnitTestUserNotification.Instance.LastMessage.Text);

				formForTest.PeriodsGrid_ForTestOnly.UnSelectAll();
				formForTest.PeriodsGrid_ForTestOnly.Select(0);
				AccPeriodManagement period = (AccPeriodManagement)formForTest.PeriodsGrid_ForTestOnly.SelectedElements[0];
				period.AM_IsSubLedgerClosed = true;
				period.AM_IsGeneralLedgerClosed = true;
				period.AM_IsSubledgerClosedForAdjustments = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				formForTest.ReopenPeriod(null, null);
				AssertEquals("Last message", @"You cannot reopen this period.
You do not have sufficient Reopen Period Security Level rights.", UnitTestUserNotification.Instance.LastMessage.Text);

				PeriodReopenLevelsCollection levelsCollection = new PeriodReopenLevelsCollection();
				levelsCollection.AddNew();
				levelsCollection[0].AuthorisationRequirement = PeriodReopenLevels.AuthorisationRequirementCodes.FirstApprovalRequired;
				levelsCollection[0].Range = PeriodReopenLevels.RangeCodes.Unlimited;
				AccountingConfigurationRegistry.Instance.PeriodReopenLevels.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, levelsCollection);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				formForTest.ReopenPeriod(null, null);
				AssertEquals("Last message", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(typeof(ReopenPeriodKeyForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				collection.AddNew();
				collection[1].StaffPK = GlbStaff.CurrentUser.PK;
				AccountingConfigurationRegistry.Instance.UsersAuthorizedToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				formForTest.ReopenPeriod(null, null);
				AssertEquals("Last message", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(typeof(ReopenPeriodForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestReopenPeriod_ShowLoginForm()
		{
			var objectCreator = new TestObjectCreator(Factory);
			SecurityTestObject.CreateTestUser(true, Env.Security.PeriodManagementReOpenPeriodLevel3.Code, "YOU", "YourName", "YourPassword");
			var testUser = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "YOU"));
			testUser.GS_FullName = "user1";
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (var hostForm = new ZForm())
			using (var form = new PeriodManagementForm())
			{
				hostForm.Controls.Add(form);
				hostForm.Show();
				TryReopenAndAssert("Please select a period to reopen");

				var newYear = new NewYearPeriodSettings
				{
					StartDate = new ZDateTime(1999, 1, 1),
					EndDate = new ZDateTime(1999, 12, 31)
				};
				var manager = new PeriodManager(Factory);
				form.GenerateNewPeriod_ForTestOnly(newYear, Factory, manager);
				Factory.Save();

				form.PeriodManager_ForTestOnly.FinancialYear = 1999;
				form.PeriodsGrid_ForTestOnly.SelectAllElements();
				TryReopenAndAssert("Please select a period to reopen");

				form.PeriodsGrid_ForTestOnly.UnSelectAll();
				form.PeriodsGrid_ForTestOnly.Select(0);
				TryReopenAndAssert("Nothing to reopen!");

				var period = (AccPeriodManagement)form.PeriodsGrid_ForTestOnly.SelectedElements[0];
				period.AM_IsSubLedgerClosed = true;
				period.AM_IsGeneralLedgerClosed = true;
				period.AM_IsSubledgerClosedForAdjustments = true;
				TryReopenAndAssert(@"You cannot reopen this period.
You do not have sufficient Reopen Period Security Level rights.");

				var levels = new PeriodReopenLevelsCollection
				{
					new PeriodReopenLevels
					{
						Range = PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod,
						Days = 1,
						AuthorisationRequirement = PeriodReopenLevels.AuthorisationRequirementCodes.FirstApprovalRequired
					},
					new PeriodReopenLevels
					{
						Range = PeriodReopenLevels.RangeCodes.DaysAfterEndOfPeriod,
						Days = 400,
						AuthorisationRequirement = PeriodReopenLevels.AuthorisationRequirementCodes.SecondApprovalRequired
					},
					new PeriodReopenLevels
					{
						Range = PeriodReopenLevels.RangeCodes.Unlimited,
						Days = 0,
						AuthorisationRequirement = PeriodReopenLevels.AuthorisationRequirementCodes.ThirdApprovalRequired
					}
				};

				var noRightsUser = "user1";
				var noRightsPass = "pass1";
				var unauthorizedUser = "user2";
				var unauthorizedPass = "pass2";
				var authorizedUser = "user3";
				var authorizedPass = "pass3";

				SecurityTestObject.CreateTestUser(true, Env.Security.None.Code, "US1", noRightsUser, noRightsPass);
				SecurityTestObject.CreateTestUser(true, Env.Security.PeriodManagementReOpenPeriodLevel3.Code, "US2", unauthorizedUser, unauthorizedPass);
				SecurityTestObject.CreateTestUser(true, Env.Security.PeriodManagementReOpenPeriodLevel3.Code, "US3", authorizedUser, authorizedPass);

				var staffNoRights = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "US1"));
				staffNoRights.GS_FullName = "No Rights User";
				var staffUnauthorized = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "US2"));
				staffUnauthorized.GS_FullName = "Unauthorized User";
				var staffAuthorized = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "US3"));
				staffAuthorized.GS_FullName = "Authorized User";
				var collection = new UsersAuthorizedToReopenClosedPeriodsCollection();
				collection.AddNew().StaffPK = staffNoRights.PK;
				collection.AddNew().StaffPK = staffAuthorized.PK;
				Factory.Save();

				AccountingConfigurationRegistry.Instance.PeriodReopenLevels.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, levels);
				AccountingConfigurationRegistry.Instance.EnableSelfAdministrationToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.EnableUsersAuthorisedToReopenClosedPeriodsRegistry.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.UsersAuthorizedToReopenClosedPeriods.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

				SetLoginDelegate(noRightsUser, noRightsPass);
				TryReopenAndAssert(@"The Authorizing user does not have security rights for this Branch or Department. Your system administrator maintains each user's security rights.");
				AssertEquals("LastFormShownDialogForTest", "Security Override Login", ZFormModaliser.LastFormShownDialogForTest.Text);

				SetLoginDelegate(unauthorizedUser, unauthorizedPass);
				TryReopenAndAssert(@"You do not have the appropriate security right to run this function.
If you require access to this function, ask your system administration to change your Staff or Group Security Rights to allow access to:
Manage > General Ledger > Period Management > Reopen Period");
				AssertEquals("LastFormShownDialogForTest", "Security Override Login", ZFormModaliser.LastFormShownDialogForTest.Text);

				SetLoginDelegate(authorizedUser, authorizedPass);
				TryReopenAndAssert(null);
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(typeof(ReopenPeriodForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				void TryReopenAndAssert(string expectedMessage)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.ReopenPeriod(null, null);
					AssertEquals("Last message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			void SetLoginDelegate(string user, string pass)
			{
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
				{
					if (form is LoginForm loginForm)
					{
						loginForm.DoLoginForTest(user, pass);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});
			}
		}

		public void TestReAggregateAllCompaniesConfirmation_Yes()
		{
			AssertReAggregateAllCompaniesCallsReAggregatorWithMessage(DialogResult.Yes, Times.Once());
		}

		public void TestReAggregateAllCompaniesConfirmation_No()
		{
			AssertReAggregateAllCompaniesCallsReAggregatorWithMessage(DialogResult.No, Times.Never());
		}

		public void TestReAggregateThisCompanyConfirmation_Yes()
		{
			AssertReAggregateThisCompanyCallsReAggregatorWithMessage(DialogResult.Yes, Times.Once());
		}

		public void TestReAggregateThisCompanyConfirmation_No()
		{
			AssertReAggregateThisCompanyCallsReAggregatorWithMessage(DialogResult.No, Times.Never());
		}

		public void TestPurgeGLD()
		{
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(DateTime.Today.Year);
			var creator = new TestObjectCreator(Factory);
			TestObjectCreator.SetTemporaryControlAccounts();

			Factory.Save();

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var period = periodCalculator.GetFirstPeriodForYear(DateTime.Today.Year);
			var firstPeriodStartDate = periodCalculator.GetFirstDayForPeriod(period);

			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today);
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, firstPeriodStartDate.ToDateTime());
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today);
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today);

			var generalLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			generalLedgerData.GLD_Type = "PST";
			generalLedgerData.GLD_GLAccountType = "ARC";
			generalLedgerData.GLD_PostDate = ZDateTime.Today;
			generalLedgerData.GLD_PostPeriod = 1;
			generalLedgerData.GLD_GC_Company = GlbCompany.CurrentCompany.PK;
			generalLedgerData.GLD_GB_Branch = GlbBranch.CurrentBranch.PK;
			generalLedgerData.GLD_GE_Department = GlbDepartment.CurrentDepartment.PK;

			var invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 0M, 100M, 0M);

			Factory.Save();

			var sql = $@"INSERT INTO dbo.AccTransactionPostingToGLDQueue (APQ_ParentID, APQ_ParentTableCode, APQ_GC_Company, APQ_JournalDate, APQ_IsReverse, APQ_SystemCreateTimeUtc, APQ_SystemCreateUser)
VALUES('{invoice.PK.ToGuid()}', 'AH', '{GlbCompany.CurrentCompany.PK.ToGuid()}', '{ZDateTime.Today}', '0', GetUtcDate(), '{GlbStaff.CurrentUser.GS_Code}')";
			Db.Connection.ExecuteNonQuery(sql);

			using (PeriodManagementForm form = new PeriodManagementForm())
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.PurgeGLDButton_ForTestOnly.PerformClick();
				AssertEquals($"Are you sure you want to delete ALL General Ledger Data and Settings from Company {GlbCompany.CurrentCompany.CompanyName}?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PurgeGLDButton_ForTestOnly.PerformClick();
				var expectedMessage = @"All existing General Ledger Data (GLD) records have been deleted and all related GLD configurations have been cleared.

However, new GLD records may be inserted as a result of queued records processed by GLD Service Tasks before or during the delete operation.

Thus, please check that no new GLD records can be found in Manage > General Ledger > Accounting Journals module.

If there are, please run the ""Clear GLD configurations and Data"" function again.";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Assert(!Db.Connection.Exists(
					$@"FROM dbo.StmData WHERE SD_Owner = '{GlbCompany.CurrentCompany.PK.ToGuid()}' AND SD_Name IN (
'GenerateJournalEntriesCDCStartDate',
'GenerateJournalEntriesStartDate',
'JournalEntriesLastProcessedDate',
'JournalEntriesLastQueuedDate')"));
				Assert(!Db.Connection.Exists($@"FROM dbo.AccGeneralLedgerData WHERE GLD_GC_Company = '{GlbCompany.CurrentCompany.PK.ToGuid()}'"));
				Assert(!Db.Connection.Exists($@"FROM dbo.AccTransactionPostingToGLDQueue WHERE APQ_GC_Company = '{GlbCompany.CurrentCompany.PK.ToGuid()}'"));
			}
		}

		public void TestButtonVisiblity()
		{
			string gS_LoginNameOldValue = GlbStaff.CurrentUser.GS_LoginName;

			try
			{
				GlbStaff.CurrentUser.GS_LoginName = "james bond";

				using (PeriodManagementForm pUserControl = new PeriodManagementForm())
				{
					AssertVisibilityOfControls(pUserControl, false);
				}

				GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;

				using (PeriodManagementForm pUserControl = new PeriodManagementForm())
				{
					AssertVisibilityOfControls(pUserControl, true);
				}
			}
			finally
			{
				GlbStaff.CurrentUser.GS_LoginName = gS_LoginNameOldValue;
			}
		}

		void AssertVisibilityOfControls(PeriodManagementForm form, bool shouldBeVisible)
		{
			AssertEquals("FinancialYearCalcEdit_ForTestOnly.Visible", true, form.FinancialYearCalcEdit_ForTestOnly.Visible);
			AssertEquals("CloseSubledgerButton_ForTestOnly.Visible", true, form.CloseSubledgerButton_ForTestOnly.Visible);
			AssertEquals("CloseGLButton_ForTestOnly.Visible", true, form.CloseGLButton_ForTestOnly.Visible);
			AssertEquals("SetUpNextYearButton_ForTestOnly.Visible", true, form.SetUpNextYearButton_ForTestOnly.Visible);
			AssertEquals("EditPeriodEndDateButton_ForTestOnly.Visible", true, form.EditPeriodEndDateButton_ForTestOnly.Visible);

			AssertEquals("ReSetPeriodsButton_ForTestOnly.Visible", shouldBeVisible, form.ReSetPeriodsButton_ForTestOnly.Visible);
			AssertEquals("ExpectedWipAcrAggRes_ForTestOnly.Visible", shouldBeVisible, form.ExpectedWipAcrAggRes_ForTestOnly.Visible);
			AssertEquals("ExpectedDebtorCreditorAggRes_ForTestOnly.Visible", shouldBeVisible, form.ExpectedDebtorCreditorAggRes_ForTestOnly.Visible);

			AssertEquals("ReaggregateAllCompaniesButton_ForTestOnly.Visible", shouldBeVisible, form.ReaggregateAllCompaniesButton_ForTestOnly.Visible);
			AssertEquals("ReaggregateThisCompanyButton_ForTestOnly.Visible", shouldBeVisible, form.ReaggregateThisCompanyButton_ForTestOnly.Visible);
			AssertEquals("SimulateButton_ForTestOnly.Visible", shouldBeVisible, form.SimulateButton_ForTestOnly.Visible);
			AssertEquals("ReportOnlyButton_ForTestOnly.Visible", shouldBeVisible, form.ReportOnlyButton_ForTestOnly.Visible);
			AssertEquals("RecoverPartOfGldPeriodsButton_ForTestOnly.Visible", shouldBeVisible, form.RecoverPartOfGldPeriodsButton_ForTestOnly.Visible);

			AssertEquals("PeriodExcludeCalcEdit_ForTestOnly.Visible", shouldBeVisible, form.PeriodExcludeCalcEdit_ForTestOnly.Visible);
			AssertEquals("ZLabel2_ForTestOnly.Visible", shouldBeVisible, form.ZLabel2_ForTestOnly.Visible);
			AssertEquals("ReverseToPeriodEdit_ForTestOnly.Visible", shouldBeVisible, form.ReverseToPeriodEdit_ForTestOnly.Visible);
			AssertEquals("PurgeGLDButton_ForTestOnly.Visible", shouldBeVisible, form.PurgeGLDButton_ForTestOnly.Visible);
		}

		[TestDate(1999, 7, 2)]
		public void TestPeriodManagementEndDateEditSecurity()
		{
			bool originalPeriodManagementEndDateEditAllowed = Env.Security.PeriodManagementEndDateEdit.IsAllowed;
			Env.Security.PeriodManagementEndDateEdit.IsAllowed = false;

			using (PeriodManagementForm pManagementForm = new PeriodManagementForm())
			{
				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 7, 1);
				testNewYear.EndDate = new ZDateTime(2000, 6, 30);

				PeriodManager testManager = new PeriodManager(Factory);
				pManagementForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				pManagementForm.EditPeriodEndDateButton_ForTestOnly.PerformClick();

				Assert("Last message shown is regarding security", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.PeriodManagementEndDateEdit.IsAllowed = true;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				pManagementForm.EditPeriodEndDateButton_ForTestOnly.PerformClick();
				Assert("Should show PeriodEndDateEditForm", ZFormModaliser.LastFormShownDialogForTest is PeriodEndDateEditForm);

				AssertNull("Message not shown", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.PeriodManagementEndDateEdit.IsAllowed = originalPeriodManagementEndDateEditAllowed;
		}

		[ExpectNoExceptions]
		public void TestNoExceptionThrown_WhenNoMoreSubLedgerToCloseForAdjustment()
		{
			using (PeriodManagementForm pManagementForm = new PeriodManagementForm())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 7, 1);
				testNewYear.EndDate = new ZDateTime(2000, 6, 30);

				PeriodManager testManager = new PeriodManager(Factory);
				pManagementForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);
				Factory.Save();

				while (testManager.NextUnClosedForAdjustmentsSubLedgerPeriod != null)
				{
					testManager.CloseSubLedgerPeriod();
					testManager.CloseGLPeriod();
					testManager.CloseGLPeriodForAdjustments();
				}

				AssertNull("All open sub ledgers are closed for adjustment", testManager.NextUnClosedForAdjustmentsSubLedgerPeriod);

				pManagementForm.CloseGLAdjustmentsButton_ForTestOnly.PerformClick();
			}
		}

		[ExpectNoExceptions()]
		public void TestSetUpNextYearButtonClickOK()
		{
			using (PeriodManagementForm pManagementForm = new PeriodManagementForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				pManagementForm.SetUpNextYearButton_ForTestOnly.PerformClick();
			}
		}

		[ExpectNoExceptions()]
		public void TestOpenPeriodEndDateEditFormNewYearSetting()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

			using (PeriodManagementForm pManagementForm = new PeriodManagementForm())
			{
				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 6, 1);
				testNewYear.EndDate = new ZDateTime(2000, 5, 31);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				pManagementForm.OpenPeriodEndDateEditFormNewYearSetting_ForTestOnly(testNewYear);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				pManagementForm.OpenPeriodEndDateEditFormNewYearSetting_ForTestOnly(testNewYear);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestPeriodManagementEndDateEditButtonClickOK()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

			using (PeriodManagementForm pManagementForm = new PeriodManagementForm())
			{
				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 6, 1);
				testNewYear.EndDate = new ZDateTime(2000, 5, 31);

				PeriodManager testManager = new PeriodManager(Factory);
				pManagementForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);
				ZInt financialYear = testManager.FinancialYear;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				pManagementForm.EditPeriodEndDateButton_ForTestOnly.PerformClick();

				AssertEquals("Financial year should remain the same", financialYear, testManager.FinancialYear);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestGenerateNewPeriod()
		{
			using (PeriodManagementForm testForm = new PeriodManagementForm())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(Env.Time.CurrentLocalDate.Year, Env.Time.CurrentLocalDate.Month, 1);
				testNewYear.EndDate = testNewYear.StartDate.AddMonths(12);
				PeriodManager testManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);

				Factory.Save();

				ZQuery periodFilter = new ZQuery(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.LessThan, testNewYear.StartDate);
				AccPeriodManagementCollection testPeriods = new AccPeriodManagementCollection(Factory, periodFilter);
				testPeriods.Load();

				AssertEquals("Should contain 12 accounting period objects", 12, testPeriods.Count);
				AssertEquals("AccPeriodManagement objects should belong to the current company", GlbCompany.CurrentCompany.PK, testPeriods[0].AM_GC_Company);
				Assert("Each AccPeriodManagement object should NOT have its subledger closed and GL closed", !(testPeriods[0].AM_IsGeneralLedgerClosed || testPeriods[0].AM_IsSubLedgerClosed));
				Assert("Each AccPeriodManagement object should NOT have its subledger closed and GL closed", !(testPeriods[1].AM_IsSubLedgerClosed || testPeriods[1].AM_IsGeneralLedgerClosed));

				NewYearPeriodSettings testNextYear = new NewYearPeriodSettings();
				testNextYear.StartDate = testForm.LastPeriod_ForTestOnly.AM_EndDate.AddDays(1).Date;
				testForm.GenerateNewPeriod_ForTestOnly(testNextYear, Factory, testManager);

				periodFilter = new ZQuery(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.LessThan, testNextYear.StartDate);
				testPeriods = new AccPeriodManagementCollection(Factory, periodFilter);
				testPeriods.Load();

				AssertEquals("Should contain 24 accounting periods", 24, testPeriods.Count);
			}
		}

		[TestDate(2023, 7, 2)]
		public void TestGenerateNewPeriod_WithAccountingBasedType()
		{
			using (PeriodManagementForm testForm = new PeriodManagementForm())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(2023, 7, 1);
				testNewYear.EndDate = testNewYear.StartDate.AddMonths(12);
				PeriodManager testManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);

				AssertEquals("Should Period's Year equals to EndDate's Year", testNewYear.EndDate.Year, testManager.Periods[0].AM_Year);

				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
				NewYearPeriodSettings testStartNewYear = new NewYearPeriodSettings();
				testStartNewYear.StartDate = new ZDateTime(2024, 7, 1);
				testStartNewYear.EndDate = testStartNewYear.StartDate.AddMonths(12);
				testStartNewYear.AccountingYearBasedType = NewYearPeriodSettings.AccountingYearBaseTypes.StartDateCalendarYear;
				PeriodManager testStartManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testStartNewYear, Factory, testStartManager);

				AssertEquals("Should Period's Year equals to StartDate's Year", testStartNewYear.StartDate.Year, testStartManager.Periods[0].AM_Year);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestNewPeriodYearIdentifierIsAlwaysLastPeriodYearIdentifierPlusOne()
		{
			using (PeriodManagementForm testForm = new PeriodManagementForm())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 1, 1);
				testNewYear.EndDate = new ZDateTime(1999, 12, 31);
				PeriodManager testManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);

				AssertEquals("Should be 1999", (short)1999, testManager.FinancialYear);
				AssertEquals("Should be 1999", (short)1999, testManager.Periods[testManager.Periods.Count - 1].AM_Year);

				Factory.Save();

				NewYearPeriodSettings testNextYear = new NewYearPeriodSettings();
				testNextYear.StartDate = new ZDateTime(2000, 1, 5);
				testNextYear.EndDate = new ZDateTime(2001, 1, 5);
				testManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testNextYear, Factory, testManager);

				AssertEquals("Should be 2000", (short)2000, testManager.FinancialYear);
				AssertEquals("Should be 2000", (short)2000, testManager.Periods[testManager.Periods.Count - 1].AM_Year);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestNewPeriodFinancialYear()
		{
			using (PeriodManagementForm testForm = new PeriodManagementForm())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 1, 1);
				testNewYear.EndDate = new ZDateTime(1999, 12, 31);
				PeriodManager testManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);

				AssertEquals("Should be 1999", (short)1999, testManager.FinancialYear);
				AssertEquals("Should be 1999", (short)1999, testManager.Periods[testManager.Periods.Count - 1].AM_Year);

				Factory.Save();

				NewYearPeriodSettings testNextYear = new NewYearPeriodSettings();
				testNextYear.StartDate = new ZDateTime(2000, 1, 1);
				testNextYear.EndDate = new ZDateTime(2000, 12, 31);
				testManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testNextYear, Factory, testManager);

				AssertEquals("Should be 2000", (short)2000, testManager.FinancialYear);
				AssertEquals("Should be 2000", (short)2000, testManager.Periods[testManager.Periods.Count - 1].AM_Year);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestCurrentPeriodNameWhenStartEndDatesInTheSameYear()
		{
			using (PeriodManagementForm testForm = new PeriodManagementForm())
			{
				var testFactory = new BusinessObjectFactory();
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 1, 3);
				testNewYear.EndDate = new ZDateTime(1999, 12, 30);
				PeriodManager testManager = new PeriodManager(testFactory);
				testForm.GenerateNewPeriod_ForTestOnly(testNewYear, testFactory, testManager);

				AssertEquals("TestManager.FinancialYear", (short)1999, testManager.FinancialYear);
				AssertEquals("Last period year", (short)1999, testManager.Periods[testManager.Periods.Count - 1].AM_Year);
				AssertEquals("First period StartDate", new ZDateTime(1999, 1, 3), testManager.Periods[0].AM_StartDate);
				AssertEquals("Last period EndDate", new ZDateTime(1999, 12, 30, 23, 59, 00), testManager.Periods[testManager.Periods.Count - 1].AM_EndDate);

				testManager.FinancialYear--;
				AssertEquals("TestManager.FinancialYear", (short)1998, testManager.FinancialYear);
				AssertEquals("Last period year", (short)1998, testManager.Periods[testManager.Periods.Count - 1].AM_Year);
				AssertEquals("First period StartDate", new ZDateTime(1998, 1, 3), testManager.Periods[0].AM_StartDate);
				AssertEquals("Last period EndDate", new ZDateTime(1999, 1, 2, 23, 59, 00), testManager.Periods[testManager.Periods.Count - 1].AM_EndDate);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestSubledgerClosingWarningMessage()
		{
			using (PeriodManagementForm testForm = new PeriodManagementForm())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 6, 1);
				testNewYear.EndDate = new ZDateTime(2000, 5, 31);

				PeriodManager testManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);
				Factory.Save();

				testForm.CloseSubledgerButton_ForTestOnly.PerformClick();

				AssertEquals("Sub-ledger period closing warning message", "Are you sure you want to close Sub Ledger period "
					+ testManager.NextUnClosedSubLedgerPeriod.AM_Period + "?"
					+ System.Environment.NewLine
					+ "Please note that once a period is closed, it cannot be reopened.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestGLClosingWarningMessage()
		{
			using (PeriodManagementForm testForm = new PeriodManagementForm())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 6, 1);
				testNewYear.EndDate = new ZDateTime(2000, 5, 31);

				PeriodManager testManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);
				Factory.Save();

				testForm.CloseGLButton_ForTestOnly.PerformClick();

				AssertEquals("GL period closing warning message", "Are you sure you want to close General Ledger period "
					+ testManager.NextUnClosedGLPeriod.AM_Period + "?"
					+ System.Environment.NewLine
					+ "Please note that once a period is closed, it cannot be reopened.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestSubledgerClosingWarningMessageYes()
		{
			using (PeriodManagementForm testForm = new PeriodManagementForm())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 6, 1);
				testNewYear.EndDate = new ZDateTime(2000, 5, 31);

				PeriodManager testManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testForm.CloseSubledgerButton_ForTestOnly.PerformClick();

				ZQuery filter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
				filter.ReLoadExistingRows = true;
				filter.OrderBy = AccPeriodManagementSchema.Constants.AM_EndDate + " ASC";
				AccPeriodManagement period = Factory.LoadTop1<AccPeriodManagement>(filter);

				AssertEquals("Yes message", "Sub-ledger period " + period.AM_Period + " closed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestGLClosingWarningMessageYes()
		{
			using (PeriodManagementForm testForm = new PeriodManagementForm())
			{
				TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);

				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 6, 1);
				testNewYear.EndDate = new ZDateTime(2000, 5, 31);

				PeriodManager testManager = new PeriodManager(Factory);
				testForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);

				AccPeriodManagement period = testManager.CloseSubLedgerPeriod();
				if (period != null)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					testForm.CloseGLButton_ForTestOnly.PerformClick();

					AssertEquals("Yes message", "General Ledger period " + period.AM_Period + " closed.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestGLCloseForAdjustmentsSecurity()
		{
			bool periodManagementCloseGLPeriodForAdjustmentsAllowed = Env.Security.PeriodManagementCloseGLPeriodForAdjustments.IsAllowed;
			Env.Security.PeriodManagementCloseGLPeriodForAdjustments.IsAllowed = false;

			using (PeriodManagementForm pManagementForm = new PeriodManagementForm())
			{
				NewYearPeriodSettings testNewYear = new NewYearPeriodSettings();
				testNewYear.StartDate = new ZDateTime(1999, 7, 1);
				testNewYear.EndDate = new ZDateTime(2000, 6, 30);

				PeriodManager testManager = new PeriodManager(Factory);
				pManagementForm.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				pManagementForm.CloseGLAdjustmentsButton_ForTestOnly.PerformClick();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				Assert("Last message shown is regarding security", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.PeriodManagementCloseGLPeriodForAdjustments.IsAllowed = true;

				pManagementForm.CloseSubledgerButton_ForTestOnly.PerformClick();
				pManagementForm.CloseGLButton_ForTestOnly.PerformClick();

				AccPeriodManagement period = testManager.CloseGLPeriodForAdjustments();
				period.AM_IsGeneralLedgerClosed = true;
				period.AM_IsSubLedgerClosed = true;

				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				pManagementForm.CloseGLAdjustmentsButton_ForTestOnly.PerformClick();

				AssertEquals("User is now allowed to close period", true, Env.Security.PeriodManagementCloseGLPeriodForAdjustments.IsAllowed);
				AssertEquals("Yes message", "General Ledger period " + period.AM_Period + " closed for adjustments.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			Env.Security.PeriodManagementCloseGLPeriodForAdjustments.IsAllowed = periodManagementCloseGLPeriodForAdjustmentsAllowed;
		}

		public void TestRecoverPartOfGldPeriodsButton()
		{
			using (var formForTest = new PeriodManagementForm())
			{
				ZFormModaliser.LastFormShownDialogForTest = null;

				formForTest.RecoverPartOfGldPeriodsButton_ForTestOnly.PerformClick();
				AssertNotNull("PreCondition", ZFormModaliser.LastFormShownDialogForTest);
				AssertType<RecoverPartOfGldPeriodsDateRangeForm>(ZFormModaliser.LastFormShownDialogForTest);

				ZFormModaliser.LastFormShownDialogForTest.Close();
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestShowGLPeriodFormAsModalForm()
		{
			using (var testMainForm = new MainForm())
			{
				testMainForm.Show();
				Application.DoEvents();

				var containersMainFormModule = new MainFormModule(ModuleIDs.PeriodManagement);
				testMainForm.OpenModule(containersMainFormModule, false);

				using (var hostForm = new ZForm())
				using (var formForTest = new PeriodManagementForm())
				{
					var testNewYear = new NewYearPeriodSettings();
					testNewYear.StartDate = new ZDateTime(1999, 7, 1);
					testNewYear.EndDate = new ZDateTime(2000, 6, 30);

					var testManager = new PeriodManager(Factory);
					formForTest.GenerateNewPeriod_ForTestOnly(testNewYear, Factory, testManager);
					Factory.Save();

					hostForm.Controls.Add(formForTest);
					hostForm.Show();

					formForTest.PeriodManager_ForTestOnly.FinancialYear = 1999;
					formForTest.PeriodsGrid_ForTestOnly.Select(0);
					Application.DoEvents();
					formForTest.PeriodsGrid_ForTestOnly.PerformDoubleClickForTest();

					using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
					{
						AssertNotNull(lastShownForm);
						AssertEquals(typeof(GlPeriodForm), lastShownForm.GetType());
					}
				}
			}
		}

		public void TestHandleGLDComplianceReport_WhenPurgeGLDButtonClick()
		{
			var generalLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			generalLedgerData.GLD_Type = "PST";
			generalLedgerData.GLD_GLAccountType = "ARC";
			generalLedgerData.GLD_PostDate = ZDateTime.Today;
			generalLedgerData.GLD_PostPeriod = 1;
			generalLedgerData.GLD_GC_Company = GlbCompany.CurrentCompany.PK;
			generalLedgerData.GLD_GB_Branch = GlbBranch.CurrentBranch.PK;
			generalLedgerData.GLD_GE_Department = GlbDepartment.CurrentDepartment.PK;

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var gldReportConfig = complianceConfig.AddNew();
			gldReportConfig.ReportCode = "TST";
			gldReportConfig.ReportTitle = "Test Tax Report";
			gldReportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.GeneralLedgerData;
			gldReportConfig.ReportPeriodicity = ReportPeriodicityCodes.AccountingPeriod;
			gldReportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gldReportConfig.TaxRegistrationType = "ABN";
			gldReportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBookWithoutGrouping;

			var ahReportConfig = complianceConfig.AddNew();
			ahReportConfig.ReportCode = "STA";
			ahReportConfig.ReportTitle = "Test Tax Report";
			ahReportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
			ahReportConfig.ReportPeriodicity = ReportPeriodicityCodes.AccountingPeriod;
			ahReportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ahReportConfig.TaxRegistrationType = "ABN";
			ahReportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBookWithoutGrouping;

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceConfig);

			var ahComplianceReportFIN = Factory.NewWithValidTestData<AccComplianceReport>();
			ahComplianceReportFIN.ACR_ReportType = ahReportConfig.ReportCode;
			ahComplianceReportFIN.ACR_DateFrom = ZDate.Today.AddDays(-2);
			ahComplianceReportFIN.ACR_DateTo = ZDate.Today.AddDays(2);
			ahComplianceReportFIN.ACR_Status = AccComplianceReport.Status.ReportFinalised;

			var complianceReportGEN = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReportGEN.ACR_ReportType = gldReportConfig.ReportCode;
			complianceReportGEN.ACR_DateFrom = ZDate.Today.AddDays(-2);
			complianceReportGEN.ACR_DateTo = ZDate.Today.AddDays(2);
			complianceReportGEN.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			Factory.Save();

			var sql = $@"INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
(NEWID(), '{generalLedgerData.PK}', 'GLD', '{GlbCompany.CurrentCompany.PK}', '{complianceReportGEN.PK}', 1),
(NEWID(), NEWID(), 'AH', '{GlbCompany.CurrentCompany.PK}', '{ahComplianceReportFIN.PK}', 2)";

			TestConnection.ExecuteNonQuery(sql);

			using (var form = new PeriodManagementForm())
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PurgeGLDButton_ForTestOnly.PerformClick();
				var expectedMessage = @"All existing General Ledger Data (GLD) records have been deleted and all related GLD configurations have been cleared.

However, new GLD records may be inserted as a result of queued records processed by GLD Service Tasks before or during the delete operation.

Thus, please check that no new GLD records can be found in Manage > General Ledger > Accounting Journals module.

If there are, please run the ""Clear GLD configurations and Data"" function again.";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!Db.Connection.Exists($@"FROM dbo.AccGeneralLedgerData WHERE GLD_GC_Company = '{GlbCompany.CurrentCompany.PK.ToGuid()}'"));
				Assert(!Db.Connection.Exists($@"FROM dbo.AccTransactionPostingToGLDQueue WHERE APQ_GC_Company = '{GlbCompany.CurrentCompany.PK.ToGuid()}'"));

				DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM AccComplianceReportTransactionPivot");
				AssertEquals(1, result.Rows.Count);
				AssertEquals(ahReportConfig.ReportBaseTablePrefix, result.Rows[0]["ACL_ParentTableCode"]);

				var complianceReportFIN = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReportFIN.ACR_ReportType = gldReportConfig.ReportCode;
				complianceReportFIN.ACR_DateFrom = ZDate.Today.AddDays(-2);
				complianceReportFIN.ACR_DateTo = ZDate.Today.AddDays(2);
				complianceReportFIN.ACR_Status = AccComplianceReport.Status.ReportFinalised;

				Factory.Save();

				sql = $@"INSERT INTO AccComplianceReportTransactionPivot (ACL_PK, ACL_ParentID, ACL_ParentTableCode, ACL_GC_Company, ACL_ACR_Report, ACL_ReportSequence) VALUES
(NEWID(), '{generalLedgerData.PK}', 'GLD', '{GlbCompany.CurrentCompany.PK}', '{complianceReportFIN.PK}', 3)";

				TestConnection.ExecuteNonQuery(sql);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.PurgeGLDButton_ForTestOnly.PerformClick();

				expectedMessage = "You are going to change the status of Compliance Report that uses the GLD data from \"FIN - Report Finalised\" to \"INV - Report Invalidated by Transactions Updated\"";

				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMessage));

				var newFactory = Factory.CreateNewFactory();
				var complianceReports = newFactory.Load<AccComplianceReport>(new ZQuery());
				AssertEquals(false, complianceReports.Any(x => x.ACR_ReportType == gldReportConfig.ReportCode && x.ACR_Status == AccComplianceReport.Status.ReportFinalised));

				result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM AccComplianceReportTransactionPivot");
				AssertEquals(1, result.Rows.Count);
				AssertEquals(ahReportConfig.ReportBaseTablePrefix, result.Rows[0]["ACL_ParentTableCode"]);
			}
		}

		#region Helpers

		void AssertReAggregateAllCompaniesCallsReAggregatorWithMessage(DialogResult response, Times expectedTimes)
		{
			var mockReAggregator = new Mock<IReAggregator>();
			ObjectFactory.Substitute(mockReAggregator.Object);

			using (var formForTest = new PeriodManagementForm())
			{
				formForTest.Show();

				UnitTestUserNotification.Instance.AddAnswer(response);
				formForTest.GetControl<ZButton>("ReaggregateAllCompaniesButton").PerformClick();

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
						@"Are you sure you want to re-aggregate transactions for all companies?

Please ensure that 'Compact General Ledger Aggregate Service Task' is disabled before running this."));

				mockReAggregator.Verify(r => r.ReAggregate(), expectedTimes);
			}
		}

		void AssertReAggregateThisCompanyCallsReAggregatorWithMessage(DialogResult response, Times expectedTimes)
		{
			var mockSingleCompanyReAggregator = new Mock<ISingleCompanyReAggregator>();
			ObjectFactory.Substitute(mockSingleCompanyReAggregator.Object);

			using (var formForTest = new PeriodManagementForm())
			{
				formForTest.Show();

				UnitTestUserNotification.Instance.AddAnswer(response);
				formForTest.GetControl<ZButton>("ReaggregateThisCompanyButton").PerformClick();

				Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(
						@"Are you sure you want to re-aggregate transactions for this company?

Please ensure that 'Compact General Ledger Aggregate Service Task' is disabled before running this."));

				mockSingleCompanyReAggregator.Verify(r => r.ReAggregate(), expectedTimes);
			}
		}

		#endregion
	}
}
