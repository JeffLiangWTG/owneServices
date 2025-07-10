using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(CASSExportBillingForm))]
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	sealed class CASSExportBillingFormBasherTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestDeleteMenuItemEnabled()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				Assert(testForm.DiscrepancyReportGrid_ForTestOnly.AllowReadOnlyRowsToBeDeleted);
				Assert(testForm.ExportLinesGrid_ForTestOnly.AllowReadOnlyRowsToBeDeleted);
				Assert(testForm.ImportLinesGrid_ForTestOnly.AllowReadOnlyRowsToBeDeleted);
				Assert(testForm.APTransactionsGrid_ForTestOnly.AllowReadOnlyRowsToBeDeleted);
			}
		}

		public void TestShowError()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				AssertNotNull(testForm);
				testForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.ShowError("To be or not to be?", "Gamlet");
				AssertEquals("To be or not to be?", UnitTestUserNotification.Instance.LastMessage.Text);

				testForm.CASSBusinessEntity_ForTestOnly.APTransactions.Add(Factory.NewWithValidTestData<APInvoice>());
				AssertNull(testForm.CASSBusinessEntity_ForTestOnly.APTransactions[0].ShowError);
				testForm.PreviewButton_ForTestOnly.PerformClick();
				AssertNotNull(testForm.CASSBusinessEntity_ForTestOnly.APTransactions[0].ShowError);
				testForm.Close();
				AssertNull(testForm.CASSBusinessEntity_ForTestOnly.APTransactions[0].ShowError);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoNotAutomaticallyCloseClaimCheckBoxShouldBeVisible()
		{
			AssertEquals(true, PrepareDoNotAutomaticallyCloseClaimCheckBoxBindPropertyValue(CASSAutoCreateClaim.OverBilled));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoNotAutomaticallyCloseClaimCheckBoxShouldBeInVisible()
		{
			AssertEquals(false, PrepareDoNotAutomaticallyCloseClaimCheckBoxBindPropertyValue(CASSAutoCreateClaim.NotCreated));
		}

		bool PrepareDoNotAutomaticallyCloseClaimCheckBoxBindPropertyValue(string accountingConfigurationRegistryCASSAutoCreateClaim)
		{
			var claim = TestObjectCreator.CreateAPClaim(100M);
			var airline = TestObjectCreator.CreateAirLine("172");
			TestObjectCreator.ABIGAS.OH_IsCreditor = true;
			TestObjectCreator.SetupCreditorAirline(airline, TestObjectCreator.ABIGAS);
			Factory.Save();

			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, accountingConfigurationRegistryCASSAutoCreateClaim);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = TestFileName_ClaimExisting;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				testForm.CASSBusinessEntity_ForTestOnly.ForceRecalculateData(); //refresh
				testForm.Show();
				testForm.ImportButton_ForTestOnly.PerformClick();
				return testForm.AutoCloseClaimCheckBox_ForTestOnly.Visible;
			}
		}

		public void TestIsPostedError()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Show();
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);

				CASSBillingLine line = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				TestObjectCreator.GLHeader1.AG_Description = "Test Account";
				AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
				TestObjectCreator.SetupCASSBillingLine(line);
				TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(line.AirlinePrefix), TestObjectCreator.AALSHI);
				SetupAssociatedBizos();
				var costline = new CASSCostExportLine(Factory, CASSCostLineType.Default);
				costline.AirlinePrefix = line.AirlinePrefix;
				line.AddCostLine(costline, "AUD");
				Factory.Save();

				testForm.CASSBusinessEntity_ForTestOnly.ForceRecalculateData();  //refresh
				testForm.PreviewButton_ForTestOnly.PerformClick(); //calculate AP invoices
				testForm.APTransactionsGrid_ForTestOnly.SelectAllElements();
				MenuItem viewMenuItem = testForm.APTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("&View");

				AssertEquals(1, testForm.CASSBusinessEntity_ForTestOnly.APTransactions.Count);
				testForm.CASSBusinessEntity_ForTestOnly.APTransactions[0].IsPostedToCASSOrSaved = true;

				viewMenuItem.PerformClick();
				AssertEquals("The posted invoice can't be found. Is Posted state was reset.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsPostedToCASS should be reset to false", false, testForm.CASSBusinessEntity_ForTestOnly.APTransactions[0].IsPostedToCASSOrSaved);
			}
		}

		public void TestValidationOnRefresh()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testForm.Show();
				CASSBillingLine line = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				TestObjectCreator.GLHeader1.AG_Description = "Test Account";
				AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
				TestObjectCreator.SetupCASSBillingLine(line);
				TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(line.AirlinePrefix), TestObjectCreator.AALSHI);
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				Factory.Save();

				testForm.RefreshButton_ForTestOnly.PerformClick();
				Assert("Precondition: there are no errors must be in CASS bizo.", !testForm.CASSBusinessEntity_ForTestOnly.HasErrors);
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
				line.AddCostLine(new CASSCostExportLine(Factory, CASSCostLineType.Default), "");
				testForm.RefreshButton_ForTestOnly.PerformClick();
				AssertNoErrors("the non-persistant bizO shouldn't have the line errors", testForm.CASSBusinessEntity_ForTestOnly);
			}
		}

		public void TestAPInvoicePosting_CalculateInvoicesDuringPosting()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
				testForm.Show();
				var line = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				TestObjectCreator.GLHeader1.AG_Description = "Test Account";
				AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
				TestObjectCreator.SetupCASSBillingLine(line);
				TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(line.AirlinePrefix), TestObjectCreator.AALSHI);
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				Factory.Save();

				var logger = new ZStringBuilder();
				EventHandler<CASSBilling.LongTimeEventArgs> progressLogger = (sender, e) =>
				{
					AssertType("ActiveForm in progress", typeof(ProgressForm), ZFormModaliser.ActiveForm);
					var progressForm = (ProgressForm)ZFormModaliser.ActiveForm;
					Assert("ShowCancelButton", !progressForm.ShowCancelButton);
					logger.AppendLine($"{progressForm.Status}. PercentComplete: {progressForm.PercentComplete}");
				};
				testForm.CASSBusinessEntity_ForTestOnly.OnStartLongTimeProcess += progressLogger;
				testForm.CASSBusinessEntity_ForTestOnly.OnProgressLongTimeProcess += progressLogger;
				testForm.CASSBusinessEntity_ForTestOnly.OnEndLongTimeProcess += (sender, e) =>
				{
					AssertNull("ActiveForm at the end.", ZFormModaliser.ActiveForm);
					AssertType("LastFormShownForTest at the end.", typeof(ProgressForm), ZFormModaliser.LastFormShownForTest);
					Assert("ProgressForm is disposed.", ZFormModaliser.LastFormShownForTest.IsDisposed);
				};

				testForm.CASSBusinessEntity_ForTestOnly.ForceRecalculateData();
				testForm.FireSaveButton();

				var expextedInvoices = Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.AALSHI.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("One APInvoice should be posted", 1, expextedInvoices.Length);
				AssertEquals("Lines.Count", 2, expextedInvoices[0].Lines.Count);
				AssertEquals("AH_OSTotalAmount", line.CASSCostValue + line.CASSCostAdjustedValue + line.CASSCostTaxValue + line.CASSCostTaxAdjustedValue, expextedInvoices[0].AH_OSTotalAmount);

				var timeToCompleteTemplate = new Regex("00:00:0[0-9]");
				AssertContains("Progress form log. Note, the first status is not set in ProgressForm because due to its current implementation it sets status after 250 ms passed.",
@". PercentComplete: 0
Posting of AP Invoices 1 of 1
time elapsed: 01:43:34. PercentComplete: 100
",
							timeToCompleteTemplate.Replace(logger.ToString(), "01:43:34"));
			}
		}

		public void TestAPInvoicePosting_CalcualteInvoicesBeforePosting()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
				testForm.Show();
				var line = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				TestObjectCreator.GLHeader1.AG_Description = "Test Account";
				AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
				TestObjectCreator.SetupCASSBillingLine(line);
				TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(line.AirlinePrefix), TestObjectCreator.AALSHI);
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				Factory.Save();

				var logger = new ZStringBuilder();
				EventHandler<CASSBilling.LongTimeEventArgs> progressLogger = (sender, e) =>
				{
					AssertType("ActiveForm in progress", typeof(ProgressForm), ZFormModaliser.ActiveForm);
					var progressForm = (ProgressForm)ZFormModaliser.ActiveForm;
					Assert("ShowCancelButton", !progressForm.ShowCancelButton);
					logger.AppendLine($"{progressForm.Status}. PercentComplete: {progressForm.PercentComplete}");
				};
				testForm.CASSBusinessEntity_ForTestOnly.OnStartLongTimeProcess += progressLogger;
				testForm.CASSBusinessEntity_ForTestOnly.OnProgressLongTimeProcess += progressLogger;
				testForm.CASSBusinessEntity_ForTestOnly.OnEndLongTimeProcess += (sender, e) =>
				{
					AssertNull("ActiveForm at the end.", ZFormModaliser.ActiveForm);
					AssertType("LastFormShownForTest at the end.", typeof(ProgressForm), ZFormModaliser.LastFormShownForTest);
					Assert("ProgressForm is disposed.", ZFormModaliser.LastFormShownForTest.IsDisposed);
				};

				testForm.CASSBusinessEntity_ForTestOnly.ForceRecalculateData();
				var calculateInvoicesButton = testForm.GetControl<ZButton>("PreviewButton", true);
				calculateInvoicesButton.PerformClick();
				AssertEquals("Precondition: an invoice should be calculated before posting", 1, testForm.CASSBusinessEntity_ForTestOnly.APTransactions.Count);

				testForm.FireSaveButton();

				var expextedInvoices = Factory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.AALSHI.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
				AssertEquals("One APInvoice should be posted", 1, expextedInvoices.Length);
				AssertEquals("Lines.Count", 2, expextedInvoices[0].Lines.Count);
				AssertEquals("AH_OSTotalAmount", line.CASSCostValue + line.CASSCostAdjustedValue + line.CASSCostTaxValue + line.CASSCostTaxAdjustedValue, expextedInvoices[0].AH_OSTotalAmount);

				var timeToCompleteTemplate = new Regex("00:00:0[0-9]");
				AssertContains("Progress form log. Note, the first status is not set in ProgressForm because due to its current implementation it sets status after 250 ms passed.",
@". PercentComplete: 0
Posting of AP Invoices 1 of 1
time elapsed: 01:43:34. PercentComplete: 100
",
							timeToCompleteTemplate.Replace(logger.ToString(), "01:43:34"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
				testForm.Show();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = TestFileName;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testForm.ImportButton_ForTestOnly.PerformClick();
				AssertEquals("CASS HOT files should be proposed in filter", "CASS HOT files (*.hot)|*.hot|All files (*.*)|*.*", testForm.OpenFileDialog_ForTestOnly.Filter);
				AssertEquals("Cass file should be loaded", 3, testForm.CASSBusinessEntity_ForTestOnly.Lines.Count);
				AssertEquals("IssueDate column IsVisible", true, testForm.DiscrepancyReportGrid_ForTestOnly.GetColumnStyle(CASSBillingLine.Schema.IssueDate).IsVisible);
				AssertEquals("DateOfArrival column IsVisible", false, testForm.DiscrepancyReportGrid_ForTestOnly.GetColumnStyle(CASSBillingLine.Schema.DateOfArrival).IsVisible);
				AssertEquals("DateOfDelivery column IsVisible", false, testForm.DiscrepancyReportGrid_ForTestOnly.GetColumnStyle(CASSBillingLine.Schema.DateOfDelivery).IsVisible);
			}
		}

		public void TestImportCancel()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testForm.Show();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				AssertNoExceptionThrown(() => testForm.ImportButton_ForTestOnly.PerformClick());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportSelectThenCancel()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testForm.Show();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = TestFileName;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				AssertNoExceptionThrown(() => testForm.ImportButton_ForTestOnly.PerformClick());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportWithECRRecord()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
				testForm.Show();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = TestFileNameWithECRRecords;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testForm.ImportButton_ForTestOnly.PerformClick();
				AssertEquals("CASS HOT files should be proposed in filter", "CASS HOT files (*.hot)|*.hot|All files (*.*)|*.*", testForm.OpenFileDialog_ForTestOnly.Filter);
				AssertEquals("Cass file should be loaded", 2, testForm.CASSBusinessEntity_ForTestOnly.Lines.Count);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testForm.Show();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = TestFileNameWithECRRecords;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testForm.ImportButton_ForTestOnly.PerformClick();
				AssertEquals("CASS FIL files should be proposed in filter", "CASS FIL files (*.fil)|*.fil|All files (*.*)|*.*", testForm.OpenFileDialog_ForTestOnly.Filter);
				AssertEquals("Cass file should be loaded", 4, testForm.CASSBusinessEntity_ForTestOnly.Lines.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_ImportRecords()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
				testForm.Show();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = TestFileNameWithImportRecords;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				testForm.ImportButton_ForTestOnly.PerformClick();
				AssertEquals("CASS HOT files should be proposed in filter", "CASS HOT files (*.hot)|*.hot|All files (*.*)|*.*", testForm.OpenFileDialog_ForTestOnly.Filter);
				AssertEquals("Cass file should be loaded", 61, testForm.CASSBusinessEntity_ForTestOnly.Lines.Count);
				AssertEquals("IssueDate column IsVisible", false, testForm.DiscrepancyReportGrid_ForTestOnly.GetColumnStyle(CASSBillingLine.Schema.IssueDate).IsVisible);
				AssertEquals("DateOfArrival column IsVisible", true, testForm.DiscrepancyReportGrid_ForTestOnly.GetColumnStyle(CASSBillingLine.Schema.DateOfArrival).IsVisible);
				AssertEquals("DateOfDelivery column IsVisible", true, testForm.DiscrepancyReportGrid_ForTestOnly.GetColumnStyle(CASSBillingLine.Schema.DateOfDelivery).IsVisible);
			}
		}

		public void TestPrintDiscrepanciesDocument()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				AssertNotNull(testForm);
				testForm.Show();
				testForm.PrintButton_ForTestOnly.PerformClick();
				AssertEquals("Enterprise.DocumentEngine.GUI.DocDeliveryForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
			}
		}

		public void TestAPInvoiceView()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testForm.Show();
				var cassBO = testForm.CASSBusinessEntity_ForTestOnly;
				var line = cassBO.Lines.AddNew();
				var glHeader = TestObjectCreator.GLHeader1;
				glHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				glHeader.AG_Description = "Test Account";

				var registry = AccountingConfigurationRegistry.Instance;
				using (registry.CASSGLAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid()))
				{
					TestObjectCreator.SetupCASSBillingLine(line);
					TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(line.AirlinePrefix), TestObjectCreator.AALSHI);
					SetupAssociatedBizos();
					Factory.Save();

					cassBO.ForceRecalculateData();

					var previewBtn = testForm.PreviewButton_ForTestOnly;
					var transGrid = testForm.APTransactionsGrid_ForTestOnly;

					var registryValue = new CASSFileImportDefaultTaxID();
					registryValue.StandardRatedTaxID = Guid.Empty;
					registryValue.ZeroRatedTaxID = Guid.Empty;
					using (registry.CASSFileImportDefaultTaxID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
					{
						previewBtn.PerformClick();
						string msg = @"CASS file Import Default Tax ID is not set, CASS import cannot be processed at the moment.
Please contact support for assistance with this.";
						AssertEquals("Rather user will be shown message", msg, UnitTestUserNotification.Instance.LastMessage.Text);

						transGrid.SelectAllElements();
						AssertEquals("There should be no invoice created as registry setting is incorrect.", 0, transGrid.SelectedRowCount);
					}

					registryValue.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
					registryValue.StandardRatedTaxID = TestObjectCreator.GST1.PK;
					using (registry.CASSFileImportDefaultTaxID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
					{
						previewBtn.PerformClick();

						transGrid.SelectAllElements();
						AssertEquals("Precondition: an invoice must be selected for preview.", 1, transGrid.SelectedRowCount);
						var viewMenuItem = transGrid.ContextMenu.MenuItems.FindByText("&View");
						AssertNotNull("View menu item must exist.", viewMenuItem);
						viewMenuItem.PerformClick();

						var viewForm = ZFormModaliser.LastFormShownForTest;
						AssertNotNull("View screen must be shown.", viewForm);
						AssertType(typeof(CreditNoteForm), viewForm);
						AssertEquals("Invoice form should be read only", ODisplayMode.ReadOnly, ((ZForm)viewForm).DisplayMode);

						ZFormModaliser.LastFormShownForTest = null;
						Env.Security.NewPayablesCreditNote.IsAllowed = false;
						AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () => viewMenuItem.PerformClick());
						AssertNull("If user haven't appropriate rights View screen mustn't be shown.", ZFormModaliser.LastFormShownForTest);
					}
				}
			}
		}

		public void TestAPInvoiceView_SaveAsIncomplete_ExpectedTotals()
		{
			var registry = AccountingConfigurationRegistry.Instance;
			var registryValue = new CASSFileImportDefaultTaxID();
			registryValue.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
			registryValue.StandardRatedTaxID = TestObjectCreator.GST1.PK;
			var glHeader = TestObjectCreator.GLHeader1;

			using (registry.CASSGLAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid()))
			using (registry.CASSFileImportDefaultTaxID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (var testCassForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testCassForm.Show();
				var cassBO = testCassForm.CASSBusinessEntity_ForTestOnly;
				var cassLine = cassBO.Lines.AddNew();
				glHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				glHeader.AG_Description = "Test Account";

				TestObjectCreator.SetupCASSCostComponent(cassLine, false, 6801.8m, 0m, 0m, 0m, 0m, 0m, 680.1m);
				TestObjectCreator.SetupCASSCostComponent(cassLine, true, 10156.8m, 0m, 0m, 0m, 0m, 0m, 0m, 1015.6m);

				var creditor = TestObjectCreator.AALSHI;
				TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(cassLine.AirlinePrefix), creditor);
				string origin = RefUNLOCO.LoadFromIATA(Factory, cassLine.LoadPortIATA).RL_Code;
				string destination = RefUNLOCO.LoadFromIATA(Factory, cassLine.DischargePortIATA).RL_Code;

				var consol = TestObjectCreator.CreateConsol(origin, destination, "C0001");
				consol.JK_MasterBillNum = cassLine.MAWBNumber;

				var shipment1 = TestObjectCreator.CreateShipment("SHIP1", origin, destination, consol);
				var job1 = TestObjectCreator.CreateJob(shipment1, false);

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				Factory.Save();

				cassBO.ForceRecalculateData();
				testCassForm.PreviewButton_ForTestOnly.PerformClick();
				var invoice = cassBO.APTransactions[0];
				AssertNotNull("AP Invoice", invoice);
				AssertEquals("AH_OSTotalAmount", 3690.5m, invoice.AH_OSTotalAmount);
				AssertEquals("AH_GSTAmount", 335.5m, invoice.AH_GSTAmount);

				var transGrid = testCassForm.APTransactionsGrid_ForTestOnly;
				transGrid.SelectAllElements();
				AssertEquals("Precondition: an invoice must be selected for preview.", 1, transGrid.SelectedRowCount);

				var viewMenuItem = transGrid.ContextMenu.MenuItems.FindByText("&View");
				AssertNotNull("View menu item must exist.", viewMenuItem);
				viewMenuItem.PerformClick();

				var testForm = ZFormModaliser.LastFormShownForTest as BaseInvoicingForm;
				AssertNotNull("View Invoice screen must be shown.", testForm);
				AssertEquals("Invoice form should be read only", ODisplayMode.ReadOnly, testForm.DisplayMode);

				testForm.ActionsMenuItem_ForTestOnly.OnPopup(EventArgs.Empty);
				Assert("SaveAsIncompleteMenuItem.Enabled", testForm.SaveAsIncompleteMenuItem.Enabled);
				testForm.Dispose();

				using (registry.DefaultExpectedTotalValue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					viewMenuItem.PerformClick();
					testForm = ZFormModaliser.LastFormShownForTest as BaseInvoicingForm;

					Application.DoEvents();
					var invDetails = testForm.InvoiceDetails;
					Assert("ExpectedTotal flag Visible", invDetails.InvoiceTotalValidationCheckBox.Visible);
					Assert("ExpectedTotal amount Visible", invDetails.ValidInvoiceTotalCalcEdit.Visible);

					bool expected = creditor.CompanyData.IsAPTaxApplicable;

					AssertEquals("ExpectedTax amount Visible", expected, invDetails.ExpectedTaxCalcEdit.Visible);
					AssertEquals("ExpectedExclTax amount Visible", expected, invDetails.ExpectedExclTaxCalcEdit.Visible);

					var formInvoice = testForm.Invoice_ForTestOnly;
					AssertEquals("ExpectedTax amount", expected ? 335.5m : 0, formInvoice.ExpectedInvoiceTaxTotal);
					AssertEquals("ExpectedExclTax amount", expected ? 3355m : 0, formInvoice.ExpectedInvoiceExclTaxTotal);
					AssertEquals("ExpectedTotal amount", expected ? 3690.5m : 3355, formInvoice.ExpectedInvoiceTotal);

					testForm.Dispose();
				}
			}
		}

		public void TestAPInvoiceView_SaveAsIncomplete_ComplianceSequence()
		{
			var registry = AccountingConfigurationRegistry.Instance;
			var registryValue = new CASSFileImportDefaultTaxID();
			registryValue.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
			registryValue.StandardRatedTaxID = TestObjectCreator.GST1.PK;
			var glHeader = TestObjectCreator.GLHeader1;

			var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			var accMasterRegistry = AccountingMasterFilesRegistry.Instance;
			using (accMasterRegistry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (accMasterRegistry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			using (registry.CASSGLAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, glHeader.PK.ToGuid()))
			using (registry.CASSFileImportDefaultTaxID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (var testCassForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testCassForm.Show();
				var cassBO = testCassForm.CASSBusinessEntity_ForTestOnly;
				var cassLine = cassBO.Lines.AddNew();
				glHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				glHeader.AG_Description = "Test Account";
				TestObjectCreator.SetupCASSBillingLine(cassLine);
				TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(cassLine.AirlinePrefix), TestObjectCreator.AALSHI);

				string origin = RefUNLOCO.LoadFromIATA(Factory, cassLine.LoadPortIATA).RL_Code;
				string destination = RefUNLOCO.LoadFromIATA(Factory, cassLine.DischargePortIATA).RL_Code;

				var consol = TestObjectCreator.CreateConsol(origin, destination, "C0001");
				consol.JK_MasterBillNum = cassLine.MAWBNumber;

				var shipment1 = TestObjectCreator.CreateShipment("SHIP1", origin, destination, consol);
				var job1 = TestObjectCreator.CreateJob(shipment1, false, false);
				job1.JH_GE = TestObjectCreator.FESDepartment.PK;

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				Factory.Save();

				cassBO.ForceRecalculateData();
				testCassForm.PreviewButton_ForTestOnly.PerformClick();
				var transGrid = testCassForm.APTransactionsGrid_ForTestOnly;
				transGrid.SelectAllElements();
				AssertEquals("Precondition: an invoice must be selected for preview.", 1, transGrid.SelectedRowCount);

				var viewMenuItem = transGrid.ContextMenu.MenuItems.FindByText("&View");
				AssertNotNull("View menu item must exist.", viewMenuItem);
				viewMenuItem.PerformClick();

				var testForm = ZFormModaliser.LastFormShownForTest as BaseInvoicingForm;
				AssertNotNull("View Invoice screen must be shown.", testForm);
				testForm.SaveAsIncomplete_ForTestOnly();
				Assert(!UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNoRowError("No error in Compliance Sequence for Incomplete", testForm.Invoice_ForTestOnly, ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage);
			}
		}

		public void TestAPInvoiceViewShowMutexErrors()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testForm.Show();
				CASSBillingLine line = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				TestObjectCreator.GLHeader1.AG_Description = "Test Account";
				AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
				TestObjectCreator.SetupCASSBillingLine(line);
				TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(line.AirlinePrefix), TestObjectCreator.AALSHI);
				SetupAssociatedBizos();
				Job1.Delete();
				Factory.Save();

				string expectedMessage =
@"You have created the job SHIP1 on another form, but haven't saved it yet.
Please close or save other forms that use job SHIP1 to continue.";
				var newFactory = new BusinessObjectFactory();
				var jobWithMutex = new Job.Loader(newFactory, Shipment1).TryCreateWithMutex();

				testForm.CASSBusinessEntity_ForTestOnly.ForceRecalculateData();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.PreviewButton_ForTestOnly.PerformClick();
				AssertEquals("Mutex error should be shown", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				jobWithMutex.Dispose();
				jobWithMutex.Delete();
				testForm.PreviewButton_ForTestOnly.PerformClick();

				testForm.APTransactionsGrid_ForTestOnly.SelectAllElements();
				AssertEquals("Precondition: an invoice must be selected for preview.", 1, testForm.APTransactionsGrid_ForTestOnly.SelectedRowCount);
				MenuItem viewMenuItem = testForm.APTransactionsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("&View");

				jobWithMutex = new Job.Loader(newFactory, Shipment1).TryCreateWithMutex();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				viewMenuItem.PerformClick();
				Assert("Mutex error should be shown", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));

				jobWithMutex.Dispose();
				jobWithMutex.Delete();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				viewMenuItem.PerformClick();
				AssertNull("Mutex error should not be shown", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("View screen must be shown.", ZFormModaliser.LastFormShownForTest);
				ZFormModaliser.LastFormShownForTest.Dispose();
			}
		}

		public void TestShowRejectedClaimControls()
		{
			AssertEquals("Precondition: IsRejectedClaimLinesExpected", false, CASSBilling.IsRejectedClaimLinesExpected);
			int totalsGroupBoxHeight;
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testForm.Show();
				testForm.TabControl_ForTestOnly.SelectedIndex = testForm.TabControl_ForTestOnly.TabPages.IndexOf(testForm.DiscrepancyReportTabPage_ForTestOnly);

				AssertEquals("Rejected Claims column is unavailable", true, testForm.DiscrepancyReportGrid_ForTestOnly.GetColumnStyle(CASSBillingLine.Schema.CASSRejectedClaimValueInLocalCurrencyForDisplay).IsUnavailable);
				AssertEquals("RejectedClaimsPanel_ForTestOnly.Visible", false, testForm.RejectedClaimsPanel_ForTestOnly.Visible);

				totalsGroupBoxHeight = testForm.TotalsGroupBox_ForTestOnly.Size.Height - testForm.TotalsGroupBox_ForTestOnly.DisplayRectangle.Bottom + testForm.SystemCostAccrualPanel_ForTestOnly.Bottom;
				AssertEquals("TotalsGroupBox_ForTestOnly.Size", totalsGroupBoxHeight, testForm.TotalsGroupBox_ForTestOnly.Size.Height);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			AssertEquals("Precondition: IsRejectedClaimLinesExpected", true, CASSBilling.IsRejectedClaimLinesExpected);
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testForm.Show();
				testForm.TabControl_ForTestOnly.SelectedIndex = testForm.TabControl_ForTestOnly.TabPages.IndexOf(testForm.DiscrepancyReportTabPage_ForTestOnly);

				AssertEquals("Rejected Claims column is unavailable", false, testForm.DiscrepancyReportGrid_ForTestOnly.GetColumnStyle(CASSBillingLine.Schema.CASSRejectedClaimValueInLocalCurrencyForDisplay).IsUnavailable);
				AssertEquals("RejectedClaimsPanel_ForTestOnly.Visible", true, testForm.RejectedClaimsPanel_ForTestOnly.Visible);

				AssertEquals("TotalsGroupBox_ForTestOnly.Size", totalsGroupBoxHeight + testForm.RejectedClaimsPanel_ForTestOnly.Size.Height, testForm.TotalsGroupBox_ForTestOnly.Size.Height);
			}
		}

		public void TestColumnExistance()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				var columnNamesList = from ZGridColumnInfo cs in testForm.DiscrepancyReportGrid_ForTestOnly.ColumnStyles.ToArray() orderby cs.ColumnName select (ZString)cs.ColumnName;
				var columnNamesString = ZString.Join(",", columnNamesList.ToArray());
				AssertEquals("We have the expected columns, all of the expected columns and only the expected columns",
					"Airline2LetterCode,BranchCode,CASSCostAdjustedValueInLocalCurrency,CASSCostValueInLocalCurrencyForDisplay,CASSRejectedClaimValueInLocalCurrencyForDisplay,CASSWeight,CASSWeightUnit,ConsolCreateTime,ConsolCreateUser,ConsolID,ConsolLastEditTime,ConsolLastEditUser,CostDifference,CostDifferenceMargin,CreditorCode,DateOfArrival,DateOfDelivery,DischargePort,InvoiceNumber,IsCASSAmendment,IssueDate,LoadPort,MAWBNumber,NetCASSCost,StatusForBinding,SystemCostAccrualValue,SystemCostPostedValue,SystemWeight,WeightDifference,WeightDifferenceMargin",
					columnNamesString);
			}
		}

		public void TestAuditColumnsNotVisibleByDefault()
		{
			var columnsToTest = new[] { "ConsolCreateUser", "ConsolCreateTime", "ConsolLastEditUser", "ConsolLastEditTime", "InvoiceNumber" };

			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				var goodColumns = from ZGridColumnInfo cs in testForm.DiscrepancyReportGrid_ForTestOnly.ColumnStyles.ToArray() where columnsToTest.Contains(cs.ColumnName) && !cs.IsVisible select cs;
				AssertEquals(5, goodColumns.Count());
			}
		}

		public void TestVisibilitySetup()
		{
			var prevValue = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				using (var testForm = GetFormToBashCore() as CASSExportBillingForm)
				{
					testForm.Show();
					AssertEquals("Should not be Visible", false, testForm.ExportButton_ForTestOnly.Visible);
				}

				foreach (ZString countryCode in new ZString[] { Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Canada })
				{
					GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryCode;
					using (var testForm = GetFormToBashCore() as CASSExportBillingForm)
					{
						testForm.Show();
						AssertEquals("Should not be Visible", true, testForm.ExportButton_ForTestOnly.Visible);
					}
				}

				Env.Security.APCASSCostFileModification.IsAllowed = false;
				using (var testForm = GetFormToBashCore() as CASSExportBillingForm)
				{
					testForm.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					testForm.ExportButton_ForTestOnly.PerformClick();
					AssertEquals("Security Error Message : ExportButton", Env.Security.APCASSCostFileModification.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Security : ExportLinesGrid_ForTestOnly", false, testForm.ExportLinesGrid_ForTestOnly.Visible);
					AssertEquals("Security Error Message : ExportLinesGrid", true, testForm.SecurityCheckpointMessageForExportTabLabel_ForTestOnly.Visible);
					AssertEquals("Security : ImportLinesGrid_ForTestOnly", false, testForm.ImportLinesGrid_ForTestOnly.Visible);
					AssertEquals("Security Error Message : ImportLinesGrid", true, testForm.SecurityCheckpointMessageForImportTabLabel_ForTestOnly.Visible);
				}

				Env.Security.APCASSCostFileModification.IsAllowed = true;
				using (var testForm = GetFormToBashCore() as CASSExportBillingForm)
				{
					testForm.Show();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testForm.ExportButton_ForTestOnly.PerformClick();

					AssertEquals("Security Error Message : ExportButton", "No Adjustment Made", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Security : ExportLinesGrid_ForTestOnly", true, testForm.ExportLinesGrid_ForTestOnly.Visible);
					AssertEquals("Security Error Message : ExportLinesGrid", false, testForm.SecurityCheckpointMessageForExportTabLabel_ForTestOnly.Visible);
					AssertEquals("Security : ImportLinesGrid_ForTestOnly", true, testForm.ImportLinesGrid_ForTestOnly.Visible);
					AssertEquals("Security Error Message : ImportLinesGrid", false, testForm.SecurityCheckpointMessageForImportTabLabel_ForTestOnly.Visible);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = prevValue;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExportFilePrintPrompt()
		{
			var prevValue = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
				TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				TestObjectCreator.GLHeader1.AG_Description = "Test Account";
				AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
				Env.Security.APCASSCostFileModification.IsAllowed = true;

				using (var testForm = GetFormToBashCore() as CASSExportBillingForm)
				{
					AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
					testForm.Show();
					AssertEquals("Should not be Visible", false, testForm.PromptForAdjustmentFilePrint_ForTestOnly);

					testForm.Show();
					ZFormModaliser.FileNameToSelectInShowCommonDialog = TestFileName;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					testForm.ImportButton_ForTestOnly.PerformClick();
					AssertEquals("CASS HOT files should be proposed in filter", "CASS HOT files (*.hot)|*.hot|All files (*.*)|*.*", testForm.OpenFileDialog_ForTestOnly.Filter);
					AssertEquals("Cass file should be loaded", 3, testForm.CASSBusinessEntity_ForTestOnly.Lines.Count);

					(testForm.CASSBusinessEntity_ForTestOnly.CostHeader.Lines.First() as CASSCostExportLine).WeightChargePP = 2005.36M;
					AssertEquals("Should not be Visible", true, testForm.PromptForAdjustmentFilePrint_ForTestOnly);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = prevValue;
			}
		}

		public void TestShowInvoiceNumberControl()
		{
			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.DefaultValue);
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testForm.Show();

				AssertEquals("Invoice Number column is available", false, testForm.DiscrepancyReportGrid_ForTestOnly.GetColumnStyle(CASSBillingLine.Schema.InvoiceNumber).IsUnavailable);
				AssertEquals("IsWholeRowSelectedOnClick is set to false", false, testForm.DiscrepancyReportGrid_ForTestOnly.IsWholeRowSelectedOnClick);
			}

			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 1000);
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				testForm.Show();

				AssertEquals("Invoice Number column is unavailable", true, testForm.DiscrepancyReportGrid_ForTestOnly.GetColumnStyle(CASSBillingLine.Schema.InvoiceNumber).IsUnavailable);
				AssertEquals("IsWholeRowSelectedOnClick is set to true", true, testForm.DiscrepancyReportGrid_ForTestOnly.IsWholeRowSelectedOnClick);
			}
		}

		public void TestCalculateAPInvoicesWithDuplicateInvoiceNumberErrors()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Show();

				var org = TestObjectCreator.ABIGAS;
				org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
				org.OH_IsCreditor = true;

				AccTransactionHeader aPInvoice = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
				aPInvoice.AH_OH = org.PK;
				aPInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
				aPInvoice.AH_TransactionNum = "CASS1";
				Factory.Save();

				CASSBillingLine cassLine1 = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.SetupCASSCostComponent(cassLine1, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
				cassLine1.InvoiceNumber = "CASS1";
				CASSBillingLine cassLine2 = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.SetupCASSCostComponent(cassLine2, true, 101568M, 0m, 0m, 0m, 0m, 0m, adjustedVatDueAirlineAmount: 10156M, airlinePrefix: "081", awbSerialNumber: "67828075", agentCode: "23470/068-510", weight: 2150M, currency: "USD");
				cassLine2.InvoiceNumber = "CASS1";
				CASSBillingLine cassLine3 = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.SetupCASSCostComponent(cassLine3, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828076", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
				cassLine3.InvoiceNumber = "CASS1";

				testForm.CASSBusinessEntity_ForTestOnly.ForceRecalculateData();  //refresh
				testForm.PreviewButton_ForTestOnly.PerformClick(); //calculate AP invoices

				AssertEquals("There are errors that need correcting before the AP invoices can be calculated. Please hover over the red stop errors for specific details.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, testForm.CASSBusinessEntity_ForTestOnly.APTransactions.Count);

				cassLine2.InvoiceNumber = "CASS2";
				testForm.PreviewButton_ForTestOnly.PerformClick(); //calculate AP invoices

				AssertEquals("There are errors that need correcting before the AP invoices can be calculated. Please hover over the red stop errors for specific details.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, testForm.CASSBusinessEntity_ForTestOnly.APTransactions.Count);

				cassLine1.InvoiceNumber = "CASS3";
				cassLine3.InvoiceNumber = "CASS3";

				testForm.PreviewButton_ForTestOnly.PerformClick(); //calculate AP invoices
				AssertEquals(2, testForm.CASSBusinessEntity_ForTestOnly.APTransactions.Count);
			}
		}

		public void TestAPInvoicePosting_CalculateInvoicesDuringPostingWithErrors()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Show();

				var airline = TestObjectCreator.CreateAirLine("081");
				var org = TestObjectCreator.ABIGAS;
				org.MiscServ.OM_RM_Airline = airline.PK;
				org.OH_IsCreditor = true;

				AccTransactionHeader aPInvoice = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
				aPInvoice.AH_OH = org.PK;
				aPInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
				aPInvoice.AH_TransactionNum = "CASS1";
				TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
				TestObjectCreator.GLHeader1.AG_Description = "Test Account";
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				Factory.Save();

				CASSBillingLine cassLine1 = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.SetupCASSCostComponent(cassLine1, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
				TestObjectCreator.SetupCreditorAirline(airline, TestObjectCreator.ABIGAS);
				cassLine1.InvoiceNumber = "CASS1";
				CASSBillingLine cassLine2 = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.SetupCASSCostComponent(cassLine2, true, 101568M, 0m, 0m, 0m, 0m, 0m, adjustedVatDueAirlineAmount: 10156M, airlinePrefix: "081", awbSerialNumber: "67828075", agentCode: "23470/068-510", weight: 2150M, currency: "USD");
				cassLine2.InvoiceNumber = "CASS1";
				TestObjectCreator.SetupCreditorAirline(airline, TestObjectCreator.ABIGAS);
				CASSBillingLine cassLine3 = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.SetupCASSCostComponent(cassLine3, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828076", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
				cassLine3.InvoiceNumber = "CASS1";
				TestObjectCreator.SetupCreditorAirline(airline, TestObjectCreator.ABIGAS);

				testForm.FireSaveButton();

				AssertEquals("There are errors that need correcting before the AP invoices can be calculated. Please hover over the red stop errors for specific details.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, testForm.CASSBusinessEntity_ForTestOnly.APTransactions.Count);

				cassLine2.InvoiceNumber = "CASS2";

				AssertEquals("There are errors that need correcting before the AP invoices can be calculated. Please hover over the red stop errors for specific details.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, testForm.CASSBusinessEntity_ForTestOnly.APTransactions.Count);

				cassLine1.InvoiceNumber = "CASS3";
				cassLine3.InvoiceNumber = "CASS3";

				testForm.FireSaveButton();
				AssertEquals("Two AP invoice should be generated.", 2, testForm.CASSBusinessEntity_ForTestOnly.APTransactions.Count);
			}
		}

		public void TestAPInvoicePosting_UpdateInvoiceNumberAfterAPInvoicesCalculated()
		{
			using (CASSExportBillingForm testForm = GetFormToBashCore() as CASSExportBillingForm)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.Show();

				var org = TestObjectCreator.ABIGAS;
				org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
				org.OH_IsCreditor = true;
				Factory.Save();

				CASSBillingLine cassLine1 = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.SetupCASSCostComponent(cassLine1, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
				cassLine1.InvoiceNumber = "CASS1";
				CASSBillingLine cassLine2 = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.SetupCASSCostComponent(cassLine2, true, 101568M, 0m, 0m, 0m, 0m, 0m, adjustedVatDueAirlineAmount: 10156M, airlinePrefix: "081", awbSerialNumber: "67828075", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
				cassLine2.InvoiceNumber = "CASS1";
				CASSBillingLine cassLine3 = testForm.CASSBusinessEntity_ForTestOnly.Lines.AddNew();
				TestObjectCreator.SetupCASSCostComponent(cassLine3, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828076", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
				cassLine3.InvoiceNumber = "CASS1";

				testForm.PreviewButton_ForTestOnly.PerformClick(); //calculate AP invoices
				cassLine3.InvoiceNumber = "CASS2";
				testForm.FireSaveButton();

				AssertEquals("You have changed the AP Invoice Number on one or more lines but haven't recalculated AP invoices to update the invoice number/s.\r\nDo you wish to continue without recalculating?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, testForm.CASSBusinessEntity_ForTestOnly.APTransactions.Count);
			}
		}
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			BusinessObjectFactory testFactory = new BusinessObjectFactory();
			CASSBilling testCASSBilling = new CASSBilling(Factory);

			return new CASSExportBillingForm(testCASSBilling);
		}

		string TestFileName
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\CASSBilling\Testing\23470060003_HOTFILESAMPLE_SMALL.HOT"; }
		}

		string TestFileNameWithECRRecords
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\CASSBilling\Testing\CNS_CLT_1004 - SMALL.fil"; }
		}

		string TestFileNameWithImportRecords
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\CASSBilling\Testing\20100801_20100815_Import.hot"; }
		}

		string TestFileName_ClaimExisting
		{
			get { return BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\CASSBilling\Testing\HOTFILESAMPLE_SMALL_ClaimExisting.hot"; }
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		ForwardingConsol Consol;
		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;
		Job Job1;

		void SetupAssociatedBizos()
		{
			CASSBillingLine cassLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(cassLine);

			string origin = RefUNLOCO.LoadFromIATA(Factory, cassLine.LoadPortIATA).RL_Code;
			string destination = RefUNLOCO.LoadFromIATA(Factory, cassLine.DischargePortIATA).RL_Code;

			Consol = TestObjectCreator.CreateConsol(origin, destination, "C0001");
			Consol.JK_MasterBillNum = cassLine.MAWBNumber;

			Shipment1 = TestObjectCreator.CreateShipment("SHIP1", origin, destination, Consol);
			Shipment1.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			Shipment2 = TestObjectCreator.CreateShipment("SHIP2", origin, destination, Consol);
			Shipment2.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			Job1 = TestObjectCreator.CreateJob(Shipment1, false);
			TestObjectCreator.CreateJob(Shipment2, false);

			TestObjectCreator.SetExchangeRate(Job1, cassLine.CASSCostCurrency, 2M);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestObjectCreator.GLHeader1.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
		}

		protected override void SetUp()
		{
			base.SetUp();

			var rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainFreeGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(0);
			rate.Factory.Save();

			rate = AccTaxRate.Helper.FindTaxRate(new BusinessObjectFactory(), AccTaxRate.Helper.MainGSTTaxRegistryID, Env.CurrentCompanyPK);
			rate.SetRateNumerator_ForTestOnly(10);
			rate.Factory.Save();
		}

		#endregion

	}
}
