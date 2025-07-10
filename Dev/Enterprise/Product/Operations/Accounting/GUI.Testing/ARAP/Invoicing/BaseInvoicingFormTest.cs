using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.Accounting.Business.Presentation.GUI;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.TaxFramework.GUI;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using ExRateOption = Enterprise.Accounting.Business.AccountingConstants.InvoicePostingExchangeRateOption;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.GUI.Testing
{
	public abstract class BaseInvoicingFormTest : AccountingZFormBasherTest
	{
		protected override bool ShouldHaveAuditPlugIn => true;

		public virtual void TestOriginalInvoiceReferenceNumberForAmendVisibility()
		{
			using (var form = GetFormByInvoice(GetInvoiceWithValidTestData()))
			{
				AssertOriginalInvoiceReferenceGUIVisibilty(form, false, false);
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

			var invoice = GetInvoiceWithValidTestData();
			using (AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chart.PK.ToGuid()))
			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();
				AssertNotNull(invoice.Lines.ShowGLAccountsForImportAction);
				var grid = (ZGrid)form.Controls.Find("TransactionLinesGrid", true)[0];
				grid.SetDataBinding(invoice, "FilteredLines");
				Application.DoEvents();
				var style = grid.Columns["GenericCharge"].ColumnStyle;
				grid.BeginEdit(style, 0);
				var codeBox = ((ZGridGuidFindBox)grid.LastFocusedColumn.EditControl).CodeBox;
				codeBox.Text = "111";
				form.Controls.Find("AH_DescTextbox", true)[0].Focus();
				Application.DoEvents();
				var selectionForm = (GLAccountSelectionForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertNotNull(selectionForm);
			}
		}

		public void TestSaveAsIncomplete_WhenInvoiceDoNotHaveLines()
		{
			var invoice = GetInvoiceWithValidTestData(true);
			if (invoice.IsAPTransaction && new[] { LedgerTypes.AccountsPayable, LedgerTypes.IncompleteTransactions }.Contains(invoice.AH_Ledger.ToString()))
			{
				AssertEquals("Precondition", false, invoice.Lines.Any());
				AssertEquals("Precondition", 0m, invoice.AH_OSTotalAmount);

				using (var testForm = GetFormByInvoice(invoice))
				{
					testForm.Show();
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					AssertEquals("Precondition", true, testForm.SaveAsIncompleteMenuItem.Enabled);
					AssertNullOrEmpty("Precondition", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Precondition", false, invoice.AH_OSTotalAmountInfo.HasError("The sum of the transaction lines should not equal zero."));

					testForm.SaveAsIncompleteMenuItem.PerformClick();
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(true, invoice.AH_OSTotalAmountInfo.HasError("The sum of the transaction lines should not equal zero."));
				}
			}
			else
			{
				Assert(true);
			}
		}

		[UseSnapshotProtection([DatabaseType.Main, DatabaseType.SingleSharedRef])]
		public abstract class NonTransactionedBaseInvoicingFormTest : TestCase
		{
			public void TestSaveAsIncomplete_AfterCriticalValidationError()
			{
				var invoice = GetInvoiceWithValidTestData();
				var creator = new TestObjectCreator(invoice.Factory);

				if (invoice.AH_Ledger != LedgerTypes.AccountsPayable)
				{
					Assert("Only AP transactions can be saved as Incomplete Invoice", true);
					return;
				}

				creator.CreateTestPeriods(ZDateTime.Today);

				invoice.AH_OH = creator.TestOrganisation.PK;
				creator.CreateInvoiceLine(invoice, creator.GLHeader1.PK, 100);
				invoice.RunPreSaveValidation();
				AssertNoErrors("Invoice is valid for saving", invoice);

				using (var testForm = GetFormByInvoice(invoice))
				{
					testForm.DisplayMode = ODisplayMode.New;
					testForm.Show();

					OnSavingCriticalCheckException expectedException = null;
					invoice.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12);

					try
					{
						testForm.ValidateAndSave_ForTestOnly();
					}
					catch (OnSavingCriticalCheckException e)
					{
						expectedException = e;
					}
					finally
					{
						invoice.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.NoError);
					}

					AssertNotNull(expectedException);
					AssertEquals("ExceptionReporter should catch the critical validation error and raise 1 issue", 1, ExceptionReporterTestListener.Instance.Count);
					AssertEquals("InnerException message", expectedException.Message, ExceptionReporterTestListener.Instance[0].InnerException.Message);

					testForm.SaveAsIncomplete_ForTestOnly();
					ExceptionReporterTestListener.Instance.Clear();

					AssertEquals("IsInDatabase", true, invoice.IsInDatabase);
					AssertEquals("AH_Ledger", LedgerTypes.IncompleteTransactions, invoice.AH_Ledger);

					AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("Posting invoice after critical validation error must be prevented even when saving as incomplete is allowed.",
						() => testForm.ValidateAndSave_ForTestOnly());
				}
			}

			protected abstract BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice);

			protected abstract InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true);
		}

		#region EInvoicing test cases

		public void TestResetStatusToQueuedMenuItem_IsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn()
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			Assert("Registry is off by default", !registry.EnableEInvoicingFunctionality.Value);
			Assert("Registry for AP is off by default", !registry.EnableEInvoicingFunctionalityForPayables.Value);

			var currCompany = GlbCompany.CurrentCompany;
			foreach (var country in new[] { CountryCodes.Italy, CountryCodes.Spain })
			{
				using (currCompany.TemporarilySetCountry(country))
				{
					var testInvoice = GetInvoiceWithValidTestData();
					Factory.Save();
					var regFunctionality = testInvoice.AH_Ledger == LedgerTypes.AccountsPayable ? registry.EnableEInvoicingFunctionalityForPayables : registry.EnableEInvoicingFunctionality;

					using (regFunctionality.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (var testForm1 = new BaseInvoicingForm(testInvoice))
					{
						testForm1.Show();
						AssertNull("'Reset Status to Queued' Menu Item should not be available when 'Enable E-Reporting Functionality' registry is off", testForm1.ResetStatusToQueuedMenuItem);
					}

					using (regFunctionality.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (var testForm2 = new BaseInvoicingForm(testInvoice))
					{
						testForm2.Show();
						if (testInvoice.IsEligibleToCreateEInvoicingTransactionPivot)
						{
							AssertNotNull("'Reset Status to Queued' Menu Item should be available when 'Enable E-Reporting Functionality' registry is on", testForm2.ResetStatusToQueuedMenuItem);
						}
						else
						{
							AssertNull("Reset Status to Queued Menu Item should not exist for transactions not eligible for E-Reporting.", testForm2.ResetStatusToQueuedMenuItem);
						}
					}
				}
			}
		}

		public void TestResetStatusToQueuedMenuItem_IsOnlyAvailableOnTransactionsEligibleForEInvoicingAndAlreadyInDB()
		{
			var currCompany = GlbCompany.CurrentCompany;
			foreach (var country in new[] { CountryCodes.Italy, CountryCodes.Spain })
			{
				using (currCompany.TemporarilySetCountry(country))
				{
					var testInvoice = GetInvoiceWithValidTestData();

					var registry = AccountingMasterFilesRegistry.Instance;
					var regFunctionality = testInvoice.AH_Ledger == LedgerTypes.AccountsPayable ? registry.EnableEInvoicingFunctionalityForPayables : registry.EnableEInvoicingFunctionality;
					using (regFunctionality.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (var testForm1 = new BaseInvoicingForm(testInvoice))
					{
						testForm1.Show();
						if (testInvoice.IsEligibleToCreateEInvoicingTransactionPivot)
						{
							AssertNull("Reset Status to Queued Menu Item should not exist for transactions eligible for E-Reporting but not in DB.", testForm1.ResetStatusToQueuedMenuItem);
							Factory.Save();
							using (var testForm2 = new BaseInvoicingForm(testInvoice))
							{
								testForm2.Show();
								AssertNotNull("Reset Status to Queued Menu Item should exist for transactions eligible for E-Reporting and in DB.", testForm2.ResetStatusToQueuedMenuItem);
							}
						}
						else
						{
							AssertNull("Reset Status to Queued Menu Item should not exist for transactions not eligible for E-Reporting.", testForm1.ResetStatusToQueuedMenuItem);
						}
					}
				}
			}
		}

		[TestDate(2006, 05, 10)]
		public void TestResetStatusToQueuedMenuItem_TransactionPivotStatusConcurrency_WithReloadingTransactionInNewFactory()
		{
			var testInvoice = GetInvoiceWithValidTestData(false);
			if (!(testInvoice is ARInvoice))
			{
				Assert("TODO: test concurrency for not AR Invoices", true);
				return;
			}
			testInvoice.Delete();

			var registry = AccountingMasterFilesRegistry.Instance;
			var currCompany = GlbCompany.CurrentCompany;
			var currCompGuid = currCompany.PK.ToGuid();
			var today = ZDateTime.Today;

			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (registry.EReportingComplianceDate.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, today.AddDays(-10).ToDateTime()))
			using (registry.EnableEInvoicingFunctionality.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
			{
				TestObjectCreator.CreateTestPeriods(today);
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				Assert("Precondition: eInvoicing enabled", invoice.IsEligibleToCreateEInvoicingTransactionPivot);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				var batchPK = TestObjectCreator.CreateEInvoicingBatch();
				Factory.Save();
				var batch = Factory.Load<AccEInvoicingBatch>(batchPK);
				TestObjectCreator.UpdateEInvoicingTransactionPivot(invoice.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batchPK, today, today, ZBool.True);
				AssertPivotDetails(invoice.PK, EInvoicingPivotState.Failed, "This transaction was rejected by IIS site", batchPK, today, today, ZBool.True);

				using (var user1Form = new BaseInvoicingForm(invoice))
				{
					user1Form.Show();
					var resetStatusToQueuedMenuItemInUser1Form = user1Form.ResetStatusToQueuedMenuItem;
					AssertNotNull("Reset Status to Queued Menu Item should exist.", resetStatusToQueuedMenuItemInUser1Form);

					using (var user2Form = new BaseInvoicingForm(invoice))
					{
						user2Form.Show();
						var resetStatusToQueuedMenuItemInUser2Form = user2Form.ResetStatusToQueuedMenuItem;
						AssertNotNull("Reset Status to Queued Menu Item should exist.", resetStatusToQueuedMenuItemInUser2Form);

						var userNotif2 = UnitTestUserNotification.Instance;
						userNotif2.ClearMessagesAndAnswers();
						resetStatusToQueuedMenuItemInUser2Form.PerformClick();
						AssertEquals("Transaction was successfully re-queued.", userNotif2.LastMessage.Text);
						Assert(userNotif2.LastMessage.WasInformation);
						Assert("No warning message is displayed when transaction has error status and can be requeued", !userNotif2.PreviousMessages.ContainsMessageContainingThisText("You can only reset transactions"));

						AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch.AIB_Status);
						AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
					}

					var userNotif1 = UnitTestUserNotification.Instance;
					userNotif1.ClearMessagesAndAnswers();
					resetStatusToQueuedMenuItemInUser1Form.PerformClick();
					AssertMultilineASCIIEquals("Postcondition: second re-queue perform where is no eligible transaction to re-queue", @"Previously queued for e-Reporting transactions will be re-queued if they have the following statuses:

- 'FAL' - Fail, or
- 'BER' - Batched with errors, or
- 'SNT' - Sent (when you have appropriate security rights).

No eligible transactions found to be re-queued.", userNotif1.LastMessage.Text);
					Assert(userNotif1.LastMessage.WasError);

					AssertEquals("Postcondition: batch is discarded as transaction was requeued", EInvoicingBatchState.Discarded, batch.AIB_Status);
					AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				}
			}
		}

		[TestDate(2021, 11, 11)]
		public void TestSetStatusToAwaitMenuItem_IsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn()
		{
			AssertIsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn("Await Review", true);
		}

		[TestDate(2021, 11, 10)]
		public void TestAuthorizeAndSendMenuItem_IsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn()
		{
			AssertIsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn("Authorize And Send", false);
		}

		void AssertIsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn(string messageText, bool isStatusToAwaitTest)
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			var currCompany = GlbCompany.CurrentCompany;

			Assert("Registry is off by default", !registry.EnableEInvoicingFunctionality.Value);
			Assert("Registry for AP is off by default", !registry.EnableEInvoicingFunctionalityForPayables.Value);
			foreach (var country in new[] { CountryCodes.Italy, CountryCodes.Egypt })
			{
				using (currCompany.TemporarilySetCountry(country))
				{
					var testInvoice = GetInvoiceWithValidTestData();
					Factory.Save();
					using (var testForm1 = new BaseInvoicingForm(testInvoice))
					{
						testForm1.Show();
						AssertNull($"'{messageText}' menu item should not be available when 'Enable E-Reporting Functionality' registry is OFF", testForm1.SetStatusToAwaitMenuItem);
					}

					var currCompGuid = currCompany.PK.ToGuid();
					(var regFunctionality, var regComplianceDate, var regDefaultStatus) = GetRegistryItemsForEInvoicing(testInvoice.AH_Ledger);

					using (regFunctionality?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
					using (regComplianceDate?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime().AddDays(-10)))
					using (regDefaultStatus?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
					using (var testForm2 = new BaseInvoicingForm(testInvoice))
					{
						testForm2.Show();
						var menuItem = isStatusToAwaitTest ? testForm2.SetStatusToAwaitMenuItem : testForm2.AuthorizeAndSendMenuItem;

						if (country == CountryCodes.Italy && testInvoice.IsEligibleToCreateEInvoicingTransactionPivot)
						{
							AssertNotNull($"'{messageText}' menu item should be available when 'Enable E-Reporting Functionality' registry is ON", menuItem);
						}
						else
						{
							AssertNull($"'{messageText}' menu item should not exist for transactions not eligible for E-Reporting, or if country is not Italy", menuItem);
						}
					}
				}
			}
		}

		[TestDate(2021, 11, 11)]
		public void TestSetStatusToAwaitMenuItem_IsOnlyAvailableOnTransactionsEligibleForEInvoicingAndAlreadyInDB()
		{
			AssertIsOnlyAvailableOnTransactionsEligibleForEInvoicingAndAlreadyInDB("Await Review", true);
		}

		[TestDate(2021, 11, 10)]
		public void TestAuthorizeAndSendMenuItem_IsOnlyAvailableOnTransactionsEligibleForEInvoicingAndAlreadyInDB()
		{
			AssertIsOnlyAvailableOnTransactionsEligibleForEInvoicingAndAlreadyInDB("Authorize And Send", false);
		}

		void AssertIsOnlyAvailableOnTransactionsEligibleForEInvoicingAndAlreadyInDB(string messageText, bool isStatusToAwaitTest)
		{
			var currCompany = GlbCompany.CurrentCompany;
			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var testInvoice = GetInvoiceWithValidTestData();

				var currCompGuid = currCompany.PK.ToGuid();
				(var regFunctionality, var regComplianceDate, var regDefaultStatus) = GetRegistryItemsForEInvoicing(testInvoice.AH_Ledger);

				using (regFunctionality?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
				using (regComplianceDate?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, ZDateTime.Today.ToDateTime().AddDays(-10)))
				using (regDefaultStatus?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
				using (var testForm1 = new BaseInvoicingForm(testInvoice))
				{
					testForm1.Show();
					if (testInvoice.IsEligibleToCreateEInvoicingTransactionPivot)
					{
						AssertNull($"'{messageText}' menu item should not exist for transactions eligible for E-Reporting but not in DB.", isStatusToAwaitTest ? testForm1.SetStatusToAwaitMenuItem : testForm1.AuthorizeAndSendMenuItem);
						Factory.Save();
						using (var testForm2 = new BaseInvoicingForm(testInvoice))
						{
							testForm2.Show();
							AssertNotNull($"'{messageText}' menu item should exist for transactions eligible for E-Reporting and in DB.", isStatusToAwaitTest ? testForm2.SetStatusToAwaitMenuItem : testForm2.AuthorizeAndSendMenuItem);
						}
					}
					else
					{
						AssertNull($"'{messageText}' menu item should not exist for transactions not eligible for E-Reporting.", isStatusToAwaitTest ? testForm1.SetStatusToAwaitMenuItem : testForm1.AuthorizeAndSendMenuItem);
					}
				}
			}
		}

		[TestDate(2021, 11, 11)]
		public void TestSetStatusToAwaitMenuItem_TransactionPivotStatusConcurrency_WithReloadingTransactionInNewFactory()
		{
			var currDate = ZDateTime.Today;
			var currCompany = GlbCompany.CurrentCompany;
			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(currDate);

				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test";
				taxRate.AT_Type = AccTaxRate.Types.ReverseRated;
				Factory.Save();

				var invoice = GetInvoiceWithValidTestData();

				var currCompGuid = currCompany.PK.ToGuid();
				(var regFunctionality, var regComplianceDate, var regDefaultStatus) = GetRegistryItemsForEInvoicing(invoice.AH_Ledger);

				using (regFunctionality?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
				using (regComplianceDate?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, currDate.ToDateTime().AddDays(-10)))
				using (regDefaultStatus?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
				{
					var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
					line.AL_AT = taxRate.PK;
					Factory.Save();

					if (invoice.IsEligibleToCreateEInvoicingTransactionPivot)
					{
						EInvoicingGUIActionHelperTest.AssertPivotDetails(invoice.PK, EInvoicingPivotState.Pending);

						using (var user1Form = new BaseInvoicingForm(invoice))
						{
							user1Form.Show();

							var setStatusToAwaitMenuItemInUser1Form = user1Form.SetStatusToAwaitMenuItem;
							AssertNotNull("'Await Review' menu item should exist.", setStatusToAwaitMenuItemInUser1Form);

							if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
							{
								var userNotif = UnitTestUserNotification.Instance;
								userNotif.ClearMessagesAndAnswers();
								setStatusToAwaitMenuItemInUser1Form.PerformClick();
								AssertEquals("Transaction status was successfully set to Awaiting Review.", userNotif.LastMessage.Text);
								Assert(userNotif.LastMessage.WasInformation);
								EInvoicingGUIActionHelperTest.AssertPivotDetails(invoice.PK, EInvoicingPivotState.AwaitingReview, $"Awaiting Review status set by user CWSupport on day {currDate}. To send the invoice, click 'Authorize and Send''", ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
							}
							//else TODO for AR
						}
					}
					else
					{
						Assert(true);
					}
				}
			}
		}

		[TestDate(2021, 11, 10)]
		public void TestAuthorizeAndSendMenuItem_TransactionPivotStatusConcurrency_WithReloadingTransactionInNewFactory()
		{
			var currDate = ZDateTime.Today;
			var currCompany = GlbCompany.CurrentCompany;
			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				TestObjectCreator.CreateTestPeriods(currDate);

				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "Test";
				taxRate.AT_Type = AccTaxRate.Types.ReverseRated;
				Factory.Save();

				var invoice = GetInvoiceWithValidTestData();

				var currCompGuid = currCompany.PK.ToGuid();
				(var regFunctionality, var regComplianceDate, var regDefaultStatus) = GetRegistryItemsForEInvoicing(invoice.AH_Ledger);

				using (regFunctionality?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, true))
				using (regComplianceDate?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, currDate.ToDateTime().AddDays(-10)))
				using (regDefaultStatus?.SetTemporaryValue(currCompGuid, Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
				{
					var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
					line.AL_AT = taxRate.PK;
					Factory.Save();

					if (invoice.IsEligibleToCreateEInvoicingTransactionPivot)
					{
						EInvoicingGUIActionHelperTest.AssertPivotDetails(invoice.PK, EInvoicingPivotState.Pending);

						using (var user1Form = new BaseInvoicingForm(invoice))
						{
							user1Form.Show();

							var setStatusMenuItemInUser1Form = user1Form.AuthorizeAndSendMenuItem;
							AssertNotNull($"'Authorize And Send' menu item should exist.", setStatusMenuItemInUser1Form);

							if (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
							{
								var userNotif = UnitTestUserNotification.Instance;
								userNotif.ClearMessagesAndAnswers();
								setStatusMenuItemInUser1Form.PerformClick();
								AssertEquals("Transaction is now queued for sending.", userNotif.LastMessage.Text);
								Assert(userNotif.LastMessage.WasInformation);
								EInvoicingGUIActionHelperTest.AssertPivotDetails(invoice.PK, EInvoicingPivotState.Queued, "", ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
							}
						}
					}
					else
					{
						Assert(true);
					}
				}
			}
		}

		(BooleanRegistryItem, DateTimeRegistryItem, CodePairRegistryItem) GetRegistryItemsForEInvoicing(string ledger)
		{
			var registry = AccountingMasterFilesRegistry.Instance;
			BooleanRegistryItem regFunction = null;
			DateTimeRegistryItem regDate = null;
			CodePairRegistryItem regStatus = null;
			if (ledger == LedgerTypes.AccountsReceivable)
			{
				regFunction = registry.EnableEInvoicingFunctionality;
				regDate = registry.EReportingComplianceDate;
				regStatus = registry.EReportingSubmitPivotDefaultStatus;
			}
			else if (ledger == LedgerTypes.AccountsPayable)
			{
				regFunction = registry.EnableEInvoicingFunctionalityForPayables;
				regDate = registry.EReportingComplianceDateForPayables;
				regStatus = registry.EReportingSubmitPivotDefaultStatusForPayables;
			}
			return (regFunction, regDate, regStatus);
		}

		void AssertPivotDetails(ZGuid parentPk, ZString expectedStatus, ZString expectedErrorDescription, ZGuid expectedBatchPK, ZDateTime expectedLastResponseReceived, ZDateTime expectedSentTime, ZBool expectedIsNotifiedByEmail)
		{
			EInvoicingGUIActionHelperTest.AssertPivotDetails(parentPk, expectedStatus, expectedErrorDescription, expectedBatchPK, expectedLastResponseReceived, expectedSentTime, expectedIsNotifiedByEmail);
		}

		#endregion

		[TestDate(2019, 12, 21)]
		public void TestCreateInvoiceAndReverseRunsGLAggregate()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "001001", TestObjectCreator.USD, 0.66M, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 0.66M, 200M);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.GenericCharge = TestObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			TestObjectCreator.GLHeader1.AG_Description = "GL Header 1";
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			var jobCharge = TestObjectCreator.CreateCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.USD);
			jobCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			line.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			line.PeriodStartDate = new ZDate(2020, 2, 15);
			line.PeriodEndDate = new ZDate(2020, 3, 29);

			var aggregateInDB = Factory.Load<AccGLAggregate>(new ZQuery());
			AssertEquals("pre condition", 0, aggregateInDB.Length);

			using (var form = new InvoiceForm(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				form.ValidateAndSave_ForTestOnly();
			}

			aggregateInDB = Factory.Load<AccGLAggregate>(new ZQuery());
			AssertEquals("post condition", 6, aggregateInDB.Length);
			AssertEquals(2, aggregateInDB.Count(x => x.AA_Period == 202006));
			AssertEquals(2, aggregateInDB.Count(x => x.AA_Period == 202008));
			AssertEquals(2, aggregateInDB.Count(x => x.AA_Period == 202009));
			AssertEquals(0m, aggregateInDB.Sum(x => x.AA_Amount));

			var reversing = new InvoicingBaseReversing(invoice);
			reversing.Reverse();
			reversing.ReverseTransaction.TransactionNumber = "1234567";
			Assert(invoice.IsReversed);
			Factory.Save();
			aggregateInDB = Factory.Load<AccGLAggregate>(new ZQuery());
			AssertEquals("post condition", 12, aggregateInDB.Length);
			AssertEquals(4, aggregateInDB.Count(x => x.AA_Period == 202006));
			AssertEquals(4, aggregateInDB.Count(x => x.AA_Period == 202008));
			AssertEquals(4, aggregateInDB.Count(x => x.AA_Period == 202009));
			AssertEquals(0m, aggregateInDB.Sum(x => x.AA_Amount));
		}

		[TestDate(2019, 12, 21)]
		[ExpectNoExceptions]
		public void TestAddingEDocsToInvoiceDoesNotCreateNewGLJournalsAndAggregate()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "001001", TestObjectCreator.USD, 0.66M, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 0.66M, 200M);
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.GenericCharge = TestObjectCreator.CC1.PK;
			line.AL_JH = job.PK;
			TestObjectCreator.GLHeader1.AG_Description = "GL Header 1";
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			var jobCharge = TestObjectCreator.CreateCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.USD);
			jobCharge.JR_AT_CostGSTRate = TestObjectCreator.GST1.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			line.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			line.PeriodStartDate = new ZDate(2020, 2, 15);
			line.PeriodEndDate = new ZDate(2020, 3, 29);

			var glJournalInDB = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
			AssertEquals("pre condition", 0, glJournalInDB.Length);
			var aggregateInDB = Factory.Load<AccGLAggregate>(new ZQuery());
			AssertEquals("pre condition", 0, aggregateInDB.Length);

			using (var form = new InvoiceForm(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				form.ValidateAndSave_ForTestOnly();

				glJournalInDB = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
				AssertEquals("post condition - expect journals created in the first save", 3, glJournalInDB.Length);
				aggregateInDB = Factory.Load<AccGLAggregate>(new ZQuery());
				AssertEquals("post condition - expect aggregates created in the first save", 6, aggregateInDB.Length);

				invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3, 4 }, "test2", "TXT");
				form.ValidateAndSave_ForTestOnly();

				glJournalInDB = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
				AssertEquals("post condition - expect no journals created in the second save", 3, glJournalInDB.Length);
				aggregateInDB = Factory.Load<AccGLAggregate>(new ZQuery());
				AssertEquals("post condition - expect no aggregates created in the second save", 6, aggregateInDB.Length);
			}
		}

		[TestDate(2019, 12, 21)]
		public void TestDoNotCreateGLJournalsAndAggregateWhenSaveIncompleteTransaction()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var periodManagementTestHelper = new AccountingPeriodTestHelper();
			periodManagementTestHelper.PostPeriodsForEntireYear(2020, GlbCompany.CurrentCompany.PK);
			Factory.Save();

			var invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(typeof(APInvoice), TestObjectCreator.Creditor1, 100, "INV1");
			var line = invoice.Lines[0];
			line.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			line.PeriodClearingGLAccountPK = TestObjectCreator.GLJournalClearingAccount.PK;
			line.PeriodStartDate = new ZDate(2020, 2, 15);
			line.PeriodEndDate = new ZDate(2020, 3, 29);

			TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var incompleteInvoice = newFactory.Load<InvoicingBase>(invoice.PK);
			incompleteInvoice.RestoreSavedData();

			var glJournalInDB = newFactory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
			AssertEquals("pre condition", 0, glJournalInDB.Length);
			var aggregateInDB = newFactory.Load<AccGLAggregate>(new ZQuery());
			AssertEquals("pre condition", 0, aggregateInDB.Length);

			using (var form = new InvoiceForm(incompleteInvoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				incompleteInvoice.AH_Desc = "new desc";

				form.AlwaysCreateApprovalRequest_ForTestOnly = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FireSaveButton();
				AssertEquals("IN", incompleteInvoice.AH_Ledger);
				AssertEquals("INI", incompleteInvoice.AH_TransactionType);

				glJournalInDB = newFactory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal));
				AssertEquals("post condition - Expect no journals created", 0, glJournalInDB.Length);
				aggregateInDB = newFactory.Load<AccGLAggregate>(new ZQuery());
				AssertEquals("post condition - Expect no aggregate created", 0, aggregateInDB.Length);
			}
		}

		#region TestUnrecoverableError

		public void TestUnrecoverableError_SavingFormWithPostedInvoiceLikeAddingEDocs()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var invoice = GetInvoiceWithValidTestData();
			invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 10);
			invoice.SubmittedFromInvoicingForm = true;
			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();

				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition: invoice", invoice);
				if (invoice is APInvoice || invoice is APCreditNote)
				{
					if (!invoice.IsIncompleteInvoice)
					{
						Assert("Precondition: ShouldRunJobChargeTransformer", invoice.ShouldRunJobChargeTransformer);
					}
					Assert("Precondition: HasJobChargeTransformerBeenRun", !invoice.HasJobChargeTransformerBeenRun);
				}
				AssertEquals("Precondition: FireSaveButton", ContinueWithSave.Yes, form.FireSaveButton());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoice.RunPreSaveValidation();
				if (invoice is APInvoice || invoice is APCreditNote)
				{
					Assert("Precondition: ShouldRunJobChargeTransformer. Incomplete will be completed on from saving and transformer also will be run.", !invoice.ShouldRunJobChargeTransformer);
					Assert("Precondition: HasJobChargeTransformerBeenRun", invoice.HasJobChargeTransformerBeenRun);
				}
				AssertNoErrors("Precondition", invoice);
				Assert("Precondition: IsInDatabase", invoice.IsInDatabase);
				AssertEquals("Saving like after adding eDocs for posted invoice without closing a form.", ContinueWithSave.Yes, form.ValidateAndSave_ForTestOnly());
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestUnrecoverableError()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var invoice = GetInvoiceWithValidTestData();
			invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 10);
			invoice.SubmittedFromInvoicingForm = true;
			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();

				BusinessObjectFactory.SavingEventHandler preventSavingHandler = (factory) => { throw new InvalidOperationException("TestUnrecoverableError"); };
				InvalidOperationException exceptionCaught = null;
				try
				{
					invoice.RunPreSaveValidation();
					AssertNoErrors("Precondition: invoice", invoice);
					if (invoice is APInvoice || invoice is APCreditNote)
					{
						if (!invoice.IsIncompleteInvoice)
						{
							Assert("Precondition: ShouldRunJobChargeTransformer. Incomplete will be completed on from saving and transformer also will be run.", invoice.ShouldRunJobChargeTransformer);
						}
						Assert("Precondition: HasJobChargeTransformerBeenRun", !invoice.HasJobChargeTransformerBeenRun);
					}
					Factory.Saving += preventSavingHandler;
					form.ValidateAndSave_ForTestOnly();
				}
				catch (InvalidOperationException ex)
				{
					exceptionCaught = ex;
				}

				AssertNotNull("Precondition: saving exception", exceptionCaught);
				AssertEquals("Precondition: saving exception", "TestUnrecoverableError", exceptionCaught.Message);
				if (!invoice.IsCompletingInvoice)
				{
					Assert("Precondition: User now can edit it and save one more time.", !invoice.IsInDatabase);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Saving -= preventSavingHandler;
				if (invoice is APInvoice || invoice is APCreditNote)
				{
					Assert("Precondition: ShouldRunJobChargeTransformer", invoice.ShouldRunJobChargeTransformer);
					Assert("Precondition: HasJobChargeTransformerBeenRun", invoice.HasJobChargeTransformerBeenRun);
					AssertEquals("FireSaveButton", ContinueWithSave.No, form.FireSaveButton());

					AssertEquals(@"The saving process has encountered an unrecoverable error. Please save this transaction as incomplete (Actions > Save as 'Incomplete') and post from the Incomplete Invoices module.",
						UnitTestUserNotification.Instance.LastMessage.Text);
					if (!invoice.IsCompletingInvoice)
					{
						Assert("IsInDatabase", !invoice.IsInDatabase);
					}
				}
				else
				{
					AssertEquals("FireSaveButton", ContinueWithSave.Yes, form.FireSaveButton());
					Assert("IsInDatabase", invoice.IsInDatabase);
				}

				form.Close();
			}
		}

		public void TestNoExceptionThrownWhenFormClosing()
		{
			var factory = new BusinessObjectFactory();
			var invoice = factory.NewWithValidTestData<APInvoice>();

			using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
			{
				form.Show();

				form.SetDataBinding(null, null);

				AssertNoExceptionThrown(form.Close);
			}
		}

		public void TestNoConcurrencyExceptionOnIncompleteInvoice()
		{
			if (GetInvoiceWithValidTestData().AH_Ledger != LedgerTypes.AccountsPayable)
			{
				Assert("Only AP transactions can be saved as Incomplete Invoice", true);
				return;
			}

			Factory.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			TestObjectCreator.Factory.Save();

			var invoice1 = Factory.New<APInvoice>();
			invoice1.AH_OH = TestObjectCreator.TestOrganisation.PK;
			invoice1.AH_TransactionNum = "KAGURA";
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.GLHeader1.PK, 100);
			invoice1.RunPreSaveValidation();
			Assert("No errors so we can continue to save", !invoice1.HasErrors);
			invoice1.SaveAsIncomplete();

			var invoice2 = factory2.Load<APInvoice>(invoice1.PK);
			invoice2.RestoreSavedData();

			line1.AL_OSExTaxAmount = 10;
			invoice1.RunPreSaveValidation();
			Assert("No errors so we can continue to save", !invoice1.HasErrors);
			invoice1.SaveAsIncomplete();

			invoice2.Lines[0].AL_OSExTaxAmount = 1;
			invoice2.RunPreSaveValidation();
			Assert("No errors so we can continue to save", !invoice2.HasErrors);

			using (var testForm = GetFormByInvoice(invoice2))
			{
				AssertNoExceptionThrown(() => testForm.SaveAsIncomplete_ForTestOnly());
				AssertContains(
	@"While you have been working with this form, another user has made changes.

The system cannot automatically merge your changes because there are conflicts with critical fields.
Please cancel your changes and reload the form.

The following objects have critical changes and cannot be merged:", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveAsIncomplete_CriticalValidationError()
		{
			var invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger != LedgerTypes.AccountsPayable)
			{
				Assert("Only AP transactions can be saved as Incomplete Invoice", true);
				return;
			}

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
			invoice.RunPreSaveValidation();
			AssertNoErrors("Invoice is valid for saving", invoice);

			using (var testForm = GetFormByInvoice(invoice))
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.Show();

				invoice.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12);
				try
				{
					testForm.SaveAsIncomplete_ForTestOnly();
				}
				finally
				{
					invoice.Factory.ForceCriticalValidationErrorForTestOnly(CriticalValidationErrorType.NoError);
				}

				AssertEquals("Invoice should not be in Database", false, invoice.IsInDatabase);
				var expectedMessage = "Forced Critical Validation Error - For Test Only.";
				AssertEquals("Should give notification to user", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("ExceptionReporter should catch the critical validation error and raise 1 issues", 1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("ErrorReporter should report an issue to developers", expectedMessage, ExceptionReporterTestListener.Instance[0].InnerException.Message);

				AssertExceptionThrown<CannotSaveAfterCriticalErrorException>("Posting invoice after critical validation error on incomplete saving must be prevented.",
					() => testForm.ValidateAndSave_ForTestOnly());
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestTransactionCreationRestrictionPolicy_SavingFormAsIncomplete()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AssertNotNull(testObjectCreator.Creditor1.PK);
			Factory.Save();

			var invoice = GetInvoiceWithValidTestData();
			bool shouldContainError = true;
			if (invoice.AH_Ledger == LedgerTypes.IncompleteTransactions || invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				shouldContainError = false;
			}

			if (invoice is APInvoice || invoice is APCreditNote || invoice is APAdjustmentNote)
			{
				invoice.AH_OH = testObjectCreator.Creditor1.PK;
				using (var form = GetFormByInvoice(invoice))
				{
					UnitTestUserNotification.Instance.ClearMessages();
					testObjectCreator.Creditor1.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
					AssertNoExceptionThrown(() => form.SaveAsIncomplete_ForTestOnly());
					AssertContains(@"There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(shouldContainError, invoice.NotificationsIncludingChildren.ContainsNotificationContaining(@"transaction creation restriction policy set to ALL.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data."));

					UnitTestUserNotification.Instance.ClearMessages();
					testObjectCreator.Creditor1.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.Invoice;
					AssertNoExceptionThrown(() => form.SaveAsIncomplete_ForTestOnly());
					AssertContains(@"There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(shouldContainError, invoice.NotificationsIncludingChildren.ContainsNotificationContaining(@"transaction creation restriction policy set to INV.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data."));

					UnitTestUserNotification.Instance.ClearMessages();
					testObjectCreator.Creditor1.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.None;
					AssertNoExceptionThrown(() => form.SaveAsIncomplete_ForTestOnly());
					AssertContains(@"There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(false, invoice.NotificationsIncludingChildren.ContainsNotificationContaining(@"transaction creation restriction policy set to NON.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data."));
				}
			}
			else
			{
				Assert("Not Support", true);
			}
		}

		public void TestTransactionCreationRestrictionPolicy_Post()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			var invoice = GetInvoiceWithValidTestData();
			invoice.AH_OH = testObjectCreator.Creditor1.PK;
			Factory.Save();

			if (invoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				var controllerID = invoice.AH_TransactionType == TransactionTypes.IncompleteInvoice ? ControllerIDs.APIncompleteInvoice :
					invoice.AH_TransactionType == TransactionTypes.IncompleteCreditNote ? ControllerIDs.APIncompleteCreditNote : ControllerIDs.APIncompleteAdjustmentNote;
				ZController controller = ZControllerFactory.Create(controllerID);
				using (var form = (BaseInvoicingForm)controller.ShowEditForm(invoice))
				{
					var formInvoice = form.BusinessEntity as AccTransactionHeader;

					Assert("Pre Condition:", invoice.IsInDatabase);

					UnitTestUserNotification.Instance.ClearMessages();
					formInvoice.Header.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
					AssertNoExceptionThrown(() => form.FireSaveButton());
					AssertContains(@"There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(formInvoice.NotificationsIncludingChildren.ContainsNotificationContaining(@"transaction creation restriction policy set to ALL.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data."));

					UnitTestUserNotification.Instance.ClearMessages();
					formInvoice.Header.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.Invoice;
					AssertNoExceptionThrown(() => form.FireSaveButton());
					AssertContains(@"There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(formInvoice.NotificationsIncludingChildren.ContainsNotificationContaining(@"transaction creation restriction policy set to INV.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data."));

					UnitTestUserNotification.Instance.ClearMessages();
					formInvoice.Header.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.None;
					AssertNoExceptionThrown(() => form.FireSaveButton());
					AssertContains(@"There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(!formInvoice.NotificationsIncludingChildren.ContainsNotificationContaining(@"transaction creation restriction policy set to NON.
Transaction creation restriction policy is configured under Organization > A/R or A/P > Configuration > Company Data."));
				}
			}
			else
			{
				Assert("Not Support", true);
			}
		}

		public void TestUnrecoverableError_SavingFormAsIncomplete()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var invoice = GetInvoiceWithValidTestData();
			invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 10);
			invoice.SubmittedFromInvoicingForm = true;
			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();

				BusinessObjectFactory.SavingEventHandler preventSavingHandler = (factory) => { throw new InvalidOperationException("TestUnrecoverableError"); };
				InvalidOperationException exceptionCaught = null;
				try
				{
					invoice.RunPreSaveValidation();
					AssertNoErrors("Precondition: invoice", invoice);
					if (invoice is APInvoice || invoice is APCreditNote)
					{
						if (!invoice.IsIncompleteInvoice)
						{
							Assert("Precondition: ShouldRunJobChargeTransformer. Incomplete will be completed on from saving and transformer also will be run.", invoice.ShouldRunJobChargeTransformer);
						}
						Assert("Precondition: HasJobChargeTransformerBeenRun", !invoice.HasJobChargeTransformerBeenRun);
					}
					Factory.Saving += preventSavingHandler;
					form.ValidateAndSave_ForTestOnly();
				}
				catch (InvalidOperationException ex)
				{
					exceptionCaught = ex;
				}

				AssertNotNull("Precondition: saving exception", exceptionCaught);
				AssertEquals("Precondition: saving exception", "TestUnrecoverableError", exceptionCaught.Message);
				if (!invoice.IsCompletingInvoice)
				{
					Assert("Precondition: User now can edit it and save one more time.", !invoice.IsInDatabase);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Saving -= preventSavingHandler;
				if (invoice is APInvoice || invoice is APCreditNote)
				{
					if (invoice.AH_Ledger != LedgerTypes.UnapprovedPayableTransactions)
					{
						invoice.SaveAsIncomplete();
					}
					Assert("Precondition: HasJobChargeTransformerBeenRun", invoice.HasJobChargeTransformerBeenRun);
					AssertEquals("FireSaveButton", ContinueWithSave.No, form.FireSaveButton());
					Assert("Precondition: ShouldRunJobChargeTransformer", invoice.ShouldRunJobChargeTransformer);
					AssertEquals(@"The saving process has encountered an unrecoverable error. Please save this transaction as incomplete (Actions > Save as 'Incomplete') and post from the Incomplete Invoices module.",
						UnitTestUserNotification.Instance.LastMessage.Text);
					if (!invoice.IsCompletingInvoice)
					{
						Assert("IsInDatabase", !invoice.IsInDatabase);
					}
				}
				else
				{
					AssertEquals("FireSaveButton", ContinueWithSave.Yes, form.FireSaveButton());
					Assert("IsInDatabase", invoice.IsInDatabase);
				}

				form.Close();
			}
		}

		#endregion

		#region SourceXmlTab

		public void TestSourceXmlTabVisibility_Hidden()
		{
			var invoice = GetInvoiceWithValidTestData();
			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();

				var xmlTabControl = form.GetControl<ZTemplateTabControl>("MainTabControl", false);
				Assert("sourceXmlTabPage.TabVisible", !xmlTabControl.TabPages.ContainsKey("sourceXmlTabPage"));
			}
		}

		public void TestSourceXmlTabVisibility_Visible()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (!((invoice is APInvoice || invoice is APCreditNote) && invoice.AH_Ledger == LedgerTypes.AccountsPayable))
			{
				Assert("Functionality is only supported for AP Invoice and AP Credit Note.", true);
				return;
			}

			var isInvoiceTesting = invoice is APInvoice;
			var multiplier = isInvoiceTesting ? 1 : -1;
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 60 * multiplier);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction, GetUniversalTransactionXML(isInvoiceTesting), false);
			Factory.Save();
			var convertedInvoice = TransactionAllocationConverter.ConvertUnallocatedToAP(transaction).Invoice;
			AssertType("Precondition: we are testing expected invoice type.", invoice.GetType(), convertedInvoice);
			Assert("Precondition: HasUniversalTransaction", convertedInvoice.IsImportedFromUniversalXML);
			Action<string> assertTabVisibility = messagePrefix =>
			{
				using (var form = GetFormByInvoice(convertedInvoice))
				{
					form.Show();

					var xmlTabControl = form.GetControl<ZTemplateTabControl>("MainTabControl", false);
					Assert(messagePrefix + " sourceXmlTabPage.TabVisible", xmlTabControl.TabPages.ContainsKey("sourceXmlTabPage"));
					var sourceXmlTabPage = form.GetControl<ZTabPage>("sourceXmlTabPage");
					Assert(messagePrefix + " sourceXmlTabPage", sourceXmlTabPage.TabVisible);
				}
			};
			assertTabVisibility("Invoice allocation");

			convertedInvoice.SaveAsIncomplete();
			assertTabVisibility("Incomplete Invoice");

			convertedInvoice.MoveFromIncompleteToPayableLedger();
			assertTabVisibility("Invoice completing");

			Factory.Save();
			convertedInvoice = (InvoicingBase)new BusinessObjectFactory().Load(convertedInvoice.GetType(), convertedInvoice.PK);
			convertedInvoice.SubmittedFromInvoicingForm = true;
			assertTabVisibility("Posted Invoice");
		}

		string GetUniversalTransactionXML(bool forInvoice)
		{
			return string.Format(@"
<UniversalTransaction xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <TransactionInfo>
	<Branch>
	  <Code>BER</Code>
	</Branch>
	<Department>
	  <Code>BRN</Code>
	</Department>
	<Description>AP INVOICE</Description>
	<DueDate>2015-05-01T19:09:00</DueDate>
	<Ledger>AP</Ledger>
	<LocalExVATAmount>{0}60.0000</LocalExVATAmount>
	<Number>11112222</Number>
	<NumberOfSupportingDocuments>2</NumberOfSupportingDocuments>
	<OrganizationAddress>
	  <AddressType>None</AddressType>
	  <OrganizationCode>ABCFRESYD</OrganizationCode>
	</OrganizationAddress>
	<OSCurrency>
	  <Code>USD</Code>
	  <Description>United States Dollar</Description>
	</OSCurrency>
	<OSExGSTVATAmount>{0}120.0000</OSExGSTVATAmount>
	<PostDate>2015-04-30T19:09:00</PostDate>
	<TransactionDate>2015-04-28T19:09:00</TransactionDate>
	<TransactionType>{1}</TransactionType>

	<PostingJournalCollection>
	  <PostingJournal>
		<Branch>
		  <Code>SYD</Code>
		</Branch>
		<Department>
		  <Code>BRN</Code>
		</Department>
		<Description>FREIGHT REVENUE ACTUAL</Description>
		<GLAccount>
		  <AccountCode>1010.10.10</AccountCode>
		  <Description>FREIGHT REVENUE ACTUAL</Description>
		</GLAccount>
		<IsFinalCharge>true</IsFinalCharge>
		<LocalAmount>{0}60.0000</LocalAmount>
		<OSAmount>{0}120.00</OSAmount>
		<OSCurrency>
		  <Code>USD</Code>
		</OSCurrency>
		 <LocalCurrency>
		  <Code>AUD</Code>
		</LocalCurrency>
	   <Sequence>1</Sequence>
	  </PostingJournal>
	</PostingJournalCollection>
  </TransactionInfo>
</UniversalTransaction>",
			forInvoice ? "-" : "",
			forInvoice ? "INV" : "CRD");
		}

		#endregion

		public void TestAddressWithContactControlReadOnlyWhenWritablePropertiesAddedToTransaction()
		{
			using (var form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				var transaction = form.BusinessEntity as InvoicingBase;
				Assert("AddressWithContactControl should not be read-only", !form.InvoiceDetails.AddressWithContactControl.ReadOnly);
				transaction.AddWritableProperties(new string[] { "Property" });
				Assert("AddressWithContactControl should be read-only", form.InvoiceDetails.AddressWithContactControl.ReadOnly);
			}
		}

		public void TestShowJobChargesForImportEvent()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();

				InvoicingBase invoice = form.Invoice_ForTestOnly;

				if (invoice is APInvoice || invoice is APCreditNote)
				{
					var line = (InvoicingLineBase)invoice.Lines.AddNew();
					bool showJobChargesForImportEventWasRaised = false;
					line.ShowJobChargesForImportEvent += delegate
					{ showJobChargesForImportEventWasRaised = true; };
					line.AL_JH = TestObjectCreator.Job1.PK;

					Assert("ShowJobChargesForImportEvent should be raised", showJobChargesForImportEventWasRaised);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestShowJobChargesForImportEvent_ChargePopup()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();

				InvoicingBase invoice = form.Invoice_ForTestOnly;

				if (invoice is APInvoice || invoice is APCreditNote)
				{
					var line = (InvoicingLineBase)invoice.Lines.AddNew();
					bool showJobChargesForImportEventWasRaised = false;
					line.ShowJobChargesForImportEvent += delegate
					{ showJobChargesForImportEventWasRaised = true; };

					line.SuspendChargePopup();
					line.AL_JH = TestObjectCreator.Job1.PK;
					Assert("ShowJobChargesForImportEvent should not be raised", !showJobChargesForImportEventWasRaised);

					line.ResumeChargePopup();
					line.AL_JH = TestObjectCreator.Job2.PK;
					Assert("ShowJobChargesForImportEvent should be raised", showJobChargesForImportEventWasRaised);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestChargesReloadedWhenPostingInvoice()
		{
			BusinessObjectFactory jobFactory = new BusinessObjectFactory();
			// JobFactory.RefreshEnabled = false;

			TestObjectCreator jobCreator = new TestObjectCreator(jobFactory);
			ForwardingConsol consol = jobFactory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();

			Job shipmentJob = jobCreator.CreateJob(shipment);
			shipmentJob.JH_GB = GlbBranch.CurrentBranch.PK;
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Charge shipmentJobCharge1 = shipmentJob.Charges.AddNew();
			shipmentJobCharge1.JR_AC = jobCreator.CC1.PK;
			shipmentJobCharge1.JR_OSCostAmt = 150m;

			Charge shipmentJobCharge2 = shipmentJob.Charges.AddNew();
			shipmentJobCharge2.JR_AC = jobCreator.CC1.PK;
			shipmentJobCharge2.JR_OSCostAmt = 200m;

			shipmentJob.Factory.Save();

			ZQuery accrualsFilter = new ZQuery(AccTransactionLinesSchema.AL_LineType, ZArchitecture.Core.TransactionLineTypes.Accrual);
			accrualsFilter.OrderBy = AccTransactionLinesSchema.Constants.AL_LineAmount;
			WIPAccrualCollection accruals = new WIPAccrualCollection(jobFactory, accrualsFilter);
			accruals.Load();

			AssertEquals("Should be two accruals in the database", 2, accruals.Count);
			Accrual firstAccrual = (Accrual)accruals[0];
			AssertEquals("Accrual should be for 150", 150m, firstAccrual.AL_LineAmount);
			Accrual secondAccrual = (Accrual)accruals[1];
			AssertEquals("Accrual should be for 200", 200m, secondAccrual.AL_LineAmount);

			BusinessObjectFactory invoiceFactory = new BusinessObjectFactory();
			// InvoiceFactory.RefreshEnabled = false;

			TestObjectCreator invoiceCreator = new TestObjectCreator(invoiceFactory);
			InvoiceForConcurrencyTest = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(invoiceFactory);
			InvoiceForConcurrencyTest.SubmittedFromInvoicingForm = true;
			((APInvoiceLineCollection)InvoiceForConcurrencyTest.Lines).ShowJobChargesForImportEvent += TransactionLineJobChargeTransformerTest_ShowChargesForImportEvent;
			InvoiceForConcurrencyTest.AH_OH = invoiceCreator.AALSHI.PK;

			using (InvoiceForm form = new InvoiceForm(InvoiceForConcurrencyTest))
			{
				form.Show();

				InvoiceForConcurrencyTest = (APInvoice)form.Invoice_ForTestOnly;

				APInvoiceLine aPLine1 = (APInvoiceLine)InvoiceForConcurrencyTest.Lines.AddNew();
				aPLine1.AL_AC = shipmentJobCharge1.JR_AC;
				aPLine1.AL_JH = shipmentJob.PK;
				aPLine1.AL_OSExTaxAmount = 100m;

				APInvoiceLine aPLine2 = (APInvoiceLine)InvoiceForConcurrencyTest.Lines[1];
				aPLine2.AL_AC = shipmentJobCharge2.JR_AC;
				aPLine2.AL_JH = shipmentJob.PK;
				aPLine2.AL_OSExTaxAmount = 140m;

				shipmentJobCharge1.JR_OSCostAmt = 100m;
				shipmentJobCharge1.Factory.Save(); // Concurrency issue now - accrual imported into invoice was reversed

				accruals.Load();
				AssertEquals("Should be 3 accruals", 3, accruals.Count);
				AssertEquals("Should be two accruals that are reversed", 1, accruals.Find(new ZQuery(AccTransactionLinesSchema.AL_ReverseDate, SQLComparisonOperator.NotEqual, null)).Length);

				InvoiceForConcurrencyTest.Factory.Save();

				AssertEquals("Should be 3 charges", 3, shipmentJob.Charges.Count);

				Charge charge1 = (Charge)shipmentJob.Charges.Find(new ZQuery(JobChargeSchema.JR_AL_APLine, aPLine1.PK))[0];
				Charge charge2 = (Charge)shipmentJob.Charges.Find(new ZQuery(JobChargeSchema.JR_AL_APLine, aPLine2.PK))[0];

				AssertNotNull("Should have a line linked to invoice line 1", charge1);
				AssertNotNull("Should have a line linked to invoice line 2", charge2);

				ZQuery unPostedChargeFilter = new ZQuery(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, charge1.PK);
				unPostedChargeFilter.AddToFilter(JobChargeSchema.PK, SQLComparisonOperator.NotEqual, charge2.PK);

				Charge charge3 = (Charge)shipmentJob.Charges.Find(unPostedChargeFilter)[0];

				AssertNotNull("Unposted charge shouldn't be null", charge3);
			}
		}

		public virtual void TestDoNotCreateNewControllerWhenReopenWithCorrectBizOType()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var invoice = GetInvoiceWithValidTestData(true);
			invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 10);
			invoice.SubmittedFromInvoicingForm = true;

			using (var testForm = GetFormByInvoice(invoice))
			{
				testForm.Show();

				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition: ", invoice);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var postingButtonsUserControl = testForm.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl");
				postingButtonsUserControl.SaveButton.PerformClick();

				AssertEquals("form1: Should NOT create new ZController.", null, testForm.ReopenedControllerForTest);
			}
		}

		APInvoice InvoiceForConcurrencyTest;

		void TransactionLineJobChargeTransformerTest_ShowChargesForImportEvent(object sender, EventArgs e)
		{
			InvoiceForConcurrencyTest.ImportJobChargesIntoInvoice(new JobChargesImporter(InvoiceForConcurrencyTest.Lines[0]).JobChargesCollection.ToArray<Charge>(), (APInvoiceLine)InvoiceForConcurrencyTest.Lines[0]);
		}

		public void TestAmendingWithCreditNoteWhenUserHasNoSecurityRights()
		{
			var creator = new TestObjectCreator(Factory);
			var job = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "001");
			invoice.AH_OH = creator.AALSHI.PK;
			creator.CreateInvoiceLine(invoice, job, creator.FRT, 100m, creator.AUD, 1);
			creator.CreateCharge(invoice.Lines[0]);
			Factory.Save();

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;
			var originalAmending = invoice as IAmending;
			var amendment = originalAmending.GenerateAmendingTransaction(TransactionTypes.CreditNote) as BusinessObject;
			amendment.SetContext(BusinessContext.AmendingInvoice);

			var setting = new AuthorizationModeAndSettings();
			var collection = setting.AuthorisationSettings;
			var settings1 = collection.AddNew();
			settings1.Range = AmountBasedTwoLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings1.Amount = 1000.00m;
			settings1.AuthorisationRequirement = AmountBasedTwoLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			var settings2 = collection.AddNew();
			settings2.Range = AmountBasedTwoLevelAuthorisationRequirement.RangeCodes.Above;
			settings2.Amount = 1000.00m;
			settings2.AuthorisationRequirement = AmountBasedTwoLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), setting);

			using (var form = new BaseInvoicingForm((InvoicingBase)amendment))
			{
				AssertEquals("cannot continue save as it requires security override login", ContinueWithSave.No, form.ShowPreSaveDialogs_ForTestOnly());
				AssertEquals("Security Override Login", Enterprise.ZArchitecture.GUI.ZFormModaliser.LastFormShownDialogForTest.Text);
			}
		}

		public void TestAmendingWithInvoiceReversedInvoice()
		{
			AmendingReversedInvoice(TransactionTypes.Invoice);
		}

		public void TestAmendingWithCreditNoteReversedInvoice()
		{
			AmendingReversedInvoice(TransactionTypes.CreditNote);
		}

		void AmendingReversedInvoice(string amendWith)
		{
			var creator = new TestObjectCreator(Factory);
			var job = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "001");
			creator.CreateInvoiceLine(invoice, job, creator.FRT, 100m, creator.AUD, 1);
			creator.CreateCharge(invoice.Lines[0]);
			Factory.Save();

			var originalAmending = invoice as IAmending;

			var amendment = originalAmending.GenerateAmendingTransaction(amendWith) as BusinessObject;
			amendment.SetContext(BusinessContext.AmendingInvoice);

			var newFactory = new BusinessObjectFactory();
			var invoiceInNewFactory = newFactory.Load<ARInvoice>(invoice.PK);
			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoiceInNewFactory);

			reversing.Reverse();
			newFactory.Save();

			using (var form = new BaseInvoicingForm((InvoicingBase)amendment))
			{
				AssertEquals("cannot continue save as invoice is reversed", form.ShowPreSaveDialogs_ForTestOnly(), ContinueWithSave.No);
				Assert("Is Error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Cannot amend selected transaction as this has been reversed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSecurityOverrideProviderDoesNotResetWhileReversing()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_TransactionType == TransactionTypes.Invoice)
			{
				if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					Factory.Save();
					invoice.GenerateReverseTransaction(false);

					ZController controller = ZControllerFactory.Create(ControllerIDs.ARInvoice);
					using (var form = (BaseInvoicingForm)controller.ShowDeleteForm(invoice))
					{
						AssertNotEquals("AR invoice security override provider should not be DefaultAccessSecurityProvider",
							typeof(DefaultAccessSecurityProvider), SecurityOverrideProviderSource.Get(form.BusinessEntity).Provider.GetType());

						AssertEquals("AR invoice security override provider should be ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider",
							typeof(ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider), SecurityOverrideProviderSource.Get(form.BusinessEntity).Provider.GetType());
					}
				}
				else if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					Factory.Save();
					invoice.GenerateReverseTransaction(false);

					ZController controller = ZControllerFactory.Create(ControllerIDs.APInvoice);
					using (var form = (BaseInvoicingForm)controller.ShowDeleteForm(invoice))
					{
						AssertNotEquals("AP invoice security override provider should not be DefaultAccessSecurityProvider",
							typeof(DefaultAccessSecurityProvider), SecurityOverrideProviderSource.Get(form.BusinessEntity).Provider.GetType());

						AssertEquals("AR invoice security override provider should be InvoicingSecurityOverrideProvider",
							typeof(InvoicingSecurityOverrideProvider), SecurityOverrideProviderSource.Get(form.BusinessEntity).Provider.GetType());
					}
				}
				else
				{
					Assert(true);
				}
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2018, 01, 01)]
		public void TestSetInvoiceCreditNoteForAuthorisationCalculation()
		{
			InvoicingBase arCreditNote = TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
			using (var form = new BaseInvoicingForm(arCreditNote))
			{
				form.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals(1, ((ARCreditNote)arCreditNote).TransactionsForAuthorisationCalculation.Count);
			}

			InvoicingBase adjustmentNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("101", 90M, 5M, new ZDateTime(2008, 05, 15), TestObjectCreator.AALSHI.PK);
			using (var form = new BaseInvoicingForm(adjustmentNote))
			{
				form.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals(1, ((ARAdjustmentNote)adjustmentNote).TransactionsForAuthorisationCalculation.Count);
			}
		}

		[TestDate(2018, 01, 01)]
		public void TestCreateARCreditNoteWithSEQModeRequiresTwoApprover()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff.PK, true);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff.PK, false);

			var staff2 = TestObjectCreator.CreateStaffWithSecurityRights("newuser2", "ts2", Env.Security.APInvoiceApproval.Code, "password2", false);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff2.PK, true);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff2.PK, true);

			var staff3 = TestObjectCreator.CreateStaffWithSecurityRights("newuser3", "ts3", Env.Security.APInvoiceApproval.Code, "password3", false);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff3.PK, true);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff3.PK, true);

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("20001002"), false);
				job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;
				job.JH_GE = TestObjectCreator.FIADepartment.PK;
				var currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				var currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(currentBranch, currentDepartment, 100m, 200m, mode: "DEF");
				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(job.Branch.PK, job.Department.PK, 100m, 200m, mode: "SEQ");

				InvoicingBase arCreditNote = TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
				arCreditNote.AH_GB = GlbBranch.CurrentBranch.PK;
				arCreditNote.AH_GE = GlbDepartment.CurrentDepartment.PK;
				var arCrdLine = (ARCreditNoteLine)arCreditNote.Lines.AddNew();
				arCrdLine.AL_AC = TestObjectCreator.CC1.PK;
				arCrdLine.AL_JH = job.PK;
				arCrdLine.AL_GB = job.Branch.PK;
				arCrdLine.AL_GE = job.Department.PK;
				arCrdLine.AL_LineAmount = -250m;

				arCreditNote.AH_LocalExTaxAmount = 250m;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginFormWithTwoCredentialSupportBranchDepartmentLevel;
					if (loginForm != null)
					{
						loginForm.DoLogin1ForTest(staff.GS_LoginName, staff.StaffPlainTextPassword);
						loginForm.DoLogin2ForTest(staff2.GS_LoginName, staff2.StaffPlainTextPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
					}
				});
				using (var form = new BaseInvoicingForm(arCreditNote))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("cannot continue save as it requires security override login", ContinueWithSave.No, form.ShowPreSaveDialogs_ForTestOnly());
					AssertEquals("Security Override Login", ZFormModaliser.LastFormShownDialogForTest.Text);
					AssertEquals("The Authorizing user does not have security rights for this Branch or Department. Your system administrator maintains each user's security rights.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginFormWithTwoCredentialSupportBranchDepartmentLevel;
					if (loginForm != null)
					{
						loginForm.DoLogin1ForTest(staff2.GS_LoginName, staff2.StaffPlainTextPassword);
						loginForm.DoLogin2ForTest(staff3.GS_LoginName, staff3.StaffPlainTextPassword);
					}
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
				});
				using (var form = new BaseInvoicingForm(arCreditNote))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Expect call completed succeeded", ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[TestDate(2018, 01, 01)]
		public void TestPreSaveDialog_CheckCreditNoteSecurityRight()
		{
			bool originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			bool originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;

			try
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = true;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;
				var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("20001002"), false);
				TestObjectCreator.SetBranchDepartmentAuthorizationLevelSettings(job.Branch.PK, job.Department.PK, 100m, 200m);

				var arCreditNote = TestObjectCreator.CreateARCreditNote(TestObjectCreator.GetRandomString(3), TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 1M);
				arCreditNote.AH_GB = job.Branch.PK;
				arCreditNote.AH_GE = job.Department.PK;

				AssertPreSaveDialog_For_CreditNote(arCreditNote, 200M, ContinueWithSave.Yes, string.Empty);
				AssertPreSaveDialog_For_CreditNote(arCreditNote, 300M, ContinueWithSave.No, "Security Override Login");

				InvoicingBase arAdjustmentCreditNote = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("001", 90M, 5M, new ZDateTime(2008, 05, 15), TestObjectCreator.AALSHI.PK);
				arAdjustmentCreditNote.AH_GB = job.Branch.PK;
				arAdjustmentCreditNote.AH_GE = job.Department.PK;

				AssertPreSaveDialog_For_CreditNote(arAdjustmentCreditNote, -180M, ContinueWithSave.Yes, string.Empty);
				AssertPreSaveDialog_For_CreditNote(arAdjustmentCreditNote, -280M, ContinueWithSave.No, "Security Override Login");
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		void AssertPreSaveDialog_For_CreditNote(InvoicingBase invoicingBase, decimal amount, ContinueWithSave continueWithSave, string expectLastFormShownDialogForTest)
		{
			invoicingBase.AH_LocalExTaxAmount = amount;
			using (var form = new BaseInvoicingForm(invoicingBase))
			{
				AssertEquals("cannot continue save as it requires security override login", continueWithSave, form.ShowPreSaveDialogs_ForTestOnly());

				if (continueWithSave == ContinueWithSave.No)
				{
					AssertEquals(expectLastFormShownDialogForTest, Enterprise.ZArchitecture.GUI.ZFormModaliser.LastFormShownDialogForTest.Text);
				}
			}
		}

		public void TestShowPreSaveDialogsShowsLoginCompanyMissingTaxRegNumForSAFTWarning()
		{
			var creditNote1 = TestObjectCreator.CreateARCreditNote(200m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
			creditNote1.AH_GB = BranchSYD.PK;
			creditNote1.AH_GE = TestObjectCreator.FIADepartment.PK;
			TestObjectCreator.AddLineToCreditNote(creditNote1, ZGuid.Empty, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 50m);
			TestObjectCreator.AddLineToCreditNote(creditNote1, ZGuid.Empty, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 40m);
			TestObjectCreator.AddLineToCreditNote(creditNote1, ZGuid.Empty, BranchSYD.PK, TestObjectCreator.FIADepartment.PK, 110m);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			using (var form = new BaseInvoicingForm(creditNote1))
			{
				GlbCompany.CurrentCompany.GC_BusinessRegNo = ZString.Empty;
				var formResult = form.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals("Tax Registration Number of the Login Company is missing, this is mandatory for your country/region reporting.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
			}
		}

		public void TestShowPreSaveDialogsWithCreditNoteLineLevelAuthorisation()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);

			var creditNote1 = TestObjectCreator.CreateARCreditNote(-200m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
			creditNote1.AH_GB = BranchSYD.PK;
			creditNote1.AH_GE = TestObjectCreator.FIADepartment.PK;
			TestObjectCreator.AddLineToCreditNote(creditNote1, ZGuid.Empty, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 50m);
			TestObjectCreator.AddLineToCreditNote(creditNote1, ZGuid.Empty, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 40m);
			TestObjectCreator.AddLineToCreditNote(creditNote1, ZGuid.Empty, BranchSYD.PK, TestObjectCreator.FIADepartment.PK, 110m);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			using (var form = new BaseInvoicingForm(creditNote1))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), GetReceivableAuthorizationSettings(100m)))
			{
				var formResult = form.ShowPreSaveDialogs_ForTestOnly();

				AssertEquals(3, creditNote1.TransactionsForAuthorisationCalculation.Count);
				AssertEquals(2, creditNote1.TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation.Count);
				Assert(!creditNote1.LevelAuthorizationRequired);
			}

			using (var form = new BaseInvoicingForm(creditNote1))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, BranchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), GetReceivableAuthorizationSettings(80m)))
			{
				Guid currentDepartment = GlbDepartment.CurrentDepartment.PK.ToGuid();
				Guid currentBranch = GlbBranch.CurrentBranch.PK.ToGuid();
				TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), BranchSYD.PK.ToGuid(), staff.PK, false);
				Env.Security.ResetData(null, staff.PK.ToGuid(), BranchSYD.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), Env.CurrentCompanyPK, true);

				using (Env.SetTemporaryUserContext("newuser", BranchSYD.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid()))
				{
					var formResult = form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals("Should prompt login form", typeof(LoginFormForARCreditNoteApprovalOverride), ZFormModaliser.LastFormShownDialogForTest.GetType());

					AssertEquals(3, creditNote1.TransactionsForAuthorisationCalculation.Count);
					AssertEquals(2, creditNote1.TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation.Count);
					Assert(creditNote1.LevelAuthorizationRequired);
				}
			}

			var creditNote2 = TestObjectCreator.CreateARCreditNote(200m, TestObjectCreator.LocalClient.PK, ZGuid.Empty, ZGuid.Empty);
			creditNote2.AH_GB = BranchSYD.PK;
			creditNote2.AH_GE = TestObjectCreator.FIADepartment.PK;
			TestObjectCreator.AddLineToCreditNote(creditNote2, ZGuid.Empty, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 50m);
			TestObjectCreator.AddLineToCreditNote(creditNote2, ZGuid.Empty, BranchBNE.PK, TestObjectCreator.FIADepartment.PK, 40m);
			TestObjectCreator.AddLineToCreditNote(creditNote2, ZGuid.Empty, BranchSYD.PK, TestObjectCreator.FISDepartment.PK, 110m);
			TestObjectCreator.AddLineToCreditNote(creditNote2, ZGuid.Empty, BranchTES.PK, TestObjectCreator.FISDepartment.PK, 50m);
			TestObjectCreator.AddLineToCreditNote(creditNote2, ZGuid.Empty, BranchTES.PK, TestObjectCreator.FIADepartment.PK, 100m);

			using (var form = new BaseInvoicingForm(creditNote2))
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, BranchTES.PK.ToGuid(), TestObjectCreator.FISDepartment.PK.ToGuid(), GetReceivableAuthorizationSettings(60m)))
			{
				var formResult = form.ShowPreSaveDialogs_ForTestOnly();
				AssertEquals("Should prompt login form", typeof(LoginFormForARCreditNoteApprovalOverride), ZFormModaliser.LastFormShownDialogForTest.GetType());

				AssertEquals(5, creditNote2.TransactionsForAuthorisationCalculation.Count);
				AssertEquals(5, creditNote2.TransactionsWithUniqueBranchDepartmentForAuthorisationCalculation.Count);
				Assert(!creditNote2.LevelAuthorizationRequired);
			}
		}

		AuthorizationModeAndSettings GetReceivableAuthorizationSettings(decimal amount)
		{
			var setting = new AuthorizationModeAndSettings();
			var valuesForTest = setting.AuthorisationSettings;
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = amount;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = amount;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = PaymentAuthorisationSettings.RangeCodes.Above;

			return setting;
		}

		GlbBranch BranchBNE => Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
		GlbBranch BranchSYD => Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SYD"));
		GlbBranch BranchTES => Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "TES"));

		public void TestTransactionLinesGrid_RowDeleting()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.Shipments.AddNew();

			factory.Save();

			var inv = factory.NewWithValidTestData<APInvoice>();
			inv.AH_OH = TestObjectCreator.AALSHI.PK;

			try
			{
				using (var invForm = new InvoiceForm(inv))
				{
					invForm.Show();
					invForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					var cost = inv.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
					cost.E6_AC_ChargeCode = creator.CC12.PK;
					cost.E6_OSCostAmount = 100M;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					apportionmentForm.Close();

					invForm.InvoiceDetails.TransactionLinesGrid.SelectAllElements();
					AssertNotNull("ApportionmentChargeImportedFrom Should be not null", ((InvoicingLineBase)invForm.InvoiceDetails.TransactionLinesGrid.GetFirstSelectedRow()).ApportionmentChargeImportedFrom);

					invForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;

					cost.Delete();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					apportionmentForm.Close();
					Application.DoEvents();
					invForm.InvoiceDetails.TransactionLinesGrid.Select(0);
					AssertEquals("Should be empty in TransactionLinesGrid", 1, invForm.InvoiceDetails.TransactionLinesGrid.SelectedRowCount);
					AssertNoExceptionThrown("Should be no Exception to delete.", delegate
					{
						TestKeyStrokeHelper.SendKeyToControl(invForm.InvoiceDetails.TransactionLinesGrid, Keys.Delete, true);
					});
				}
			}
			finally
			{
				inv.ClearApportionmentJobMutexes();
			}
		}

		public void TestTransactionLinesGrid_RowDeletingCatchesException()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			factory.Save();

			var inv = factory.NewWithValidTestData<APInvoice>();
			inv.AH_OH = TestObjectCreator.AALSHI.PK;

			using (var invForm = new InvoiceForm(inv))
			{
				invForm.Show();
				invForm.InvoiceDetails.ApportionChargesButton.PerformClick();
				AssertEquals("Should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
				var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
				var cost = inv.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = creator.CC12.PK;
				cost.E6_OSCostAmount = 100M;
				cost.E6_AH_APInvoice = inv.PK;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 30M;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 30M;
				cost.ApportionmentCharges[2].JR_OSCostAmt = 40M;
				apportionmentForm.Close();

				invForm.ThrowExceptionOnRowDelete_ForTestOnly = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				invForm.InvoiceDetails.TransactionLinesGrid.Select(0);
				TestKeyStrokeHelper.SendKeyToControl(invForm.InvoiceDetails.TransactionLinesGrid, Keys.Delete, true);
				Assert("Is Error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Row Delete Error", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertContains("ApportionmentChargeLinkedToInvoiceLineDeleted", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestTransactionLinesGrid_RowDeletingChecksForValidCharges()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol1 = factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol1.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment = consol1.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;

			var consol2 = factory.NewWithValidTestData<ForwardingConsol>();
			shipment = consol2.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment = consol2.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			shipment = consol2.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;

			factory.Save();

			var inv = factory.NewWithValidTestData<APInvoice>();
			inv.AH_OH = TestObjectCreator.AALSHI.PK;

			try
			{
				using (var invForm = new InvoiceForm(inv))
				{
					invForm.Show();
					invForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					var cost1_1 = inv.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol1);
					cost1_1.E6_AC_ChargeCode = creator.CC12.PK;
					cost1_1.E6_OSCostAmount = 100M;
					var cost2_1 = inv.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol2);
					cost2_1.E6_AC_ChargeCode = creator.CC12.PK;
					cost2_1.E6_OSCostAmount = 200M;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					apportionmentForm.Close();
					Application.DoEvents();

					invForm.InvoiceDetails.TransactionLinesGrid.SelectAllElements();
					AssertNotNull("ApportionmentChargeImportedFrom Should not be null", ((InvoicingLineBase)invForm.InvoiceDetails.TransactionLinesGrid.GetFirstSelectedRow()).ApportionmentChargeImportedFrom);

					TestKeyStrokeHelper.SendKeyToControl(invForm.InvoiceDetails.TransactionLinesGrid, Keys.Delete, true);

					AssertNull("Lines should all be deleted", ((InvoicingLineBase)invForm.InvoiceDetails.TransactionLinesGrid.GetFirstSelectedRow()));
					Assert("Should not still have reporting suspended after delete", !inv.IsReportingDeletedApportionmentChargesSuspended);

					invForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the apportionment form", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					JobConsolCost cost1_2 = inv.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol1);
					cost1_2.E6_AC_ChargeCode = creator.CC12.PK;
					cost1_2.E6_OSCostAmount = 100M;
					JobConsolCost cost2_2 = inv.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol2);
					cost2_2.E6_AC_ChargeCode = creator.CC12.PK;
					cost2_2.E6_OSCostAmount = 200M;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					apportionmentForm.Close();

					invForm.InvoiceDetails.TransactionLinesGrid.SelectAllElements();
					AssertNotNull("ApportionmentChargeImportedFrom Should not be null", ((InvoicingLineBase)invForm.InvoiceDetails.TransactionLinesGrid.GetFirstSelectedRow()).ApportionmentChargeImportedFrom);

					var firstLinePK1_2 = inv.Lines.First(x => ((InvoicingLineBase)x).ImportedApportionmentID == cost1_2.PK);
					var firstLinePK2_2 = inv.Lines.First(x => ((InvoicingLineBase)x).ImportedApportionmentID == cost2_2.PK);
					var rows = invForm.InvoiceDetails.TransactionLinesGrid.SelectedElements;
					for (int i = 4; i > 0; i--)
					{
						if (rows[i]?.PK != firstLinePK1_2.PK && rows[i]?.PK != firstLinePK2_2.PK)
						{
							invForm.InvoiceDetails.TransactionLinesGrid.UnSelect(i);
						}
					}

					Application.DoEvents();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					TestKeyStrokeHelper.SendKeyToControl(invForm.InvoiceDetails.TransactionLinesGrid, Keys.Delete, true);
					Application.DoEvents();
					AssertEquals("Should show message saying that all apportionment related lines must be selected",
						"This line relates to an apportionment of cost. All lines relating to this apportionment must also be deleted. Would you like to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Should show question to user with yes and no buttons", UnitTestUserNotification.Instance.LastMessage.WasQuestion);

					invForm.InvoiceDetails.TransactionLinesGrid.UnSelectAll();
					invForm.InvoiceDetails.TransactionLinesGrid.Select(0);
					AssertNull("Lines should all be deleted", ((InvoicingLineBase)invForm.InvoiceDetails.TransactionLinesGrid.GetFirstSelectedRow()));
					Assert("Should not still have reporting suspended after delete", !inv.IsReportingDeletedApportionmentChargesSuspended);

					AssertNoExceptionThrown("Should be not Exception to delete.",
						() => TestKeyStrokeHelper.SendKeyToControl(invForm.InvoiceDetails.TransactionLinesGrid, Keys.Delete, true));
				}
			}
			finally
			{
				inv.ClearApportionmentJobMutexes();
			}
		}

		public void TestTransactionLinesGrid_RowDeletedShouldNotThrowException()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var shipment = creator.CreateShipment("S00001");
			var job = creator.CreateJob(shipment);
			var charge = creator.CreateCharge(job, creator.FRT, 100m, 1000m);
			factory.Save();

			var invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			using (var form = new InvoiceForm(invoice))
			{
				form.Show();

				var invoicingLine = (APInvoiceLine)invoice.Lines.AddNew();
				invoicingLine.AL_AC = TestObjectCreator.CC1.PK;
				invoicingLine.AL_JH = job.PK;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				form.InvoiceDetails.TransactionLinesGrid.Select(0);
				AssertNoExceptionThrown("Should not throw Null Reference Exception", () => TestKeyStrokeHelper.SendKeyToControl(form.InvoiceDetails.TransactionLinesGrid, Keys.Delete, true));
			}
		}

		public void TestTransactionLinesGrid_RowDeletedShouldNotThrowException2()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol = creator.CreateConsol();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			factory.Save();

			var invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = creator.AALSHI.PK;

			using (var form = new InvoiceForm(invoice))
			{
				form.Show();

				form.InvoiceDetails.ApportionChargesButton.PerformClick();
				var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
				JobConsolCost cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = creator.CC12.PK;
				cost.E6_OSCostAmount = 100M;
				cost.E6_AH_APInvoice = invoice.PK;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 50M;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 50M;
				apportionmentForm.Close();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				form.InvoiceDetails.TransactionLinesGrid.Select(0);
				TestKeyStrokeHelper.SendKeyToControl(form.InvoiceDetails.TransactionLinesGrid, Keys.Delete, true);

				var line = (APInvoiceLine)invoice.Lines.AddNew();
				line.AL_AC = creator.CC1.PK;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				form.InvoiceDetails.TransactionLinesGrid.Select(0);
				TestKeyStrokeHelper.SendKeyToControl(form.InvoiceDetails.TransactionLinesGrid, Keys.Delete, true);

				form.InvoiceDetails.ApportionChargesButton.PerformClick();
				var apportionmentForm1 = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
				JobConsolCost cost1 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost1.E6_AC_ChargeCode = creator.CC12.PK;
				cost1.E6_OSCostAmount = 100M;
				cost1.E6_AH_APInvoice = invoice.PK;
				cost1.ApportionmentCharges[0].JR_OSCostAmt = 50M;
				cost1.ApportionmentCharges[1].JR_OSCostAmt = 50M;
				apportionmentForm1.Close();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				form.InvoiceDetails.TransactionLinesGrid.Select(0);
				AssertNoExceptionThrown("Should not throw exception.", () => TestKeyStrokeHelper.SendKeyToControl(form.InvoiceDetails.TransactionLinesGrid, Keys.Delete, true));
			}
		}

		public void TestTransactionLinesGrid_RowDeletedShouldNotThrowException3()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol = creator.CreateConsol();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			factory.Save();

			var invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = creator.AALSHI.PK;

			using (var form = new InvoiceForm(invoice))
			{
				form.Show();

				form.InvoiceDetails.ApportionChargesButton.PerformClick();
				var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
				JobConsolCost cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = creator.CC12.PK;
				cost.E6_OSCostAmount = 100M;
				cost.E6_AH_APInvoice = invoice.PK;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 50M;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 50M;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				apportionmentForm.Close();

				form.InvoiceDetails.TransactionLinesGrid.Select(0);
				MenuItem menuItem = form.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
				menuItem.PerformClick();

				var singleConsolCostingEditForm = (APInvoiceConsolCostingSingleEditForm)ZFormModaliser.ActiveForm;
				var jobConsolCost = (JobConsolCost)singleConsolCostingEditForm.BusinessEntity;
				jobConsolCost.E6_OSCostAmount = 50M;
				jobConsolCost.RefreshBinding();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Application.DoEvents();
				singleConsolCostingEditForm.Close();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				singleConsolCostingEditForm.Close();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				form.InvoiceDetails.TransactionLinesGrid.Select(0);
				AssertNoExceptionThrown("Should not throw exception.", () => TestKeyStrokeHelper.SendKeyToControl(form.InvoiceDetails.TransactionLinesGrid, Keys.Delete, true));
			}
		}

		public void TestDeleteAPInCompleteInvoiceShouldNotThrowException()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol = creator.CreateConsol();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			factory.Save();

			var invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = creator.AALSHI.PK;

			using (var form = new InvoiceForm(invoice))
			{
				form.Show();

				form.InvoiceDetails.ApportionChargesButton.PerformClick();
				var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
				JobConsolCost cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = creator.CC12.PK;
				cost.E6_OSCostAmount = 100M;
				cost.E6_AH_APInvoice = invoice.PK;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 50M;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 50M;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				apportionmentForm.Close();

				invoice.AH_TransactionType = TransactionTypes.IncompleteInvoice;
				invoice.SaveAsIncomplete();
			}

			var controller = ZControllerFactory.Create(ControllerIDs.APIncompleteInvoice);
			using (var deleteForm = controller.ShowDeleteForm(invoice))
			{
				deleteForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //"You are about to delete this record permanently from the system. Do you want to proceed?"

				AssertNoExceptionThrown(() => ((IPostingButtonsProvider)deleteForm).CommandButtonPost.PerformClick());
				AssertNull("Invoice should be deleted and so nothing to load.", new BusinessObjectFactory().Load<APInvoice>(invoice.PK));
			}
		}

		public void TestSaveAPInvoiceInCompleteInvoiceShouldNotDisposeJob()
		{
			AssertSaveInCompleteInvoiceShouldNotDisposeJob<APInvoice>();
		}

		public void TestSaveAPCreditNoteInCompleteInvoiceShouldNotDisposeJob()
		{
			AssertSaveInCompleteInvoiceShouldNotDisposeJob<APCreditNote>();
		}

		void AssertSaveInCompleteInvoiceShouldNotDisposeJob<T>() where T : InvoicingBase
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol = creator.CreateConsol();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			factory.Save();

			var invoice = factory.NewWithValidTestData<T>();
			invoice.AH_OH = creator.AALSHI.PK;

			var newFactory = new BusinessObjectFactory();
			Job newJob1;
			Job newJob2;

			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();

				form.InvoiceDetails.ApportionChargesButton.PerformClick();
				var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = creator.CC12.PK;
				cost.E6_OSCostAmount = 100M;
				cost.E6_AH_APInvoice = invoice.PK;
				cost.ApportionmentCharges[0].JR_OSCostAmt = 50M;
				cost.ApportionmentCharges[1].JR_OSCostAmt = 50M;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				apportionmentForm.Close();

				invoice.AH_TransactionType = TransactionTypes.IncompleteInvoice;
				invoice.SaveAsIncomplete();

				newJob1 = new Job.Loader(newFactory, consol.Shipments[0]).TryCreateWithMutex();
				newJob2 = new Job.Loader(newFactory, consol.Shipments[1]).TryCreateWithMutex();
				AssertNull(newJob1);
				AssertNull(newJob2);
			}

			newJob1 = new Job.Loader(newFactory, consol.Shipments[0]).TryCreateWithMutex();
			newJob2 = new Job.Loader(newFactory, consol.Shipments[1]).TryCreateWithMutex();
			AssertNotNull(newJob1);
			AssertNotNull(newJob2);
			newJob1.Delete();
			newJob2.Delete();
		}

		public void TestStoreErrorMessageWhenComplianceNumberExceedsLengthLimit()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.VietNam);

			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var item = collection.AddNew();
			item.Country = Core.Constants.CountryCodes.VietNam;
			item.SubType = "TXI";
			item.LedgerType = "AR";
			item.InvoiceType = "INV";
			item.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT;
			item.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			item.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			item.OrganisationLocation = "";

			AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);

			var configurationCollection = ComplianceNumberSequenceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
			AccountingMasterFilesRegistry.Instance.ComplianceNumberSequenceConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationCollection);
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(newFactory);
			var menuPK = newFactory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "ARInvoice PE Factura")).PK;
			var sequence = testObjectCreator.CreateNewComplianceSequence(menuPK, "TXI", 1, 100, 25);
			sequence.XD_Prefix = "TEST";
			sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
			sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
			sequence.XD_MaximumNumberDigits = 9;
			sequence.XD_NumberFormat = "CCC";
			newFactory.Save();

			var invoice = GetInvoiceWithValidTestData();
			invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
			invoice.AH_Desc = "test";
			invoice.AH_TransactionReference = "";
			invoice.AH_ComplianceSubType = "TXI";
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.GenericCharge = TestObjectCreator.ExchangeGainLossControlAccount.PK;
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_OSExTaxAmount = 5;
			Factory.Save();

			if (invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				var converter = new UnapprovedTransactionConverter(Factory);
				invoice = converter.ConvertToAP(invoice, false);
			}

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();

				form.PostingButtonsUserControl.SaveButton.PerformClick();

				var expectedMessage = "The length of generated compliance number('TXI     TEST000000025') has exceeded the total length of 20 characters.\r\n Please update the compliance invoice book with a number format that will not exceed the length limit.";
				AssertEquals(expectedMessage, invoice.EventArgsForCompliance.complianceSequenceRelatedException.UserFriendlyMessage);
				ZApplication.GetOpenForms()[0]?.Close();
			}
		}

		public void TestBaseInvoicingFormConstructorSetAH_PostedToEFTBeforeSetEventHandlers()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_ExchangeRate = 1.5;
			invoice.IsConvertedFromARInvoice = true;

			APInvoiceLine invoicingLine = (APInvoiceLine)invoice.Lines.AddNew();
			invoicingLine.AL_ExchangeRate = 2.3;
			invoicingLine.ApportionmentChargeImportedFrom = Factory.NewWithValidTestData<ApportionSplitCharge>();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
			{
				AssertEquals("No message should be shown", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHideColumnsForNonGstRegisteredCompany()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = "USD";
				Application.DoEvents();
				AssertEquals(true, form.AH_OSTaxAmountCalcEdit.Visible);
				AssertEquals(true, form.AH_LocalTaxAmountCalcEdit.Visible);
				AssertEquals(true, form.GetControl<ZCalcFindBox>("OSSubTotalExTaxAmountCalcEdit", true).Visible);
				AssertEquals(true, form.GetControl<ZCalcFindBox>("LocalSubTotalExTaxAmountCalcEdit", true).Visible);
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSGSTAmount).IsUnavailable);
				if (form.Invoice_ForTestOnly.AH_Ledger == "AP")
				{
					if (form.Invoice_ForTestOnly is APAdjustmentNote)
					{
						AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle("GSTInclusiveAmount"));
					}
					else
					{
						AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle("GSTInclusiveAmount").IsUnavailable);
					}
				}
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalGSTAmount).IsUnavailable);
				AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalTaxAmount).IsUnavailable);
				AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalTotalAmount).IsUnavailable);
				AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OverseasTotal).IsUnavailable);

				if (form.InvoiceDetails.JobSummaryTabPage.TabVisible)
				{
					var tabControl = form.InvoiceDetails.GetControl<ZTabControl>("TabControl");
					tabControl.SelectedIndex = 1;
					AssertEquals(false, form.InvoiceDetails.JobSummaryGrid.GetColumnStyle("JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency").IsUnavailable);
					AssertEquals(false, form.InvoiceDetails.JobSummaryGrid.GetColumnStyle("JH_RelatedInvoiceLinesCostTaxAmount").IsUnavailable);
					AssertEquals(false, form.InvoiceDetails.JobSummaryGrid.GetColumnStyle("JH_RelatedInvoiceLinesTotalCost").IsUnavailable);
					AssertEquals(false, form.InvoiceDetails.JobSummaryGrid.GetColumnStyle("JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency").IsUnavailable);
				}
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = "USD";
				Application.DoEvents();
				AssertEquals(false, form.AH_OSTaxAmountCalcEdit.Visible);
				AssertEquals(false, form.AH_LocalTaxAmountCalcEdit.Visible);
				AssertEquals(false, form.GetControl<ZCalcFindBox>("OSSubTotalExTaxAmountCalcEdit", true).Visible);
				AssertEquals(false, form.GetControl<ZCalcFindBox>("LocalSubTotalExTaxAmountCalcEdit", true).Visible);
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSGSTAmount));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle("GSTInclusiveAmount"));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalGSTAmount));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalTaxAmount));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalTotalAmount));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OverseasTotal));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_AT));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_TaxDate));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_A9_VATClass));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_GB_TaxBranch));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.TaxBranchName));

				if (form.InvoiceDetails.JobSummaryTabPage.TabVisible)
				{
					var tabControl = form.InvoiceDetails.GetControl<ZTabControl>("TabControl");
					tabControl.SelectedIndex = 1;
					AssertNull(form.InvoiceDetails.JobSummaryGrid.GetColumnStyle("JH_RelatedInvoiceLinesTotalCostInInvoiceCurrency"));
					AssertNull(form.InvoiceDetails.JobSummaryGrid.GetColumnStyle("JH_RelatedInvoiceLinesCostTaxAmount"));
					AssertNull(form.InvoiceDetails.JobSummaryGrid.GetColumnStyle("JH_RelatedInvoiceLinesTotalCost"));
					AssertNull(form.InvoiceDetails.JobSummaryGrid.GetColumnStyle("JH_RelatedInvoiceLinesCostTaxAmountInInvoiceCurrency"));
				}
			}
		}

		public void TestGovernmentAllocatedIDFieldVisibility()
		{
			var invoice = GetInvoiceWithValidTestData(true);

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable
				|| invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions
				|| invoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				foreach (var value in new[] { false, true })
				{
					AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, value);
					AssertResult(value);
				}
			}
			else
			{
				AssertResult(false);
			}

			void AssertResult(bool expectVisible)
			{
				using (var form = new BaseInvoicingForm(invoice))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals("Govt ID field is visible only for AP, UA and IN ledger invoice when registry is enabled",
						expectVisible, form.InvoiceDetails.GovernmentAllocatedIDTextBox.Visible);
				}
			}
		}

		public void TestSourceReferenceFieldVisibility()
		{
			var invoice = GetInvoiceWithValidTestData(true);

			if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable
				&& (invoice.AH_TransactionType == TransactionTypes.Invoice
				|| invoice.AH_TransactionType == TransactionTypes.CreditNote))
			{
				AssertResult(false);
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
				{
					AssertResult(true);
				}
			}
			else
			{
				AssertResult(false);
			}

			void AssertResult(bool expectVisible)
			{
				using (var form = new BaseInvoicingForm(invoice))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals("Source Ref field is visible only for AR INV and AR CRD When country is Portugal",
						expectVisible, form.InvoiceDetails.SourceReferenceTextBox.Visible);
				}
			}
		}

		InvoicingBase CreateInvoiceForTest(InvoicingBase invoice, bool isTaxIncluded)
		{
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, invoice.TransactionCurrency, invoice.AH_ExchangeRate, 80m, 0m, 0m, 80m, 0m, 0m);
			if (isTaxIncluded)
			{
				var gST1 = TestObjectCreator.CreateTaxRate("GST1", "GST Rate 1", AccTaxRate.Types.Rated, 10, string.Empty, 0, 1);
				line1.AL_AT = gST1.PK;
			}
			return invoice;
		}

		void AssertTaxRelatedFieldsandColumnsDisplay(InvoicingBase invoice, bool hasTaxLines)
		{
			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = "USD";
				Application.DoEvents();

				AssertEquals(hasTaxLines, form.AH_OSTaxAmountCalcEdit.Visible);
				AssertEquals(hasTaxLines, form.AH_LocalTaxAmountCalcEdit.Visible);
				AssertEquals(hasTaxLines, form.GetControl<ZCalcFindBox>("OSSubTotalExTaxAmountCalcEdit", true).Visible);
				AssertEquals(hasTaxLines, form.GetControl<ZCalcFindBox>("LocalSubTotalExTaxAmountCalcEdit", true).Visible);

				var linesGrid = form.InvoiceDetails.TransactionLinesGrid;

				if (form.Invoice_ForTestOnly.AH_Ledger == "AP")
				{
					AssertEquals(hasTaxLines, linesGrid.Columns.Contains(InvoiceLine.Schema.AL_OSTaxAmount_Recoverable));
					AssertEquals(hasTaxLines, linesGrid.Columns.Contains(InvoiceLine.Schema.AL_OSTaxAmount_NotRecoverable));
					AssertEquals(hasTaxLines, linesGrid.Columns.Contains(InvoiceLine.Schema.AL_LocalTaxAmount_Recoverable));
					AssertEquals(hasTaxLines, linesGrid.Columns.Contains(InvoiceLine.Schema.AL_LocalTaxAmount_NotRecoverable));
				}
				else
				{
					AssertEquals(false, linesGrid.Columns.Contains(InvoiceLine.Schema.AL_OSTaxAmount_Recoverable));
					AssertEquals(false, linesGrid.Columns.Contains(InvoiceLine.Schema.AL_OSTaxAmount_NotRecoverable));
					AssertEquals(false, linesGrid.Columns.Contains(InvoiceLine.Schema.AL_LocalTaxAmount_Recoverable));
					AssertEquals(false, linesGrid.Columns.Contains(InvoiceLine.Schema.AL_LocalTaxAmount_NotRecoverable));
				}

				AssertEquals(hasTaxLines, linesGrid.Columns.Contains("TaxReportingBasisHumanReadableName"));
				if (hasTaxLines)
				{
					AssertEquals(false, linesGrid.GetColumnStyle(InvoiceLine.Schema.AL_AT).IsUnavailable);
					AssertEquals(false, linesGrid.GetColumnStyle(InvoiceLine.Schema.AL_TaxDate).IsUnavailable);
					AssertEquals(false, linesGrid.GetColumnStyle(InvoiceLine.Schema.AL_A9_VATClass).IsUnavailable);
					AssertEquals(false, linesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSTaxAmount).IsUnavailable);
					AssertEquals(false, linesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalTaxAmount).IsUnavailable);
				}
				else
				{
					AssertNull(linesGrid.GetColumnStyle(InvoiceLine.Schema.AL_AT));
					AssertNull(linesGrid.GetColumnStyle(InvoiceLine.Schema.AL_TaxDate));
					AssertNull(linesGrid.GetColumnStyle(InvoiceLine.Schema.AL_A9_VATClass));
					AssertNull(linesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSTaxAmount));
					AssertNull(linesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalTaxAmount));
				}
			}
		}

		public void TestTaxRelatedFieldsandColumnsDisplayForPostedInvoiceNotHavingTaxLines()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			var aPInv = CreateInvoiceForTest(Factory.NewWithValidTestData<APInvoice>(), false);
			var aPCNote = CreateInvoiceForTest(Factory.NewWithValidTestData<APCreditNote>(), false);
			var aPAdjust = CreateInvoiceForTest(Factory.NewWithValidTestData<APAdjustmentNote>(), false);
			var aRInv = CreateInvoiceForTest(Factory.NewWithValidTestData<ARInvoice>(), false);
			var aRCNote = CreateInvoiceForTest(Factory.NewWithValidTestData<ARCreditNote>(), false);
			var aRAdjust = CreateInvoiceForTest(Factory.NewWithValidTestData<ARAdjustmentNote>(), false);

			Factory.Save();

			AssertTaxRelatedFieldsandColumnsDisplay(aPInv, false);
			AssertTaxRelatedFieldsandColumnsDisplay(aPCNote, false);
			AssertTaxRelatedFieldsandColumnsDisplay(aPAdjust, false);
			AssertTaxRelatedFieldsandColumnsDisplay(aRInv, false);
			AssertTaxRelatedFieldsandColumnsDisplay(aRCNote, false);
			AssertTaxRelatedFieldsandColumnsDisplay(aRAdjust, false);
		}

		public void TestTaxRelatedFieldsandColumnsDisplayForPostedInvoiceHavingTaxLines()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var aPInv = CreateInvoiceForTest(Factory.NewWithValidTestData<APInvoice>(), true);
			var aPCNote = CreateInvoiceForTest(Factory.NewWithValidTestData<APCreditNote>(), true);
			var aPAdjust = CreateInvoiceForTest(Factory.NewWithValidTestData<APAdjustmentNote>(), true);
			var aRInv = CreateInvoiceForTest(Factory.NewWithValidTestData<ARInvoice>(), true);
			var aRCNote = CreateInvoiceForTest(Factory.NewWithValidTestData<ARCreditNote>(), true);
			var aRAdjust = CreateInvoiceForTest(Factory.NewWithValidTestData<ARAdjustmentNote>(), true);

			Factory.Save();

			AssertTaxRelatedFieldsandColumnsDisplay(aPInv, true);
			AssertTaxRelatedFieldsandColumnsDisplay(aPCNote, true);
			AssertTaxRelatedFieldsandColumnsDisplay(aPAdjust, true);
			AssertTaxRelatedFieldsandColumnsDisplay(aRInv, true);
			AssertTaxRelatedFieldsandColumnsDisplay(aRCNote, true);
			AssertTaxRelatedFieldsandColumnsDisplay(aRAdjust, true);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			AssertTaxRelatedFieldsandColumnsDisplay(aPInv, true);
			AssertTaxRelatedFieldsandColumnsDisplay(aPCNote, true);
			AssertTaxRelatedFieldsandColumnsDisplay(aPAdjust, true);
			AssertTaxRelatedFieldsandColumnsDisplay(aRInv, true);
			AssertTaxRelatedFieldsandColumnsDisplay(aRCNote, true);
			AssertTaxRelatedFieldsandColumnsDisplay(aRAdjust, true);
		}

		public void TestCloseFormIfAnotherAlreadyOpened()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				ControllerID normalID = null;
				ControllerID incompleteID = null;
				var message = string.Empty;

				if (invoice is APInvoice)
				{
					normalID = ControllerIDs.APInvoice;
					incompleteID = ControllerIDs.APIncompleteInvoice;
					message = "This Invoice is open in another form. Please close that form before continuing.";
				}
				else if (invoice is APCreditNote)
				{
					normalID = ControllerIDs.APCreditNote;
					incompleteID = ControllerIDs.APIncompleteCreditNote;
					message = "This Credit Note is open in another form. Please close that form before continuing.";
				}
				else if (invoice is APAdjustmentNote)
				{
					normalID = ControllerIDs.APAdjustmentNote;
					incompleteID = ControllerIDs.APIncompleteAdjustmentNote;
					invoice = Factory.NewWithValidTestData<APAdjustmentNote>();
					message = "This Adjustment Note is open in another form. Please close that form before continuing.";
				}
				else
				{
					throw new Exception("you might need to adjust this UT for new classes.");
				}
				ZController controller = ZControllerFactory.Create(normalID);
				using (BaseInvoicingForm normalForm = (BaseInvoicingForm)controller.ShowFormForNewEntity(invoice))
				{
					normalForm.Show();
					invoice.SaveAsIncomplete();
					controller = ZControllerFactory.Create(incompleteID);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var formCache = OpenedFormCache.GetInstance();
					Assert(string.Format("Form with Key '{0}' should Exist", invoice.PK.ToGuid() + normalID.Name), formCache.FormCache.ContainsKey(invoice.PK.ToGuid() + normalID.Name));

					using (BaseInvoicingForm formIncomplete = (BaseInvoicingForm)controller.ShowEditForm(invoice))
					{
						formIncomplete.Show();
						Assert("Should be the same form", ReferenceEquals(formIncomplete, normalForm));
						Assert(string.Format("Form with Key '{0}' should Exist", invoice.PK.ToGuid() + normalID.Name), formCache.FormCache.ContainsKey(invoice.PK.ToGuid() + incompleteID.Name));
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDoNotAskToSaveAfterSavingAsIncomplete()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger != LedgerTypes.AccountsPayable)
			{
				Assert("Can't be saved as incomplete", true);
				return;
			}

			ZController controller = null;
			if (invoice is APInvoice)
			{
				controller = ZControllerFactory.Create(ControllerIDs.APInvoice);
			}
			else if (invoice is APCreditNote)
			{
				controller = ZControllerFactory.Create(ControllerIDs.APCreditNote);
			}
			else if (invoice is APAdjustmentNote)
			{
				controller = ZControllerFactory.Create(ControllerIDs.APAdjustmentNote);
			}

			using (var formIncomplete = (BaseInvoicingForm)controller.ShowFormForNewEntity(invoice))
			{
				formIncomplete.Show();
				Application.DoEvents();

				invoice.SaveAsIncomplete();
				invoice.AH_Desc = "test";
				Assert("Precondition: HasChanges", invoice.HasChanges);
				formIncomplete.Close();
				Assert("Must ask Save or Not", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			invoice = GetInvoiceWithValidTestData();
			using (var formIncomplete = (BaseInvoicingForm)controller.ShowFormForNewEntity(invoice))
			{
				formIncomplete.Show();
				Application.DoEvents();

				invoice.SaveAsIncomplete();
				Assert("Precondition: HasChanges", !invoice.HasChanges);
				Assert("Must not ask Save or Not", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestViewJobDetails()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			var invoice = GetInvoiceWithValidTestData();
			invoice.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			invoice.Lines.AddNew();

			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job.PK;

			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull("View Job Details item must be in list", form.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("View Job Details"));

				form.ViewJobDetails_ForTestOnly(null, null);
				AssertEquals("Please select an invoice line.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.InvoiceDetails.TransactionLinesGrid.Select(0);
				form.ViewJobDetails_ForTestOnly(null, null);
				AssertEquals("This invoice line doesn't have a job.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.InvoiceDetails.TransactionLinesGrid.UnSelectAll();
				form.InvoiceDetails.TransactionLinesGrid.Select(1);
				form.ViewJobDetails_ForTestOnly(null, null);
				AssertEquals("You cannot view job details when the transaction is from another company.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHasClosedJob()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			InvoicingBase invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_TransactionType == TransactionTypes.Invoice || invoice.AH_TransactionType == TransactionTypes.CreditNote
				|| invoice.AH_TransactionType == TransactionTypes.UAInvoice || invoice.AH_TransactionType == TransactionTypes.UACreditNote)
			{
				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_JH = job.PK;

				using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
				{
					Assert(!form.HasClosedJob_ForTestOnly);
					job.JH_Status = JobHeaderStatus.Closed.Code;
					Assert(form.HasClosedJob_ForTestOnly);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestShouldReopenJob()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			InvoicingBase invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_TransactionType == TransactionTypes.Invoice || invoice.AH_TransactionType == TransactionTypes.CreditNote
				|| invoice.AH_TransactionType == TransactionTypes.UAInvoice || invoice.AH_TransactionType == TransactionTypes.UACreditNote)
			{
				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_JH = job.PK;
				TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

				using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
				{
					form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
					Factory.Save();
					job.JH_Status = JobHeaderStatus.Closed.Code;
					form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals(JobHeaderStatus.Closed.Code, job.JH_Status);
				}

				job = Factory.NewJobWithValidTestDataForTesting<Job>();
				job.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();
				invoice = Factory.NewWithValidTestData<APInvoice>();
				line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_JH = job.PK;
				TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

				((IReversing)invoice).GenerateReverseTransaction(false);

				InvoicingBase reversal = invoice.ReverseTransaction as InvoicingBase;
				reversal.AH_TransactionNum = "S0000001";
				AssertNotNull(reversal);
				TestObjectCreator.CreateJobCharge(reversal.Lines[0], job, TestObjectCreator.CC1, TestObjectCreator.AUD);

				using (BaseInvoicingForm form = new BaseInvoicingForm(reversal))
				{
					job.JH_Status = JobHeaderStatus.Closed.Code;
					form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
					Factory.Save();
					job.JH_Status = JobHeaderStatus.Closed.Code;
					form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals(JobHeaderStatus.Closed.Code, job.JH_Status);
				}

				job = Factory.NewJobWithValidTestDataForTesting<Job>();
				job.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();
				invoice = Factory.NewWithValidTestData<UAInvoice>();
				line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_JH = job.PK;
				TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

				using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
				{
					job.JH_Status = JobHeaderStatus.Closed.Code;
					form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
					Factory.Save();
					job.JH_Status = JobHeaderStatus.Closed.Code;
					form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals(JobHeaderStatus.Closed.Code, job.JH_Status);

					invoice.AH_Ledger = "AP";
					invoice.AH_TransactionType = "INV";
					line.AL_LineType = "CST";
					form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
				}

				job = Factory.NewJobWithValidTestDataForTesting<Job>();
				job.JH_Status = JobHeaderStatus.Closed.Code;
				Factory.Save();
				invoice = Factory.NewWithValidTestData<APInvoice>();
				line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.AL_JH = job.PK;

				using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
				{
					job.JH_Status = JobHeaderStatus.Closed.Code;
					form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
					invoice.AH_Ledger = LedgerTypes.IncompleteTransactions;
					invoice.AH_TransactionType = TransactionTypes.IncompleteInvoice;
					Factory.Save();
					job.JH_Status = JobHeaderStatus.Closed.Code;
					form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals(JobHeaderStatus.Closed.Code, job.JH_Status);

					invoice.AH_Ledger = LedgerTypes.AccountsPayable;
					invoice.AH_TransactionType = TransactionTypes.Invoice;
					form.ShowPreSaveDialogs_ForTestOnly();
					AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDefaultExpectedTotalValueIsDefaultedFromRegistry()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger != LedgerTypes.IncompleteTransactions)
			{
				foreach (bool value in new[] { false, true })
				{
					AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
					invoice.SetIsValidationOfValidateExpectedInvoiceTotalEnabled_ForTestOnly(false);

					using (new BaseInvoicingForm(invoice))
					{
						if (!value)
						{
							Assert("Should always be false", !invoice.ValidateExpectedInvoiceTotal);
						}
						else
						{
							Assert("Should depend on transaction type", invoice.ValidateExpectedInvoiceTotal == (invoice is APInvoice || invoice is APCreditNote || invoice is APAdjustmentNote));
						}
					}
				}
			}

			invoice.ValidateExpectedInvoiceTotal = false;
			Factory.Save();

			foreach (bool value in new[] { false, true })
			{
				AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

				using (new BaseInvoicingForm(invoice))
				{
					Assert("Should always be false", !invoice.ValidateExpectedInvoiceTotal);
				}
			}
		}

		public virtual void TestAH_PostedToEFTIsNotDefaultedFromRegistry()
		{
			InvoicingBase invoice = null;

			foreach (bool value in new[] { false, true })
			{
				AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

				invoice = GetInvoiceWithValidTestData();

				using (new BaseInvoicingForm(invoice))
				{
					AssertEquals("Precondition, new invoice always defaults to local currency", Env.CurrentCompany.LocalCurrency.Code, invoice.AH_RX_NKTransactionCurrency);
					Assert("Should always be false, regardless of registry setting, due to local currency.", !invoice.AH_PostedToEFT);
				}
			}

			invoice.AH_PostedToEFT = false;
			Factory.Save();

			foreach (bool value in new[] { false, true })
			{
				AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);

				using (new BaseInvoicingForm(invoice))
				{
					Assert("Should always be false", !invoice.AH_PostedToEFT);
				}
			}
		}

		public void TestExpectedTotalCheckBoxVisibilityWhenDefaultJobExchangeSetToYes()
		{
			AccountingConfigurationRegistry.Instance.UseJobExchangeRateDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			APInvoice invoice = Factory.New<APInvoice>();

			using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();
				Assert(form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
				Assert(form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
			}
		}

		public void TestExpectedTotalCheckBoxVisibilityWhenIsImportedFromFile()
		{
			bool originalValue = AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				InvoicingBase invoice = GetInvoiceWithValidTestData();
				invoice.IsImportedFromFile = true;

				using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();
					bool expected = invoice.AH_Ledger != LedgerTypes.AccountsReceivable;
					AssertEquals("InvoiceDetails.InvoiceTotalValidationCheckBox.Visible", expected, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
					AssertEquals("InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible", expected, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
				}
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestGSTInclusiveAmountsCheckBoxVisibility()
		{
			bool expected = false;
			AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingBase invoice = GetInvoiceWithValidTestData(false);

			using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();
				if (invoice.IsAPInvoiceOrCreditNote || invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					expected = true;
				}
				AssertEquals("InvoiceDetails.InvoiceTotalValidationCheckBox.Visible", expected, form.InvoiceDetails.GSTInclusiveAmountsCheckBox.Visible);
			}

			expected = false;
			invoice.IsConvertedFromARInvoice = true;
			using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();
				if (invoice.IsAPInvoiceOrCreditNote && invoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					expected = true;
				}
				AssertEquals("InvoiceDetails.InvoiceTotalValidationCheckBox.Visible", expected, form.InvoiceDetails.GSTInclusiveAmountsCheckBox.Visible);
			}
		}

		public void TestExpectedTotalCheckBoxVisibilityWhenIsConvertedFromARInvoice()
		{
			bool originalValue = AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				InvoicingBase invoice = GetInvoiceWithValidTestData();
				invoice.IsConvertedFromARInvoice = true;

				using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();
					bool expected = invoice.AH_Ledger != LedgerTypes.AccountsReceivable;
					AssertEquals("InvoiceDetails.InvoiceTotalValidationCheckBox.Visible", expected, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
					AssertEquals("InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible", expected, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
				}
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestExpectedTotalWhenAllocatingInvoicePendingAllocation()
		{
			bool originalValue = AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				TransactionPendingAllocation invoicePendingAllocation = new BusinessObjectFactory().New<TransactionPendingAllocation>();
				invoicePendingAllocation.AH_TransactionNum = "ABC123";
				invoicePendingAllocation.AH_OH = TestObjectCreator.Creditor1.PK;
				invoicePendingAllocation.AH_DueDate = ZDateTime.Now;
				invoicePendingAllocation.AH_OSExTaxAmount = 1000m;
				invoicePendingAllocation.Factory.Save();

				InvoicingBase invoice = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;

				using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();
					bool expected = invoice.AH_Ledger != LedgerTypes.AccountsReceivable;
					AssertEquals("InvoiceDetails.InvoiceTotalValidationCheckBox.Visible", expected, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
					AssertEquals("InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible", expected, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
					AssertEquals("ExpectedTotal should be 1000", 1000m, invoice.ExpectedInvoiceTotal);
				}
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestOverrideExchangeRateControlsVisibility_HasSecurity()
			=> TestOverrideExchangeRateControlsVisibilityCore(true);

		public void TestOverrideExchangeRateControlsVisibility_WithoutSecurity()
			=> TestOverrideExchangeRateControlsVisibilityCore(false);

		void TestOverrideExchangeRateControlsVisibilityCore(bool newPayablesOverridePostingExchangeRateAllows)
		{
			AssertEquals("PreCondition, Local Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestObjectCreator.AUD.Code);
			Env.Security.NewPayablesOverridePostingExchangeRateAllows.IsAllowed = newPayablesOverridePostingExchangeRateAllows;

			var invoice = GetInvoiceWithValidTestData();
			if (ShouldSupportOverrideExRateCheckbox)
			{
				invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
				AssertCase("When ledger type supports overriding ex rate, ExRateOption is DEF and invoice is Local Currency, then UseJobExRate should be visible and override ex rate checkbox should not be visible."
					, ExRateOption.Default.Code
					, expectedOverrideExRateVisiable: false
					, expectedOverrideExRateCaption: string.Empty
					, expectedUseJobExRateCheckBoxVisiable: true);

				if (newPayablesOverridePostingExchangeRateAllows)
				{
					foreach (var exRateOption in ExRateOption.CodeList.GetAllCodes().Except(ExRateOption.Default.Code))
					{
						invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
						AssertCase("When ledger type supports overriding ex rate, PayablesOverridePostingExchangeRate is allowed, ExRateOption is not DEF and invoice is Local Currency, then UseJobExRate should not be visible and override ex rate checkbox should be visible with the caption 'Override Line Ex. Rate'."
							, exRateOption
							, expectedOverrideExRateVisiable: true
							, expectedOverrideExRateCaption: "Override Line Ex. Rate"
							, expectedUseJobExRateCheckBoxVisiable: false);

						invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
						AssertCase("When ledger type supports overriding ex rate, PayablesOverridePostingExchangeRate is allowed, ExRateOption is not DEF and invoice is Foreign Currency, then UseJobExRate should be visible and override ex rate checkbox should be visible too with the caption 'Override'."
							, exRateOption
							, expectedOverrideExRateVisiable: true
							, expectedOverrideExRateCaption: "Override"
							, expectedUseJobExRateCheckBoxVisiable: true);
					}
				}
				else
				{
					foreach (var exRateOption in ExRateOption.CodeList.GetAllCodes().Except(ExRateOption.Default.Code))
					{
						invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
						AssertCase("When ledger type supports overriding ex rate, PayablesOverridePostingExchangeRate is not allowed, ExRateOption is not DEF and invoice is Local Currency, then UseJobExRate should not be visible and override ex rate checkbox should not be visible."
							, exRateOption
							, expectedOverrideExRateVisiable: false
							, expectedOverrideExRateCaption: "Override Line Ex. Rate"
							, expectedUseJobExRateCheckBoxVisiable: true);

						invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
						AssertCase("When ledger type supports overriding ex rate, PayablesOverridePostingExchangeRate is not allowed, ExRateOption is not DEF and invoice is Foreign Currency, then UseJobExRate should be visible and override ex rate checkbox should not be visible."
							, exRateOption
							, expectedOverrideExRateVisiable: false
							, expectedOverrideExRateCaption: "Override"
							, expectedUseJobExRateCheckBoxVisiable: true);
					}
				}
			}
			else
			{
				foreach (var exRateOption in ExRateOption.CodeList.GetAllCodes())
				{
					invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
					AssertCase("When ledger type does not support overriding ex rate, then override ex rate and use job ex rate checkbox should not be visible."
						, exRateOption
						, expectedOverrideExRateVisiable: false
						, expectedOverrideExRateCaption: string.Empty
						, expectedUseJobExRateCheckBoxVisiable: false);

					invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
					AssertCase("When ledger type does not support overriding ex rate, then override ex rate and use job ex rate checkbox should not be visible."
						, exRateOption
						, expectedOverrideExRateVisiable: false
						, expectedOverrideExRateCaption: string.Empty
						, expectedUseJobExRateCheckBoxVisiable: false);
				}
			}

			void AssertCase(string comment, string invoicePostingExchangeRateOptionAP, bool expectedOverrideExRateVisiable, string expectedOverrideExRateCaption, bool expectedUseJobExRateCheckBoxVisiable)
			{
				AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, invoicePostingExchangeRateOptionAP);

				using (var form = new BaseInvoicingForm(invoice))
				{
					form.Show();
					Application.DoEvents();

					CombineAssertions($"Transaction Type:{invoice.AH_TransactionType}: {comment}"
						, () =>
						{
							AssertEquals("Override Ex Rate Check Box visibility", expectedOverrideExRateVisiable, form.InvoiceDetails.OverrideJobExRateCheckBox.Visible);
							AssertEquals("Use Job Ex Rate Check Box visibility", expectedUseJobExRateCheckBoxVisiable, form.InvoiceDetails.UseJobExRateCheckBox.Visible);

							if (expectedOverrideExRateVisiable)
							{
								AssertEquals("Override Ex Rate Caption", expectedOverrideExRateCaption, form.InvoiceDetails.OverrideJobExRateCheckBox.GetExtension<LabelCaptionRenderer>().Caption);
							}
						});
				}
			}
		}

		protected virtual bool ShouldSupportOverrideExRateCheckbox => false;

		public void TestBulkChargeImportButtonVisibility()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice is APInvoice || testInvoice is APCreditNote)
			{
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					Assert("Should be visible", testForm.InvoiceDetails.BulkChargeImportButton.Visible);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestBulkChargeImportButtonVisibleForIncompleteInvoicesInDb()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice is APInvoice)
			{
				testInvoice.AH_TransactionType = TransactionTypes.IncompleteInvoice;
				testInvoice.SaveAsIncomplete();
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					Application.DoEvents();
					Assert("Should be visible", testForm.InvoiceDetails.BulkChargeImportButton.Visible);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestBulkChargeImportButtonVisibleForApprovingInvoices()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();
			APInvoice invoice = testInvoice as APInvoice;
			if (invoice != null)
			{
				invoice = ConvertToApprovingInvoice(invoice);
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
				{
					testForm.Show();
					Application.DoEvents();
					Assert("Bulk charge import button should NOT be visible for an approving invocie", !testForm.InvoiceDetails.BulkChargeImportButton.Visible);
				}
			}
			else
			{
				Assert(true);
			}
		}

		APInvoice ConvertToApprovingInvoice(APInvoice invoice)
		{
			invoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			invoice.AH_TransactionType = TransactionTypes.UAInvoice;
			Factory.Save();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			Assert("With Ledger UA in db and AP in businesss layer Invoice should be approving", invoice.IsApprovingInvoice);
			return invoice;
		}

		public void TestInvoiceRemittanceReferenceEditableForAllocatingInvoices()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();
			var invoice = testInvoice as APInvoice;
			if (invoice != null && !invoice.IsInDatabase)
			{
				invoice.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
				invoice.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
				Factory.Save();
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
				{
					testForm.Show();
					Application.DoEvents();
					Assert("Invoice Remittance Reference field should be editable for an allocating invocie", !invoice.InvoiceRemittanceReferenceInfo.ReadOnly);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestAutoAllocateDiscrepancyMenuItemEnabledForAllocateTransactions()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			TransactionPendingAllocation invoicePendingAllocation = Factory.New<TransactionPendingAllocation>();
			invoicePendingAllocation.AH_TransactionNum = "invoice";
			invoicePendingAllocation.AH_OH = org.PK;
			invoicePendingAllocation.AH_OSExTaxAmount = 100m;
			Factory.Save();
			InvoicingBase transaction = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			using (BaseInvoicingForm testForm = new BaseInvoicingForm(transaction))
			{
				testForm.Show();
				Application.DoEvents();
				Assert("Should be enabled", testForm.AutoAllocateDiscrepancyMenuItem.Enabled);
			}
		}

		public void TestAutoAllocateDiscrepancyMenuItemEnabledForIncompleteTransactions()
		{
			SetUp();

			InvoicingBase testInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", TestObjectCreator.AUD, 1.0m, 20m, 0.0m, 20m, 0.0m);

			testInvoice.SaveAsIncomplete();

			AssertEquals("The invoice AH_Ledger type should be incomplete (\"IN\")", LedgerTypes.IncompleteTransactions, testInvoice.AH_Ledger);

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
			{
				testForm.Show();
				Application.DoEvents();
				AssertEquals("The AutoAllocatedDescrepancy menu item should be enabled for transactions marked as incomplete", true, testForm.AutoAllocateDiscrepancyMenuItem.Enabled);
			}
		}

		#region Tests for AutoAllocateDiscrepancy dependancy on HasAnyActiveAccTaxConfiguration

		[ExpectNoExceptions]
		public virtual void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithValidLedgerWhenInvoiceLedgerIsValid()
		{
			AssertAutoAllocateDiscrepancy_Usability(
				(_, _) => { },
				(ledger, mockITaxFrameworkConfigurationHelper) =>
				{
					var expectedLedger = ledger == LedgerTypes.IncompleteTransactions ? LedgerTypes.AccountsPayable : ledger;
					mockITaxFrameworkConfigurationHelper.Verify(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), expectedLedger));
				});
		}

		[ExpectNoExceptions]
		public virtual void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithInvoiceCompany()
		{
			var newCompany = TestObjectCreator.CreateNewCompany("XYZ");
			Factory.Save();

			AssertAutoAllocateDiscrepancy_Usability(
				(_, invoice) => invoice.AH_GC = newCompany.PK,
				(_, mockITaxFrameworkConfigurationHelper) => mockITaxFrameworkConfigurationHelper.Verify(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), newCompany, It.IsAny<ZString>())));
		}

		[ExpectNoExceptions]
		public virtual void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithInvoiceFactory()
		{
			var newFactory = new BusinessObjectFactory();

			AssertAutoAllocateDiscrepancy_Usability(
				(_, _) => { },
				(_, mockITaxFrameworkConfigurationHelper) => mockITaxFrameworkConfigurationHelper.Verify(x => x.HasAnyActiveAccTaxConfiguration(newFactory, It.IsAny<GlbCompany>(), It.IsAny<ZString>())),
				newFactory);
		}

		public virtual void TestAutoAllocateDiscrepancy_IsAllowed_WhenNoTaxConfigExistsForTheLedger
			()
		{
			AssertAutoAllocateDiscrepancy_Usability(
				(mockITaxFrameworkConfigurationHelper, _) => mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(false),
				(ledger, _) => AssertNotEquals($"Tax framework configuration IS present for {ledger} ledger", AutoAllocateDiscrepancyDisabledForTaxConfigCase(ledger), UnitTestUserNotification.Instance.LastMessage.Text));
		}

		public virtual void TestAutoAllocateDiscrepancy_IsNotAllowed_WhenTaxConfigExistsForTheLedger()
		{
			AssertAutoAllocateDiscrepancy_Usability(
				(mockITaxFrameworkConfigurationHelper, _) => mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true),
				(ledger, _) => AssertEquals($"Tax framework configurationn IS NOT present for {ledger} ledger", AutoAllocateDiscrepancyDisabledForTaxConfigCase(ledger), UnitTestUserNotification.Instance.LastMessage.Text));
		}

		string AutoAllocateDiscrepancyDisabledForTaxConfigCase(string ledger) => $"Auto-allocate Discrepancy feature is disabled because the login company has one or more active {ledger} Tax Framework Configurations.";

		protected void AssertAutoAllocateDiscrepancy_Usability(Action<Mock<ITaxFrameworkConfigurationHelper>, InvoicingBase> testSpecificSetup, Action<string, Mock<ITaxFrameworkConfigurationHelper>> testSpecificAssert, BusinessObjectFactory factory = null)
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();

			var invoice = GetInvoiceWithValidTestData(fillTestData: false, factory: factory);
			invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
			invoice.AH_TransactionNum = "TEST101";
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			testSpecificSetup(mockITaxFrameworkConfigurationHelper, invoice);

			using (var testForm = new BaseInvoicingForm(invoice))
			{
				testForm.Show();

				testForm.AutoAllocateDiscrepancyMenuItem.PerformClick();
				testSpecificAssert(invoice.AH_Ledger, mockITaxFrameworkConfigurationHelper);
			}
		}

		#endregion

		public void TestExpectedInvoiceTotalControlsShownForIncompleteInvoice()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice.AH_TransactionType == TransactionTypes.Invoice)
			{
				invoice.SaveAsIncomplete();

				using (BaseInvoicingForm form = GetFormByInvoice(invoice))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals("InvoiceTotalValidationCheckBox.Visible", true, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
					AssertEquals("ValidInvoiceTotalCalcEdit.Visible", true, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestExpectedInvoiceTotalControlsForIncompleteInvoiceWithTaxApplicable()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice.AH_TransactionType == TransactionTypes.Invoice)
			{
				OrgHeader orgHeader = TestObjectCreator.AALSHI;
				orgHeader.CompanyData.SetAPTaxApplicable(true);
				invoice.AH_OH = orgHeader.PK;
				invoice.ExpectedInvoiceTotal = 220m;
				invoice.ExpectedInvoiceTaxTotal = 20m;
				invoice.ExpectedInvoiceExclTaxTotal = 200m;
				invoice.ValidateExpectedInvoiceTotal = true;

				invoice.SaveAsIncomplete();

				using (BaseInvoicingForm form = GetFormByInvoice(invoice))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals("InvoiceTotalValidationCheckBox.Visible", true, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
					AssertEquals("ValidInvoiceTotalCalcEdit.Visible", true, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
					AssertEquals("ExpectedExclTaxCalcEdit.Visible", true, form.InvoiceDetails.ExpectedExclTaxCalcEdit.Visible);
					AssertEquals("ExpectedTaxCalcEdit.Visible", true, form.InvoiceDetails.ExpectedTaxCalcEdit.Visible);

					var tabControl = form.GetControl<ZTabControl>("TotalAndUnallocatedTabControl", true);
					tabControl.SelectedTab = form.GetControl<ZTabPage>("UnallocatedTabPage", true);
					AssertEquals("ExcludingTaxPanel.Visible", true, form.GetControl<ZPanel>("ExcludingTaxPanel", true).Visible);
					AssertEquals("TaxTotalPanel.Visible", true, form.GetControl<ZPanel>("TaxTotalPanel", true).Visible);

					AssertEquals("ExpectedInvoiceTotal", 220m, invoice.ExpectedInvoiceTotal);
					AssertEquals("ExpectedInvoiceTaxTotal", 20m, invoice.ExpectedInvoiceTaxTotal);
					AssertEquals("ExpectedInvoiceExclTaxTotal", 200m, invoice.ExpectedInvoiceExclTaxTotal);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestExpectedInvoiceTotalControlsForImportedInvoiceWithTaxApplicable()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice.AH_TransactionType == TransactionTypes.Invoice)
			{
				bool originalValue = AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.Value;

				try
				{
					AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					OrgHeader orgHeader = TestObjectCreator.AALSHI;
					orgHeader.CompanyData.SetAPTaxApplicable(true);

					invoice.AH_OH = orgHeader.PK;
					invoice.IsImportedFromFile = true;

					InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
					line.AL_AT = TestObjectCreator.GST1.PK;
					line.AL_OSExTaxAmount = 2000m;
					line.AL_OSTaxAmount = 200m;
					line.AL_OSAmount = 2200m;

					AssertEquals("AH_OSTotalAmount", 2200m, invoice.AH_OSTotalAmount);
					AssertEquals("IsExpectedTaxTotalVisible", true, invoice.IsExpectedTaxTotalVisible);

					using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
					{
						form.DisplayMode = ODisplayMode.Edit;
						form.Show();
						Application.DoEvents();

						AssertEquals("InvoiceDetails.InvoiceTotalValidationCheckBox.Visible", true, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
						AssertEquals("InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible", true, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
						AssertEquals("InvoiceDetails.ExpectedTaxCalcEdit.Visible", true, form.InvoiceDetails.ExpectedTaxCalcEdit.Visible);
						AssertEquals("InvoiceDetails.ExpectedExclTaxCalcEdit.Visible", true, form.InvoiceDetails.ExpectedExclTaxCalcEdit.Visible);

						var tabControl = form.GetControl<ZTabControl>("TotalAndUnallocatedTabControl", true);
						tabControl.SelectedTab = form.GetControl<ZTabPage>("UnallocatedTabPage", true);
						AssertEquals("ExcludingTaxPanel.Visible", true, form.GetControl<ZPanel>("ExcludingTaxPanel", true).Visible);
						AssertEquals("TaxTotalPanel.Visible", true, form.GetControl<ZPanel>("TaxTotalPanel", true).Visible);

						AssertEquals("ExpectedInvoice_ForTestOnlyTaxTotal", 200m, form.Invoice_ForTestOnly.ExpectedInvoiceTaxTotal);
						AssertEquals("ExpectedInvoice_ForTestOnlyExclTaxTotal", 2000m, form.Invoice_ForTestOnly.ExpectedInvoiceExclTaxTotal);
						AssertEquals("ExpectedInvoice_ForTestOnlyTotal", 2200m, form.Invoice_ForTestOnly.ExpectedInvoiceTotal);
					}
				}
				finally
				{
					AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestExpectedInvoiceTotalControlsForImportedInvoiceWithNoTaxApplicable()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice.AH_TransactionType == TransactionTypes.Invoice)
			{
				bool originalValue = AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.Value;

				try
				{
					AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					OrgHeader orgHeader = TestObjectCreator.AALSHI;
					orgHeader.CompanyData.SetAPTaxApplicable(false);

					invoice.AH_OH = orgHeader.PK;
					invoice.IsImportedFromFile = true;

					InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
					line.AL_OSExTaxAmount = 2000m;
					line.AL_OSAmount = 2000m;

					AssertEquals("AH_OSTotalAmount", 2000m, invoice.AH_OSTotalAmount);
					AssertEquals("IsExpectedTaxTotalVisible", false, invoice.IsExpectedTaxTotalVisible);

					using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
					{
						form.DisplayMode = ODisplayMode.Edit;
						form.Show();
						Application.DoEvents();

						AssertEquals("InvoiceDetails.InvoiceTotalValidationCheckBox.Visible", true, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
						AssertEquals("InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible", true, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
						AssertEquals("InvoiceDetails.ExpectedTaxCalcEdit.Visible", false, form.InvoiceDetails.ExpectedTaxCalcEdit.Visible);
						AssertEquals("InvoiceDetails.ExpectedExclTaxCalcEdit.Visible", false, form.InvoiceDetails.ExpectedExclTaxCalcEdit.Visible);
						AssertEquals("ExcludingTaxPanel.Visible", false, form.GetControl<ZPanel>("ExcludingTaxPanel", true).Visible);
						AssertEquals("TaxTotalPanel.Visible", false, form.GetControl<ZPanel>("TaxTotalPanel", true).Visible);

						AssertEquals("ExpectedInvoice_ForTestOnlyTaxTotal", 0m, form.Invoice_ForTestOnly.ExpectedInvoiceTaxTotal);
						AssertEquals("ExpectedInvoice_ForTestOnlyExclTaxTotal", 0m, form.Invoice_ForTestOnly.ExpectedInvoiceExclTaxTotal);
						AssertEquals("ExpectedInvoice_ForTestOnlyTotal", 2000m, form.Invoice_ForTestOnly.ExpectedInvoiceTotal);
					}
				}
				finally
				{
					AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestExpectedInvoiceTotalControlsForInvoiceShouldNotShow()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice.AH_TransactionType == TransactionTypes.Invoice)
			{
				bool originalValue = AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.Value;

				try
				{
					AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					OrgHeader orgHeader = TestObjectCreator.AALSHI;
					orgHeader.CompanyData.SetAPTaxApplicable(true);

					invoice.AH_OH = orgHeader.PK;

					using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
					{
						form.Show();
						form.DisplayMode = ODisplayMode.Browse;
						Application.DoEvents();

						AssertEquals("Precondition: Form display mode", ODisplayMode.Browse, form.DisplayMode);
						AssertEquals("InvoiceDetails.InvoiceTotalValidationCheckBox.Visible", false, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
						AssertEquals("InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible", false, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
						AssertEquals("InvoiceDetails.ExpectedTaxCalcEdit.Visible", false, form.InvoiceDetails.ExpectedTaxCalcEdit.Visible);
						AssertEquals("InvoiceDetails.ExpectedExclTaxCalcEdit.Visible", false, form.InvoiceDetails.ExpectedExclTaxCalcEdit.Visible);
						AssertEquals("ExcludingTaxPanel.Visible", false, form.GetControl<ZPanel>("ExcludingTaxPanel", true).Visible);
						AssertEquals("TaxTotalPanel.Visible", false, form.GetControl<ZPanel>("TaxTotalPanel", true).Visible);
					}
				}
				finally
				{
					AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestVisibleOfUnallocatedTabPage()
		{
			var invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable ||
				invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
				invoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
			{
				using (var form = new BaseInvoicingForm(invoice))
				{
					form.Show();
					invoice.ValidateExpectedInvoiceTotal = true;

					var unallocatedTabPage = form.GetControl<ZTabPage>("UnallocatedTabPage", true);
					AssertEquals("UnallocatedTabPage.TabVisible", true, unallocatedTabPage.TabVisible);

					invoice.ValidateExpectedInvoiceTotal = false;
					AssertEquals("UnallocatedTabPage.TabVisible", false, unallocatedTabPage.TabVisible);
				}
			}
			else
			{
				using (var form = new BaseInvoicingForm(invoice))
				{
					form.Show();
					invoice.ValidateExpectedInvoiceTotal = true;

					AssertEquals("UnallocatedTabPage not found", 0, form.Controls.Find("UnallocatedTabPage", true).Length);
				}
			}
		}

		public void TestTotalAndUnallocatedTabControlHeight()
		{
			var invoice = GetInvoiceWithValidTestData();
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;

			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();
				Application.DoEvents();

				AssertEquals(false, form.GetControl<ZCalcFindBox>("AH_OSSubTotalAmountCalcEdit", true).Visible);
				AssertEquals(false, form.GetControl<ZCalcFindBox>("AH_OSOtherTaxesAmountCalcEdit", true).Visible);

				var wHTPanel = form.GetControl<ZPanel>("WHTPanel", true);
				var extraTaxPanel = form.GetControl<ZPanel>("ExtraTaxPanel", true);
				Assert(!wHTPanel.Visible);
				Assert(!extraTaxPanel.Visible);

				var totalAndUnallocatedTabControl = form.GetControl<ZTabControl>("TotalAndUnallocatedTabControl", true);
				var expectedHeight = 135;
				AssertEquals(expectedHeight, totalAndUnallocatedTabControl.MinimumSize.Height);
				AssertEquals(expectedHeight, totalAndUnallocatedTabControl.Size.Height);
			}

			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();
				Application.DoEvents();

				var wHTPanel = form.GetControl<ZPanel>("WHTPanel", true);
				Assert(wHTPanel.Visible);

				var totalAndUnallocatedTabControl = form.GetControl<ZTabControl>("TotalAndUnallocatedTabControl", true);
				AssertEquals(135, totalAndUnallocatedTabControl.MinimumSize.Height);
				AssertEquals(157, totalAndUnallocatedTabControl.Size.Height);
			}

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Canada);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;

			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();
				Application.DoEvents();

				var wHTPanel = form.GetControl<ZPanel>("WHTPanel", true);
				var extraTaxPanel = form.GetControl<ZPanel>("ExtraTaxPanel", true);
				Assert(wHTPanel.Visible);
				Assert(extraTaxPanel.Visible);

				var totalAndUnallocatedTabControl = form.GetControl<ZTabControl>("TotalAndUnallocatedTabControl", true);
				AssertEquals(135, totalAndUnallocatedTabControl.MinimumSize.Height);
				AssertEquals(179, totalAndUnallocatedTabControl.Size.Height);
			}

			TaxFrameworkTestObjectCreator.CreateTaxConfiguration(invoice.Company, TaxConfigurationLedgers.AccountsPayable.Code);
			TaxFrameworkTestObjectCreator.CreateTaxConfiguration(invoice.Company, TaxConfigurationLedgers.AccountsReceivable.Code);
			AssertEquals(ShouldTestForOtherTaxes, taxRecordParent.ShouldCalculateTaxTransactions);

			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(ShouldTestForOtherTaxes, form.GetControl<ZCalcFindBox>("AH_OSSubTotalAmountCalcEdit", true).Visible);
				AssertEquals(ShouldTestForOtherTaxes, form.GetControl<ZCalcFindBox>("AH_OSOtherTaxesAmountCalcEdit", true).Visible);

				var wHTPanel = form.GetControl<ZPanel>("WHTPanel", true);
				var extraTaxPanel = form.GetControl<ZPanel>("ExtraTaxPanel", true);
				Assert(wHTPanel.Visible);
				Assert(extraTaxPanel.Visible);

				var totalAndUnallocatedTabControl = form.GetControl<ZTabControl>("TotalAndUnallocatedTabControl", true);
				AssertEquals(135, totalAndUnallocatedTabControl.MinimumSize.Height);
				if (ShouldTestForOtherTaxes)
				{
					AssertEquals(223, totalAndUnallocatedTabControl.Size.Height);
				}
				else
				{
					AssertEquals(179, totalAndUnallocatedTabControl.Size.Height);
				}
			}
		}

		public void TestFormVerbForIncompleteTransactionsInDb()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				testInvoice.SaveAsIncomplete();

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					Application.DoEvents();
					AssertEquals("Edit", testForm.FormVerb);
					AssertEquals("Should be visible", testInvoice is APInvoice || testInvoice is APCreditNote, testForm.InvoiceDetails.BulkChargeImportButton.Visible);
				}

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.DisplayMode = ODisplayMode.ReadOnly;
					testForm.Show();
					Application.DoEvents();
					AssertEquals("View", testForm.FormVerb);
					Assert("Should not be visible", !testForm.InvoiceDetails.BulkChargeImportButton.Visible);
				}

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.DisplayMode = ODisplayMode.Delete;
					testForm.Show();
					Application.DoEvents();
					AssertEquals("Delete", testForm.FormVerb);
				}

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.DisplayMode = ODisplayMode.Delete;
					testForm.CancelInsteadOfDelete = true;
					testForm.Show();
					Application.DoEvents();
					AssertEquals("Cancel", testForm.FormVerb);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestDisplayModeForIncompleteTransactions()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				testInvoice.SaveAsIncomplete();

				var modesThatShowNewButton = new ODisplayMode[] { ODisplayMode.Browse, ODisplayMode.New, ODisplayMode.NewSaved };

				foreach (ODisplayMode mode in Enum.GetValues(typeof(ODisplayMode)))
				{
					using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
					{
						testForm.DisplayMode = mode;
						testForm.Show();
						Application.DoEvents();

						var expectedMode = modesThatShowNewButton.Contains(mode) ? ODisplayMode.Edit : mode;
						AssertEquals("New button should not be shown on Incomplete invoice form.", expectedMode, testForm.DisplayMode);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestPreviewInvoiceForIncompleteAPINVWithOrWithoutApprovalRequest()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();
			var testObjectCreator = new TestObjectCreator(Factory);

			var invoiceWithoutApprovalRequest = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1, 10, 0, 10, 0, testObjectCreator.Creditor1, testObjectCreator.GLHeader1.PK);
			invoiceWithoutApprovalRequest.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			invoiceWithoutApprovalRequest.Lines[0].AL_Desc = "desc";
			invoiceWithoutApprovalRequest.SaveAsIncomplete();
			using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoiceWithoutApprovalRequest))
			{
				testForm.Show();
				testForm.PreviewInvoice_Click_ForTestOnly(testForm, null);
				AssertEquals("This transaction type is not supported to preview invoices", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Preview Invoices", UnitTestUserNotification.Instance.LastMessage.Caption);
			}

			UnitTestUserNotification.Instance.ClearMessages();

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockPrintTaskUIProvider.Setup(m => m.ShowPreview(null, null, null));
			mockPrintTaskUIProvider.Setup(m => m.ShowRuntimeOptionsUI(It.IsAny<PrintTask>(), It.IsAny<AllowedDeliveryOptions>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>()))
				.Returns(false);
			var invoiceWithApprovalRequest = testObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(testObjectCreator.AALSHI, 100);
			var approvalRequestInOtherFactory = testObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoiceWithApprovalRequest);
			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoiceWithApprovalRequest))
			{
				testForm.Show();
				testForm.PreviewInvoice_Click_ForTestOnly(testForm, null);
				AssertEquals("Expect no error message", null, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Expect no error message", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPreviewInvoice_Click_ShowsError_WhenDocumentMenuSetupIncorrectly()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();
			var testObjectCreator = new TestObjectCreator(Factory);

			var invoice = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1, 10, 0, 10, 0, testObjectCreator.Creditor1, testObjectCreator.GLHeader1.PK);
			invoice.Lines[0].AL_AT = TestObjectCreator.GST1.PK;
			invoice.Lines[0].AL_Desc = "desc";

			var menuFactory = new BusinessObjectFactory();
			var menu = menuFactory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.CostConfirmationDocument));
			var anotherMenu = menuFactory.New<StmMenuItem>();
			anotherMenu.SU_MenuName = menu.SU_MenuName;
			anotherMenu.SU_MenuPath = menu.SU_MenuPath;
			anotherMenu.SU_BusinessContext = menu.SU_BusinessContext;
			anotherMenu.SU_IsPublished = menu.SU_IsPublished;
			anotherMenu.SU_IsSystemDefined = menu.SU_IsSystemDefined;
			anotherMenu.SU_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			menuFactory.Save();
			using (var testForm = new BaseInvoicingForm(invoice))
			{
				testForm.Show();
				testForm.PreviewInvoice_Click_ForTestOnly(testForm, null);
				AssertEquals("Unable to find Invoice document command for transaction INV2 with menu name: Cost Confirmation Document, menu path: . Total commands found: 2.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Preview Invoices", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
			UnitTestUserNotification.Instance.ClearMessages();

			var menus = menuFactory.Load<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, JobInvoicingEDocsProviderSupporter.CostConfirmationDocument));
			menus.ForEach(x => x.SU_MenuName += "XYZ");
			menuFactory.Save();
			using (var testForm = new BaseInvoicingForm(invoice))
			{
				testForm.Show();
				testForm.PreviewInvoice_Click_ForTestOnly(testForm, null);
				AssertEquals("Unable to find Invoice document command for transaction INV2 with menu name: Cost Confirmation Document, menu path: . Total commands found: 0.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Preview Invoices", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestPreviewInvoice_Click_ShowsError_WhenRestoreSavedDataReturnsFailed()
		{
			AssertPreviewInvoice_Click(
				_ => { },
				invoice =>
				{
					Factory.LoadTop1<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, invoice.PK).AddToFilter(StmNoteSchema.ST_Table, invoice.TableName)).Delete();
					Factory.Save();
				},
				() =>
				{
					AssertEquals("Preview Invoice", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("There is a problem with the transaction and this transaction can no longer be used. You will need to delete this transaction from your system", UnitTestUserNotification.Instance.LastMessage.Text);
				});
		}

		public void TestPreviewInvoice_Click_ShowsError_WhenRestoreSavedDataReturnsSuccessWithErrors()
		{
			var testChargeCode = TestObjectCreator.CreateChargeCode("TESTCODE", createWithoutZZ: true);
			AssertPreviewInvoice_Click(
				invoice => { invoice.Lines[0].AL_AC = testChargeCode.PK; },
				_ =>
				{
					var factoryToDelete = new BusinessObjectFactory();
					factoryToDelete.Load<AccChargeCode>(testChargeCode.PK).Delete();
					factoryToDelete.Save();
				},
				() =>
				{
					AssertEquals("Preview Invoice", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals("Could not find generic charge TESTCODE", UnitTestUserNotification.Instance.LastMessage.Text);
				});
		}

		void AssertPreviewInvoice_Click(Action<InvoicingBase> testSpecificSetup, Action<InvoicingBase> modifyData, Action testSpecificAssert)
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var mockPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockPrintTaskUIProvider.Setup(m => m.ShowPreview(null, null, null));
			mockPrintTaskUIProvider.Setup(m => m.ShowRuntimeOptionsUI(It.IsAny<PrintTask>(), It.IsAny<AllowedDeliveryOptions>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ISecurityCheckpoint>()))
				.Returns(false);
			var invoiceWithApprovalRequest = TestObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(TestObjectCreator.AALSHI, 100);

			testSpecificSetup(invoiceWithApprovalRequest);

			var approvalRequestInOtherFactory = TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoiceWithApprovalRequest);

			modifyData(invoiceWithApprovalRequest);

			var invoiceInNewFactory = new BusinessObjectFactory().Load<InvoicingBase>(invoiceWithApprovalRequest.PK);

			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskUIProvider.Object))
			using (var testForm = new BaseInvoicingForm(invoiceInNewFactory))
			{
				testForm.Show();
				testForm.PreviewInvoice_Click_ForTestOnly(testForm, null);
				testSpecificAssert();
			}
		}

		public void TestPreviewInvoiceVisibleOnlyToAccountsReceivable()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					AssertNotNull(testForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Preview Invoice"));
				}
			}
			else
			{
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					AssertNull(testForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Preview Invoice"));
				}
			}
		}

		public void TestPreviewCostVisibleOnlyToAP_UN()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice.AH_Ledger == LedgerTypes.AccountsPayable || testInvoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					AssertNotNull(testForm.PreviewCostMenuItem);
				}
			}
			else
			{
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					AssertNull(testForm.PreviewCostMenuItem);
				}
			}
		}

		public void TestSaveAsIncompleteAccessibility_NewComplete()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice.AH_Ledger == LedgerTypes.AccountsPayable && !(testInvoice is APAdjustmentNote))
			{
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert(testForm.SaveAsIncompleteMenuItem.Enabled);
					Factory.Save();
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert(!testForm.SaveAsIncompleteMenuItem.Enabled);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestAdditionalActionMenusWithReadOnlyMode()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.DisplayMode = ODisplayMode.ReadOnly;
					testForm.Show();
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert(!testForm.SaveAsIncompleteMenuItem.Enabled);
					Assert(!testForm.AutoAllocateDiscrepancyMenuItem.Enabled);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSaveAsIncompleteAccessibility_NewIncomplete()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert(testForm.SaveAsIncompleteMenuItem.Enabled);
					testInvoice.SaveAsIncomplete();
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert(testForm.SaveAsIncompleteMenuItem.Enabled);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSaveAsIncompleteAccessibility_SavedComplete()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				Factory.Save();

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert(!testForm.SaveAsIncompleteMenuItem.Enabled);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSaveAsIncompleteAccessibility_SavedIncomplete()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				testInvoice.SaveAsIncomplete();

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert(testForm.SaveAsIncompleteMenuItem.Enabled);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSaveAsIncompleteAccessibility_NewReversal()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			if (testInvoice.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				ReversingFactory reversingFactory = new ReversingFactory();
				ReversingBase reversing = reversingFactory.NewReversing(testInvoice);
				reversing.Reverse();
				InvoicingBase reversal = testInvoice.ReverseTransaction as InvoicingBase;
				reversal.AH_TransactionNum = "S0000001";
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(reversal))
				{
					testForm.Show();
					Assert("Set true on initialization", testForm.LastSaveSuccessful_ForTestOnly);
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert("Save as incomplete is not enabled", !testForm.SaveAsIncompleteMenuItem.Enabled);

					testForm.LastSaveSuccessful_ForTestOnly = false;
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert("Save as incomplete is not enabled after saving error", !testForm.SaveAsIncompleteMenuItem.Enabled);

					Factory.Save();
					testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert("Save as incomplete is not enabled for saved invoice", !testForm.SaveAsIncompleteMenuItem.Enabled);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSaveAsIncompleteAccessibility_TransactionAllocation()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			Factory.Save();

			InvoicingBase testInvoice = TransactionAllocationConverter.ConvertUnallocatedToAP(transaction).Invoice;

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
			{
				testForm.Show();
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("SaveAsIncompleteMenuItem.Enabled", testForm.SaveAsIncompleteMenuItem.Enabled);
			}
		}

		public void TestOverrideTransactionBranchAndDepartmentAccessibility_AR()
		{
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.Debtor);

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
			{
				testForm.Show();
				Env.Security.ReceivableOverrideTransactionBranchAndDepartment.IsAllowed = true;
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);

				testForm.OverrideBranchAndDepartmentMenuItem.PerformClick();
				Assert("no errors", UnitTestUserNotification.Instance.LastMessage.WasNone);

				Env.Security.ReceivableOverrideTransactionBranchAndDepartment.IsAllowed = false;
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);
				testForm.OverrideBranchAndDepartmentMenuItem.PerformClick();
				AssertEquals("Validation Error Message Should Be Shown", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Actions -> Override Transaction Branch / Department", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", !testForm.OverrideBranchAndDepartmentMenuItem.Enabled);

				Env.Security.ReceivableOverrideTransactionBranchAndDepartment.IsAllowed = false;
			}
		}

		public void TestOverrideTransactionBranchAndDepartmentAccessibility_AP()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0M, 150M, 50M, 0M, 200M, 50M, 0M, TestObjectCreator.Creditor1);

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
			{
				testForm.Show();
				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = true;
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);

				testForm.OverrideBranchAndDepartmentMenuItem.PerformClick();
				Assert("no errors", UnitTestUserNotification.Instance.LastMessage.WasNone);

				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = false;
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);
				testForm.OverrideBranchAndDepartmentMenuItem.PerformClick();
				AssertEquals("Validation Error Message Should Be Shown", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Payables Transactions -> Actions -> Override Transaction Branch / Department", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", !testForm.OverrideBranchAndDepartmentMenuItem.Enabled);

				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = false;
			}
		}

		public void TestOverrideTransactionBranchAndDepartmentAccessibility_incomplete()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0M, 150M, 50M, 0M, 200M, 50M, 0M, TestObjectCreator.Creditor1);
			invoice.SaveAsIncomplete();
			using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
			{
				testForm.Show();
				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = true;
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);
				testForm.OverrideBranchAndDepartmentMenuItem.PerformClick();
				Assert("no errors", UnitTestUserNotification.Instance.LastMessage.WasNone);

				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = false;
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);
				testForm.OverrideBranchAndDepartmentMenuItem.PerformClick();
				AssertEquals("Validation Error Message Should Be Shown", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Payables Transactions -> Actions -> Override Transaction Branch / Department", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("In database but still incomplete", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);

				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = false;
			}
		}

		public void TestOverrideTransactionBranchAndDepartmentAccessibility_Unapproved()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0M, 150M, 50M, 0M, 200M, 50M, 0M, TestObjectCreator.Creditor1);

			foreach (AccTransactionLines line in invoice.Lines)
			{
				line.AL_LineType = TransactionLineTypes.UnapprovedCost;
			}

			invoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			invoice.AH_TransactionType = TransactionTypes.UAInvoice;

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
			{
				testForm.Show();
				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = true;
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);
				testForm.OverrideBranchAndDepartmentMenuItem.PerformClick();
				Assert("no errors", UnitTestUserNotification.Instance.LastMessage.WasNone);

				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = false;
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);
				testForm.OverrideBranchAndDepartmentMenuItem.PerformClick();
				AssertEquals("Validation Error Message Should Be Shown", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Payables Transactions -> Actions -> Override Transaction Branch / Department", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", !testForm.OverrideBranchAndDepartmentMenuItem.Enabled);

				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = false;
			}
		}

		public void TestOverrideTransactionBranchAndDepartmentAccessibility_Allocating()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0M, 150M, 50M, 0M, 200M, 50M, 0M, TestObjectCreator.Creditor1);
			invoice.AH_Ledger = LedgerTypes.TransactionsPendingAllocation;
			invoice.AH_TransactionType = TransactionTypes.InvoicePendingAllocation;
			Factory.Save();
			invoice.AH_Ledger = LedgerTypes.AccountsPayable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;
			AssertEquals("Transaction.IsAllocatingInvoice should now be true", true, invoice.IsAllocatingInvoice);

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
			{
				testForm.Show();

				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = true;
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);

				testForm.OverrideBranchAndDepartmentMenuItem.PerformClick();
				Assert("no errors", UnitTestUserNotification.Instance.LastMessage.WasNone);

				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = false;
				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", testForm.OverrideBranchAndDepartmentMenuItem.Enabled);
				testForm.OverrideBranchAndDepartmentMenuItem.PerformClick();
				AssertEquals("Validation Error Message Should Be Shown", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Payables -> Payables Transactions -> Actions -> Override Transaction Branch / Department", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("OverrideBranchAndDepartmentMenuItem.Enabled", !testForm.OverrideBranchAndDepartmentMenuItem.Enabled);

				Env.Security.PayablesOverrideTransactionBranchAndDepartment.IsAllowed = false;
			}
		}

		public void TestSaveAsIncompletePossibleWhenLastSaveisNotSuccessful()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			Factory.Save();
			var testInvoice = TransactionAllocationConverter.ConvertUnallocatedToAP(transaction).Invoice;

			using (var testForm = new BaseInvoicingForm(testInvoice))
			{
				testForm.DisplayMode = ODisplayMode.ReadOnly;
				Assert("Set true on initialization", testForm.LastSaveSuccessful_ForTestOnly);
				Assert("Save as incomplete is not possible", !testForm.IsSaveAsIncompletePossible_ForTestOnly);
				testForm.Show();
				testForm.LastSaveSuccessful_ForTestOnly = false;
				Assert("Save as incomplete is possible", testForm.IsSaveAsIncompletePossible_ForTestOnly);
			}
		}

		public void TestSaveAsIncompletePossibleWhenLastSaveisNotSuccessfulForPostedInvoice()
		{
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();

			Assert("Precondition", invoice.IsPosted);
			Assert("Precondition", !invoice.IsDeleted);
			Assert("Precondition", !invoice.IsReversed);

			using (var testForm = new BaseInvoicingForm(invoice))
			{
				testForm.DisplayMode = ODisplayMode.ReadOnly;
				Assert("Set true on initialization", testForm.LastSaveSuccessful_ForTestOnly);
				Assert("Save as incomplete is not possible", !testForm.IsSaveAsIncompletePossible_ForTestOnly);

				testForm.LastSaveSuccessful_ForTestOnly = false;
				Assert("Save as incomplete is still not possible", !testForm.IsSaveAsIncompletePossible_ForTestOnly);
			}
		}

		public void TestSaveAsIncompleteForPostedInvoice()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			using (var testForm = new BaseInvoicingForm(invoice))
			{
				AssertNull("Save As Incomplete menu is null", testForm.SaveAsIncompleteMenuItem);
				AssertNoExceptionThrown("Should not throw exception", () => testForm.SetSaveAsIncompleteAccessibility_ForTestOnly());
			}
		}

		public void TestRelatedInvoicesTabVisible()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(ShouldShowRelatedInvoicesTab, form.RelatedInvoicesTabPage_ForTestOnly.TabVisible);
			}
		}

		public void TestRemoveDocumentsMenu()
		{
			if (ShouldShowRelatedInvoicesTab)
			{
				InvoicingBase testInvoice = GetInvoiceWithValidTestData();
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
				{
					testForm.Show();
					testForm.MainTabControl.SelectedIndex = 1;
					testForm.RelatedInvoicesGrid_ForTestOnly.ContextMenu.MenuItems.Add("Documents");
					testForm.RelatedInvoicesGridContextMenu_Popup_ForTestOnly(this, null);
					bool isDocumentsExist = false;
					for (int i = 0; i < testForm.RelatedInvoicesGrid_ForTestOnly.ContextMenu.MenuItems.Count; i++)
					{
						if (testForm.RelatedInvoicesGrid_ForTestOnly.ContextMenu.MenuItems[i].Text == "Documents")
						{
							isDocumentsExist = true;
						}
					}
					Assert("Menu Item 'Documents' must be deleted", !isDocumentsExist);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSetReadOnlyIncludingChildren()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();
			if (testInvoice.AH_Ledger != LedgerTypes.UnapprovedPayableTransactions)
			{
				testInvoice.GenerateReverseTransaction(false);
				using (BaseInvoicingForm testForm = new BaseInvoicingForm((InvoicingBase)testInvoice.ReverseTransaction))
				{
					AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					testForm.DisplayMode = ODisplayMode.Delete;
					testForm.Show();
					Application.DoEvents();
					Assert("AH_PostDateEdit must be editable", !testForm.InvoiceDetails.AH_PostDateEdit.ReadOnly);
					foreach (Control control in testForm.InvoiceDetails.AH_PostDateEdit.Controls)
					{
						Assert("AH_PostDateEdit must be editable", !control.GetReadOnly());
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		[ExpectNoExceptions]
		public void TestSaveAsIncompleteWithChargeCodeIsDeleted()
		{
			var taxRate = TestObjectCreator.CreateTaxRate("T01", "testTaxRate", 1);

			var newFactory = new BusinessObjectFactory();
			AccChargeCode chargeCode = newFactory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "C01";
			chargeCode.AC_Desc = "testACCode";
			chargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			chargeCode.AC_MarginPercentage = 100;
			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCode.AC_AT_GSTRate = taxRate.PK;
			chargeCode.AC_DepartmentFilterList = "ALL";
			chargeCode.AC_IsActive = true;
			chargeCode.SetGLAccountDataForTesting(TestObjectCreator.CreateGLHeader());
			newFactory.Save();

			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "test000001");
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "GBLON";

			var job = TestObjectCreator.CreateJobHeader();
			job.JH_ParentID = shipment.PK;

			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1m, TestObjectCreator.Creditor1);
			invoice.AH_TransactionNum = "testnum001";
			invoice.AH_JH = job.PK;

			var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_AC_ChargeCode = chargeCode.PK;
			cost.E6_OSCostAmount = 10;

			invoice.ImportAllApportionmentsFromCosting();
			invoice.ConsolCostValidatedPKList.Add(cost.PK);

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
			{
				testForm.Show();
				chargeCode.Delete();
				newFactory.Save();
				testForm.SaveAsIncompleteMenuItem.PerformClick();
			}
		}

		public void TestIncompleteTransactionIsSavedAsIncompleteOnFormClosing()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				if (form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					form.Show();
					form.Invoice_ForTestOnly.FillWithValidTestData();
					form.Invoice_ForTestOnly.AH_OH = TestObjectCreator.AALSHI.PK;
					InvoicingLineBase line = (InvoicingLineBase)form.Invoice_ForTestOnly.Lines.AddNew();
					line.FillWithValidTestData();
					line.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Default;
					var chargeList = line.ChargeList;
					chargeList.Load();
					line.GenericCharge = chargeList[0].PK;
					line.AL_LineAmount = line.AL_OSExTaxAmount = 10m;
					line.AL_LocalExTaxAmount = 10m;

					form.Invoice_ForTestOnly.AH_TransactionNum = "00001010";
					form.Invoice_ForTestOnly.SaveAsIncomplete();

					form.Invoice_ForTestOnly.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.PostingButtonsUserControl.CloseButton.PerformClick();

					AssertEquals("Invoice_ForTestOnly should remain incomplete", LedgerTypes.IncompleteTransactions, form.Invoice_ForTestOnly.AH_Ledger);
					AssertEquals(@"This record has been modified.
Would you like to save the changes?

'Yes' will save your changes as incomplete.
'No' will discard your changes.
'Cancel' will return you to the form.", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					AssertEquals("Save as incomplete successful", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestSaveAsIncomplete_ExtraMsg()
		{
			var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			var userNotif = UnitTestUserNotification.Instance;

			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			using (var form = (BaseInvoicingForm)GetFormToBashCore())
			{
				var invoice = form.Invoice_ForTestOnly;
				if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					form.Show();
					invoice.FillWithValidTestData();
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					invoice.AH_ComplianceSubType = "TXI";

					var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 10);

					userNotif.ClearMessagesAndAnswers();
					form.SaveAsIncomplete_ForTestOnly();
					AssertContains(@"Note: Incomplete Invoice is saved without Compliance Sub Type, because it must be set only when Compliance Number is allocated",
						userNotif.LastMessage.Text);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestSaveAsIncomplete_ComplianceSequence()
		{
			var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			var accMasterRegistry = AccountingMasterFilesRegistry.Instance;
			var userNotif = UnitTestUserNotification.Instance;
			var errorMessage = ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage;

			using (accMasterRegistry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (accMasterRegistry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			using (var form = (BaseInvoicingForm)GetFormToBashCore())
			{
				var invoice = form.Invoice_ForTestOnly;
				if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					form.Show();
					invoice.FillWithValidTestData();
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					invoice.AH_ComplianceSubType = ZString.Empty;
					var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 10);
					line.AL_AT = TestObjectCreator.GST1.PK;

					userNotif.ClearMessagesAndAnswers();
					form.ValidateAndSave_ForTestOnly();
					Assert(userNotif.LastMessage.WasError);
					AssertHasRowError("Error in Compliance Sequence on Posting", invoice, errorMessage);

					userNotif.ClearMessagesAndAnswers();
					form.SaveAsIncomplete_ForTestOnly();
					Assert(!userNotif.LastMessage.WasError);
					AssertNoRowError("No error in Compliance Sequence for Incomplete", invoice, errorMessage);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestTransactionIsMovedToAPBeforePosting()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var testChargeCode = TestObjectCreator.CC1;
			var glAccount = TestObjectCreator.CreateARSuspenseControlAccount();
			Factory.Save();

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				if (form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					form.Show();
					form.Invoice_ForTestOnly.FillWithValidTestData();
					form.Invoice_ForTestOnly.AH_OH = TestObjectCreator.AALSHI.PK;
					form.Invoice_ForTestOnly.Lines.AddNew();
					if (form.Invoice_ForTestOnly is AdjustmentNote)
					{
						form.Invoice_ForTestOnly.Lines[0].GenericCharge = glAccount.PK;
						form.Invoice_ForTestOnly.Lines[0].AL_AT = TestObjectCreator.FREEVAT.PK;
					}
					else
					{
						form.Invoice_ForTestOnly.Lines[0].GenericCharge = testChargeCode.PK;
						form.Invoice_ForTestOnly.Lines[0].AL_JH = job.PK;
					}
					form.Invoice_ForTestOnly.Lines[0].AL_Desc = "Testing";
					form.Invoice_ForTestOnly.Lines[0].AL_GE = TestObjectCreator.NonCurrentDepartment.PK;
					form.Invoice_ForTestOnly.Lines[0].AL_OSExTaxAmount = form.Invoice_ForTestOnly.Lines[0].AL_OSAmount = form.Invoice_ForTestOnly.Lines[0].AL_LineAmount = 10m;
					form.Invoice_ForTestOnly.Lines[0].PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Default;
					form.Invoice_ForTestOnly.SaveAsIncomplete();
					AssertEquals(LedgerTypes.IncompleteTransactions, form.Invoice_ForTestOnly.AH_Ledger);

					form.ValidateAndSave_ForTestOnly();
					AssertNoErrors(form.Invoice_ForTestOnly);  //check these erorrs again and fix tests
					AssertEquals(LedgerTypes.AccountsPayable, form.Invoice_ForTestOnly.AH_Ledger);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestIsPostOnly()
		{
			UAInvoice invoice = Factory.New<UAInvoice>();
			invoice.IsReverseTransaction = true;
			invoice.AH_TransactionNum = "TEST0001";
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.UAInvoice);
			using (InvoiceForm form = (InvoiceForm)controller.ShowDeleteForm(invoice))
			{
				form.Show();
				Assert("IsPostOnly must be true bacause IsUATransaction true", form.IsPostOnly);
			}
		}

		public void TestShouldEnableApportionChargesButton()
		{
			UAInvoice invoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<UAInvoice>(Factory);
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();

			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				AssertEquals("shouldn't show ApportionChargesButton when APInvoice is already saved", false, form.ShouldEnableApportionChargesButton_ForTestOnly);

				IDataGridLayoutIdentifierRoot idRoot = form;
				AssertEquals(form.Name + invoice.GetType().Name, idRoot.ID);
			}
		}

		public void TestShouldEnableApportionChargesWhenAllocatingInvoice()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			TransactionPendingAllocation invoicePendingAllocation = Factory.New<TransactionPendingAllocation>();
			invoicePendingAllocation.AH_TransactionNum = "invoice";
			invoicePendingAllocation.AH_OH = org.PK;
			invoicePendingAllocation.AH_OSExTaxAmount = 100m;

			Factory.Save();

			InvoicingBase result = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
			using (InvoiceForm form = new InvoiceForm(result))
			{
				form.Show();
				Assert("ShouldEnableApportionChargesButton_ForTestOnly", form.ShouldEnableApportionChargesButton_ForTestOnly);

				Application.DoEvents();
				Assert("should be visible when allocating invoices", form.InvoiceDetails.ApportionChargesButton.Visible);
			}
		}

		public void TestHideChargeButtonsWhenViewOrDelete()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();

			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();

				form.DisplayMode = ODisplayMode.ReadOnly;
				Assert("IsViewOrDeleteMode_ForTestOnly", form.IsViewOrDeleteMode_ForTestOnly);
				AssertEquals("ShouldEnableApportionChargesButton_ForTestOnly", false, form.ShouldEnableApportionChargesButton_ForTestOnly);
				AssertEquals("ShouldEnableBulkChargeImportButton_ForTestOnly", false, form.ShouldEnableBulkChargeImportButton_ForTestOnly);

				form.DisplayMode = ODisplayMode.Delete;
				Assert("IsViewOrDeleteMode_ForTestOnly", form.IsViewOrDeleteMode_ForTestOnly);
				AssertEquals("ShouldEnableApportionChargesButton_ForTestOnly", false, form.ShouldEnableApportionChargesButton_ForTestOnly);
				AssertEquals("ShouldEnableBulkChargeImportButton_ForTestOnly", false, form.ShouldEnableBulkChargeImportButton_ForTestOnly);

				Application.DoEvents();
				AssertEquals("ApportionChargesButton should be hidden when reversing invoice", false, form.InvoiceDetails.ApportionChargesButton.Visible);
				AssertEquals("BulkChargeImportButton should be hidden when reversing invoice", false, form.InvoiceDetails.BulkChargeImportButton.Visible);
			}
		}

		public void TestHideChargesButtonsWhenReversing()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();

			invoice.GenerateReverseTransaction(true);
			Assert("IsReversing", invoice.IsReversing);

			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				AssertEquals("ShouldEnableApportionChargesButton_ForTestOnly", false, form.ShouldEnableApportionChargesButton_ForTestOnly);
				AssertEquals("ShouldEnableBulkChargeImportButton_ForTestOnly", false, form.ShouldEnableBulkChargeImportButton_ForTestOnly);

				Application.DoEvents();
				AssertEquals("ApportionChargesButton should be hidden when reversing invoice", false, form.InvoiceDetails.ApportionChargesButton.Visible);
				AssertEquals("BulkChargeImportButton should be hidden when reversing invoice", false, form.InvoiceDetails.BulkChargeImportButton.Visible);
			}
		}

		public void TestHideChargesButtonsWhenApproving()
		{
			UAInvoice invoice = Factory.NewWithValidTestData<UAInvoice>();
			invoice.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			invoice.AH_TransactionType = TransactionTypes.UAInvoice;
			Factory.Save();

			InvoicingBase convertedInvoice = new UnapprovedTransactionConverter(Factory).ConvertToAP(invoice, false);
			Assert("IsApprovingInvoice", convertedInvoice.IsInvoiceApproving);

			using (InvoiceForm form = new InvoiceForm(convertedInvoice))
			{
				form.Show();
				AssertEquals("ShouldEnableApportionChargesButton_ForTestOnly", false, form.ShouldEnableApportionChargesButton_ForTestOnly);
				AssertEquals("ShouldEnableBulkChargeImportButton_ForTestOnly", false, form.ShouldEnableBulkChargeImportButton_ForTestOnly);

				form.FireSaveButton();

				//Have to Save StmALogs manually to get the 'IsInDatabseIncludeChildren' true. In functional test it is getting true after saving form
				convertedInvoice.Logs.Factory.Save();

				Assert("must be in the database", form.BusinessEntity.IsInDatabaseIncludingChildren);
				AssertEquals("ShouldEnableApportionChargesButton_ForTestOnly", false, form.ShouldEnableApportionChargesButton_ForTestOnly);
				AssertEquals("ShouldEnableBulkChargeImportButton_ForTestOnly", false, form.ShouldEnableBulkChargeImportButton_ForTestOnly);
			}
		}

		public void TestApportionToConsolsScreenDisplayMode()
		{
			var creator = new TestObjectCreator(Factory);

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				if (form.Invoice_ForTestOnly is APInvoice)
				{
					form.Show();
					form.DisplayMode = ODisplayMode.Edit;

					form.Invoice_ForTestOnly.AH_OH = creator.ABIGAS.PK;
					form.InvoiceDetails.ApportionChargesButton.PerformClick();

					AssertNotNull("View screen must be shown.", ZFormModaliser.LastFormShownForTest);
					AssertType(typeof(APInvoiceConsolCostingForm), ZFormModaliser.LastFormShownForTest);
					AssertEquals("Apportions to consols form should be in edit mode", ODisplayMode.Edit, ((APInvoiceConsolCostingForm)ZFormModaliser.LastFormShownForTest).DisplayMode);
				}
				else
				{
					Assert(true);
				}
			}

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				if (form.Invoice_ForTestOnly is APInvoice)
				{
					form.Show();
					form.Invoice_ForTestOnly.AH_OH = creator.ABIGAS.PK;
					form.DisplayMode = ODisplayMode.ReadOnly;

					form.InvoiceDetails.ApportionChargesButton.PerformClick();

					AssertNotNull("View screen must be shown.", ZFormModaliser.LastFormShownForTest);
					AssertType(typeof(APInvoiceConsolCostingForm), ZFormModaliser.LastFormShownForTest);
					AssertEquals("Apportions to consols form should be read only as invoice screen is read only", ODisplayMode.ReadOnly, ((APInvoiceConsolCostingForm)ZFormModaliser.LastFormShownForTest).DisplayMode);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestApportionToConsolsScreenDisplayModeWhenEditingApportionment()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				if (form.Invoice_ForTestOnly is APInvoice)
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();

					form.Invoice_ForTestOnly.Lines.Add(TestObjectCreator.CreateInvoiceLine(form.Invoice_ForTestOnly, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, TestObjectCreator.CC1.PK));
					form.Invoice_ForTestOnly.AH_OH = TestObjectCreator.AALSHI.PK;
					form.Invoice_ForTestOnly.Lines[0].AL_AC = Guid.Empty;

					form.InvoiceDetails.TransactionLinesGrid.SelectAllElements();
					AssertEquals("Precondition: an invoice must be selected for preview.", 2, form.InvoiceDetails.TransactionLinesGrid.SelectedRowCount);
					MenuItem editApportionmentMenuItem = form.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");

					editApportionmentMenuItem.PerformClick();

					AssertNotNull("View screen must be shown.", ZFormModaliser.LastFormShownForTest);
					AssertType(typeof(APInvoiceConsolCostingForm), ZFormModaliser.LastFormShownForTest);
					AssertEquals("Apportions to consols form should be in edit mode", ODisplayMode.Edit, ((APInvoiceConsolCostingForm)ZFormModaliser.LastFormShownForTest).DisplayMode);
				}
				else
				{
					Assert(true);
				}
			}

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				if (form.Invoice_ForTestOnly is APInvoice)
				{
					form.Show();
					Application.DoEvents();

					form.Invoice_ForTestOnly.Lines.Add(TestObjectCreator.CreateInvoiceLine(form.Invoice_ForTestOnly, TestObjectCreator.AUD, 1M, 100M, 0M, 0M, TestObjectCreator.CC1.PK));
					form.Invoice_ForTestOnly.AH_OH = TestObjectCreator.AALSHI.PK;
					form.Invoice_ForTestOnly.Lines[0].AL_AC = Guid.Empty;

					form.DisplayMode = ODisplayMode.ReadOnly;

					form.InvoiceDetails.TransactionLinesGrid.SelectAllElements();
					AssertEquals("Precondition: an invoice must be selected for preview.", 2, form.InvoiceDetails.TransactionLinesGrid.SelectedRowCount);
					MenuItem editApportionmentMenuItem = form.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");

					editApportionmentMenuItem.PerformClick();

					AssertNotNull("View screen must be shown.", ZFormModaliser.LastFormShownForTest);
					AssertType(typeof(APInvoiceConsolCostingForm), ZFormModaliser.LastFormShownForTest);
					AssertEquals("Apportions to consols form should be read only as invoice screen is read only", ODisplayMode.ReadOnly, ((APInvoiceConsolCostingForm)ZFormModaliser.LastFormShownForTest).DisplayMode);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestButtonVisibilityForInvoiceCreatedFromCassImport()
		{
			using (var form = (BaseInvoicingForm)GetFormToBashCore())
			{
				if (form.Invoice_ForTestOnly is APInvoice)
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();

					AssertEquals("ShouldEnableApportionChargesButton_ForTestOnly", true, form.ShouldEnableApportionChargesButton_ForTestOnly);
					AssertEquals("ShouldEnableBulkChargeImportButton_ForTestOnly", true, form.ShouldEnableBulkChargeImportButton_ForTestOnly);

					AssertEquals("ApportionChargesButton should be visible", true, form.InvoiceDetails.ApportionChargesButton.Visible);
					AssertEquals("BulkChargeImportButton should be visible", true, form.InvoiceDetails.BulkChargeImportButton.Visible);
				}
				else
				{
					Assert(true);
				}
			}

			using (var form = (BaseInvoicingForm)GetFormToBashCore())
			{
				if (form.Invoice_ForTestOnly is APInvoice)
				{
					form.DisplayMode = ODisplayMode.ReadOnly;
					form.Show();
					Application.DoEvents();

					AssertEquals("ShouldEnableApportionChargesButton_ForTestOnly", false, form.ShouldEnableApportionChargesButton_ForTestOnly);
					AssertEquals("ShouldEnableBulkChargeImportButton_ForTestOnly", false, form.ShouldEnableBulkChargeImportButton_ForTestOnly);

					AssertEquals("ApportionChargesButton should be visible", false, form.InvoiceDetails.ApportionChargesButton.Visible);
					AssertEquals("BulkChargeImportButton should be visible", false, form.InvoiceDetails.BulkChargeImportButton.Visible);
				}
				else
				{
					Assert(true);
				}
			}

			using (var form = (BaseInvoicingForm)GetFormToBashCore())
			{
				if (form.Invoice_ForTestOnly is APInvoice)
				{
					form.Invoice_ForTestOnly.Factory.SetContext(BusinessContext.CASS);
					form.DisplayMode = ODisplayMode.ReadOnly;
					form.Show();
					Application.DoEvents();

					AssertEquals("ShouldEnableApportionChargesButton_ForTestOnly", true, form.ShouldEnableApportionChargesButton_ForTestOnly);
					AssertEquals("ShouldEnableBulkChargeImportButton_ForTestOnly", false, form.ShouldEnableBulkChargeImportButton_ForTestOnly);

					AssertEquals("ApportionChargesButton should be visible for invoices created from cass import", true, form.InvoiceDetails.ApportionChargesButton.Visible);
					AssertEquals("BulkChargeImportButton should be hidden for invoices created from cass import", false, form.InvoiceDetails.BulkChargeImportButton.Visible);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestFormVerbforUaTransactions()
		{
			UAInvoice invoice = Factory.New<UAInvoice>();
			invoice.AH_TransactionNum = "S0000001";
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.UAInvoice);
			using (InvoiceForm form = (InvoiceForm)controller.ShowDeleteForm(invoice))
			{
				form.Show();
				AssertEquals("Rejecting", form.FormVerb);
			}
		}

		public void TestContextMenu_Popup()
		{
			UAInvoice invoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<UAInvoice>(Factory);
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";
			Factory.Save();

			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				form.TransactionLinesGridContextMenu_Popup_ForTestOnly(form, null);
				Assert("Edit Apportionment item must be enabled bacause IsInvoiceApproving is true", form.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems[0].Enabled);
			}
		}

		#region Tax Transactions

		public void TestViewModelValidation()
		{
			var invoice = GetInvoiceWithValidTestData(true);
			var mock = new Mock<IInvoicingBaseTaxFrameworkViewModel>();
			TaxFrameworkObjectFactory.SubstituteInvoicingBaseTaxFrameworkViewModel_ForTestOnly(invoice, mock.Object);

			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();
				Application.DoEvents();

				var errorMessage = "Error message from ValidateForPosting";
				mock.Setup(v => v.ValidateForPosting()).Returns(errorMessage);
				AssertEquals(ContinueWithSave.No, form.ShowPreSaveDialogs_ForTestOnly());
				Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Validation error message", errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				mock.Setup(v => v.ValidateForPosting()).Returns(ZString.Empty);
				AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				Assert(!UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNullOrEmpty("No validation error", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCalculateOtherTaxesButton_Visibility()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();

			var org = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);

			Factory.Save();

			var invoice = GetInvoiceWithValidTestData(true);
			invoice.AH_OH = org.PK;
			var chargeCode = TestObjectCreator.NonAccrualChargeCode;
			var line = TestObjectCreator.CreateInvoiceLine(invoice, null, chargeCode, 100M);
			line.GenericCharge = chargeCode.PK;
			line.AL_GE = TestObjectCreator.FIADepartment.PK;

			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			Assert(!taxRecordParent.ShouldCalculateTaxTransactions);
			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();
				AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
			}

			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			AssertEquals(ShouldTestForOtherTaxes, taxRecordParent.ShouldCalculateTaxTransactions);
			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();
				Application.DoEvents();
				AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
			}

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				if (ShouldTestForOtherTaxes)
				{
					AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
					AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);
				}
				else
				{
					AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
					return;
				}

				AssertNotEquals(0, invoice.Lines);
				Assert(!form.InvoiceDetails.IsTaxTransactionSummaryTabActivated_ForTest);
				Assert(!taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);

				form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).PerformClick();
				AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
				Assert(form.InvoiceDetails.IsTaxTransactionSummaryTabActivated_ForTest);
				AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);
				Assert(taxRecordParent.IsTaxTransactionsCalculatedBeforePosting);

				AssertContainsExactElementsInAnyOrder(invoice.Lines.Cast<InvoicingLineBase>().Select(l => TaxFrameworkObjectFactory.GetInvoicingLineBaseForOtherTaxesDisplay(l)),
					TaxFrameworkObjectFactory.GetInvoicingBaseForDisplayOtherTaxes(invoice).TaxRecordTransactionLinePivotForDisplay.TransactionLinesForOtherTaxesDisplay);

				taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = false;
				AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);

				taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
				AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);

				int prevCountOfEventHandlers = taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly;
				form.ValidateAndSave_ForTestOnly();

				AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
				AssertEquals("OnOtherTaxesCalculatedBeforePosting_Changed event handlers added by the form are unhooked on the invoice when Calculate Tax Transactions button becomes invisible", --prevCountOfEventHandlers, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
			}
		}

		public virtual void TestPreviewInvoiceMenuItem_OnOpeningFormWhenTaxFrameworkIsDisabled()
		{
			var invoice = GetInvoiceWithValidTestData();
			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				AssertEquals("Precondition: CalculateTaxTransactionsButton visibility", false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
				var actionsMenu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				actionsMenu.OnPopup(EventArgs.Empty);
				Assert("Preview Invoice Menu item accessibilty", actionsMenu.MenuItems.FindByText("Preview Invoice").Enabled);
			}
		}

		public virtual void TestPreviewInvoiceMenuItem_OnOpeningFormWhenTaxFrameworkIsEnabled()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var invoice = GetInvoiceWithValidTestData();
			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				var calculateTaxTransactionsButton = form.GetControl<ZButton>("CalculateTaxTransactionsButton", true);
				Assert("Precondition: CalculateTaxTransactionsButton visibility", calculateTaxTransactionsButton.Visible);
				Assert("Precondition: CalculateTaxTransactionsButton accessibilty", calculateTaxTransactionsButton.Enabled);

				var actionsMenu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				actionsMenu.OnPopup(EventArgs.Empty);
				Assert("Preview Invoice Menu item accessibility", !actionsMenu.MenuItems.FindByText("Preview Invoice").Enabled);
			}
		}

		public virtual void TestPreviewInvoiceMenuItem_WhenCalculateTaxTransactionsButtonDisabled()
		{
			AssertPreviewInvoiceMenuItem_WhenCalculateTaxTransactionsButtonDisabledAndEnabled(false);
		}

		public virtual void TestPreviewInvoiceMenuItem_WhenCalculateTaxTransactionsButtonDisabledAndEnabled()
		{
			AssertPreviewInvoiceMenuItem_WhenCalculateTaxTransactionsButtonDisabledAndEnabled(true);
		}

		void AssertPreviewInvoiceMenuItem_WhenCalculateTaxTransactionsButtonDisabledAndEnabled(bool clearTaxes)
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var invoice = GetInvoiceWithValidTestData(true);
			invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);

			TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				var actionsMenu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var calculateTaxTransactionsButton = form.GetControl<ZButton>("CalculateTaxTransactionsButton", true);
				Assert("Precondition: CalculateTaxTransactionsButton visibility, When TaxFramework is enabled", calculateTaxTransactionsButton.Visible);

				calculateTaxTransactionsButton.PerformClick();
				actionsMenu.OnPopup(EventArgs.Empty);
				Assert("Precondition: CalculateTaxTransactionsButton accessibility, When Tax Transactions are Calculated", !calculateTaxTransactionsButton.Enabled);
				Assert("Preview Invoice Menu item accessibility", actionsMenu.MenuItems.FindByText("Preview Invoice").Enabled);

				if (clearTaxes)
				{
					form.InvoiceDetails.GetControl<ZButton>("RemoveTaxTransactionsButton", true).PerformClick();
					actionsMenu.OnPopup(EventArgs.Empty);
					Assert("Precondition: CalculateTaxTransactionsButton accessibilty, When Tax Transactions are Cleared", calculateTaxTransactionsButton.Enabled);
					Assert("Preview Invoice Menu item accessibility", !actionsMenu.MenuItems.FindByText("Preview Invoice").Enabled);
				}
			}
		}

		public void TestIsTaxTransactionsCalculatedBeforePostingMakesInvoiceReadOnly()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var invoice = GetInvoiceWithValidTestData(true);
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			if (ShouldTestForOtherTaxes)
			{
				Assert(taxRecordParent.ShouldCalculateTaxTransactions);
			}
			else
			{
				Assert(!taxRecordParent.ShouldCalculateTaxTransactions);
				return;
			}

			var viewModelMock = new Mock<IInvoicingBaseTaxFrameworkViewModel>();
			TaxFrameworkObjectFactory.SubstituteInvoicingBaseTaxFrameworkViewModel_ForTestOnly(invoice, viewModelMock.Object);
			var componentsRestorerMock = new Mock<IGUIComponentsStateRestorer>();

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				form.SubstituteGUIComponentsStateRestorer_ForTestOnly(componentsRestorerMock.Object);
				ZButton[] buttonParams = new ZButton[2];
				MenuItem[] menuItemParams = new MenuItem[1];

				var transactionLinesGrid = form.GetControl<ZGrid>("TransactionLinesGrid");
				var bulkChargeImportButton = form.GetControl<ZButton>("BulkChargeImportButton");
				var apportionChargesButton = form.GetControl<ZButton>("ApportionChargesButton");
				var editApportionmentMenuItem = transactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
				var overrideBranchAndDepartmentMenuItem = form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Override Transaction Branch / Department");
				var previewInvoiceMenuItem = form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Preview Invoice");
				var ahDescTextBox = form.GetControl<ZTextBox>("AH_DescTextbox");
				var validInvoiceTotalCalcEdit = form.GetControl<ZCalcEdit>("ValidInvoiceTotalCalcEdit");
				var invoiceUserControl = form.GetControl<InvoiceUserControl>("InvoiceDetails");
				invoiceUserControl.ActivateOtherTaxesTab();
				var taxRecordsGrid = form.GetControl<ZGrid>("TaxRecordsGrid");
				ZCheckBox cashInvoiceOnCheckBox = null;
				try
				{
					cashInvoiceOnCheckBox = form.GetControl<ZCheckBox>("CashInvoiceOnCheckBox");
				}
				catch (AssertionFailedError)
				{
				}

				componentsRestorerMock.Setup(c => c.DisableControlsAndMenuItems(It.IsAny<ZButton[]>(), It.IsAny<MenuItem[]>())).Callback((ZButton[] p1, MenuItem[] p2) => { buttonParams = p1; menuItemParams = p2; });
				CombineAssertions(() =>
				{
					Assert(!transactionLinesGrid.ReadOnly);
					Assert(cashInvoiceOnCheckBox == null || !cashInvoiceOnCheckBox.ReadOnly);
					Assert(!ahDescTextBox.ReadOnly);
					Assert(!taxRecordsGrid.ReadOnly);
				});

				invoice.ValidateExpectedInvoiceTotal = true;
				taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
				CombineAssertions(() =>
				{
					Assert(transactionLinesGrid.ReadOnly);
					Assert(cashInvoiceOnCheckBox == null || cashInvoiceOnCheckBox.ReadOnly);
					Assert(!ahDescTextBox.ReadOnly);
					Assert(!validInvoiceTotalCalcEdit.ReadOnly);
					Assert(!taxRecordsGrid.ReadOnly);
					viewModelMock.Verify(v => v.TrackHasChanges(true), Times.Once);
					componentsRestorerMock.Verify(c => c.DisableControlsAndMenuItems(It.IsAny<ZButton[]>(), It.IsAny<MenuItem[]>()), Times.Once);
					AssertContainsExactElementsInAnyOrder(new[] { bulkChargeImportButton, apportionChargesButton }, buttonParams);
					AssertContainsExactElementsInAnyOrder(new[] { editApportionmentMenuItem, overrideBranchAndDepartmentMenuItem }, menuItemParams);
				});
				componentsRestorerMock.Verify(c => c.DisableMenuItemIfApplicableAndUpdateCurrentState(overrideBranchAndDepartmentMenuItem, true), Times.Never);
				form.ActionsMenuItem_ForTestOnly.ShowPopupMenu();
				if (overrideBranchAndDepartmentMenuItem != null)
				{
					componentsRestorerMock.Verify(c => c.DisableMenuItemIfApplicableAndUpdateCurrentState(overrideBranchAndDepartmentMenuItem, true), Times.Once);
				}

				componentsRestorerMock.Setup(c => c.RestoreControlsAndMenuItems(It.IsAny<ZButton[]>(), It.IsAny<MenuItem[]>())).Callback((ZButton[] p1, MenuItem[] p2) => { buttonParams = p1; menuItemParams = p2; });
				taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = false;
				CombineAssertions(() =>
				{
					Assert(!transactionLinesGrid.ReadOnly);
					Assert(cashInvoiceOnCheckBox == null || !cashInvoiceOnCheckBox.ReadOnly);
					viewModelMock.Verify(v => v.TrackHasChanges(false), Times.Once);
					componentsRestorerMock.Verify(c => c.RestoreControlsAndMenuItems(It.IsAny<ZButton[]>(), It.IsAny<MenuItem[]>()), Times.Once);
					AssertContainsExactElementsInAnyOrder(new[] { bulkChargeImportButton, apportionChargesButton }, buttonParams);
					AssertContainsExactElementsInAnyOrder(new[] { editApportionmentMenuItem, overrideBranchAndDepartmentMenuItem }, menuItemParams);
				});
			}
		}

		public void TestTaxTransactionsSummaryTab_Active_WhenIsTaxTransactionsCalculatedBeforePosting_True()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			AssertActivate_TaxTransactionsSummaryTab(true, (form) => AssertEquals("TaxTransactionSummaryTabPage", form.GetControl<ZTabControl>("TabControl").SelectedTab.Name));
		}

		public void TestTaxTransactionsSummaryTab_NotActive_WhenIsTaxTransactionsCalculatedBeforePosting_False()
		{
			AssertActivate_TaxTransactionsSummaryTab(false, (form) => AssertNotEquals("TaxTransactionSummaryTabPage", form.GetControl<ZTabControl>("TabControl").SelectedTab.Name));
		}

		void AssertActivate_TaxTransactionsSummaryTab(bool isTaxTransactionsCalculatedBeforePosting, Action<BaseInvoicingForm> testCaseSpecificAssert)
		{
			if (!ShouldTestForOtherTaxes)
			{
				Assert(true);
				return;
			}

			var invoice = GetInvoiceWithValidTestData();
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = isTaxTransactionsCalculatedBeforePosting;

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				testCaseSpecificAssert(form);
			}
		}

		public void TestCalculateOtherTaxesButtonDisabled_OnShown_When_TaxTransactionsAlreadyCalculatedIsTrue()
		{
			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: true, (invoice, _) => { }, (form) => AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton").Enabled));

			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: false, (invoice, _) => { }, (form) => AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton").Enabled));
		}

		[ExpectNoExceptions]
		public void TestDisableControlsAndMenuItems_Invoked_OnShown_When_TaxTransactionsAlreadyCalculatedIsTrue()
		{
			var componentsRestorerMock = new Mock<IGUIComponentsStateRestorer>();

			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: false,
				(invoice, form) =>
				{
					form.SubstituteGUIComponentsStateRestorer_ForTestOnly(componentsRestorerMock.Object);
				},
				(_) => componentsRestorerMock.Verify(c => c.DisableControlsAndMenuItems(It.IsAny<ZButton[]>(), It.IsAny<MenuItem[]>()), Times.Never));

			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: true,
				(invoice, form) =>
				{
					form.SubstituteGUIComponentsStateRestorer_ForTestOnly(componentsRestorerMock.Object);
				},
				(_) => componentsRestorerMock.Verify(c => c.DisableControlsAndMenuItems(It.IsAny<ZButton[]>(), It.IsAny<MenuItem[]>()), Times.Once));
		}

		[ExpectNoExceptions]
		public void TestTrackHasChanges_Invoked_OnShown_When_TaxTransactionsAlreadyCalculatedIsTrue()
		{
			var viewModelMock = new Mock<IInvoicingBaseTaxFrameworkViewModel>();

			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: false,
				(invoice, form) =>
				{
					TaxFrameworkObjectFactory.SubstituteInvoicingBaseTaxFrameworkViewModel_ForTestOnly(invoice, viewModelMock.Object);
				},
				(_) => viewModelMock.Verify(v => v.TrackHasChanges(It.IsAny<bool>()), Times.Never));

			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: true,
				(invoice, form) =>
				{
					TaxFrameworkObjectFactory.SubstituteInvoicingBaseTaxFrameworkViewModel_ForTestOnly(invoice, viewModelMock.Object);
				},
				(_) => viewModelMock.Verify(v => v.TrackHasChanges(true), Times.Once));
		}

		public void TestFormControls_ReadOnly_OnShown_When_TaxTransactionsAlreadyCalculatedIsTrue()
		{
			AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: false, (invoice, _) => { },
				(form) =>
				{
					if (ShouldTestCashInvoiceOnCheckBoxControl)
					{
						AssertEquals(false, form.GetControl<ZCheckBox>("CashInvoiceOnCheckBox").ReadOnly);
					}
					AssertEquals(false, form.GetControl<ZGrid>("TransactionLinesGrid").ReadOnly);
				});

			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: true, (invoice, _) => { },
				(form) =>
				{
					if (ShouldTestCashInvoiceOnCheckBoxControl)
					{
						AssertEquals(true, form.GetControl<ZCheckBox>("CashInvoiceOnCheckBox").ReadOnly);
					}
					AssertEquals(true, form.GetControl<ZGrid>("TransactionLinesGrid").ReadOnly);

					AssertEquals(false, form.GetControl<ZTextBox>("AH_DescTextbox").ReadOnly);
					var invoiceUserControl = form.GetControl<InvoiceUserControl>("InvoiceDetails");
					invoiceUserControl.ActivateOtherTaxesTab();
					AssertEquals(false, form.GetControl<ZGrid>("TaxRecordsGrid").ReadOnly);
				});
		}

		public void TestDisabledButtons_OnShown_When_TaxTransactionsAlreadyCalculatedIsTrue()
		{
			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: false, (invoice, _) => { },
				(form) =>
				{
					AssertEquals(true, form.GetControl<ZButton>("BulkChargeImportButton").Enabled);
					AssertEquals(true, form.GetControl<ZButton>("ApportionChargesButton").Enabled);
				});

			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: true, (invoice, _) => { },
				(form) =>
				{
					AssertEquals(false, form.GetControl<ZButton>("BulkChargeImportButton").Enabled);
					AssertEquals(false, form.GetControl<ZButton>("ApportionChargesButton").Enabled);
				});
		}

		public void TestDisabledMenuItems_OnShown_When_TaxTransactionsAlreadyCalculatedIsTrue()
		{
			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: false, (invoice, _) => { },
				(form) =>
				{
					var editApportionmentMenuItem = form.GetControl<ZGrid>("TransactionLinesGrid").ContextMenu.MenuItems.FindByText("Edit Apportionment");
					Assert(editApportionmentMenuItem == null || editApportionmentMenuItem.Enabled);
					AssertEquals(true, form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Override Transaction Branch / Department").Enabled);
				});

			AssertCalculateTaxTransactionsButtonEnabled(isTaxTransactionsCalculatedBeforePosting: true, (invoice, _) => { },
				(form) =>
				{
					var editApportionmentMenuItem = form.GetControl<ZGrid>("TransactionLinesGrid").ContextMenu.MenuItems.FindByText("Edit Apportionment");
					Assert(editApportionmentMenuItem == null || !editApportionmentMenuItem.Enabled);
					AssertEquals(false, form.ActionsMenuItem_ForTestOnly.MenuItems.FindByText("Override Transaction Branch / Department").Enabled);
				});
		}

		void AssertCalculateTaxTransactionsButtonEnabled(bool isTaxTransactionsCalculatedBeforePosting, Action<InvoicingBase, BaseInvoicingForm> testCaseSpecificSetup, Action<BaseInvoicingForm> testCaseSpecificAssert)
		{
			if (!ShouldTestForOtherTaxes)
			{
				Assert(true);
				return;
			}

			var invoice = GetInvoiceWithValidTestData(true);
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

			if (isTaxTransactionsCalculatedBeforePosting)
			{
				taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = isTaxTransactionsCalculatedBeforePosting;
				var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
				mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);
			}

			using (var form = GetFormByInvoice(invoice))
			{
				testCaseSpecificSetup(invoice, form);

				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();
				testCaseSpecificAssert(form);
			}
		}

		public void TestTrackingHasChangesSuspendedWhenSaveAsIncomplete()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var invoice = GetInvoiceWithValidTestData(true);
			if (invoice.AH_Ledger != LedgerTypes.AccountsPayable)
			{
				Assert("Cannot be saved as Incomplete", true);
				return;
			}
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			if (ShouldTestForOtherTaxes)
			{
				Assert(taxRecordParent.ShouldCalculateTaxTransactions);
			}
			else
			{
				Assert(!taxRecordParent.ShouldCalculateTaxTransactions);
				return;
			}
			invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
			TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
				form.SaveAsIncomplete_ForTestOnly();
				var apportionChargesButton = form.GetControl<ZButton>("ApportionChargesButton");
				Assert(!apportionChargesButton.Enabled);
				form.ValidateAndSave_ForTestOnly();
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestOnOtherTaxesCalculated_Changed_UnhookedOnDispose()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();

			var org = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);

			Factory.Save();

			var invoice = GetInvoiceWithValidTestData(true);
			invoice.AH_OH = org.PK;
			var chargeCode = TestObjectCreator.NonAccrualChargeCode;
			var line = TestObjectCreator.CreateInvoiceLine(invoice, null, chargeCode, 100M);
			line.GenericCharge = chargeCode.PK;
			line.AL_GE = TestObjectCreator.FIADepartment.PK;

			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			AssertEquals(ShouldTestForOtherTaxes, taxRecordParent.ShouldCalculateTaxTransactions);

			ZUserControl control = null;
			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				control = form.GetControl<InvoiceUserControl>("InvoiceDetails", true);
				Assert(!control.IsDisposed);
				Application.DoEvents();

				if (ShouldTestForOtherTaxes)
				{
					AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
					AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);
				}
				else
				{
					AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
					return;
				}

				AssertEquals("OnOtherTaxesCalculatedBeforePosting_Changed event handlers added by the BaseInvoicingForm and InvoiceUserControl", 2, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
			}

			Assert(control.IsDisposed);
			AssertEquals("No OnOtherTaxesCalculatedBeforePosting_Changed event handlers after BaseInvoicingForm and InvoiceUserControl are disposed", 0, taxRecordParent.OnOtherTaxesCalculatedBeforePosting_ChangedEventHandlerCount_ForTestOnly);
		}

		public void TestCalculateOtherTaxesButton_CalculatesTaxRecords()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001", true);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();

			Factory.Save();

			var invoice = GetInvoiceWithValidTestData(true);

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				var factory2 = new BusinessObjectFactory();
				var accountingTestObjectCreator = new MasterFiles.Business.Testing.Accounting.Helpers.AccountingTestObjectCreator(factory2);

				var company = GlbCompany.CurrentCompany;
				var org = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);
				var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "test charge code", invoice.AH_Ledger == LedgerTypes.AccountsReceivable ? Constants.ChargeType.Revenue : Constants.ChargeType.Margin, 100M, TestObjectCreator.GST1, null, company);
				accountingTestObjectCreator.SetupMinimumSettingsForTaxFramework(company, org, chargeCode, invoice.AH_Ledger, TaxRateSources.TaxIDOnly.Code);

				invoice.AH_OH = org.PK;
				var line = TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCode, 100M, TestObjectCreator.AUD, 1M, "test charge");
				line.GenericCharge = chargeCode.PK;

				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();

					if (ShouldTestForOtherTaxes)
					{
						AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
						AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);
					}
					else
					{
						AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
						return;
					}

					form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).PerformClick();

					var expectedMessage = "There are errors that need to be corrected before this operation can be performed.";
					AssertEquals("Validation error prevented from calculating Tax Transactions", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					expectedMessage = "Error - AL_GE: Cannot issue job charges for a miscellaneous department.";
					AssertEquals(expectedMessage, form.BusinessEntity.GetErrors().ToMessageListString());

					AssertEquals("Calculate other tax button should be kept enabled since other tax has not been calculated.", true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);

					var taxRecords = Factory.Load<AccTaxTransaction>(new ZQuery());
					AssertEquals("No tax transaction has been created, since other tax has not been calculated due to validation error", 0, taxRecords.Length);

					UnitTestUserNotification.Instance.ClearMessages();

					line.AL_GE = TestObjectCreator.FIADepartment.PK;

					invoice.ValidateExpectedInvoiceTotal = true;
					invoice.ExpectedInvoiceTotal = 1000m;
					AssertNotEquals("Precondition: Invoice Total does not equal to Expected Invoice Total", invoice.AH_OSTotalAmount, invoice.ExpectedInvoiceTotal);

					form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).PerformClick();
					AssertEquals("No more validation error should be there", null, UnitTestUserNotification.Instance.LastMessage.Text);

					taxRecords = Factory.Load<AccTaxTransaction>(new ZQuery());
					AssertEquals("taxRecords.Count", 1, taxRecords.Length);

					AssertEquals("Calculate other tax button is disabled since other tax has been calculated once already.", false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);

					try
					{
						form.ValidateAndSave_ForTestOnly();
					}
					catch
					{
					}
					AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				}
			}
			else
			{
				Assert("Not applicable for IncompleteTransaction and UnapprovedTransaction", true);
			}
		}

		public void TestCalculateOtherTaxesButton_ShowsMessageWhenExceptionOccurs()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001", true);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();

			Factory.Save();

			var invoice = GetInvoiceWithValidTestData(true);

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				var factory2 = new BusinessObjectFactory();
				var accountingTestObjectCreator = new MasterFiles.Business.Testing.Accounting.Helpers.AccountingTestObjectCreator(factory2);

				var company = GlbCompany.CurrentCompany;
				var org = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);
				var companyData = org.GetCompanyDataForGlbCompany(company);
				var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "test charge code", invoice.AH_Ledger == LedgerTypes.AccountsReceivable ? Constants.ChargeType.Revenue : Constants.ChargeType.Margin, 100M, TestObjectCreator.GST1, null, company);

				var taxAuthority = accountingTestObjectCreator.CreateTaxAuthority("TA");

				var taxSystem = accountingTestObjectCreator.CreateTaxSystem("TS");
				taxSystem.TaxBaseCalculationMethod = "RAN";

				var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
				taxSystemsConfigCollection.Add(taxSystem);

				var taxConfigCompany = accountingTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, invoice.AH_Ledger);

				using (AccountingMasterFilesRegistry.Instance.TaxSystems.DataType.SuspendValidation())
				{
					AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
				}
				taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(factory2);

				factory2.Save();

				accountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany, companyData);

				var taxFrameTaxOverrideGroup = accountingTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigCompany);
				accountingTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup, chargeCode);

				var taxID = accountingTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.TaxIDOnly.Code);
				var numerator = 10;
				var denominator = 1;
				taxID.SetRate_ForTestOnly(numerator, denominator);

				accountingTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup, taxID.PK);

				factory2.Save();

				invoice.AH_OH = org.PK;
				var line = TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCode, 100M, TestObjectCreator.AUD, 1M, "test charge");
				line.GenericCharge = chargeCode.PK;
				line.AL_GE = TestObjectCreator.FIADepartment.PK;

				using (var form = GetFormByInvoice(invoice))
				{
					var language = Enterprise.Core.Constants.Languages.Spanish;
					using (Res.TemporarilySwitchLanguage(language))
					using (var mockCache = Res.GetLanguageInstance(language).UseMockData())
					{
						var expectedErrorMessageInSpanish = "Sistema impositivo TaxBaseCalculationMethod RAN no es compatible.";
						var expectedErrorMessageInEnglish = "Tax Base Calculation Method RAN for the Tax System is not supported.";
						mockCache.Put("7E293DBA-AE13-4FBA-9729-163D2CC97B0F", new ResourceStringData("7E293DBA-AE13-4FBA-9729-163D2CC97B0F", expectedErrorMessageInSpanish));

						form.DisplayMode = ODisplayMode.Edit;
						form.Show();
						Application.DoEvents();
						if (ShouldTestForOtherTaxes)
						{
							AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
							AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);
						}
						else
						{
							AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
							return;
						}

						form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).PerformClick();

						AssertNotNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(expectedErrorMessageInSpanish, UnitTestUserNotification.Instance.LastMessage.Text);

						AssertNotNull("LastExceptionReported", ErrorReporter.LastExceptionReported);
						AssertEquals("LastExceptionReported.Message", expectedErrorMessageInEnglish, ErrorReporter.LastExceptionReported.Message);

						AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);

						ErrorReporter.Clear();
					}
				}
			}
		}

		[TestDate(2020, 01, 30)]
		public void TestCalculateOtherTaxes_WhenNoTaxRateFound()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001", true);
			var job = TestObjectCreator.CreateJob(shipment, false);

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();

			Factory.Save();

			var invoice = GetInvoiceWithValidTestData(true);

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				var factory2 = new BusinessObjectFactory();
				var accountingTestObjectCreator = new MasterFiles.Business.Testing.Accounting.Helpers.AccountingTestObjectCreator(factory2);

				var company = GlbCompany.CurrentCompany;
				var org = TestObjectCreator.CreateOrgHeader("TESTORG", true, true);
				var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "test charge code", invoice.AH_Ledger == LedgerTypes.AccountsReceivable ? Constants.ChargeType.Revenue : Constants.ChargeType.Margin, 100M, TestObjectCreator.GST1, null, company);

				var companyData = org.GetCompanyDataForGlbCompany(company);
				var taxAuthority = accountingTestObjectCreator.CreateTaxAuthority("TA");
				var taxSystem = accountingTestObjectCreator.CreateTaxSystem("TS");

				var taxConfigCompany = accountingTestObjectCreator.CreateTaxConfiguration(company, taxAuthority, taxSystem, invoice.AH_Ledger);

				var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
				taxSystemsConfigCollection.Add(taxSystem);
				AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
				taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(factory2);
				factory2.Save();

				var orgConfig = accountingTestObjectCreator.CreateOrgTaxConfiguration(taxConfigCompany, companyData);
				accountingTestObjectCreator.AddOrgTaxRateItem(orgConfig, ZDate.Today.AddDays(-5), ZDate.Today.AddDays(-1), RateSourceMethods.ManualOverride.Code, (10, 1));

				var taxFrameTaxOverrideGroup1 = accountingTestObjectCreator.CreateTaxOverrideGroup(company, taxConfigCompany);
				accountingTestObjectCreator.CreateTaxOverrideGroupChargeCodePivot(taxFrameTaxOverrideGroup1, chargeCode);

				var taxID1 = accountingTestObjectCreator.CreateTaxRate_RateSource(TaxRateSources.OrganisationOnly.Code, "GS1");

				accountingTestObjectCreator.CreateTaxOverrideRule(taxFrameTaxOverrideGroup1, taxID1.PK);

				factory2.Save();

				invoice.AH_OH = org.PK;
				var line = TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCode, 100M, TestObjectCreator.AUD, 1M, "test charge");
				line.GenericCharge = chargeCode.PK;

				line.AL_GE = TestObjectCreator.FIADepartment.PK;

				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();
					if (ShouldTestForOtherTaxes)
					{
						AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
						AssertEquals(true, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Enabled);
					}
					else
					{
						AssertEquals(false, form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).Visible);
						return;
					}

					invoice.RunPreSaveValidation();

					AssertNoErrors("Precondition: invoice has no errors", invoice);

					form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).PerformClick();

					var taxRecords = Factory.Load<AccTaxTransaction>(new ZQuery());
					AssertEquals("taxRecords.Count", 1, taxRecords.Length);
					var taxRecord = taxRecords[0];
					AssertEquals("ATT_RateNumerator", 0, taxRecord.ATT_RateNumerator);
					AssertHasError("ATT_RateInfo", taxRecord.ATT_RateInfo, "No valid tax rate found for tax ID 'GS1'. Please check the Rate Source of the tax ID and make sure there are valid tax rate for the Rate Source.");
					Assert("invoice.HasErrors", invoice.HasErrors);
				}
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestCalculateTaxTransactions_SuspendTaxRecordsGridOnProcessTaxes()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001", true);
			var job = TestObjectCreator.CreateJob(shipment, false);

			Factory.Save();

			var invoice = GetInvoiceWithValidTestData(true);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			if (ShouldTestForOtherTaxes && (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.AccountsReceivable))
			{
				var company = GlbCompany.CurrentCompany;
				var org = TestObjectCreator.TestOrganisation;
				var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "test charge code", invoice.AH_Ledger == LedgerTypes.AccountsReceivable ? Constants.ChargeType.Revenue : Constants.ChargeType.Margin, 100M, TestObjectCreator.GST1, null, company);

				var factory = new BusinessObjectFactory();
				var accountingTestObjectCreator = new MasterFiles.Business.Testing.Accounting.Helpers.AccountingTestObjectCreator(factory);
				accountingTestObjectCreator.SetupMinimumSettingsForTaxFramework(company, org, chargeCode, invoice.AH_Ledger, TaxRateSources.TaxIDOnly.Code);

				invoice.AH_OH = org.PK;
				var line = TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCode, 100M, TestObjectCreator.AUD, 1M, "test charge");
				line.GenericCharge = chargeCode.PK;
				line.AL_GE = TestObjectCreator.FIADepartment.PK;

				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();

					form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).PerformClick();

					var invoiceTaxTransactionsControl = form.InvoiceDetails.GetControl<InvoiceOtherTaxesControl>("InvoiceTaxTransactionsControl");
					var pointingToTransactionLines = invoiceTaxTransactionsControl.GetField("PointingToTransactionLines") as ZToolStripButton;
					var taxRecordsGrid = invoiceTaxTransactionsControl.GetField("TaxRecordsGrid") as ZGrid;
					var linesGrid = invoiceTaxTransactionsControl.GetField("LinesGrid") as ZGrid;

					AssertGridCount(1, taxRecordsGrid.VisibleRowCount, linesGrid.VisibleRowCount);

					taxRecordsGrid.Select(0);
					pointingToTransactionLines.PerformClick();
					AssertEquals(1, linesGrid.VisibleRowCount);

					form.InvoiceDetails.RemoveTaxTransactionsButton_ForTestOnly.PerformClick();
					AssertGridCount(0, taxRecordsGrid.VisibleRowCount, linesGrid.VisibleRowCount);

					form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).PerformClick();
					AssertGridCount(1, taxRecordsGrid.VisibleRowCount, linesGrid.VisibleRowCount);

					taxRecordsGrid.Select(0);
					pointingToTransactionLines.PerformClick();
					AssertEquals(1, linesGrid.VisibleRowCount);

					void AssertGridCount(int expectedCount, params int[] gridCount)
					{
						gridCount.ForEach(item =>
						{
							AssertEquals(expectedCount, item);
						});
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestRemoveTaxTransactionsButtonShouldNotChangeLineChargesGridToEditable()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001", true);
			var job = TestObjectCreator.CreateJob(shipment, false);

			Factory.Save();

			var invoice = GetInvoiceWithValidTestData(true);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			if (ShouldTestForOtherTaxes && (invoice.AH_Ledger == LedgerTypes.AccountsPayable || invoice.AH_Ledger == LedgerTypes.AccountsReceivable))
			{
				var company = GlbCompany.CurrentCompany;
				var org = TestObjectCreator.TestOrganisation;
				var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "test charge code", invoice.AH_Ledger == LedgerTypes.AccountsReceivable ? Constants.ChargeType.Revenue : Constants.ChargeType.Margin, 100M, TestObjectCreator.GST1, null, company);

				var factory = new BusinessObjectFactory();
				var accountingTestObjectCreator = new MasterFiles.Business.Testing.Accounting.Helpers.AccountingTestObjectCreator(factory);
				accountingTestObjectCreator.SetupMinimumSettingsForTaxFramework(company, org, chargeCode, invoice.AH_Ledger, TaxRateSources.TaxIDOnly.Code);

				invoice.AH_OH = org.PK;
				var line = TestObjectCreator.CreateInvoiceLine(invoice, job, chargeCode, 100M, TestObjectCreator.AUD, 1M, "test charge");
				line.GenericCharge = chargeCode.PK;
				line.AL_GE = TestObjectCreator.FIADepartment.PK;

				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();

					form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).PerformClick();

					var invoiceTaxTransactionsControl = form.InvoiceDetails.GetControl<InvoiceOtherTaxesControl>("InvoiceTaxTransactionsControl");
					var pointingToTransactionLines = invoiceTaxTransactionsControl.GetField("PointingToTransactionLines") as ZToolStripButton;
					var taxRecordsGrid = invoiceTaxTransactionsControl.GetField("TaxRecordsGrid") as ZGrid;
					var linesChargesGrid = form.LineChargesGrid;

					taxRecordsGrid.Select(0);
					pointingToTransactionLines.PerformClick();
					Assert(linesChargesGrid.ReadOnly);

					form.InvoiceDetails.RemoveTaxTransactionsButton_ForTestOnly.PerformClick();
					Assert(linesChargesGrid.ReadOnly);

					form.GetControl<ZButton>("CalculateTaxTransactionsButton", true).PerformClick();
					Assert(linesChargesGrid.ReadOnly);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestInvoiceTotalsForOtherTaxes()
		{
			var invoice = GetInvoiceWithValidTestData(true);
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.Code;
			Assert(!taxRecordParent.ShouldCalculateTaxTransactions);

			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert(!form.GetControl<ZCalcFindBox>("AH_OSSubTotalAmountCalcEdit", true).Visible);
				Assert(!form.GetControl<ZCalcFindBox>("AH_OSOtherTaxesAmountCalcEdit", true).Visible);
				Assert(!form.GetControl<ZCalcFindBox>("AH_LocalSubTotalAmountCalcEdit", true).Visible);
				Assert(!form.GetControl<ZCalcFindBox>("AH_LocalOtherTaxesAmountCalcEdit", true).Visible);
			}

			TaxFrameworkTestObjectCreator.CreateTaxConfiguration(invoice.Company, TaxConfigurationLedgers.AccountsPayable.Code);
			TaxFrameworkTestObjectCreator.CreateTaxConfiguration(invoice.Company, TaxConfigurationLedgers.AccountsReceivable.Code);
			AssertEquals(ShouldTestForOtherTaxes, taxRecordParent.ShouldCalculateTaxTransactions);

			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(ShouldTestForOtherTaxes, form.GetControl<ZCalcFindBox>("AH_OSSubTotalAmountCalcEdit", true).Visible);
				AssertEquals(ShouldTestForOtherTaxes, form.GetControl<ZCalcFindBox>("AH_OSOtherTaxesAmountCalcEdit", true).Visible);
				Assert(!form.GetControl<ZCalcFindBox>("AH_LocalSubTotalAmountCalcEdit", true).Visible);
				Assert(!form.GetControl<ZCalcFindBox>("AH_LocalOtherTaxesAmountCalcEdit", true).Visible);

				invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
				AssertEquals(ShouldTestForOtherTaxes, form.GetControl<ZCalcFindBox>("AH_OSSubTotalAmountCalcEdit", true).Visible);
				AssertEquals(ShouldTestForOtherTaxes, form.GetControl<ZCalcFindBox>("AH_OSOtherTaxesAmountCalcEdit", true).Visible);
				AssertEquals(ShouldTestForOtherTaxes, form.GetControl<ZCalcFindBox>("AH_LocalSubTotalAmountCalcEdit", true).Visible);
				AssertEquals(ShouldTestForOtherTaxes, form.GetControl<ZCalcFindBox>("AH_LocalOtherTaxesAmountCalcEdit", true).Visible);

				Factory.Save();
			}

			Assert(invoice.IsInDatabase);
			var taxRecord = Factory.New<AccTaxTransaction>();
			taxRecord.ATT_AH = invoice.PK;

			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert(form.GetControl<ZCalcFindBox>("AH_OSSubTotalAmountCalcEdit", true).Visible);
				Assert(form.GetControl<ZCalcFindBox>("AH_OSOtherTaxesAmountCalcEdit", true).Visible);
				Assert(form.GetControl<ZCalcFindBox>("AH_LocalSubTotalAmountCalcEdit", true).Visible);
				Assert(form.GetControl<ZCalcFindBox>("AH_LocalOtherTaxesAmountCalcEdit", true).Visible);
			}
		}

		public virtual void TestTotalsForOtherTaxesOnReversal()
		{
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);
			var anyLedgerTaxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, TaxConfigurationLedgers.AccountsReceivable.Code);

			var reversalInvoiceWithoutTaxRecords = CreateReversalTransaction(false);
			var reversalInvoiceWithTaxRecords = CreateReversalTransaction(true);

			using (var form = new BaseInvoicingForm(reversalInvoiceWithoutTaxRecords))
			{
				form.Show();
				Application.DoEvents();
				Assert(!form.GetControl<ZCalcFindBox>("AH_OSSubTotalAmountCalcEdit", true).Visible);
				Assert(!form.GetControl<ZCalcFindBox>("AH_OSOtherTaxesAmountCalcEdit", true).Visible);
				Assert(!form.GetControl<ZCalcFindBox>("AH_LocalSubTotalAmountCalcEdit", true).Visible);
				Assert(!form.GetControl<ZCalcFindBox>("AH_LocalOtherTaxesAmountCalcEdit", true).Visible);
			}

			using (var form = new BaseInvoicingForm(reversalInvoiceWithTaxRecords))
			{
				form.Show();
				Application.DoEvents();
				Assert(form.GetControl<ZCalcFindBox>("AH_OSSubTotalAmountCalcEdit", true).Visible);
				Assert(form.GetControl<ZCalcFindBox>("AH_OSOtherTaxesAmountCalcEdit", true).Visible);
				Assert(form.GetControl<ZCalcFindBox>("AH_LocalSubTotalAmountCalcEdit", true).Visible);
				Assert(form.GetControl<ZCalcFindBox>("AH_LocalOtherTaxesAmountCalcEdit", true).Visible);
			}

			InvoicingBase CreateReversalTransaction(bool addTaxRecords)
			{
				InvoicingBase invoice = GetInvoiceWithValidTestData(true);
				invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;

				if (addTaxRecords)
				{
					var taxRecord = Factory.New<AccTaxTransaction>();
					taxRecord.ATT_AH = invoice.PK;
					taxRecord.ATT_ETC = anyLedgerTaxConfig.PK;
					taxRecord.ATT_GC = GlbCompany.CurrentCompany.PK;
				}

				var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);

				Assert("Precondition: ShouldCalculateTaxTransactions on original transaction", taxRecordParent.ShouldCalculateTaxTransactions);

				TestObjectCreator.CreateReversalTransaction(invoice, "");
				Assert("Precondition: IsReverseTransaction", invoice.ReverseInvoice.IsReverseTransaction);

				var reversalInvoice = invoice.ReverseInvoice;
				var taxRecordReversalParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(reversalInvoice);
				Assert("Precondition: ShouldCalculateTaxTransactions on reversal transaction", !taxRecordReversalParent.ShouldCalculateTaxTransactions);
				if (addTaxRecords)
				{
					var reversalTaxRecords = Factory.Load<AccTaxTransaction>(new ZQuery(AccTaxTransactionSchema.ATT_AH, reversalInvoice.PK));
					AssertEquals("Precondition: reversalTaxRecords.Length", 1, reversalTaxRecords.Length);
				}

				return reversalInvoice;
			}
		}

		[ExpectNoExceptions]
		public void TestTrackHasChanges_WhenDeletingAnInvoice()
		{
			if (!ShouldTestForOtherTaxes)
			{
				Assert(true);
				return;
			}
			var viewModelMock = new Mock<IInvoicingBaseTaxFrameworkViewModel>();
			var invoice = GetInvoiceWithValidTestData();
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			using (var testForm = GetFormByInvoice(invoice))
			{
				TaxFrameworkObjectFactory.SubstituteInvoicingBaseTaxFrameworkViewModel_ForTestOnly(invoice, viewModelMock.Object);
				testForm.DisplayMode = ODisplayMode.Delete;
				testForm.Show();
				Application.DoEvents();
				viewModelMock.Verify(v => v.TrackHasChanges(true), IsTrackHasChangesCalledWhenDeleteingAnInvoice ? Times.Once : Times.Never);
			}
		}

		[ExpectNoExceptions]
		public void TestTrackHasChanges_IsInvoked_WhenEditingAnInvoice()
		{
			if (!ShouldTestForOtherTaxes)
			{
				Assert(true);
				return;
			}
			var viewModelMock = new Mock<IInvoicingBaseTaxFrameworkViewModel>();
			var invoice = GetInvoiceWithValidTestData();
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			taxRecordParent.IsTaxTransactionsCalculatedBeforePosting = true;
			var mockITaxFrameworkConfigurationHelper = TaxFrameworkTestObjectCreator.SetupMockTaxFrameworkConfigurationHelper();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(true);

			using (var testForm = GetFormByInvoice(invoice))
			{
				TaxFrameworkObjectFactory.SubstituteInvoicingBaseTaxFrameworkViewModel_ForTestOnly(invoice, viewModelMock.Object);
				testForm.DisplayMode = ODisplayMode.Edit;
				testForm.Show();
				Application.DoEvents();
				viewModelMock.Verify(v => v.TrackHasChanges(true), Times.Once);
			}
		}

		protected virtual bool ShouldTestForOtherTaxes => true;

		protected virtual bool ShouldTestCashInvoiceOnCheckBoxControl => true;

		protected virtual bool IsTrackHasChangesCalledWhenDeleteingAnInvoice => true;

		#endregion

		public void TestSetInvoiceHeaderDefaults()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				var modeToSet = ODisplayMode.New;
				form.DisplayMode = modeToSet;
				form.Show();
				Application.DoEvents();

				bool isAPInvoice = (form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.AccountsPayable ||
									form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
									form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.IncompleteTransactions);
				var expectedMode = form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.IncompleteTransactions ? ODisplayMode.Edit : modeToSet;
				AssertEquals("Precondition: Form display mode", expectedMode, form.DisplayMode);
				AssertEquals("InvoiceTotalValidationCheckBox.Visible", isAPInvoice, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
				AssertEquals("ValidInvoiceTotalCalcEdit.Visible", isAPInvoice, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);

				AssertEquals(false, form.Invoice_ForTestOnly.IsDisbursementOrFinal);
				AssertEquals(!isAPInvoice ? InvoiceTypesList.Codes.FinalInvoice : TransactionCategory.Codes.Standard, form.Invoice_ForTestOnly.AH_TransactionCategory);
			}

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Invoice_ForTestOnly.AH_TransactionCategory = "";
				var modeToSet = ODisplayMode.Browse;
				form.DisplayMode = modeToSet;
				form.Show();
				Application.DoEvents();

				bool expectedVisibleValue = form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.IncompleteTransactions;

				var expectedMode = form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.IncompleteTransactions ? ODisplayMode.Edit : modeToSet;
				AssertEquals("Precondition: Form display mode", expectedMode, form.DisplayMode);
				AssertEquals("InvoiceTotalValidationCheckBox.Visible", expectedVisibleValue, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
				AssertEquals("ValidInvoiceTotalCalcEdit.Visible", expectedVisibleValue, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);

				AssertEquals(false, form.Invoice_ForTestOnly.IsDisbursementOrFinal);
				AssertEquals("", form.Invoice_ForTestOnly.AH_TransactionCategory);
			}

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				Application.DoEvents();

				bool isAPInvoice = (form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.AccountsPayable ||
									form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
									form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.IncompleteTransactions);
				AssertEquals("Precondition: Form display mode", ODisplayMode.Edit, form.DisplayMode);
				AssertEquals("InvoiceTotalValidationCheckBox.Visible", isAPInvoice, form.InvoiceDetails.InvoiceTotalValidationCheckBox.Visible);
				AssertEquals("ValidInvoiceTotalCalcEdit.Visible", isAPInvoice, form.InvoiceDetails.ValidInvoiceTotalCalcEdit.Visible);
			}

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Invoice_ForTestOnly.AH_TransactionCategory = "";
				form.Show();
				Application.DoEvents();

				bool isAPInvoice = (form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.AccountsPayable ||
									form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
									form.Invoice_ForTestOnly.AH_Ledger == LedgerTypes.IncompleteTransactions);
				AssertEquals("IsDisbursementInvoiceCheckBox.Visible", true, form.InvoiceDetails.IsDisbursementInvoiceCheckBox.Visible);
				AssertEquals("InvoiceTermLabel.Visible", !isAPInvoice, form.InvoiceDetails.AH_InvoiceTermDropEdit.GetExtension<LabelCaptionRenderer>().Visible);
				AssertEquals("TermDaysLabel.Visible", !isAPInvoice, form.InvoiceDetails.AH_InvoiceTermDaysCalcEdit.GetExtension<LabelCaptionRenderer>().Visible);
				AssertEquals("AH_InvoiceTermDaysCalcEdit.Visible", !isAPInvoice, form.InvoiceDetails.AH_InvoiceTermDaysCalcEdit.Visible);
				AssertEquals("AH_InvoiceTermDropEdit.Visible", !isAPInvoice, form.InvoiceDetails.AH_InvoiceTermDropEdit.Visible);

				AssertEquals(false, form.Invoice_ForTestOnly.IsDisbursementOrFinal);
				AssertEquals("", form.Invoice_ForTestOnly.AH_TransactionCategory);
			}

			var currCompany = GlbCompany.CurrentCompany;

			using (currCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("CashBasisVATIndicatorCheckbox.Visible", false, form.InvoiceDetails.CashBasisVATIndicatorCheckbox.Visible);
				AssertEquals("AH_OSTaxAmountCalcEdit displays correct tax name", "GST", form.AH_OSTaxAmountCalcEdit.CaptionResourceString.Caption);
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
				AssertEquals("AH_OSTaxAmountCalcEdit displays correct tax name", "GST in Local Currency", form.AH_LocalTaxAmountCalcEdit.CaptionResourceString.Caption);
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.Ireland))
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("AH_OSTaxAmountCalcEdit displays correct tax name", "VAT", form.AH_OSTaxAmountCalcEdit.CaptionResourceString.Caption);
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
				AssertEquals("AH_OSTaxAmountCalcEdit displays correct tax name", "VAT in Local Currency", form.AH_LocalTaxAmountCalcEdit.CaptionResourceString.Caption);
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.France))
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("AH_OSTaxAmountCalcEdit displays correct tax name", "TVA", form.AH_OSTaxAmountCalcEdit.CaptionResourceString.Caption);
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.Code;
				AssertEquals("AH_OSTaxAmountCalcEdit displays correct tax name", "TVA in Local Currency", form.AH_LocalTaxAmountCalcEdit.CaptionResourceString.Caption);
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.Thailand))
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("CashBasisVATIndicatorCheckbox.Visible", true, form.InvoiceDetails.CashBasisVATIndicatorCheckbox.Visible);
			}

			var regEnableComplDocModule = AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule;
			var currCompPK = Env.CurrentCompanyPK;
			foreach (string country in TestObjectCreator.CountriesSupportComplianceSubtype)
			{
				using (currCompany.TemporarilySetCountry(country))
				{
					using (regEnableComplDocModule.SetTemporaryValue(currCompPK, Guid.Empty, Guid.Empty, true))
					using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();
						var details = form.InvoiceDetails;

						var subType = details.AH_ComplianceSubTypeDropEdit;
						Assert("NOT AH_ComplianceSubTypeDropEdit.Visible", !subType.Visible);
						Assert("NOT AH_ComplianceSubTypeDropEdit.GetExtension<LabelCaptionRenderer>() visible", !subType.GetExtension<LabelCaptionRenderer>().Visible);

						Assert("NOT AH_TransactionReferenceTextBox.Visible", !details.AH_TransactionReferenceTextBox.Visible);
						Assert("NOT ComplianceSequenceTextBox.Visible", !details.ComplianceSequenceTextBox.Visible);
					}

					using (regEnableComplDocModule.SetTemporaryValue(currCompPK, Guid.Empty, Guid.Empty, false))
					using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();
						var details = form.InvoiceDetails;

						var subType = details.AH_ComplianceSubTypeDropEdit;
						Assert("AH_ComplianceSubTypeDropEdit.Visible", subType.Visible);
						Assert("NOT AH_ComplianceSubTypeDropEdit.ReadOnly", !subType.ReadOnly);
						Assert("AH_ComplianceSubTypeDropEdit.GetExtension<LabelCaptionRenderer>() visible", subType.GetExtension<LabelCaptionRenderer>().Visible);

						var number = details.AH_TransactionReferenceTextBox;
						Assert("AH_TransactionReferenceTextBox.Visible", number.Visible);
						Assert("AH_TransactionReferenceTextBox.ReadOnly", number.ReadOnly);

						var sequence = details.ComplianceSequenceTextBox;
						Assert("ComplianceSequenceTextBox.Visible", sequence.Visible);
						Assert("ComplianceSequenceTextBox.ReadOnly", sequence.ReadOnly);
						AssertNull(form.Invoice_ForTestOnly.ComplianceSequence);
						AssertNullOrEmpty(sequence.Text);
					}
				}
			}

			using (currCompany.TemporarilySetCountry(CountryCodes.Afghanistan))
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				var details = form.InvoiceDetails;

				var subType = details.AH_ComplianceSubTypeDropEdit;
				Assert("NOT AH_ComplianceSubTypeDropEdit.Visible", !subType.Visible);
				Assert("AH_ComplianceSubTypeDropEdit.GetExtension<LabelCaptionRenderer>() NOT visible", !subType.GetExtension<LabelCaptionRenderer>().Visible);

				Assert("NOT AH_TransactionReferenceTextBox.Visible", !details.AH_TransactionReferenceTextBox.Visible);
				Assert("NOT ComplianceSequenceTextBox.Visible", !details.ComplianceSequenceTextBox.Visible);
			}
		}

		public void TestIsDisbursementInvoiceCheckBoxBindingForIncompleteInvoice()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice.AH_TransactionType == TransactionTypes.Invoice)
			{
				invoice.SaveAsIncomplete();

				using (BaseInvoicingForm form = GetFormByInvoice(invoice))
				{
					form.Show();
					Application.DoEvents();

					AssertEquals("IsDisbursementInvoiceCheckBox.Visible", true, form.InvoiceDetails.IsDisbursementInvoiceCheckBox.Visible);
					AssertEquals("IsDisbursementInvoiceCheckBox.BindTo", "IsSelfBillingInvoice", form.InvoiceDetails.IsDisbursementInvoiceCheckBox.BindTo);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestIsDisbursementInvoiceFlagIsNotResetWhenAmendingAnInvoice()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				invoice.IsDisbursementOrFinal = true;

				using (BaseInvoicingForm form = GetFormByInvoice(invoice))
				{
					form.Show();
					Application.DoEvents();
					AssertEquals("Disbursement flag shouldn't be reset", true, invoice.IsDisbursementOrFinal);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestTaxBranchVisibility_OldTestWrittenBeforeUsingViewModel()
		{
			// This test has been retained to ensure that original functionality is not affected after view model is used to make column visibility decision.
			// Do not re-use this test's method to write any test in the future.
			var invoice = GetInvoiceWithValidTestData();

			AssertTaxBranchVisibility("EnableTaxBranchReporting: true, isGSTRegistered: true", true, true);
			AssertTaxBranchVisibility("EnableTaxBranchReporting: true, isGSTRegistered: false", true, false);
			AssertTaxBranchVisibility("EnableTaxBranchReporting: false, isGSTRegistered: true", false, true);
			AssertTaxBranchVisibility("EnableTaxBranchReporting: false, isGSTRegistered: false", false, false);

			void AssertTaxBranchVisibility(string message, bool isEnableRegistry, bool isGSTRegistered)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableRegistry))
				using (var form = GetFormByInvoice(invoice))
				{
					form.Show();
					Application.DoEvents();

					var linesGrid = form.InvoiceDetails.TransactionLinesGrid;
					var taxBranchFindBox = form.InvoiceDetails.AH_GB_TaxBranchFindBox;
					var expectedVisible = isEnableRegistry && isGSTRegistered;

					AssertEquals(message, expectedVisible, linesGrid.Columns.Contains(InvoicingLineBase.Schema.AL_GB_TaxBranch));
					AssertEquals(message, expectedVisible, linesGrid.Columns.Contains(InvoicingLineBase.Schema.TaxBranchName));
					AssertEquals(message, expectedVisible, taxBranchFindBox.Visible);
				}
			}
		}

		public void TestTaxBranchVisibility()
		{
			var invoice = GetInvoiceWithValidTestData();
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			var invoiceFormPresentationProviderMock = new Mock<IInvoiceFormPresentationProvider>();
			var accountingPresentationProviderFactoryMock = new Mock<IAccountingPresentationProviderFactory>();

			ObjectFactory.Substitute(accountingPresentationProviderFactoryMock.Object);

			AssertTaxBranchColumnVisibility("When IsTaxBranchColumnVisible() returns false", false);
			AssertTaxBranchColumnVisibility("When IsTaxBranchColumnVisible() returns true", true);

			void AssertTaxBranchColumnVisibility(string message, bool isColumnVisible)
			{
				accountingPresentationProviderFactoryMock.Setup(x => x.GetInvoiceFormPresentationProvider()).Returns(invoiceFormPresentationProviderMock.Object);
				invoiceFormPresentationProviderMock.Setup(x => x.IsTaxBranchColumnVisible()).Returns(isColumnVisible);

				using (var form = GetFormByInvoice(invoice))
				{
					form.Show();
					Application.DoEvents();

					var linesGrid = form.InvoiceDetails.TransactionLinesGrid;

					invoiceFormPresentationProviderMock.Verify(x => x.IsTaxBranchColumnVisible(), Times.Exactly(2));
					var columnInfos = linesGrid.ColumnStyles.Cast<ZGridColumnInfo>();
					AssertEquals(message, isColumnVisible, columnInfos.Any(x => x.ColumnName == InvoicingLineBase.Schema.AL_GB_TaxBranch));
					AssertEquals(message, isColumnVisible, columnInfos.Any(x => x.ColumnName == InvoicingLineBase.Schema.TaxBranchName));

					invoiceFormPresentationProviderMock.Invocations.Clear();
				}
			}
		}

		public virtual void TestOnLoad_FinalFlagVisibility()
		{
			Assert(true);
		}

		public void TestDoNoReOpenClosedJobForIncompleteTransactions()
		{
			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
			Job job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			InvoicingBase invoice = GetInvoiceWithValidTestData();

			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice.AH_TransactionType == TransactionTypes.Invoice)
			{
				invoice.SaveAsIncomplete();

				AssertEquals("Incomplete", LedgerTypes.IncompleteTransactions, invoice.AH_Ledger);

				using (BaseInvoicingForm form = GetFormByInvoice(invoice))
				{
					form.Show();
					Assert(form.IsINTransaction_ForTestOnly);

					InvoicingLineBase invoicingLine = (InvoicingLineBase)invoice.Lines.AddNew();

					invoicingLine.AL_JH = job.PK;
					invoicingLine.Job.JH_Status = JobHeaderStatus.Closed.Code;
					job.JH_Status = JobHeaderStatus.Closed.Code;

					Env.Security.ReopenJob.IsAllowed = true;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.DisplayMode = ODisplayMode.Delete;
					form.FireSaveButton();

					AssertEquals(JobHeaderStatus.Closed.Code, job.JH_Status);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestInitSecurityOverrideProvider()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				AssertEquals(ExpectedSecurityProviderType, SecurityOverrideProviderSource.Get(form.Invoice_ForTestOnly).Provider.GetType());
			}
		}

		protected virtual Type ExpectedSecurityProviderType
		{
			get { return typeof(InvoicingSecurityOverrideProvider); }
		}

		public void TestReOpenClosedJobGranted()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();

				InvoicingBase invoice = form.Invoice_ForTestOnly;

				if (invoice.AH_TransactionType == TransactionTypes.Invoice || invoice.AH_TransactionType == TransactionTypes.CreditNote
					|| invoice.AH_TransactionType == TransactionTypes.UAInvoice || invoice.AH_TransactionType == TransactionTypes.UACreditNote)
				{
					var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
					invoiceLine.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(invoiceLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

					Env.Security.ReopenJob.IsAllowed = true;
					form.ReOpenClosedJob_ForTestOnly();

					AssertEquals(JobHeaderStatus.Working.Code, invoiceLine.Job.JH_Status);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestReOpenClosedJobDenied()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();

				form.Invoice_ForTestOnly.AH_TransactionNum = "S0000001";
				InvoicingLineBase invoicingLine = (InvoicingLineBase)form.Invoice_ForTestOnly.Lines.AddNew();
				invoicingLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				Job job = TestObjectCreator.CreateJob(shipment, false);
				Factory.Save();

				invoicingLine.AL_JH = job.PK;
				invoicingLine.Job.JH_Status = JobHeaderStatus.Closed.Code;

				Env.Security.ReopenJob.IsAllowed = false;
				form.ReOpenClosedJob_ForTestOnly();

				AssertEquals(JobHeaderStatus.Closed.Code, invoicingLine.Job.JH_Status);
			}
		}

		public void TestSecurityOverrideReOpenClosed()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001234");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_Status = JobHeaderStatus.Closed.Code;
			Factory.Save();

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();

				NonInteractiveSecurityOverrideProvider testProvider = new NonInteractiveSecurityOverrideProvider();
				testProvider.OverrideLogin = "newuser";
				testProvider.OverridePassword = "password";
				SecurityOverrideProviderSource.Get(form.Invoice_ForTestOnly).Provider = testProvider;

				InvoicingBase invoice = form.Invoice_ForTestOnly;
				if (invoice.AH_TransactionType == TransactionTypes.Invoice || invoice.AH_TransactionType == TransactionTypes.CreditNote
				|| invoice.AH_TransactionType == TransactionTypes.UAInvoice || invoice.AH_TransactionType == TransactionTypes.UACreditNote)
				{
					var invoicingLine = (InvoicingLineBase)invoice.Lines.AddNew();
					invoicingLine.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(invoicingLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

					Env.Security.ReopenJob.IsAllowed = false;
					form.ReOpenClosedJob_ForTestOnly();
					AssertEquals(JobHeaderStatus.Closed.Code, invoicingLine.Job.JH_Status);

					SecurityTestObject.CreateTestUser(true, Env.Security.JobCosting.Code, "tst", "newuser", "password");

					form.ReOpenClosedJob_ForTestOnly();
					AssertEquals(JobHeaderStatus.Working.Code, invoicingLine.Job.JH_Status);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestSecurityOverrideReOpenClosedForm()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Invoice_ForTestOnly.AH_TransactionNum = "S0000001";
				form.Show();

				InteractiveSecurityOverrideProvider testProvider = new InteractiveSecurityOverrideProvider();
				SecurityOverrideProviderSource.Get(form.Invoice_ForTestOnly).Provider = testProvider;

				InvoicingBase invoice = form.Invoice_ForTestOnly;
				if (invoice.AH_TransactionType == TransactionTypes.Invoice || invoice.AH_TransactionType == TransactionTypes.CreditNote
				|| invoice.AH_TransactionType == TransactionTypes.UAInvoice || invoice.AH_TransactionType == TransactionTypes.UACreditNote)
				{
					InvoicingLineBase invoicingLine = (InvoicingLineBase)invoice.Lines.AddNew();
					ForwardingShipment shipment = TestObjectCreator.CreateShipment("S00001234");
					Job job = TestObjectCreator.CreateJob(shipment, false);

					invoicingLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					invoicingLine.AL_JH = job.PK;
					TestObjectCreator.CreateJobCharge(invoicingLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
					job.JH_Status = JobHeaderStatus.Closed.Code;

					Env.Security.ReopenJob.IsAllowed = false;
					ZFormModaliser.LastFormShownDialogForTest = null;
					form.ReOpenClosedJob_ForTestOnly();
					AssertEquals("Should prompt login form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					job.Factory.Save();

					ZFormModaliser.LastFormShownDialogForTest = null;
					invoicingLine.Job.JH_Status = JobHeaderStatus.Closed.Code;
					invoice.Factory.SetContext(BusinessContext.AllowReopenJobWhenImporting);
					form.ReOpenClosedJob_ForTestOnly();
					AssertNull("Should NOT prompt login form", ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Job should be reopened", JobHeaderStatus.Working.Code, job.JH_Status);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestSecurityOverrideInvoicingLevels()
		{
			using (BaseInvoicingForm form = GetFormByInvoice(GetInvoiceWithValidTestData(false)))
			{
				form.Show();

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				bool isLevelsApplicable = form.Invoice_ForTestOnly.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable &&
					(form.Invoice_ForTestOnly.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote ||
					form.Invoice_ForTestOnly.AH_TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote);
				bool isApplicableAdjustmentNote = isLevelsApplicable && form.Invoice_ForTestOnly.AH_TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote;

				form.Invoice_ForTestOnly.AH_LocalExTaxAmount = 100;
				if (isApplicableAdjustmentNote)
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());

					form.Invoice_ForTestOnly.AH_LocalExTaxAmount = -100;
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				}
				else
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				}

				form.Invoice_ForTestOnly.AH_LocalExTaxAmount = 300;
				if (isApplicableAdjustmentNote)
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());

					form.Invoice_ForTestOnly.AH_LocalExTaxAmount = -300;
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				}
				else
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				}

				NonInteractiveSecurityOverrideProvider testProvider = new NonInteractiveSecurityOverrideProvider();
				testProvider.OverrideLogin = "newuser";
				testProvider.OverridePassword = "password";
				SecurityOverrideProviderSource.Get(form.Invoice_ForTestOnly).Provider = testProvider;

				var setting = new AuthorizationModeAndSettings();
				var valuesForTest = setting.AuthorisationSettings;
				var newSetting = valuesForTest.AddNew();
				newSetting.Amount = 200;
				newSetting.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
				newSetting.Range = RangeCodes.UpTo;

				newSetting = valuesForTest.AddNew();
				newSetting.Amount = 200;
				newSetting.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
				newSetting.Range = RangeCodes.Above;

				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), setting);

				form.Invoice_ForTestOnly.AH_LocalExTaxAmount = 100;
				if (isApplicableAdjustmentNote)
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());

					form.Invoice_ForTestOnly.AH_LocalExTaxAmount = -100;
					AssertEquals(ContinueWithSave.No, form.ShowPreSaveDialogs_ForTestOnly());
				}
				else
				{
					AssertEquals(isLevelsApplicable ? ContinueWithSave.No : ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				}

				form.Invoice_ForTestOnly.AH_LocalExTaxAmount = 300;
				if (isApplicableAdjustmentNote)
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());

					form.Invoice_ForTestOnly.AH_LocalExTaxAmount = -300;
					AssertEquals(ContinueWithSave.No, form.ShowPreSaveDialogs_ForTestOnly());
				}
				else
				{
					AssertEquals(isLevelsApplicable ? ContinueWithSave.No : ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				}

				SecurityTestObject.CreateTestUser(true, Env.Security.ReceivablesCreditAdjustmentNotePostingApproval.Code, "tst", "newuser", "password");

				form.Invoice_ForTestOnly.AH_LocalExTaxAmount = 100;
				if (isApplicableAdjustmentNote)
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());

					form.Invoice_ForTestOnly.AH_LocalExTaxAmount = -100;
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				}
				else
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				}

				form.Invoice_ForTestOnly.AH_LocalExTaxAmount = 300;
				if (isApplicableAdjustmentNote)
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());

					form.Invoice_ForTestOnly.AH_LocalExTaxAmount = -300;
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				}
				else
				{
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
				}
			}
		}

		public void TestSecurityOverrideInvoicingLevelsForm()
		{
			using (BaseInvoicingForm form = GetFormByInvoice(GetInvoiceWithValidTestData(false)))
			{
				form.Show();

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				var setting = new AuthorizationModeAndSettings();
				var valuesForTest = setting.AuthorisationSettings;
				var newSetting = valuesForTest.AddNew();
				newSetting.Amount = 200;
				newSetting.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
				newSetting.Range = RangeCodes.UpTo;

				newSetting = valuesForTest.AddNew();
				newSetting.Amount = 200;
				newSetting.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
				newSetting.Range = RangeCodes.Above;

				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), setting);

				bool isLevelsApplicable = form.Invoice_ForTestOnly.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable &&
					(form.Invoice_ForTestOnly.AH_TransactionType == ZArchitecture.Core.TransactionTypes.CreditNote ||
					form.Invoice_ForTestOnly.AH_TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote);

				form.Invoice_ForTestOnly.AH_LocalExTaxAmount = 100;
				ZFormModaliser.LastFormShownDialogForTest = null;
				form.ShowPreSaveDialogs_ForTestOnly();
				if (isLevelsApplicable)
				{
					if (form.Invoice_ForTestOnly.AH_TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote)
					{
						AssertNull(ZFormModaliser.LastFormShownDialogForTest);

						form.Invoice_ForTestOnly.AH_LocalExTaxAmount = -100;
						form.ShowPreSaveDialogs_ForTestOnly();
						AssertEquals("Should prompt login form", typeof(LoginFormForARCreditNoteApprovalOverride), ZFormModaliser.LastFormShownDialogForTest.GetType());
					}
					else
					{
						AssertEquals("Should prompt login form", typeof(LoginFormForARCreditNoteApprovalOverride), ZFormModaliser.LastFormShownDialogForTest.GetType());
					}
				}
				else
				{
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				}

				form.Invoice_ForTestOnly.AH_LocalExTaxAmount = 300;
				ZFormModaliser.LastFormShownDialogForTest = null;
				form.ShowPreSaveDialogs_ForTestOnly();
				if (isLevelsApplicable)
				{
					if (form.Invoice_ForTestOnly.AH_TransactionType == ZArchitecture.Core.TransactionTypes.AdjustmentNote)
					{
						AssertNull(ZFormModaliser.LastFormShownDialogForTest);

						form.Invoice_ForTestOnly.AH_LocalExTaxAmount = -300;
						form.ShowPreSaveDialogs_ForTestOnly();
						AssertEquals("Should prompt login form", typeof(LoginFormForARCreditNoteApprovalOverride), ZFormModaliser.LastFormShownDialogForTest.GetType());
					}
					else
					{
						AssertEquals("Should prompt login form", typeof(LoginFormForARCreditNoteApprovalOverride), ZFormModaliser.LastFormShownDialogForTest.GetType());
					}
				}
				else
				{
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		public void TestSecurityOverride_CostVariance_NewAPInvoice()
		{
			TestSecurityOverride_CostVariance(true);
		}

		public void TestSecurityOverride_CostVariance_ExistingAPInvoice()
		{
			TestSecurityOverride_CostVariance(false);
		}

		void TestSecurityOverride_CostVariance(bool newAPInvoice)
		{
			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo = valuesForTest.AuthorisationRequirements.AddNew();
			upTo.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			upTo.Range = RangeCodes.UpTo;
			upTo.Amount = 100M;
			CostVarianceApprovalAuthorisationRequirement above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			above.Range = RangeCodes.Above;
			above.Amount = upTo.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;
			Env.Security.CostVarianceApprovalLevel2.IsAllowed = false;

			GlbBranch sYD = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, "SYD");
			GlbDepartment fEA = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "FEA");
			AccChargeCode cAF = TestObjectCreator.CC1;

			AssertNotNull(sYD);
			AssertNotNull(fEA);
			AssertNotNull(cAF);

			Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001001"), false);
			Charge charge = TestObjectCreator.CreateCharge(job, cAF, "",
				TestObjectCreator.AUD, 100M, TestObjectCreator.Creditor1,
				TestObjectCreator.AUD, 0M, null);
			charge.JR_GB = sYD.PK;
			charge.JR_GE = fEA.PK;

			if (newAPInvoice)
			{
				Factory.Save();
			}

			APInvoice invoice = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			line.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			line.AL_AC = cAF.PK;
			line.AL_GB = sYD.PK;
			line.AL_GE = fEA.PK;
			line.AL_RX_NKTransactionCurrency = "AUD";
			line.AL_ExchangeRate = 1M;
			if (!newAPInvoice)
			{
				line.AL_OSExTaxAmount = 100M;
				charge.JR_AL_APLine = line.PK;

				Factory.Save();
			}

			using (BaseInvoicingForm form = new InvoiceForm(invoice))
			{
				form.Show();

				line.AL_OSExTaxAmount = 250M;
				ZFormModaliser.LastFormShownDialogForTest = null;
				form.ShowPreSaveDialogs_ForTestOnly();
				if (newAPInvoice)
				{
					AssertEquals("Should prompt login form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				}
				else
				{
					AssertNull("Should not prompt login form", ZFormModaliser.LastFormShownDialogForTest);
				}

				line.AL_OSExTaxAmount = 150M;
				ZFormModaliser.LastFormShownDialogForTest = null;
				form.ShowPreSaveDialogs_ForTestOnly();
				AssertNull("Should not prompt login form", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public virtual void TestApportionChargesButtonEnabled()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				Assert("Should not apportion button enabled by default", !form.InvoiceDetails.ApportionChargesButton.Visible);
			}
		}

		public void TestJobChargesPopupFormIsDisposed()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			using (BaseInvoicingForm form = new BaseInvoicingForm(invoice))
			{
				ARInvoiceLine invoicingLine = (ARInvoiceLine)invoice.Lines.AddNew();
				invoicingLine.AL_AC = TestObjectCreator.CC1.PK;
				invoicingLine.AL_JH = Factory.NewJobForTesting<Job>().PK;
				Charge charge = Factory.New<Charge>();
				charge.JR_AC = invoicingLine.AL_AC;
				charge.JR_JH = invoicingLine.AL_JH;
				charge.JR_LocalCostAmt = 100M;

				form.InvoiceForm_ShowJobChargesForImportEvent_ForTestOnly(invoicingLine, EventArgs.Empty);
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertType(typeof(JobChargesPopupForm), ZFormModaliser.LastFormShownDialogForTest);
				Assert("JobChargesPopupForm should be disposed", ZFormModaliser.LastFormShownDialogForTest.IsDisposed);
			}
		}

		public void TestGSTInclusiveAmountColumnExists()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Invoice_ForTestOnly.AH_TransactionNum = "S0000001";
				form.Show();
				bool doesGSTInclusiveAmountColumnExist = false;
				foreach (ZGridColumnInfo columnStyle in form.InvoiceDetails.TransactionLinesGrid.ColumnStyles)
				{
					if (columnStyle.ColumnName == "GSTInclusiveAmount")
					{
						doesGSTInclusiveAmountColumnExist = true;
						break;
					}
				}
				var isVisible = form.Invoice_ForTestOnly != null && (form.Invoice_ForTestOnly is APInvoice || form.Invoice_ForTestOnly is APCreditNote) && form.Invoice_ForTestOnly.AH_Ledger != LedgerTypes.IncompleteTransactions;
				AssertEquals("IsVisible", isVisible, doesGSTInclusiveAmountColumnExist);
			}

			InvoicingBase invoice = GetInvoiceWithValidTestData();
			Factory.Save();
			using (BaseInvoicingForm form = GetFormByInvoice(invoice))
			{
				form.Show();
				bool doesGSTInclusiveAmountColumnExist = false;
				foreach (ZGridColumnInfo columnStyle in form.InvoiceDetails.TransactionLinesGrid.ColumnStyles)
				{
					if (columnStyle.ColumnName == "GSTInclusiveAmount")
					{
						doesGSTInclusiveAmountColumnExist = true;
						break;
					}
				}
				AssertEquals("IsVisible", false, doesGSTInclusiveAmountColumnExist);
			}
		}

		public void TestControlsHidingForCanada()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Invoice_ForTestOnly.AH_TransactionNum = "S0000001";
				AssertEquals("Company should not be in Canada ", false, GlbCompany.CurrentCompany.Country == Core.Constants.CountryCodes.Canada);
				form.Show();
				Application.DoEvents();
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = "USD";
				AssertEquals(false, form.GetControl<ZPanel>("ExtraTaxPanel", true).Visible);
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSExtraTaxAmount).IsUnavailable);
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalExtraTaxAmount).IsUnavailable);
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSGSTAmount).IsUnavailable);
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalGSTAmount).IsUnavailable);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			Factory.Save();
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Company should be in Canada ", true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = "AUD";
				Application.DoEvents();
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = "USD";
				AssertEquals(true, form.GetControl<ZPanel>("ExtraTaxPanel", true).Visible);
				AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSExtraTaxAmount).IsUnavailable);
				AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalExtraTaxAmount).IsUnavailable);
				AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSGSTAmount).IsUnavailable);
				AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalGSTAmount).IsUnavailable);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = "AUD";
				Application.DoEvents();
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = "USD";
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSExtraTaxAmount).IsUnavailable);
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalExtraTaxAmount).IsUnavailable);
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSGSTAmount));
				AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalGSTAmount));
			}
		}

		public void TestControlsHidingForIndia() =>
			AssertControlsHidingForCountry("India", Core.Constants.CountryCodes.India, "INR", "USD", "SGST Amount", "SGST Amt in Local Currency");

		public void TestControlsHidingForMexico() =>
			AssertControlsHidingForCountry("Mexico", Core.Constants.CountryCodes.Mexico, "MXN", "USD", "RET Amount", "RET Amt in Local Currency");

		public void TestControlsHidingForTurkey() =>
			AssertControlsHidingForCountry("Turkey", Core.Constants.CountryCodes.Turkey, "TRY", "USD", "VAT Withholding Amount", "VAT Withholding Amt in Local Currency");

		public void TestControlsHidingForGhana() =>
			AssertControlsHidingForCountry("Ghana", Core.Constants.CountryCodes.Ghana, "GHS", "USD", "NHIL/GETFL Amt", "NHIL/GETFL Amt in Local Currency");

		void AssertControlsHidingForCountry(string countryName, string countryCode, string nationalCurrencyCode, string foreignCurrencyCode, string expectedExtraTaxAmountCalcEditCaption, string expectedLocalExtraTaxAmountCalcEditCaption)
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				AssertNotEquals(string.Format("Not {0} company", countryName), countryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				form.Show();
				Application.DoEvents();
				form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = nationalCurrencyCode;
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSExtraTaxAmount).IsUnavailable);
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalExtraTaxAmount).IsUnavailable);
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSGSTAmount).IsUnavailable);
				AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalGSTAmount).IsUnavailable);
			}

			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(countryCode);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, countryCode);
			try
			{
				using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
				{
					form.Show();
					Application.DoEvents();
					form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = nationalCurrencyCode;
					Application.DoEvents();
					form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = foreignCurrencyCode;
					AssertEquals(true, form.GetControl<ZPanel>("ExtraTaxPanel", true).Visible);
					AssertEquals(TransactionHeaderWithLines.Schema.AH_LocalExtraTaxAmount, form.AH_LocalExtraTaxAmountCalcEdit.BindToAmount);
					AssertEquals(TransactionHeaderWithLines.Schema.AH_OSExtraTaxAmount, form.AH_OSExtraTaxAmountCalcEdit_ForTestOnly.BindToAmount);
					AssertEquals(expectedExtraTaxAmountCalcEditCaption, form.AH_OSExtraTaxAmountCalcEdit_ForTestOnly.GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals(expectedLocalExtraTaxAmountCalcEditCaption, form.AH_LocalExtraTaxAmountCalcEdit.GetExtension<ILabelCaptionRenderer>().Caption);

					AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSExtraTaxAmount).IsUnavailable);
					AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalExtraTaxAmount).IsUnavailable);
					AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSGSTAmount).IsUnavailable);
					AssertEquals(false, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalGSTAmount).IsUnavailable);
				}

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
				{
					form.Show();
					Application.DoEvents();
					form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = nationalCurrencyCode;
					Application.DoEvents();
					form.Invoice_ForTestOnly.AH_RX_NKTransactionCurrency = foreignCurrencyCode;

					AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSExtraTaxAmount).IsUnavailable);
					AssertEquals(true, form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalExtraTaxAmount).IsUnavailable);
					AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_OSGSTAmount));
					AssertNull(form.InvoiceDetails.TransactionLinesGrid.GetColumnStyle(InvoiceLine.Schema.AL_LocalGSTAmount));
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestRecognizeRevenueBehaviourForDateBeforeFirstPeriod()
		{
			string expectedMessage = "cannot be set because an appropriate General Ledger Accounting Period has not been created to include this date. A General Ledger Accounting Period cannot be created because it is earlier than the first period currently existing.";

			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			RevenueRecognitionCollection valuesForTest = new RevenueRecognitionCollection();
			RevenueRecognition setting = valuesForTest.AddNew();
			setting.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PickupDate;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				if (form.Invoice_ForTestOnly is APInvoice || form.Invoice_ForTestOnly is APCreditNote)
				{
					InvoicingLineBase line = form.Invoice_ForTestOnly.Lines.AddNew() as InvoicingLineBase;
					line.AL_AC = TestObjectCreator.CC1.PK;

					shipment.DocsAndCartage.JP_EstimatedPickup = ZDateTime.BrettsBirthday;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					line.AL_JH = job.PK;
					AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertHasErrors(line.AL_JHInfo);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					line.AL_JH = ZGuid.Empty;
					line.AL_JH = job.PK;
					AssertNull("System uses previous response and don't show message.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertHasErrors(line.AL_JHInfo);

					job.ResetPreviouseRespose_ShouldUseImmediateRevenueRecognisedDate();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					line.AL_JH = ZGuid.Empty;
					line.AL_JH = job.PK;
					AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNoErrors(line.AL_JHInfo);
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestButtonsAndMenuItemVisibilityUpdatedAfterInvoicePosted()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Invoice_ForTestOnly.AH_TransactionNum = "S0000001";
				form.Show();
				Application.DoEvents();

				var invoice = form.Invoice_ForTestOnly;
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				invoice.AH_FullyPaidDate = ZDateTime.Empty;
				InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.FillWithValidTestData();
				var chargeList = line.ChargeList;
				chargeList.Load();
				line.GenericCharge = chargeList[0].PK;
				line.AL_OSExTaxAmount = 10m;
				new AccountingPeriodTestHelper(Factory).SetupPeriods();
				form.ValidateAndSave_ForTestOnly();

				invoice = form.Invoice_ForTestOnly;
				Assert("Prerequisite: invoice is saved", invoice.IsInDatabase);
				var details = form.InvoiceDetails;
				AssertEquals("ApportionChargesButton visibility", form.ShouldEnableApportionChargesButton_ForTestOnly, details.ApportionChargesButton.Visible);
				AssertEquals("BulkChargeImportButton visibility", form.ShouldEnableBulkChargeImportButton_ForTestOnly, details.BulkChargeImportButton.Visible);
				if (invoice is APInvoice || invoice is APCreditNote)
				{
					AssertNotNull("Edit Apportionment menu item must exist.", details.TransactionLinesGrid.ContextMenu.MenuItems.FindByText(form.EditApportionmentMenuItemName_ForTestOnly));
				}
			}
		}

		public void TestNoExceptionThrownWhenTransactionReferenceNotEndWithDigital()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				var invoice = GetInvoiceWithValidTestData();
				invoice.AH_ComplianceSubType = "tst";
				invoice.AH_TransactionReference = "010T";

				using (var form = GetFormByInvoice(invoice))
				{
					form.Show();
					Application.DoEvents();
					AssertNullOrEmpty(form.InvoiceDetails.ComplianceSequenceTextBox.Text);
				}
			}
		}

		public void TestComplianceSequenceTextValueAfterInvoicePosted()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var sequenceRec = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 1, 100, 25);
				sequenceRec.XD_Code = "ARI_21";
				sequenceRec.XD_Description = "ARI TEST";

				var sequencePay = TestObjectCreator.CreateNewComplianceSequence(ZGuid.Empty, ItalyComplianceInfo.ComplianceSubTypeCodes.API, 1, 100, 25);
				sequencePay.XD_Code = "API_21";
				sequencePay.XD_Description = "API TEST";

				Factory.Save();

				using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
				{
					form.Invoice_ForTestOnly.AH_TransactionNum = "S0000001";
					form.Show();
					Application.DoEvents();

					var invoice = form.Invoice_ForTestOnly;
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					string subType = null;
					switch (invoice.AH_Ledger)
					{
						case LedgerTypes.AccountsReceivable:
							subType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
							break;
						case LedgerTypes.AccountsPayable:
							subType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
							break;
					}
					invoice.AH_ComplianceSubType = subType;

					InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
					line.FillWithValidTestData();
					var chargeList = line.ChargeList;
					chargeList.Load();
					line.GenericCharge = chargeList[0].PK;
					line.AL_OSExTaxAmount = 10m;
					new AccountingPeriodTestHelper(Factory).SetupPeriods();
					form.ValidateAndSave_ForTestOnly();

					var complSeq = form.Invoice_ForTestOnly.ComplianceSequence;
					var complSeqText = form.InvoiceDetails.ComplianceSequenceTextBox.Text;
					if (subType != null)
					{
						AssertNotNull(complSeq);
						AssertEquals($"{subType}_21 - {subType} TEST", complSeqText);
					}
					else
					{
						AssertNull(complSeq);
						AssertNullOrEmpty(complSeqText);
					}
				}
			}
		}

		public void TestShowErrorHandlerIsDetachedAfterFormIsClosed()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				InvoicingBase invoice = (InvoicingBase)form.BusinessEntity;
				AssertNotNull("ShowError handler should be attached", invoice.ShowError);
				form.Show();
				form.CancelButton.PerformClick();
				AssertNull("ShowError handler should be detached", invoice.ShowError);
			}
		}

		public void TestSetTextUpdatesAccountLabel()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
			{
				testForm.Show();

				if (testInvoice.AH_Ledger == LedgerTypes.AccountsPayable ||
					testInvoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions ||
					testInvoice.AH_Ledger == LedgerTypes.IncompleteTransactions)
				{
					AssertAccountLabelText(testForm, "Creditor Information");
				}
				else
				{
					AssertAccountLabelText(testForm, "Debtor Information");
				}
			}
		}

		public void TestAL_SequenceColumnVisibilityAndSorting()
		{
			InvoicingBase testInvoice = GetInvoiceWithValidTestData();

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
			{
				testForm.Show();
				Application.DoEvents();

				bool containsSequence = (from ZGridColumnInfo column in testForm.InvoiceDetails.TransactionLinesGrid.ColumnStyles where column.ColumnName == InvoiceLine.Schema.AL_Sequence select column).Any();
				Assert("Should always contain Line Sequence Column", containsSequence);

				Assert("Line Sorting should be allowed", testForm.InvoiceDetails.TransactionLinesGrid.AllowSorting);
			}

			Factory.Save();

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
			{
				testForm.Show();
				Application.DoEvents();

				bool containsSequence = (from ZGridColumnInfo column in testForm.InvoiceDetails.TransactionLinesGrid.ColumnStyles where column.ColumnName == InvoiceLine.Schema.AL_Sequence select column).Any();
				Assert("Should always contain Line Sequence Column", containsSequence);

				bool expectSortingAllowed = testInvoice.AH_Ledger == LedgerTypes.IncompleteTransactions || testInvoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions;
				AssertEquals("Line Sorting", expectSortingAllowed, testForm.InvoiceDetails.TransactionLinesGrid.AllowSorting);
			}
		}

		public void TestAutoAllocateDiscrepencyValidatesInvoiceBeforeRunning()
		{
			InvoicingBase testInvoice = Factory.New<APInvoice>();
			OrgHeader orgHeader = TestObjectCreator.AALSHI;
			orgHeader.CompanyData.SetAPTaxApplicable(true);
			testInvoice.AH_OH = orgHeader.PK;

			testInvoice.RunPreSaveValidation();
			AssertEquals("Should have errors", true, testInvoice.HasErrors());

			using (BaseInvoicingForm testForm = new BaseInvoicingForm(testInvoice))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.AutoAllocateDiscrepancyMenuItem.PerformClick();
				AssertEquals("Validation Error Message Should Be Shown", "There are errors - can't auto-allocate.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.CreateInvoiceLine(testInvoice, TestObjectCreator.GLHeader1.PK, 100);
				testInvoice.AH_TransactionNum = "0000000010";
				testInvoice.RunPreSaveValidation();
				AssertEquals("Should not have errors", false, testInvoice.HasErrors());

				testInvoice.ValidateExpectedInvoiceTotal = true;
				testInvoice.ExpectedInvoiceTotal = 1000m;
				testInvoice.ExpectedInvoiceTaxTotal = 200m;
				testInvoice.ExpectedInvoiceExclTaxTotal = 800m;
				var oSCurrencyDecimals = testInvoice.OSCurrencyDecimals;
				Assert("Precondition: Should have error on Expected Invoice Total", testInvoice.ExpectedInvoiceTotalInfo.HasError(string.Format(@"The invoice total of {0} {1} does not equal to the expected amount of {2} {1}.
The difference is {3} {1}.", testInvoice.AH_OSTotalAmount.ToString(oSCurrencyDecimals), testInvoice.AH_RX_NKTransactionCurrency, testInvoice.ExpectedInvoiceTotal.ToString(oSCurrencyDecimals), testInvoice.UnallocatedInvoiceTotal.ToString(oSCurrencyDecimals))));

				testForm.AutoAllocateDiscrepancyMenuItem.PerformClick();
				AssertEquals("Expected Message Should Be Shown", "Proceed to auto-allocate the discrepancy values proportionately based on entered/imported amounts?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		#region Test show error on button click

		[DisableZeroExchangeRateOverriding]
		public void TestShowErrorOnBulkChargeImportButtonClick()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				AssertShowChargeOrConsolCostImportErrorOnButtonClick(form, form.InvoiceDetails.BulkChargeImportButton);
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestShowErrorOnApportionToConsolsButtonClick()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				AssertShowChargeOrConsolCostImportErrorOnButtonClick(form, form.InvoiceDetails.ApportionChargesButton);
			}
		}

		void AssertShowChargeOrConsolCostImportErrorOnButtonClick(BaseInvoicingForm testForm, ZButton testButton)
		{
			InvoicingBase invoice = (InvoicingBase)testForm.BusinessEntity;
			if (invoice is APInvoice || invoice is APCreditNote)
			{
				ExchangeRateReader.GetReaderInstance().ClearCache();

				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				invoice.AH_ExchangeRate = 2M;

				testForm.Show();

				Assert("Precondition: ", !invoice.AH_OH.IsEmpty);
				Assert("Precondition: ", !invoice.AH_RX_NKTransactionCurrency.IsEmpty);
				Assert("Precondition: ", !invoice.AH_ExchangeRate.IsEmpty);
				Assert("Precondition: ", !invoice.AH_PostedToEFT);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testButton.PerformClick();
				AssertNull("Error must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoice.AH_ExchangeRate = 0M;
				Assert("AH_ExchangeRate should be empty", invoice.AH_ExchangeRate.IsEmpty);
				testButton.PerformClick();
				string expectedErrorMessage = @"To access this function, you must fill all of the following fields:

- Creditor
- Currency
- Exchange Rate";
				AssertEquals("Error must be shown.", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_OSExTaxAmount = 1m;
				invoice.AH_PostedToEFT = true;
				testButton.PerformClick();
				expectedErrorMessage = @"When ""Use Job Exchange Rate"" is ticked the Bulk Charge Import screen can only be accessed when 
- The invoice Creditor and Currency have been entered; and
- The invoice Currency has a current Buy exchange rate.
Please ensure a Creditor and Transaction Currency have been entered.
Please ensure that the Transaction Currency has a current BUY exchange rate recorded against it.";
				AssertEquals("Error must be shown.", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoice.AH_ExchangeRate = 3M;
				testButton.PerformClick();
				AssertNull("Error must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoice.AH_ExchangeRate = 0M;
				Assert("AH_ExchangeRate should be empty", invoice.AH_ExchangeRate.IsEmpty);
				testButton.PerformClick();
				AssertEquals("Error must be shown.", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				RefExchangeRate buyExRate = new BusinessObjectFactory().NewWithValidTestData<RefExchangeRate>();
				buyExRate.RE_GC = GlbCompany.CurrentCompany.PK;
				buyExRate.RE_StartDate = ZDateTime.Today;
				buyExRate.RE_ExpiryDate = ZDateTime.Today;
				buyExRate.RE_SellRate = 2m;
				buyExRate.RE_RX_NKExCurrency = invoice.AH_RX_NKTransactionCurrency;
				buyExRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
				buyExRate.Factory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();

				testButton.PerformClick();
				AssertNull("Error must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoice.AH_OH = ZGuid.Empty;
				testButton.PerformClick();
				AssertEquals("Error must be shown.", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				invoice.AH_PostedToEFT = true;
				testButton.PerformClick();
				AssertNull("Error must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoice.AH_RX_NKTransactionCurrency = ZString.Empty;
				testButton.PerformClick();
				AssertEquals("Error must be shown.", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoice.AH_RX_NKTransactionCurrency = "USD";
				testButton.PerformClick();
				AssertNull("Error must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				Assert(true);
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestShowErrorOnBulkChargeImportButtonClick_EnableTaxBranchReporting()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				AssertShowChargeOrConsolCostImportErrorOnButtonClick_EnableTaxBranchReporting(form, form.InvoiceDetails.BulkChargeImportButton);
			}
		}

		[DisableZeroExchangeRateOverriding]
		public void TestShowErrorOnApportionToConsolsButtonClick_EnableTaxBranchReporting()
		{
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				form.Show();
				AssertShowChargeOrConsolCostImportErrorOnButtonClick_EnableTaxBranchReporting(form, form.InvoiceDetails.ApportionChargesButton);
			}
		}

		void AssertShowChargeOrConsolCostImportErrorOnButtonClick_EnableTaxBranchReporting(BaseInvoicingForm testForm, ZButton testButton)
		{
			var invoice = (InvoicingBase)testForm.BusinessEntity;
			if (invoice is APInvoice || invoice is APCreditNote)
			{
				ExchangeRateReader.GetReaderInstance().ClearCache();

				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_OH = TestObjectCreator.AALSHI.PK;
				invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				invoice.AH_ExchangeRate = 2M;
				invoice.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;

				testForm.Show();

				Assert("Precondition: ", !invoice.AH_OH.IsEmpty);
				Assert("Precondition: ", !invoice.AH_RX_NKTransactionCurrency.IsEmpty);
				Assert("Precondition: ", !invoice.AH_ExchangeRate.IsEmpty);
				Assert("Precondition: ", !invoice.AH_PostedToEFT);
				Assert("Precondition: ", !invoice.AH_GB_TaxBranch.IsEmpty);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testButton.PerformClick();
				AssertNull("Error must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoice.AH_GB_TaxBranch = ZGuid.Empty;
				Assert("AH_GB_TaxBranch should be empty", invoice.AH_GB_TaxBranch.IsEmpty);
				testButton.PerformClick();
				string expectedErrorMessage = @"To access this function, you must fill all of the following fields:

- Creditor
- Currency
- Exchange Rate
- Tax Branch";
				AssertEquals("Error must be shown.", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var line = (InvoicingLineBase)invoice.Lines.AddNew();
				line.AL_OSExTaxAmount = 1m;
				invoice.AH_PostedToEFT = true;
				testButton.PerformClick();
				expectedErrorMessage = @"When ""Use Job Exchange Rate"" is ticked the Bulk Charge Import screen can only be accessed when 
- The invoice Creditor, Tax Branch and Currency have been entered; and
- The invoice Currency has a current Buy exchange rate.
Please ensure a Creditor and Transaction Currency have been entered.
Please ensure that the Transaction Currency has a current BUY exchange rate recorded against it.";
				AssertEquals("Error must be shown.", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				invoice.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
				testButton.PerformClick();
				AssertNull("Error must not be shown.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		public virtual void TestSavingWhenDisplayModeIsDelete()
		{
			InvoicingBase invoice = GetInvoiceWithValidTestData();

			if (invoice is APInvoice || invoice is APCreditNote)
			{
				AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				var shipment = TestObjectCreator.CreateShipment("S00001001");
				Job job = TestObjectCreator.CreateJob(shipment, false);
				InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1.0m, 100m);
				line.AL_AC = TestObjectCreator.CC1.PK;
				line.AL_JH = job.PK;

				Charge charge = TestObjectCreator.CreateCharge(line, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
				charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				invoice.GenerateReverseTransaction(true);
				invoice.SetCancellationFlag(true);
				InvoicingBase reverseInvoice = invoice.ReverseInvoice;
				reverseInvoice.AH_TransactionNum = "001";
				reverseInvoice.SetCancellationFlag(true);
				invoice.GenerateMatchLinks();
				reverseInvoice.GenerateMatchLinks();
				TestObjectCreator.SetupMatchLinkMatchDate(invoice);
				TestObjectCreator.SetupMatchLinkMatchDate(reverseInvoice);

				using (BaseInvoicingForm form = GetFormByInvoice(reverseInvoice))
				{
					form.DisplayMode = ODisplayMode.Delete;
					form.Show();
					Application.DoEvents();
					form.FindSingle<ZPostingButtonsUserControl>().SaveAndCloseButton.PerformClick();
					Assert(string.Format("Error: {0}\r\nHeader Errors: {1}\r\nLine Errors: {2}",
						UnitTestUserNotification.Instance.LastMessage.Text,
						reverseInvoice.Notifications.ToUniqueMessageListString(),
						reverseInvoice.Lines[0].Notifications.ToUniqueMessageListString()),
						UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestConfirmComplianceSubTypeWhenReverse()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var testObjectCreatorinNewFactory = new TestObjectCreator(newFactory);
			testObjectCreatorinNewFactory.CreateTestPeriods(ZDate.Today);

			var shipment = testObjectCreator.CreateShipment("S00001024");
			var job = testObjectCreator.CreateJob(shipment);
			job.JH_JobNum = "S00001024";

			Factory.Save();

			var aPinvoice = testObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m);
			aPinvoice.AH_JH = job.PK;
			aPinvoice.AH_Ledger = LedgerTypes.AccountsPayable;
			aPinvoice.AH_TransactionType = TransactionTypes.Invoice;
			aPinvoice.AH_TransactionReference = "test";
			aPinvoice.AH_ComplianceSubType = "tes";

			var reverseInvoice = testObjectCreatorinNewFactory.CreateARCreditNote("1000", testObjectCreatorinNewFactory.AALSHI, testObjectCreatorinNewFactory.AUD, 1m);
			reverseInvoice.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(reverseInvoice, job);
			reverseInvoice.AH_OH = testObjectCreatorinNewFactory.Debtor1.PK;
			reverseInvoice.AH_OSTotalAmount = 100;
			reverseInvoice.OriginalTransaction = aPinvoice;

			Factory.Save();

			aPinvoice.AH_ConsolidatedInvoiceRef = "S000001024";
			Factory.Save();

			AssertPreDeleteDialog_For_Reversing(reverseInvoice);
		}

		void AssertPreDeleteDialog_For_Reversing(InvoicingBase revInv, string expMsg = null)
		{
			using (var form = new BaseInvoicingForm(revInv))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.FReversingCode_ForTestOnly = "IDE";
				form.FReversingReason_ForTestOnly = "Correct Data Entry";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ShowPreDeleteDialogs_ForTestOnly();
				AssertEquals(expMsg ?? "A compliance sub type and number has been assigned to this transaction. Do you want to reverse the transaction?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		InvoicingBase CreateInvoiceForCompliance(Type invType, string invNum, ZDate allocationDate, string subType, AccComplianceSequence sequence, string complNr = null, string allocationDateOption = null)
		{
			var inv = TestObjectCreator.CreateInvoice(invType, TestObjectCreator.EUR, 1);
			inv.AH_TransactionNum = invNum;

			if (allocationDateOption != null && allocationDateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				inv.AH_InvoiceDate = allocationDate;
			}
			else
			{
				inv.AH_PostDate = allocationDate;
			}

			inv.AH_ComplianceSubType = subType;
			inv.AH_XD_ComplianceBook = sequence.PK;
			inv.AH_TransactionReference = complNr;
			return inv;
		}

		(string, CodePairRegistryItem, CodePairRegistryItem) GetComplianceParams(InvoicingBase invoice)
		{
			switch (invoice.AH_TransactionType)
			{
				case TransactionTypes.Invoice:
				case TransactionTypes.CreditNote:
				case TransactionTypes.AdjustmentNote:
					break;
				default:
					return (null, null, null);
			}

			string subType = null;
			CodePairRegistryItem complianceDocumentNumberAllocationRegistry;
			CodePairRegistryItem complianceNumberAllocationDateRegistry;
			switch (invoice.TypeOfReverseTransaction_ForTestOnly.Name)
			{
				case string x when x.Contains("AP"):
					subType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
					complianceDocumentNumberAllocationRegistry = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Payables;
					complianceNumberAllocationDateRegistry = AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP;
					break;
				case string x when x.Contains("AR"):
					subType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
					complianceDocumentNumberAllocationRegistry = AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables;
					complianceNumberAllocationDateRegistry = AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR;
					break;
				default:
					return (null, null, null);
			}
			return (subType, complianceDocumentNumberAllocationRegistry, complianceNumberAllocationDateRegistry);
		}

		[TestDate(2021, 1, 15)]
		public void TestEnsureNoPastTransactionsWithEmptyComplNum_PST()
		{
			AssertEnsureNoPastTransactionsWithEmptyComplNum(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2021, 1, 15)]
		public void TestEnsureNoPastTransactionsWithEmptyComplNum_INV()
		{
			AssertEnsureNoPastTransactionsWithEmptyComplNum(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		public void AssertEnsureNoPastTransactionsWithEmptyComplNum(string dateOption)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var invoice = GetInvoiceWithValidTestData();
				(var subType, var complianceDocumentNumberAllocationRegistry, var complianceNumberAllocationDateRegistry) = GetComplianceParams(invoice);
				if (subType == null
					|| (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code && complianceNumberAllocationDateRegistry.Name == AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.Name))
				{
					Assert(true);
					return;
				}

				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ITMIL";
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var sequence = TestObjectCreator.SetupComplianceSequence(menuPK, subType, subType + ".21-", 1, 100, 3);
				sequence.XD_StartDate = new ZDate(2021, 1, 1);
				sequence.XD_ExpiryDate = new ZDateTime(2021, 3, 31);
				sequence.XD_IsActive = true;

				Type revInvType = invoice.TypeOfReverseTransaction_ForTestOnly;
				var inv1 = CreateInvoiceForCompliance(revInvType, "INV1", sequence.XD_StartDate, subType, sequence, subType + ".21-0001", allocationDateOption: dateOption);
				var inv2 = CreateInvoiceForCompliance(revInvType, "INV2", new ZDate(2021, 1, 10), subType, sequence, allocationDateOption: dateOption);
				var inv3 = CreateInvoiceForCompliance(revInvType, "INV3", ZDate.Today, subType, sequence, subType + ".21-0002", allocationDateOption: dateOption);

				Factory.Save();

				var reverseInvoice = CreateInvoiceForCompliance(revInvType, "1000", new ZDate(2021, 1, 20), subType, sequence, allocationDateOption: dateOption);
				reverseInvoice.OriginalTransaction = invoice;

				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
				using (complianceNumberAllocationDateRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (complianceDocumentNumberAllocationRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					var allocationDate = dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code ? reverseInvoice.AH_InvoiceDate : reverseInvoice.AH_PostDate;

					AssertPreDeleteDialog_For_Reversing(reverseInvoice, string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToSparseComplianceBookMessage(dateOption),
						subType, allocationDate.ToShortDateString()));
				}
			}
		}

		public void TestCheckIfComplianceBookIsFullOrExpired_PST()
		{
			AssertCheckIfComplianceBookIsFullOrExpired(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestCheckIfComplianceBookIsFullOrExpired_INV()
		{
			AssertCheckIfComplianceBookIsFullOrExpired(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		public void AssertCheckIfComplianceBookIsFullOrExpired(string dateOption)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				var invoice = GetInvoiceWithValidTestData();
				(var subType, var complianceDocumentNumberAllocationRegistry, var complianceNumberAllocationDateRegistry) = GetComplianceParams(invoice);
				if (subType == null
					|| (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code && complianceNumberAllocationDateRegistry.Name == AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.Name))
				{
					Assert(true);
					return;
				}

				var invFullSeq = GetInvoiceWithValidTestData();

				if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
				{
					invoice.AH_PostDate = new ZDate(2021, 1, 1);
					invFullSeq.AH_PostDate = new ZDate(2021, 4, 1);
				}
				else if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
				{
					invoice.AH_InvoiceDate = new ZDate(2021, 1, 1);
					invFullSeq.AH_InvoiceDate = new ZDate(2021, 4, 1);
				}

				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "PTLIS";
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var alterSeq = TestObjectCreator.SetupComplianceSequence(menuPK, subType, subType + ".21-", 1, 100, 2);
				alterSeq.XD_StartDate = new ZDate(2021, 1, 1);
				alterSeq.XD_ExpiryDate = new ZDateTime(2021, 3, 30);
				alterSeq.XD_IsActive = true;

				var fullSeq = TestObjectCreator.SetupComplianceSequence(menuPK, subType, subType + ".21_", 1, 100, 101);
				fullSeq.XD_StartDate = new ZDate(2021, 4, 1);
				fullSeq.XD_ExpiryDate = new ZDateTime(2021, 6, 30);
				fullSeq.XD_IsActive = true;

				Factory.Save();

				Type revInvType = invoice.TypeOfReverseTransaction_ForTestOnly;

				var revInvAlter = CreateInvoiceForCompliance(revInvType, "1001", alterSeq.XD_ExpiryDate.Date.AddDays(1), subType, alterSeq, allocationDateOption: dateOption);
				revInvAlter.OriginalTransaction = invoice;

				var revInvFull = CreateInvoiceForCompliance(revInvType, "1000", new ZDateTime(2021, 4, 1).Date, subType, fullSeq, allocationDateOption: dateOption);
				revInvFull.OriginalTransaction = invFullSeq;

				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
				using (complianceNumberAllocationDateRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (complianceDocumentNumberAllocationRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					AssertPreDeleteDialog_For_Reversing(revInvAlter, ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
					AssertPreDeleteDialog_For_Reversing(revInvFull, ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage);
				}
			}
		}

		[TestDate(2021, 1, 15)]
		public void TestCheckPostDateEarlierThanLastDateUsed_PST()
		{
			AssertCheckPostDateEarlierThanLastDateUsed(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2021, 1, 15)]
		public void TestCheckPostDateEarlierThanLastDateUsed_INV()
		{
			AssertCheckPostDateEarlierThanLastDateUsed(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		public void AssertCheckPostDateEarlierThanLastDateUsed(string dateOption)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var invoice = GetInvoiceWithValidTestData();
				(var subType, var complianceDocumentNumberAllocationRegistry, var complianceNumberAllocationDateRegistry) = GetComplianceParams(invoice);
				if (subType == null
					|| (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code && complianceNumberAllocationDateRegistry.Name == AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AP.Name))
				{
					Assert(true);
					return;
				}

				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "ITMIL";
				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;

				var complSeq = TestObjectCreator.SetupComplianceSequence(menuPK, subType, subType + ".21-", 1, 100, 3);
				complSeq.XD_StartDate = new ZDate(2021, 1, 1);
				complSeq.XD_ExpiryDate = new ZDateTime(2021, 3, 31);
				complSeq.XD_IsActive = true;

				var lastDateUsed = new ZDate(2021, 1, 10);

				Type revInvType = invoice.TypeOfReverseTransaction_ForTestOnly;
				var inv1 = CreateInvoiceForCompliance(revInvType, "INV1", new ZDate(2021, 1, 5), subType, complSeq, subType + ".21-0001", allocationDateOption: dateOption);
				var inv2 = CreateInvoiceForCompliance(revInvType, "INV2", new ZDate(2021, 1, 10), subType, complSeq, subType + ".21-0002", allocationDateOption: dateOption);

				Factory.Save();

				var reverseInvoice = CreateInvoiceForCompliance(revInvType, "1000", new ZDate(2021, 1, 5), subType, complSeq, allocationDateOption: dateOption);
				reverseInvoice.OriginalTransaction = invoice;

				var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
				using (complianceNumberAllocationDateRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (complianceDocumentNumberAllocationRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					AssertPreDeleteDialog_For_Reversing(reverseInvoice, string.Format(ComplianceSequenceNumberAllocationErrorMessages.GetUnableToAllocateNumberDueToAllocationDateEarlierThanLastDateUsedMessage(dateOption),
						subType, lastDateUsed.ToShortDateString()));
				}
			}
		}

		public void TestComplianceNumberAfterInvoicePosted_WrongSubType_PST()
		{
			AssertComplianceNumberAfterInvoicePosted_WrongSubType(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestComplianceNumberAfterInvoicePosted_WrongSubType_INV()
		{
			AssertComplianceNumberAfterInvoicePosted_WrongSubType(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		public void AssertComplianceNumberAfterInvoicePosted_WrongSubType(string dateOption)
		{
			var invoice = GetInvoiceWithValidTestData(false);
			invoice.AH_TransactionNum = "DUMMY";
			if (!invoice.IsInvoiceOrCreditNoteOrAdjustmentNote_ForTestOnly)
			{
				Assert(true);
				return;
			}

			var currCompany = GlbCompany.CurrentCompany;
			var currBranch = GlbBranch.CurrentBranch;
			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var today = ZDateTime.Today;
				new AccountingPeriodTestHelper(Factory).SetupPeriods();

				TestObjectCreator.CreateNewComplianceSequence(currCompany.PK, currBranch.PK, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI);
				TestObjectCreator.CreateNewComplianceSequence(currCompany.PK, currBranch.PK, ItalyComplianceInfo.ComplianceSubTypeCodes.API);

				invoice = GetInvoiceWithValidTestData();
				invoice.AH_TransactionNum = "INVx";
				invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
				TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 10);
				if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
				{
					invoice.AH_InvoiceDate = today;
				}
				else
				{
					invoice.AH_PostDate = today;
				}

				string subType = null, wrongSubType = null;
				var registry = AccountingMasterFilesRegistry.Instance;
				CodePairRegistryItem complianceDocumentNumberAllocationRegistry = null;
				CodePairRegistryItem complianceNumberAllocationDateRegistry = null;

				switch (invoice.Ledger_ForTestOnly)
				{
					case LedgerTypes.AccountsPayable:

						if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
						{
							Assert(true); //INV registry option is not compatible with AP invoice
							return;
						}

						subType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
						wrongSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
						complianceDocumentNumberAllocationRegistry = registry.ComplianceDocumentNumberAllocation_Payables;
						complianceNumberAllocationDateRegistry = registry.ComplianceNumberAllocationDate_AP;
						break;
					case LedgerTypes.AccountsReceivable:
						subType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARI;
						wrongSubType = ItalyComplianceInfo.ComplianceSubTypeCodes.API;
						complianceDocumentNumberAllocationRegistry = registry.ComplianceDocumentNumberAllocation_Receivables;
						complianceNumberAllocationDateRegistry = registry.ComplianceNumberAllocationDate_AR;
						break;
				}

				var guid_GC = currCompany.PK.ToGuid();
				using (complianceNumberAllocationDateRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (complianceDocumentNumberAllocationRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					using (var form = GetFormByInvoice(invoice))
					{
						form.DisplayMode = ODisplayMode.New;
						form.Show();

						invoice.AH_ComplianceSubType = wrongSubType;
						invoice.RunPreSaveValidation();
						AssertHasErrors("Error in Compliance Sequence on Posting", invoice.AH_ComplianceSubTypeInfo);

						invoice.AH_ComplianceSubType = subType;
						invoice.RunPreSaveValidation();
						AssertNoErrors("No more errors with right SubType", invoice);

						form.PostingButtonsUserControl.SaveButton.PerformClick();
						AssertStartsWith("Compliance Number in right SubType", subType, invoice.AH_TransactionReference);
					}
				}
			}
		}

		[TestDate(2023, 11, 15)]
		public void TestComplianceNumberAfterInvoicePosted_WrongDate_PST()
		{
			AssertComplianceNumberAfterInvoicePosted_WrongDate(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2023, 11, 15)]
		public void TestComplianceNumberAfterInvoicePosted_WrongDate_INV()
		{
			AssertComplianceNumberAfterInvoicePosted_WrongDate(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		public void AssertComplianceNumberAfterInvoicePosted_WrongDate(string dateOption)
		{
			var invoice = GetInvoiceWithValidTestData(false);
			invoice.AH_TransactionNum = "DUMMY";
			if (!invoice.IsInvoiceOrCreditNote_ForTestOnly)
			{
				Assert(true);
				return;
			}

			var currCompany = GlbCompany.CurrentCompany;
			var currBranch = GlbBranch.CurrentBranch;
			using (currCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				string subType = null;
				var registry = AccountingMasterFilesRegistry.Instance;
				CodePairRegistryItem complianceDocumentNumberAllocationRegistry = null;
				CodePairRegistryItem complianceNumberAllocationDateRegistry = null;
				switch (invoice.Ledger_ForTestOnly)
				{
					case LedgerTypes.AccountsPayable:
						if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
						{
							Assert(true);
							return;
						}

						complianceDocumentNumberAllocationRegistry = registry.ComplianceDocumentNumberAllocation_Payables;
						complianceNumberAllocationDateRegistry = registry.ComplianceNumberAllocationDate_AP;
						switch (invoice.TransactionType_ForTestOnly)
						{
							case TransactionTypes.Invoice:
								subType = PortugalComplianceInfo.ComplianceSubTypeCodes.PTX;
								break;
							case TransactionTypes.CreditNote:
								subType = PortugalComplianceInfo.ComplianceSubTypeCodes.PCR;
								break;
						}
						break;
					case LedgerTypes.AccountsReceivable:
						complianceDocumentNumberAllocationRegistry = registry.ComplianceDocumentNumberAllocation_Receivables;
						complianceNumberAllocationDateRegistry = registry.ComplianceNumberAllocationDate_AR;
						switch (invoice.TransactionType_ForTestOnly)
						{
							case TransactionTypes.Invoice:
								subType = PortugalComplianceInfo.ComplianceSubTypeCodes.TXI;
								break;
							case TransactionTypes.CreditNote:
								subType = PortugalComplianceInfo.ComplianceSubTypeCodes.TCR;
								break;
						}
						break;
				}

				var today = ZDateTime.Today;
				var wrongDate = today.AddMonths(-1);
				var middleDate = new ZDate(today.Year, today.Month, 1);

				var seq = TestObjectCreator.SetupComplianceSequence(ZGuid.Empty, subType, "1-", 1, 99, 1, currCompany.PK, currBranch.PK, new ZDate(today.Year, 1, 1), middleDate.AddDays(-1));
				seq.XD_Code = subType + "1";
				var previousInvoice1 = GetInvoiceWithValidTestData();
				previousInvoice1.AH_PostDate = previousInvoice1.AH_InvoiceDate = wrongDate.AddDays(1);
				previousInvoice1.AH_XD_ComplianceBook = seq.PK;

				seq = TestObjectCreator.SetupComplianceSequence(ZGuid.Empty, subType, "2-", 1, 99, 1, currCompany.PK, currBranch.PK, middleDate, new ZDate(today.Year, 12, 31));
				seq.XD_Code = subType + "2";
				var previousInvoice2 = GetInvoiceWithValidTestData();
				previousInvoice2.AH_PostDate = previousInvoice2.AH_InvoiceDate = middleDate;
				previousInvoice2.AH_XD_ComplianceBook = seq.PK;

				Factory.Save();

				new AccountingPeriodTestHelper(Factory).SetupPeriods();

				invoice = GetInvoiceWithValidTestData();
				invoice.AH_TransactionNum = "INVx";
				invoice.AH_OH = TestObjectCreator.TestOrganisation.PK;
				TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 10);
				invoice.AH_ComplianceSubType = subType;

				var guid_GC = currCompany.PK.ToGuid();
				using (complianceNumberAllocationDateRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
				using (complianceDocumentNumberAllocationRegistry.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				{
					using (var form = GetFormByInvoice(invoice))
					{
						form.DisplayMode = ODisplayMode.New;
						form.Show();

						SetInvoiceOrPostDate(wrongDate);
						invoice.RunPreSaveValidation();
						if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
						{
							AssertHasErrors("Error in InvoiceDate on Posting", invoice.AH_InvoiceDateInfo);
						}
						else
						{
							AssertHasErrors("Error in PostDate on Posting", invoice.AH_PostDateInfo);
						}

						SetInvoiceOrPostDate(today);
						invoice.RunPreSaveValidation();
						AssertNoErrors("No more errors with right Date", invoice);

						form.PostingButtonsUserControl.SaveButton.PerformClick();
						AssertStartsWith("Compliance Number in right Sequence", subType + " 2-", invoice.AH_TransactionReference);
					}
				}
			}

			void SetInvoiceOrPostDate(ZDateTime date)
			{
				if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
				{
					invoice.AH_InvoiceDate = date;
				}
				else
				{
					invoice.AH_PostDate = date;
				}
			}
		}

		public void TestAPUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcs()
		{
			var originalSecurityValue = Env.Security.APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcs.IsAllowed;
			try
			{
				var sisterCompanyPK = TestObjectCreator.CreateNewCompany("AAA").PK;
				var businessObject = Factory.NewWithValidTestData<ARInvoice>();
				Factory.Save();

				Assert(!APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcsHelper(businessObject, false, sisterCompanyPK));
				Assert(APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcsHelper(businessObject, true, sisterCompanyPK));
				Assert(APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcsHelper(businessObject, false, GlbCompany.CurrentCompany.PK));
				Assert(APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcsHelper(businessObject, true, GlbCompany.CurrentCompany.PK));
			}
			finally
			{
				Env.Security.APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcs.IsAllowed = originalSecurityValue;
			}
		}

		public void TestCaptionForInsertingIntoLabelsIsHonoured()
		{
			using (BaseInvoicingForm form = (BaseInvoicingForm)GetFormToBashCore())
			{
				Assert("CaptionForInsertingIntoLabels_ForTestOnly has been set to something", form.CaptionForInsertingIntoLabels_ForTestOnly != null && form.CaptionForInsertingIntoLabels_ForTestOnly.Length > 0);
				form.Show();
				Application.DoEvents();
				AssertEquals("Invoice details control edit date uses caption from base form.", form.InvoiceDetails.AH_InvoiceDateEdit.GetExtension<ILabelCaptionRenderer>().Caption, form.CaptionForInsertingIntoLabels_ForTestOnly + " Date");
				AssertEquals("Invoice details control transaction number uses caption from base form.", form.InvoiceDetails.AH_TransactionNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption, form.CaptionForInsertingIntoLabels_ForTestOnly + " Number");
			}
		}

		public void TestMutexMessageWhenReversing()
		{
			var dummyInvoice = GetInvoiceWithValidTestData();
			var invoiceType = dummyInvoice.GetType();

			if (dummyInvoice.AH_Ledger == LedgerTypes.AccountsPayable && (invoiceType == typeof(APInvoice) || invoiceType == typeof(APCreditNote)))
			{
				bool isTestingInvoiceForm = invoiceType == typeof(APInvoice);

				Job job1 = null;
				Job job2 = null;

				try
				{
					var consol = TestObjectCreator.CreateConsol("AUSYD", "AUMEL", "C001");
					consol.JK_UniqueConsignRef = "C0000558";

					var shipment = consol.Shipments.AddNew();
					shipment.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
					shipment.JS_UniqueConsignRef = "S0000558";

					var jobLoader = new Job.Loader(shipment);
					job1 = jobLoader.TryLoadOrCreateWithoutMutexForTestOnly();
					Factory.Save();

					var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.FRT, TestObjectCreator.Creditor1, null);
					cost.E6_ApportionmentMethod = ZArchitecture.Core.AllocationMethod.Shipment;
					cost.E6_OSCostAmount = isTestingInvoiceForm ? -1000 : 1000;
					cost.E6_InvoiceDate = ZDateTime.Today;
					cost.E6_PaymentDate = ZDateTime.Today;
					cost.E6_InvoiceNum = "123";
					Factory.Save();

					var jobCollection = new[] { job1 };
					var postManager = new ConsolInvoicingPostManager(Factory, jobCollection, consol, new ApportionmentListing(Factory, consol));
					var transactions = postManager.CreateTransactions(JobInvoicingPostingOption.Costs).GetAllAPTransactions();
					AssertEquals("precondition, 1 ap transaction created", 1, transactions.Length);
					var transactionToReversePK = transactions[0].PK;

					var shipment2 = consol.Shipments.AddNew();
					shipment2.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
					shipment2.JS_UniqueConsignRef = "S0000559";

					Factory.Save();

					job2 = new Job.Loader(shipment2).TryCreateWithMutex();

					foreach (bool useFireSaveButton in new[] { false, true })
					{
						var factory2 = new BusinessObjectFactory();
						var transactionToReverse = factory2.Load<InvoicingBase>(transactionToReversePK);

						ZController controller = transactionToReverse is APInvoice ? ZControllerFactory.Create(ControllerIDs.APInvoice) : ZControllerFactory.Create(ControllerIDs.APCreditNote);

						using (var form = (BaseInvoicingForm)controller.ShowDeleteForm(transactionToReverse))
						{
							AssertEquals("Shows correct reversal form for test", isTestingInvoiceForm ? typeof(InvoiceForm) : typeof(CreditNoteForm), form.GetType());

							var formTransaction = (InvoicingBase)form.BusinessEntity;
							formTransaction.AH_TransactionNum = "1234" + (useFireSaveButton ? "F" : "");
							formTransaction.RunPreSaveValidation();
							AssertNoErrors(formTransaction);

							form.SetReversingReason_ForTestOnly();

							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

							if (useFireSaveButton)
							{
								form.FireSaveButton();
							}
							else
							{
								((IPostingButtonsProvider)form).CommandButtonPost.PerformClick();
							}

							AssertEquals("Expected popup message shown as error, once", @"You have created the job S0000559 on another form, but haven't saved it yet.
Please close or save other forms that use job S0000559 to continue.", UnitTestUserNotification.Instance.LastMessage.Text);
							var messages = UnitTestUserNotification.Instance.PreviousMessages;
							var messageCount = messages.ToList().Count(m => !m.WasNone);
							AssertEquals("Only popup shown should be this message. I.e. printing pop ups are not shown", 1, messageCount);
						}
					}
				}
				finally
				{
					if (job1 != null)
					{
						job1.Dispose();
					}
					if (job2 != null)
					{
						job2.Dispose();
					}
				}
			}
			else
			{
				Assert("Only testing APInvoice and APCreditNote", true);
			}
		}

		#region Approval Requests

		public void TestSaveAsIncompleteMenuDisabledWhenRequestCreated()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && (invoice is APInvoice || invoice is APCreditNote))
			{
				invoice.Delete();

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.SetupInvoiceApprovalRequestAuthorization();
				invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(invoice.GetType(), TestObjectCreator.Creditor1, 100);
				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice);

				using (var form = GetFormByInvoice(invoice))
				{
					form.Show();

					Assert("Precondition: HasApprovalRequest", !invoice.HasApprovalRequest);
					invoice.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);
					form.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert("Invoice Editing: SaveAsIncompleteMenuItem.Enabled", !form.SaveAsIncompleteMenuItem.Enabled);
					invoice.RemoveContext(APInvoiceChargesApprovalRequest.Context.Editing);

					form.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert("New Invoice: SaveAsIncompleteMenuItem.Enabled", form.SaveAsIncompleteMenuItem.Enabled);

					invoice.SetContext(APInvoiceChargesApprovalRequest.Context.Posting);
					form.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert("Invoice Posting: SaveAsIncompleteMenuItem.Enabled", !form.SaveAsIncompleteMenuItem.Enabled);
					invoice.RemoveContext(APInvoiceChargesApprovalRequest.Context.Posting);

					form.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert("New Invoice: SaveAsIncompleteMenuItem.Enabled", form.SaveAsIncompleteMenuItem.Enabled);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					invoice.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);
					form.FireSaveButton();
					Assert("Precondition: HasApprovalRequest", invoice.HasApprovalRequest);
					form.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
					Assert("New Invoice saved with approval request: SaveAsIncompleteMenuItem.Enabled", !form.SaveAsIncompleteMenuItem.Enabled);
				}
			}
			else
			{
				Assert("Only AP Invoice and Credit Note can have approval request linked", true);
			}
		}

		public void TestFormBehaviourForNewInvoiceWithApprovalRequest()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice is APInvoice)
			{
				invoice.Delete();

				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.SetupInvoiceApprovalRequestAuthorization();
				invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(invoice.GetType(), TestObjectCreator.Creditor1, 100);
				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice);

				var dummyObjectToCheckIfFactorySaved = Factory.New<DummyBusinessObject>();

				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.New;
					form.Show();
					Application.DoEvents();

					var postButtonsProvider = form as IPostingButtonsProvider;
					AssertStartsWith("Form caption", "New AP", form.Text);
					AssertEquals("Apply button text", "Post", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Post  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", postButtonsProvider.CommandButtonPost.Enabled);
					Assert("ApportionChargesButton.Visible", form.InvoiceDetails.ApportionChargesButton.Visible);
					AssertNull("Precondition: No messages should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					form.AlwaysCreateApprovalRequest_ForTestOnly = true;
					form.FireSaveButton();
					form.AlwaysCreateApprovalRequest_ForTestOnly = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.None;
					Assert("Precondition: invoice is not posted", invoice.IsIncompleteInvoice);
					Assert("Precondition: HasApprovalRequest", invoice.HasApprovalRequest);
					Assert("Invoice factory should not be saved when only request is created.", !dummyObjectToCheckIfFactorySaved.IsInDatabase);
					AssertStartsWith("Form caption", "Edit AP", form.Text);
					AssertEquals("Apply button text", "New", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Post  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", !postButtonsProvider.CommandButtonPost.Enabled);
					Assert("ApportionChargesButton.Visible", form.InvoiceDetails.ApportionChargesButton.Visible);
					AssertNull("No print request should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

					invoice.AH_Desc += "Desc"; //edit invoice
					AssertEquals("Apply button text", "Post", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Post  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", postButtonsProvider.CommandButtonPost.Enabled);

					form.FireSaveButton();
					Assert("Precondition: invoice is posted", !invoice.IsIncompleteInvoice);
					Assert("Invoice factory should be saved when invoice posting.", dummyObjectToCheckIfFactorySaved.IsInDatabase);
					AssertEquals("Apply button text", "New", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Save  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", !postButtonsProvider.CommandButtonPost.Enabled);
					Assert("ApportionChargesButton.Visible", !form.InvoiceDetails.ApportionChargesButton.Visible);
					AssertEquals("Print request should be shown.", "Do you want to print a Cost Confirmation Document for this transaction?", UnitTestUserNotification.Instance.LastMessage.Text);

					invoice.AH_Desc += "Desc"; //edit invoice - like add eDocs
					AssertEquals("Apply button text", "Save", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Save  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", postButtonsProvider.CommandButtonPost.Enabled);
				}
			}
			else
			{
				Assert("Only AP Invoice and Credit Note can have approval request linked", true);
			}
		}

		public void TestConcurrencyOnProcessTaskForInvoiceWithApprovalRequest()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && (invoice is APInvoice || invoice is APCreditNote))
			{
				invoice.Delete();

				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.SetupInvoiceApprovalRequestAuthorization();
				invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(invoice.GetType(), TestObjectCreator.Creditor1, 100);
				TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);

				var processTask = ((IWorkflowProvider)invoice).WorkflowItems.AddNew();
				processTask.P9_Description = "1";
				processTask.P9_Type = "TRG";
				((IBaseTrigger)processTask).TriggerEventCode = Events.TransactionApprovalActionedCode;

				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice);

				Factory.Save();
				invoice.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);
				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();

					processTask.P9_Description = "2";

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					AssertNoExceptionThrown(() => form.FireSaveButton());
				}
			}
			else
			{
				Assert("Not applicable", true);
			}
		}

		public void TestFormBehaviourForEditingInvoiceWithApprovalRequest()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && (invoice is APInvoice || invoice is APCreditNote))
			{
				invoice.Delete();

				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.SetupInvoiceApprovalRequestAuthorization();
				invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(invoice.GetType(), TestObjectCreator.Creditor1, 100);
				TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);
				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice);

				var dummyObjectToCheckIfFactorySaved = Factory.New<DummyBusinessObject>();

				invoice.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);
				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();
					var postButtonsProvider = form as IPostingButtonsProvider;
					AssertStartsWith("Form caption", "Edit Unapproved AP", form.Text);
					AssertEquals("Apply button text", "Save", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Save  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", postButtonsProvider.CommandButtonPost.Enabled);
					Assert("ApportionChargesButton.Visible", form.InvoiceDetails.ApportionChargesButton.Visible);
					AssertNull("Precondition: No messages should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form.FireSaveButton();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.None;
					Assert("Precondition: invoice is not posted", invoice.IsIncompleteInvoice);
					Assert("Precondition: HasApprovalRequest", invoice.HasApprovalRequest);
					Assert("Invoice factory should not be saved when only request is created.", !dummyObjectToCheckIfFactorySaved.IsInDatabase);
					AssertStartsWith("Form caption", "Edit Unapproved AP", form.Text);
					AssertEquals("Apply button text", "Save", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", !postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Save  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", !postButtonsProvider.CommandButtonPost.Enabled);
					Assert("ApportionChargesButton.Visible", form.InvoiceDetails.ApportionChargesButton.Visible);
					var expectedMessage = @"There is another request for this transaction. Only one request is permitted.

Do you want to cancel previous request and queue this one for approval?";
					AssertEquals("No print request should be shown.", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					invoice.AH_Desc += "Desc"; //edit invoice
					AssertEquals("Apply button text", "Save", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Save  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", postButtonsProvider.CommandButtonPost.Enabled);
				}
			}
			else
			{
				Assert("Only AP Invoice and Credit Note can have approval request linked", true);
			}
		}

		public void TestSavingUnapprovalInvoiceWithEmptyComplianceSequenceWhenOrderingByPostDateEnabled()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && (invoice is APInvoice || invoice is APCreditNote))
			{
				invoice.Delete();
				var registry = AccountingMasterFilesRegistry.Instance;

				using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
				{
					var expectedWarningMessage = "Note: Unapproved Invoice is saved without Compliance Sub Type, because it must be set only when Compliance Number is allocated";
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
					invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(invoice.GetType(), TestObjectCreator.Creditor1, 100);
					invoice.RunPreSaveValidation();
					invoice.AH_ComplianceSubType = "API";
					invoice.SetContext(APInvoiceChargesApprovalRequest.Context.Editing);

					using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
					{
						using (var form = GetFormByInvoice(invoice))
						{
							form.DisplayMode = ODisplayMode.Edit;
							form.Show();
							Application.DoEvents();
							form.FireSaveButton();
							Assert("Compliance SubType is empty", invoice.AH_ComplianceSubType.IsEmpty);
							AssertEquals(expectedWarningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}

					invoice.AH_ComplianceSubType = "API";
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print))
					{
						using (var form = GetFormByInvoice(invoice))
						{
							form.DisplayMode = ODisplayMode.Edit;
							form.Show();
							Application.DoEvents();
							form.FireSaveButton();
							AssertEquals("Compliance SubType is API", "API", invoice.AH_ComplianceSubType);
							AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
				}
			}
			else
			{
				Assert("Only AP Invoice and Credit Note can have approval request linked", true);
			}
		}

		public void TestFormBehaviourForPostingInvoiceWithApprovalRequest()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && (invoice is APInvoice || invoice is APCreditNote))
			{
				invoice.Delete();

				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.SetupInvoiceApprovalRequestAuthorization();
				invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(invoice.GetType(), TestObjectCreator.Creditor1, 100);
				TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);
				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice);

				var dummyObjectToCheckIfFactorySaved = Factory.New<DummyBusinessObject>();

				invoice.SetContext(APInvoiceChargesApprovalRequest.Context.Posting);
				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					Application.DoEvents();

					var postButtonsProvider = form as IPostingButtonsProvider;
					AssertStartsWith("Form caption", "Post AP", form.Text);
					AssertEquals("Apply button text", "Post", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Post  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", postButtonsProvider.CommandButtonPost.Enabled);
					Assert("ApportionChargesButton.Visible", form.InvoiceDetails.ApportionChargesButton.Visible);
					AssertNull("Precondition: No messages should be shown.", UnitTestUserNotification.Instance.LastMessage.Text);

					form.FireSaveButton();
					Assert("Precondition: invoice is posted", !invoice.IsIncompleteInvoice);
					Assert("Invoice factory should be saved when invoice posting.", dummyObjectToCheckIfFactorySaved.IsInDatabase);
					AssertStartsWith("Form caption", "Post AP", form.Text);
					AssertEquals("Apply button text", "Save", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", !postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Save  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", !postButtonsProvider.CommandButtonPost.Enabled);
					Assert("ApportionChargesButton.Visible", !form.InvoiceDetails.ApportionChargesButton.Visible);
					AssertEquals("Print request should be shown.", "Do you want to print a Cost Confirmation Document for this transaction?", UnitTestUserNotification.Instance.LastMessage.Text);

					invoice.AH_Desc += "Desc"; //edit invoice - like add eDocs
					AssertEquals("Apply button text", "Save", postButtonsProvider.CommandButtonApply.Text.Replace("&", ""));
					Assert("Apply button enabled", postButtonsProvider.CommandButtonApply.Enabled);
					AssertEquals("Post button text", "Save  Close", postButtonsProvider.CommandButtonPost.Text.Replace("&", ""));
					Assert("Post button enabled", postButtonsProvider.CommandButtonPost.Enabled);
				}
			}
			else
			{
				Assert("Only AP Invoice and Credit Note can have approval request linked", true);
			}
		}

		public void TestFormBehaviourForInvoiceAllocationWhenApprovalRequestIsRequired()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice is APInvoice)
			{
				invoice.Delete();

				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.SetupInvoiceApprovalRequestAuthorization();
				var invoicePendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.Creditor1, 110);
				Factory.Save();
				invoice = TransactionAllocationConverter.ConvertUnallocatedToAP(invoicePendingAllocation).Invoice;
				TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 100);
				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice);

				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.Edit;
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					form.AlwaysCreateApprovalRequest_ForTestOnly = true;
					form.FireSaveButton();
					form.AlwaysCreateApprovalRequest_ForTestOnly = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.None;
					Assert("invoice is not posted", invoice.IsIncompleteInvoice);
					Assert("HasApprovalRequest", invoice.HasApprovalRequest);
				}
			}
			else
			{
				Assert("Only AP Invoice and Credit Note can have approval request linked", true);
			}
		}

		public void TestAmendingWithCreditNotePopsUpLoginFormWithRequestApprovalButton()
		{
			AssertAmendingWithCreditNotePopsUpLoginFormWithRequestApprovalButton(true);
			AssertAmendingWithCreditNotePopsUpLoginFormWithRequestApprovalButton(false);
		}

		void AssertAmendingWithCreditNotePopsUpLoginFormWithRequestApprovalButton(bool isAmendingWithInvoice)
		{
			var shipment = TestObjectCreator.CreateShipment(isAmendingWithInvoice ? "S0001" : "S0002");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), isAmendingWithInvoice ? "ARInv" : "ARCrd", TestObjectCreator.AUD, 1M, TestObjectCreator.ABIGAS);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 1500M, 0M, 0M, 1500M, 0M, 0M, TestObjectCreator.CC1.PK);
			line.AL_JH = job.PK;
			line.AL_GE = TestObjectCreator.FESDepartment.PK;
			TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1);
			Factory.Save();

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			var amendingTransaction = ((IAmending)invoice).GenerateAmendingTransaction(isAmendingWithInvoice ? TransactionTypes.Invoice : TransactionTypes.CreditNote);
			var amendingTransactionAsInvoicingBase = amendingTransaction as InvoicingBase;
			if (isAmendingWithInvoice && amendingTransactionAsInvoicingBase != null)
			{
				amendingTransactionAsInvoicingBase.Lines[0].AL_OSExTaxAmount = 100m;
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			SetReceivableAuthorizationLevelSettings();
			amendingTransactionAsInvoicingBase.RunPreSaveValidation();
			AssertNoErrors("Precondition", amendingTransactionAsInvoicingBase);

			Env.Instance.Registry.ShowSaveProgressBox = false;
			using (var form = new BaseInvoicingForm(amendingTransactionAsInvoicingBase))
			{
				form.Show();

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((formShown) =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = formShown.GetType() == typeof(LoginFormWithRequest) ? DialogResult.Ignore : DialogResult.OK;
				});

				var transactions = Factory.Load<AccTransactionHeader>(new ZDBOnlyQuery(typeof(AccTransactionHeader)));
				var approvals = Factory.Load<ARCreditNoteApprovalRequest>(new ARCreditNoteApprovalRequestCollection(Factory).CompleteFilter);
				Assert("Original Invoice Posted", transactions.Any(x => x.PK == invoice.PK));
				Assert("Amending Invoice / Credit Note Not Posted", !transactions.Any(x => x.PK == amendingTransaction.PK));
				Assert("No Approval Created", !approvals.Any(x => x.XP_ParentID == invoice.PK));

				form.FireSaveButton();

				transactions = Factory.Load<AccTransactionHeader>(new ZDBOnlyQuery(typeof(AccTransactionHeader)));
				approvals = Factory.Load<ARCreditNoteApprovalRequest>(new ARCreditNoteApprovalRequestCollection(Factory).CompleteFilter);
				if (isAmendingWithInvoice)
				{
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
					Assert("Original Invoice Posted", transactions.Any(x => x.PK == invoice.PK));
					Assert("Amending Invoice Posted", transactions.Any(x => x.PK == amendingTransaction.PK));
					Assert("No Approval Created", !approvals.Any(x => x.XP_ParentID == invoice.PK));
				}
				else
				{
					AssertEquals(typeof(ARCreditNoteApprovalBulkForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
					Assert("Original Invoice Posted", transactions.Any(x => x.PK == invoice.PK));
					Assert("Amending Credit Note Not Posted", !transactions.Any(x => x.PK == amendingTransaction.PK));
					Assert("Approval Created", approvals.Any(x => x.XP_ParentID == invoice.PK));
					AssertEquals(amendingTransactionAsInvoicingBase.TransactionRelatedApprovalRequest.PK, approvals[0].PK);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, approvals[0].XP_ApprovalStatus);
				}
			}

			if (!isAmendingWithInvoice)
			{
				var factoryForApprovals = new BusinessObjectFactory();
				factoryForApprovals.LoadTop1<ARCreditNoteApprovalRequest>(new ARCreditNoteApprovalRequestCollection(Factory).CompleteFilter).XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
				factoryForApprovals.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (var form = new BaseInvoicingForm(amendingTransactionAsInvoicingBase))
				{
					form.Show();
					form.FireSaveButton();
					AssertNotEquals(typeof(ARCreditNoteApprovalRequest), ZFormModaliser.LastFormShownDialogForTest.GetType());

					var transactions = Factory.Load<AccTransactionHeader>(new ZDBOnlyQuery(typeof(AccTransactionHeader)));
					var approvals = factoryForApprovals.Load<ARCreditNoteApprovalRequest>(new ARCreditNoteApprovalRequestCollection(Factory).CompleteFilter);
					Assert("Original Invoice Posted", transactions.Any(x => x.PK == invoice.PK));
					Assert("Amending Credit Note Posted", transactions.Any(x => x.PK == amendingTransaction.PK));
					Assert("Approval Created", approvals.Any(x => x.XP_ParentID == invoice.PK));
					AssertEquals(amendingTransactionAsInvoicingBase.TransactionRelatedApprovalRequest.PK, approvals[0].PK);
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Posted, approvals[0].XP_ApprovalStatus);
				}
			}
		}

		#region TestFormBehaviourForSystemComapnyInvoiceWhenApprovalRequestIsNOTRequired

		public void TestFormBehaviourForSystemComapnyInvoiceWhenApprovalRequestIsNOTRequired()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice is APInvoice)
			{
				invoice.Delete();

				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.SetupInvoiceApprovalRequestAuthorization();

				invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(invoice.GetType(), TestObjectCreator.Creditor1, 100, "INV1");
				invoice.IsConvertedFromARInvoice = true;
				AssertRequestIsCreated(invoice, shouldRequestBeCreated: false);

				invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(invoice.GetType(), TestObjectCreator.Creditor1, 100, "INV2");
				invoice.IsConvertedFromARInvoice = false;
				AssertRequestIsCreated(invoice, shouldRequestBeCreated: true);
			}
			else
			{
				Assert("Only AP Invoice and Credit Note can have approval request linked", true);
			}
		}

		void AssertRequestIsCreated(InvoicingBase invoice, bool shouldRequestBeCreated)
		{
			invoice.RunPreSaveValidation();
			AssertNoErrors("Precondition", invoice);

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.AlwaysCreateApprovalRequest_ForTestOnly = true;
				form.FireSaveButton();

				Assert("invoice is saved", invoice.IsInDatabase);
				AssertEquals("AH_Ledger", shouldRequestBeCreated ? LedgerTypes.IncompleteTransactions : LedgerTypes.AccountsPayable, invoice.AH_Ledger);
				AssertEquals("HasApprovalRequest", shouldRequestBeCreated, invoice.HasApprovalRequest);
			}
		}

		protected void SetReceivableAuthorizationLevelSettings()
		{
			var setting = new AuthorizationModeAndSettings();
			var collection = setting.AuthorisationSettings;
			var settings1 = collection.AddNew();
			settings1.Range = AmountBasedTwoLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings1.Amount = 1000.00m;
			settings1.AuthorisationRequirement = AmountBasedTwoLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			var settings2 = collection.AddNew();
			settings2.Range = AmountBasedTwoLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings2.Amount = 2000.00m;
			settings2.AuthorisationRequirement = AmountBasedTwoLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			var settings3 = collection.AddNew();
			settings3.Range = AmountBasedTwoLevelAuthorisationRequirement.RangeCodes.Above;
			settings3.Amount = 2000.00m;
			settings3.AuthorisationRequirement = AmountBasedTwoLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), setting);
		}

		#endregion

		#endregion

		#region Child Factory With Transaction Pending Allocation Approval Request

		public void TestChildFactoryIsSavedWhenOnlyApprovalRequestIsSaved()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice is APInvoice)
			{
				invoice.Delete();

				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.SetupInvoiceApprovalRequestAuthorization();
				invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(typeof(APInvoice), TestObjectCreator.Creditor1, 100);
				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice);

				var dummyObjectToCheckIfFactorySaved = Factory.New<DummyBusinessObject>();

				var childFactory = new TransactionPendingAllocationApprovalRequestFactory();
				var requestShouldBeSavedInChildFactory = childFactory.NewWithValidTestData<TransactionPendingAllocationApprovalRequest>();
				invoice.Factory.ChildFactories.Add(childFactory);

				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.New;
					form.Show();
					Application.DoEvents();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					form.AlwaysCreateApprovalRequest_ForTestOnly = true;
					form.FireSaveButton();
					form.AlwaysCreateApprovalRequest_ForTestOnly = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.None;
					Assert("Precondition: invoice is not posted", invoice.IsIncompleteInvoice);
					Assert("Precondition: HasApprovalRequest", invoice.HasApprovalRequest);
					Assert("Invoice factory should not be saved when only request is created.", !dummyObjectToCheckIfFactorySaved.IsInDatabase);
					Assert("Transaction Pending Allocation Approval Request in child factory should be saved when only request is created.", requestShouldBeSavedInChildFactory.IsInDatabase);

					requestShouldBeSavedInChildFactory = childFactory.NewWithValidTestData<TransactionPendingAllocationApprovalRequest>();
					invoice.AH_Desc += "Desc"; //edit invoice
					form.FireSaveButton();
					Assert("Precondition: invoice is posted", !invoice.IsIncompleteInvoice);
					Assert("Invoice factory should be saved when invoice posting.", dummyObjectToCheckIfFactorySaved.IsInDatabase);
					Assert("Transaction Pending Allocation Approval Request in child factory should be saved when invoice is posted.", requestShouldBeSavedInChildFactory.IsInDatabase);
				}
			}
			else
			{
				Assert("Only AP Invoice and Credit Note can have approval request linked", true);
			}
		}

		public void TestChildFactoryIsSavedWhenInvoiceSavedAsIncomplete()
		{
			var invoice = GetInvoiceWithValidTestData();
			if (invoice.AH_Ledger == LedgerTypes.AccountsPayable && invoice is APInvoice)
			{
				invoice.Delete();

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				invoice = TestObjectCreator.CreateAPInvoiceForApprovalRequest(typeof(APInvoice), TestObjectCreator.Creditor1, 100);
				invoice.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice);

				var childFactory = new TransactionPendingAllocationApprovalRequestFactory();
				var requestShouldBeSavedInChildFactory = childFactory.NewWithValidTestData<TransactionPendingAllocationApprovalRequest>();
				invoice.Factory.ChildFactories.Add(childFactory);

				using (var form = GetFormByInvoice(invoice))
				{
					form.DisplayMode = ODisplayMode.New;
					form.Show();
					Application.DoEvents();

					invoice.SaveAsIncomplete();
					Assert("Precondition: invoice is not posted", invoice.IsIncompleteInvoice);
					Assert("Transaction Pending Allocation Approval Request in child factory should be saved when only request is created.", requestShouldBeSavedInChildFactory.IsInDatabase);
				}
			}
			else
			{
				Assert("Only AP Invoice and Credit Note can be saved as Incomplete", true);
			}
		}

		#endregion

		#region Tax Summary Tab Page Test

		public void TestTaxSummaryTabPageDataWhenGSTRegisteredIsChanged()
		{
			var invoice = GetInvoiceWithValidTestData();
			invoice.AH_OH = TestObjectCreator.CreateOrgHeader("TES", true, true).PK;
			invoice.AH_GC = GlbCompany.CurrentCompany.PK;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();

				var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 12m, 22m, 32m);
				invoiceLine.AL_AT = TestObjectCreator.GST1.PK;
				form.InvoiceDetails.TabControl_ForTest.SelectTab("TaxSummaryTabPage");

				Assert("IsTaxSummaryTabSelected is true", form.Invoice_ForTestOnly.IsTaxSummaryTabSelected);
				AssertEquals("Should have 1 item", 1, form.Invoice_ForTestOnly.InvoiceLineTaxSummaries.Count);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				form.InvoiceDetails.TabControl_ForTest.SelectTab(0);
				form.InvoiceDetails.TabControl_ForTest.SelectTab("TaxSummaryTabPage");

				AssertEquals("Should still have 1 item", 1, form.Invoice_ForTestOnly.InvoiceLineTaxSummaries.Count);
			}
		}

		public void TestTaxSummaryGridTabChangeRefresh()
		{
			var invoice = GetInvoiceWithValidTestData();
			invoice.AH_OH = TestObjectCreator.CreateOrgHeader("TES", true, true).PK;

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();

				Assert("Should more than 2 tab pages", form.InvoiceDetails.TabControl_ForTest.TabCount >= 2);
				Assert("First tab page should not be TaxSummaryTabPage", form.InvoiceDetails.TabControl_ForTest.TabPages[0].Name != "TaxSummaryTabPage");

				form.InvoiceDetails.TabControl_ForTest.SelectTab(0);
				Assert("IsTaxSummaryTabSelected is false", !form.Invoice_ForTestOnly.IsTaxSummaryTabSelected);

				var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 10m, 20m, 30m);
				var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 11m, 21m, 31m);

				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);

				form.InvoiceDetails.TabControl_ForTest.SelectTab("TaxSummaryTabPage");

				Assert("IsTaxSummaryTabSelected is true", form.Invoice_ForTestOnly.IsTaxSummaryTabSelected);
				AssertEquals("Should group by 0 item", 0, form.Invoice_ForTestOnly.InvoiceLineTaxSummaries.Count);

				form.InvoiceDetails.TabControl_ForTest.SelectTab(0);

				var invoiceLine3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 12m, 22m, 32m);
				invoiceLine3.AL_AT = TestObjectCreator.GST1.PK;

				Assert("IsTaxSummaryTabSelected is false", !form.Invoice_ForTestOnly.IsTaxSummaryTabSelected);
				AssertEquals("Should group by 0 item", 0, form.Invoice_ForTestOnly.InvoiceLineTaxSummaries.Count);

				form.InvoiceDetails.TabControl_ForTest.SelectTab("TaxSummaryTabPage");
				Assert("IsTaxSummaryTabSelected is true", form.Invoice_ForTestOnly.IsTaxSummaryTabSelected);
				AssertEquals("Should group by 1 items", form.Invoice_ForTestOnly.InvoiceLineTaxSummaries.Count, 1);
				TaxSummaryTestHelper.AssertTaxSummaryResult(form.Invoice_ForTestOnly.InvoiceLineTaxSummaries[0], TestObjectCreator.GST1.AT_Code, null, 12, invoiceLine3.AL_LocalTaxAmount, invoiceLine3.AL_LocalTotalAmount, TestObjectCreator.GST1.AT_Description);
			}
		}

		public void TestTaxSummaryGridWhenDebtorChanged()
		{
			var invoice = GetInvoiceWithValidTestData();

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();

				var org = TestObjectCreator.CreateOrgHeader("123", true, true);
				invoice.AH_OH = org.PK;

				Assert("Should more than 2 tab pages", form.InvoiceDetails.TabControl_ForTest.TabCount >= 2);
				Assert("First tab page should not be TaxSummaryTabPage", form.InvoiceDetails.TabControl_ForTest.TabPages[0].Name != "TaxSummaryTabPage");

				form.InvoiceDetails.TabControl_ForTest.SelectTab(0);
				Assert("IsTaxSummaryTabSelected is false", !form.Invoice_ForTestOnly.IsTaxSummaryTabSelected);

				var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 10m, 20m, 30m);
				var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 11m, 21m, 31m);
				invoiceLine1.AL_AT = TestObjectCreator.GST1.PK;
				invoiceLine2.AL_AT = TestObjectCreator.GST1.PK;

				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);

				invoice.AH_OH = ZGuid.Empty; // change debtor/creditor
				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);

				invoice.AH_OH = org.PK;
				invoiceLine1.AL_AT = TestObjectCreator.GST1.PK;
				invoiceLine2.AL_AT = TestObjectCreator.GST1.PK;

				form.InvoiceDetails.TabControl_ForTest.SelectTab("TaxSummaryTabPage");
				Assert("IsTaxSummaryTabSelected is true", form.Invoice_ForTestOnly.IsTaxSummaryTabSelected);
				AssertEquals("Should contain 1 item", 1, form.Invoice_ForTestOnly.InvoiceLineTaxSummaries.Count);
				TaxSummaryTestHelper.AssertTaxSummaryResult(form.Invoice_ForTestOnly.InvoiceLineTaxSummaries[0], TestObjectCreator.GST1.AT_Code, null, 21, invoiceLine1.AL_LocalTaxAmount + invoiceLine2.AL_LocalTaxAmount, invoiceLine1.AL_LocalTotalAmount + invoiceLine2.AL_LocalTotalAmount, TestObjectCreator.GST1.AT_Description);
			}
		}

		public void TestTaxSummaryGridWhenExchangeRateChanged()
		{
			var invoice = GetInvoiceWithValidTestData();
			invoice.AH_ExchangeRate = 1m;
			invoice.AH_OH = TestObjectCreator.CreateOrgHeader("TES", true, true).PK;

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();

				Assert("Should more than 2 tab pages", form.InvoiceDetails.TabControl_ForTest.TabCount >= 2);
				Assert("First tab page should not be TaxSummaryTabPage", form.InvoiceDetails.TabControl_ForTest.TabPages[0].Name != "TaxSummaryTabPage");

				form.InvoiceDetails.TabControl_ForTest.SelectTab(0);
				Assert("IsTaxSummaryTabSelected is false", !form.Invoice_ForTestOnly.IsTaxSummaryTabSelected);

				var invoiceLine1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 10m, 20m, 30m);
				var invoiceLine2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 11m, 21m, 31m);
				invoiceLine1.AL_AT = TestObjectCreator.GST1.PK;
				invoiceLine2.AL_AT = TestObjectCreator.GST1.PK;

				var exTaxAmount = invoiceLine1.AL_LocalExTaxAmount + invoiceLine2.AL_LocalExTaxAmount;
				var taxAmount = invoiceLine1.AL_LocalTaxAmount + invoiceLine2.AL_LocalTaxAmount;
				var totalAmount = invoiceLine1.AL_LocalTotalAmount + invoiceLine2.AL_LocalTotalAmount;

				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);

				invoice.AH_ExchangeRate = 2m; // change exchange rate
				AssertEquals("ExchangeRate should be", 2m, invoice.AH_ExchangeRate);
				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);

				invoice.AH_ExchangeRate = 1m;
				AssertEquals("ExchangeRate should be", 1m, invoice.AH_ExchangeRate);

				form.InvoiceDetails.TabControl_ForTest.SelectTab("TaxSummaryTabPage");
				Assert("IsTaxSummaryTabSelected is true", form.Invoice_ForTestOnly.IsTaxSummaryTabSelected);
				AssertEquals("Should contain 1 item", form.Invoice_ForTestOnly.InvoiceLineTaxSummaries.Count, 1);
				TaxSummaryTestHelper.AssertTaxSummaryResult(form.Invoice_ForTestOnly.InvoiceLineTaxSummaries[0], TestObjectCreator.GST1.AT_Code, null, exTaxAmount, taxAmount, totalAmount, TestObjectCreator.GST1.AT_Description); // tax info is empty

				invoice.AH_ExchangeRate = 2m; // change exchange rate
				AssertEquals("ExchangeRate should be", 2m, invoice.AH_ExchangeRate);
				AssertEquals("Should contain 1 item", form.Invoice_ForTestOnly.InvoiceLineTaxSummaries.Count, 1);
				TaxSummaryTestHelper.AssertTaxSummaryResult(form.Invoice_ForTestOnly.InvoiceLineTaxSummaries[0], TestObjectCreator.GST1.AT_Code, null, exTaxAmount / 2, taxAmount / 2, totalAmount / 2, TestObjectCreator.GST1.AT_Description); // amount changed correctly
			}
		}

		#endregion

		#region Test Prompt To Print Compliance Document

		protected void AssertForPromptToPrintComplianceDocument()
		{
			AssertEquals("User should be prompted to print Compliance Document", "Do you want to print Compliance Document?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("LastFormShownDialogForTest should be DocDeliveryForm", typeof(DocumentEngine.GUI.DocDeliveryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("LastIBusinessShownOnDialogForTest should be DeliveryInstructions", typeof(DeliveryInstructions), ZFormModaliser.LastIBusinessShownOnDialogForTest.GetType());
		}

		protected void AssertForNotPromptToPrintComplianceDocument()
		{
			AssertNotEquals("User should not be prompted to print Compliance Document", "Do you want to print Compliance Document?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("LastFormShownDialogForTest should be null", ZFormModaliser.LastFormShownDialogForTest);
			AssertNull("LastIBusinessShownOnDialogForTest should be null", ZFormModaliser.LastIBusinessShownOnDialogForTest);
		}

		public void TestPromptToPrintComplianceDocumentCore(bool isPromptToPrintComplianceDocumentOnCreation, Action assert)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.PromptToPrintComplianceDocumentOnCreation.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isPromptToPrintComplianceDocumentOnCreation))
			{
				TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

				var periodHelper = new AccountingPeriodTestHelper(Factory);
				periodHelper.SetupPeriods();

				var organisation = testObjectCreator.ABIGAS;
				organisation.CompanyData.OB_ARVATConfig = Constants.OrganisationTaxConfiguartionTypes.Default;
				organisation.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;
				organisation.OH_IsCreditor = true;
				organisation.CompanyData.OB_APVATConfig = Constants.OrganisationTaxConfiguartionTypes.Default;
				organisation.CompanyData.OB_APCreateVATComplianceDocumentOnPosting = Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;
				organisation.CompanyData.OB_APCostsSelfBilled = true;

				var stmMenu = Factory.Load<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Triplicate Cash Register GUI").AddToFilter(StmMenuItemSchema.SU_BusinessContext, "ARComplianceDocument")).First();
				testObjectCreator.SetupComplianceSequence(stmMenu.PK, "TDP", "AAA", 1, 100, 1);
				testObjectCreator.SetupComplianceSequence(stmMenu.PK, "TCD", "AAA", 1, 100, 1);

				Factory.Save();

				var invoice = GetInvoiceWithValidTestData();
				invoice.AH_OH = organisation.PK;
				var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
				var chargeList = invoiceLine.ChargeList;
				chargeList.Load();
				invoiceLine.GenericCharge = chargeList[0].PK;
				invoiceLine.AL_OSExTaxAmount = 10m;
				invoiceLine.AL_AT = testObjectCreator.GST1.PK;

				if (invoice.CanCreateComplianceDocument)
				{
					using (var form = GetFormByInvoice(invoice))
					{
						form.DisplayMode = ODisplayMode.New;
						form.Show();

						ZFormModaliser.LastFormShownDialogForTest = null;
						ZFormModaliser.LastIBusinessShownOnDialogForTest = null;

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

						form.ValidateAndSave_ForTestOnly();

						AssertNotNull(invoice.GetTransactionGeneratedComplianceDocument());

						if (this.GetType() != typeof(APIncompleteInvoiceFormTest) && this.GetType() != typeof(APIncompleteCreditNoteFormTest))
						{
							Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
						}

						assert();
					}
				}
				else
				{
					Assert(true);
				}
			}
		}

		#endregion

		public void TestNotPromptPostingGroupsMessageWhenComplianceDocumentModuleEnabled()
		{
			var vat3 = TestObjectCreator.CreateTaxRate("VAT3", "VAT3", 3);
			vat3.AT_PostingGroupId = 0;

			var vat5 = TestObjectCreator.CreateTaxRate("VAT5", "VAT5", 5);
			vat5.AT_PostingGroupId = 1;

			Factory.Save();

			using (var form = GetFormByInvoice(GetInvoiceWithValidTestData(false)))
			{
				form.Show();

				var invoice = form.Invoice_ForTestOnly;
				var line1 = (InvoicingLineBase)form.Invoice_ForTestOnly.Lines.AddNew();
				var line2 = (InvoicingLineBase)form.Invoice_ForTestOnly.Lines.AddNew();
				line1.AL_AT = vat3.PK;
				line2.AL_AT = vat5.PK;

				using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPostingGroupsPromptYesOrNo()
		{
			var tax1 = Factory.NewWithValidTestData<AccTaxRate>();
			var tax2 = Factory.NewWithValidTestData<AccTaxRate>();

			tax1.AT_Type = AccTaxRate.Types.Rated;
			tax1.AT_PostingGroupId = 1;
			tax2.AT_Type = AccTaxRate.Types.Rated;
			tax2.AT_PostingGroupId = 2;

			Factory.Save();

			using (var form = GetFormByInvoice(GetInvoiceWithValidTestData(false)))
			{
				form.Show();

				var invoice = form.Invoice_ForTestOnly;
				var line1 = (InvoicingLineBase)form.Invoice_ForTestOnly.Lines.AddNew();
				var line2 = (InvoicingLineBase)form.Invoice_ForTestOnly.Lines.AddNew();
				line1.AL_AT = tax1.PK;
				line2.AL_AT = tax2.PK;

				if (!invoice.IsReversalTransaction)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
					{
						Assert(!AccTaxRate.IsPostingGroupsEnabled("AU"));
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
						AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					}

					using (GlbCompany.CurrentCompany.TemporarilySetCountry("VN"))
					{
						Assert(AccTaxRate.IsPostingGroupsEnabled("VN"));
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
						AssertEquals("You have prepared charges using a mix of Tax ID Posting Groups. Are you sure you want to post these charges?"
							, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				else
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals(ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestPeriodApportionmentTabHiddenWhenPosted()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			TestObjectCreator.CreateInvoiceLine(invoice, 100);

			void assertTabVisibility(bool visible)
			{
				using (var form = new InvoiceForm(invoice))
				{
					form.Show();
					Application.DoEvents();
					AssertEquals(visible, form.PeriodApportionmentTabPage.TabVisible);
				}
			}

			assertTabVisibility(true);
			Factory.Save();
			assertTabVisibility(false);
		}

		public void TestPeriodApportionmentTabFocus()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			TestObjectCreator.CreateInvoiceLine(invoice, 100);

			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				form.InvoiceDetails.TransactionLinesGrid.SetAllColumnsVisible(true);
				Application.DoEvents();
				var grid = form.InvoiceDetails.TransactionLinesGrid;

				ChangeCellAndAssert(grid, nameof(InvoicingLineBase.AL_AT), form.LineChargesTabPage);
				ChangeCellAndAssert(grid, nameof(InvoicingLineBase.PeriodStartDate), form.PeriodApportionmentTabPage);
				ChangeCellAndAssert(grid, nameof(InvoicingLineBase.AL_LocalTaxAmount), form.LineChargesTabPage);
				ChangeCellAndAssert(grid, nameof(InvoicingLineBase.PeriodEndDate), form.PeriodApportionmentTabPage);
				ChangeCellAndAssert(grid, nameof(InvoicingLineBase.AL_OverseasTotal), form.LineChargesTabPage);
				ChangeCellAndAssert(grid, nameof(InvoicingLineBase.PeriodClearingGLAccountPK), form.PeriodApportionmentTabPage);
				ChangeCellAndAssert(grid, nameof(InvoicingLineBase.AL_OSTaxAmount), form.LineChargesTabPage);
				ChangeCellAndAssert(grid, nameof(InvoicingLineBase.PeriodApportionmentMethod), form.PeriodApportionmentTabPage);
			}
		}

		public void TestPeriodApportionmentTabVisibility()
		{
			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", organisation: TestObjectCreator.Creditor1);
			var line = TestObjectCreator.CreateInvoiceLine(invoice, 100);

			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals(true, form.PeriodApportionmentTabPage.TabVisible);
			}
		}

		public void TestSubAccountsTabPageFocus()
		{
			var glHeader = TestObjectCreator.CreateGLHeaderWithSubAccount(OrgHeaderSchema.Constants.Prefix, false);
			var invoice = GetInvoiceWithValidTestData();
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = glHeader.PK;
			using (var form = GetFormByInvoice(invoice))
			{
				form.Show();
				form.InvoiceDetails.TransactionLinesGrid.SetAllColumnsVisible(true);
				Application.DoEvents();
				var grid = form.InvoiceDetails.TransactionLinesGrid;

				ChangeCellAndAssert(grid, nameof(DependentTransactionLine.Schema.AL_AT), form.LineChargesTabPage);
				ChangeCellAndAssert(grid, nameof(DependentTransactionLine.Schema.AL_Calc_FirstSubClassParent), form.SubAccountsTabPage_ForTestOnly);

				ChangeCellAndAssert(grid, nameof(DependentTransactionLine.Schema.AL_LocalTaxAmount), form.LineChargesTabPage);
				ChangeCellAndAssert(grid, nameof(DependentTransactionLine.Schema.AL_Calc_FirstSubClassParentId), form.SubAccountsTabPage_ForTestOnly);

				ChangeCellAndAssert(grid, nameof(DependentTransactionLine.Schema.AL_OverseasTotal), form.LineChargesTabPage);
				ChangeCellAndAssert(grid, nameof(DependentTransactionLine.Schema.AL_Calc_SecondSubClassParent), form.SubAccountsTabPage_ForTestOnly);

				ChangeCellAndAssert(grid, nameof(DependentTransactionLine.Schema.AL_OSTaxAmount), form.LineChargesTabPage);
				ChangeCellAndAssert(grid, nameof(DependentTransactionLine.Schema.AL_Calc_SecondSubClassParentId), form.SubAccountsTabPage_ForTestOnly);
			}
		}

		public virtual void TestExtendDropEdit_AH_Calc_AmendStatusCode()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertExtendDropEdit();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertExtendDropEdit();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertExtendDropEdit();

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertExtendDropEdit();

			void AssertExtendDropEdit()
			{
				var invoice = GetInvoiceWithValidTestData();
				using (var form = GetFormByInvoice(invoice))
				{
					form.Show();
					form.InvoiceDetailsTabPage.Select();

					AssertExtendDropEditDefault(form);
				}
			}
		}

		#region US Sales Tax Menu Item

		public void TestRequestSalesTaxMenuItems_Visibility()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0M, 150M, 50M, 0M, 200M, 50M, 0M, TestObjectCreator.Creditor1);

			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				mockCalculator.Setup(x => x.IsEnabled(It.IsNotNull<GlbBranch>()))
								.Returns(false);
				mockCalculator.Setup(x => x.ShouldShowMenuItemsOnInvoiceForm(It.IsNotNull<InvoicingBase>()))
								.Returns(true);

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
				{
					testForm.Show();
					AssertNull("'Request Sales Tax Calculation' Menu Item should not be available when functionality is disabled", testForm.RequestSalesTaxCalculationMenuItem);
					AssertNull("'Submit Sales Tax for Transaction' Menu Item should not be available when functionality is disabled", testForm.RequestSalesTaxSubmissionMenuItem);
				}
			}

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				mockCalculator.Setup(x => x.IsEnabled(It.IsNotNull<GlbBranch>()))
								.Returns(true);
				mockCalculator.Setup(x => x.ShouldShowMenuItemsOnInvoiceForm(It.IsNotNull<InvoicingBase>()))
								.Returns(false);

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
				{
					testForm.Show();
					AssertNull("'Request Sales Tax Calculation' Menu Item should not be available when menu item method is false", testForm.RequestSalesTaxCalculationMenuItem);
					AssertNull("'Submit Sales Tax for Transaction' Menu Item should not be available when menu item method is false", testForm.RequestSalesTaxSubmissionMenuItem);
				}
			}

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				mockCalculator.Setup(x => x.IsEnabled(It.IsNotNull<GlbBranch>()))
							  .Returns(true);
				mockCalculator.Setup(x => x.ShouldShowMenuItemsOnInvoiceForm(It.IsNotNull<InvoicingBase>()))
							  .Returns(true);

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
				{
					testForm.Show();
					AssertNotNull("'Request Sales Tax Calculation' Menu Item should be visible when functionality is enabled and menu item method is true", testForm.RequestSalesTaxCalculationMenuItem);
					AssertNotNull("'Submit Sales Tax for Transaction' Menu Item should be visible when functionality is enabled and menu item method is true", testForm.RequestSalesTaxSubmissionMenuItem);

					AssertEquals("'Request Sales Tax Calculation' Menu Item should use generic text when not set via interface", "Request Sales Tax Calculation", testForm.RequestSalesTaxCalculationMenuItem.Text);
					AssertEquals("'Submit Sales Tax for Transaction' Menu Item should use generic text when not set via interface", "Submit Sales Tax for Transaction", testForm.RequestSalesTaxSubmissionMenuItem.Text);
				}
			}

			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				mockCalculator.Setup(x => x.IsEnabled(It.IsNotNull<GlbBranch>()))
							  .Returns(true);
				mockCalculator.Setup(x => x.ShouldShowMenuItemsOnInvoiceForm(It.IsNotNull<InvoicingBase>()))
							  .Returns(true);
				mockCalculator.Setup(x => x.CalculateMenuItemText)
							  .Returns((NoResString)"Calculate All the Sales Taxes!!");
				mockCalculator.Setup(x => x.SubmitMenuItemText)
							  .Returns((NoResString)"Submit All the Sales Taxes!! (final; no undo is possible)");

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
				{
					testForm.Show();
					AssertEquals("'Request Sales Tax Calculation' Menu Item should use interface provided text", "Calculate All the Sales Taxes!!", testForm.RequestSalesTaxCalculationMenuItem.Text);
					AssertEquals("'Submit Sales Tax for Transaction' Menu Item should use interface provided text", "Submit All the Sales Taxes!! (final; no undo is possible)", testForm.RequestSalesTaxSubmissionMenuItem.Text);
				}
			}
		}

		public void TestRequestSalesTaxMenuItems_DoNotRun_WhenDisabled()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0M, 150M, 50M, 0M, 200M, 50M, 0M, TestObjectCreator.Creditor1);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				mockCalculator.Setup(x => x.IsEnabled(It.IsNotNull<GlbBranch>()))
								.Returns(true);
				mockCalculator.Setup(x => x.ShouldShowMenuItemsOnInvoiceForm(It.IsNotNull<InvoicingBase>()))
								.Returns(true);
				using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
				{
					AssertNotNull("Precondition: menu item is enabled", testForm.RequestSalesTaxCalculationMenuItem);
					mockCalculator.Setup(x => x.IsEnabled(It.IsNotNull<GlbBranch>()))
									.Returns(false);

					testForm.RequestSalesTaxCalculationMenuItem.PerformClick();

					AssertNotNull("When disabled, no popups should be shown for calculating sales tax", UnitTestUserNotification.Instance.LastMessage);
					mockCalculator.Verify(x => x.CalculateSalesTax(It.IsAny<InvoicingBase>()),
									Times.Never,
									"CalculateSalesTax() should not be called when functionality is disabled."
					);

					testForm.RequestSalesTaxSubmissionMenuItem.PerformClick();

					AssertNotNull("When disabled, no popups should be shown for submitting sales tax", UnitTestUserNotification.Instance.LastMessage);
					mockCalculator.Verify(x => x.SubmitSalesTax(It.IsAny<InvoicingBase>()),
									Times.Never,
									"SubmitSalesTax() should not be called when functionality is disabled."
					);
				}
			}
		}

		public void TestRequestSalesTaxMenuItems_ShowsSecurityError_WhenCheckpointIsNotAllowed()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0M, 150M, 50M, 0M, 200M, 50M, 0M, TestObjectCreator.Creditor1);

			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				var calculationCheckpoint = new SecurityCheckpoint("Arbitrary Checkpoint Calculation", (NoResString)"Arbitrary Checkpoint for Calculation", null, Env.Security.SecurityInstance, addToLookUpTable: false);
				calculationCheckpoint.IsAllowed = false;
				var submissionCheckpoint = new SecurityCheckpoint("Arbitrary Checkpoint Submission", (NoResString)"Arbitrary Checkpoint for Submission", null, Env.Security.SecurityInstance, addToLookUpTable: false);
				submissionCheckpoint.IsAllowed = false;

				mockCalculator.Setup(x => x.IsEnabled(It.IsNotNull<GlbBranch>()))
								.Returns(true);
				mockCalculator.Setup(x => x.ShouldShowMenuItemsOnInvoiceForm(It.IsNotNull<InvoicingBase>()))
								.Returns(true);
				mockCalculator.Setup(x => x.CheckpointForCalculationMenuItem(It.IsNotNull<InvoicingBase>()))
								.Returns(calculationCheckpoint);
				mockCalculator.Setup(x => x.CheckpointForSubmitMenuItem(It.IsNotNull<InvoicingBase>()))
								.Returns(submissionCheckpoint);

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testForm.RequestSalesTaxCalculationMenuItem.PerformClick();

					var expectedSecurityMessageCalculation = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Arbitrary Checkpoint for Calculation";
					AssertEquals("Security Error Message Should Be Shown when checkpoint is not allowed", expectedSecurityMessageCalculation, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					mockCalculator.Verify(x => x.CalculateSalesTax(It.IsAny<InvoicingBase>()),
									Times.Never,
									"CalculateSalesTax() should not be called when security is denied."
					);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testForm.RequestSalesTaxSubmissionMenuItem.PerformClick();

					var expectedSecurityMessageSubmission = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Arbitrary Checkpoint for Submission";
					AssertEquals("Security Error Message Should Be Shown when checkpoint is not allowed", expectedSecurityMessageSubmission, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					mockCalculator.Verify(x => x.SubmitSalesTax(It.IsAny<InvoicingBase>()),
									Times.Never,
									"SubmitSalesTax() should not be called when security is denied."
					);
				}
			}
		}

		public void TestRequestSalesTaxMenuItems_ShowsErrorPopup_WhenErrorReturned()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0M, 150M, 50M, 0M, 200M, 50M, 0M, TestObjectCreator.Creditor1);

			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				var checkpoint = new SecurityCheckpoint("Arbitrary Checkpoint", (NoResString)"Arbitrary Checkpoint", null, Env.Security.SecurityInstance, addToLookUpTable: false);
				checkpoint.IsAllowed = true;

				mockCalculator.Setup(x => x.Name)
								.Returns("MockCalculator");
				mockCalculator.Setup(x => x.IsEnabled(It.IsNotNull<GlbBranch>()))
								.Returns(true);
				mockCalculator.Setup(x => x.ShouldShowMenuItemsOnInvoiceForm(It.IsNotNull<InvoicingBase>()))
								.Returns(true);
				mockCalculator.Setup(x => x.CheckpointForCalculationMenuItem(It.IsNotNull<InvoicingBase>()))
								.Returns(checkpoint);
				mockCalculator.Setup(x => x.CheckpointForSubmitMenuItem(It.IsNotNull<InvoicingBase>()))
								.Returns(checkpoint);
				mockCalculator.Setup(x => x.MenuItemTroubleshootingHint)
								.Returns((NoResString)"For further diagnostic information, attach a debugger");
				var nestedExceptionForCalculate = new Exception("Some error when calculating sales tax.", new ApplicationException("Some inner error"));
				mockCalculator.Setup(x => x.CalculateSalesTax(It.IsNotNull<InvoicingBase>()))
								.Returns((null, nestedExceptionForCalculate));
				var nestedExceptionForSubmit = new Exception("Some error when submitting sales tax.", new ApplicationException("Some other inner error"));
				mockCalculator.Setup(x => x.SubmitSalesTax(It.IsNotNull<InvoicingBase>()))
								.Returns((null, nestedExceptionForSubmit));

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testForm.RequestSalesTaxCalculationMenuItem.PerformClick();

					var expectedErrorMessageForCalculate = @"An error occurred calculating sales tax using MockCalculator:
  Exception: Some error when calculating sales tax.
  ApplicationException: Some inner error

For further diagnostic information, attach a debugger";
					AssertEquals("Error Message Should Be Shown when returned from calculation", expectedErrorMessageForCalculate, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testForm.RequestSalesTaxSubmissionMenuItem.PerformClick();

					var expectedErrorMessageForSubmit = @"An error occurred submitting sales tax using MockCalculator:
  Exception: Some error when submitting sales tax.
  ApplicationException: Some other inner error

For further diagnostic information, attach a debugger";
					AssertEquals("Error Message Should Be Shown when returned from submission", expectedErrorMessageForSubmit, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
				}
			}
		}

		public void TestRequestSalesTaxMenuItems_ShowsSummaryPopup_WhenIsSuccessful()
		{
			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0M, 150M, 50M, 0M, 200M, 50M, 0M, TestObjectCreator.Creditor1);

			var mockCalculator = new Mock<IUSSalesTaxCalculator>();
			using (ObjectFactory.Substitute(mockCalculator.Object))
			{
				var checkpoint = new SecurityCheckpoint("Arbitrary Checkpoint", (NoResString)"Arbitrary Checkpoint", null, Env.Security.SecurityInstance, addToLookUpTable: false);
				checkpoint.IsAllowed = true;

				mockCalculator.Setup(x => x.Name)
								.Returns("MockCalculator");
				mockCalculator.Setup(x => x.IsEnabled(It.IsNotNull<GlbBranch>()))
								.Returns(true);
				mockCalculator.Setup(x => x.GetConfiguration(It.IsNotNull<GlbBranch>()))
								.Returns(ConfigurationStatus.Sandbox);
				mockCalculator.Setup(x => x.ShouldShowMenuItemsOnInvoiceForm(It.IsNotNull<InvoicingBase>()))
								.Returns(true);
				mockCalculator.Setup(x => x.CheckpointForCalculationMenuItem(It.IsNotNull<InvoicingBase>()))
								.Returns(checkpoint);
				mockCalculator.Setup(x => x.CheckpointForSubmitMenuItem(It.IsNotNull<InvoicingBase>()))
								.Returns(checkpoint);
				mockCalculator.Setup(x => x.MenuItemTroubleshootingHint)
								.Returns((NoResString)"For further diagnostic information, attach a debugger");
				mockCalculator.Setup(x => x.GetChargeCode(It.IsNotNull<GlbBranch>()))
								.Returns((ZGuid.NewZGuid(), "SALESTAX999"));
				var calcResult = new CalculationResult(5555.55m, totalInvoiceAmountExcludingSalesTax: 47328.00m);
				mockCalculator.Setup(x => x.CalculateSalesTax(It.IsNotNull<InvoicingBase>()))
								.Returns((calcResult, null));
				mockCalculator.Setup(x => x.GetCurrentSalesTaxAmount(It.IsNotNull<InvoicingBase>()))
								.Returns(123.45m);

				using (BaseInvoicingForm testForm = new BaseInvoicingForm(invoice))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testForm.RequestSalesTaxCalculationMenuItem.PerformClick();

					var expectedSummaryMessageForCalculate = @"Calculated Sales Tax Amount: $5,555.55

Current Sales Tax Amount: $123.45
Total Invoice Amount Used for Calculation: $47,328.00

Charge Code: SALESTAX999
Environment: Sandbox
Provider: MockCalculator

For further diagnostic information, attach a debugger";
					AssertEquals("Summary Message Should Be Shown when returned from calculation", expectedSummaryMessageForCalculate, UnitTestUserNotification.Instance.LastMessage.Text);

					// Test case for Submit menu item
					var calcResultWithStatus = new CalculationResult(5555.55m,
						totalInvoiceAmountExcludingSalesTax: 47328.00m,
						submissionStatus: "Good"
					);
					mockCalculator.Setup(x => x.SubmitSalesTax(It.IsNotNull<InvoicingBase>())).Returns((calcResultWithStatus, null));
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testForm.RequestSalesTaxSubmissionMenuItem.PerformClick();

					var expectedSummaryMessageForSubmit = @"Submitted Sales Tax Amount: $5,555.55

Current Sales Tax Amount: $123.45
Total Invoice Amount Used for Calculation: $47,328.00
Status: Good

Charge Code: SALESTAX999
Environment: Sandbox
Provider: MockCalculator

For further diagnostic information, attach a debugger";
					AssertEquals("Summary Message Should Be Shown when returned from submission", expectedSummaryMessageForSubmit, UnitTestUserNotification.Instance.LastMessage.Text);

					// Test case for optional warning
					var calcResultWithWarning = new CalculationResult(5555.55m,
						totalInvoiceAmountExcludingSalesTax: 47328.00m,
						warningMessage: "There's some more details, check the details widget to see them"
					);
					mockCalculator.Setup(x => x.CalculateSalesTax(It.IsNotNull<InvoicingBase>())).Returns((calcResultWithWarning, null));

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testForm.RequestSalesTaxCalculationMenuItem.PerformClick();

					var expectedSummaryMessageWithWarning = @"Calculated Sales Tax Amount: $5,555.55

Current Sales Tax Amount: $123.45
Total Invoice Amount Used for Calculation: $47,328.00

Charge Code: SALESTAX999
Environment: Sandbox
Provider: MockCalculator

WARNING: There's some more details, check the details widget to see them

For further diagnostic information, attach a debugger";
					AssertEquals("Summary Message Should contain optional warning message", expectedSummaryMessageWithWarning, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		protected void AssertExtendDropEditDefault(BaseInvoicingForm form)
		{
			var extendDropEdit = form.InvoiceDetails.ExtendDropEdit_ForTestOnly;
			CombineAssertions("Hide as default", () =>
			{
				AssertEquals("Visible", false, extendDropEdit.Visible);
				AssertEquals("Enabled", false, extendDropEdit.Enabled);
				AssertEquals("Text", ZString.Empty, extendDropEdit.Text);
			});
		}

		protected void AssertExtendDropEditStatusCode(BaseInvoicingForm form)
		{
			var extendDropEdit = form.InvoiceDetails.ExtendDropEdit_ForTestOnly;
			CombineAssertions("Show as StatusCode", () =>
			{
				AssertEquals("Visible", true, extendDropEdit.Visible);
				AssertEquals("Enabled", true, extendDropEdit.Enabled);
				AssertEquals("Text", "Amend Status Code", extendDropEdit.CaptionResourceString.Caption);
			});
		}

		public void TestReversalDropEdit_IsVisible()
		{
			var invoice = GetInvoiceWithValidTestData();
			var mockIInvoiceFormPresentationProvider = new Mock<IInvoiceFormPresentationProvider>();
			var mockIAccountingPresentationProviderFactory = new Mock<IAccountingPresentationProviderFactory>();
			mockIInvoiceFormPresentationProvider.Setup(x => x.GetIsReversalStatusCodeVisible(It.IsAny<InvoicingBase>())).Returns(false);
			mockIAccountingPresentationProviderFactory.Setup(x => x.GetInvoiceFormPresentationProvider()).Returns(mockIInvoiceFormPresentationProvider.Object);

			ObjectFactory.Substitute(mockIAccountingPresentationProviderFactory.Object);

			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();

				AssertEquals(false, form.InvoiceDetails.ReversalDropEdit.Visible);
				mockIInvoiceFormPresentationProvider.Verify(x => x.GetIsReversalStatusCodeVisible(invoice));
			}

			mockIInvoiceFormPresentationProvider.Setup(x => x.GetIsReversalStatusCodeVisible(It.IsAny<InvoicingBase>())).Returns(true);

			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();

				AssertEquals(true, form.InvoiceDetails.ReversalDropEdit.Visible);
				mockIInvoiceFormPresentationProvider.Verify(x => x.GetIsReversalStatusCodeVisible(invoice));
			}
		}

		public void TestReversalDropEdit_GetCountryFactory_MustBeCalledWith_TransactionCountryCode()
		{
			var expectedCountryCode = "MX";
			var invoice = GetInvoiceWithValidTestData();
			invoice.Company.GC_RN_NKCountryCode = expectedCountryCode;

			AssertNotEquals("Precondition, current login country and invoice's country must be different", invoice.Company.GC_RN_NKCountryCode, GlbCompany.CurrentCompany.Country.Code);

			var mockGlobalAccouting = new Mock<IGlobalAccountingCountryFactory>();
			mockGlobalAccouting.Setup(x => x.GetCountryFactory(It.IsAny<ZString>()));
			ObjectFactory.Substitute(mockGlobalAccouting.Object);

			using (var form = new BaseInvoicingForm(invoice))
			{
				form.Show();

				mockGlobalAccouting.Verify(x => x.GetCountryFactory(expectedCountryCode));
			}
		}

		public void TestEditCostForm_FormClosedShouldHaveLinePropertiesChangeStack()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);
			var consol = creator.CreateConsol();
			var shipment = consol.Shipments.AddNew();
			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			factory.Save();

			var invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = creator.AALSHI.PK;

			using (var form = new InvoiceForm(invoice))
			{
				form.Show();
				form.InvoiceDetails.ApportionChargesButton.PerformClick();
				var apportionmentForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
				var cost1 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost1.E6_AC_ChargeCode = creator.CC12.PK;
				cost1.E6_OSCostAmount = 50M;
				cost1.E6_AH_APInvoice = invoice.PK;
				cost1.ApportionmentCharges[0].JR_OSCostAmt = 50M;

				var cost2 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost2.E6_AC_ChargeCode = creator.CC12.PK;
				cost2.E6_OSCostAmount = 50M;
				cost2.E6_AH_APInvoice = invoice.PK;
				cost2.ApportionmentCharges[0].JR_OSCostAmt = 50M;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				apportionmentForm.Close();

				form.InvoiceDetails.TransactionLinesGrid.Select(0);
				var menuItem = form.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
				menuItem.PerformClick();

				var singleConsolCostingEditForm = (APInvoiceConsolCostingSingleEditForm)ZFormModaliser.ActiveForm;

				factory.SetContext(BusinessContext.APInvoiceApportionToConsol);
				invoice.Lines[0].RaiseApportionedLineModified_ForTestOnly();
				invoice.Lines[0].AL_TaxDate = ZDate.Today;
				invoice.Lines[1].RaiseApportionedLineModified_ForTestOnly();

				AssertNoExceptionThrown(() => { form.EditCostForm_FormClosed_ForTestOnly(singleConsolCostingEditForm, null); });
				AssertContains("LastMessageReported contains message errors", "ModifyingConsolCostDetailsFromAPInvoice context should have been removed already.", ErrorReporter.LastMessageReported);
				AssertContains("LastMessageReported contains stack trace", $"Line PK:{invoice.Lines[0].PK}\r\n   at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase.RaiseApportionedLineModified()\r\n   at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase.RaiseApportionedLineModified_ForTestOnly()\r\n", ErrorReporter.LastExceptionReported.Message);
				AssertContains("LastMessageReported contains stack trace", $"Line PK:{invoice.Lines[0].PK}\r\n   at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase.RaiseApportionedLineModified()\r\n   at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase.set_AL_TaxDate(ZDate value)\r\n", ErrorReporter.LastExceptionReported.Message);
				AssertContains("LastMessageReported contains stack trace", $"Line PK:{invoice.Lines[1].PK}\r\n   at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase.RaiseApportionedLineModified()\r\n   at Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingLineBase.RaiseApportionedLineModified_ForTestOnly()\r\n", ErrorReporter.LastExceptionReported.Message);

				singleConsolCostingEditForm.BusinessEntity.Factory.RemoveContext(BusinessContext.ModifyingConsolCostDetailsFromAPInvoice);
				singleConsolCostingEditForm.Close();
				form.Close();
				ErrorReporter.Clear();
			}
		}

		#region Implementation

		protected void AssertOriginalInvoiceReferenceGUIVisibilty(BaseInvoicingForm form, bool isReferenceGUIVisible, bool isReasonGUIVisible)
		{
			form.Show();
			form.SetInvoiceHeaderDefaults_ForTestOnly();

			AssertEquals("Original Invoice Reference TransactionGuidFindBox visibility.", isReferenceGUIVisible, form.InvoiceDetails.TransactionGuidFindBox.Visible);
			AssertEquals("Original Invoice Reference AH_OriginalInvoiceDateEdit visibility.", isReferenceGUIVisible, form.InvoiceDetails.AH_OriginalInvoiceDateEdit.Visible);
			AssertEquals("Original Invoice Reference AH_OriginalTransactionNumTextBox visibility.", isReferenceGUIVisible, form.InvoiceDetails.AH_OriginalTransactionNumTextBox.Visible);
			AssertEquals("Original Invoice Reference ReasonDescriptionTextBox visibility.", isReasonGUIVisible, form.InvoiceDetails.ReasonDescriptionTextBox.Visible);
			AssertEquals("Original Invoice Reference ReasonCodeDropEdit visibility.", isReasonGUIVisible, form.InvoiceDetails.ReasonCodeDropEdit.Visible);
		}

		bool APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcsHelper(ARInvoice aRInvoice, bool aPUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcs, ZGuid invoiceCompanyPK)
		{
			bool result;
			Env.Security.APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcs.IsAllowed = aPUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcs;
			aRInvoice.AH_GC = invoiceCompanyPK;
			using (var generatedForm = new BaseInvoicingForm(aRInvoice))
			{
				AssertNotNull(generatedForm);
				generatedForm.Show();
				result = generatedForm.LineChargesGrid.Visible &&
					!generatedForm.RestrictedLineChargesLabel_ForTestOnly.Visible &&
					generatedForm.RestrictedLineChargesLabel_ForTestOnly.Text != Env.Security.APUnapprovedInvoicesAlwVisLnChrgGrdIntrCmpnyInvcs.ErrorMessageForNotAllowed;
			}
			return result;
		}

		void AssertAccountLabelText(BaseInvoicingForm form, string expectedText)
		{
			Control[] found = form.Controls.Find("AddressWithContactControl", true);
			AssertNotNull("AddressWithContactControl should be found", found);
			AssertEquals("Only one AddressWithContactControl should be found", 1, found.Length);
			AssertEquals("AddressWithContactControl Resource String Caption should be as expected", expectedText, found[0].GetExtension<ILabelCaptionRenderer>().Caption);
		}

		protected abstract BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice);

		protected abstract InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null);

		protected abstract bool ShouldShowRelatedInvoicesTab { get; }

		protected sealed override Form GetFormToBashCore()
		{
			var invoice = GetInvoiceWithValidTestData(false);
			TaxFrameworkTestObjectCreator.CreateTaxConfiguration(invoice.Company, TaxConfigurationLedgers.AccountsPayable.Code);
			TaxFrameworkTestObjectCreator.CreateTaxConfiguration(invoice.Company, TaxConfigurationLedgers.AccountsReceivable.Code);
			return GetFormByInvoice(invoice);
		}

		protected TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get { return taxFrameworkTestObjectCreator ?? (taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory)); }
		}
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;

		#region Tab Page Focus

		void SelectCell(ZGrid grid, string columnName)
		{
			var columnIndex = grid.TableStyles[0].GridColumnStyles.IndexOf(grid.TableStyles[0].GridColumnStyles[columnName]);
			grid.CurrentCell = new DataGridCell(0, columnIndex);
			Application.DoEvents();
		}

		void AssertCellIsStillSelected(ZGrid grid, string columnName, bool assertRefocused = true)
		{
			var focusedCell = grid.CurrentCell;
			var focusedCellName = grid.TableStyles[0].GridColumnStyles[focusedCell.ColumnNumber]?.MappingName;
			if (assertRefocused)
			{
				AssertEquals(columnName, focusedCellName);
			}
		}

		void ChangeCellAndAssert(ZGrid grid, string columnName, ZTabPage expectedTabPage, bool assertRefocused = true)
		{
			SelectCell(grid, columnName);
			AssertEquals(expectedTabPage, (grid.FindForm() as BaseInvoicingForm).ChargesAndApportionmentsTabControl.SelectedTab);
			AssertCellIsStillSelected(grid, columnName, assertRefocused);
		}

		#endregion

		#endregion
	}
}
