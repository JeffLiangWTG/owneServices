using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	public class InvoiceBatchControllerTest : TestCaseWithFactory
	{
		public void TestFormCancelThenClose()
		{
			var controllerMock = new Mock<PeriodicInvoiceBulkPosterController>(new object[] { new PeriodicInvoiceBulkPoster(), null });
			var controller = controllerMock.Object;
			var isGuiOperationApplied = false;

			controller.ProgressForm_ForTestOnly.Shown += (s, e) =>
			{
				var cancelButton = (Button)controller.ProgressForm_ForTestOnly.CancelButton;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				cancelButton.PerformClick();
				AssertEquals("cancelProgressButton.Enabled", true, cancelButton.Enabled);
				AssertEquals("cancelProgressButton.Text", "Cancel", cancelButton.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				cancelButton.PerformClick();
				AssertEquals("cancelProgressButton.Enabled", false, cancelButton.Enabled);
				AssertEquals("cancelProgressButton.Text", "Canceling", cancelButton.Text);

				controller.UnHookEvents_ForTestOnly();
				controller.ResetProgressForm_ForTestOnly();
				AssertEquals("cancelProgressButton.Enabled", true, cancelButton.Enabled);
				AssertEquals("cancelProgressButton.Text", "Close", cancelButton.Text);

				cancelButton.PerformClick();

				isGuiOperationApplied = true;
			};

			controller.StartPosting();
			Application.DoEvents();

			AssertEquals("Ensure to run the Shown event.", true, isGuiOperationApplied);

			controller.LastProgressFormForTestOnly.Dispose();
		}

		public void TestDeliveryAllWithTAXInvoice_VN()
		{
			var controllerMock = new Mock<PeriodicInvoiceBulkPosterController>(new object[] { new PeriodicInvoiceBulkPoster(), null });
			var controller = controllerMock.Object;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.GovtTaxInvoice);
			SetupComplianceNumber();
			controllerMock.Protected()
				.Setup<YesNoYesAllNoAllMessageBoxResult>("GetUserChoice")
				.Returns(YesNoYesAllNoAllMessageBoxResult.YesToAll);
			controller.Poster_PostingStarted_ForTestOnly(2);
			controller.Poster_PostingFinished_ForTestOnly(false, 2, postedInvoicePKs, "", null);

			AssertNull(controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly);
			AssertNotNull(controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly);
			List<DocumentPack> docPacks = controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly.GetDocumentPacks().ToList();
			AssertDocumentPacksContainsCorrectReports(docPacks, 1, 1, "Class A Invoice Preprinted", null, "Class A Invoice Preprinted", null);
			controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly.Dispose();
			controller.LastProgressFormForTestOnly.Dispose();
			controllerMock.VerifyAll();
		}

		public void TestDeliveryAllWithENTInvoice_VN()
		{
			var controllerMock = new Mock<PeriodicInvoiceBulkPosterController>(new object[] { new PeriodicInvoiceBulkPoster(), null });
			var controller = controllerMock.Object;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.EnterpriseInvoice);
			SetupComplianceNumber();
			controllerMock.Protected()
				.Setup<YesNoYesAllNoAllMessageBoxResult>("GetUserChoice")
				.Returns(YesNoYesAllNoAllMessageBoxResult.YesToAll);

			controller.Poster_PostingStarted_ForTestOnly(2);
			controller.Poster_PostingFinished_ForTestOnly(false, 2, postedInvoicePKs, "", null);

			AssertNotNull(controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly);
			AssertNull(controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly);
			List<DocumentPack> docPacks = controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.GetDocumentPacks().ToList();
			AssertDocumentPacksContainsCorrectReports(docPacks, 1, 1, "Invoice", null, "Invoice", null);
			controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.Dispose();
			controller.LastProgressFormForTestOnly.Dispose();
			controllerMock.VerifyAll();
		}

		public void TestDeliveryAllWithTAXAndENTInvoice_VN()
		{
			var controllerMock = new Mock<PeriodicInvoiceBulkPosterController>(new object[] { new PeriodicInvoiceBulkPoster(), null });
			var controller = controllerMock.Object;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
			SetupComplianceNumber();
			controllerMock.Protected()
				.Setup<YesNoYesAllNoAllMessageBoxResult>("GetUserChoice")
				.Returns(YesNoYesAllNoAllMessageBoxResult.YesToAll);

			controller.Poster_PostingStarted_ForTestOnly(2);
			controller.Poster_PostingFinished_ForTestOnly(false, 2, postedInvoicePKs, "", null);

			AssertNull(controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly);
			AssertNotNull(controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly);
			List<DocumentPack> docPacks = controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly.GetDocumentPacks().ToList();
			AssertDocumentPacksContainsCorrectReports(docPacks, 2, 2, "Class A Invoice Preprinted", "Invoice", "Class A Invoice Preprinted", "Invoice");
			controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly.Dispose();
			controller.LastProgressFormForTestOnly.Dispose();
			controllerMock.VerifyAll();
		}

		public void TestDeliveryAllWithTAXInvoice_CN()
		{
			var controllerMock = new Mock<PeriodicInvoiceBulkPosterController>(new object[] { new PeriodicInvoiceBulkPoster(), null });
			var controller = controllerMock.Object;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.GovtTaxInvoice);
			SetupComplianceNumber();
			controllerMock.Protected()
				.Setup<YesNoYesAllNoAllMessageBoxResult>("GetUserChoice")
				.Returns(YesNoYesAllNoAllMessageBoxResult.YesToAll);

			controller.Poster_PostingStarted_ForTestOnly(2);
			controller.Poster_PostingFinished_ForTestOnly(false, 2, postedInvoicePKs, "", null);

			AssertNotNull(controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly);
			AssertNull(controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly);
			List<DocumentPack> docPacks = controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.GetDocumentPacks().ToList();
			AssertDocumentPacksContainsCorrectReports(docPacks, 1, 1, "Invoice", null, "Invoice", null);
			controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.Dispose();
			controller.LastProgressFormForTestOnly.Dispose();
			controllerMock.VerifyAll();
		}

		public void TestDeliveryAllWithENTInvoice_CN()
		{
			var controllerMock = new Mock<PeriodicInvoiceBulkPosterController>(new object[] { new PeriodicInvoiceBulkPoster(), null });
			var controller = controllerMock.Object;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.EnterpriseInvoice);
			SetupComplianceNumber();
			controllerMock.Protected()
				.Setup<YesNoYesAllNoAllMessageBoxResult>("GetUserChoice")
				.Returns(YesNoYesAllNoAllMessageBoxResult.YesToAll);

			controller.Poster_PostingStarted_ForTestOnly(2);
			controller.Poster_PostingFinished_ForTestOnly(false, 2, postedInvoicePKs, "", null);

			AssertNotNull(controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly);
			AssertNull(controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly);
			List<DocumentPack> docPacks = controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.GetDocumentPacks().ToList();
			AssertDocumentPacksContainsCorrectReports(docPacks, 1, 1, "Invoice", null, "Invoice", null);
			controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.Dispose();
			controller.LastProgressFormForTestOnly.Dispose();
			controllerMock.VerifyAll();
		}

		public void TestDeliveryAllWithTAXAndENTInvoice_CN()
		{
			var controllerMock = new Mock<PeriodicInvoiceBulkPosterController>(new object[] { new PeriodicInvoiceBulkPoster(), null });
			var controller = controllerMock.Object;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
			SetupComplianceNumber();
			controllerMock.Protected()
				.Setup<YesNoYesAllNoAllMessageBoxResult>("GetUserChoice")
				.Returns(YesNoYesAllNoAllMessageBoxResult.YesToAll);

			controller.Poster_PostingStarted_ForTestOnly(2);
			controller.Poster_PostingFinished_ForTestOnly(false, 2, postedInvoicePKs, "", null);

			AssertNotNull(controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly);
			AssertNull(controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly);
			List<DocumentPack> docPacks = controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.GetDocumentPacks().ToList();
			AssertDocumentPacksContainsCorrectReports(docPacks, 1, 1, "Invoice", null, "Invoice", null);
			controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.Dispose();
			controller.LastProgressFormForTestOnly.Dispose();
			controllerMock.VerifyAll();
		}

		public void TestDeliveryAllWithTAXInvoice_AU()
		{
			var controllerMock = new Mock<PeriodicInvoiceBulkPosterController>(new object[] { new PeriodicInvoiceBulkPoster(), null });
			var controller = controllerMock.Object;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.GovtTaxInvoice);
			controllerMock.Protected()
				.Setup<YesNoYesAllNoAllMessageBoxResult>("GetUserChoice")
				.Returns(YesNoYesAllNoAllMessageBoxResult.YesToAll);

			controller.Poster_PostingStarted_ForTestOnly(2);
			controller.Poster_PostingFinished_ForTestOnly(false, 2, postedInvoicePKs, "", null);

			AssertNotNull(controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly);
			AssertNull(controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly);
			List<DocumentPack> docPacks = controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.GetDocumentPacks().ToList();
			AssertDocumentPacksContainsCorrectReports(docPacks, 1, 1, "Invoice", null, "Invoice", null);
			controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.Dispose();
			controller.LastProgressFormForTestOnly.Dispose();
			controllerMock.VerifyAll();
		}

		public void TestDeliveryAllWithENTInvoice_AU()
		{
			var controllerMock = new Mock<PeriodicInvoiceBulkPosterController>(new object[] { new PeriodicInvoiceBulkPoster(), null });
			var controller = controllerMock.Object;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.EnterpriseInvoice);
			controllerMock.Protected()
				.Setup<YesNoYesAllNoAllMessageBoxResult>("GetUserChoice")
				.Returns(YesNoYesAllNoAllMessageBoxResult.YesToAll);

			controller.Poster_PostingStarted_ForTestOnly(2);
			controller.Poster_PostingFinished_ForTestOnly(false, 2, postedInvoicePKs, "", null);

			AssertNotNull(controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly);
			AssertNull(controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly);
			List<DocumentPack> docPacks = controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.GetDocumentPacks().ToList();
			AssertDocumentPacksContainsCorrectReports(docPacks, 1, 1, "Invoice", null, "Invoice", null);
			controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.Dispose();
			controller.LastProgressFormForTestOnly.Dispose();
			controllerMock.VerifyAll();
		}

		public void TestDeliveryAllWithTAXAndENTInvoice_AU()
		{
			var controllerMock = new Mock<PeriodicInvoiceBulkPosterController>(new object[] { new PeriodicInvoiceBulkPoster(), null });
			var controller = controllerMock.Object;

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			AccountingConfigurationRegistry.Instance.InvoicePrintingOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice);
			controllerMock.Protected()
				.Setup<YesNoYesAllNoAllMessageBoxResult>("GetUserChoice")
				.Returns(YesNoYesAllNoAllMessageBoxResult.YesToAll);

			controller.Poster_PostingStarted_ForTestOnly(2);
			controller.Poster_PostingFinished_ForTestOnly(false, 2, postedInvoicePKs, "", null);

			AssertNotNull(controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly);
			AssertNull(controller.GovernmentInvoicesPrintTaskForTesting_ForTestOnly);
			List<DocumentPack> docPacks = controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.GetDocumentPacks().ToList();
			AssertDocumentPacksContainsCorrectReports(docPacks, 1, 1, "Invoice", null, "Invoice", null);
			controller.EnterpriseInvoicesPrintTaskForTesting_ForTestOnly.Dispose();
			controller.LastProgressFormForTestOnly.Dispose();
			controllerMock.VerifyAll();
		}

		void AssertDocumentPacksContainsCorrectReports(List<DocumentPack> docPacks, int expectFirstDocPackCount, int expectSecondDocPackCount,
				string fistDocPackFirstReportName, string fistDocPackSecondReportName, string secondDocPackFirstReportName, string secondDocPackSecondReportName)
		{
			AssertEquals("Print task should have 2 print tasks", 2, docPacks.Count);
			AssertEquals(expectFirstDocPackCount, docPacks[0].Count);
			AssertEquals(expectSecondDocPackCount, docPacks[1].Count);
			if (fistDocPackFirstReportName != null)
			{
				AssertEquals(fistDocPackFirstReportName, docPacks[0][0].MenuItem.SU_MenuName);
			}
			if (fistDocPackSecondReportName != null)
			{
				AssertEquals(fistDocPackSecondReportName, docPacks[0][1].MenuItem.SU_MenuName);
			}
			if (secondDocPackFirstReportName != null)
			{
				AssertEquals(secondDocPackFirstReportName, docPacks[1][0].MenuItem.SU_MenuName);
			}
			if (secondDocPackSecondReportName != null)
			{
				AssertEquals(secondDocPackSecondReportName, docPacks[1][1].MenuItem.SU_MenuName);
			}
		}

		[TestDate(2019, 10, 20)]
		public void TestHandleNegativeCompliancesFailedToCreate()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			AssertHandleNegativeCompliancesFailedToCreate(true);
			AssertHandleNegativeCompliancesFailedToCreate(false);
		}

		void AssertHandleNegativeCompliancesFailedToCreate(bool flag)
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			var plugin = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var plugin1 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			Job job = testObjectCreator.CreateJob(plugin, testObjectCreator.ABIGAS, 0, testObjectCreator.ZECTRA, 0);
			job.JH_UniqueJobInvoiceNumber = 6;

			Job job1 = testObjectCreator.CreateJob(plugin1, testObjectCreator.ABIGAS, 0, testObjectCreator.ZECTRA, 0);
			job1.JH_UniqueJobInvoiceNumber = 6;

			OrgInvoiceType orgInvoiceType = testObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			orgInvoiceType.PI_RS_NKServiceLevel = "STD";

			OrgInvTypeDeferredCharges orgInvTypeDeferredCharges = orgInvoiceType.DeferredCharges.AddNew();
			orgInvTypeDeferredCharges.PO_AC = testObjectCreator.MRG100.PK;

			Factory.Save();

			testObjectCreator.ABIGAS.CompanyData.OB_ARVATConfig = "DEF";
			testObjectCreator.ABIGAS.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = "RCC";

			Charge charge = testObjectCreator.CreateCharge(job, testObjectCreator.MRG100, "Test Charge", testObjectCreator.AUD, 0, testObjectCreator.AALSHI, testObjectCreator.AUD, -3333, testObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;
			charge.JR_AT_SellGSTRate = testObjectCreator.GST1.PK;
			testObjectCreator.GST1.AT_PostingGroupId = 1;

			Charge charge1 = testObjectCreator.CreateCharge(job1, testObjectCreator.MRG100, "Test Charge", testObjectCreator.AUD, 0, testObjectCreator.AALSHI, testObjectCreator.AUD, 5555, testObjectCreator.ABIGAS);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			charge1.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;
			charge1.JR_AT_SellGSTRate = testObjectCreator.GST2.PK;
			testObjectCreator.GST2.AT_PostingGroupId = 2;

			Charge charge2 = testObjectCreator.CreateCharge(job1, testObjectCreator.MRG100, "Test Charge", testObjectCreator.AUD, 0, testObjectCreator.AALSHI, testObjectCreator.AUD, -80, testObjectCreator.ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			charge2.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;
			charge2.JR_AT_SellGSTRate = testObjectCreator.GST2.PK;

			Factory.Save();

			var periodicInvoice = new PeriodicInvoiceBulk(Factory);
			periodicInvoice.CurrencyNK = "AUD";
			periodicInvoice.LoadJobs();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (var form = new PeriodicInvoicingBulkForm(periodicInvoice))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var periodicInvoiceBulkPoster = form.GeneratePoster();
				var postController = new PeriodicInvoiceBulkPosterController(periodicInvoiceBulkPoster, null);
				postController.HookEvents_ForTestOnly();
				periodicInvoiceBulkPoster.Post();
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(AccountingConstants.GetComplianceDocumentNegativeMessage()));
				UnitTestUserNotification.Instance.ClearMessages();
				postController.LastProgressFormForTestOnly.Dispose();
			}
		}

		[TestDate(2020, 10, 1)]
		public void TestPeriodicInvoiceBulkWhenDebtorPECIsNotDefined_WhenComplianceSubTypeIsEAR()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			SetupDataForTestPeriodicInvoiceBulkWhenDebtorPECIsNotDefined(testObjectCreator);

			var periodicInvoice = new PeriodicInvoiceBulk(Factory);
			periodicInvoice.CurrencyNK = "TRY";
			periodicInvoice.LoadJobs();

			using (testObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(GlbBranch.CurrentBranch.PK.ToGuid(), ZDateTime.Today.AddDays(-30).ToDateTime()))
			using (var form = new PeriodicInvoicingBulkForm(periodicInvoice))
			{
				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var periodicInvoiceBulkPoster = form.GeneratePoster();
				var postController = new PeriodicInvoiceBulkPosterController(periodicInvoiceBulkPoster, null);
				postController.HookEvents_ForTestOnly();
				periodicInvoiceBulkPoster.Post();
				var progressForm = (ProgressWithDetailesForm)postController.LastProgressFormForTestOnly;

				var expectedMessage = @"
01-Oct-20 00:00: Posting is started.
01-Oct-20 00:00: ERROR - Invoice ABIGAS, TRY, FID: Transaction was not posted.
ABIGAS: An email address is required for this Debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the Organization record for the Debtor to include a valid email address (Maintain > Master Data > Organization).

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)
01-Oct-20 00:00: Posting is finished. Posted 0, failed 1 of 1 periodic invoices.
Elapsed time: 00:00:00

