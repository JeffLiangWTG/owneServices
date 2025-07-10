using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.Registry.Business.AccountingConfigurationRegistry;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.Testing
{
	public abstract class NewMatchGroupFormTest<T> : TestCaseWithFactory where T : IBusinessObjectCollection
	{
		public class NewMatchGroupFormForTest : NewMatchGroupForm
		{
			public NewMatchGroupFormForTest(MatchingBase matchingBizO)
				: base(matchingBizO)
			{
			}

			public IZForm TestJournalBaseForm;

			protected override IZForm HandleEditARAPJournal(ZGrid grid)
			{
				TestJournalBaseForm = base.HandleEditARAPJournal(grid);
				return TestJournalBaseForm;
			}

			public bool IsTransactionsTabSelected_ForTestOnly => IsTransactionsTabSelected;

			public bool IsCashAdvanceTabSelected_ForTestOnly => IsCashAdvanceTabSelected;

			public bool IsAPJournalsTabPageSelected_ForTestOnly => IsAPJournalsTabPageSelected;

			public bool IsARJournalsTabPageSelected_ForTestOnly => IsARJournalsTabPageSelected;

			public void UnmatchedCashAdvanceRequestsGrid_DoubleClick_ForTestOnnly() => UnmatchedCashAdvanceRequestsGrid_DoubleClick(null, null);
		}

		#region abstract Method and Properties

		protected abstract T BalancingJournals { get; }

		protected abstract ZGrid JournalsGrid { get; }

		protected abstract ZString BindGridName { get; }

		protected abstract ZTabPage JournalsTabPage { get; }

		protected abstract Type JournalBaseFormBizoType { get; }

		protected abstract void JournalsGridMenu_Popup();

		protected abstract NewMatchGroupFormForTest GetForm();

		protected abstract BooleanRegistryItem EnableCashAdvanceFunctionalityRegistry { get; }

		protected abstract BooleanRegistryItem AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry { get; }

		protected abstract AccountingRegistryItem CashAdvanceClearingAccount { get; }

		protected abstract OrgHeader CashAdvanceOrganizationForTest { get; }

		protected abstract OrgHeader CashAdvanceSettlementOrgForTest { get; }

		protected abstract string CashAdvanceLedgerType { get; }

		#endregion

		#region Properties

		MenuItem MenuItem => JournalsGrid.ContextMenu.MenuItems.FindByText("Edit", true);

		protected MatchingBase TestMatchingBase;
		protected NewMatchGroupFormForTest TestNewMatchGroupForm;

		#endregion

		#region Test Menu Item

		public void TestJournalsGridContextMenuWithMultipleSubAccounts()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());

			using (Form form = GetForm())
			{
				form.Show();
				form.GetControl<ZTemplateTabControl>("MatchingTabControl").SelectedTab = JournalsTabPage;

				AssertEquals(0, JournalsGrid.SelectedElements.Length);
				JournalsGridMenu_Popup();
				Assert("Should not contain 'Edit' menu item", !JournalsGrid.ContextMenu.MenuItems.Contains(MenuItem));

				BalancingJournals.AddNew();
				(BalancingJournals[0] as Journal).AH_OSExTaxAmount = 1;
				JournalsGrid.SelectAllElements();
				AssertEquals(1, JournalsGrid.SelectedElements.Length);
				JournalsGridMenu_Popup();
				Assert("Should contain 'Edit' menu item", JournalsGrid.ContextMenu.MenuItems.Contains(MenuItem));
				AssertEquals("'Edit' menu item index", 0, JournalsGrid.ContextMenu.MenuItems.IndexOf(MenuItem));
				MenuItem.PerformClick();
				Assert(TestNewMatchGroupForm.TestJournalBaseForm is BalancingJournalForm);
				AssertEquals(ODisplayMode.Edit, TestNewMatchGroupForm.TestJournalBaseForm.DisplayMode);
				AssertType(JournalBaseFormBizoType, TestNewMatchGroupForm.TestJournalBaseForm.BusinessEntityForPersistingForm);
				var control = ((Control)TestNewMatchGroupForm.TestJournalBaseForm).GetControl<ZPostingButtonsUserControl>("ButtonsUserControl");
				Assert(control.CloseButton.Enabled);
				AssertEquals("Close", control.CloseButton.Text);
				Assert(!control.SaveAndCloseButton.Enabled);
				Assert(!control.SaveAndCloseButton.Visible);
				Assert(!control.SaveButton.Enabled);
				Assert(!control.SaveButton.Visible);

				((Journal)BalancingJournals[0]).AH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
				control.CloseButton.PerformClick();

				BalancingJournals.AddNew();
				JournalsGrid.SelectAllElements();
				AssertEquals(2, JournalsGrid.SelectedElements.Length);
				JournalsGridMenu_Popup();
				Assert("Should not contain 'Edit' menu item", !JournalsGrid.ContextMenu.MenuItems.Contains(MenuItem));
				JournalsGrid.ContextMenu.Dispose();
			}
		}

		#endregion

		#region Test Save As Draft

		public void TestSaveAsDraftButtonWillNotShow()
		{
			using (NewMatchGroupForm form = GetForm())
			{
				form.Show();
				Application.DoEvents();

				//"Save As Draft" Button disabled and not Visible
				var saveAsDraftButton = form.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				AssertNotNull(saveAsDraftButton);
				Assert(!saveAsDraftButton.Enabled);
				Assert(!saveAsDraftButton.Visible);
			}
		}

		public void TestSaveAsDraftButtonWillShow_PaymentApprovalWithoutAuthorisation()
		{
			var payment = GetPaymentApprovalWithoutAuthorisation();
			using (var form = new PaymentApprovalForm(payment))
			{
				form.Show();
				Application.DoEvents();

				var paymentDetailButton = form.Controls.Find("PaymentDetailButton", true)[0] as ZButton;
				AssertNotNull(paymentDetailButton);
				paymentDetailButton.PerformClick();
				Application.DoEvents();

				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);

				//"Save As Draft" Button disabled and not Visible
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				AssertNotNull(saveAsDraftButton);
				Assert(saveAsDraftButton.Visible);
				Assert(saveAsDraftButton.Enabled);
			}
		}

		public void TestSaveAsDraftButtonWillShow_PaymentApprovalWithAuthorisation_NotInDB()
		{
			var approval = GetPaymentApprovalWithAuthorisation();
			AssertEquals(false, approval.IsInDatabase);

			//Not in DB, will have SaveAsDraftButton
			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				//Click Save And Close, will show match Form
				form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.PerformClick();
				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);

				//Match form have "Save As Draft" Button
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				AssertNotNull(saveAsDraftButton);
				AssertEquals(true, saveAsDraftButton.Visible);
				AssertEquals(true, saveAsDraftButton.Enabled);
			}
		}

		public void TestSaveAsDraftButtonWillShow_PaymentApprovalWithAuthorisation_DraftApproval_InDB()
		{
			var approval = GetPaymentApprovalWithAuthorisation();
			approval.AV_Status = PaymentApprovalStatus.Draft;
			Factory.Save();
			AssertEquals(true, approval.IsInDatabase);
			AssertEquals(PaymentApprovalStatus.Draft, approval.AV_Status);

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				approval.AV_Amount = 95m;

				//Click Save And Close, will show match Form
				form.PostingButtonsUserControl_ForTestOnly.SaveAndCloseButton.PerformClick();
				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);

				//Match form have "Save As Draft" Button
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				AssertNotNull(saveAsDraftButton);
				AssertEquals(true, saveAsDraftButton.Visible);
				AssertEquals(true, saveAsDraftButton.Enabled);
			}
		}

		public void TestSaveAsDraftButtonWillShow_PaymentApprovalWithAuthorisation_NotDraftApproval_InDB()
		{
			var approval = GetPaymentApprovalWithAuthorisation();
			Factory.Save();
			AssertEquals(true, approval.IsInDatabase);
			AssertEquals(PaymentApprovalStatus.FullyApproved, approval.AV_Status);

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				//Click Save And Close, will show match Form
				form.PaymentDetailButton_ForTestOnly.PerformClick();
				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);

				//Match form have "Save As Draft" Button
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				AssertNotNull(saveAsDraftButton);
				AssertEquals(true, saveAsDraftButton.Visible);
				AssertEquals(true, saveAsDraftButton.Enabled);
			}
		}

		public void TestCanSaveAsDraftWhenBalanceIsNotZero_InDB()
		{
			AssertCanSaveAsDraftWhenBalanceIsNotZero(true);
		}

		public void TestCanSaveAsDraftWhenBalanceIsNotZero_NotInDB()
		{
			AssertCanSaveAsDraftWhenBalanceIsNotZero(false);
		}

		public void AssertCanSaveAsDraftWhenBalanceIsNotZero(bool isInDB)
		{
			var approval = GetPaymentApprovalWithAuthorisation();
			if (isInDB)
			{
				approval.AV_Status = PaymentApprovalStatus.Draft;
				Factory.Save();
			}
			AssertEquals(isInDB, approval.IsInDatabase);
			AssertEquals(isInDB ? PaymentApprovalStatus.Draft : PaymentApprovalStatus.FullyApproved, approval.AV_Status);

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				//Click Save And Close, will show match Form
				form.PaymentDetailButton_ForTestOnly.PerformClick();
				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);

				//Match form have "Save As Draft" Button
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				AssertNotNull(saveAsDraftButton);
				AssertEquals(true, saveAsDraftButton.Visible);
				AssertEquals(true, saveAsDraftButton.Enabled);
				AssertNotEquals("Pre-condition: The balance should not equal 0.", 0m, approval.MatchingBaseObject.Balance);
				saveAsDraftButton.PerformClick();
				AssertEquals("Should not show any message as saved successfully.", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The status should be 'DFT' as save as draft successfully.", PaymentApprovalStatus.Draft, approval.AV_Status);
				AssertEquals("Should save successfully even the balance is NOT zero as save as draft.", true, approval.IsInDatabase);
			}
		}

		public void TestShowMessageWhenMatchedTransactionCollectionChangedInDB()
		{
			var approval = GetPaymentApprovalWithAuthorisation();
			approval.AV_Status = PaymentApprovalStatus.Draft;
			Factory.Save();
			AssertEquals(true, approval.IsInDatabase);
			AssertEquals(PaymentApprovalStatus.Draft, approval.AV_Status);

			using (PaymentApprovalWithAuthorisationForm form = new PaymentApprovalWithAuthorisationForm(approval))
			{
				form.Show();
				Application.DoEvents();

				approval.AV_Amount = 95m;

				//Click Save And Close, will show match Form
				form.PaymentDetailButton_ForTestOnly.PerformClick();
				AssertType<NewMatchGroupForm>(ZFormModaliser.LastFormShownForTest);

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				newFactory.SetContext(BusinessContext.SavingPaymentApprovalAsDraft);
				var creator = new TestObjectCreator(newFactory);
				var invoice1 = creator.CreateInvoiceWithLine(typeof(ARInvoice), "123456789", creator.USD, 2, 100M, 20, 200, 40);
				invoice1.AH_OH = approval.AV_OH;
				newFactory.Save();

				InvoicingBaseCollection invoices = new InvoicingBaseCollection(newFactory);
				invoices.AddRange(new BusinessObject[] { invoice1 });
				var reloadApproval = newFactory.Load<PaymentApprovalWithAuthorisation>(approval.PK);
				reloadApproval.PaymentMatchingBaseObject.MoveFromUnmatchToMatch(invoices.ToArray());
				reloadApproval.PaymentMatchingBaseObject.MatchAndClearTransactions();
				newFactory.Save();

				//Match form have "Save As Draft" Button
				var newMatchGroupForm = ZFormModaliser.LastFormShownForTest as NewMatchGroupForm;
				var saveAsDraftButton = newMatchGroupForm.Controls.Find("SaveAsDraftButton", true)[0] as ZButton;
				AssertNotNull(saveAsDraftButton);
				AssertEquals(true, saveAsDraftButton.Visible);
				AssertEquals(true, saveAsDraftButton.Enabled);

				saveAsDraftButton.PerformClick();
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("The matched transactions were already changed by another user. Please reopen this form."));
			}
		}

		public abstract PaymentApprovalWithoutAuthorisation GetPaymentApprovalWithoutAuthorisation();

		public abstract PaymentApprovalWithAuthorisation GetPaymentApprovalWithAuthorisation();

		#endregion

		#region Test Sub Account Column

		public void TestSubAccountColumnsWithMultipleSubAccounts()
		{
			using (Form form = GetForm())
			{
				form.Show();
				form.GetControl<ZTemplateTabControl>("MatchingTabControl").SelectedTab = JournalsTabPage;

				Assert(JournalsGrid.Columns.Contains(Journal.Schema.AH_Calc_FirstSubClassParent));
				Assert(JournalsGrid.Columns.Contains(Journal.Schema.AH_Calc_FirstSubClassParentId));
				Assert(JournalsGrid.Columns.Contains(Journal.Schema.AH_Calc_SecondSubClassParent));
				Assert(JournalsGrid.Columns.Contains(Journal.Schema.AH_Calc_SecondSubClassParentId));

				AssertEquals("AH_Calc_FirstSubClassParent caption", "Sub Account 1 Type", JournalsGrid.GetColumnCaption(Journal.Schema.AH_Calc_FirstSubClassParent));
				AssertEquals("AH_Calc_FirstSubClassParentId caption", "Sub Account 1", JournalsGrid.GetColumnCaption(Journal.Schema.AH_Calc_FirstSubClassParentId));
				AssertEquals("AH_Calc_SecondSubClassParent caption", "Sub Account 2 Type", JournalsGrid.GetColumnCaption(Journal.Schema.AH_Calc_SecondSubClassParent));
				AssertEquals("AH_Calc_SecondSubClassParentId caption", "Sub Account 2", JournalsGrid.GetColumnCaption(Journal.Schema.AH_Calc_SecondSubClassParentId));
				AssertEquals("AH_OSExTaxAmount caption", "Amount", JournalsGrid.GetColumnCaption("AH_OSExTaxAmount"));
			}
		}

		#endregion

		#region Test Notifications about SubAccounts

		public void TestSubAccountNotificationsWithMultipleSubAccounts()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateGLHeaderSubAccount(testObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, true);
			testObjectCreator.CreateGLHeaderSubAccount(testObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, true);

			AccountingConfigurationRegistry.Instance.ARJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APJournalAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());

			using (Form form = GetForm())
			{
				form.Show();
				form.GetControl<ZTemplateTabControl>("MatchingTabControl").SelectedTab = JournalsTabPage;

				BalancingJournals.AddNew();
				var balancingJournal = (Journal)BalancingJournals[0];
				AssertEquals(2, balancingJournal.SubAccounts.Count);
				Assert(!balancingJournal.NotificationsIncludingChildren.Contains("Error - AH_AG: This GL account has 3 sub account types. Some or all are mandatory. You can specify the additional sub accounts by selecting the journal, then click on the Edit menu."));

				testObjectCreator.CreateGLHeaderSubAccount(testObjectCreator.GLHeader1, GlbStaffSchema.Constants.Prefix, true);
				balancingJournal.AH_AG = testObjectCreator.GLHeader1.PK;
				balancingJournal.Validation.ValidateAH_AG();
				AssertEquals(3, balancingJournal.SubAccounts.Count);
				Assert(balancingJournal.EnableCheckSubAccountsForGLHeader);
				Assert(balancingJournal.NotificationsIncludingChildren.Contains("Error - AH_AG: This GL account has 3 sub account types. Some or all are mandatory. You can specify the additional sub accounts by selecting the journal, then click on the Edit menu."));

				JournalsGrid.SelectAllElements();
				JournalsGridMenu_Popup();
				MenuItem.PerformClick();
				TestNewMatchGroupForm.TestJournalBaseForm.BusinessEntityForPersistingForm.RunPreSaveValidation();
				Assert(!balancingJournal.EnableCheckSubAccountsForGLHeader);
				Assert(!TestNewMatchGroupForm.TestJournalBaseForm.BusinessEntityForPersistingForm.Notifications.Contains("Error - AH_AG: This GL account has 3 sub account types. Some or all are mandatory. You can specify the additional sub accounts by selecting the journal, then click on the Edit menu."));

				var journalSubAccounts = ((Journal)TestNewMatchGroupForm.TestJournalBaseForm.BusinessEntityForPersistingForm).SubAccounts;
				AssertEquals(3, journalSubAccounts.Count);
				Assert(journalSubAccounts[0].Notifications.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));
				Assert(journalSubAccounts[1].Notifications.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));
				Assert(journalSubAccounts[2].Notifications.Contains("Error - AHS_SubClassParentId: Please enter a Sub Account."));
			}
		}

		#endregion

		public void TestAgreedPaymentMethodColumns()
		{
			const string apmField = Journal.Schema.AH_AgreedPaymentMethodOverride;

			using (Form form = GetForm())
			{
				form.Show();
				form.GetControl<ZTemplateTabControl>("MatchingTabControl").SelectedTab = JournalsTabPage;

				Assert(JournalsGrid.Columns.Contains(apmField));
				AssertEquals("Agreed Payment Method", JournalsGrid.GetColumnCaption(apmField));
				var apmStyle = JournalsGrid.GetColumnStyle(apmField) as ZDropEditColumnStyleInfo;
				AssertNotNull(apmStyle);
				Assert(apmStyle.IsVisible);
			}
		}

		public void TestShowGLAccountsForImportAction()
		{
			var creator = new TestObjectCreator(Factory);
			var chart = creator.CreateAlternateChart("TRR");
			Factory.Save();

			var alternateAccount = creator.CreateAccAlternateGlAccount(chart.PK, "111", Core.Constants.AccountType.BalanceSheetAccount);
			var glHeader = creator.CreateGLHeader("3333.33.33");
			var glHeader2 = creator.CreateGLHeader("4444.33.33");
			creator.CreateAccAlternateGlAccountAttribute(alternateAccount, glHeader.PK);
			creator.CreateAccAlternateGlAccountAttribute(alternateAccount, glHeader2.PK);
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid()))
			using (var form = GetForm())
			{
				form.Show();
				var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
				AssertNotNull(TestMatchingBase.BalancingARJournals.ShowGLAccountsForImportAction);
				AssertNotNull(TestMatchingBase.BalancingAPJournals.ShowGLAccountsForImportAction);

				tabControl.SelectedTab = JournalsTabPage;
				JournalsGrid.SetDataBinding(TestMatchingBase, BindGridName);
				Application.DoEvents();
				var arColumnStyle = JournalsGrid.Columns["AH_AG"].ColumnStyle;
				JournalsGrid.BeginEdit(arColumnStyle, 0);
				var codeBox = ((ZGridGuidFindBox)JournalsGrid.LastFocusedColumn.EditControl).CodeBox;
				codeBox.Text = "111";
				form.Controls.Find("PrimaryOrgGuidFindBox", true)[0].Focus();
				Application.DoEvents();
				var selectionForm = (GLAccountSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertNotNull(selectionForm);
			}
		}

		public void TestCashAdvanceFilterSearch()
		{
			if (!CanCashAdvanceRequestBeMatched)
			{
				Assert(true);
				return;
			}

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(DateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, CashAdvanceOrganizationForTest, CashAdvanceLedgerType, 200M, 200M, "AUD");
			Factory.Save();

			using (EnableCashAdvanceFunctionalityRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (NewMatchGroupFormForTest form = GetForm())
				{
					form.Show();

					var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
					var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "CashAdvanceTabPage") as ZTabPage;
					AssertNotNull(tabPage);

					var filterControl = tabPage.GetControl<AccountingOnFormFilterControl>("CashAdvanceFilterControl");
					AssertNotNull(filterControl);

					var bizO = (form.BusinessEntity as MatchingBase);
					bizO.PrimaryOrganization = CashAdvanceOrganizationForTest.PK;
					AssertEquals("Item Count", 1, bizO.UnmatchedCashAdvanceRequests.Count);

					var currenyFilter = filterControl.FilterBusinessObject["Currency"] as ModuleNkFilter;
					currenyFilter.Property = Core.Constants.CurrencyCodes.Philippines;
					currenyFilter.IsActive = true;
					AssertNotNull(currenyFilter);

					filterControl.FirePerformSearch();
					AssertEquals("Item Count", 0, bizO.UnmatchedCashAdvanceRequests.Count);

					currenyFilter.Property = Core.Constants.CurrencyCodes.Australia;
					filterControl.FirePerformSearch();
					AssertEquals("Item Count", 1, bizO.UnmatchedCashAdvanceRequests.Count);
				}
			}
		}

		public void TestCashAdvanceFilterShowsErrorIfPrimaryOrgIsMissing()
		{
			if (!CanCashAdvanceRequestBeMatched)
			{
				Assert(true);
				return;
			}

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(DateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, CashAdvanceOrganizationForTest, CashAdvanceLedgerType, 200M, 200M, "AUD");
			Factory.Save();

			using (EnableCashAdvanceFunctionalityRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (NewMatchGroupFormForTest form = GetForm())
				{
					form.Show();

					var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
					var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "CashAdvanceTabPage") as ZTabPage;
					AssertNotNull(tabPage);

					var filterControl = tabPage.GetControl<AccountingOnFormFilterControl>("CashAdvanceFilterControl");
					AssertNotNull(filterControl);

					var bizO = (form.BusinessEntity as MatchingBase);
					bizO.PrimaryOrganization = CashAdvanceOrganizationForTest.PK;
					AssertEquals("Item Count", 1, bizO.UnmatchedCashAdvanceRequests.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					bizO.PrimaryOrganization = ZGuid.Empty;

					var currenyFilter = filterControl.FilterBusinessObject["Currency"] as ModuleNkFilter;
					currenyFilter.Property = "BDT";
					currenyFilter.IsActive = true;
					AssertNotNull(currenyFilter);

					filterControl.FirePerformSearch();
					AssertEquals("Item Count", 0, bizO.UnmatchedCashAdvanceRequests.Count);

					Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Primary account cannot be empty"));
				}
			}
		}

		public void TestSelectionOfCashAdvanceTabReloadsCashAdvanceRequests()
		{
			if (!CanCashAdvanceRequestBeMatched)
			{
				Assert(true);
				return;
			}

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(DateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, CashAdvanceOrganizationForTest, CashAdvanceLedgerType, 200M, 200M, "AUD");
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, CashAdvanceSettlementOrgForTest, CashAdvanceLedgerType, 150M, 150M, "AUD");
			Factory.Save();

			using (EnableCashAdvanceFunctionalityRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (NewMatchGroupFormForTest form = GetForm())
				{
					form.Show();

					var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
					var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "CashAdvanceTabPage") as ZTabPage;
					AssertNotNull(tabPage);

					var filterControl = tabPage.GetControl<AccountingOnFormFilterControl>("CashAdvanceFilterControl");
					AssertNotNull(filterControl);

					var bizO = (form.BusinessEntity as MatchingBase);
					bizO.PrimaryOrganization = CashAdvanceOrganizationForTest.PK;
					AssertEquals("Item Count", 1, bizO.UnmatchedCashAdvanceRequests.Count);

					var settlementOrg2 = bizO.MatchingFilterBizO.SettlementOrgInfos.AddNew();
					settlementOrg2.OrganisationBizO = CashAdvanceSettlementOrgForTest;

					tabControl.SelectedTab = tabPage;
					AssertEquals("Item Count", 2, bizO.UnmatchedCashAdvanceRequests.Count);
				}
			}
		}

		public abstract void TestUnmatchedTransactionsGridColumns();

		public void TestMatchedTransactionsGridColumns()
		{
			using (Form form = GetForm())
			{
				form.Show();

				var matchedTransactions = form.GetControl<ZGrid>("MatchTransactionsGrid");
				AssertNotNull(matchedTransactions);

				AssertColumn("InvoiceTransactionReference", "Invoice Transaction Reference", matchedTransactions);
				AssertColumn("TransactionReference", "Compliance Number", matchedTransactions);
			}
		}

		public void TestCashAdvanceGridColumns()
		{
			if (!CanCashAdvanceRequestBeMatched)
			{
				Assert(true);
				return;
			}

			using (EnableCashAdvanceFunctionalityRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (Form form = GetForm())
				{
					form.Show();

					var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
					var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "CashAdvanceTabPage") as ZTabPage;
					AssertNotNull(tabPage);

					var displayGrid = tabPage.GetControl<ZDisplayGrid>("unmatchedCashAdvanceRequestsGrid");
					AssertNotNull(displayGrid);
					AssertColumn("OrganizationCode", "Organization", displayGrid);
					AssertColumn("CAH_Ledger", "Ledger", displayGrid);
					AssertColumn("CAH_RequestReferenceNumber", "Request Id", displayGrid);
					AssertColumn("CAH_RX_NKTransactionCurrency", "Currency", displayGrid);
					AssertColumn("CAH_OSOutstandingAmount", "OS Out. Amount", displayGrid);
					AssertColumn("CAH_LocalOutstandingAmount", "Local Out. Amount", displayGrid);
					AssertColumn("CAH_SystemCreateTimeUtc", "Request Date (UTC)", displayGrid);
					AssertColumn("JobBranch", "Branch", displayGrid);
					AssertColumn("JobDepartment", "Department", displayGrid);
					AssertColumn("JobNumber", "Job Number", displayGrid);
					AssertColumn("ShipmentHouseBill", "House Bill", displayGrid);
					AssertColumn("ShipmentMasterBill", "Master Bill", displayGrid);
					AssertColumn("DebtorAddress", "Address", displayGrid);
				}
			}
		}

		public void TestCashAdvanceTabPageConditionalVisibility()
		{
			foreach (var (functionalityEnabled, manualUpdateToPaidStatus) in new[] { (true, true), (true, false), (false, true), (false, false) })
			{
				using (EnableCashAdvanceFunctionalityRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, functionalityEnabled))
				using (AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, manualUpdateToPaidStatus))
				{
					var expectedVisibility = CanCashAdvanceRequestBeMatched && functionalityEnabled && !manualUpdateToPaidStatus;
					AssertTabPageVisibility("CashAdvanceTabPage", expectedVisibility);
				}
			}
		}

		public void TestTabpageSelection()
		{
			CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (EnableCashAdvanceFunctionalityRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTabPageVisibility("CashAdvanceTabPage", CanCashAdvanceRequestBeMatched, (f) => Assert(f.IsCashAdvanceTabSelected_ForTestOnly));
				AssertTabPageVisibility("GridsTabPage", true, (f) => Assert(f.IsTransactionsTabSelected_ForTestOnly));
				AssertTabPageVisibility("APJournalsTabPage", true, (f) => Assert(f.IsAPJournalsTabPageSelected_ForTestOnly));
				AssertTabPageVisibility("ARJournalsTabPage", true, (f) => Assert(f.IsARJournalsTabPageSelected_ForTestOnly));
			}
		}

		public void TestDoubleClickingOfAnUnmatchedCashAdvanceRequestMovesItToMatchedGrid()
		{
			if (!CanCashAdvanceRequestBeMatched)
			{
				Assert(true);
				return;
			}

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(DateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, CashAdvanceOrganizationForTest, CashAdvanceLedgerType, 200M, 200M, "AUD");
			Factory.Save();

			CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (EnableCashAdvanceFunctionalityRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (NewMatchGroupFormForTest form = GetForm())
				{
					form.Show();

					var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
					var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "CashAdvanceTabPage") as ZTabPage;
					AssertNotNull(tabPage);

					tabControl.SelectedTab = tabPage;
					var filterControl = tabPage.GetControl<AccountingOnFormFilterControl>("CashAdvanceFilterControl");
					AssertNotNull(filterControl);

					var bizO = (form.BusinessEntity as MatchingBase);
					bizO.PrimaryOrganization = CashAdvanceOrganizationForTest.PK;
					AssertEquals("Unmatched Item Count", 1, bizO.UnmatchedCashAdvanceRequests.Count);
					AssertEquals("Matched Item Count", 0, bizO.MatchedCashAdvanceRequests.Count);

					var unmatchedCAHGrid = tabPage.GetControl<ZDisplayGrid>("unmatchedCashAdvanceRequestsGrid");
					AssertNotNull(unmatchedCAHGrid);

					unmatchedCAHGrid.SelectSingleElementByPK(bizO.UnmatchedCashAdvanceRequests[0].PK);
					form.UnmatchedCashAdvanceRequestsGrid_DoubleClick_ForTestOnnly();
					AssertEquals("Matched Item Count", 1, bizO.MatchedCashAdvanceRequests.Count);
					AssertEquals("Unmatched Item Count", 0, bizO.UnmatchedCashAdvanceRequests.Count);
				}
			}
		}

		public void TestMoveAllDownButtonClickMovesAnUnmatchedCashAdvanceRequestToMatchedGrid()
		{
			if (!CanCashAdvanceRequestBeMatched)
			{
				Assert(true);
				return;
			}

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(DateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(job, CashAdvanceOrganizationForTest, CashAdvanceLedgerType, 200M, 200M, "AUD");
			Factory.Save();

			CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (EnableCashAdvanceFunctionalityRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (NewMatchGroupFormForTest form = GetForm())
				{
					form.Show();

					var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
					var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "CashAdvanceTabPage") as ZTabPage;
					AssertNotNull(tabPage);

					tabControl.SelectedTab = tabPage;
					var filterControl = tabPage.GetControl<AccountingOnFormFilterControl>("CashAdvanceFilterControl");
					AssertNotNull(filterControl);

					var bizO = (form.BusinessEntity as MatchingBase);
					bizO.PrimaryOrganization = CashAdvanceOrganizationForTest.PK;
					AssertEquals("Unmatched Item Count", 1, bizO.UnmatchedCashAdvanceRequests.Count);
					AssertEquals("Matched Item Count", 0, bizO.MatchedCashAdvanceRequests.Count);

					var unmatchedCAHGrid = tabPage.GetControl<ZDisplayGrid>("unmatchedCashAdvanceRequestsGrid");
					AssertNotNull(unmatchedCAHGrid);

					var moveAllDown = form.GetControl<ZButton>("SelectAllButton");
					moveAllDown.PerformClick();
					AssertEquals("Matched Item Count", 1, bizO.MatchedCashAdvanceRequests.Count);
					AssertEquals("Unmatched Item Count", 0, bizO.UnmatchedCashAdvanceRequests.Count);

					var journal = bizO.MatchedCashAdvanceRequests.Keys.First();
					AssertNotNull(journal);
					if (journal.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						Assert("Added to Balancing AR Journal", bizO.BalancingARJournals.Contains(journal));
					}
					else
					{
						Assert("Added to Balancing AP Journal", bizO.BalancingAPJournals.Contains(journal));
					}
					Assert("Added to Matched Transactions", bizO.MatchedTransactions.Contains(journal));

					var expectedTransactionCategory = journal.AH_Ledger == LedgerTypes.AccountsReceivable ? TransactionCategory.Codes.CashAdvanceReceived : TransactionCategory.Codes.CashAdvancePaid;
					AssertEquals(nameof(journal.AH_TransactionCategory), expectedTransactionCategory, journal.AH_TransactionCategory);

					AssertEquals(nameof(journal.AH_OH), CashAdvanceOrganizationForTest.PK, journal.AH_OH);
					AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Payment of Advance Payment Request [{cah.CAH_RequestReferenceNumber}]"), journal.AH_Desc);

					var expectedSign = journal.AH_Ledger == LedgerTypes.AccountsReceivable ? Core.Constants.DebitCredit.Credit : Core.Constants.DebitCredit.Debit;
					AssertEquals(nameof(journal.DebitCreditSign), expectedSign, journal.DebitCreditSign);

					AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
					AssertEquals(nameof(journal.AH_OSExTaxAmount), cah.CAH_OSOutstandingAmount, journal.AH_OSExTaxAmount);
					AssertEquals(nameof(journal.AH_LocalExTaxAmount), cah.CAH_LocalOutstandingAmount, journal.AH_LocalExTaxAmount);
					AssertEquals(nameof(journal.AH_AG), TestObjectCreator.GLHeader1.PK, journal.AH_AG);
				}
			}
		}
		public void TestMatchCashAdvanceRequestWhenNoCashAdvanceClearingAccountIsPresent()
		{
			if (!CanCashAdvanceRequestBeMatched)
			{
				Assert(true);
				return;
			}

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(DateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			TestObjectCreator.CreateCashAdvanceRequestHeader(job, CashAdvanceOrganizationForTest, CashAdvanceLedgerType, 200M, 200M, "AUD");
			Factory.Save();

			using (EnableCashAdvanceFunctionalityRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (NewMatchGroupFormForTest form = GetForm())
				{
					form.Show();

					var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
					var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "CashAdvanceTabPage") as ZTabPage;
					AssertNotNull(tabPage);

					tabControl.SelectedTab = tabPage;
					var filterControl = tabPage.GetControl<AccountingOnFormFilterControl>("CashAdvanceFilterControl");
					AssertNotNull(filterControl);

					var bizO = (form.BusinessEntity as MatchingBase);
					bizO.PrimaryOrganization = CashAdvanceOrganizationForTest.PK;
					AssertEquals("Unmatched Item Count", 1, bizO.UnmatchedCashAdvanceRequests.Count);
					AssertEquals("Matched Item Count", 0, bizO.MatchedCashAdvanceRequests.Count);

					var unmatchedCAHGrid = tabPage.GetControl<ZDisplayGrid>("unmatchedCashAdvanceRequestsGrid");
					AssertNotNull(unmatchedCAHGrid);

					unmatchedCAHGrid.SelectSingleElementByPK(bizO.UnmatchedCashAdvanceRequests[0].PK);
					form.UnmatchedCashAdvanceRequestsGrid_DoubleClick_ForTestOnnly();
					if (bizO.LedgerType == LedgerTypes.AccountsReceivable)
					{
						AssertEquals("This transaction cannot be matched.\r\nOn matching this transaction, a journal to the Receivables Advance Payment Clearing Account will be created, which requires a Advance Payment Clearing Account to be recorded in the Accounting -> Advance Payments -> Receivables -> Advance Payment Clearing Account registry.\r\nPlease ensure this registry has an account recorded and then match the transaction.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertEquals("This transaction cannot be matched.\r\nOn matching this transaction, a journal to the Payables Advance Payment Clearing Account will be created, which requires a Advance Payment Clearing Account to be recorded in the Accounting -> Advance Payments -> Payables -> Payables Advance Payment Clearing Account registry.\r\nPlease ensure this registry has an account recorded and then match the transaction.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					AssertEquals("Matched Item Count", 0, bizO.MatchedCashAdvanceRequests.Count);
					AssertEquals("Unmatched Item Count", 1, bizO.UnmatchedCashAdvanceRequests.Count);
				}
			}
		}
		public void TestMoveAllUpButtonClickMovesAnUnmatchedCashAdvanceRequestToMatchedGrid()
		{
			if (!CanCashAdvanceRequestBeMatched)
			{
				Assert(true);
				return;
			}

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(DateTime.Today.Year);
			var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
			var cah = TestObjectCreator.CreateCashAdvanceRequestHeader(job, CashAdvanceOrganizationForTest, CashAdvanceLedgerType, 200M, 200M, "AUD");
			Factory.Save();

			CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			using (EnableCashAdvanceFunctionalityRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				using (NewMatchGroupFormForTest form = GetForm())
				{
					form.Show();

					var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
					var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "CashAdvanceTabPage") as ZTabPage;
					AssertNotNull(tabPage);

					tabControl.SelectedTab = tabPage;
					var filterControl = tabPage.GetControl<AccountingOnFormFilterControl>("CashAdvanceFilterControl");
					AssertNotNull(filterControl);

					var bizO = (form.BusinessEntity as MatchingBase);
					bizO.PrimaryOrganization = CashAdvanceOrganizationForTest.PK;
					AssertEquals("Unmatched Item Count", 1, bizO.UnmatchedCashAdvanceRequests.Count);
					AssertEquals("Matched Item Count", 0, bizO.MatchedCashAdvanceRequests.Count);

					var unmatchedCAHGrid = tabPage.GetControl<ZDisplayGrid>("unmatchedCashAdvanceRequestsGrid");
					AssertNotNull(unmatchedCAHGrid);

					var moveAllDown = form.GetControl<ZButton>("SelectAllButton");
					moveAllDown.PerformClick();
					AssertEquals("Matched Item Count", 1, bizO.MatchedCashAdvanceRequests.Count);
					AssertEquals("Unmatched Item Count", 0, bizO.UnmatchedCashAdvanceRequests.Count);

					var journal = bizO.MatchedCashAdvanceRequests.Keys.First();
					AssertNotNull(journal);
					var journalLedgerType = journal.AH_Ledger;
					if (journalLedgerType == LedgerTypes.AccountsReceivable)
					{
						Assert("Added to Balancing AR Journal", bizO.BalancingARJournals.Contains(journal));
					}
					else
					{
						Assert("Added to Balancing AP Journal", bizO.BalancingAPJournals.Contains(journal));
					}

					Assert("Added to Matched Transactions", bizO.MatchedTransactions.Contains(journal));

					var moveAllUp = form.GetControl<ZButton>("UnselectAllButton");
					moveAllUp.PerformClick();
					AssertEquals("Matched Item Count", 0, bizO.MatchedCashAdvanceRequests.Count);
					AssertEquals("Unmatched Item Count", 1, bizO.UnmatchedCashAdvanceRequests.Count);

					if (journalLedgerType == LedgerTypes.AccountsReceivable)
					{
						Assert("Removed from Balancing AR Journal", !bizO.BalancingARJournals.Contains(journal));
					}
					else
					{
						Assert("Removed from Balancing AP Journal", !bizO.BalancingAPJournals.Contains(journal));
					}

					Assert("Removed from Matched Transactions", !bizO.MatchedTransactions.Contains(journal));
				}
			}
		}

		public void TestFilterStripPanelHeight_WithMoreThan5Filters_ShouldLessThanMaximumHeight()
		{
			using (NewMatchGroupFormForTest form = GetForm())
			{
				form.Show();

				var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
				var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "GridsTabPage") as ZTabPage;
				var transactionsTopPanel = tabPage.GetControl<ZPanel>("TransactionsTopPanel");
				var unmatchedTransactionsGroupBox = transactionsTopPanel.GetControl<ZGroupBox>("UnmatchedTransactionsGroupBox");
				var transactionFilterControl = transactionsTopPanel.GetControl<MatchingFormFilterControl>("TransactionFilterControl");
				var stripFilterPanel = transactionFilterControl.GetControl<KPanel>("FilterStripsPanel");

				transactionFilterControl.AddNewFilterStrip();
				transactionFilterControl.AddNewFilterStrip();
				transactionFilterControl.AddNewFilterStrip();
				transactionFilterControl.AddNewFilterStrip();
				transactionFilterControl.AddNewFilterStrip();
				transactionFilterControl.AddNewFilterStrip();

				AssertEquals("Should have more than 5 filters", 7, stripFilterPanel.Controls.Find("ZFilterStrip", true).Length);
				AssertLessThanOrEqualTo(stripFilterPanel.Height, transactionFilterControl.MaxFilterStripPanelHeight_internalValue);
			}
		}

		public void TestNewMatchGroupFormMimimumSize_ReduceFormSize_SizeShouldEqualMimimumSize()
		{
			using (NewMatchGroupFormForTest form = GetForm())
			{
				form.Show();
				form.Size = ControlDpiScalingHelper.NewScaledSize(100, 100);

				AssertEquals(form.MinimumSize.Width, form.Size.Width);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(1250), form.Size.Width);
				AssertEquals(form.MinimumSize.Height, form.Size.Height);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiY(725), form.Size.Height);
			}
		}

		public void TestNewMatchGroupForm_OrganizationTab_ShouldHavePrimaryOrgAndSettlementOrgPages()
		{
			using (NewMatchGroupFormForTest form = GetForm())
			{
				form.Show();

				var tabControl = form.GetControl<ZTemplateTabControl>("OrganisationTabControl");
				AssertNotNull(tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "PrimaryOrganisationTabPage") as ZTabPage);
				AssertNotNull(tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "SettlementOrgsTabPage") as ZTabPage);
			}
		}

		void AssertTabPageVisibility(string tabPageName, bool expectedVisibility, Action<NewMatchGroupFormForTest> additionalAction = null)
		{
			using (NewMatchGroupFormForTest form = GetForm())
			{
				var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
				var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == tabPageName) as ZTabPage;
				AssertNotNull(tabPage);
				AssertEquals(FormattableString.Invariant($"{tabPageName} visibility"), expectedVisibility, tabPage.TabVisible);

				if (tabPage.TabVisible)
				{
					tabControl.SelectedTab = tabPage;
					additionalAction?.Invoke(form);
				}
			}
		}

		public void AssertColumn(string columnName, string caption, ZGrid zGrid, bool isVisible = true)
		{
			var columnStyle = zGrid.GetColumnStyle(columnName);
			AssertNotNull(columnStyle);
			AssertEquals(nameof(columnStyle.IsVisible), true, columnStyle.IsVisible);
			AssertEquals(caption, columnStyle.CaptionResourceString.Caption);
		}

		public void TestShowAlternateGLAccountNumberAndDescription_HasGLAccountSelectionAndEntry()
		{
			var chart = TestObjectCreator.CreateAlternateChart("MGT", "Management Reporting", isGlobal: true);
			TestObjectCreator.CreateAccAlternateChartFormat(chart, 1, "X", "tier 1");
			Factory.Save();

			var glHeader = TestObjectCreator.CreateAccGLHeader("1991.01.10", "AS", "BANK ACCOUNT", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			var alternateGLAccount = TestObjectCreator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1, description: "AlternateGLAccount1");
			TestObjectCreator.CreateAccAlternateGlAccountAttribute(alternateGLAccount, glHeader.PK);
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid());

			AssertShowAlternateGLAccountNumberAndDescription(true);
		}

		public void TestShowAlternateGLAccountNumberAndDescription_NoGLAccountSelectionAndEntry()
		{
			AssertShowAlternateGLAccountNumberAndDescription(false);
		}

		void AssertShowAlternateGLAccountNumberAndDescription(bool hasGLAccountSelectionAndEntry)
		{
			using (var form = GetForm())
			{
				form.Show();
				var alternateGLAccountNumber = JournalsGrid.GetColumnStyle("AlternateGLAccountNumber");
				AssertNotNull(alternateGLAccountNumber);
				AssertEquals(true, hasGLAccountSelectionAndEntry ? alternateGLAccountNumber.IsVisible : alternateGLAccountNumber.IsUnavailable);

				var alternateGLAccountDescription = JournalsGrid.GetColumnStyle("AlternateGLAccountDescription");
				AssertNotNull(alternateGLAccountDescription);
				AssertEquals(true, hasGLAccountSelectionAndEntry ? alternateGLAccountNumber.IsVisible : alternateGLAccountDescription.IsUnavailable);
			}
		}

		protected abstract bool CanCashAdvanceRequestBeMatched { get; }

		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator fTestObjectCreator;
	}

	public class NewMatchGroupForm_ARJournalsTest : NewMatchGroupFormTest<ARJournalCollection>
	{
		#region Implementation

		protected override ZGrid JournalsGrid => TestNewMatchGroupForm.GetControl<ZGrid>("ARJournalsGrid");

		protected override ZTabPage JournalsTabPage => TestNewMatchGroupForm.GetControl<ZTabPage>("ARJournalsTabPage");

		protected override ARJournalCollection BalancingJournals => TestMatchingBase.BalancingARJournals;

		protected override ZString BindGridName => "BalancingARJournals";

		protected override Type JournalBaseFormBizoType => typeof(ARJournal);

		protected override void JournalsGridMenu_Popup()
		{
			TestNewMatchGroupForm.ARJournalsGridMenu_Popup(null, EventArgs.Empty);
		}

		protected override NewMatchGroupFormForTest GetForm()
		{
			TestMatchingBase = new ARMatchingBase(Factory, Factory.NewWithValidTestData<ARPayment>());
			TestNewMatchGroupForm = new NewMatchGroupFormForTest(TestMatchingBase);
			return TestNewMatchGroupForm;
		}

		public override PaymentApprovalWithoutAuthorisation GetPaymentApprovalWithoutAuthorisation()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var payment = Factory.NewWithValidTestData<ARPaymentApprovalWithoutAuthorisation>();
			payment.AV_OH = TestObjectCreator.Debtor1.PK;
			payment.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			payment.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			payment.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			payment.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			payment.AV_Amount = 100M;
			return payment;
		}

		public override PaymentApprovalWithAuthorisation GetPaymentApprovalWithAuthorisation()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var payment = Factory.NewWithValidTestData<ARPaymentApprovalWithAuthorisation>();
			payment.AV_OH = TestObjectCreator.Debtor1.PK;
			payment.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			payment.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			payment.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			payment.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			payment.AV_Amount = 100M;
			return payment;
		}

		public override void TestUnmatchedTransactionsGridColumns()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (Form form = GetForm())
			{
				form.Show();

				var unmatchedTransactionsGrid = form.GetControl<ZDisplayGrid>("UnmatchedTransactionsGrid");
				AssertNotNull(unmatchedTransactionsGrid);

				AssertColumn("InvoiceTransactionReference", "Invoice Transaction Reference", unmatchedTransactionsGrid);
				AssertColumn("TransactionReference", "Compliance Number", unmatchedTransactionsGrid);
				AssertColumn("RelatedDisbursementTransactions", "Disbursement Relating To", unmatchedTransactionsGrid);
			}
		}

		protected override bool CanCashAdvanceRequestBeMatched => true;

		protected override BooleanRegistryItem EnableCashAdvanceFunctionalityRegistry => Instance.EnableReceivablesCashAdvanceFunctionality;

		protected override BooleanRegistryItem AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry => Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid;

		protected override AccountingRegistryItem CashAdvanceClearingAccount => Instance.CashAdvanceClearingAccount;

		protected override OrgHeader CashAdvanceOrganizationForTest => TestObjectCreator.Debtor;

		protected override OrgHeader CashAdvanceSettlementOrgForTest => TestObjectCreator.Debtor1;

		protected override string CashAdvanceLedgerType => LedgerTypes.AccountsReceivable;

		#endregion
	}

	public class NewMatchGroupForm_APJournalsTest : NewMatchGroupFormTest<APJournalCollection>
	{
		#region Implementation

		protected override ZGrid JournalsGrid => TestNewMatchGroupForm.GetControl<ZGrid>("APJournalsGrid");

		protected override ZTabPage JournalsTabPage => TestNewMatchGroupForm.GetControl<ZTabPage>("APJournalsTabPage");

		protected override APJournalCollection BalancingJournals => TestMatchingBase.BalancingAPJournals;

		protected override ZString BindGridName => "BalancingAPJournals";

		protected override Type JournalBaseFormBizoType => typeof(APJournal);

		protected override void JournalsGridMenu_Popup()
		{
			TestNewMatchGroupForm.APJournalsGridMenu_Popup(null, EventArgs.Empty);
		}

		protected override NewMatchGroupFormForTest GetForm()
		{
			TestMatchingBase = new APMatchingBase(Factory, Factory.NewWithValidTestData<APPayment>());
			TestNewMatchGroupForm = new NewMatchGroupFormForTest(TestMatchingBase);
			return TestNewMatchGroupForm;
		}

		public override PaymentApprovalWithoutAuthorisation GetPaymentApprovalWithoutAuthorisation()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var payment = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			payment.AV_OH = TestObjectCreator.Creditor1.PK;
			payment.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			payment.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			payment.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			payment.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			payment.AV_Amount = 100M;
			return payment;
		}

		public override PaymentApprovalWithAuthorisation GetPaymentApprovalWithAuthorisation()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var payment = Factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment.AV_OH = TestObjectCreator.Creditor1.PK;
			payment.AV_AB = TestObjectCreator.AUDBankAccount.PK;
			payment.AV_AK = TestObjectCreator.AUDChequeBook.PK;
			payment.ExchangeRate.Currency = TestObjectCreator.AUD.RX_Code;
			payment.AV_ChequeOrReference = TestObjectCreator.AUDChequeBook.AK_CurrentNo.ToString();
			payment.AV_Amount = 100M;
			return payment;
		}

		public override void TestUnmatchedTransactionsGridColumns()
		{
			using (Form form = GetForm())
			{
				form.Show();

				var unmatchedTransactionsGrid = form.GetControl<ZDisplayGrid>("UnmatchedTransactionsGrid");
				AssertNotNull(unmatchedTransactionsGrid);

				AssertColumn("InvoiceTransactionReference", "Invoice Transaction Reference", unmatchedTransactionsGrid);
				AssertColumn("TransactionReference", "Compliance Number", unmatchedTransactionsGrid);
				AssertColumn("RelatedDisbursementTransactions", "Disbursement Relating To", unmatchedTransactionsGrid, false);
			}
		}

		protected override bool CanCashAdvanceRequestBeMatched => true;

		protected override BooleanRegistryItem EnableCashAdvanceFunctionalityRegistry => Instance.EnablePayablesCashAdvanceFunctionality;

		protected override BooleanRegistryItem AllowManualSettingOfCashAdvanceRequestStatusToPaidRegistry => Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid;

		protected override AccountingRegistryItem CashAdvanceClearingAccount => Instance.PayablesCashAdvanceClearingAccount;

		protected override OrgHeader CashAdvanceOrganizationForTest => TestObjectCreator.Creditor1;

		protected override OrgHeader CashAdvanceSettlementOrgForTest => TestObjectCreator.Creditor2;

		protected override string CashAdvanceLedgerType => LedgerTypes.AccountsPayable;

		#endregion
	}
}
