using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ErrorMessages = Enterprise.Accounting.Business.AccountingConstants.ChequeNumberAllocationErrorMessages;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(InvoiceForm))]
	public class APInvoiceFormTest : InvoiceFormTest
	{
		public class NonTransactionedAPCreditNoteFormTest : NonTransactionedBaseInvoicingFormTest
		{
			protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
			{
				return APInvoiceFormTest.GetFormByInvoice(invoice);
			}

			protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true)
			{
				return APInvoiceFormTest.GetInvoiceWithValidTestData();
			}

			APInvoiceFormTest APInvoiceFormTest
			{
				get
				{
					if (apInvoiceFormTest == null)
					{
						apInvoiceFormTest = new APInvoiceFormTest();
					}

					return apInvoiceFormTest;
				}
			}
			APInvoiceFormTest apInvoiceFormTest;
		}

		[TestDate(2012, 12, 12)]
		public void TestInvoiceFormDisplayModeWithQueryClaim()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var creator = new TestObjectCreator(Factory);

			var testOrgHeader = Factory.New<OrgHeader>();
			testOrgHeader.OH_Code = TestObjectCreator.GetRandomString(8);
			testOrgHeader.CompanyData.OB_IsCreditor = true;

			creator.CreateBranch("PRX", GlbCompany.CurrentCompany, creator.Agent);

			var sisterCompanyInvoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", creator.AUD, 1M, 100M, 0M, 100M, 0M, testOrgHeader, creator.CC1.PK);
			sisterCompanyInvoice.AH_OH = creator.Agent.PK;

			AccChargeCode accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			accChargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			sisterCompanyInvoice.Lines[0].AL_AC = accChargeCode.PK;

			Factory.Save();

			var converter = new UnapprovedTransactionConverter(Factory);
			var invoice = converter.ConvertToAP(sisterCompanyInvoice, false);

			using (InvoiceForm aPInvoiceForm = new InvoiceForm(invoice))
			{
				aPInvoiceForm.Show();
				invoice.InitialiseApprovingWithClaim(sisterCompanyInvoice);
				aPInvoiceForm.Save_ForTestOnly(new ITransactionParticipant[] { invoice.Factory });

				AssertEquals("Should be showing the claim form", typeof(AccQueryClaimForm), ZFormModaliser.ActiveForm.GetType());
				AccQueryClaimForm claimForm = (AccQueryClaimForm)ZFormModaliser.ActiveForm;
				var claim = (APAccQueryClaim)claimForm.BusinessEntity;

				AssertNotEquals("Claim should be created in a different factory", invoice.Factory, claim.Factory);

				claim.CreateAndAttachRelatedCreditNote();
				claimForm.Close();

				AssertEquals(ODisplayMode.Browse, aPInvoiceForm.DisplayMode);
				AssertEquals("Apply button text", "&New", aPInvoiceForm.FApplyButton_ForTestOnly.Text);
			}
		}

		public void TestTaxSummaryGridApportionChargesButtonRefresh()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";
			var shipment1 = consol.Shipments.AddNew();
			var job1 = creator.CreateJob(shipment1);
			var shipment2 = consol.Shipments.AddNew();
			var job2 = creator.CreateJob(shipment2);

			factory.Save();

			var invoice = factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = creator.AALSHI.PK;

			try
			{
				using (var invoiceForm = new InvoiceForm(invoice))
				{
					invoiceForm.DisplayMode = ODisplayMode.New;
					invoice.Company.GC_IsGSTRegistered = true;
					invoiceForm.Show();

					Assert("Should more than 2 tab pages", invoiceForm.InvoiceDetails.TabControl_ForTest.TabCount > 2);
					Assert("First tab page should not be TaxSummaryTabPage", invoiceForm.InvoiceDetails.TabControl_ForTest.TabPages[0].Name != "TaxSummaryTabPage");

					invoiceForm.InvoiceDetails.TabControl_ForTest.SelectTab(0);
					Assert("IsTaxSummaryTabSelected is false", !invoiceForm.Invoice_ForTestOnly.IsTaxSummaryTabSelected);
					TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoiceForm.Invoice_ForTestOnly);

					//Set up the test data
					invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					var consolCostingForm1 = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					var cost1 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
					cost1.E6_AC_ChargeCode = creator.CC12.PK;
					cost1.E6_OSCostAmount = 70M;
					cost1.E6_ApportionmentMethod = "SHP";

					TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode, false, invoice);

					cost1.SplitApportionAmount();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					consolCostingForm1.Close();
					Application.DoEvents();

					AssertEquals("There should be 2 lines in the AP Invoice.", 2, invoice.Lines.Count);
					var cc1Job1Line = invoice.Lines[0];
					var cc1Job2Line = invoice.Lines[1];

					TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoiceForm.Invoice_ForTestOnly);

					invoiceForm.InvoiceDetails.TabControl_ForTest.SelectTab("TaxSummaryTabPage");
					Assert("IsTaxSummaryTabSelected is true", invoiceForm.Invoice_ForTestOnly.IsTaxSummaryTabSelected);
					AssertEquals("Should contain 1 item", invoice.InvoiceLineTaxSummaries.Count, 1);
					TaxSummaryTestHelper.AssertTaxSummaryResult(invoice.InvoiceLineTaxSummaries[0], cc1Job1Line.TaxRate.AT_Code, null, cc1Job1Line.AL_LocalExTaxAmount + cc1Job2Line.AL_LocalExTaxAmount, cc1Job1Line.AL_LocalTaxAmount + cc1Job2Line.AL_LocalTaxAmount, cc1Job1Line.AL_LocalTotalAmount + cc1Job2Line.AL_LocalTotalAmount, cc1Job1Line.TaxRate.AT_Description);

					//Set up the test data
					invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
					AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
					var consolCostingForm2 = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
					var cost2 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
					cost2.E6_AC_ChargeCode = creator.CC12.PK;
					cost2.E6_OSCostAmount = 70M;
					cost2.E6_ApportionmentMethod = "SHP";
					cost2.SplitApportionAmount();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					consolCostingForm2.Close();
					Application.DoEvents();

					AssertEquals("There should be 4 lines in the AP Invoice.", 4, invoice.Lines.Count);
					cc1Job1Line = invoice.Lines[0];
					cc1Job2Line = invoice.Lines[1];
					var cc1Job3Line = invoice.Lines[2];
					var cc1Job4Line = invoice.Lines[3];

					AssertEquals("Should contain 1 item", invoice.InvoiceLineTaxSummaries.Count, 1);
					TaxSummaryTestHelper.AssertTaxSummaryResult(invoice.InvoiceLineTaxSummaries[0], cc1Job1Line.TaxRate.AT_Code, null,
						cc1Job1Line.AL_LocalExTaxAmount + cc1Job2Line.AL_LocalExTaxAmount + cc1Job3Line.AL_LocalExTaxAmount + cc1Job4Line.AL_LocalExTaxAmount,
						cc1Job1Line.AL_LocalTaxAmount + cc1Job2Line.AL_LocalTaxAmount + cc1Job3Line.AL_LocalTaxAmount + cc1Job4Line.AL_LocalTaxAmount,
						cc1Job1Line.AL_LocalTotalAmount + cc1Job2Line.AL_LocalTotalAmount + cc1Job3Line.AL_LocalTotalAmount + cc1Job4Line.AL_LocalTotalAmount,
						cc1Job1Line.TaxRate.AT_Description);
				}
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public void TestTaxSummaryGridBulkChargeImportButtonRefresh()
		{
			var consol = TestObjectCreator.CreateConsol("D", "S", "CTEST01");
			consol.JK_MasterBillNum = "111111";
			var shipment1 = TestObjectCreator.CreateShipment("STEST01", consol);
			shipment1.JS_HouseBill = "11111";
			var sJob = TestObjectCreator.CreateJob(shipment1);
			var sCharge = TestObjectCreator.CreateCharge(sJob, TestObjectCreator.CC1, null, TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, TestObjectCreator.Debtor);

			var shipment2 = TestObjectCreator.CreateShipment("STEST02", consol);
			shipment2.JS_HouseBill = "22222";
			var sJob2 = TestObjectCreator.CreateJob(shipment2);
			var sCharge2 = TestObjectCreator.CreateCharge(sJob2, TestObjectCreator.CC2, null, TestObjectCreator.AUD, 100m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 200m, TestObjectCreator.Debtor);

			Factory.Save();

			var invoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, null, null, null);
			invoice.AH_OH = TestObjectCreator.Creditor1.PK;
			invoice.AH_TransactionNum = "T000001";

			using (InvoiceForm invoiceForm = new InvoiceForm(invoice))
			{
				invoiceForm.DisplayMode = ODisplayMode.New;
				invoice.Company.GC_IsGSTRegistered = true;
				invoiceForm.Show();

				Assert("Should more than 2 tab pages", invoiceForm.InvoiceDetails.TabControl_ForTest.TabCount > 2);
				Assert("First tab page should not be TaxSummaryTabPage", invoiceForm.InvoiceDetails.TabControl_ForTest.TabPages[0].Name != "TaxSummaryTabPage");

				invoiceForm.InvoiceDetails.TabControl_ForTest.SelectTab(0);
				Assert("IsTaxSummaryTabSelected is false", !invoice.IsTaxSummaryTabSelected);
				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoiceForm.Invoice_ForTestOnly);

				//Because to we need InvoicingBaseBulkChargeImporter to import data, mock open the InvoicingBaseBulkChargeImportForm
				var importer = new InvoicingBaseBulkChargeImporter(invoice);
				var creditorFilter = (ModuleGuidFilter)importer.Filters[InvoiceBulkOperationFilterHelper.CreditorFilterName];
				creditorFilter.IsActive = true;
				creditorFilter.Property = TestObjectCreator.Creditor1.PK;
				var accuralFilter = (ModuleFlagsFilter)importer.Filters[InvoiceBulkOperationFilterHelper.IncludeAccrualsWithNoCreditorFilterName];
				accuralFilter.IsActive = true;
				accuralFilter.Property0 = ZBool.False;
				var chargeCodeFilter = (ModuleGuidFilter)importer.Filters[InvoiceBulkOperationFilterHelper.ChargeCodeFilterName];
				chargeCodeFilter.IsActive = true;
				chargeCodeFilter.Property = TestObjectCreator.CC1.PK;

				using (var importtest = new InvoicingBaseBulkChargeImportForm(importer))
				{
					ZFormModaliser.Show(importtest, invoiceForm);
					importer.LoadJobsCollection();
					importer.Import();
					importtest.Close();
					invoiceForm.ValidateAll_ForTestOnly(ValidationType.Light);
				}

				AssertEquals("Should contain 1 invoice line", invoice.Lines.Count, 1);
				TaxSummaryTestHelper.AssertInvoiceLineTaxSummariesBeNull(invoice);

				invoiceForm.InvoiceDetails.TabControl_ForTest.SelectTab("TaxSummaryTabPage");
				Assert("IsTaxSummaryTabSelected is true", invoice.IsTaxSummaryTabSelected);
				AssertEquals("Should contain 1 item", invoice.InvoiceLineTaxSummaries.Count, 1);
				TaxSummaryTestHelper.AssertTaxSummaryResult(invoice.InvoiceLineTaxSummaries[0], invoice.Lines[0].TaxRate.AT_Code, null, invoice.Lines[0].AL_LocalExTaxAmount, invoice.Lines[0].AL_LocalTaxAmount, invoice.Lines[0].AL_LocalTotalAmount, invoice.Lines[0].TaxRate.AT_Description);

				Factory.SeedQueryCache("JobHeader", new ZQuery(JobHeaderSchema.PK, sJob.PK));

				chargeCodeFilter.Property = TestObjectCreator.CC2.PK;

				using (var importtest = new InvoicingBaseBulkChargeImportForm(importer))
				{
					ZFormModaliser.Show(importtest, invoiceForm);
					importer.LoadJobsCollection();
					importer.Import();
					importtest.Close();
					invoiceForm.ValidateAll_ForTestOnly(ValidationType.Light);
				}

				AssertEquals("Should contain 2 invoice lines", invoice.Lines.Count, 2);
				AssertEquals("Should contain 1 item", invoice.InvoiceLineTaxSummaries.Count, 1);
				TaxSummaryTestHelper.AssertTaxSummaryResult(invoice.InvoiceLineTaxSummaries[0], invoice.Lines[0].TaxRate.AT_Code, null,
					invoice.Lines[0].AL_LocalExTaxAmount + invoice.Lines[1].AL_LocalExTaxAmount,
					invoice.Lines[0].AL_LocalTaxAmount + invoice.Lines[1].AL_LocalTaxAmount,
					invoice.Lines[0].AL_LocalTotalAmount + invoice.Lines[1].AL_LocalTotalAmount,
					invoice.Lines[0].TaxRate.AT_Description);
			}
		}

		public void TestAPInvoiceWithJobCollectionForSilentErrors()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(factory);
			ForwardingConsol consol = creator.CreateConsol("D", "S", "CTEST01");
			consol.JK_MasterBillNum = "222222";
			ForwardingShipment shipment = creator.CreateShipment("STEST01", consol);
			shipment.JS_HouseBill = "11111";
			Job sJob = creator.CreateJob(shipment);
			Charge sCharge = creator.CreateCharge(sJob, creator.CC1, null, creator.AUD, 100m, creator.Creditor1, creator.AUD, 200m, null);
			factory.Save();

			factory.SeedQueryCache("JobHeader", new ZQuery(JobHeaderSchema.PK, sJob.PK));

			APInvoice apInvoice = (APInvoice)creator.CreateInvoice(typeof(APInvoice), creator.AUD, null, null, null);
			apInvoice.AH_OH = creator.Creditor1.PK;
			apInvoice.AH_TransactionNum = "T000001";

			using (InvoiceForm form = new InvoiceForm(apInvoice))
			{
				form.Show();
				InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(apInvoice);
				ModuleGuidFilter creditorFilter = (ModuleGuidFilter)importer.Filters[InvoiceBulkOperationFilterHelper.CreditorFilterName];
				creditorFilter.IsActive = true;
				creditorFilter.Property = creator.Creditor1.PK;
				ModuleFlagsFilter accuralFilter = (ModuleFlagsFilter)importer.Filters[InvoiceBulkOperationFilterHelper.IncludeAccrualsWithNoCreditorFilterName];
				accuralFilter.IsActive = true;
				accuralFilter.Property0 = ZBool.False;
				InvoicingBaseBulkChargeImportForm importtest = new InvoicingBaseBulkChargeImportForm(importer);
				ZFormModaliser.Show(importtest, form);
				importer.LoadJobsCollection();
				importer.Import();
				importtest.Close();
				form.ValidateAll_ForTestOnly(ValidationType.Light);

				AssertEquals("No DeveloperNotificationException", "", ErrorReporter.LastMessageReported);
			}
		}

		public void TestSaveAsIncomplete_AfterBulkChargeImport_PST()
		{
			AssertSaveAsIncomplete_AfterBulkChargeImport(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestSaveAsIncomplete_AfterBulkChargeImport_NOT()
		{
			AssertSaveAsIncomplete_AfterBulkChargeImport(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code);
		}

		public void AssertSaveAsIncomplete_AfterBulkChargeImport(string dateOption)
		{
			var consol = TestObjectCreator.CreateConsol("D", "S", "CTEST01");
			consol.JK_MasterBillNum = "222222";
			var shipment = TestObjectCreator.CreateShipment("STEST01", consol);
			shipment.JS_HouseBill = "11111";
			var sJob = TestObjectCreator.CreateJob(shipment);
			TestObjectCreator.CreateCharge(sJob, TestObjectCreator.CC1, null, TestObjectCreator.EUR, 100m, TestObjectCreator.Creditor1, TestObjectCreator.EUR, 200m, null);

			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			Factory.Save();

			Factory.SeedQueryCache("JobHeader", new ZQuery(JobHeaderSchema.PK, sJob.PK));

			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.EUR, null, null, null);
			apInvoice.AH_OH = TestObjectCreator.Creditor1.PK;
			apInvoice.AH_TransactionNum = "T000001";

			var registry = AccountingMasterFilesRegistry.Instance;
			var guid_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			foreach (bool incomplete in new[] { true, false })
			{
				foreach (var complianceAllocation in new[] {
						AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post,
						AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Print })
				{
					using (registry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, dateOption))
					using (registry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, complianceAllocation))
					{
						using (InvoiceForm form = new InvoiceForm(apInvoice))
						{
							form.Show();

							var importer = new InvoicingBaseBulkChargeImporter(apInvoice);
							var creditorFilter = (ModuleGuidFilter)importer.Filters[InvoiceBulkOperationFilterHelper.CreditorFilterName];
							creditorFilter.IsActive = true;
							creditorFilter.Property = TestObjectCreator.Creditor1.PK;
							var accrualFilter = (ModuleFlagsFilter)importer.Filters[InvoiceBulkOperationFilterHelper.IncludeAccrualsWithNoCreditorFilterName];
							accrualFilter.IsActive = true;
							accrualFilter.Property0 = ZBool.False;

							var importtest = new InvoicingBaseBulkChargeImportForm(importer);
							ZFormModaliser.Show(importtest, form);
							importer.LoadJobsCollection();
							importer.Import();
							importtest.Close();

							var userNotif = UnitTestUserNotification.Instance;
							userNotif.ClearMessagesAndAnswers();

							if (incomplete)
							{
								form.SaveAsIncomplete_ForTestOnly();
								Assert(!apInvoice.HasErrors);
								AssertContains("Save as incomplete successful", userNotif.LastMessage.Text);

								Assert(apInvoice.AH_Ledger == LedgerTypes.IncompleteTransactions);
								apInvoice.AH_Desc = "Test Edit";
								form.SaveAsIncomplete_ForTestOnly();
								Assert(!apInvoice.HasErrors);
								AssertContains("Save as incomplete successful", userNotif.LastMessage.Text);
							}
							else
							{
								form.ValidateAndSave_ForTestOnly();
								bool shouldCheckCompliance = dateOption != AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code
									&& complianceAllocation == AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post;
								AssertEquals(shouldCheckCompliance, apInvoice.HasErrors);
								if (shouldCheckCompliance)
								{
									AssertContains(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage, apInvoice.Notifications.ToUniqueMessageListString());
									AssertContains("There are errors - can't save.", userNotif.LastMessage.Text);
								}
							}
						}
					}
				}
			}
		}

		public void TestNoExceptionWhenApporvingInvoiceWithCliams_NoSecurityRight()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ARInvoice arInv = creator.CreateARInvoice<ARInvoice>("ARInv", creator.AUD, 1m, creator.ABIGAS);
			APInvoice apInv = creator.CreateAPInvoice<APInvoice>("APInv", creator.AUD, 1m, 100m, 0m, 0m, 0m, 0m, 0m);
			apInv.IsConvertedFromARInvoice = true;
			apInv.SubmittedFromInvoicingForm = true;
			apInv.InitialiseApprovingWithClaim(arInv);

			Env.Security.PayablesClaimsAndQueriesNew.IsAllowed = false;

			using (InvoiceForm form = (InvoiceForm)GetFormByInvoice(apInv))
			{
				form.Show();
				Application.DoEvents();
				AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", delegate { form.SaveInternal_ForTestOnly(); });
			}
		}

		public void TestSetupReceiptPaymentPanel()
		{
			using (InvoiceForm form = (InvoiceForm)GetFormToBashCore())
			{
				form.Show();

				AssertEquals("Controls.Count", 1, form.ReceiptPaymentPanel_ForTestOnly.Controls.Count);
				AssertEquals("Type of Control", typeof(InvoicePaymentUserControl), form.ReceiptPaymentPanel_ForTestOnly.Controls[0].GetType());
				InvoicePaymentUserControl control = form.ReceiptPaymentPanel_ForTestOnly.Controls[0] as InvoicePaymentUserControl;
				control.Parent.Visible = true; // simulate when binding will start
				AssertEquals(control.ReceiptPaymentAH_InvoiceDateEdit.CaptionResourceString.Caption, "Payment Date");

				if (form.Invoice_ForTestOnly is UAInvoice)
				{
					AssertEquals("DataBindings.Count", 0, control.ReceiptPaymentAH_ReceiptTypeDropEdit.DataBindings.Count);
				}
				else
				{
					AssertNotEquals("DataBindings.Count", 0, control.ReceiptPaymentAH_ReceiptTypeDropEdit.DataBindings.Count);
				}
			}
		}

		public override void TestPreviewInvoiceMenuItem_OnOpeningFormWhenTaxFrameworkIsDisabled()
		{
			Assert("Not applicable here", true);
		}

		public override void TestPreviewInvoiceMenuItem_OnOpeningFormWhenTaxFrameworkIsEnabled()
		{
			Assert("Not applicable here", true);
		}

		public override void TestPreviewInvoiceMenuItem_WhenCalculateTaxTransactionsButtonDisabled()
		{
			Assert("Not applicable here", true);
		}

		public override void TestPreviewInvoiceMenuItem_WhenCalculateTaxTransactionsButtonDisabledAndEnabled()
		{
			Assert("Not applicable here", true);
		}

		#region CheckingCashInvoiceDisplaysHotCheques Test

		public void TestCheckingCashInvoiceDisplaysHotCheques()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_GB = GlbBranch.CurrentBranch.PK;
			AccHotCheque hotCheque = GetNewHotCheque(org, chequeBook);

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = org.PK;
			using (ZFormModaliser.SuspendDispose())
			{
				using (MockAPInvoiceForm form = new MockAPInvoiceForm(invoice))
				{
					form.Show();
					form.APInvoice.SubmittedFromInvoicingForm = true;
					form.APInvoice.IsInvoiceReceiptPayment = true;
					Application.DoEvents();
					HotChequeLinkForm hotChequesForm = ZFormModaliser.LastFormShownDialogForTest as HotChequeLinkForm;
					AssertNotNull("HotChequeLinkForm should have popped up", hotChequesForm);
					AssertEquals("There should be one HotCheque in the collection", 1, hotChequesForm.HotChequeLink.HotCheques.Count);
					Assert("The collection should contain the test HotCheque", hotChequesForm.HotChequeLink.HotCheques.Contains(hotCheque));
				}
			}
		}

		#endregion

		#region SelectHotChequeImportsIntoAPInvoice Test

		public void TestSelectHotChequeImportsIntoAPInvoice()
		{
			// select hot cheque in link form and assert payment properties are set correctly
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_GB = GlbBranch.CurrentBranch.PK;
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AB = bank.PK;
			AccHotCheque hotCheque = GetNewHotCheque(org, chequeBook);
			hotCheque.AQ_ChequeNumber = "789900";
			hotCheque.AQ_Amount = 7799.90m;

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_OH = org.PK;
			using (ZFormModaliser.SuspendDispose())
			using (MockAPInvoiceForm form = new MockAPInvoiceForm(invoice))
			{
				form.Show();
				form.APInvoice.SubmittedFromInvoicingForm = true;
				form.APInvoice.IsInvoiceReceiptPayment = true;
				Application.DoEvents();
				HotChequeLinkFormTest.MockHotChequeLinkForm hotChequesForm =
					ZFormModaliser.LastFormShownDialogForTest as HotChequeLinkFormTest.MockHotChequeLinkForm;
				AssertNotNull(hotChequesForm);
				hotChequesForm.Show();
				AssertEquals("There should be 1 HotCheque in the collection", 1, hotChequesForm.HotChequeLink.HotCheques.Count);
				hotChequesForm.HotChequeGrid_Exposed.ListManager.Position = 1;
				hotChequesForm.HandleDoubleClick_Exposed(hotChequesForm, EventArgs.Empty);

				AssertEquals("Payment type should be Cheque", ReceiptTypes.Cheque, form.APInvoice.ReceiptPaymentAH_ReceiptType);
				AssertEquals("Payment Bank account should be TestBank", bank.PK, form.APInvoice.ReceiptPaymentAH_AB);
				AssertEquals("Payment Cheque book should be TestChequeBook", chequeBook.PK, form.APInvoice.ReceiptPaymentAK_AB);
				AssertEquals("Payment cheque number should be 789900", "789900", form.APInvoice.ReceiptPaymentAH_ChequeOrReference);
				AssertEquals("Payment amount should be 7799.90", 7799.90m, form.APInvoice.ReceiptPaymentAH_OSTotalAmount);
			}
		}

		#endregion

		#region NotifyUserPaymentUneditable

		public void TestNotifyUserPaymentUneditable()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AB = bank.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 100;
			AccHotCheque hotCheque = GetNewHotCheque(org, chequeBook);
			hotCheque.AQ_ChequeNumber = "000089";
			hotCheque.AQ_Amount = 10m;

			using (MockAPInvoiceForm form = new MockAPInvoiceForm(Factory.New<APInvoice>()))
			{
				form.Show();
				form.APInvoice.ImportSelectedHotCheque(hotCheque);

				form.APInvoice.ReceiptPaymentAH_ChequeOrReference = "000090";
				Assert("Popup information message should be shown", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Information text should be as follows", APInvoice.HotChequeErrorMessages.AH_ChequeOrReferenceError, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.APInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cash;
				Assert("Popup information message should be displayed", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Information text should relate be as follows", APInvoice.HotChequeErrorMessages.AH_ReceiptTypeError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		public override void TestOnLoad_FinalFlagVisibility()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				foreach (ZGridColumnInfo columnStyle in form.InvoiceDetails.TransactionLinesGrid.ColumnStyles)
				{
					if (columnStyle.ColumnName == Business.ARAP.Invoicing.InvoiceLine.Schema.AL_IsFinalCharge)
					{
						AssertEquals("IsVisible", true, columnStyle.IsVisible);
						break;
					}
				}
			}

			invoice.IsReverseTransaction = true;
			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				foreach (ZGridColumnInfo columnStyle in form.InvoiceDetails.TransactionLinesGrid.ColumnStyles)
				{
					if (columnStyle.ColumnName == Business.ARAP.Invoicing.InvoiceLine.Schema.AL_IsFinalCharge)
					{
						AssertEquals("IsVisible", false, columnStyle.IsVisible);
						break;
					}
				}
			}

			invoice.IsReverseTransaction = false;
			Factory.Save();
			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				foreach (ZGridColumnInfo columnStyle in form.InvoiceDetails.TransactionLinesGrid.ColumnStyles)
				{
					if (columnStyle.ColumnName == Business.ARAP.Invoicing.InvoiceLine.Schema.AL_IsFinalCharge)
					{
						AssertEquals("IsVisible", false, columnStyle.IsVisible);
						break;
					}
				}
			}
		}

		public void TestDeleteConsolApportionmentRowsFromGrid()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			var creator = new TestObjectCreator(Factory);
			var invoice = Factory.New<APInvoice>();

			try
			{
				using (var form = new InvoiceForm(invoice))
				{
					form.Show();
					var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
					cost.E6_AC_ChargeCode = creator.CC1.PK;
					cost.E6_OSCostAmount = 100m;
					cost.E6_ApportionmentMethod = "SHP";
					cost.SetIsUsedForApportionment();
					invoice.ImportAllApportionmentsFromCosting();

					MethodInfo handleDeleteInfo = typeof(ZGrid).GetMethod("HandleDelete", BindingFlags.Instance | BindingFlags.NonPublic);
					AssertNotNull(handleDeleteInfo);
					form.InvoiceDetails.TransactionLinesGrid.Select(0);
					handleDeleteInfo.Invoke(form.InvoiceDetails.TransactionLinesGrid, new object[] { 1 });

					AssertEquals("Should show message saying that all apportionment related lines must be selected",
						"This line relates to an apportionment of cost. All lines relating to this apportionment must also be deleted. Would you like to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Should show question to user with yes and no buttons", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				}
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		public override void TestApportionChargesButtonEnabled()
		{
			using (InvoiceForm form = (InvoiceForm)GetFormToBashCore())
			{
				form.Show();
				Assert("Should have apportion button visible", form.InvoiceDetails.ApportionChargesButton.Visible);
				Assert("Should have apportion button enabled", form.InvoiceDetails.ApportionChargesButton.Enabled);
			}
		}

		public void TestPromptToPrintSelfBillingInvoice()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			OrgHeader organisation = testObjectCreator.AALSHI;
			organisation.CompanyData.OB_IsCreditor = true;
			organisation.CompanyData.SetAPTaxApplicable(false);
			organisation.CompanyData.OB_APCostsSelfBilled = true;
			organisation.CompanyData.FillWithValidTestData();
			Factory.Save();

			// new invoice should also prompt
			ZController controller = ZControllerFactory.Create(ControllerIDs.APInvoice);
			using (InvoiceForm form = (InvoiceForm)controller.ShowNewForm())
			{
				AssertEquals("IsPostOnly", false, form.IsPostOnly);
				APInvoice aPInv = (APInvoice)form.BusinessEntity;
				aPInv.FillWithValidTestData();
				aPInv.AH_OH = organisation.PK;
				APInvoiceLine aPInvLine = (APInvoiceLine)aPInv.Lines.AddNew();
				var chargeList = aPInvLine.ChargeList;
				chargeList.Load();
				aPInvLine.GenericCharge = chargeList[0].PK;
				aPInvLine.AL_OSExTaxAmount = 10m;

				Assert("should be self billing invoice", aPInv.IsSelfBillingInvoice);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSave_ForTestOnly();

				Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print invoice", "Do you want to print Self Billing Invoice SB00001000?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPromptToPrintIncompleteInvoice()
		{
			new AccountingPeriodTestHelper(new BusinessObjectFactory()).SetupPeriods();
			AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = testObjectCreator.AALSHI.PK;
			APInvoiceLine invoiceLine = (APInvoiceLine)invoice.Lines.AddNew();
			var chargeList = invoiceLine.ChargeList;
			chargeList.Load();
			invoiceLine.GenericCharge = chargeList[0].PK;
			invoiceLine.AL_AT = testObjectCreator.GST1.PK;
			invoiceLine.AL_OSExTaxAmount = 10m;
			invoice.SaveAsIncomplete();

			invoice = new BusinessObjectFactory().Load<APInvoice>(invoice.PK);
			invoice.SubmittedFromInvoicingForm = true;
			invoice.RestoreSavedData();

			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSave_ForTestOnly();

				Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print invoice", "Do you want to print a Cost Confirmation Document for this transaction?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPromptToPrintCostConfirmationDocumentInvoice()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			ZController controller = ZControllerFactory.Create(ControllerIDs.APInvoice);
			using (InvoiceForm form = (InvoiceForm)controller.ShowNewForm())
			{
				AssertEquals("IsPostOnly", false, form.IsPostOnly);
				APInvoice aPInv = (APInvoice)form.BusinessEntity;
				aPInv.FillWithValidTestData();
				aPInv.AH_OH = testObjectCreator.AALSHI.PK;
				APInvoiceLine aPInvLine = (APInvoiceLine)aPInv.Lines.AddNew();
				var chargeList = aPInvLine.ChargeList;
				chargeList.Load();
				aPInvLine.GenericCharge = chargeList[0].PK;
				aPInvLine.AL_OSExTaxAmount = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSave_ForTestOnly();

				Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print credit note", "Do you want to print a Cost Confirmation Document for this transaction?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMessageIfChequeBookUsesSamePrinterShown()
		{
			var testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_QueueName = "Queue Name";

			var testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_GB = GlbBranch.CurrentBranch.PK;

			var chequeBook1 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook1.AK_AB = testBank.PK;
			chequeBook1.AK_GB = GlbBranch.CurrentBranch.PK;
			chequeBook1.AK_Code = "Book1";
			chequeBook1.AK_AutoPrintCheque = ZBool.True;
			chequeBook1.AK_SQ = printer.PK;

			var chequeBook2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook2.AK_AB = testBank.PK;
			chequeBook2.AK_GB = chequeBook1.AK_GB;
			chequeBook2.AK_Code = "Book2";
			chequeBook2.AK_AutoPrintCheque = ZBool.True;
			chequeBook2.AK_SQ = printer.PK;
			Factory.Save();

			var newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;

			using (var form = new InvoiceForm(newInvoice))
			{
				form.Show();

				newInvoice.AH_OH = TestDataCreator.AALSHI.PK;
				newInvoice.AH_InvoiceDate = ZDateTime.Now;
				newInvoice.AH_TransactionNum = "888888888";
				newInvoice.AH_GB = chequeBook1.AK_GB;
				newInvoice.SubmittedFromInvoicingForm = true;
				newInvoice.IsInvoiceReceiptPayment = true;
				newInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
				newInvoice.ReceiptPaymentAH_AB = chequeBook1.AK_AB;
				newInvoice.ReceiptPaymentAK_AB = chequeBook1.PK;

				var line = (APInvoiceLine)newInvoice.Lines.AddNew();
				var chargeList = line.ChargeList;
				chargeList.Load();

				var genericCharge = chargeList.Find(c => !string.IsNullOrEmpty(c.VC_Description)).FirstOrDefault();
				AssertNotNull("WI00835004: A valid charge with a non-blank description must be picked from the ChargeList", genericCharge);
				line.GenericCharge = genericCharge.PK;
				line.AL_AT = new TestObjectCreator(Factory).GST1.PK;
				line.AL_OSExTaxAmount = 100m;
				line.AL_Desc = "Test Desc";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				var lastUserMessageReceived = UnitTestUserNotification.Instance.LastMessage.Text;
				var expectedLastUserMessage = AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, chequeBook1.AK_CurrentNo);
				if (lastUserMessageReceived == expectedLastUserMessage)
				{
					Assert(true);
				}
				else
				{
					var errorsOnInvoice = newInvoice.NotificationsIncludingChildren.Where(x => x.Type == CargoWise.ComponentModel.NotificationType.Error).Select(x => x.Message);
					var errorsFormattedForDisplay = new ZStringBuilder(errorsOnInvoice).ToStringWithNewLineBetweenAppends();
					Fail($"""
						Expected ChequeBookWithSamePrinterMessage but received [{lastUserMessageReceived}].
						Errors on Invoice:
						{errorsFormattedForDisplay}
						Line Generic Charge PK: {line.GenericCharge.ToString()}
						""");
				}
			}
		}

		public void TestChequeNumberOnNewlyCreatedPayment()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_QueueName = "Queue Name";
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;

			AccChequeBook chequeBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 1000;
			chequeBook.AK_CurrentNo = 100;

			ForwardingShipment shipment = TestDataCreator.CreateShipment("S1");
			Job job = TestDataCreator.CreateJob(shipment);

			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			newInvoice.AH_OH = TestDataCreator.AALSHI.PK;
			APInvoiceLine line1 = (APInvoiceLine)newInvoice.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100M;
			line1.GenericCharge = TestDataCreator.CC1.PK;
			line1.AL_AC = TestDataCreator.CC1.PK;
			line1.AL_JH = job.PK;

			var charge = TestDataCreator.CreateJobCharge(line1, job, TestDataCreator.CC1, newInvoice.TransactionCurrency);

			Factory.Save();
			using (InvoiceForm form = new InvoiceForm(newInvoice))
			{
				form.Show();

				newInvoice.SubmittedFromInvoicingForm = true;
				newInvoice.AH_InvoiceDate = ZDateTime.Now;
				newInvoice.AH_OH = TestDataCreator.AALSHI.PK;
				newInvoice.AH_TransactionNum = "888888888";
				newInvoice.IsInvoiceReceiptPayment = true;
				newInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
				newInvoice.ReceiptPaymentAH_AB = chequeBook.AK_AB;
				newInvoice.ReceiptPaymentAK_AB = chequeBook.PK;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("Created Payment must have autoallocated cheque num ", "000100", newInvoice.ReceiptPaymentAH_ChequeOrReference);
				if (ZFormModaliser.LastFormShownDialogForTest != null)
				{
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
				}
			}
		}

		public void TestChequeNumberNotIncrementedWhenCashInvoiceIsUnticked()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_QueueName = "Queue Name";
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;

			AccChequeBook chequeBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 1000;
			chequeBook.AK_CurrentNo = 100;

			ForwardingShipment shipment = TestDataCreator.CreateShipment("S1");
			Job job = TestDataCreator.CreateJob(shipment);

			APInvoice newInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			newInvoice.AH_OH = TestDataCreator.AALSHI.PK;
			APInvoiceLine line1 = (APInvoiceLine)newInvoice.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100M;
			line1.GenericCharge = TestDataCreator.CC1.PK;
			line1.AL_AC = TestDataCreator.CC1.PK;
			line1.AL_JH = job.PK;

			var charge = TestDataCreator.CreateJobCharge(line1, job, TestDataCreator.CC1, newInvoice.TransactionCurrency);

			Factory.Save();
			using (InvoiceForm form = new InvoiceForm(newInvoice))
			{
				form.Show();

				newInvoice.SubmittedFromInvoicingForm = true;
				newInvoice.AH_InvoiceDate = ZDateTime.Now;
				newInvoice.AH_OH = TestDataCreator.AALSHI.PK;
				newInvoice.AH_TransactionNum = "888888888";
				newInvoice.IsInvoiceReceiptPayment = true;
				newInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
				newInvoice.ReceiptPaymentAK_AB = chequeBook.PK;

				newInvoice.IsInvoiceReceiptPayment = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("No receipt payment created", null, newInvoice.ReceiptPayment);

				chequeBook.Reload();
				AssertEquals("Current number should not be updated on chequebook", 100m, chequeBook.AK_CurrentNo);
				AssertEquals("Cheque book should still be active", true, chequeBook.AK_IsActive);

				if (ZFormModaliser.LastFormShownDialogForTest != null)
				{
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
				}
			}
		}

		public void TestPaymentPrintingShownWhenInvoicePosted()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();

			APInvoiceLine line1 = (APInvoiceLine)aPInv.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100M;
			line1.GenericCharge = Factory.LoadTop1<Business.GenericCharge.GenericCharge>(line1.ChargeList.CompleteFilter).PK;
			line1.AL_AT = TestDataCreator.GST1.PK;

			using (InvoiceForm form = new InvoiceForm(aPInv))
			{
				form.Show();

				aPInv.AH_OH = TestDataCreator.AALSHI.PK;
				aPInv.SubmittedFromInvoicingForm = true;
				aPInv.IsInvoiceReceiptPayment = true;
				aPInv.AH_Desc = "Description";
				aPInv.ReceiptPaymentAH_AB = TestDataCreator.AUDBankAccount.PK;
				aPInv.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
				aPInv.ReceiptPaymentAH_ChequeOrReference = "1";
				aPInv.ReceiptPaymentAH_DrawerBank = "NAB";
				aPInv.ReceiptPaymentAH_DrawerBranch = "Branch";
				aPInv.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				aPInv.ReceiptPaymentAK_AB = TestDataCreator.AUDChequeBook.PK;
				aPInv.AH_OutstandingAmount = -110M;

				form.FireSaveButton();
				Assert("The flag should not be set on print manager", !form.Test_ChequeIsAutoPrintedSetOnPrintManager);

				PaymentDocumentsPrintPopup remittanceForm = null;
				try
				{
					remittanceForm = ZFormModaliser.LastFormShownDialogForTest as PaymentDocumentsPrintPopup;
					AssertNotNull("Remittance Form should be instantiated", remittanceForm);
				}
				finally
				{
					if (remittanceForm != null)
					{
						remittanceForm.Dispose();
					}
				}
			}
		}

		public void TestApportionButtonDisabledOnEditing()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "TEST0001";
			invoice.Factory.Save();
			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert("Apportion button should not be visible", !form.InvoiceDetails.ApportionChargesButton.Visible);
			}
		}

		public void TestApportionButtonDisabledOnReversing()
		{
			APCreditNote creditNote = Factory.New<APCreditNote>();
			creditNote.AH_TransactionNum = "TEST0001";
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.APCreditNote);
			using (InvoiceForm form = (InvoiceForm)controller.ShowDeleteForm(creditNote))
			{
				form.Show();
				Application.DoEvents();
				Assert("Apportion button should not be visible", !form.InvoiceDetails.ApportionChargesButton.Visible);
			}
		}

		public void TestAutoAllocateAndPrintReceiptPayment()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			AccChequeBook chequeBook = GetAutoPrintChequeBook(1, 3, 3, TestDataCreator.AUDChequeBook, TestDataCreator.AUDBankAccount);
			APInvoice testInvoice = Factory.NewWithValidTestData<APInvoice>();
			testInvoice.SubmittedFromInvoicingForm = true;
			testInvoice.IsInvoiceReceiptPayment = true;
			APInvoiceLine line1 = (APInvoiceLine)testInvoice.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100M;
			line1.GenericCharge = Factory.LoadTop1<Business.GenericCharge.GenericCharge>(line1.ChargeList.CompleteFilter).PK;
			line1.AL_AT = TestDataCreator.GST1.PK;

			using (InvoiceForm form = new InvoiceForm(testInvoice))
			{
				form.Show();
				testInvoice.AH_OH = TestDataCreator.AALSHI.PK;
				testInvoice.AH_Desc = "Description";
				testInvoice.IsInvoiceReceiptPayment = true;
				testInvoice.ReceiptPaymentAH_AB = TestDataCreator.AUDBankAccount.PK;
				testInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
				testInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
				testInvoice.ReceiptPaymentAH_DrawerBank = "NAB";
				testInvoice.ReceiptPaymentAH_DrawerBranch = "Branch";
				testInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				testInvoice.ReceiptPaymentAK_AB = TestDataCreator.AUDChequeBook.PK;
				Assert("Cheque number should be empty", testInvoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
				Assert("Auto allocation should be enabled", ((IChequeNumberAutoAllocation)testInvoice).IsAutoAllocationEnabled);

				form.FireSaveButton();
				Assert("Cheque should be auto printed", ((IChequeNumberAutoAllocation)testInvoice).ChequeIsAutoPrinted);
				Assert("Cheque number should be auto allocated", ((IChequeNumberAutoAllocation)testInvoice).IsAllocationPerformed);
				AssertEquals("Cheque number should be updated on invoice", "3", testInvoice.ReceiptPayment.AH_ChequeOrReference);
				AssertEquals("Cheque number should be updated on ReceiptPayment", "3", testInvoice.ReceiptPaymentAH_ChequeOrReference);
				Assert("The flag should have been set on print manager", form.Test_ChequeIsAutoPrintedSetOnPrintManager);

				chequeBook.Reload();
				AssertEquals("Current number should be updated on chequebook", 4m, chequeBook.AK_CurrentNo);
				Assert("Cheque book should become inactive", !chequeBook.AK_IsActive);

				if (ZFormModaliser.LastFormShownDialogForTest != null)
				{
					ZFormModaliser.LastFormShownDialogForTest.Dispose();
				}
			}
		}

		public void TestAutoAllocationAndPrintChequesErrorInTransaction()
		{
			(new AccountingPeriodTestHelper()).SetupPeriods();
			var chequeBook = GetAutoPrintChequeBook(1, 3, 3, TestDataCreator.AUDChequeBook, TestDataCreator.AUDBankAccount);
			chequeBook.AK_Desc = "TestAutoAllocationAndPrintCheques";
			Factory.Save();

			var testInvoice = Factory.NewWithValidTestData<APInvoice>();
			testInvoice.SubmittedFromInvoicingForm = true;
			testInvoice.IsInvoiceReceiptPayment = true;
			testInvoice.AH_OH = TestDataCreator.AALSHI.PK;
			testInvoice.AH_Desc = "Description";
			testInvoice.IsInvoiceReceiptPayment = true;
			testInvoice.ReceiptPaymentAH_AB = TestDataCreator.AUDBankAccount.PK;
			testInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
			testInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
			testInvoice.ReceiptPaymentAH_DrawerBank = "NAB";
			testInvoice.ReceiptPaymentAH_DrawerBranch = "Branch";
			testInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;
			testInvoice.ReceiptPaymentAK_AB = TestDataCreator.AUDChequeBook.PK;

			var line1 = (APInvoiceLine)testInvoice.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100M;
			line1.GenericCharge = Factory.LoadTop1<Business.GenericCharge.GenericCharge>(line1.ChargeList.CompleteFilter).PK;
			line1.AL_AT = TestDataCreator.GST1.PK;

			using (var form = new InvoiceForm(testInvoice))
			{
				form.Show();
				Assert("Error shown to user", PaymentDocumentsPrinter.PerformTestAutoAllocationAndPrintChequesFailure(() => form.FireSaveButton()));
			}
		}

		[ExpectNoExceptions()]
		public void TestExceptionIsHandled_ChequeBookIsFull()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			ForwardingShipment shipment = TestDataCreator.CreateShipment("S1");
			Job job = TestDataCreator.CreateJob(shipment, false);

			AccChequeBook chequeBook = GetAutoPrintChequeBook(1, 3, 3, TestDataCreator.AUDChequeBook, TestDataCreator.AUDBankAccount);
			APInvoice testInvoice = Factory.NewWithValidTestData<APInvoice>();
			InvoicingLineBase line1 = TestDataCreator.CreateInvoiceLine(testInvoice, testInvoice.TransactionCurrency, testInvoice.AH_ExchangeRate, 100m, 0m, 0m, TestDataCreator.CC1.PK);
			line1.AL_JH = job.PK;
			var charge = TestDataCreator.CreateJobCharge(line1, job, TestDataCreator.CC1, testInvoice.TransactionCurrency);

			Factory.Save();

			using (InvoiceForm form = new InvoiceForm(testInvoice))
			{
				form.Show();
				testInvoice.AH_OH = TestDataCreator.AALSHI.PK;
				testInvoice.AH_Desc = "Description";
				testInvoice.SubmittedFromInvoicingForm = true;
				testInvoice.IsInvoiceReceiptPayment = true;
				testInvoice.ReceiptPaymentAH_AB = TestDataCreator.AUDBankAccount.PK;
				testInvoice.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
				testInvoice.ReceiptPaymentAH_ChequeOrReference = "1";
				testInvoice.ReceiptPaymentAH_DrawerBank = "NAB";
				testInvoice.ReceiptPaymentAH_DrawerBranch = "Branch";
				testInvoice.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				testInvoice.ReceiptPaymentAK_AB = TestDataCreator.AUDChequeBook.PK;
				form.Test_DeactivateChequeBookOnAllocation = ZBool.True;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Cheque number should be empty", testInvoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
				Assert("Auto allocation should be enabled", ((IChequeNumberAutoAllocation)testInvoice).IsAutoAllocationEnabled);
				form.FireSaveButton();
				AssertEquals("There should be a message shown, saying that the Cheque Book is full.", ErrorMessages.ChequeBookIsFullExceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Cheque should not be auto printed", !((IChequeNumberAutoAllocation)testInvoice).ChequeIsAutoPrinted);
				Assert("Cheque number should not be auto allocated", !((IChequeNumberAutoAllocation)testInvoice).IsAllocationPerformed);
				Assert("Cheque number should be empty", testInvoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
				AssertNull("ReceiptPayment object should be empty", testInvoice.ReceiptPayment);

				chequeBook.Reload();
				AssertEquals("Cheque book should not be incremented", 3m, chequeBook.AK_CurrentNo);
			}
		}

		public void TestEDIMessagesPluginAttached()
		{
			using (InvoiceForm form = (InvoiceForm)GetFormToBashCore())
			{
				Assert("Form should have the plugin attached", form.PlugIns.GetPlugIn(ControllerIDs.LinkedeNettEDIMessage) != null);
			}
		}

		public void TestPostCashInvoiceWithCreditCardPaymentViaENett()
		{
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);
			new AccountingPeriodTestHelper().SetupPeriods();
			bool originalComPayEnabled = AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value;

			try
			{
				AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				APInvoice invoice = Factory.New<APInvoice>();
				using (InvoiceForm form = new InvoiceForm(invoice))
				{
					form.Show();

					OrgHeader orgHeader = TestObjectCreator.AALSHI;
					OrgCusCode cusCode = orgHeader.CustomsCodes.AddNew();
					cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
					cusCode.OK_CustomsRegNo = "123456";
					cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					AssertEquals("eNettRegistrationNumber", "123456", orgHeader.ENettRegistrationNumber);

					invoice.AH_GC = GlbCompany.CurrentCompany.PK;
					invoice.AH_OH = orgHeader.PK;
					invoice.AH_InvoiceDate = ZDateTime.Now;
					invoice.AH_PostDate = ZDateTime.Now;
					invoice.AH_DueDate = ZDateTime.Now;
					invoice.AH_ExchangeRate = 1.0m;
					invoice.AH_OSExTaxAmount = 40.00m;
					invoice.AH_TransactionNum = "00001111";

					AccGLHeader genericChargeHeader = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "4710.00.00"));

					APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
					line.GenericCharge = genericChargeHeader.PK;
					line.AL_GB = GlbBranch.CurrentBranch.PK;
					line.AL_GE = GlbDepartment.CurrentDepartment.PK;
					line.AL_OSExTaxAmount = 40.00m;
					line.AL_AT = objectCreator.GST1.PK;

					invoice.SubmittedFromInvoicingForm = true;
					invoice.IsInvoiceReceiptPayment = true;

					var header = Factory.NewWithValidTestData<AccGLHeader>();
					header.AG_AccountNum = "ZZAUDAcc";
					var bankAccount = objectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", objectCreator.AUD, "123456", "12345678", header);
					bankAccount.AB_DebitCreditCardExpiry = "0699";
					bankAccount.AB_DebitCreditCardName = "MR JOHN SMITH";
					var encoder = new TwoWayEncoder(bankAccount.PK.ToGuid());
					bankAccount.AB_DebitCreditCardNumber = encoder.Encrypt("1234567812345678");
					bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
					bankAccount.AB_AccountNum = "**** **** ***4 5678";

					invoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.eNettCreditCard;
					invoice.ReceiptPaymentAH_AB = bankAccount.PK;
					invoice.ReceiptPaymentAH_ChequeOrReference = "111";
					invoice.ReceiptPaymentCardSecurityCode = "123";

					invoice.RunPreSaveValidation();
					AssertEquals("APInvoice should not have errors. Errors: " + invoice.NotificationsIncludingChildren.ToUniqueMessageListString(), false, invoice.HasErrors);

					int initialInvokedCount = MockENettWebService.Instance.CountProcessCreditCardWasInvoked;
					eNettWebServiceWrapper.UseRealWebService_ForTesting = false;
					MockENettWebService.Instance.SetupForTesting("CARGOWISE");

					form.FireSaveButton();

					BusinessObjectFactory newFactory = new BusinessObjectFactory();
					APInvoice postedInvoice = newFactory.Load<APInvoice>(invoice.PK);
					AssertNotNull("Invoice should be in database", postedInvoice);
					AssertNotNull("Payment should not be null", invoice.ReceiptPayment);

					APPayment payment = newFactory.Load<APPayment>(invoice.ReceiptPayment.PK);
					AssertNotNull("Payment should be in database", payment);

					AssertEquals("ProcessCreditCard should have been called", 1, MockENettWebService.Instance.CountProcessCreditCardWasInvoked - initialInvokedCount);
				}
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalComPayEnabled);
			}
		}

		[TestDate(2018, 06, 07)]
		public void TestEditApportionment_CostIsNull()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var job1 = TestObjectCreator.CreateJob(shipment1);
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			using (var invoiceForm = new InvoiceForm(invoice))
			{
				invoiceForm.Show();

				//Set up the test data
				invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
				AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
				var consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
				AssertNotNull(consolCostingForm);
				var cost1 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost1.E6_AC_ChargeCode = TestObjectCreator.CC12.PK;
				cost1.E6_OSCostAmount = 70M;
				Assert(!cost1.HasErrors);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				consolCostingForm.Close();
				AssertEquals("LastMessage.Text", "Do you want to apply changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				Application.DoEvents();

				AssertEquals("There should be 1 line in the AP Invoice.", 1, invoice.Lines.Count);

				invoice.ConsolCosting.ConsolCosts.Remove(cost1);
				invoiceForm.InvoiceDetails.TransactionLinesGrid.Select(0);
				var menuItem = invoiceForm.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
				AssertNotNull(menuItem);
				menuItem.PerformClick();

				var expectMsg = GetDeveloperMessage("Cannot find the consol cost.", cost1, invoice);
				AssertContains("Cost is null", expectMsg, ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
			}
		}

		[TestDate(2018, 06, 07)]
		public void TestEditApportionment_ParentAPInvoiceIsNull()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var job1 = TestObjectCreator.CreateJob(shipment1);
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			using (var invoiceForm = new InvoiceForm(invoice))
			{
				invoiceForm.Show();

				//Set up the test data
				invoiceForm.InvoiceDetails.ApportionChargesButton.PerformClick();
				AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingForm), ZFormModaliser.ActiveForm.GetType());
				var consolCostingForm = (APInvoiceConsolCostingForm)ZFormModaliser.ActiveForm;
				AssertNotNull(consolCostingForm);
				var cost1 = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost1.E6_AC_ChargeCode = TestObjectCreator.CC12.PK;
				cost1.E6_OSCostAmount = 70M;
				Assert(!cost1.HasErrors);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				consolCostingForm.Close();
				AssertEquals("LastMessage.Text", "Do you want to apply changes?", UnitTestUserNotification.Instance.LastMessage.Text);
				Application.DoEvents();

				AssertEquals("There should be 1 line in the AP Invoice.", 1, invoice.Lines.Count);

				cost1.ParentAPInvoice = null;
				invoiceForm.InvoiceDetails.TransactionLinesGrid.Select(0);
				var menuItem = invoiceForm.InvoiceDetails.TransactionLinesGrid.ContextMenu.MenuItems.FindByText("Edit Apportionment");
				AssertNotNull(menuItem);
				menuItem.PerformClick();

				var expectMsg = GetDeveloperMessage("ParentAPInvoice is null.", cost1, invoice);
				AssertContains("Parent AP Invoice is null", expectMsg, ErrorReporter.LastMessageReported);

				AssertEquals("Should be showing the right form.", typeof(APInvoiceConsolCostingSingleEditForm), ZFormModaliser.ActiveForm.GetType());
				ErrorReporter.Clear();
			}
		}

		string GetDeveloperMessage(string title, JobConsolCost cost, APInvoice invoice)
		{
			var line = invoice.Lines[0];
			var charge = cost.ApportionmentCharges[0];
			var costParentCollections = ((IBusinessObjectInternals)cost).ParentCollections;
			var message = string.Empty;
			if (costParentCollections.Length > 0)
			{
				message = string.Format(@"{0}
Consol cost PK: '{1}'
Current line:
Line: PK = {2}, Charge Code = ZZCC12, GL Account = {8}, Type = CST, OS Amount = -77.0, Local Amount = -70, GST = -7.0, Tax Rate = ZZGST1, Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {3}, Job PK = {4}, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = Yes, Sub Accounts = , Has Changes = Yes.
Related charge:
Charge: PK = {5}, Type = ApportionSplitCharge|Charge, Charge Type = , Job PK = {4}, Job Number = S00001000, Charge Code = ZZCC12, Charge Code Type = MRG, Cost Account = AALSHI, OS Cost Amount = 70, Local Cost Amount = 70, OS Cost Exchange Rate = 1, Cost GST is Overridden = Yes, OS Cost GST Amount = 7.0, OS Cost WHT Amount = 0, AP Invoice # = {7}, AP Invoice Date = {9}, Supplier Cost Reference = , Payment Date = {9}, Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = {1}, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = AUD, Is In DB = No, Has Changes = Yes, Is Saved By Factory = Yes, Cost GST Rate = ZZGST1, Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , Cost Tax Date = 07-Jun-18 00:00:00, Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = Factory Level : (APInvoiceForm).
Related Consol Cost:
Job Consol Cost:
	PK = {1}
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {12}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = Factory Level : (APInvoiceForm),BizObj Level : (EnableDirectSettingConsolCostParent)

	Charge Code = ZZCC12, Invoice # = {7}, Invoice Date = {9}, Currency = AUD, OS Cost Amount = 70, GST is Overridden = Yes, OS GST Amount = 7.0, Exchange Rate = 1, Local Cost Amount = 70, Creditor = AALSHI, PPDCLT = ALL, Apportionment Method = CHG, Supplier Cost Reference = , Payment Date = {9}, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = {10}, Tax Date = 07-Jun-18 00:00:00, AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = {6}, Is Final = Yes, Tax Code = {11}, Supply Type = {14}, Tax Branch = {15}.
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.Accounting.Business.ARAP.Invoicing.APInvoiceConsolCostCollection
	Element Type = Enterprise.Accounting.Business.ConsolCosting.JobConsolCost
	Factory Instance = {12}
	Hash Code = {13}
	Contains bizo = True
	Has Changes = True
	Number of elements = 1
	Is List Changed Suspended = False
	Has Changes From Delete = False
	Masters Are Deleted = False
	Masters Are In Database  = True
Apportionment Charges (1):
Charge: PK = {5}, Type = ApportionSplitCharge|Charge, Charge Type = , Job PK = {4}, Job Number = S00001000, Charge Code = ZZCC12, Charge Code Type = MRG, Cost Account = AALSHI, OS Cost Amount = 70, Local Cost Amount = 70, OS Cost Exchange Rate = 1, Cost GST is Overridden = Yes, OS Cost GST Amount = 7.0, OS Cost WHT Amount = 0, AP Invoice # = {7}, AP Invoice Date = {9}, Supplier Cost Reference = , Payment Date = {9}, Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = {1}, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = AUD, Is In DB = No, Has Changes = Yes, Is Saved By Factory = Yes, Cost GST Rate = ZZGST1, Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , Cost Tax Date = 07-Jun-18 00:00:00, Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = Factory Level : (APInvoiceForm).",
title, cost.PK, line.PK, invoice.PK, line.Job.PK, charge.PK, cost.E6_ParentID, invoice.AH_TransactionNum, line.GLHeader.AG_AccountNum, invoice.AH_InvoiceDate, cost.E6_AT_TaxRate, cost.TaxRate == null ? ZString.Empty : cost.TaxRate.AT_Code, cost.Factory._Instance, ((IBusinessObjectInternals)cost).ParentCollections[0].GetHashCode(), cost.E6_SupplyType, cost.CostTaxBranch?.GB_Code ?? ZString.Empty);
			}
			else
			{
				message = string.Format(@"{0}
Consol cost PK: '{1}'
Current line:
Line: PK = {2}, Charge Code = ZZCC12, GL Account = {8}, Type = CST, OS Amount = -77.0, Local Amount = -70, GST = -7.0, Tax Rate = ZZGST1, Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {3}, Job PK = {4}, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = Yes, Sub Accounts = , Has Changes = Yes.
Related charge:
Charge: PK = {5}, Type = ApportionSplitCharge|Charge, Charge Type = , Job PK = {4}, Job Number = S00001000, Charge Code = ZZCC12, Charge Code Type = MRG, Cost Account = AALSHI, OS Cost Amount = 70, Local Cost Amount = 70, OS Cost Exchange Rate = 1, Cost GST is Overridden = Yes, OS Cost GST Amount = 7.0, OS Cost WHT Amount = 0, AP Invoice # = {7}, AP Invoice Date = {9}, Supplier Cost Reference = , Payment Date = {9}, Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = {1}, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = AUD, Is In DB = No, Has Changes = Yes, Is Saved By Factory = Yes, Cost GST Rate = ZZGST1, Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , Cost Tax Date = 07-Jun-18 00:00:00, Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = Factory Level : (APInvoiceForm).
Related Consol Cost:
Job Consol Cost:
	PK = {1}
	Type = JobConsolCost
	Types around row = JobConsolCost
	Factory Instance = {12}
	IsDeleted = False
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = Factory Level : (APInvoiceForm),BizObj Level : (EnableDirectSettingConsolCostParent)

	Charge Code = ZZCC12, Invoice # = {7}, Invoice Date = {9}, Currency = AUD, OS Cost Amount = 70, GST is Overridden = Yes, OS GST Amount = 7.0, Exchange Rate = 1, Local Cost Amount = 70, Creditor = AALSHI, PPDCLT = ALL, Apportionment Method = CHG, Supplier Cost Reference = , Payment Date = {9}, Payment Type = , Cheque # = , Bank Account = 00000000-0000-0000-0000-000000000000, Cheque Book = 00000000-0000-0000-0000-000000000000, Tax Rate = {10}, Tax Date = 07-Jun-18 00:00:00, AR Invoice = 00000000-0000-0000-0000-000000000000, AP Invoice = 00000000-0000-0000-0000-000000000000, Is For Collect Invoice = N, Consol = {6}, Is Final = Yes, Tax Code = {11}, Supply Type = {13}, Tax Branch = {14}.
Parent collections:
Apportionment Charges (1):
Charge: PK = {5}, Type = ApportionSplitCharge|Charge, Charge Type = , Job PK = {4}, Job Number = S00001000, Charge Code = ZZCC12, Charge Code Type = MRG, Cost Account = AALSHI, OS Cost Amount = 70, Local Cost Amount = 70, OS Cost Exchange Rate = 1, Cost GST is Overridden = Yes, OS Cost GST Amount = 7.0, OS Cost WHT Amount = 0, AP Invoice # = {7}, AP Invoice Date = {9}, Supplier Cost Reference = , Payment Date = {9}, Payment Type = , Cheque # = , Cheque Book = 00000000-0000-0000-0000-000000000000, Cheque Book Code = , Bank Account = 00000000-0000-0000-0000-000000000000, Is Cost Posted = No, AP Line = 00000000-0000-0000-0000-000000000000, Sell Account = , OS Sell Exchange Rate = 1, OS Sell Amount = 0, Local Sell Amount = 0, OS Sell GST Amount = 0, OS Sell WHT Amount = 0, Is Revenue Posted = No, AR Line = 00000000-0000-0000-0000-000000000000, CFX Line = 00000000-0000-0000-0000-000000000000, Invoice Type = , Order Reference = , OP Product = 00000000-0000-0000-0000-000000000000, Product Quantity = 0, Consol Cost = {1}, Gateway Sell Header = 00000000-0000-0000-0000-000000000000, Cost Currency = AUD, Sell Currency = AUD, Is In DB = No, Has Changes = Yes, Is Saved By Factory = Yes, Cost GST Rate = ZZGST1, Sell GST Rate = , Cost Tax Class = , Sell Tax Class = , Sell Invoice Currency = , Cost Tax Date = 07-Jun-18 00:00:00, Sell Tax Date = ,Cost Supply Type = ,Cost Tax Branch = ,Business Contexts = Factory Level : (APInvoiceForm).",
title, cost.PK, line.PK, invoice.PK, line.Job.PK, charge.PK, cost.E6_ParentID, invoice.AH_TransactionNum, line.GLHeader.AG_AccountNum, invoice.AH_InvoiceDate, cost.E6_AT_TaxRate, cost.TaxRate == null ? ZString.Empty : cost.TaxRate.AT_Code, cost.Factory._Instance, cost.E6_SupplyType, cost.CostTaxBranch?.GB_Code ?? ZString.Empty);
			}
			return message;
		}

		#region Implementation
		public override void TestPromptToPrintComplianceDocumentWhenEnablePrompt()
		{
			TestPromptToPrintComplianceDocumentCore(true, AssertForNotPromptToPrintComplianceDocument);
		}

		public override void TestPromptToPrintComplianceDocumentWhenDisablePrompt()
		{
			TestPromptToPrintComplianceDocumentCore(false, AssertForNotPromptToPrintComplianceDocument);
		}

		public AccChequeBook GetAutoPrintChequeBook(ZDecimal startNo, ZDecimal currentNo, ZDecimal lastNo)
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			StmTemplate chequeTemplate = Factory.NewWithValidTestData<StmTemplate>();
			bankAccount.AB_SO_ChequeTemplate = chequeTemplate.PK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNo;
			chequeBook.AK_CurrentNo = currentNo;
			chequeBook.AK_LastNo = lastNo;
			Assertion.Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		public AccChequeBook GetAutoPrintChequeBook(ZDecimal startNo, ZDecimal currentNo, ZDecimal lastNo, AccChequeBook chequeBook, AccBankAccount bankAccount)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNo;
			chequeBook.AK_CurrentNo = currentNo;
			chequeBook.AK_LastNo = lastNo;
			Assertion.Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			return invoice is Invoice ? new InvoiceForm(invoice) { ControllerID = ControllerIDs.APInvoice } :
														new CreditNoteForm(invoice) { ControllerID = ControllerIDs.APCreditNote };
		}

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var invoice = factory != null ? factory.New<APInvoice>() : Factory.New<APInvoice>();
			if (fillTestData)
			{
				invoice.FillWithValidTestData();
			}

			return invoice;
		}

		protected override bool ShouldShowRelatedInvoicesTab
		{
			get { return true; }
		}

		#region TestDataCreator

		TestObjectCreator TestDataCreator
		{
			get
			{
				if (fTestDataCreator == null)
				{
					fTestDataCreator = new TestObjectCreator(Factory);
				}
				return fTestDataCreator;
			}
		}
		TestObjectCreator fTestDataCreator;

		#endregion

		AccHotCheque GetNewHotCheque(OrgHeader org, AccChequeBook chequeBook)
		{
			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_OH = org.PK;
			hotCheque.AQ_AK = chequeBook.PK;
			hotCheque.AQ_Cancelled = false;
			hotCheque.AQ_AH = ZGuid.Empty;
			return hotCheque;
		}

		public class MockAPInvoiceForm : InvoiceForm
		{
			public MockAPInvoiceForm(APInvoice aPInv)
				: base(aPInv)
			{
			}

			public APInvoice APInvoice
			{
				get { return BusinessEntity as APInvoice; }
			}

			protected override HotChequeLinkForm GetHotChequeLinkForm(HotChequeLink chequeLink)
			{
				return new HotChequeLinkFormTest.MockHotChequeLinkForm(chequeLink);
			}

			public void Delete_Exposed()
			{
				base.Delete();
			}
		}

		protected override bool ShouldSupportOverrideExRateCheckbox => true;

		protected override void SetUp()
		{
			base.SetUp();
			MockENettWebService.ClearInstance();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockENettWebService.ClearInstance();
		}

		#endregion
	}
}
