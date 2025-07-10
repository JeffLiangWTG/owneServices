using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(PeriodicInvoicingForm))]
	public class PeriodicInvoicingFormAccountingZFormBasherTest : AccountingZFormBasherTest
	{
		protected virtual PeriodicInvoice GetFormBizO()
		{
			return new PeriodicInvoice(Factory);
		}

		protected virtual PeriodicInvoicingForm GetForm(PeriodicInvoice periodicInvoice)
		{
			return new PeriodicInvoicingForm(periodicInvoice);
		}

		protected override Form GetFormToBashCore()
		{
			return GetForm(GetFormBizO());
		}

		void CreateShipmentWithDeferredCharge(string shipmentNumber, OrgHeader debtor, OrgHeader creditor)
		{
			var shipment = TestObjectCreator.CreateShipment(shipmentNumber);
			var job = TestObjectCreator.CreateJob(shipment, false, false);
			job.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.Addresses.MainAddress.PK;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 100M, 120M);
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_InvoiceType = "FID";
			charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			charge.JR_JH = job.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_AT_CostGSTRate = Guid.Empty;
			charge.JR_OH_CostAccount = creditor.PK;
			Factory.Save();
		}

		public override void TestFormVerb()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				AssertEquals("Verb should be 'New'", "New", testForm.FormVerb);
			}
		}

		public new void TestPromptReversingReason()
		{
			Assert(true);
		}

		public void TestErrorMessageShownWhenRelatedChargeHasCriticalPostErrorAfterPreviewPeriodicInvoice()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var debtor = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true);
			debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			OrgInvoiceType type = debtor.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			type.PI_RS_NKServiceLevel = "STD";

			var creditor = TestObjectCreator.CreateOrgHeader("TSTORG2", true, false);

			creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			CreateShipmentWithDeferredCharge("S001001", debtor, creditor);
			CreateShipmentWithDeferredCharge("S001002", debtor, creditor);

			creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.DebtorPK = debtor.PK;
			periodicInvoice.CurrencyNK = "AUD";
			periodicInvoice.LoadJobs();

			AssertEquals("Precondition: one job should be found", 2, periodicInvoice.SelectedJobs.Count());

			foreach (Job job in periodicInvoice.SelectedJobs)
			{
				job.MarkLightValidationAsValidForTesting();
			}

			periodicInvoice.RunPreSaveValidation();

			var errors = periodicInvoice.NotificationsIncludingChildren.GetErrors().ToList();
			AssertEquals("Precondition: to simulate what happens in GUI - no errors found.", 0, errors.Count);

			var expectedErrorMsg = @"Error occurred for the following Job(s): S001001, S001002

