using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceChargesApprovalBulkForm))]
	class APInvoiceChargesApprovalBulkFormBasherTest :
		TransactionApprovalBulkFormBasherTest<APInvoiceChargesApprovalBulkForm, InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest()
		{
			return Factory.New<APInvoiceChargesApprovalRequest>();
		}

		protected override APInvoiceChargesApprovalBulkForm GetRequestForm(params APInvoiceChargesApprovalRequest[] bizos)
		{
			return new APInvoiceChargesApprovalBulkForm(new APInvoiceChargesApprovalBulk(Factory, new InteractiveSecurityOverrideProvider(), bizos), ApprovalFormMode);
		}

		protected override TransactionApprovalFormModes[] ModesWhenReasonDescriptionShouldNotBeReadOnly
		{
			get { return new TransactionApprovalFormModes[] { TransactionApprovalFormModes.SetDescription, TransactionApprovalFormModes.Reject }; }
		}

		public void TestPlaceOfSupplyDropEditVisiblity()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				using (var testForm = GetFormToBash())
				{
					testForm.Show();
					var detailsGrid = testForm.GetControl<ZGrid>("DetailsGrid");
					AssertNotNull(detailsGrid.Columns["PlaceOfSupply"]);
				}
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (var testForm = GetFormToBash())
				{
					testForm.Show();
					var detailsGrid = testForm.GetControl<ZGrid>("DetailsGrid");
					AssertNull(detailsGrid.Columns["PlaceOfSupply"]);
				}
			}
		}
	}

	abstract class APInvoiceChargesApprovalBulkFormWithMultipleRequestsBasherTest :
		TransactionApprovalBulkFormWithMultipleRequestsBasherTest<APInvoiceChargesApprovalBulkForm, InvoicingBase, APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		#region TestPrintAfterPosting

		public void TestPrintAfterPostingWhenIsNotAllowed()
		{
			AssertPrintAfterPosting(false, false);
		}

		public void TestPrintAfterPostingWhenIsAllowed()
		{
			AssertPrintAfterPosting(true, false);
		}

		public void TestPrintAfterPostingWhenIsNotAllowed_BulkPosting()
		{
			AssertPrintAfterPosting(false, true);
		}

		public void TestPrintAfterPostingWhenIsAllowed_BulkPosting()
		{
			AssertPrintAfterPosting(true, true);
		}

		void AssertPrintAfterPosting(bool isPrintingAllowed, bool bulkPosting)
		{
			AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isPrintingAllowed);

			var requests = GetApprovalRequests(bulkPosting ? 2 : 1, 0);
			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			using (var bulkApprovalForm = GetRequestForm(requests))
			{
				bulkApprovalForm.Show();
				var saveAndPostButton = bulkApprovalForm
					.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
					.SaveButton;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (isPrintingAllowed)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				}

				saveAndPostButton.PerformClick();

				AssertEquals("Postcondition: XP_ApprovalStatus", Core.Constants.GenApprovalRequestApprovalStatus.Posted, requests[0].XP_ApprovalStatus);

				var expectedMessage = bulkPosting ? "Do you want to print Cost Confirmation Documents?" : "Do you want to print a Cost Confirmation Document for this transaction?";
				HelperMethodsForTests.AssertWindowsAndMessagesShown(isPrintingAllowed ? new[] { expectedMessage } : null, isPrintingAllowed ? "CostConfirmationDocTypePopupForm" : null);
			}
		}

		#endregion

		protected override void SetPostingSecurityRight(bool isAllowed)
		{
			Env.Security.APInvoiceApproval_Post.IsAllowed = isAllowed;
		}

		protected override APInvoiceChargesApprovalBulkForm GetRequestForm(params APInvoiceChargesApprovalRequest[] bizos)
		{
			return new APInvoiceChargesApprovalBulkForm(new APInvoiceChargesApprovalBulk(Factory, new InteractiveSecurityOverrideProvider(), bizos), ApprovalFormMode);
		}

		protected override TransactionApprovalFormModes[] ModesWhenReasonDescriptionShouldNotBeReadOnly
		{
			get { return new TransactionApprovalFormModes[] { TransactionApprovalFormModes.SetDescription, TransactionApprovalFormModes.Reject }; }
		}

		protected override bool ShouldSaveButtonVisibleAtFirst => true;

		protected override bool IsRequestIDDependsOnXP_ParentID => true;
	}

	[TestedType(typeof(APInvoiceChargesApprovalBulkForm))]
	class APInvoiceChargesApprovalBulkFormWithMultipleRequestsBasher_TransactionRelatedTest : APInvoiceChargesApprovalBulkFormWithMultipleRequestsBasherTest
	{
		public void TestPostingWithCostVarianceWhenRelatedJobIsReadyForFinancialClosureWithPostSecurity()
		{
			AssertPostingWithCostVarianceWhenRelatedJobIsReadyForFinancialClosure(true);
		}

		public void TestPostingWithCostVarianceWhenRelatedJobIsReadyForFinancialClosureWithOutPostSecurity()
		{
			AssertPostingWithCostVarianceWhenRelatedJobIsReadyForFinancialClosure(false);
		}

		void AssertPostingWithCostVarianceWhenRelatedJobIsReadyForFinancialClosure(bool isHavePostSecurity)
		{
			var valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			var above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			above.Range = RangeCodes.Above;
			above.Amount = 0M;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = true;

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false, false, localClientOrg: TestObjectCreator.LocalClient);
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10, 10);
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
			invoice.FillWithValidTestData();
			TestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100, setCurrentDepartment: false);

			var requestList = new List<APInvoiceChargesApprovalRequest>();
			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeInvoiceRelated(invoice);
			request.XP_ReasonDescription = "Test";
			request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			request.PrepareFoSaving();
			requestList.Add(request);

			Factory.Save();

			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			var cacheValue = Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed;

			using (new DisposableAction(() => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = isHavePostSecurity, () => Env.Security.AllowPostingChargesforFinancialClosureJob.IsAllowed = cacheValue))
			using (var bulkApprovalForm = GetRequestForm(requestList.ToArray()))
			{
				bulkApprovalForm.Show();
				var saveAndPostButton = bulkApprovalForm
					.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
					.SaveButton;

				saveAndPostButton.PerformClick();
				if (isHavePostSecurity)
				{
					AssertNoRowErrors(request);
				}
				else
				{
					AssertEquals(@"Error(s) have been found during posting. They can be fixed possibly by editing the related transaction. Do you want to edit it now?
Errors:
This request is approved, but related transaction has validation errors.
Job Number: Cannot post this charge, because the job has Jobs Ready for Financial Closure status.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				var newFactory = new BusinessObjectFactory();
				var invoiceInDB = newFactory.Load<APInvoice>(invoice.PK);
				var approvalRequestInDB = newFactory.Load<APInvoiceChargesApprovalRequest>(request.PK);

				AssertEquals("Postcondition: XP_ApprovalStatus", isHavePostSecurity ? Core.Constants.GenApprovalRequestApprovalStatus.Posted :
	Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestInDB.XP_ApprovalStatus);
				AssertEquals("Postcondition: invoice.IsPosted", isHavePostSecurity, invoiceInDB.IsPosted);
			}
		}

		#region TestPostingWithClosedJob

		public void TestPostingWithClosedJobWhenReopenIsNotAllowed()
		{
			AssertPostingWithClosedJob(false, false);
		}

		public void TestPostingWithClosedJobWhenReopenIsAllowedButUserDeclined()
		{
			AssertPostingWithClosedJob(true, false);
		}

		public void TestPostingWithClosedJobWhenReopenIsAllowed()
		{
			AssertPostingWithClosedJob(true, false, true);
		}

		public void TestPostingWithClosedJobWhenReopenIsNotAllowed_BulkPosting()
		{
			AssertPostingWithClosedJob(false, true);
		}

		public void TestPostingWithClosedJobWhenReopenIsAllowedButUserDeclined_BulkPosting()
		{
			AssertPostingWithClosedJob(true, true);
		}

		void AssertPostingWithClosedJob(bool isJobReopenAllowed, bool bulkPosting, bool agreeToReopen = false)
		{
			Env.Security.ReopenJob.IsAllowed = isJobReopenAllowed;

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false, false, localClientOrg: TestObjectCreator.LocalClient);
			Factory.Save();

			var newFactoryTestObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var invoice = newFactoryTestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
			invoice.FillWithValidTestData();
			newFactoryTestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 10, setCurrentDepartment: false);

			var requestList = new List<APInvoiceChargesApprovalRequest>();
			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeInvoiceRelated(invoice);
			request.XP_ReasonDescription = "Test";
			request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			request.PrepareFoSaving();
			requestList.Add(request);

			if (bulkPosting)
			{
				var invoice2 = newFactoryTestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
				invoice2.FillWithValidTestData();
				newFactoryTestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.GLHeader1.PK, 10);
				invoice2.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice2);

				var request2 = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
				request2.InitializeInvoiceRelated(invoice2);
				request2.XP_ReasonDescription = "Test2";
				request2.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
				request2.PrepareFoSaving();
				requestList.Add(request2);
			}

			job.Close((closingJob, message) => Fail($"Unexpected job closure error: {message}"), (sender, args) => Fail($"Unexpected job closure question: {args.QueryMessage}"));

			Factory.Save();

			Assert("Precondition: job is closed", job.IsClosed);

			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			using (var bulkApprovalForm = GetRequestForm(requestList.ToArray()))
			{
				bulkApprovalForm.Show();
				var saveAndPostButton = bulkApprovalForm
					.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
					.SaveButton;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				if (!isJobReopenAllowed || bulkPosting)
				{
					agreeToReopen = false;
				}
				if (isJobReopenAllowed)
				{
					UnitTestUserNotification.Instance.AddAnswer(agreeToReopen ? DialogResult.OK : DialogResult.Cancel);
				}

				saveAndPostButton.PerformClick();

				var expectedRowErrorMessage =
@"This request is approved, but related transaction has validation errors.
Accounts Payable Invoice: Posting requires job reopening.";
				var expectedErrorMessage =
$@"Error(s) have been found during posting. They can be fixed possibly by editing the related transaction. Do you want to edit it now?
Errors:
{expectedRowErrorMessage}";
				if (bulkPosting)
				{
					HelperMethodsForTests.AssertWindowsAndMessagesShown(new[] { BulkPostingMessageAboutErrors });
				}
				else
				{
					var expectedQuestion =
@"Closed Job(s) :S001
You are about to reopen these closed jobs. Do you want to proceed?";
					if (isJobReopenAllowed)
					{
						HelperMethodsForTests.AssertWindowsAndMessagesShown(agreeToReopen ? new[] { expectedQuestion } : new[] { expectedQuestion, expectedErrorMessage });
					}
					else
					{
						HelperMethodsForTests.AssertWindowsAndMessagesShown(new[] { expectedErrorMessage }, "LoginForm");
					}
				}

				var newFactory = new BusinessObjectFactory();
				var invoiceInDB = newFactory.Load<APInvoice>(invoice.PK);
				var approvalRequestInDB = newFactory.Load<APInvoiceChargesApprovalRequest>(request.PK);

				AssertEquals("Postcondition: XP_ApprovalStatus", agreeToReopen ? Core.Constants.GenApprovalRequestApprovalStatus.Posted :
					Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestInDB.XP_ApprovalStatus);
				if (!agreeToReopen)
				{
					AssertHasRowError(request, expectedRowErrorMessage);
				}
				else
				{
					AssertNoRowErrors(request);
				}
				AssertEquals("Postcondition: invoice.IsPosted", agreeToReopen, invoiceInDB.IsPosted);
				AssertEquals("job.IsClosed", !agreeToReopen, job.IsClosed);
			}
		}

		#endregion

		#region TestPostingWithCostVariance

		public void TestPostingWithCostVarianceWhenIsNotAllowed()
		{
			AssertPostingWithCostVariance(false, false);
		}

		public void TestPostingWithCostVarianceWhenIsAllowed()
		{
			AssertPostingWithCostVariance(true, false);
		}

		public void TestPostingWithCostVarianceWhenIsNotAllowed_BulkPosting()
		{
			AssertPostingWithCostVariance(false, true);
		}

		public void TestPostingWithCostVarianceWhenIsAllowed_BulkPosting()
		{
			AssertPostingWithCostVariance(true, true);
		}

		void AssertPostingWithCostVariance(bool isAllowed, bool bulkPosting)
		{
			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement above = valuesForTest.AuthorisationRequirements.AddNew();
			above.AuthorisationRequirement = AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			above.Range = RangeCodes.Above;
			above.Amount = 0m;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			Env.Security.CostVarianceApprovalLevel1.IsAllowed = isAllowed;

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment, false, false, localClientOrg: TestObjectCreator.LocalClient);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 10, 10);
			Factory.Save();

			var newFactoryTestObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var invoice = newFactoryTestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
			invoice.FillWithValidTestData();
			newFactoryTestObjectCreator.CreateInvoiceLine(invoice, job, TestObjectCreator.CC1, 100, setCurrentDepartment: false);

			var requestList = new List<APInvoiceChargesApprovalRequest>();
			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeInvoiceRelated(invoice);
			request.XP_ReasonDescription = "Test";
			request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			request.PrepareFoSaving();
			requestList.Add(request);

			if (bulkPosting)
			{
				var invoice2 = newFactoryTestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
				invoice2.FillWithValidTestData();
				newFactoryTestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.GLHeader1.PK, 10);
				invoice2.RunPreSaveValidation();
				AssertNoErrors("Precondition", invoice2);

				var request2 = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
				request2.InitializeInvoiceRelated(invoice2);
				request2.XP_ReasonDescription = "Test2";
				request2.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
				request2.PrepareFoSaving();
				requestList.Add(request2);
			}

			Factory.Save();

			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			using (var bulkApprovalForm = GetRequestForm(requestList.ToArray()))
			{
				bulkApprovalForm.Show();
				var saveAndPostButton = bulkApprovalForm
					.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
					.SaveButton;

				saveAndPostButton.PerformClick();

				var expectedRowErrorMessage =
@"This request is approved, but related transaction has validation errors.
Accounts Payable Invoice: You don't have security right to post this transaction.";
				HelperMethodsForTests.AssertWindowsAndMessagesShown(isAllowed ? null :
					bulkPosting ?
					new[] { BulkPostingMessageAboutErrors } :
					new[] {
$@"Error(s) have been found during posting. They can be fixed possibly by editing the related transaction. Do you want to edit it now?
Errors:
{expectedRowErrorMessage}" },
					isAllowed || bulkPosting ? null : "LoginForm");
				if (!isAllowed)
				{
					AssertHasRowError(request, expectedRowErrorMessage);
				}
				else
				{
					AssertNoRowErrors(request);
				}

				var newFactory = new BusinessObjectFactory();
				var invoiceInDB = newFactory.Load<APInvoice>(invoice.PK);
				var approvalRequestInDB = newFactory.Load<APInvoiceChargesApprovalRequest>(request.PK);

				AssertEquals("Approval request", request.PK, invoiceInDB.ApprovalRequestForAP.PK);
				AssertEquals("Postcondition: XP_ApprovalStatus", isAllowed ? Core.Constants.GenApprovalRequestApprovalStatus.Posted :
					Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestInDB.XP_ApprovalStatus);
				AssertEquals("Postcondition: invoice.IsPosted", isAllowed, invoiceInDB.IsPosted);
			}
		}

		#endregion

		#region TestPostingWithMixOfTaxIDPostingGroups

		public void TestPostMixOfTaxIDPostingGroupWhenNotAllowed()
		{
			AssertPostingWithMixOfTaxIDPostingGroups(false, false);
		}
		public void TestPostMixOfTaxIDPostingGroupWhenNotAllowed_ButUserPoceeded()
		{
			AssertPostingWithMixOfTaxIDPostingGroups(false, false, true);
		}

		public void TestPostMixOfTaxIDPostingGroupWhenAllowed()
		{
			AssertPostingWithMixOfTaxIDPostingGroups(true, false);
		}

		public void TestPostMixOfTaxIDPostingGroupNotAllowed_BulkPosting()
		{
			AssertPostingWithMixOfTaxIDPostingGroups(false, true);
		}

		public void TestPostMixOfTaxIDPostingGroupWhenAllowed_BulkPosting()
		{
			AssertPostingWithMixOfTaxIDPostingGroups(true, true);
		}

		void AssertPostingWithMixOfTaxIDPostingGroups(bool isAllowed, bool bulkPosting, bool agreeToPostAnyway = false)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(isAllowed ? "AU" : "VN"))
			{
				var tax1 = TestObjectCreator.GST1;
				var tax2 = TestObjectCreator.GSTFREE1;
				tax1.AT_PostingGroupId = 1;
				tax2.AT_PostingGroupId = 2;

				Factory.Save();

				var newFactoryTestObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
				var invoice = newFactoryTestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
				invoice.FillWithValidTestData();
				var line1 = newFactoryTestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader1.PK, 10);
				var line2 = newFactoryTestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.GLHeader2.PK, 10);
				line1.AL_AT = tax1.PK;
				line1.AL_AT = tax2.PK;

				var requestList = new List<APInvoiceChargesApprovalRequest>();
				var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
				request.InitializeInvoiceRelated(invoice);
				request.XP_ReasonDescription = "Test";
				request.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
				request.PrepareFoSaving();
				requestList.Add(request);

				if (bulkPosting)
				{
					var invoice2 = newFactoryTestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
					invoice2.FillWithValidTestData();
					newFactoryTestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.GLHeader1.PK, 10);
					invoice2.RunPreSaveValidation();
					AssertNoErrors("Precondition", invoice2);

					var request2 = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
					request2.InitializeInvoiceRelated(invoice2);
					request2.XP_ReasonDescription = "Test2";
					request2.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
					request2.PrepareFoSaving();
					requestList.Add(request2);
				}

				Factory.Save();

				AssertEquals("Precondition: invoice.HasInvalidPostingGroups", !isAllowed, invoice.HasInvalidPostingGroups);

				ApprovalFormMode = TransactionApprovalFormModes.Approve;
				using (var bulkApprovalForm = GetRequestForm(requestList.ToArray()))
				{
					bulkApprovalForm.Show();
					var saveAndPostButton = bulkApprovalForm
						.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
						.SaveButton;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					if (bulkPosting)
					{
						agreeToPostAnyway = false;
					}
					if (!isAllowed)
					{
						UnitTestUserNotification.Instance.AddAnswer(agreeToPostAnyway ? DialogResult.Yes : DialogResult.No);
					}

					saveAndPostButton.PerformClick();

					var expectedRowErrorMessage =
@"This request is approved, but related transaction has validation errors.
Accounts Payable Invoice: You have prepared charges using a mix of Tax ID Posting Groups.";
					string expectedErrorMessage =
$@"Error(s) have been found during posting. They can be fixed possibly by editing the related transaction. Do you want to edit it now?
Errors:
{expectedRowErrorMessage}";

					if (isAllowed)
					{
						HelperMethodsForTests.AssertWindowsAndMessagesShown();
					}
					else if (bulkPosting)
					{
						HelperMethodsForTests.AssertWindowsAndMessagesShown(new[] { BulkPostingMessageAboutErrors });
					}
					else
					{
						var expectedQuestion = "You have prepared charges using a mix of Tax ID Posting Groups. Are you sure you want to post these charges?";
						HelperMethodsForTests.AssertWindowsAndMessagesShown(agreeToPostAnyway ? new[] { expectedQuestion } : new[] { expectedQuestion, expectedErrorMessage });
					}

					var newFactory = new BusinessObjectFactory();
					var invoiceInDB = newFactory.Load<APInvoice>(invoice.PK);
					var approvalRequestInDB = newFactory.Load<APInvoiceChargesApprovalRequest>(request.PK);

					AssertEquals("Approval request", request.PK, invoiceInDB.ApprovalRequestForAP.PK);
					AssertEquals("Postcondition: XP_ApprovalStatus", isAllowed || agreeToPostAnyway ? Core.Constants.GenApprovalRequestApprovalStatus.Posted :
						Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestInDB.XP_ApprovalStatus);
					if (isAllowed || agreeToPostAnyway)
					{
						AssertNoRowErrors(request);
					}
					else
					{
						AssertHasRowError(request, expectedRowErrorMessage);
					}
					AssertEquals("Postcondition: invoice.IsPosted", isAllowed || agreeToPostAnyway, invoiceInDB.IsPosted);
				}
			}
		}

		#endregion

		protected override string EditFormTypeName => "InvoiceForm";

		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest(bool isValidParent)
		{
			var newFactoryTestObjectCreator = new TestObjectCreator(new BusinessObjectFactory());
			var invoice = newFactoryTestObjectCreator.CreateInvoice(typeof(APInvoice), organisation: TestObjectCreator.Creditor1);
			invoice.FillWithValidTestData();
			newFactoryTestObjectCreator.CreateInvoiceLine(invoice, (isValidParent ? TestObjectCreator.ExchangeGainLossControlAccount : InactiveTestGLAccount).PK, 10);

			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeInvoiceRelated(invoice);
			request.PrepareFoSaving();

			return request;
		}

		protected override string ErrorMessageForParentWithValidationError => "Generic Charge: Enter a valid selection.";

		protected override ZString ParentType => "transaction";
	}

	abstract class APInvoiceChargesApprovalBulkFormWithMultipleRequestsBasher_NonTransactionRelatedTest : APInvoiceChargesApprovalBulkFormWithMultipleRequestsBasherTest
	{
		#region TestPrintAfterPosting

		public void TestPostingWithMessages()
		{
			AssertPostingWithMessages(false);
		}

		public void TestPostingWithMessages_BulkPosting()
		{
			AssertPostingWithMessages(true);
		}

		void AssertPostingWithMessages(bool bulkPosting)
		{
			var requests = GetApprovalRequests(bulkPosting ? 2 : 1, 1);
			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			using (var bulkApprovalForm = GetRequestForm(requests))
			{
				bulkApprovalForm.Show();
				var saveAndPostButton = bulkApprovalForm
					.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
					.SaveButton;

				saveAndPostButton.PerformClick();

				AssertEquals("Postcondition: XP_ApprovalStatus", Core.Constants.GenApprovalRequestApprovalStatus.Approved, requests[0].XP_ApprovalStatus);

				var expectedMessage = bulkPosting ? BulkPostingMessageAboutErrors : ErrorMessageForParentWithValidationError;

				HelperMethodsForTests.AssertWindowsAndMessagesShown(new[] { expectedMessage });
				if (bulkPosting)
				{
					AssertHasRowError(requests[0], ErrorMessageForParentWithValidationError);
				}
				else
				{
					AssertNoRowErrors(requests[0]);
				}
			}
		}

		#endregion

		protected override string ErrorMessageForParentWithValidationError =>
@"No Transactions have been posted.

The following organizations must be marked as 'Payables':

	ABABEU";

		protected override bool IsTransactionRelatedRequest => false;
	}

	[TestedType(typeof(APInvoiceChargesApprovalBulkForm))]
	class APInvoiceChargesApprovalBulkFormWithMultipleRequestsBasher_JobRelatedTest : APInvoiceChargesApprovalBulkFormWithMultipleRequestsBasher_NonTransactionRelatedTest
	{
		protected override string EditFormTypeName => "ShipmentForm";

		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest(bool isValidParent)
		{
			var newFactory = new BusinessObjectFactory();
			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), true);
			var job = TestObjectCreator.CreateJob(shipment, false, false, localClientOrg: TestObjectCreator.LocalClient, newFactory: newFactory);
			var charge = TestObjectCreator.CreateCharge(job, creditor: isValidParent ? TestObjectCreator.Creditor1 : TestObjectCreator.InActiveOrg, invoiceNum: TestObjectCreator.GetRandomString(5));
			newFactory.Save();

			var apInvoiceCharges = new APInvoiceCharges(charge.CostAccount.OH_Code, charge.JR_APInvoiceNum, job.PK, job.TablePrefix, null);
			apInvoiceCharges.Charges.Add(charge);
			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeJobRelated(apInvoiceCharges, job.PK, job.TablePrefix);

			return request;
		}

		protected override ZString ParentType => "job";
	}

	[TestedType(typeof(APInvoiceChargesApprovalBulkForm))]
	class APInvoiceChargesApprovalBulkFormWithMultipleRequestsBasher_ConsolRelatedTest : APInvoiceChargesApprovalBulkFormWithMultipleRequestsBasher_NonTransactionRelatedTest
	{
		protected override string EditFormTypeName => "ConsolForm";

		protected override APInvoiceChargesApprovalRequest GetNewApprovalRequest(bool isValidParent)
		{
			var newFactory = new BusinessObjectFactory();
			var consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", TestObjectCreator.GetRandomString(4));
			var shipment = TestObjectCreator.CreateShipment(TestObjectCreator.GetRandomString(4), consol: consol, saveIt: true);
			var job = TestObjectCreator.CreateJob(shipment, false, false, localClientOrg: TestObjectCreator.LocalClient, newFactory: newFactory);
			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, isValidParent ? TestObjectCreator.Creditor1 : TestObjectCreator.InActiveOrg, apportionmentListing: new ApportionmentListing(newFactory, consol));
			consolCost.E6_InvoiceNum = TestObjectCreator.GetRandomString(5);
			consolCost.E6_InvoiceDate = ZDateTime.Now;
			consolCost.E6_PaymentDate = ZDateTime.Now;
			newFactory.Save();

			var apInvoiceCharges = new APInvoiceCharges(consolCost.Creditor.OH_Code, consolCost.E6_InvoiceNum, ZGuid.Empty, "", null);
			apInvoiceCharges.Charges.AddRange(consolCost.Factory.Load<Charge>(new ZQuery(JobChargeSchema.PK, consolCost.ApportionmentCharges.GetPKs())));
			var request = Factory.NewWithValidTestData<APInvoiceChargesApprovalRequest>();
			request.InitializeJobRelated(apInvoiceCharges, consol.PK, consol.TablePrefix);

			return request;
		}

		protected override ZString ParentType => "consol";
	}
}
