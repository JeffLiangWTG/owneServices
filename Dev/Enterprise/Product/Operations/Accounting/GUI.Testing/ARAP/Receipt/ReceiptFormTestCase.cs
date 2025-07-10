using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.Module;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.DocumentScanning.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Testing
{
	[TestedType(typeof(ReceiptForm))]
	sealed class ReceiptFormTestCase : AccountingZFormBasherTest
	{
		public void TestOnLoad_CloseButtonText()
		{
			APReceipt aPRec = Factory.NewWithValidTestData<APReceipt>();
			Factory.Save();

			using (ReceiptForm recForm = new ReceiptForm(aPRec))
			{
				recForm.DisplayMode = ODisplayMode.Browse;
				recForm.Show();
				AssertEquals("CloseButton.Text", "Close", ((IAccessReceiptFormInternalsForTest)recForm).CloseButton.Text);
			}

			using (ReceiptForm recForm = new ReceiptForm(aPRec))
			{
				recForm.DisplayMode = ODisplayMode.Delete;
				recForm.Show();
				AssertEquals("CloseButton.Text", ZFormPostingButtonsStrategy.CancelButtonText(recForm).Text, ((IAccessReceiptFormInternalsForTest)recForm).CloseButton.Text);
			}
		}

		public void TestDisplayModeChanged()
		{
			using (ReceiptForm recForm = (ReceiptForm)GetFormToBashCore())
			{
				recForm.DisplayMode = ODisplayMode.ReadOnly;
				Assert("Post button should not be enabled", !((IAccessReceiptFormInternalsForTest)recForm).PostButton.Enabled);
			}
		}

		public void TestReferenceNumberLabel()
		{
			using (ReceiptForm form = (ReceiptForm)GetFormToBashCore())
			{
				AssertEquals("Text on ChequeNo label should be 'Reference No'", "Reference No", ((IAccessReceiptFormInternalsForTest)form).ChequeNoTextBox.GetExtension<LabelCaptionRenderer>().Caption);
			}
		}

		public void TestEDIMessagesPluginAttached()
		{
			using (ReceiptForm form = new ReceiptForm(Factory.NewWithValidTestData<ARReceipt>()))
			{
				Assert("AR Receipt form should have the plugin attached", form.PlugIns.GetPlugIn(ControllerIDs.LinkedeNettEDIMessage) != null);
			}
			using (ReceiptForm form = new ReceiptForm(Factory.NewWithValidTestData<APReceipt>()))
			{
				Assert("AP Receipt form should NOT have the plugin attached", form.PlugIns.GetPlugIn(ControllerIDs.LinkedeNettEDIMessage) == null);
			}
		}

		[GuiTest]
		public void TestReceiptPrintPrompting()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			OrgHeader org = GetNewTestOrg();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();

			APReceipt testAPReceipt = GetNewAPReceipt(org, bank);

			using (ReceiptForm form = new ReceiptForm(testAPReceipt))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.New;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((IAccessReceiptFormInternalsForTest)form).OnApplyButtonClick(this, EventArgs.Empty);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}

			ARReceipt testARReceipt = GetNewARReceipt(org, bank);

			using (ReceiptForm form = new ReceiptForm(testARReceipt))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.New;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((IAccessReceiptFormInternalsForTest)form).OnApplyButtonClick(this, EventArgs.Empty);

				AssertEquals("Last message should ask users if they want to print the receipt",
					"This option can be turned off via the following Registry: Accounting > Receivable Defaults > Default Settings > Receipt Print Prompting.\r\n\r\nDo you want to print receipt now?",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AccountingConfigurationRegistry.Instance.AccountingReceiptPrintPrompting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			testARReceipt = GetNewARReceipt(org, bank);

			using (ReceiptForm form = new ReceiptForm(testARReceipt))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.New;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				((IAccessReceiptFormInternalsForTest)form).OnApplyButtonClick(this, EventArgs.Empty);

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[GuiTest]
		public void TestShouldNotShowReceiptPrintForm()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var org = GetNewTestOrg();
			var receipt = GetNewARReceipt(org, TestObjectCreator.AUDBankAccount);

			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				receiptForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				receiptForm.PostWithoutMatchingClick();
				Assert("IsInDataBase", receipt.IsInDatabase);
				AssertEquals("Last message should ask users if they want to print the receipt",
					"This option can be turned off via the following Registry: Accounting > Receivable Defaults > Default Settings > Receipt Print Prompting.\r\n\r\nDo you want to print receipt now?",
					UnitTestUserNotification.Instance.LastMessage.Text);

				receipt.AH_Desc = "Desc";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				receiptForm.PostWithoutMatchingClick();
				AssertNull("Shouldn't show print form again", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReceiptFormDoesNotForceMatchingContextAndUsesProperValidation()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var org = GetNewTestOrg();
			var bank = Factory.NewWithValidTestData<AccBankAccount>();

			var testAPReceipt = GetNewAPReceipt(org, bank);
			AssertEquals("Use ReceiptValidation for new Receipt", typeof(ReceiptValidation), testAPReceipt.Validation.GetType());
			AssertEquals("Is not in Matching context", false, testAPReceipt.IsInMatchingContext);

			using (ReceiptForm form = new ReceiptForm(testAPReceipt))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.New;

				((IAccessReceiptFormInternalsForTest)form).OnApplyButtonClick(this, EventArgs.Empty);
			}
			Assert("IsInDataBase", testAPReceipt.IsInDatabase);
			AssertEquals("Still not in Matching context", false, testAPReceipt.IsInMatchingContext);
			AssertEquals("Should be TransactionHeaderEmptyValidation for saved Receipt", typeof(TransactionHeaderEmptyValidation), testAPReceipt.Validation.GetType());

			ARReceipt testARReceipt = GetNewARReceipt(org, bank);
			AssertEquals("Use ReceiptValidation for new Receipt", typeof(ReceiptValidation), testARReceipt.Validation.GetType());
			AssertEquals("Is not in Matching context", false, testARReceipt.IsInMatchingContext);

			AccountingConfigurationRegistry.Instance.AccountingReceiptPrintPrompting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (ReceiptForm form = new ReceiptForm(testARReceipt))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.New;

				((IAccessReceiptFormInternalsForTest)form).OnApplyButtonClick(this, EventArgs.Empty);
			}
			Assert("IsInDataBase", testARReceipt.IsInDatabase);
			AssertEquals("Still not in Matching context", false, testARReceipt.IsInMatchingContext);
			AssertEquals("Should be TransactionHeaderEmptyValidation for saved Receipt", typeof(TransactionHeaderEmptyValidation), testARReceipt.Validation.GetType());
		}

		public void TestMatchingFormPostSuccessful()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			OrgHeader testOrg = GetNewTestOrg();
			Factory.Save();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S1");
			Job job = TestObjectCreator.CreateJob(shipment, false);

			APCreditNote testAPInv = Factory.NewWithValidTestData<APCreditNote>();
			testAPInv.AH_OH = testOrg.PK;
			ZGuid genericChargeCodeGuid = TestObjectCreator.CC1.PK;

			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 34m, 0m, 0m, 34m, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;

			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);

			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			APReceipt receipt = GetNewAPReceipt(testOrg, TestObjectCreator.AUDBankAccount);

			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					receiptForm.Show();
					receiptForm.ReceiptDetailButtonClick();

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					MatchingBase matchingBizO = matchingForm.BusinessEntity as MatchingBase;

					matchingBizO.MoveAllFromUnmatchToMatch();

					AssertNotNull("Matching Object should be APMatchingBase", matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					matchingForm.MatchAndCloseButton.PerformClick();

					AssertEquals("DisplayMode of Original receiptForm should be Browse", ODisplayMode.Browse,
						receiptForm.DisplayMode);
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
				}
			}
		}

		public void TestMatchingFormHasCorrectButtonsWhenLoadedByClickingReceiptDetailButton()
		{
			SaveMatchingFormFilterLayout();

			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			OrgHeader testOrg = GetNewTestOrg();
			Factory.Save();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S1");
			Job job = TestObjectCreator.CreateJob(shipment, false);

			APCreditNote testAPInv = Factory.NewWithValidTestData<APCreditNote>();
			testAPInv.AH_OH = testOrg.PK;
			ZGuid genericChargeCodeGuid = TestObjectCreator.CC1.PK;

			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(testAPInv, testAPInv.TransactionCurrency, testAPInv.AH_ExchangeRate, 34m, 0m, 0m, 34m, 0m, 0m, genericChargeCodeGuid);
			line.AL_JH = job.PK;

			var charge = TestObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, testAPInv.TransactionCurrency);

			testAPInv.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			APReceipt receipt = GetNewAPReceipt(testOrg, TestObjectCreator.AUDBankAccount);

			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					receiptForm.Show();
					receiptForm.ReceiptDetailButtonClick();

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);

					AssertEquals("matchingForm.MatchAndContinueButton.Enabled", false, matchingForm.MatchAndContinueButton.Enabled);
					AssertEquals("matchingForm.MatchAndCloseButton.Enabled", true, matchingForm.MatchAndCloseButton.Enabled);
					AssertEquals("matchingForm.CloseButton.Enabled", true, matchingForm.CloseButton.Enabled);

					AssertEquals("matchingForm.MatchAndContinueButton.Visible", false, matchingForm.MatchAndContinueButton.Visible);
					AssertEquals("matchingForm.MatchAndCloseButton.Visible", true, matchingForm.MatchAndCloseButton.Visible);
					AssertEquals("matchingForm.CloseButton.Visible", true, matchingForm.CloseButton.Visible);
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
				}
			}
		}

		void SaveMatchingFormFilterLayout()
		{
			var matchingBase = new ARMatchingBase(Factory, Factory.NewWithValidTestData<ARPayment>());
			using (var form = new NewMatchGroupForm(matchingBase))
			{
				form.Show();

				var tabControl = form.GetControl<ZTemplateTabControl>("MatchingTabControl");
				var tabPage = tabControl.AllTabPages.FirstOrDefault(tp => tp.Name == "GridsTabPage") as ZTabPage;
				AssertNotNull(tabPage);

				var filterControl = tabPage.GetControl<MatchingFormFilterControl>("TransactionFilterControl");
				AssertNotNull(filterControl);

				var allNumberFilter = filterControl.FilterBusinessObject[MatchingFilterBusinessObject.AllNumbers];
				allNumberFilter.IsActive = true;
				AssertNotNull(allNumberFilter);

				filterControl.FilterBusinessObject.SaveLayout("TstARLayout");
			}
		}

		public void TestSavingReceiptAfterMatching()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var testOrg = GetNewTestOrg();

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			arInvoice.AH_OH = testOrg.PK;
			arInvoice.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			var receipt = GetNewAPReceipt(testOrg, TestObjectCreator.AUDBankAccount);

			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					receiptForm.Show();
					receiptForm.ReceiptDetailButtonClick();

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					MatchingBase matchingBizO = matchingForm.BusinessEntity as MatchingBase;

					matchingBizO.MoveAllFromUnmatchToMatch();

					AssertNotNull(matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);

					var overpayment = new MiscellaneousTransactionCreatorAR(Factory).CreateOverpayment(24M, 1M, testOrg);
					matchingBizO.AddMiscellaneousTransaction(overpayment);
					AssertEquals("Should be 3 transactions in MatchedTransactions", 3, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					matchingForm.MatchAndCloseButton.PerformClick();

					AssertEquals("DisplayMode of Original receiptForm should be Browse", ODisplayMode.Browse, receiptForm.DisplayMode);

					AssertNull("Shouldn't have any errors", UnitTestUserNotification.Instance.LastMessage.Text);
					receipt.AH_Desc = "Desc";
					receiptForm.PostWithoutMatchingClick();

					AssertNull("Shouldn't have any errors", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
				}
			}
		}

		public void TestShowRetryMessageAndDisableSaveButton_WhenMatchFailedByConcurrentData()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var testOrg = GetNewTestOrg();

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			arInvoice.AH_OH = testOrg.PK;
			arInvoice.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();

			var receipt = GetNewAPReceipt(testOrg, TestObjectCreator.AUDBankAccount);
			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				NewMatchGroupForm matchingForm = null;
				try
				{
					receiptForm.Show();
					receiptForm.ReceiptDetailButtonClick();

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					var matchingBizO = matchingForm.BusinessEntity as MatchingBase;
					AssertNotNull(matchingBizO);
					matchingBizO.MoveAllFromUnmatchToMatch();

					var exchangeDifference = TestObjectCreator.CreateExchangeDifference<ARExchangeDifference>(-24m, ZDateTime.Today, testOrg.PK);
					exchangeDifference.AH_OutstandingAmount = 24M;
					matchingBizO.AddMiscellaneousTransaction(exchangeDifference);
					AssertEquals("Should be 3 transactions in MatchedTransactions", 3, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Should exist exchange difference in MatchedTransactions", true, matchingBizO.MatchedTransactions.Cast<TransactionHeader>().Any(n => n is ARExchangeDifference));
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					var newFactory = NewFactory();
					newFactory.RefreshEnabled = false;
					var arInv = newFactory.Load<ARInvoice>(arInvoice.PK);
					arInv.AH_Desc = "A new Desc";
					newFactory.Save();

					UnitTestUserNotification.Instance.AddOKAnswer();
					matchingForm.MatchAndCloseButton.PerformClick();
					AssertContains("Should notify user concurrent data", "another user has made changes", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					UnitTestUserNotification.Instance.AddOKAnswer();
					matchingForm.CancelButton.PerformClick();
					var retryMessage = "An error has occurred. Your unsaved work must be re-entered.\r\n\r\nPlease close the form in which you were working and re-enter the data.";
					AssertEquals("Should notify user to close and try again", retryMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					AssertEquals("ReceiptDetailButton should be disabled", false, receiptForm.ReceiptDetailButtonEnabled);
					AssertEquals("PostWithoutMatchingButton should be disabled", false, receiptForm.PostWithoutMatchingButtonEnabled);
					AssertEquals("Receipt form should be ReadOnly", ODisplayMode.ReadOnly, receiptForm.DisplayMode);

					AssertNoExceptionThrown("Should close receipt form without exception", receiptForm.CancelButton.PerformClick);
					AssertEquals("Should not notify user", null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should not report any errors", string.Empty, ErrorReporter.LastMessageReported);
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
				}
			}
		}

		public void TestUnmatchDateControl()
		{
			var receipt = Factory.NewWithValidTestData<APReceipt>();
			Factory.Save();

			using (var form = new ReceiptFormForTest(receipt))
			{
				receipt.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Now, AllowBackPosting = true });
				AssertEquals("Precondition: MinUnmatchDate value", true, receipt.UnmatchingData.WasUnmatched);
				form.Show();
				AssertEquals("Precondition: UnmatchDateInfo_ReadOnly", false, receipt.UnmatchDate_ReadOnly);
				AssertEquals("Precondition: No. of Documents should have correct Top position.", form.PostDateEditExposed.Top, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Top);
				AssertEquals("Precondition: Unmatch Date should have correct Top position.", form.PostDateEditExposed.Top, form.UnmatchDateEditExposed.Top);
				AssertEquals("Precondition: No. of Documents should not be visible", false, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Visible);
				AssertEquals("UnmatchDateEditExposed.Visible", true, form.UnmatchDateEditExposed.Visible);
				AssertEquals("Unmatch Date should have correct Left position.", form.ReceiptNoTextBoxExposed.Left, form.UnmatchDateEditExposed.Left);
			}

			using (var form = new ReceiptFormForTest(receipt))
			{
				receipt.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Empty, AllowBackPosting = true });
				AssertEquals("Precondition: MinUnmatchDate value", false, receipt.UnmatchingData.WasUnmatched);
				form.Show();
				AssertEquals("Precondition: UnmatchDateInfo_ReadOnly", true, receipt.UnmatchDate_ReadOnly);
				AssertEquals("Precondition: No. of Documents should not be visible", false, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Visible);
				AssertEquals("UnmatchDateEditExposed.Visible", false, form.UnmatchDateEditExposed.Visible);
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);

			using (var form = new ReceiptFormForTest(receipt))
			{
				int initialUnmatchDateLeftPosition = form.UnmatchDateEditExposed.Left;
				AssertEquals("Precondition: UnmatchDateEdit should have correct initial Left position", true,
					form.PostDateEditExposed.Left < initialUnmatchDateLeftPosition && initialUnmatchDateLeftPosition < form.AH_NumberOfSupportingDocumentsCalcEditExposed.Left);
				receipt.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Now, AllowBackPosting = true });
				AssertEquals("Precondition: MinUnmatchDate value", true, receipt.UnmatchingData.WasUnmatched);
				form.Show();
				AssertEquals("Precondition: UnmatchDateInfo_ReadOnly", false, receipt.UnmatchDate_ReadOnly);
				AssertEquals("Precondition: No. of Documents should have correct Top position.", form.PostDateEditExposed.Top, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Top);
				AssertEquals("Precondition: Unmatch Date should have correct Top position.", form.PostDateEditExposed.Top, form.UnmatchDateEditExposed.Top);
				AssertEquals("Precondition: No. of Documents should be visible for China", true, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Visible);
				AssertEquals("UnmatchDateEditExposed.Visible", true, form.UnmatchDateEditExposed.Visible);
				AssertEquals("No. of Documents should have correct Right position.", form.ReceiptNoTextBoxExposed.Right, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Right);
				AssertEquals("Unmatch Date should have correct Left position.", initialUnmatchDateLeftPosition, form.UnmatchDateEditExposed.Left);
			}

			using (var form = new ReceiptFormForTest(receipt))
			{
				receipt.UnmatchingData.InitializaReverseTransactionData(new DummyUnmatchingData() { MaxMatchDate = ZDateTime.Empty, AllowBackPosting = true });
				AssertEquals("Precondition: MinUnmatchDate value", false, receipt.UnmatchingData.WasUnmatched);
				form.Show();
				AssertEquals("Precondition: UnmatchDateInfo_ReadOnly", true, receipt.UnmatchDate_ReadOnly);
				AssertEquals("Precondition: No. of Documents should have correct Top position.", form.PostDateEditExposed.Top, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Top);
				AssertEquals("Precondition: Unmatch Date should have correct Top position.", form.PostDateEditExposed.Top, form.UnmatchDateEditExposed.Top);
				AssertEquals("Precondition: No. of Documents should be visible for China", true, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Visible);
				AssertEquals("UnmatchDateEditExposed.Visible", false, form.UnmatchDateEditExposed.Visible);
				AssertEquals("No. of Documents should have correct Left position.", form.ReceiptNoTextBoxExposed.Left, form.AH_NumberOfSupportingDocumentsCalcEditExposed.Left);
			}
		}

		public void TestPostReceiptWithoutMatchButWithMatchedDiscount()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			OrgHeader org = GetNewTestOrg();
			APReceipt receipt = GetNewAPReceipt(org, TestObjectCreator.AUDBankAccount);

			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					receiptForm.Show();
					receiptForm.ReceiptDetailButtonClick();

					Assert("Post with matching flag is true", receipt.IsPostWithMatching);

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					MatchingBase matchingBizO = matchingForm.BusinessEntity as MatchingBase;

					matchingBizO.AddMiscellaneousTransaction(matchingBizO.GetMiscellaneousTransaction(TransactionTypes.Discount));

					AssertNotNull("Matching Object should not be null", matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Matching balance should be 0", 0M, matchingBizO.Balance);

					matchingForm.CloseButton.PerformClick();

					AssertEquals("Post with matching flag is false", false, receipt.IsPostWithMatching);

					receiptForm.PostWithoutMatchingClick();

					AssertNotEquals("MatchedTransactions should not have 2 transactions anymore", 2, matchingBizO.MatchedTransactions.Count);
					AssertNotEquals("Matching balance should not be 0", 0M, matchingBizO.Balance);
				}
				catch (OnSavingCriticalCheckException e)
				{
					Fail("Should not throw a Critical Validation exception but was throwing: " + e.Message);
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
				}
			}

			ReleaseFactory();
			var receiptLoaded = Factory.Load<APReceipt>(receipt.PK);

			ZQuery matchLinkFilter = new ZQuery(AccTransactionMatchLinkSchema.AP_AH, receiptLoaded.PK);
			TransactionMatchLink matchLink = Factory.LoadTop1<TransactionMatchLink>(matchLinkFilter);
			AssertNull("No matching link should exist", matchLink);
			Assert("Matching balance should not be 0", receiptLoaded.MatchingBaseObject.Balance != 0M);

			ZQuery discountFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Discount);
			discountFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, receipt.AH_GC);
			TransactionHeader discounts = Factory.LoadTop1<TransactionHeader>(discountFilter);
			AssertNull("No discounts should be created", discounts);
		}

		public void TestPostReceiptUsesProperValidation()
		{
			APReceipt receipt = Factory.New<APReceipt>();

			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				receiptForm.DisplayMode = ODisplayMode.New;
				receiptForm.Show();

				receiptForm.ReceiptDetailButtonClick();
				Assert("Receipt has errors", receipt.HasErrors);
				AssertEquals("Receipt is not saved", false, receipt.IsInDatabase);

				receiptForm.PostWithoutMatchingClick();
				Assert("Receipt has errors", receipt.HasErrors);
				AssertEquals("Receipt is not saved", false, receipt.IsInDatabase);
			}
		}

		public void TestAccountReadOnlyBeahaviourAfterClickReceiptDetailButtonWhenValidationErrorOccurs()
		{
			APReceipt receipt = Factory.New<APReceipt>();

			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				receiptForm.DisplayMode = ODisplayMode.New;
				receiptForm.Show();

				receiptForm.ReceiptDetailButtonClick();
				Assert("Receipt has errors", receipt.HasErrors);
				Assert("Receipt is not saved", !receipt.IsInDatabase);

				Assert("Account is Editable", !receiptForm.OrganisationGuidFindBoxReadOnly);
			}
		}

		public void TestAccountReadOnlyBeahaviourAfterClickReceiptDetailButtonWhenValidationPassed()
		{
			var receipt = (Receipt)TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				receiptForm.DisplayMode = ODisplayMode.New;
				receiptForm.Show();

				receiptForm.ReceiptDetailButtonClick();
				Assert("Receipt has No errors", !receipt.HasErrors);
				Assert("Account is Read Only", receiptForm.OrganisationGuidFindBoxReadOnly);
			}
		}

		public void TestReceiptFormShowsIncompatibleBranchDepartmentError()
		{
			var aaaBranch = Factory.NewWithValidTestData<GlbBranch>();
			var aaaDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var bbbDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			aaaBranch.GB_GC = Env.CurrentCompany.PK;
			Factory.Save();

			aaaBranch.AllowedDepartments.DeleteAll();
			GlbBranchCombinationValidationTest.SetAllowedBranchDepartmentCombinations(aaaBranch, new GlbDepartment[] { aaaDepartment });

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, aaaBranch.PK.ToGuid(), bbbDepartment.PK.ToGuid()))
			{
				var time = ZDateTime.Now;
				var receipt = TestObjectCreator.CreateARReceipt(1m, 10m, time, time, GetNewTestOrg().PK, TestObjectCreator.AUDBankAccount.PK);
				AssertEquals(aaaBranch.PK, receipt.AH_GB);
				AssertEquals(bbbDepartment.PK, receipt.AH_GE);

				using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
				{
					receiptForm.DisplayMode = ODisplayMode.New;
					receiptForm.Show();

					receiptForm.ReceiptDetailButtonClick();
					AssertIncompatibleBranchDepartment(receipt);

					receiptForm.PostWithoutMatchingClick();
					AssertIncompatibleBranchDepartment(receipt);
				}
			}
		}

		void AssertIncompatibleBranchDepartment(ARReceipt receipt)
		{
			Assert("Receipt has errors", receipt.HasErrors);
			AssertEquals("Receipt is not saved", false, receipt.IsInDatabase);
			AssertHasError(receipt.AH_GEInfo, string.Format(CultureInfo.InvariantCulture, @"The department {0} cannot be used with the branch {1}.
To change this configuration, set up the Branch/Department Combinations in the Edit Branch Window > Departments Tab.", Env.CurrentDepartment.Code, Env.CurrentBranch.Code));
			AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPostWithoutMatchingButtonTextChangeFromNewToSaveWhenAnEdocIsAdded()
		{
			var receipt = (Receipt)TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			using (var receiptForm = new ReceiptFormForTest(receipt))
			{
				receiptForm.Show();
				Application.DoEvents();
				receiptForm.PlugIns.SelectPlugInTabPage(ControllerIDs.eDocsPlugIn);
				AssertEquals("&New", receiptForm.PostWithoutMatchingButtonText);
				var filePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\Sample.PDF");
				string[] filenames = new string[] { filePath };
				using (var eDocsPlugin = (eDocsPlugIn)receiptForm.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn))
				{
					eDocsPlugin.Add(filenames);
				}
				Application.DoEvents();
				AssertEquals("&Save", receiptForm.PostWithoutMatchingButtonText);
			}
		}

		public void TestPostWithoutMatchingButtonTextChangeFromNewToSaveWhenEditWorkflowItems()
		{
			var receipt = (Receipt)TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			using (var receiptForm = new ReceiptFormForTest(receipt))
			{
				receiptForm.ControllerID = ControllerIDs.ZARReceipt;
				receiptForm.Show();
				Application.DoEvents();
				var tabControl = receiptForm.FindAll<ZTemplateTabControl>().Single();
				var workflowProvider = receiptForm.BusinessEntity as IWorkflowProvider;
				tabControl.SelectedIndex = tabControl.TabPages.IndexOfKey("WorkflowTabPage");
				AssertEquals("&New", receiptForm.PostWithoutMatchingButtonText);
				workflowProvider.WorkflowItems.Tasks.AddNew();
				workflowProvider.WorkflowItems.Tasks[0].P9_Description = "Test";
				Application.DoEvents();
				AssertEquals("&Save", receiptForm.PostWithoutMatchingButtonText);
			}
		}

		public void TestPostWithoutMatchingButtonTextWhenDisplayModeIsDelete()
		{
			var receipt = (Receipt)TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();
			var controller = new ZARReceiptController();
			using (var receiptForm = controller.ShowDeleteForm(receipt))
			{
				receiptForm.DisplayMode = ODisplayMode.Delete;
				receiptForm.Show();
				Application.DoEvents();
				var button = (receiptForm as ZForm).Controls.Find("PostWithoutMatchingButton", true)[0] as ZPostOrCancelButton;
				AssertNotNull(button);
				AssertEquals("&Reverse", button.Text);
			}
		}

		public void TestWorkflowTabPage()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var tabControl = form.FindAll<ZTemplateTabControl>().Single();
				AssertNotEquals("WorkflowTabPage Visible", -1, tabControl.TabPages.IndexOfKey("WorkflowTabPage"));
			}
		}

		public void TestReceiptIsNotTopLevelBusinessObjectWhenMatchGroupFormIsOpened()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();
			var receipt = (Receipt)TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cash, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 10m, TestObjectCreator.AUDBankAccount.PK);
			receipt.AH_OH = GetNewTestOrg().PK;

			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				receiptForm.Show();
				Assert("Receipt must be the top level business object for Receipt form", receipt.IsTopLevel);
				receiptForm.ReceiptDetailButtonClick();

				var matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
				AssertNotNull(matchingForm);
				var matchingBase = matchingForm.BusinessEntity as MatchingBase;
				Assert("Matching Base must be the top level business object for Match Group form", matchingBase.IsTopLevel);
				Assert("Receipt must not be the top level business object for Match Group form", !receipt.IsTopLevel);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				matchingForm.CloseButton.PerformClick();

				Assert(matchingForm.IsDisposed);
				Assert("Receipt must be the top level business object for Receipt form", receipt.IsTopLevel);
			}
		}

		#region TestDeleteBalancingJournals_PostWithoutMatching

		public void TestDeleteBalancingJournals_PostWithoutMatching()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();
			var org = GetNewTestOrg();

			AssertDeleteAPorAP_BalancingJournals(LedgerTypes.AccountsPayable, org.PK);
			AssertDeleteAPorAP_BalancingJournals(LedgerTypes.AccountsReceivable, org.PK);
		}

		void AssertDeleteAPorAP_BalancingJournals(string ledgerTypes, ZGuid orgPk)
		{
			Receipt receipt = null;
			IActiveBusinessObjectCollection balancingJournals = null;
			var time = ZDateTime.Now;

			if (ledgerTypes == LedgerTypes.AccountsPayable)
			{
				receipt = TestObjectCreator.CreateAPReceipt(1m, 10m, time, time, orgPk, TestObjectCreator.AUDBankAccount.PK);
				balancingJournals = receipt.MatchingBaseObject.BalancingAPJournals;
			}
			else
			{
				receipt = TestObjectCreator.CreateARReceipt(1m, 10m, time, time, orgPk, TestObjectCreator.AUDBankAccount.PK);
				balancingJournals = receipt.MatchingBaseObject.BalancingARJournals;
			}

			var journal = (Business.ARAP.Journal.Journal)balancingJournals.AddNew();
			journal.AH_AG = ZGuid.Empty;

			AssertEquals("There is one Journal in the collection", 1, balancingJournals.Count);
			AssertEquals("It is the invalid Journal we created", journal, balancingJournals[0]);

			using (ReceiptFormForTest receiptForm = new ReceiptFormForTest(receipt))
			{
				receiptForm.Show();
				Application.DoEvents();
				receiptForm.DisplayMode = ODisplayMode.Edit;
				receiptForm.PostWithoutMatchingClick();

				AssertEquals("BalancingJournals were clear by PostWithoutMatching", 0, balancingJournals.Count);
				Assert("Journal was deleted", journal.IsDeleted);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		#endregion

		public void TestUseReceiptValidation_HasErrorWhenMatchClose()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var testOrg = GetNewTestOrg();

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			arInvoice.AH_OH = testOrg.PK;
			arInvoice.AH_FullyPaidDate = ZDateTime.Empty;

			Factory.Save();

			var receipt = GetNewAPReceipt(testOrg, TestObjectCreator.AUDBankAccount);

			using (var receiptForm = new ReceiptFormForTest(receipt))
			{
				NewMatchGroupForm matchingForm = null;

				try
				{
					receiptForm.Show();
					receiptForm.ReceiptDetailButtonClick();

					((IMatching)receipt).OSPartialPaymentAmount = -40M;

					matchingForm = ZFormModaliser.ActiveForm as NewMatchGroupForm;
					AssertNotNull("The active form should be the matching form", matchingForm);
					MatchingBase matchingBizO = matchingForm.BusinessEntity as MatchingBase;

					matchingBizO.MoveAllFromUnmatchToMatch();

					AssertNotNull(matchingBizO);
					AssertEquals("Should be 2 transactions in MatchedTransactions", 2, matchingBizO.MatchedTransactions.Count);

					var overpayment = new MiscellaneousTransactionCreatorAR(Factory).CreateOverpayment(30M, 1M, testOrg);
					matchingBizO.AddMiscellaneousTransaction(overpayment);
					AssertEquals("Should be 3 transactions in MatchedTransactions", 3, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					matchingForm.CloseButton.PerformClick();

					AssertEquals("UseReceiptValidation is false", false, receipt.UseReceiptValidation);

					((IMatching)receipt).OSPartialPaymentAmount = -34M;

					var discount = new MiscellaneousTransactionCreatorAR(Factory).CreateDiscount(-6M, testOrg);
					matchingBizO.AddMiscellaneousTransaction(discount);
					AssertEquals("Should be 4 transactions in MatchedTransactions", 4, matchingBizO.MatchedTransactions.Count);
					AssertEquals("Balance should be 0", 0M, matchingBizO.Balance);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					matchingForm.CloseButton.PerformClick();

					AssertEquals("UseReceiptValidation is true", true, receipt.UseReceiptValidation);
				}
				finally
				{
					if (matchingForm != null)
					{
						matchingForm.Dispose();
					}
				}
			}
		}

		public void TestPostDateEditCaption()
		{
			using (ReceiptForm form = new ReceiptForm(Factory.NewWithValidTestData<ARReceipt>()))
			{
				var postDateEdit = (ZDateEdit)form.Controls.Find("PostDateEdit", true).FirstOrDefault();
				AssertEquals("Expecting the caption of PostDateEdit Control is 'Post Date'", "Post Date", postDateEdit.CaptionResourceString.Caption);
			}
		}

		#region Implementation

		public class ReceiptFormForTest : ReceiptForm
		{
			public ReceiptFormForTest(Receipt receiptBizO)
				: base(receiptBizO)
			{ }

			public void ReceiptDetailButtonClick()
			{
				ReceiptDetailButton.PerformClick();
			}

			public void PostWithoutMatchingClick()
			{
				PostWithoutMatchingButton.PerformClick();
			}

			public ZDateEdit UnmatchDateEditExposed
			{
				get { return UnmatchDateEdit; }
			}

			public ZDateEdit PostDateEditExposed
			{
				get { return PostDateEdit; }
			}

			public ZTextBox ReceiptNoTextBoxExposed
			{
				get { return ReceiptNoTextBox; }
			}

			public ZCalcEdit AH_NumberOfSupportingDocumentsCalcEditExposed
			{
				get { return AH_NumberOfSupportingDocumentsCalcEdit; }
			}

			public ZString PostWithoutMatchingButtonText
			{
				get { return PostWithoutMatchingButton.Text; }
			}

			public bool ReceiptDetailButtonEnabled
			{
				get { return ReceiptDetailButton.Enabled; }
			}

			public bool PostWithoutMatchingButtonEnabled
			{
				get { return PostWithoutMatchingButton.Enabled; }
			}

			public bool OrganisationGuidFindBoxReadOnly
			{
				get { return OrganisationGuidFindBox.ReadOnly; }
			}
		}

		protected override Form GetFormToBashCore()
		{
			ARReceipt testReceipt = Factory.New<ARReceipt>();

			var result = new ReceiptForm(testReceipt);
			result.ControllerID = ControllerIDs.ZARReceipt;
			return result;
		}

		protected override bool ShouldHaveAuditPlugIn => true;

		OrgHeader GetNewTestOrg()
		{
			var newOrg = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			newOrg.OH_IsDebtor = true;
			newOrg.OH_IsCreditor = true;
			newOrg.CompanyData.SetAPTaxApplicable(false);
			newOrg.Factory.Save();

			return Factory.Load<OrgHeader>(newOrg.PK);
		}

		APReceipt GetNewAPReceipt(OrgHeader testOrg, AccBankAccount testBank)
		{
			APReceipt aPRec = Factory.NewWithValidTestData<APReceipt>();
			aPRec.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aPRec.AH_OSExTaxAmount = 34M;
			aPRec.AH_OH = testOrg.PK;
			aPRec.AH_AB = testBank.PK;
			aPRec.AH_ChequeOrReference = "34";
			aPRec.AH_ChequeDrawer = "SDDSF";
			aPRec.AH_DrawerBank = "FGGF";
			aPRec.AH_DrawerBranch = "YYU";
			return aPRec;
		}

		ARReceipt GetNewARReceipt(OrgHeader testOrg, AccBankAccount testBank)
		{
			ARReceipt aPRec = Factory.NewWithValidTestData<ARReceipt>();
			aPRec.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aPRec.AH_OSExTaxAmount = 34M;
			aPRec.AH_OH = testOrg.PK;
			aPRec.AH_AB = testBank.PK;
			aPRec.AH_ChequeOrReference = "34";
			aPRec.AH_ChequeDrawer = "SDDSF";
			aPRec.AH_DrawerBank = "FGGF";
			aPRec.AH_DrawerBranch = "YYU";
			return aPRec;
		}

		#endregion
	}
}
