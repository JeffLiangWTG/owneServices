using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public abstract class InvoicingPostManagerGUIWrapper_PostingTransactionApprovalTestTest : PostManagerGUIWrapper_PostingTransactionApprovalTest
	{
		protected override PostManagerGUIWrapper GetNewGUIWrapperCore(params Job[] jobs)
		{
			return new InvoicingPostManagerGUIWrapper(JobInvoicingPostingOption.All, Factory, jobs[0], null);
		}

		[TestDate(2015, 5, 10)]
		public virtual void TestApprovalRequestNotCreatedWithBackdatingAndAmountChanged_AR()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			SetUpARAPInvoicePostingExchangeRateOptionAndARAPBackdating(isAP: false);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupSecurity(true, isARCreditNoteTestingForced: true);
			SetupSecurity(true, isARCreditNoteTestingForced: false);
			var job = SetupJobData();
			var chargeSell = SetupChargeData(job, 300m, TestObjectCreator.CC3, isARCreditNoteTestingForced: true);

			chargeSell.JR_RX_NKSellCurrency = "USD";
			chargeSell.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			chargeSell.JR_LocalCostAmt = 0m;
			chargeSell.JR_LocalSellAmt = -300m; //auth. needed
			chargeSell.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			TestObjectCreator.CreateUSDBuyRate(0.5m, new DateTime(2015, 5, 10));
			TestObjectCreator.CreateUSDBuyRate(1.5m, new DateTime(2015, 4, 30)); //backdating rate

			AssertEquals("USD", chargeSell.JR_RX_NKSellCurrency);
			AssertEquals(0.5m, chargeSell.JR_OSSellExRate);
			AssertEquals(-300m, chargeSell.JR_LocalSellAmt);
			AssertEquals(-150m, chargeSell.JR_OSSellAmt);
			AssertEquals(0m, chargeSell.JR_LocalCostAmt);
			AssertEquals(0m, chargeSell.JR_OSCostAmt);

			var arCreditNotes = Factory.Load<ARCreditNote>(new ZQuery());
			AssertEquals(0, arCreditNotes.Length);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				DialogResult result = DialogResult.Yes;
				var formType = form.GetType();

				if (formType == typeof(LoginFormWithRequest))
				{
					result = DialogResult.Ignore;
				}
				else if (formType == typeof(ChangeTransactionDatesMessageBox))
				{
					result = DialogResult.Yes;
				}
				else if (formType == typeof(ARCreditNoteApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else if (formType == typeof(APInvoiceChargesApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else
				{
					throw new Exception("unexpected Dialog : " + formType.ToString());
				}

				ZFormModaliser.ResultToReturnFromShowDialog = result;
			});
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();

			AssertEquals(true, chargeSell.IsRevenuePosted && !chargeSell.IsCostPosted);
			arCreditNotes = Factory.Load<ARCreditNote>(new ZQuery());
			AssertEquals(1, arCreditNotes.Length);

			var arApprovalCollection = LoadAllApprovalRequests(true);
			AssertEquals("AR Approval request.", 0, arApprovalCollection.Length);

			var apApprovalCollection = LoadAllApprovalRequests(false);
			AssertEquals("AP Approval request.", 0, apApprovalCollection.Length);

			//posted in backdating's new rate.
			AssertEquals("USD", chargeSell.JR_RX_NKSellCurrency);
			AssertEquals(1.5m, chargeSell.JR_OSSellExRate);
			AssertEquals(-100m, chargeSell.JR_LocalSellAmt); //-150 / 1.5, auth. not needed.
			AssertEquals(-150m, chargeSell.JR_OSSellAmt);
			AssertEquals(0m, chargeSell.JR_LocalCostAmt);
			AssertEquals(0m, chargeSell.JR_OSCostAmt);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 10)]
		[DisableZeroExchangeRateOverriding]
		public virtual void TestApprovalRequestNotCreatedWithBackdatingAndAmountChanged_AP()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			SetUpARAPInvoicePostingExchangeRateOptionAndARAPBackdating(isAP: true);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupSecurity(true, isARCreditNoteTestingForced: true);
			SetupSecurity(true, isARCreditNoteTestingForced: false);
			var job = SetupJobData();
			var chargeCost = SetupChargeData(job, 310m, TestObjectCreator.CC3, isARCreditNoteTestingForced: false);

			chargeCost.JR_RX_NKCostCurrency = "USD";
			chargeCost.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			chargeCost.JR_LocalSellAmt = 0m;
			chargeCost.JR_LocalCostAmt = 310m; //auth. needed
			chargeCost.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			TestObjectCreator.CreateUSDBuyRate(0.5m, new DateTime(2015, 5, 10));
			TestObjectCreator.CreateUSDBuyRate(1.5m, new DateTime(2015, 4, 30)); //backdating rate

			AssertEquals("USD", chargeCost.JR_RX_NKCostCurrency);
			AssertEquals(0.5m, chargeCost.JR_OSCostExRate);
			AssertEquals(310m, chargeCost.JR_LocalCostAmt);
			AssertEquals(155m, chargeCost.JR_OSCostAmt);
			AssertEquals(0m, chargeCost.JR_LocalSellAmt);
			AssertEquals(0m, chargeCost.JR_OSSellAmt);

			var apInvoices = Factory.Load<APInvoice>(new ZQuery());
			AssertEquals(0, apInvoices.Length);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				DialogResult result = DialogResult.Yes;
				var formType = form.GetType();

				if (formType == typeof(LoginFormWithRequest))
				{
					result = DialogResult.Ignore;
				}
				else if (formType == typeof(ChangeTransactionDatesMessageBox))
				{
					result = DialogResult.Yes;
				}
				else if (formType == typeof(ARCreditNoteApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else if (formType == typeof(APInvoiceChargesApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else
				{
					throw new Exception("unexpected Dialog : " + formType.ToString());
				}

				ZFormModaliser.ResultToReturnFromShowDialog = result;
			});
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();

			AssertEquals(true, !chargeCost.IsRevenuePosted && chargeCost.IsCostPosted);
			apInvoices = Factory.Load<APInvoice>(new ZQuery());
			AssertEquals(1, apInvoices.Length);

			var arApprovalCollection = LoadAllApprovalRequests(true);
			AssertEquals("AR Approval request.", 0, arApprovalCollection.Length);

			var apApprovalCollection = LoadAllApprovalRequests(false);
			AssertEquals("AP Approval request.", 0, apApprovalCollection.Length);

			//posted in backdating's new rate.
			AssertEquals("USD", chargeCost.JR_RX_NKCostCurrency);
			AssertEquals(1.5m, chargeCost.JR_OSCostExRate);
			AssertEquals(103.33m, chargeCost.JR_LocalCostAmt); //155 / 1.5, auth. not needed.
			AssertEquals(155m, chargeCost.JR_OSCostAmt);
			AssertEquals(0m, chargeCost.JR_LocalSellAmt);
			AssertEquals(0m, chargeCost.JR_OSSellAmt);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 10)]
		[DisableZeroExchangeRateOverriding]
		public virtual void TestApprovalRequestCreatedWithBackdatingAndAmountChanged_AR()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			SetUpARAPInvoicePostingExchangeRateOptionAndARAPBackdating(isAP: false);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupSecurity(true, isARCreditNoteTestingForced: true);
			SetupSecurity(true, isARCreditNoteTestingForced: false);
			var job = SetupJobData();
			var chargeSell = SetupChargeData(job, 300m, TestObjectCreator.CC3, isARCreditNoteTestingForced: true);

			chargeSell.JR_RX_NKSellCurrency = "USD";
			chargeSell.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			chargeSell.JR_LocalCostAmt = 0m;
			chargeSell.JR_LocalSellAmt = -100m; //auth. not needed
			chargeSell.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			TestObjectCreator.CreateUSDBuyRate(0.5m, new DateTime(2015, 5, 10));
			TestObjectCreator.CreateUSDBuyRate(0.1m, new DateTime(2015, 4, 30)); //backdating rate

			AssertEquals("USD", chargeSell.JR_RX_NKSellCurrency);
			AssertEquals(0.5m, chargeSell.JR_OSSellExRate);
			AssertEquals(-100m, chargeSell.JR_LocalSellAmt);
			AssertEquals(-50m, chargeSell.JR_OSSellAmt);
			AssertEquals(0m, chargeSell.JR_LocalCostAmt);
			AssertEquals(0m, chargeSell.JR_OSCostAmt);

			var arCreditNotes = Factory.Load<ARCreditNote>(new ZQuery());
			AssertEquals(0, arCreditNotes.Length);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				DialogResult result = DialogResult.Yes;
				var formType = form.GetType();

				if (formType == typeof(LoginFormWithRequest))
				{
					result = DialogResult.Ignore;
				}
				else if (formType == typeof(ChangeTransactionDatesMessageBox))
				{
					result = DialogResult.Yes;
				}
				else if (formType == typeof(ARCreditNoteApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else if (formType == typeof(APInvoiceChargesApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else
				{
					throw new Exception("unexpected Dialog : " + formType.ToString());
				}

				ZFormModaliser.ResultToReturnFromShowDialog = result;
			});
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();

			AssertEquals(true, !chargeSell.IsRevenuePosted && !chargeSell.IsCostPosted);
			arCreditNotes = Factory.Load<ARCreditNote>(new ZQuery());
			AssertEquals(0, arCreditNotes.Length);

			var arApprovalCollection = LoadAllApprovalRequests(true);
			AssertEquals("AR Approval request.", 1, arApprovalCollection.Length);

			var apApprovalCollection = LoadAllApprovalRequests(false);
			AssertEquals("AP Approval request.", 0, apApprovalCollection.Length);

			ARCreditNoteApprovalRequest arApprovalRequest = arApprovalCollection[0] as ARCreditNoteApprovalRequest;
			AssertEquals("ZLOCCLT AUD FIN: -500.00", arApprovalRequest.FormattedChargeDetails); //the amount changed by backdating's rate

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 10)]
		[DisableZeroExchangeRateOverriding]
		public virtual void TestApprovalRequestCreatedWithBackdatingAndAmountChanged_AP()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			SetUpARAPInvoicePostingExchangeRateOptionAndARAPBackdating(isAP: true);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupSecurity(true, isARCreditNoteTestingForced: true);
			SetupSecurity(true, isARCreditNoteTestingForced: false);
			var job = SetupJobData();
			var chargeCost = SetupChargeData(job, 150m, TestObjectCreator.CC3, isARCreditNoteTestingForced: false);

			chargeCost.JR_RX_NKCostCurrency = "USD";
			chargeCost.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			chargeCost.JR_LocalSellAmt = 0m;
			chargeCost.JR_LocalCostAmt = 100m; //auth. not needed.
			chargeCost.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			TestObjectCreator.CreateUSDBuyRate(0.5m, new DateTime(2015, 5, 10));
			TestObjectCreator.CreateUSDBuyRate(0.1m, new DateTime(2015, 4, 30)); //backdating rate

			AssertEquals("USD", chargeCost.JR_RX_NKCostCurrency);
			AssertEquals(0.5m, chargeCost.JR_OSCostExRate);
			AssertEquals(100m, chargeCost.JR_LocalCostAmt);
			AssertEquals(50m, chargeCost.JR_OSCostAmt);
			AssertEquals(0m, chargeCost.JR_LocalSellAmt);
			AssertEquals(0m, chargeCost.JR_OSSellAmt);

			var apInvoices = Factory.Load<APInvoice>(new ZQuery());
			AssertEquals(0, apInvoices.Length);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				DialogResult result = DialogResult.Yes;
				var formType = form.GetType();

				if (formType == typeof(LoginFormWithRequest))
				{
					result = DialogResult.Ignore;
				}
				else if (formType == typeof(ChangeTransactionDatesMessageBox))
				{
					result = DialogResult.Yes;
				}
				else if (formType == typeof(ARCreditNoteApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else if (formType == typeof(APInvoiceChargesApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else if (formType == typeof(APInvoicePostingWithApprovalRequestSummaryForm))
				{
					result = DialogResult.OK;
				}
				else
				{
					throw new Exception("unexpected Dialog : " + formType.ToString());
				}

				ZFormModaliser.ResultToReturnFromShowDialog = result;
			});
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();

			AssertEquals(true, !chargeCost.IsRevenuePosted && !chargeCost.IsCostPosted);
			apInvoices = Factory.Load<APInvoice>(new ZQuery());
			AssertEquals(0, apInvoices.Length);

			var arApprovalCollection = LoadAllApprovalRequests(true);
			AssertEquals("AR Approval request.", 0, arApprovalCollection.Length);

			var apApprovalCollection = LoadAllApprovalRequests(false);
			AssertEquals("AP Approval request.", 1, apApprovalCollection.Length);

			APInvoiceChargesApprovalRequest apApprovalRequest = apApprovalCollection[0] as APInvoiceChargesApprovalRequest;
			AssertEquals("ZZCC3 500.00", apApprovalRequest.FormattedChargeDetails); //the amount changed by backdating's rate

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 10)]
		[DisableZeroExchangeRateOverriding]
		public virtual void TestApprovalRequestNotCreatedWithBackdatingErrorMessage_AP()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			SetUpARAPInvoicePostingExchangeRateOptionAndARAPBackdating(isAP: true);

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupSecurity(true, isARCreditNoteTestingForced: true);
			SetupSecurity(true, isARCreditNoteTestingForced: false);
			var job = SetupJobData();
			var chargeCost = SetupChargeData(job, 150m, TestObjectCreator.CC3, isARCreditNoteTestingForced: false);

			chargeCost.JR_RX_NKCostCurrency = "USD";
			chargeCost.CostExchangeRate.SetBuyRate_ForTestOnly(0.5m);
			chargeCost.JR_LocalSellAmt = 0m;
			chargeCost.JR_LocalCostAmt = 100m; //auth. not needed.
			chargeCost.RevenueExchangeRate.SetBuyRate_ForTestOnly(0.5m);

			Factory.Save();

			TestObjectCreator.CreateUSDBuyRate(0.5m, new DateTime(2015, 5, 10));
			//TestObjectCreator.CreateUSDBuyRate(0.1m, new DateTime(2015, 4, 30)); //backdating rate is not available

			AssertEquals("USD", chargeCost.JR_RX_NKCostCurrency);
			AssertEquals(0.5m, chargeCost.JR_OSCostExRate);
			AssertEquals(100m, chargeCost.JR_LocalCostAmt);
			AssertEquals(50m, chargeCost.JR_OSCostAmt);
			AssertEquals(0m, chargeCost.JR_LocalSellAmt);
			AssertEquals(0m, chargeCost.JR_OSSellAmt);

			var apInvoices = Factory.Load<APInvoice>(new ZQuery());
			AssertEquals(0, apInvoices.Length);

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				DialogResult result = DialogResult.Yes;
				var formType = form.GetType();

				if (formType == typeof(LoginFormWithRequest))
				{
					result = DialogResult.Ignore;
				}
				else if (formType == typeof(ChangeTransactionDatesMessageBox))
				{
					result = DialogResult.Yes;
				}
				else if (formType == typeof(ARCreditNoteApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else if (formType == typeof(APInvoiceChargesApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else if (formType == typeof(APInvoicePostingWithApprovalRequestSummaryForm))
				{
					result = DialogResult.OK;
				}
				else
				{
					throw new Exception("unexpected Dialog : " + formType.ToString());
				}

				ZFormModaliser.ResultToReturnFromShowDialog = result;
			});
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();

			AssertEquals(true, !chargeCost.IsRevenuePosted && !chargeCost.IsCostPosted);
			apInvoices = Factory.Load<APInvoice>(new ZQuery());
			AssertEquals(0, apInvoices.Length);

			var arApprovalCollection = LoadAllApprovalRequests(true);
			AssertEquals("AR Approval request.", 0, arApprovalCollection.Length);

			var apApprovalCollection = LoadAllApprovalRequests(false);
			AssertEquals("AP Approval request.", 0, apApprovalCollection.Length);

			var errorMsg = UnitTestUserNotification.Instance.LastMessage.Text;
			AssertEquals(@"AP Invoice number S001_123
The ""AP Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 30-Apr-15. Please check your data and try again.", errorMsg);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		void SetUpARAPInvoicePostingExchangeRateOptionAndARAPBackdating(bool isAP)
		{
			(isAP ? PostingExRateRegistryAP : PostingExRateRegistryAR).SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");

			//AP backdating
			var regBackDateAPConfig = new BackDateAPInvoicesConfiguration();
			var regBackDateAPConfig1 = regBackDateAPConfig.PostDateConfigurationCollection[0];
			regBackDateAPConfig1.JobType = "ALL";
			regBackDateAPConfig1.DirectionCode = "";
			regBackDateAPConfig1.Mode = "";
			regBackDateAPConfig1.BrokerCode = "";
			regBackDateAPConfig1.SignificantDateCode = "ADD";
			regBackDateAPConfig1.PriorClosedPeriod = "";
			regBackDateAPConfig1.PriorOpenPeriod = "";
			regBackDateAPConfig1.CurrentPeriod = "EPM";
			regBackDateAPConfig1.FuturePeriod = "";
			regBackDateAPConfig1.ReversalRule = "STD";
			AccountingConfigurationRegistry.Instance.BackDateAPInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, regBackDateAPConfig);

			//AR backdating
			var config = new BackDateInvoicesConfiguration();
			config.InvoiceDateConfigurationCollection[0].CurrentPeriod = InvoiceDateConfigurationLookups.SignificantDatePeriodCodes.EndOfPriorMonth;
			config.InvoiceDateConfigurationCollection[0].Today = true;
			config.DefaultPostDateFromInvoiceDate = true;
			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				DialogResult result = DialogResult.Yes;
				var formType = form.GetType();

				if (formType == typeof(LoginFormWithRequest))
				{
					result = DialogResult.Ignore;
				}
				else if (formType == typeof(ChangeTransactionDatesMessageBox))
				{
					result = DialogResult.Yes;
				}
				else if (formType == typeof(ARCreditNoteApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else if (formType == typeof(APInvoiceChargesApprovalBulkForm))
				{
					result = DialogResult.OK;
				}
				else
				{
					throw new Exception("unexpected Dialog : " + formType.ToString());
				}

				ZFormModaliser.ResultToReturnFromShowDialog = result;
			});
		}
	}

	[TestedType(typeof(InvoicingPostManagerGUIWrapper))]
	public class ARCreditNoteInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTestTest : InvoicingPostManagerGUIWrapper_PostingTransactionApprovalTestTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return true; }
		}
	}

	[TestedType(typeof(InvoicingPostManagerGUIWrapper))]
	public class APInvoiceChargesInvoicingPostManagerGUIWrapper_PostingTransactionApprovalTestTest : InvoicingPostManagerGUIWrapper_PostingTransactionApprovalTestTest
	{
		protected override bool IsARCreditNoteTesting
		{
			get { return false; }
		}
	}
}