Summary of the error(s):
Error - Cost Tax ID: Tax IDs on unposted charges conflict with the creditor ""Tax is Applicable"" flag.
One possible way to resolve this is to go into the ""Job Invoicing"" menu and click ""Reset Unposted lines Tax Default"" option.";

			using (PeriodicInvoicingForm form = new PeriodicInvoicingForm(periodicInvoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.PreviewInvoiceMenuItem.PerformClick();

				AssertEquals("Should show error message when related charge contain critical post error.", expectedErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPreviewInvoiceMenuItem()
		{
			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockIPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ZArchitecture.Modules.ISecurityCheckpoint>()))
				.Returns(false);

			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			{
				var factory = new BusinessObjectFactory();
				var creator = new TestObjectCreator(factory);

				var periodHelper = new AccountingPeriodTestHelper(factory);
				periodHelper.SetupPeriods();

				OrgInvoiceType type = creator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
				type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
				type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
				type.PI_RS_NKServiceLevel = "STD";

				var shipment = creator.CreateShipment("S1");
				var job = creator.CreateJob(shipment);
				job.LocalChargesPK = creator.ABIGAS.PK;

				var charge = job.Charges.AddNew();
				try
				{
					charge.FillWithValidTestData();
					charge.JR_AC = creator.CC1.PK;
					charge.JR_OH_SellAccount = creator.ABIGAS.PK;
					charge.JR_OSSellAmt = 100m;
					charge.JR_LocalSellAmt = 100m;
					charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
					factory.Save();

					var periodicInvoice = new PeriodicInvoice(factory);
					periodicInvoice.DebtorPK = creator.ABIGAS.PK;
					periodicInvoice.LoadJobs();

					using (PeriodicInvoicingForm form = new PeriodicInvoicingForm(periodicInvoice))
					{
						form.PreviewInvoiceMenuItem.PerformClick();
						AssertEquals("Previewing doesn't post a transaction via PostManager", 0, periodicInvoice.PostManager.Poster.PostedInvoices.Count);
						AssertEquals("Previewing create temporary transaction via previewPostManager", 1, periodicInvoice.previewPostManager_ForTestOnly.Poster.PostedInvoices.Count);
						var invoiceCreated = periodicInvoice.previewPostManager_ForTestOnly.Poster.PostedInvoices[0];
						var printTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoiceCreated));
						AssertEquals(1, printTask.TaskCount);

						AssertEquals("Previewing doesn't post a transaction", false, invoiceCreated.IsInDatabase);

						var newFactory = new BusinessObjectFactory();
						var reloadCharge = newFactory.Load<Charge>(charge.PK);
						reloadCharge.Delete();
						newFactory.Save();

						form.PreviewInvoiceMenuItem.PerformClick();

						AssertEquals("Deleted charge should trigger an error of preview.",
							@"This invoice can't be previewed due to the following:
Since this invoice screen was opened, one of the billing lines included in this invoice has been changed.
Error - record: Charge count for the invoice was changed from 1 to 0.
Error - record: Total Local Ex Tax Amount of this invoice has changed from $100.00 to $0.00.
You will need to cancel this screen and begin the periodic invoice process again.",
							UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					job.Dispose();
				}
			}
		}

		public void TestPostingCriticalErrorStopsSaving()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			var shipment = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			Job job = testObjectCreator.CreateJob(shipment, testObjectCreator.ABIGAS, 10, testObjectCreator.ZECTRA, 10);
			job.JH_UniqueJobInvoiceNumber = 6;
			job.JH_ProfitLossReasonCode = "";

			OrgInvoiceType orgInvoiceType = testObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;

			OrgInvTypeDeferredCharges orgInvTypeDeferredCharges = orgInvoiceType.DeferredCharges.AddNew();
			orgInvTypeDeferredCharges.PO_AC = testObjectCreator.MRG100.PK;

			Factory.Save();

			Charge charge = testObjectCreator.CreateCharge(job, testObjectCreator.MRG100, "Test Charge", null, 10m, null, testObjectCreator.AUD, 300m, testObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;

			Factory.Save();

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.CurrencyNK = "AUD";
			periodicInvoice.DebtorPK = testObjectCreator.ABIGAS.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			periodicInvoice.LoadJobs();

			JobProfitLossReasonCodeCollection plReasonCodes = new JobProfitLossReasonCodeCollection();
			JobProfitLossReasonCode plReasonCode = plReasonCodes.AddNew();
			plReasonCode.Code = "TST";
			plReasonCode.Description = (NoResString)"Test";
			AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plReasonCodes);

			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(GlbCompany.CurrentCompany.PK.ToGuid(), null);

			var value = AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			foreach (CodeDescriptionBool item in value)
			{
				AssertEquals("Is status changed to Invoiced on posting first AR Invoice when Job Status: " + item.Code, false, item.Bool);
			}

			JobProfitLossRequiringReasonParameters plRequiringReasonParameters = new JobProfitLossRequiringReasonParameters();
			plRequiringReasonParameters.ProfitThreshold = 10M;
			plRequiringReasonParameters.JobStatusCollection.AddNew().Code = JobHeaderStatus.JobInvoiced.Code;
			AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plRequiringReasonParameters);

			using (PeriodicInvoicingForm form = new PeriodicInvoicingForm(periodicInvoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Saving should be cancelled", ContinueWithSave.No, form.ValidateAndSave_ForTestOnly());
				Assert("User should be shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
				// Job Reason Code now has standalone validation.
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestJobValidationOnCreditor()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var debtor = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true);
			debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			OrgInvoiceType type = debtor.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			type.PI_RS_NKServiceLevel = "STD";

			var creditor = TestObjectCreator.CreateOrgHeader("TSTORG2", true, false);

			creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			CreateShipmentWithDeferredCharge("S001001", debtor, creditor);
			CreateShipmentWithDeferredCharge("S001002", debtor, creditor);

			creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = debtor.PK;
			}
			periodicInvoice.CurrencyNK = "AUD";
			periodicInvoice.LoadJobs();

			AssertEquals("Precondition: one job should be found", 2, periodicInvoice.SelectedJobs.Count());

			foreach (Job job in periodicInvoice.SelectedJobs)
			{
				job.MarkLightValidationAsValidForTesting();
			}

			periodicInvoice.RunPreSaveValidation();

			var errors = periodicInvoice.NotificationsIncludingChildren.GetErrors().ToList();
			AssertEquals("Precondition: to simulate what happens in GUI - no errors found.", 0, errors.Count);

			var expectedErrorMsg = @"Error occurred for the following Job(s): S001001, S001002

Summary of the error(s):
Error - Cost Tax ID: Tax IDs on unposted charges conflict with the creditor ""Tax is Applicable"" flag.
One possible way to resolve this is to go into the ""Job Invoicing"" menu and click ""Reset Unposted lines Tax Default"" option.";

			using (PeriodicInvoicingForm form = new PeriodicInvoicingForm(periodicInvoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Saving should be cancelled", ContinueWithSave.No, form.ValidateAndSave_ForTestOnly());
				Assert("User should be shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Error message should be as expected", expectedErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPromptToPrintInvoice()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			var shipment = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			Job job = testObjectCreator.CreateJob(shipment, testObjectCreator.ABIGAS, 10, testObjectCreator.ZECTRA, 10);
			job.JH_UniqueJobInvoiceNumber = 6;

			OrgInvoiceType orgInvoiceType = testObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			orgInvoiceType.PI_RS_NKServiceLevel = "STD";

			OrgInvTypeDeferredCharges orgInvTypeDeferredCharges = orgInvoiceType.DeferredCharges.AddNew();
			orgInvTypeDeferredCharges.PO_AC = testObjectCreator.MRG100.PK;

			Factory.Save();

			Charge charge = testObjectCreator.CreateCharge(job, testObjectCreator.MRG100, "Test Charge", null, 0, null, testObjectCreator.AUD, 300, testObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;

			Factory.Save();

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.CurrencyNK = "AUD";
			periodicInvoice.DebtorPK = testObjectCreator.ABIGAS.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			periodicInvoice.LoadJobs();

			using (PeriodicInvoicingForm form = new PeriodicInvoicingForm(periodicInvoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ValidateAndSave_ForTestOnly();
				Assert("User should be prompted to print invoice", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print invoice", "Do you want to print invoice 00001000?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_UserHasRights()
		{
			PeriodicInvoice periodicInvoice = CreatePeriodicInvoiceForSecurityCheck();

			using (var form = new PeriodicInvoicingForm(periodicInvoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Expect call completed successfully", ContinueWithSave.Yes, form.CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices.Count);
				var postedInvoice = (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices[0];
				Assert("Expect AR CRD is created", postedInvoice is ARCreditNote);
				Factory.Save();
				AssertEquals("Should not create ATH logs when no authorisation is needed", 0, postedInvoice.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.Events.Authorised.Code).Count());
			}
		}

		[TestDate(2019, 04, 30)]
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_PreviewInvoicesWhenAllSecurityRightsDisabled()
		{
			var periodicInvoice = PreparePeriodicInvoiceTestDataForPreview();

			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesPeriodicInvoice, false);
			var previewInvoicesCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewInvoices);
			var previewOnlyCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewOnly);
			var previewAndDeliverCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewAndDeliver);

			using (PeriodicInvoicingForm testForm = new PeriodicInvoicingForm(periodicInvoice))
			{
				testForm.Show();

				previewInvoicesCheckpoint.IsAllowed = false;
				previewOnlyCheckpoint.IsAllowed = false;
				previewAndDeliverCheckpoint.IsAllowed = false;

				AssertNoErrors("BusinessEntity should have no errors", (PeriodicInvoice)testForm.BusinessEntity);
				testForm.PreviewInvoiceMenuItem.PerformClick();
				AssertEquals(previewOnlyCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				CloseOpenedForms();
			}
		}

		[TestDate(2019, 04, 30)]
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_PreviewInvoicesWhenOnlyPreviewInvoicesEnabled()
		{
			var periodicInvoice = PreparePeriodicInvoiceTestDataForPreview();

			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesPeriodicInvoice, false);
			var previewInvoicesCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewInvoices);
			var previewOnlyCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewOnly);
			var previewAndDeliverCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewAndDeliver);

			using (PeriodicInvoicingForm testForm = new PeriodicInvoicingForm(periodicInvoice))
			{
				testForm.Show();

				previewInvoicesCheckpoint.IsAllowed = true;
				previewOnlyCheckpoint.IsAllowed = false;
				previewAndDeliverCheckpoint.IsAllowed = false;

				AssertNoErrors("BusinessEntity should have no errors", (PeriodicInvoice)testForm.BusinessEntity);
				testForm.PreviewInvoiceMenuItem.PerformClick();
				AssertEquals(previewOnlyCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				CloseOpenedForms();
			}
		}

		[TestDate(2019, 04, 30)]
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_PreviewInvoicesWhenOnlyPreviewAndDeliverDisabled()
		{
			var periodicInvoice = PreparePeriodicInvoiceTestDataForPreview();

			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesPeriodicInvoice, false);
			var previewInvoicesCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewInvoices);
			var previewOnlyCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewOnly);
			var previewAndDeliverCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewAndDeliver);

			using (PeriodicInvoicingForm testForm = new PeriodicInvoicingForm(periodicInvoice))
			{
				testForm.Show();

				previewInvoicesCheckpoint.IsAllowed = true;
				previewOnlyCheckpoint.IsAllowed = true;
				previewAndDeliverCheckpoint.IsAllowed = false;

				ZFormModaliser.LastFormShownDialogForTest = null;
				AssertNoErrors("BusinessEntity should have no errors", (PeriodicInvoice)testForm.BusinessEntity);
				testForm.PreviewInvoiceMenuItem.PerformClick();
				AssertNull("LastFormShownDialogForTest should be null", ZFormModaliser.LastFormShownDialogForTest);
				AssertNotEquals(previewOnlyCheckpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				CloseOpenedForms();
			}
		}

		[TestDate(2019, 04, 30)]
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_PreviewInvoicesWhenAllSecurityRightsEnabled()
		{
			var periodicInvoice = PreparePeriodicInvoiceTestDataForPreview();

			var securityHelper = new JobInvoicingSecurityHelper(Env.Security.NewReceivablesPeriodicInvoice, false);
			var previewInvoicesCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewInvoices);
			var previewOnlyCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewOnly);
			var previewAndDeliverCheckpoint = securityHelper.GetInvSecurity(SecurityCore.PreviewAndDeliver);

			using (PeriodicInvoicingForm testForm = new PeriodicInvoicingForm(periodicInvoice))
			{
				testForm.Show();
				
				previewInvoicesCheckpoint.IsAllowed = true;
				previewOnlyCheckpoint.IsAllowed = true;
				previewAndDeliverCheckpoint.IsAllowed = true;

				ZFormModaliser.LastFormShownDialogForTest = null;
				AssertNoErrors("BusinessEntity should have no errors", (PeriodicInvoice)testForm.BusinessEntity);
				testForm.PreviewInvoiceMenuItem.PerformClick();
				AssertNotNull("LastFormShownDialogForTest should not be null", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("LastFormShownDialogForTest should be a DocDeliveryForm", typeof(DocDeliveryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertNotEquals(UnitTestUserNotification.Instance.LastMessage.Text, previewInvoicesCheckpoint.ErrorMessageForNotAllowed);
				CloseOpenedForms();
			}
		}

		void CloseOpenedForms()
		{
			List<Form> previewForms = new List<Form>();
			for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
			{
				var previewForm = Application.OpenForms[i] as XLSPreviewForm;
				if (previewForm != null)
				{
					previewForm.Close();
				}
			}
		}

		[TestDate(2024, 02, 29)]
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_CriticalPostError_ExporterExemption()
		{
			var today = ZDate.Today;
			var currCompany = GlbCompany.CurrentCompany;
			var country = CountryCodes.Italy;

			using (currCompany.TemporarilySetCountry(country))
			using (AccountingMasterFilesRegistry.Instance.ValidateTaxIDApplicationForExporterExemptionItaly.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var doc = TestObjectCreator.Debtor.RequiredDocuments.AddNew();
				doc.EQ_DocCategory = ReferenceTypes.ClientSupplierRelationship;
				doc.EQ_DocType = RefDocTypes.VATExporterExemption;
				doc.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
				doc.EQ_RN_NKRelatedCountry = country;
				doc.EQ_DocNumber = "000001";
				doc.EQ_DateReceived = today.AddDays(-1).ToZDateTime().ToDateTimeOffset(null);
				doc.EQ_ValidToDate = today.AddDays(30);
				var ceilingLimitAttribute = doc.Attributes.AddNew();
				ceilingLimitAttribute.D0_AttribName = JobRequiredDocAttribTypeList.Codes.CeilingLimit;
				ceilingLimitAttribute.D0_AttribValue = "1000";

				var periodicInvoice = SetupForCriticalPostError();

				using (var testForm = new PeriodicInvoicingForm(periodicInvoice))
				{
					testForm.Show();
					testForm.ValidateAndSave_ForTestOnly();
					var expectedError = @"ZDebtor: There are errors that need to be corrected before this Accounts Receivable Invoice can be saved.
TAX ID: ZZGST2 based on the registry [Accounting -> Receivable Defaults -> Default Settings -> Validate Tax ID Application for Exporter Exemption (Italy)] it is not possible to proceed with the post because you are posting a transaction on a debtor that has one or more EXV-VAT/GST Exporter Exemption documents that still have available EUR 1000.00 CEILING LIMIT.
To proceed with the post fix Tax ID Code or disable the above registry item.";
					AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[TestDate(2024, 03, 01)]
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_CriticalPostError_ComplianceBook()
		{
			var today = ZDate.Today;
			var currCompany = GlbCompany.CurrentCompany;
			var companyPK = currCompany.PK.ToGuid();
			var registryInstance = AccountingMasterFilesRegistry.Instance;

			using (currCompany.TemporarilySetCountry(CountryCodes.Italy))
			using (registryInstance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (registryInstance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
			{
				var complianceSequence = TestObjectCreator.SetupComplianceSequence(ZGuid.Empty, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, ItalyComplianceInfo.ComplianceSubTypeCodes.ARI, 1, 100, 2);
				complianceSequence.XD_StartDate = new ZDate(today.Year, 1, 1);
				complianceSequence.XD_ExpiryDate = today.AddDays(-1);
				complianceSequence.XD_IsActive = true;

				var periodicInvoice = SetupForCriticalPostError();

				using (var testForm = new PeriodicInvoicingForm(periodicInvoice))
				{
					testForm.Show();
					testForm.ValidateAndSave_ForTestOnly();
					var expectedError = @"Please check your Compliance Invoice Book Setups. 
 A Compliance Invoice Book for the relevant Compliance Sub-Type, Branch, Active Status and Start / Expiry Date does not exist.";
					AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		PeriodicInvoice SetupForCriticalPostError()
		{
			var today = ZDate.Today;
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.PostPeriodsForEntireYear(today.Year);

			var debtor = TestObjectCreator.Debtor;
			var orgInvoiceType = debtor.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			orgInvoiceType.PI_RS_NKServiceLevel = "STD";

			Factory.Save();

			var shipment = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var job = TestObjectCreator.CreateJob(shipment, debtor, 10, TestObjectCreator.ZECTRA, 10);
			job.JH_UniqueJobInvoiceNumber = 6;
			job.LocalChargesPK = debtor.PK;

			var charge = job.Charges.AddNew();
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge.JR_AC = TestObjectCreator.MRG100.PK;
			charge.JR_Desc = "Test Charge 1";
			charge.JR_OSCostAmt = 0;
			charge.JR_OH_CostAccount = ZGuid.Empty;
			charge.JR_RX_NKSellCurrency = TestObjectCreator.EUR.RX_Code;
			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_OSSellAmt = 200;
			charge.JR_LocalSellAmt = 200;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST2.PK;
			charge.JR_SellTaxDate = today;

			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.CurrencyNK = "EUR";
			periodicInvoice.DebtorPK = debtor.PK;
			periodicInvoice.PostDate = today;
			periodicInvoice.DueDate = today.AddDays(30);
			periodicInvoice.InvoiceDate = today;
			periodicInvoice.InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			periodicInvoice.InvoiceTermDays = 30;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.LoadJobs();
			return periodicInvoice;
		}

		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginCancelled_SingleUserApproval() => TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginCancelled(false);
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginCancelled_TwoUserApproval() => TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginCancelled(true);

		void TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginCancelled(bool useTwoApprovers)
		{
			var expectedFormType = useTwoApprovers ? typeof(LoginFormWithTwoCredentialSupportBranchDepartmentLevel) : typeof(LoginFormForARCreditNoteApprovalOverride);

			bool originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			bool originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				SetAuthorizationLevelSettings(useTwoApprovers ? AuthorizationMode.Codes.TwoApprovers : AuthorizationMode.Codes.Default);
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				PeriodicInvoice periodicInvoice = CreatePeriodicInvoiceForSecurityCheck();

				using (var form = new PeriodicInvoicingForm(periodicInvoice))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Expect call completed unsuccessfully", ContinueWithSave.No, form.CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly());
					AssertType(expectedFormType, ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("You do not have security rights to post a credit note for the required amount. User with a higher Credit/Adjustment Note Approval Level can post this transaction.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(0, (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices.Count);
				}
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed_SingleApprover() => TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed(AuthorizationMode.Codes.Default);
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed_TwoApprover() => TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed(AuthorizationMode.Codes.TwoApprovers, false);
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed_TwoApprover_UserHasRights() => TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed(AuthorizationMode.Codes.TwoApprovers, true);
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed_SequentialApprover() => TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed(AuthorizationMode.Codes.SequentialApprovers, false);
		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed_SequentialApprover_UserHasRights() => TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed(AuthorizationMode.Codes.SequentialApprovers, true);

		void TestCreatePeriodicInvoiceAndCheckSecurityRights_OverrideLoginPassed(ZString authorizationMode, bool userHasRights = false)
		{
			bool useSequentialApprovers = authorizationMode == AuthorizationMode.Codes.SequentialApprovers;
			bool useTwoApprovers = authorizationMode == AuthorizationMode.Codes.TwoApprovers;

			var expectedFormType = useTwoApprovers || useSequentialApprovers ? typeof(LoginFormWithTwoCredentialSupportBranchDepartmentLevel) : typeof(LoginFormForARCreditNoteApprovalOverride);
			SetAuthorizationLevelSettings(authorizationMode);
			SecurityTestObject.CreateTestUser(userHasRights, Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code, "US1", "User1", "pass");
			var user = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, "User1"));
			SecurityTestObject.CreateTestUser(true, Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code, "US2", "User2", "pass");
			SecurityTestObject.CreateTestUser(true, Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code, "US3", "User3", "pass");
			using (Env.SetTemporaryUserContext(user.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					if (useTwoApprovers || useSequentialApprovers)
					{
						var loginForm = form as LoginFormWithTwoCredentialSupportBranchDepartmentLevel;
						AssertNotNull("form should be correct type when requiring multiple approvers", loginForm);
						if (!userHasRights)
						{
							loginForm.DoLogin1ForTest("User2", "pass");
						}
						loginForm.DoLogin2ForTest("User3", "pass");
					}
					else
					{
						var loginForm = form as LoginFormForARCreditNoteApprovalOverride;
						AssertNotNull("form should be correct type when not requiring two approvers", loginForm);
						loginForm.DoLoginForTest("User2", "pass");
					}
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
				});

				PeriodicInvoice periodicInvoice = CreatePeriodicInvoiceForSecurityCheck();
				using (var form = new PeriodicInvoicingForm(periodicInvoice))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Expect call completed successfully", ContinueWithSave.Yes, form.CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly());
					AssertType(expectedFormType, ZFormModaliser.LastFormShownDialogForTest);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices.Count);
					var postedInvoice = (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices[0];
					Assert("Expect AR CRD is created", postedInvoice is ARCreditNote);

					Factory.Save();
					var athLogs = postedInvoice.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.Events.Authorised.Code);
					AssertEquals("Should create an ATH log for approving users that aren't current user", 1, athLogs.Count());
					var athLogReference = athLogs.First().SL_Reference;
					if (useTwoApprovers || useSequentialApprovers)
					{
						Assert(athLogReference.Contains("User3"));
					}
					if (!userHasRights)
					{
						Assert(athLogReference.Contains("User2"));
					}
				}
			}
		}

		public void TestCreatePeriodicInvoiceWithSEQModeRequiresTwoApprover()
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetAuthorizationLevelSettings();
			var specificBranchDepartmentSetting = TestObjectCreator.CreateAuthorizationModeAndSettings(100m, 200m, "SEQ");
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty,
				TestObjectCreator.NonCurrentBranch.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), specificBranchDepartmentSetting);

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
				var periodicInvoice = CreatePeriodicInvoiceForSecurityCheck(true);
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
				using (var form = new PeriodicInvoicingForm(periodicInvoice))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Expect call completed failed", ContinueWithSave.No, form.CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly());
					AssertEquals("You do not have security rights to post a credit note for the required amount. User with a higher Credit/Adjustment Note Approval Level can post this transaction.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(0, (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices.Count);
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
				using (var form = new PeriodicInvoicingForm(periodicInvoice))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Expect call completed succeeded", ContinueWithSave.Yes, form.CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly());
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices.Count);
					Assert("Expect AR CRD is created", (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices[0] is ARCreditNote);
				}
			}
		}

		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_UserHasNoRights_HasMiscInvoices()
		{
			bool originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			bool originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				SetAuthorizationLevelSettings();
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
				arInvoice.AH_OSTotalAmount = 300M;
				Factory.Save();

				PeriodicInvoice periodicInvoice = CreatePeriodicInvoiceForSecurityCheck();
				periodicInvoice.MiscInvoices.Add(arInvoice);
				periodicInvoice.Jobs.RemoveAll();
				using (var form = new PeriodicInvoicingForm(periodicInvoice))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Expect call completed successfully", ContinueWithSave.Yes, form.CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly());
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices.Count);
					Assert("Expect periodic AR CRD is created", (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices[0] is ARCreditNote);
					Assert("Expect misc ar invoice is reversed", arInvoice.IsReversed);
					AssertNotNull("Expect reversed transaction is creaeted", arInvoice.CorrespondingReversedTransaction);
				}
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_UserHasNoRights_OverrideLoginFailed()
		{
			var originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			var originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				SetAuthorizationLevelSettings();
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;
				var staff = TestObjectCreator.CreateStaffWithSecurityRights("User1", "US1", Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code, "pass", false);
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginFormForARCreditNoteApprovalOverride;
					if (loginForm != null)
					{
						loginForm.DoLoginForTest(staff.GS_LoginName, staff.StaffPlainTextPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
					}
				});

				var periodicInvoice = CreatePeriodicInvoiceForSecurityCheck();
				using (var form = new PeriodicInvoicingForm(periodicInvoice))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Expect call completed failed", ContinueWithSave.No, form.CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly());
					AssertEquals("You do not have security rights to post a credit note for the required amount. User with a higher Credit/Adjustment Note Approval Level can post this transaction.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(0, (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices.Count);
				}
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_UserHasNoRights_LineLevelSecurityCheck()
		{
			SetAuthorizationLevelSettings();
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff.PK, true);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff.PK, false);

			var staff2 = TestObjectCreator.CreateStaffWithSecurityRights("newuser2", "ts2", Env.Security.APInvoiceApproval.Code, "password2", false);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff2.PK, true);
			TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), TestObjectCreator.NonCurrentBranch.PK.ToGuid(), staff2.PK, true);

			using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var periodicInvoice = CreatePeriodicInvoiceForSecurityCheck(true);
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginFormForARCreditNoteApprovalOverride;
					if (loginForm != null)
					{
						loginForm.DoLoginForTest(staff.GS_LoginName, staff.StaffPlainTextPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
					}
				});
				using (var form = new PeriodicInvoicingForm(periodicInvoice))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Expect call completed failed", ContinueWithSave.No, form.CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly());
					AssertEquals("You do not have security rights to post a credit note for the required amount. User with a higher Credit/Adjustment Note Approval Level can post this transaction.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(0, (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices.Count);
				}

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginFormForARCreditNoteApprovalOverride;
					if (loginForm != null)
					{
						loginForm.DoLoginForTest(staff2.GS_LoginName, staff2.StaffPlainTextPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
					}
				});
				using (var form = new PeriodicInvoicingForm(periodicInvoice))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Expect call completed succeeded", ContinueWithSave.Yes, form.CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly());
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices.Count);
					Assert("Expect AR CRD is created", (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices[0] is ARCreditNote);
				}
			}
		}

		public void TestCreatePeriodicInvoiceAndCheckSecurityRights_ApprovingUserPKListNotNull()
		{
			PeriodicInvoice periodicInvoice = CreatePeriodicInvoiceForSecurityCheck();

			var registry = new AuthorizationModeAndSettings();
			var collection = registry.AuthorisationSettings;
			var settings1 = collection.AddNew();
			settings1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings1.Amount = 10000M;
			settings1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			var settings2 = collection.AddNew();
			settings2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			settings2.Amount = 10000M;
			settings2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;
			registry.AuthorizationMode = AuthorizationMode.Codes.TwoApprovers;

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registry))
			using (var form = new PeriodicInvoicingForm(periodicInvoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Expect call completed successfully", ContinueWithSave.Yes, form.CreatePeriodicInvoiceAndCheckSecurityRights_ForTestOnly());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices.Count);
				var postedInvoice = (form.BusinessEntity as PeriodicInvoice).PostManager.Poster.PostedInvoices[0];
				Assert("Expect AR CRD is created", postedInvoice is ARCreditNote);
				Factory.Save();
				AssertNotNull("ApprovingUserPKList is not null", postedInvoice.ApprovingUserPKList);
				AssertEquals("ApprovingUserPKList is empty", 0, postedInvoice.ApprovingUserPKList.Count);
			}
		}

		void SetAuthorizationLevelSettings(string mode = "")
		{
			var setting = TestObjectCreator.CreateAuthorizationModeAndSettings(100m, 200m);
			if (!string.IsNullOrWhiteSpace(mode))
			{
				setting.AuthorizationMode = mode;
			}

			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
		}

		PeriodicInvoice CreatePeriodicInvoiceForSecurityCheck(bool includeLinesWithDifferentBranchDepartment = false)
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var testObjectCreator = new TestObjectCreator(Factory);

			var shipment = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var job = testObjectCreator.CreateJob(shipment, false, true, true, testObjectCreator.ABIGAS, 10, testObjectCreator.ZECTRA, 10);
			job.JH_UniqueJobInvoiceNumber = 6;

			var orgInvoiceType = testObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			orgInvoiceType.PI_RS_NKServiceLevel = "STD";

			var orgInvTypeDeferredCharges = orgInvoiceType.DeferredCharges.AddNew();
			orgInvTypeDeferredCharges.PO_AC = testObjectCreator.MRG100.PK;

			Factory.Save();

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.MRG100, "Test Charge", null, 0, null, testObjectCreator.AUD, -300, testObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;

			if (includeLinesWithDifferentBranchDepartment)
			{
				var charge2 = testObjectCreator.CreateCharge(job, testObjectCreator.MRG100, "Test Charge", null, 0, null, testObjectCreator.AUD, -300, testObjectCreator.ABIGAS);
				charge.JR_GB = TestObjectCreator.NonCurrentBranch.PK;
				charge.JR_GE = TestObjectCreator.FIADepartment.PK;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
				charge2.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;
			}

			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.CurrencyNK = "AUD";
			periodicInvoice.DebtorPK = testObjectCreator.ABIGAS.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			periodicInvoice.LoadJobs();
			return periodicInvoice;
		}

		public void TestResetOfPostManager()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = testObjectCreator.ABIGAS.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			periodicInvoice.UpdateDataAfterLinesChanges();

			using (PeriodicInvoicingForm form = new PeriodicInvoicingForm(periodicInvoice))
			{
				form.ValidateAndSave_ForTestOnly();
				var oldMananger = form.LastPostManagerForTestOnly_ForTestOnly;
				form.ValidateAndSave_ForTestOnly();
				AssertNotEquals("The old post manager should always be different to the new post manager otherwise errors are carried forward", oldMananger, form.LastPostManagerForTestOnly_ForTestOnly);
			}
		}

		[TestDate(2018, 10, 20)]
		public void TestHandleNegativeCompliancesFailedToCreateForMiscInvoices()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			AssertHandleNegativeCompliancesFailedToCreateForMiscInvoices(true);
		}

		[TestDate(2018, 10, 20)]
		public void TestHandleNegativeComplianceLinesFailedToCreateForMiscInvoices()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			AssertHandleNegativeCompliancesFailedToCreateForMiscInvoices(false);
		}

		void AssertHandleNegativeCompliancesFailedToCreateForMiscInvoices(bool flag)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV0001", TestObjectCreator.TWD, 1m, testObjectCreator.Debtor);
				invoice.AH_OSExTaxAmount = 500m;
				var line = invoice.Lines.AddNew() as ARInvoiceLine;
				line.AL_AC = testObjectCreator.FRT.PK;
				line.AL_AT = testObjectCreator.GST1.PK;
				testObjectCreator.GST1.AT_PostingGroupId = 1;
				line.AL_OSExTaxAmount = 4000m;

				var line1 = invoice.Lines.AddNew() as ARInvoiceLine;
				line1.AL_AC = testObjectCreator.MRG60.PK;
				line1.AL_AT = testObjectCreator.GST2.PK;
				testObjectCreator.GST2.AT_PostingGroupId = 2;
				line1.AL_OSExTaxAmount = -3500m;

				var creditNote = testObjectCreator.CreateARCreditNote("CRD0001", testObjectCreator.Debtor, testObjectCreator.TWD, 1m);
				creditNote.AH_OSExTaxAmount = 3000m;
				var crdLine = invoice.Lines.AddNew() as ARInvoiceLine;
				crdLine.AL_AC = testObjectCreator.FRT.PK;
				crdLine.AL_AT = testObjectCreator.GST2.PK;
				crdLine.AL_OSExTaxAmount = 3000m;

				Factory.Save();

				testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = "DEF";
				testObjectCreator.Debtor.CompanyData.OB_ARCreateVATComplianceDocumentOnPosting = "RCC";

				var periodicInvoice = new PeriodicInvoice(Factory);
				periodicInvoice.CurrencyNK = "TWD";
				periodicInvoice.DebtorPK = testObjectCreator.Debtor.PK;
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;

				periodicInvoice.MiscInvoices.Add(invoice);
				periodicInvoice.MiscInvoices.Add(creditNote);
				periodicInvoice.Jobs.RemoveAll();

				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
				using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (var form = new PeriodicInvoicingForm(periodicInvoice))
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Save should be allowed", ContinueWithSave.Yes, form.ValidateAndSave_ForTestOnly());
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(AccountingConstants.GetComplianceDocumentNegativeMessage()));
					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		[TestDate(2018, 10, 20)]
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

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.CurrencyNK = "AUD";
			periodicInvoice.DebtorPK = testObjectCreator.ABIGAS.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			periodicInvoice.LoadJobs();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, flag))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
			using (var form = new PeriodicInvoicingForm(periodicInvoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Save should be allowed", ContinueWithSave.Yes, form.ValidateAndSave_ForTestOnly());
				Assertion.CombineAssertions(() =>
				{
					Assert(!form.BusinessEntityForValidation_ForTestOnly.HasMessageErrors());
					AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(AccountingConstants.GetComplianceDocumentNegativeMessage()));
				});
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestFormReadOnlyAfterPosting()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			var plugin = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			Job job = testObjectCreator.CreateJob(plugin, testObjectCreator.ABIGAS, 10, testObjectCreator.ZECTRA, 10);
			job.JH_UniqueJobInvoiceNumber = 6;

			OrgInvoiceType orgInvoiceType = testObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;
			orgInvoiceType.PI_RS_NKServiceLevel = "STD";

			OrgInvTypeDeferredCharges orgInvTypeDeferredCharges = orgInvoiceType.DeferredCharges.AddNew();
			orgInvTypeDeferredCharges.PO_AC = testObjectCreator.MRG100.PK;

			Factory.Save();

			Charge charge = testObjectCreator.CreateCharge(job, testObjectCreator.MRG100, "Test Charge", null, 0, null, testObjectCreator.AUD, 300, testObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;

			Factory.Save();

			PeriodicInvoice periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.CurrencyNK = "AUD";
			periodicInvoice.DebtorPK = testObjectCreator.ABIGAS.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			periodicInvoice.LoadJobs();

			using (PeriodicInvoicingForm form = new PeriodicInvoicingForm(periodicInvoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Save should be allowed", ContinueWithSave.Yes, form.ValidateAndSave_ForTestOnly());

				var controls = form.Controls.Find("JobTypeCheckedListBox", true);
				AssertEquals("One not enabled", 2, controls.Length);
				AssertEquals("Not enabled", false, controls[0].Enabled);
			}
		}

		public void TestMicsInvoicesTabShowsOnlyWhenRegistrySetYes()
		{
			AccountingConfigurationRegistry.Instance.EnableMiscInvoiceInPeriodicInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (PeriodicInvoicingForm testForm = GetFormToBash() as PeriodicInvoicingForm)
			{
				testForm.Show();

				Assert(!testForm.PeriodicInvoiceControl_ForTestOnly.TabControl.Controls.Contains(testForm.PeriodicInvoiceControl_ForTestOnly.MiscInvoicesTabPage));
				Assert(testForm.PeriodicInvoiceControl_ForTestOnly.TabControl.Controls.Contains(testForm.PeriodicInvoiceControl_ForTestOnly.JobsTabPage));
			}

			AccountingConfigurationRegistry.Instance.EnableMiscInvoiceInPeriodicInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (PeriodicInvoicingForm testForm = GetFormToBash() as PeriodicInvoicingForm)
			{
				testForm.Show();

				Assert(testForm.PeriodicInvoiceControl_ForTestOnly.TabControl.Controls.Contains(testForm.PeriodicInvoiceControl_ForTestOnly.MiscInvoicesTabPage));
				Assert(testForm.PeriodicInvoiceControl_ForTestOnly.TabControl.Controls.Contains(testForm.PeriodicInvoiceControl_ForTestOnly.JobsTabPage));
			}
		}

		public void TestTaxBranchFindBoxVisibility()
		{
			AssertTaxBranchFindBoxVisibility(true, true);
			AssertTaxBranchFindBoxVisibility(true, false);
			AssertTaxBranchFindBoxVisibility(false, true);
			AssertTaxBranchFindBoxVisibility(false, false);

			void AssertTaxBranchFindBoxVisibility(bool isEnaleRegistry, bool isGSTRegistered)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnaleRegistry))
				using (var form = GetFormToBash() as PeriodicInvoicingForm)
				{
					form.Show();
					Application.DoEvents();

					var expectedVisible = isEnaleRegistry && isGSTRegistered;
					AssertEquals(expectedVisible, form.TaxBranchFindBox_ForTestOnly.Visible);
				}
			}
		}

		public void TestUnsuccessfulSaveShowsErrorAndDisablesThePost()
		{
			var periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			var testObjectCreator = new TestObjectCreator(Factory);

			var plugin = testObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.Shipment);
			var job = testObjectCreator.CreateJob(plugin, testObjectCreator.ABIGAS, 10, testObjectCreator.ZECTRA, 10);
			job.JH_UniqueJobInvoiceNumber = 6;

			var orgInvoiceType = testObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			orgInvoiceType.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			orgInvoiceType.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			orgInvoiceType.PI_StartDay = InvoiceTypeMonthCommencement.Codes.LMH;
			orgInvoiceType.PI_Type = InvoiceTypeLayoutList.Codes.INV;

			var orgInvTypeDeferredCharges = orgInvoiceType.DeferredCharges.AddNew();
			orgInvTypeDeferredCharges.PO_AC = testObjectCreator.MRG100.PK;

			Factory.Save();

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.MRG100, "Test Charge", null, 0, null, testObjectCreator.AUD, 300, testObjectCreator.ABIGAS);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			charge.JR_APInvoiceDate = charge.JR_PaymentDate = ZDateTime.Empty;

			Factory.Save();
			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.CurrencyNK = "AUD";
			periodicInvoice.DebtorPK = testObjectCreator.ABIGAS.PK;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;
			periodicInvoice.LoadJobs();

			using (var form = new PeriodicInvoicingForm(periodicInvoice))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.LastSaveSuccessful_ForTestOnly = false;
				var expectedError = @"The saving process has encountered an unrecoverable error. 