Errors:
01-Oct-20 00:00: ERROR - Invoice ABIGAS, TRY, FID: Transaction was not posted.
ABIGAS: An email address is required for this Debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the Organization record for the Debtor to include a valid email address (Maintain > Master Data > Organization).

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)
";

				AssertEquals(expectedMessage, progressForm.Log);

				UnitTestUserNotification.Instance.ClearMessages();
				postController.LastProgressFormForTestOnly.Dispose();
			}
		}

		[TestDate(2020, 10, 1)]
		public void TestPeriodicInvoiceBulkWhenDebtorPECIsNotDefined_WhenComplianceSubTypeIsNotEAR()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			SetupDataForTestPeriodicInvoiceBulkWhenDebtorPECIsNotDefined(testObjectCreator);

			var periodicInvoice = new PeriodicInvoiceBulk(Factory);
			periodicInvoice.CurrencyNK = "TRY";
			periodicInvoice.LoadJobs();

			using (testObjectCreator.SetUpForTestingEInvoicingTurkey_Receivables(GlbBranch.CurrentBranch.PK.ToGuid(), ZDateTime.Today.AddDays(-30).ToDateTime()))
			using (var form = new PeriodicInvoicingBulkForm(periodicInvoice))
			{
				testObjectCreator.ABIGAS.CustomsCodes.AddNew(TurkeyOrgCusCodeInfo.OrgCusCodes.VTE, "1234567890");
				Factory.Save();

				form.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var periodicInvoiceBulkPoster = form.GeneratePoster();
				var postController = new PeriodicInvoiceBulkPosterController(periodicInvoiceBulkPoster, null);
				postController.HookEvents_ForTestOnly();
				periodicInvoiceBulkPoster.Post();
				var progressForm = (ProgressWithDetailesForm)postController.LastProgressFormForTestOnly;

				var expectedMessage = @"
01-Oct-20 00:00: Posting is started.
01-Oct-20 00:00: Success - Invoice ABIGAS, TRY, FID: AR INV 00001002 was posted.
ABIGAS: Warning! A Post Box Alias is required for this debtor to allow the receivables transaction to be successfully posted.
Before attempting to post the charges, please update the organization record for the debtor to include a valid Post Box Alias email address using the registration number type PEC.

This can be updated at Maintain > Master Data > Organization. (Details > Config > Registration Numbers/Codes sub-tab)
01-Oct-20 00:00: Posting is finished. Posted 1 of 1 periodic invoices.
Elapsed time: 00:00:00";

				AssertEquals(expectedMessage, progressForm.Log);

				UnitTestUserNotification.Instance.ClearMessages();
				postController.LastProgressFormForTestOnly.Dispose();
			}
		}

		void SetupDataForTestPeriodicInvoiceBulkWhenDebtorPECIsNotDefined(TestObjectCreator testObjectCreator)
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var plugin = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var plugin1 = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var job = testObjectCreator.CreateJob(plugin, testObjectCreator.ABIGAS, 0, testObjectCreator.ZECTRA, 0);
			job.JH_UniqueJobInvoiceNumber = 6;

			var job1 = testObjectCreator.CreateJob(plugin1, testObjectCreator.ABIGAS, 0, testObjectCreator.ZECTRA, 0);
			job1.JH_UniqueJobInvoiceNumber = 6;

			var orgInvoiceType = testObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			orgInvoiceType.PI_RS_NKServiceLevel = "STD";

			var orgInvTypeDeferredCharges = orgInvoiceType.DeferredCharges.AddNew();
			orgInvTypeDeferredCharges.PO_AC = testObjectCreator.MRG100.PK;

			Factory.Save();

			testObjectCreator.ABIGAS.CompanyData.OB_ARVATConfig = "DEF";
			testObjectCreator.ABIGAS.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = "RCC";

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.MRG100, "Test Charge", testObjectCreator.TRY, 0, null, testObjectCreator.TRY, 3333, testObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;
			charge.JR_RX_NKSellInvoiceCurrency = testObjectCreator.TRY.RX_Code;
			charge.JR_AT_SellGSTRate = testObjectCreator.KDV18.PK;
			testObjectCreator.GST1.AT_PostingGroupId = 1;

			var charge1 = testObjectCreator.CreateCharge(job1, testObjectCreator.MRG100, "Test Charge", testObjectCreator.TRY, 0, null, testObjectCreator.TRY, 5555, testObjectCreator.ABIGAS);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge1.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;
			charge1.JR_RX_NKSellInvoiceCurrency = testObjectCreator.TRY.RX_Code;
			charge1.JR_AT_SellGSTRate = testObjectCreator.KDV18.PK;
			testObjectCreator.GST2.AT_PostingGroupId = 2;

			var charge2 = testObjectCreator.CreateCharge(job1, testObjectCreator.MRG100, "Test Charge", testObjectCreator.TRY, 0, null, testObjectCreator.TRY, 80, testObjectCreator.ABIGAS);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;
			charge2.JR_RX_NKSellInvoiceCurrency = testObjectCreator.TRY.RX_Code;
			charge2.JR_AT_SellGSTRate = testObjectCreator.KDV18.PK;

			Factory.Save();
		}

		void SetupComplianceNumber()
		{
			invoice1.AH_ComplianceSubType = "TXI";
			invoice1.AH_TransactionReference = "000001";
			invoice2.AH_ComplianceSubType = "TXI";
			invoice2.AH_TransactionReference = "000002";
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testObjectCreator = new TestObjectCreator(Factory);
			invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_OH = testObjectCreator.AALSHI.PK;

			invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AH_OH = testObjectCreator.ABIGAS.PK;
			Factory.Save();

			List<ZGuid> list = new List<ZGuid>();
			list.Add(invoice1.PK);
			list.Add(invoice2.PK);
			postedInvoicePKs = list.ToArray();
		}

		ARInvoice invoice1;
		ARInvoice invoice2;
		ZGuid[] postedInvoicePKs;
	}
}