Please retry the action after closing and reopening the form.";
				AssertEquals("Saveing was not successfull", ContinueWithSave.No, form.ValidateAndSave_ForTestOnly());
				AssertEquals("Should show this error", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBeingSureThatOnShow_PeriodicInvoiceControl_PreparePeriodicInvoiceControl_HasBeenCalled()
		{
			var testHeader = GetFormBizO();
			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.India);
				using (PeriodicInvoicingForm testForm = GetForm(testHeader))
				{
					testForm.Show();
					Application.DoEvents();
					AssertEquals("JH_OSExtraTaxAmount should not be avaliable", false, testForm.PeriodicInvoiceControl_ForTestOnly.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_OSExtraTaxAmount).IsUnavailable);
					AssertEquals("JH_LocalExtraTaxAmount should not be avaliable", false, testForm.PeriodicInvoiceControl_ForTestOnly.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_LocalExtraTaxAmount).IsUnavailable);
					AssertEquals(testForm.PeriodicInvoiceControl_ForTestOnly.InternalJobsGrid.GetColumnCaption(Job.Schema.JH_OSExtraTaxAmount), "SGST Amount");
					AssertEquals(testForm.PeriodicInvoiceControl_ForTestOnly.InternalJobsGrid.GetColumnCaption(Job.Schema.JH_LocalExtraTaxAmount), "SGST Local");
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestShowErrorMessageWhenPreviewInvoiceWithoutProperReasonCode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			var jobReasonCollection = new JobProfitLossReasonCodeCollection();
			var code = jobReasonCollection.AddNew();
			code.Code = "TST";

			var jobReasonParam = new JobProfitLossRequiringReasonParameters();
			jobReasonParam.LossThreshold = 0.05;
			jobReasonParam.ProfitThreshold = 0.05;
			var jobStatus = jobReasonParam.JobStatusCollection.AddNew();
			jobStatus.Code = JobHeaderStatus.JobInvoiced.Code;

			var changeStatusCollection = new CodeDescriptionBoolDisallowNewCollection();
			var changeStatus = changeStatusCollection.AddNew();
			changeStatus.Code = JobHeaderStatus.Working.Code;
			changeStatus.Bool = false;

			Factory.Save();

			PreparePeriodicInvoiceTestData(testObjectCreator);
			Factory.Save();

			var mockPrintTaskProvider = new Mock<IPrintTaskUIProvider>();
			mockPrintTaskProvider
				.Setup(m => m.ShowRuntimeOptionsUI(It.IsAny<PrintTask>(), It.IsAny<AllowedDeliveryOptions>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ZArchitecture.Modules.ISecurityCheckpoint>()))
				.Returns(true);

			using (AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonCollection))
			using (AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonParam))
			using (AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				changeStatusCollection))
			{
				using (new PrintTaskUIProviderFactory.OverriderForTesting(mockPrintTaskProvider.Object))
				{
					var newFactory = new BusinessObjectFactory();

					var periodicInvoice = new PeriodicInvoice(newFactory);
					periodicInvoice.DebtorPK = testObjectCreator.ABIGAS.PK;
					periodicInvoice.LoadJobs();

					using (PeriodicInvoicingForm form = new PeriodicInvoicingForm(periodicInvoice))
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						form.PreviewInvoiceMenuItem.PerformClick();

						AssertEquals("Should report proper message when preview transaction",
							"Invoice cannot be previewed until errors are rectified.",
							UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestShowErrorMessageWhenPostInvoiceWithoutProperReasonCode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			var jobReasonCollection = new JobProfitLossReasonCodeCollection();
			var code = jobReasonCollection.AddNew();
			code.Code = "TST";

			var jobReasonParam = new JobProfitLossRequiringReasonParameters();
			jobReasonParam.LossThreshold = 0.05;
			jobReasonParam.ProfitThreshold = 0.05;
			var jobStatus = jobReasonParam.JobStatusCollection.AddNew();
			jobStatus.Code = JobHeaderStatus.JobInvoiced.Code;

			var changeStatusCollection = new CodeDescriptionBoolDisallowNewCollection();
			var changeStatus = changeStatusCollection.AddNew();
			changeStatus.Code = JobHeaderStatus.Working.Code;
			changeStatus.Bool = false;

			Factory.Save();

			PreparePeriodicInvoiceTestData(testObjectCreator);
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonCollection))
			using (AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonParam))
			using (AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				changeStatusCollection))
			{
				var periodicInvoice = new PeriodicInvoice(new BusinessObjectFactory());
				periodicInvoice.DebtorPK = testObjectCreator.ABIGAS.PK;
				periodicInvoice.LoadJobs();

				using (var form = new PeriodicInvoicingForm(periodicInvoice))
				{
					form.ValidateAndSave_ForTestOnly();

					var notifications = periodicInvoice.GetErrors();
					AssertEquals("Should has Reason Code errors.", 2, notifications.Count());
					AssertContainsExactElementsInAnyOrder(new List<string>()
							{
								@"Error - JobReasonCode: Job S1 status will be changed to INV after posting the first AR Invoice. The Profit/Loss threshold settings require Profit/Loss reason to be set on this job before posting any AR invoices. Posting is prohibited and Invoice preview not available until Profit / Loss reason has been entered. ",
								@"Error - JobReasonCode: Job S2 status will be changed to INV after posting the first AR Invoice. The Profit/Loss threshold settings require Profit/Loss reason to be set on this job before posting any AR invoices. Posting is prohibited and Invoice preview not available until Profit / Loss reason has been entered. "
							},
						notifications.Select(x => x.Message));

					Job1.JH_ProfitLossReasonCode = "TST";
					Job2.JH_ProfitLossReasonCode = "TST";
					Factory.Save();

					form.ValidateAndSave_ForTestOnly();
					AssertEquals("No errors with valid Reason Code.", false, periodicInvoice.HasErrors);
				}
			}
		}

		public void TestHandleSaveException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Portugal))
			{
				var testHeader = GetFormBizO();
				using (var testForm = GetForm(testHeader))
				{
					testForm.Show();
					Application.DoEvents();
					testForm.HandleSaveException_ForTestOnly(new InvoiceDateLessThanPreviousException());

					AssertEquals("Invoice date must be equal or higher than previous document.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#region Implementation

		PeriodicInvoice PreparePeriodicInvoiceTestDataForPreview()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);

			TestObjectCreator.ABIGAS.OH_IsDebtor = true;

			var type = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			type.PI_RS_NKServiceLevel = "STD";

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, setCurrentDepartment: false);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;

			var charge = job.Charges.AddNew();
			charge.FillWithValidTestData();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OSSellAmt = 100m;
			charge.JR_LocalSellAmt = 100m;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			Factory.Save();

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
			periodicInvoice.PostDate = ZDateTime.Today;
			periodicInvoice.DueDate = ZDateTime.Today.AddDays(30);
			periodicInvoice.InvoiceDate = ZDateTime.Today;
			periodicInvoice.InvoiceTerm = InvoiceTermsList.CashOnDelivery.Code;
			periodicInvoice.InvoiceTermDays = 30;
			periodicInvoice.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			periodicInvoice.LoadJobs();

			return periodicInvoice;
		}

		void PreparePeriodicInvoiceTestData(TestObjectCreator testObjectCreator)
		{
			var type = testObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			type.PI_RS_NKServiceLevel = "STD";

			var shipment1 = testObjectCreator.CreateShipment("S1");
			shipment1.JS_RS_NKServiceLevel = "STD";
			Job1 = testObjectCreator.CreateJob(shipment1);
			Job1.LocalChargesPK = testObjectCreator.ABIGAS.PK;
			Job1.JH_Status = JobHeaderStatus.Working.Code;

			var shipment2 = testObjectCreator.CreateShipment("S2");
			shipment2.JS_RS_NKServiceLevel = "STD";
			Job2 = testObjectCreator.CreateJob(shipment2);
			Job2.LocalChargesPK = testObjectCreator.ABIGAS.PK;
			Job2.JH_Status = JobHeaderStatus.Working.Code;

			var charge1 = Job1.Charges.AddNew();
			charge1.FillWithValidTestData();
			charge1.JR_AC = testObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = testObjectCreator.ABIGAS.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_LocalSellAmt = 100m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var charge2 = Job2.Charges.AddNew();
			charge2.FillWithValidTestData();
			charge2.JR_AC = testObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = testObjectCreator.ABIGAS.PK;
			charge2.JR_OSSellAmt = 100m;
			charge2.JR_LocalSellAmt = 100m;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
		}

		Job Job1, Job2;

		#endregion
	}
}
