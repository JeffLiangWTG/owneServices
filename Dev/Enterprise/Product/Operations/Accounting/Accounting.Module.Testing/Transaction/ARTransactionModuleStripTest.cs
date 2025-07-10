using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.GUI.Riba;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Accounting.Business.Riba.AccCollectionOrder;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;
using static Enterprise.MasterFiles.Business.CountryCompliance.ChinaComplianceInfo;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARTransactionModuleStrip))]
	public class ARTransactionModuleStripTest : TransactionModuleStripTest
	{
		#region Credit Note Reversal Test Cases

		public void TestApprovalLevelRightsAreOnlyCheckedForARInvoiceAndPositiveARAdjustmentNoteReversal()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			Factory.Save();
			var transaction1 = TestObjectCreator.CreateARCreditNoteWithLine("CRD001", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1m, "", job, TestObjectCreator.CC1, 10m, ZDateTime.Today, false);
			var jobCharge = TestObjectCreator.CreateJobCharge(transaction1.Lines[0], job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			var transaction2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", -10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(transaction2, TestObjectCreator.CC1.PK, -10m, 0m);
			Factory.Save();

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two transactions in the grid", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Two transactions selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				var reverseMenuItem = testModule.DeleteMenuItem;
				AssertNotNull("Reverse menu item", reverseMenuItem);
				reverseMenuItem.PerformClick();

				using (var multipleReverseForm = testModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm)
				{
					AssertNotNull("Multiple reversing form", multipleReverseForm);
					var multipleReversingProvider = multipleReverseForm.BusinessEntity as MultipleReversingProviderForHeader;
					AssertNull("This should be null when approval levels are not checked", multipleReversingProvider.ApprovalFactory);
				}
			}
		}

		public void TestUserHasApprovalLevelRights_NoExisitingRequest()
		{
			var expectedError = "You do not have sufficient security rights to post a credit note for this amount and this reversal does not have an approved request at this time.";
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));
			var transaction2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(transaction2, TestObjectCreator.CC1.PK, 10m, 0m);
			Factory.Save();

			var approvalRequestForTransaction1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(0, approvalRequestForTransaction1.Length);
			var approvalRequestForTransaction2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(0, approvalRequestForTransaction2.Length);

			Assert(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two transactions in the grid", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Two transactions selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				var reverseMenuItem = testModule.DeleteMenuItem;
				AssertNotNull("Reverse menu item", reverseMenuItem);
				reverseMenuItem.PerformClick();

				using (var multipleReverseForm = testModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm)
				{
					AssertNotNull("Multiple reversing form", multipleReverseForm);
					var multipleReversingProvider = multipleReverseForm.BusinessEntity as MultipleReversingProviderForHeader;
					AssertNotNull("Multiple reversing provider", multipleReversingProvider);
					AssertEquals(0, multipleReversingProvider.TransactionsWithLevelAuthorizationProblems.Count);
					AssertEquals(2, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					AssertNoRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity, expectedError);
					AssertNoRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[1].WrappedBusinessEntity, expectedError);
					multipleReverseForm.FireSaveButton();
				}
			}

			var newFactory = new BusinessObjectFactory();
			var transaction1InNewFactory = newFactory.Load<InvoicingBase>(transaction1.PK);
			Assert(transaction1InNewFactory.IsReversed);
			var transaction2InNewFactory = newFactory.Load<InvoicingBase>(transaction2.PK);
			Assert(transaction2InNewFactory.IsReversed);
			approvalRequestForTransaction1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(0, approvalRequestForTransaction1.Length);
			approvalRequestForTransaction2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(0, approvalRequestForTransaction2.Length);
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndProvidesOnTheSpotAuthorization_NoExisitingRequest()
		{
			var expectedError = "You do not have sufficient security rights to post a credit note for this amount and this reversal does not have an approved request at this time.";
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));
			var transaction2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(transaction2, TestObjectCreator.CC1.PK, 10m, 0m);
			Factory.Save();

			var approvalRequestForTransaction1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(0, approvalRequestForTransaction1.Length);
			var approvalRequestForTransaction2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(0, approvalRequestForTransaction2.Length);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			SecurityTestObject.CreateTestUser(true, Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "US1", "User1", "pass");

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two transactions in the grid", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Two transactions selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((securityLoginform) =>
				{
					var loginForm = securityLoginform as LoginForm;
					if (loginForm != null)
					{
						loginForm.DoLoginForTest("User1", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
					}
				});

				var reverseMenuItem = testModule.DeleteMenuItem;
				AssertNotNull("Reverse menu item", reverseMenuItem);
				reverseMenuItem.PerformClick();

				using (var multipleReverseForm = testModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm)
				{
					AssertNotNull("Multiple reversing form", multipleReverseForm);
					var multipleReversingProvider = multipleReverseForm.BusinessEntity as MultipleReversingProviderForHeader;
					AssertNotNull("Multiple reversing provider", multipleReversingProvider);
					AssertEquals(0, multipleReversingProvider.TransactionsWithLevelAuthorizationProblems.Count);
					AssertEquals(2, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					AssertNoRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity, expectedError);
					AssertNoRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[1].WrappedBusinessEntity, expectedError);
					multipleReverseForm.FireSaveButton();
				}
			}

			var newFactory = new BusinessObjectFactory();
			var transaction1InNewFactory = newFactory.Load<InvoicingBase>(transaction1.PK);
			Assert(transaction1InNewFactory.IsReversed);
			var transaction2InNewFactory = newFactory.Load<InvoicingBase>(transaction2.PK);
			Assert(transaction2InNewFactory.IsReversed);
			approvalRequestForTransaction1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(0, approvalRequestForTransaction1.Length);
			approvalRequestForTransaction2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(0, approvalRequestForTransaction2.Length);
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndCancelOutFromSecurityOverrideScreen_NoExisitingRequest()
		{
			var expectedError = "You do not have sufficient security rights to post a credit note for this amount and this reversal does not have an approved request at this time.";
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));
			var transaction2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(transaction2, TestObjectCreator.CC1.PK, 10m, 0m);
			Factory.Save();

			var approvalRequestForTransaction1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(0, approvalRequestForTransaction1.Length);
			var approvalRequestForTransaction2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(0, approvalRequestForTransaction2.Length);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			SecurityTestObject.CreateTestUser(true, Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "US1", "User1", "pass");

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two transactions in the grid", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Two transactions selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((securityLoginform) =>
				{
					var loginForm = securityLoginform as LoginForm;
					if (loginForm != null)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel; //user cancel out from login screen
					}
				});

				var reverseMenuItem = testModule.DeleteMenuItem;
				AssertNotNull("Reverse menu item", reverseMenuItem);
				reverseMenuItem.PerformClick();

				using (var multipleReverseForm = testModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm)
				{
					AssertNotNull("Multiple reversing form", multipleReverseForm);
					var multipleReversingProvider = multipleReverseForm.BusinessEntity as MultipleReversingProviderForHeader;
					AssertNotNull("Multiple reversing provider", multipleReversingProvider);
					AssertEquals(2, multipleReversingProvider.TransactionsWithLevelAuthorizationProblems.Count);
					AssertEquals(2, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					AssertHasRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity, expectedError);
					AssertHasRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[1].WrappedBusinessEntity, expectedError);
					UnitTestUserNotification.Instance.ClearMessages();
					multipleReverseForm.FireSaveButton();
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Multiple Reverse Form is not closed, due to validation errors", multipleReverseForm.Visible);
					UnitTestUserNotification.Instance.ClearMessages();
					multipleReverseForm.CancelButton.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			var newFactory = new BusinessObjectFactory();
			var transaction1InNewFactory = newFactory.Load<InvoicingBase>(transaction1.PK);
			Assert(!transaction1InNewFactory.IsReversed);
			var transaction2InNewFactory = newFactory.Load<InvoicingBase>(transaction2.PK);
			Assert(!transaction2InNewFactory.IsReversed);
			approvalRequestForTransaction1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(0, approvalRequestForTransaction1.Length);
			approvalRequestForTransaction2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(0, approvalRequestForTransaction2.Length);
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndWantsToCreateApprovalRequests_NoExisitingRequest()
		{
			var expectedError = "You do not have sufficient security rights to post a credit note for this amount and this reversal does not have an approved request at this time.";
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));
			var transaction2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(transaction2, TestObjectCreator.CC1.PK, 10m, 0m);
			Factory.Save();

			var approvalRequestForTransaction1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(0, approvalRequestForTransaction1.Length);
			var approvalRequestForTransaction2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(0, approvalRequestForTransaction2.Length);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two transactions in the grid", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Two transactions selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((securityLoginform) =>
				{
					var loginForm = securityLoginform as LoginForm;
					if (loginForm != null)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; //user wants to create requests
					}
				});

				var reverseMenuItem = testModule.DeleteMenuItem;
				AssertNotNull("Reverse menu item", reverseMenuItem);
				reverseMenuItem.PerformClick();

				using (var multipleReverseForm = testModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm)
				{
					AssertNotNull("Multiple reversing form", multipleReverseForm);
					var multipleReversingProvider = multipleReverseForm.BusinessEntity as MultipleReversingProviderForHeader;
					AssertNotNull("Multiple reversing provider", multipleReversingProvider);
					AssertEquals(2, multipleReversingProvider.TransactionsWithLevelAuthorizationProblems.Count);
					AssertEquals(2, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					AssertHasRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity, expectedError);
					AssertHasRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[1].WrappedBusinessEntity, expectedError);
					UnitTestUserNotification.Instance.ClearMessages();
					multipleReverseForm.FireSaveButton();
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Multiple Reverse Form is not closed, due to validation errors", multipleReverseForm.Visible);
					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddOKAnswer();
					multipleReverseForm.CancelButton.PerformClick();
					AssertContains("You have pending credit note approval requests that will be lost if you cancel.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			var newFactory = new BusinessObjectFactory();
			var transaction1InNewFactory = newFactory.Load<InvoicingBase>(transaction1.PK);
			Assert(!transaction1InNewFactory.IsReversed);
			var transaction2InNewFactory = newFactory.Load<InvoicingBase>(transaction2.PK);
			Assert(!transaction2InNewFactory.IsReversed);
			approvalRequestForTransaction1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(1, approvalRequestForTransaction1.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestForTransaction1[0].XP_ApprovalStatus);
			approvalRequestForTransaction2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(1, approvalRequestForTransaction2.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestForTransaction2[0].XP_ApprovalStatus);
		}

		public void TestUserHasApprovalLevelRights_ExistingREQAndAPPRequest()
		{
			var expectedError = "You do not have sufficient security rights to post a credit note for this amount and this reversal does not have an approved request at this time.";
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();

			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));
			var transaction2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(transaction2, TestObjectCreator.CC1.PK, 10m, 0m);
			Factory.Save();

			var request1 = CreateApprovalRequest(transaction1, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			var request2 = CreateApprovalRequest(transaction2, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			request1.PostingDetails.MaxAuthorisationLevelRequired = 1;
			request2.PostingDetails.MaxAuthorisationLevelRequired = 1;
			Factory.Save();

			var approvalRequestForTransaction1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(1, approvalRequestForTransaction1.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestForTransaction1[0].XP_ApprovalStatus);
			var approvalRequestForTransaction2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(1, approvalRequestForTransaction2.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestForTransaction2[0].XP_ApprovalStatus);

			Assert(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two transactions in the grid", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Two transactions selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				var reverseMenuItem = testModule.DeleteMenuItem;
				AssertNotNull("Reverse menu item", reverseMenuItem);
				reverseMenuItem.PerformClick();

				using (var multipleReverseForm = testModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm)
				{
					AssertNotNull("Multiple reversing form", multipleReverseForm);
					var multipleReversingProvider = multipleReverseForm.BusinessEntity as MultipleReversingProviderForHeader;
					AssertNotNull("Multiple reversing provider", multipleReversingProvider);
					AssertEquals(0, multipleReversingProvider.TransactionsWithLevelAuthorizationProblems.Count);
					AssertEquals(2, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					AssertNoRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity, expectedError);
					AssertNoRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[1].WrappedBusinessEntity, expectedError);
					multipleReverseForm.FireSaveButton();
				}
			}

			var newFactory = new BusinessObjectFactory();
			var transaction1InNewFactory = newFactory.Load<InvoicingBase>(transaction1.PK);
			Assert(transaction1InNewFactory.IsReversed);
			var transaction2InNewFactory = newFactory.Load<InvoicingBase>(transaction2.PK);
			Assert(transaction2InNewFactory.IsReversed);
			approvalRequestForTransaction1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(1, approvalRequestForTransaction1.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequestForTransaction1[0].XP_ApprovalStatus);
			approvalRequestForTransaction2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(1, approvalRequestForTransaction2.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequestForTransaction2[0].XP_ApprovalStatus);
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndProvidesOnTheSpotAuthorization_ExisitngREQAndAPPRequest()
		{
			var expectedError = "You do not have sufficient security rights to post a credit note for this amount and this reversal does not have an approved request at this time.";
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));
			var transaction2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(transaction2, TestObjectCreator.CC1.PK, 10m, 0m);
			Factory.Save();

			var request1 = CreateApprovalRequest(transaction1, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			var request2 = CreateApprovalRequest(transaction2, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			request1.PostingDetails.MaxAuthorisationLevelRequired = 1;
			request2.PostingDetails.MaxAuthorisationLevelRequired = 1;
			Factory.Save();

			var approvalRequestForTransaction1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(1, approvalRequestForTransaction1.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestForTransaction1[0].XP_ApprovalStatus);
			var approvalRequestForTransaction2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(1, approvalRequestForTransaction2.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestForTransaction2[0].XP_ApprovalStatus);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			SecurityTestObject.CreateTestUser(true, Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "US1", "User1", "pass");

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two transactions in the grid", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Two transactions selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((securityLoginform) =>
				{
					var loginForm = securityLoginform as LoginForm;
					if (loginForm != null)
					{
						loginForm.DoLoginForTest("User1", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
					}
				});

				var reverseMenuItem = testModule.DeleteMenuItem;
				AssertNotNull("Reverse menu item", reverseMenuItem);
				reverseMenuItem.PerformClick();

				using (var multipleReverseForm = testModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm)
				{
					AssertNotNull("Multiple reversing form", multipleReverseForm);
					var multipleReversingProvider = multipleReverseForm.BusinessEntity as MultipleReversingProviderForHeader;
					AssertNotNull("Multiple reversing provider", multipleReversingProvider);
					AssertEquals(0, multipleReversingProvider.TransactionsWithLevelAuthorizationProblems.Count);
					AssertEquals(2, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					AssertNoRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity, expectedError);
					AssertNoRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[1].WrappedBusinessEntity, expectedError);
					multipleReverseForm.FireSaveButton();
				}
			}

			var newFactory = new BusinessObjectFactory();
			var transaction1InNewFactory = newFactory.Load<InvoicingBase>(transaction1.PK);
			Assert(transaction1InNewFactory.IsReversed);
			var transaction2InNewFactory = newFactory.Load<InvoicingBase>(transaction2.PK);
			Assert(transaction2InNewFactory.IsReversed);
			approvalRequestForTransaction1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(1, approvalRequestForTransaction1.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequestForTransaction1[0].XP_ApprovalStatus);
			approvalRequestForTransaction2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(1, approvalRequestForTransaction2.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequestForTransaction2[0].XP_ApprovalStatus);
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndCancelOutFromSecurityOverrideScreen_ExisitngREQAndAPPRequest()
		{
			var expectedError = "You do not have sufficient security rights to post a credit note for this amount and this reversal does not have an approved request at this time.";
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));
			var transaction2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(transaction2, TestObjectCreator.CC1.PK, 10m, 0m);
			Factory.Save();

			var request1 = CreateApprovalRequest(transaction1, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			var request2 = CreateApprovalRequest(transaction2, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
			request1.PostingDetails.MaxAuthorisationLevelRequired = 1;
			request2.PostingDetails.MaxAuthorisationLevelRequired = 1;
			Factory.Save();

			var approvalRequestForTransaction1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(1, approvalRequestForTransaction1.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestForTransaction1[0].XP_ApprovalStatus);
			var approvalRequestForTransaction2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(1, approvalRequestForTransaction2.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestForTransaction2[0].XP_ApprovalStatus);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two transactions in the grid", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Two transactions selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((securityLoginform) =>
				{
					var loginForm = securityLoginform as LoginForm;
					if (loginForm != null)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel; //user cancel out from login screen
					}
				});

				var reverseMenuItem = testModule.DeleteMenuItem;
				AssertNotNull("Reverse menu item", reverseMenuItem);
				reverseMenuItem.PerformClick();

				using (var multipleReverseForm = testModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm)
				{
					AssertNotNull("Multiple reversing form", multipleReverseForm);
					var multipleReversingProvider = multipleReverseForm.BusinessEntity as MultipleReversingProviderForHeader;
					AssertNotNull("Multiple reversing provider", multipleReversingProvider);
					AssertEquals(1, multipleReversingProvider.TransactionsWithLevelAuthorizationProblems.Count);
					AssertEquals(2, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					var reversedTransactionsWithErrors = multipleReversingProvider.TransactionsAlreadyReversed.Where(x => ((BusinessObject)x.WrappedBusinessEntity).RowErrors.Contains(expectedError)).ToList();
					AssertEquals(1, reversedTransactionsWithErrors.Count);
					multipleReversingProvider.TransactionsAlreadyReversed.Remove(reversedTransactionsWithErrors[0]); //stimulate user removing transaction 1 from grid because it has error
					AssertEquals(1, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					multipleReverseForm.FireSaveButton();
				}
			}

			var newFactory = new BusinessObjectFactory();
			var transaction1InNewFactory = newFactory.Load<InvoicingBase>(transaction1.PK);
			Assert(!transaction1InNewFactory.IsReversed);
			var transaction2InNewFactory = newFactory.Load<InvoicingBase>(transaction2.PK);
			Assert(transaction2InNewFactory.IsReversed);
			approvalRequestForTransaction1 = newFactory.Load<ARCreditNoteApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(1, approvalRequestForTransaction1.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestForTransaction1[0].XP_ApprovalStatus);
			approvalRequestForTransaction2 = newFactory.Load<ARCreditNoteApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(1, approvalRequestForTransaction2.Length);
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequestForTransaction2[0].XP_ApprovalStatus);
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndWantsToCreateApprovalRequests_ExisitngREQRequest_LikeToCancelExisitngREQRequest()
		{
			var expectedError = "You do not have sufficient security rights to post a credit note for this amount and this reversal does not have an approved request at this time.";
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));
			var transaction2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(transaction2, TestObjectCreator.CC1.PK, 10m, 0m);
			Factory.Save();

			CreateApprovalRequest(transaction1, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			CreateApprovalRequest(transaction2, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			Factory.Save();

			var approvalRequestForTransaction1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(1, approvalRequestForTransaction1.Length);
			var previousRequestForTransaction1 = approvalRequestForTransaction1[0];
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, previousRequestForTransaction1.XP_ApprovalStatus);
			var approvalRequestForTransaction2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(1, approvalRequestForTransaction2.Length);
			var previousRequestForTransaction2 = approvalRequestForTransaction2[0];
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, previousRequestForTransaction2.XP_ApprovalStatus);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two transactions in the grid", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Two transactions selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((securityLoginform) =>
				{
					var loginForm = securityLoginform as LoginForm;
					if (loginForm != null)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; //user wants to create new request
					}
				});

				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //user wants to cancel exisitng requests

				var reverseMenuItem = testModule.DeleteMenuItem;
				AssertNotNull("Reverse menu item", reverseMenuItem);
				reverseMenuItem.PerformClick();

				using (var multipleReverseForm = testModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm)
				{
					AssertNotNull("Multiple reversing form", multipleReverseForm);
					var multipleReversingProvider = multipleReverseForm.BusinessEntity as MultipleReversingProviderForHeader;
					AssertNotNull("Multiple reversing provider", multipleReversingProvider);
					AssertEquals(2, multipleReversingProvider.TransactionsWithLevelAuthorizationProblems.Count);
					AssertEquals(2, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					AssertHasRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity, expectedError);
					AssertHasRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[1].WrappedBusinessEntity, expectedError);
					UnitTestUserNotification.Instance.ClearMessages();
					multipleReverseForm.FireSaveButton();
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Multiple Reverse Form is not closed, due to validation errors", multipleReverseForm.Visible);
					multipleReversingProvider.TransactionsAlreadyReversed.RemoveAll(); //stimulate user removing both transactions as they have errors
					AssertEquals("No transactions available for reversing", 0, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddOKAnswer();
					multipleReverseForm.CancelButton.PerformClick();
					AssertContains("You have pending credit note approval requests that will be lost if you cancel.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			var newFactory = new BusinessObjectFactory();
			var transaction1InNewFactory = newFactory.Load<InvoicingBase>(transaction1.PK);
			Assert(!transaction1InNewFactory.IsReversed);
			var transaction2InNewFactory = newFactory.Load<InvoicingBase>(transaction2.PK);
			Assert(!transaction2InNewFactory.IsReversed);
			approvalRequestForTransaction1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(2, approvalRequestForTransaction1.Length);
			AssertEquals("Previous request cancelled", previousRequestForTransaction1.PK, approvalRequestForTransaction1.First(x => x.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Cancelled).PK);
			AssertNotEquals("New request created", previousRequestForTransaction1.PK, approvalRequestForTransaction1.First(x => x.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Requested).PK);
			approvalRequestForTransaction2 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(2, approvalRequestForTransaction2.Length);
			AssertEquals("Previous request cancelled", previousRequestForTransaction2.PK, approvalRequestForTransaction2.First(x => x.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Cancelled).PK);
			AssertNotEquals("New request created", previousRequestForTransaction2.PK, approvalRequestForTransaction2.First(x => x.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Requested).PK);
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndWantsToCreateApprovalRequests_ExisitngREQRequest_DoNotWantToCancelExistingREQRequest()
		{
			var expectedError = "You do not have sufficient security rights to post a credit note for this amount and this reversal does not have an approved request at this time.";
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			Factory.Save();
			var transaction1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			transaction1.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction1.PK));
			var transaction2 = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR002", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
			TestObjectCreator.CreateAdjusmentNoteLine(transaction2, TestObjectCreator.CC1.PK, 10m, 0m);
			Factory.Save();

			CreateApprovalRequest(transaction1, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			CreateApprovalRequest(transaction2, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
			Factory.Save();

			var approvalRequestForTransaction1 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(1, approvalRequestForTransaction1.Length);
			var previousRequestForTransaction1 = approvalRequestForTransaction1[0];
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, previousRequestForTransaction1.XP_ApprovalStatus);
			var approvalRequestForTransaction2 = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction2.PK));
			AssertEquals(1, approvalRequestForTransaction2.Length);
			var previousRequestForTransaction2 = approvalRequestForTransaction2[0];
			AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, previousRequestForTransaction2.XP_ApprovalStatus);

			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("Two transactions in the grid", 2, collection.Count);

				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Two transactions selected", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((securityLoginform) =>
				{
					var loginForm = securityLoginform as LoginForm;
					if (loginForm != null)
					{
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; //user wants to create new request
					}
				});

				UnitTestUserNotification.Instance.ClearUserResponses();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //user does not want to cancel exisitng requests

				var reverseMenuItem = testModule.DeleteMenuItem;
				AssertNotNull("Reverse menu item", reverseMenuItem);
				reverseMenuItem.PerformClick();

				using (var multipleReverseForm = testModule.LastFormShownForTest_ForTestOnly as MultipleReversingBaseForm)
				{
					AssertNotNull("Multiple reversing form", multipleReverseForm);
					var multipleReversingProvider = multipleReverseForm.BusinessEntity as MultipleReversingProviderForHeader;
					AssertNotNull("Multiple reversing provider", multipleReversingProvider);
					AssertEquals(2, multipleReversingProvider.TransactionsWithLevelAuthorizationProblems.Count);
					AssertEquals(2, multipleReversingProvider.TransactionsAlreadyReversed.Count);
					AssertHasRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity, expectedError);
					AssertHasRowError((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[1].WrappedBusinessEntity, expectedError);
					UnitTestUserNotification.Instance.ClearMessages();
					multipleReverseForm.FireSaveButton();
					AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Multiple Reverse Form is not closed, due to validation errors", multipleReverseForm.Visible);
					UnitTestUserNotification.Instance.ClearMessages();
					UnitTestUserNotification.Instance.AddOKAnswer();
					multipleReverseForm.CancelButton.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			var newFactory = new BusinessObjectFactory();
			var transaction1InNewFactory = newFactory.Load<InvoicingBase>(transaction1.PK);
			Assert(!transaction1InNewFactory.IsReversed);
			var transaction2InNewFactory = newFactory.Load<InvoicingBase>(transaction2.PK);
			Assert(!transaction2InNewFactory.IsReversed);
			approvalRequestForTransaction1 = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction1.PK));
			AssertEquals(1, approvalRequestForTransaction1.Length);
			AssertEquals("Previous request not cancelled", previousRequestForTransaction1.PK, approvalRequestForTransaction1[0].PK);
			AssertEquals(1, approvalRequestForTransaction2.Length);
			AssertEquals("Previous request not cancelled", previousRequestForTransaction2.PK, approvalRequestForTransaction2[0].PK);
		}

		AuthorizationModeAndSettings GetAuthorisationConfigSetting()
		{
			var setting = new AuthorizationModeAndSettings();
			var collection = setting.AuthorisationSettings;
			var upToPaymentAuthorisationSettings = collection.AddNew();
			upToPaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToPaymentAuthorisationSettings.Amount = 10;
			upToPaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			var abovePaymentAuthorisationSettings = collection.AddNew();
			abovePaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			abovePaymentAuthorisationSettings.Amount = 10;
			abovePaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			return setting;
		}

		ARCreditNoteApprovalRequest CreateApprovalRequest(InvoicingBase parent, ZString approvalStatus)
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.ChangeApprovalTypeForInvoiceReversal();
			approvalRequest.Initialize(new[] { parent }, parent.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			approvalRequest.XP_ApprovalStatus = approvalStatus;
			return approvalRequest;
		}

		#endregion

		#region Test PendingInvoiceActionMenuItem

		public void TestPendingInvoiceActionMenuItemExistInKorea()
		{
			GetPendingInvoiceActionMenuItemAndAssert(CountryCodes.KoreaSouth, true, EInvoicingPivotState.Pending, true);
			GetPendingInvoiceActionMenuItemAndAssert(CountryCodes.Australia, true, EInvoicingPivotState.Pending, false);
			GetPendingInvoiceActionMenuItemAndAssert(CountryCodes.KoreaSouth, false, EInvoicingPivotState.Pending, false);
			GetPendingInvoiceActionMenuItemAndAssert(CountryCodes.KoreaSouth, true, EInvoicingPivotState.Queued, false);
		}

		void GetPendingInvoiceActionMenuItemAndAssert(string countryCode, bool enableEInvoicingFunctionality, string eReportingSubmitPivotDefaultStatus, bool hasActionMenuItem)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enableEInvoicingFunctionality))
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, eReportingSubmitPivotDefaultStatus))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();
				var pendingInvoiceActionMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText("Authorize and Send");
				if (hasActionMenuItem)
				{
					AssertNotNull("Authorize And Send Menu Item should exist", pendingInvoiceActionMenuItem);
				}
				else
				{
					AssertNull("Authorize And Send Menu Item should not exist", pendingInvoiceActionMenuItem);
				}
			}
		}

		#endregion

		[TestDate(2020, 10, 1)]
		public void TestAllocateComplianceNumberAndCreatingOfSubmitPivotForTurkeyEInvoice()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
			var orgProxy = TestObjectCreator.CreateOrgHeader("AAA", true, true);
			var turkeyCompany = TestObjectCreator.CreateNewCompany("DTR", "TR", orgProxy: orgProxy);
			var branch = TestObjectCreator.CreateBranch("BTR", turkeyCompany, orgProxy);
			var department = TestObjectCreator.CreateDepartment("DTR");
			TestObjectCreator.SetCustomsCodeForOrgHeader(turkeyCompany.OrgProxy, OrgCusCode.CodeTypes.VATCode, CountryCodes.Turkey, "1234567890");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), department.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var allocateComplianceNumberMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Number");
				AssertNotNull("Allocate Compliance Number Menu Item should exist", allocateComplianceNumberMenuItem);

				var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				complianceSequence.XD_GC_Company = turkeyCompany.PK;
				complianceSequence.XD_SequenceClass = "EIN";
				complianceSequence.XD_Prefix = "AAA2021";
				complianceSequence.XD_MaximumNumberDigits = 9;
				complianceSequence.XD_StartNumber = 1000;
				complianceSequence.XD_EndNumber = 1010;
				complianceSequence.XD_NextNumber = 1001;
				complianceSequence.XD_StartDate = new ZDate(2020, 9, 1);
				complianceSequence.XD_ExpiryDate = new ZDate(2020, 10, 31);
				Factory.Save();

				var invoice1 = CreateInvoice();
				var arInvLine1 = (ARInvoiceLine)invoice1.Lines.AddNew();
				arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = 300m;
				arInvLine1.AL_AC = TestObjectCreator.FRT.PK;
				arInvLine1.AL_AT = TestObjectCreator.GST1.PK;
				invoice1.AH_ComplianceSubType = "EIN";
				Factory.Save();

				AssertEquals("Compliance Number has to be empty", "", invoice1.AH_TransactionReference);

				var pivot = GetEInvoicingPivot(invoice1.PK);
				AssertNull("invoice1 should not be queued", pivot);

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();

				testModule.DisplayGrid.SelectAllElements();
				allocateComplianceNumberMenuItem.PerformClick();

				AssertEquals("AAA2021000001001", invoice1.AH_TransactionReference);

				pivot = GetEInvoicingPivot(invoice1.PK);
				AssertNotNull("invoice1 should be queued", pivot);
				AssertEquals(EInvoicingPivotActionType.Submit, pivot.AIP_ActionType);
				AssertEquals(EInvoicingPivotState.Queued, pivot.AIP_Status);
			}
		}

		[TestDate(2020, 10, 1)]
		public void TestAllocateComplianceNumberAndCreatingOfSubmitPivotForReversedCreditNoteTurkeyIfOriginalInvoiceDoesNotHavePivot()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
			var orgProxy = TestObjectCreator.CreateOrgHeader("AAA", true, true);
			var turkeyCompany = TestObjectCreator.CreateNewCompany("DTR", "TR", orgProxy: orgProxy);
			var branch = TestObjectCreator.CreateBranch("BTR", turkeyCompany, orgProxy);
			var department = TestObjectCreator.CreateDepartment("DTR");
			TestObjectCreator.SetCustomsCodeForOrgHeader(turkeyCompany.OrgProxy, OrgCusCode.CodeTypes.VATCode, CountryCodes.Turkey, "1234567890");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Turkey))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), department.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var allocateComplianceNumberMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Number");
				AssertNotNull("Allocate Compliance Number Menu Item should exist", allocateComplianceNumberMenuItem);

				TestObjectCreator.CreateNewComplianceSequence(turkeyCompany.PK, branch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.EAR);
				TestObjectCreator.CreateNewComplianceSequence(turkeyCompany.PK, branch.PK, TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN);

				var arInvoice = CreateInvoice() as ARInvoice;
				var arInvLine1 = (ARInvoiceLine)arInvoice.Lines.AddNew();
				arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = 300m;
				arInvLine1.AL_AC = TestObjectCreator.FRT.PK;
				arInvLine1.AL_AT = TestObjectCreator.GST1.PK;
				arInvoice.AH_ComplianceSubType = "EAR";
				Factory.Save();

				var creditNote = TestObjectCreator.CreateReversalTransaction(arInvoice, "") as ARCreditNote;
				creditNote.AH_ComplianceSubType = TurkeyComplianceInfo.ComplianceSubTypeCodes.ICN;
				Factory.Save();

				AssertEquals("Compliance Number has to be empty", "", creditNote.AH_TransactionReference);
				AssertEquals(false, creditNote.IsEligibleToCreateEInvoicingTransactionPivot);

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, creditNote.PK));
				AssertNull("AR Invoice should not be queued", pivot);

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();
				testModule.DisplayGrid.SelectAllElements(x => x is InvoicingBase invoicingBase && invoicingBase.AH_TransactionType == TransactionTypes.CreditNote);
				allocateComplianceNumberMenuItem.PerformClick();

				AssertEquals(false, creditNote.AH_TransactionReference.IsEmpty);
				AssertEquals(false, creditNote.IsEligibleToCreateEInvoicingTransactionPivot);

				pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, creditNote.PK));
				AssertNull("Credit note should not be queued", pivot);
			}
		}

		AccEInvoicingTransactionPivot GetEInvoicingPivot(ZGuid parentID)
		{
			return Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, parentID));
		}

		[TestDate(2020, 10, 1)]
		public void TestAllocateComplianceNumberAndRequeueForVietnamEInvoice()
		{
			// REFACTOR ME: The behaviour of queuing an EInvoice Pivot when allocated a compliance number is controlled by IComplianceInfoElectronicInvoicingEligibleSubType
			//              These tests should be applied to all countries.

			TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
			var orgProxy = TestObjectCreator.CreateOrgHeader("AAA", true, true);
			var vietnamCompany = TestObjectCreator.CreateNewCompany("VN1", "VN", orgProxy: orgProxy);
			var branch = TestObjectCreator.CreateBranch("BVN", vietnamCompany, orgProxy);
			var department = TestObjectCreator.CreateDepartment("DVN");
			TestObjectCreator.SetCustomsCodeForOrgHeader(vietnamCompany.OrgProxy, OrgCusCode.CodeTypes.VATCode, Core.Constants.CountryCodes.VietNam, "1234567890");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), department.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var allocateComplianceNumberMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Number");
				AssertNotNull("Allocate Compliance Number Menu Item should exist", allocateComplianceNumberMenuItem);

				var invoice1 = CreateInvoice();
				var arInvLine1 = (ARInvoiceLine)invoice1.Lines.AddNew();
				arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
				arInvLine1.AL_OSExTaxAmount = 300m;
				arInvLine1.AL_AC = TestObjectCreator.FRT.PK;
				arInvLine1.AL_AT = TestObjectCreator.GST1.PK;
				invoice1.AH_ComplianceSubType = "TXI";
				Factory.Save();

				var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				complianceSequence.XD_GC_Company = vietnamCompany.PK;
				complianceSequence.XD_SequenceClass = "TXI";
				complianceSequence.XD_Prefix = "AA";
				complianceSequence.XD_MaximumNumberDigits = 7;
				complianceSequence.XD_StartNumber = 1000;
				complianceSequence.XD_EndNumber = 1010;
				complianceSequence.XD_NextNumber = 1001;
				complianceSequence.XD_StartDate = new ZDate(2020, 9, 1);
				complianceSequence.XD_ExpiryDate = new ZDate(2020, 10, 31);
				Factory.Save();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();

				testModule.DisplayGrid.SelectAllElements();
				allocateComplianceNumberMenuItem.PerformClick();

				AssertEquals(ZDate.Today, invoice1.AH_ComplianceDocumentDate);

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice1.PK).AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, Constants.EInvoicingPivotState.Queued));
				AssertNotNull("invoice1 should be queued", pivot);

				using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var creditNote = (invoice1 as IAmending).GenerateAmendingTransaction(TransactionTypes.CreditNote) as ARCreditNote;
					Factory.Save();

					testModule.PerformSearch_ForTest();
					collection = testModule.GridCollection as BusinessObjectCollection;
					collection.Load();

					testModule.DisplayGrid.SelectAllElements();
					allocateComplianceNumberMenuItem.PerformClick();

					AssertEquals("ComplianceDocumentDate should be today.", ZDate.Today, creditNote.AH_ComplianceDocumentDate);
					AssertNotNullOrEmpty("Compliance Number of Credit Notes should be allocated.", creditNote.AH_TransactionNum);

					pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, creditNote.PK).AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, Constants.EInvoicingPivotState.Queued));
					AssertNotNull("creditNote should be queued", pivot);
					AssertEquals("", EInvoicingPivotActionType.Adjustment, pivot.AIP_ActionType);
				}
			}
		}

		[TestDate(2020, 10, 1)]
		public void TestAllocateComplianceNumberAndRequeueForBrazilEInvoice()
		{
			// REFACTOR ME: The behaviour of queuing an EInvoice Pivot when allocated a compliance number is controlled by IComplianceInfoElectronicInvoicingEligibleSubType
			//              These tests should be applied to all countries.

			TestObjectCreator.CreateTestPeriods(ZDateTime.Now);
			var orgProxy = TestObjectCreator.CreateOrgHeader("AAA", true, true);
			var brazilCompany = TestObjectCreator.CreateNewCompany("BR1", CountryCodes.Brazil, orgProxy: orgProxy);
			var branch = TestObjectCreator.CreateBranch("BBR", brazilCompany, orgProxy);
			var department = TestObjectCreator.CreateDepartment("DBR");

			var complianceSubTypeRules = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(brazilCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			if (complianceSubTypeRules.Count == 0)
			{
				var rule = complianceSubTypeRules.AddNew();
				rule.Country = CountryCodes.Brazil;
				rule.SubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				rule.LedgerType = LedgerTypes.AccountsReceivable;
				rule.InvoiceType = TransactionTypes.Invoice;
				rule.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAnAmountOfTax;
				rule.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				rule.OriginalRule = OriginalRuleCodes.AllTransactions;
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Brazil))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, branch.PK.ToGuid(), department.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.TaxSystems.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateTaxSystemConfigurationForBrazil()))
			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceSubTypeRules))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.Today.AddDays(-10).ToDateTime()))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var allocateComplianceNumberMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Number");
				AssertNotNull("Allocate Compliance Number Menu Item should exist", allocateComplianceNumberMenuItem);

				var invoice1 = CreateInvoice();
				invoice1.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;

				var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
				taxTransaction.ATT_AH = invoice1.PK;
				taxTransaction.ATT_TaxSystemCode = "ISS";

				Factory.Save();

				var queuedPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice1.PK).AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, Constants.EInvoicingPivotState.Queued));
				AssertNotNull("invoice1 should be queued", queuedPivot);

				var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				complianceSequence.XD_GC_Company = brazilCompany.PK;
				complianceSequence.XD_SequenceClass = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				complianceSequence.XD_Prefix = "AA";
				complianceSequence.XD_MaximumNumberDigits = 7;
				complianceSequence.XD_StartNumber = 1000;
				complianceSequence.XD_EndNumber = 1010;
				complianceSequence.XD_NextNumber = 1001;
				complianceSequence.XD_StartDate = new ZDate(2020, 9, 1);
				complianceSequence.XD_ExpiryDate = new ZDate(2020, 10, 31);
				Factory.Save();

				testModule.PerformSearch_ForTest();
				var collection = testModule.GridCollection as BusinessObjectCollection;
				collection.Load();

				testModule.DisplayGrid.SelectAllElements();
				allocateComplianceNumberMenuItem.PerformClick();

				queuedPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice1.PK).AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, Constants.EInvoicingPivotState.Queued));
				AssertNotNull("invoice1 should be queued", queuedPivot);
			}
		}

		public void TestEInvoicingGUIActionHelper_SelectedBusinessObjects()
		{
			var menuItemText = "Authorize And Send";

			using (TestObjectCreator.SetUpForTestingEInvoicingChina(DateTime.Today.AddDays(-30)))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
			{
				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1, TestObjectCreator.DebtorTR);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1, 100);
				arInvoice1.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				Factory.Save();

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1, TestObjectCreator.DebtorTR);
				TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.AUD, 1, 100);
				arInvoice2.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
				Factory.Save();

				AssertPivotDetails(arInvoice1.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(arInvoice2.PK, EInvoicingPivotState.Pending, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);

				using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					module.PerformSearch_ForTest();
					var collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("Two transactions in the grid", 2, collection.Count);

					module.DisplayGrid.SelectAllElements();
					AssertEquals("Two transactions selected", 2, module.SelectedBusinessObjects_ForTestOnly.Length);

					var menuItems = module.GetNewActionMenuItems_ForTestOnly();
					var queuePendingInvoiceMenuName = menuItems.FindByText(menuItemText);
					queuePendingInvoiceMenuName.PerformClick();

					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertEquals(@"Transactions are now queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				AssertPivotDetails(arInvoice1.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
				AssertPivotDetails(arInvoice2.PK, EInvoicingPivotState.Queued, ZString.Empty, ZGuid.Empty, ZDateTime.Empty, ZDateTime.Empty, ZBool.False);
			}
		}

		public virtual void TestSecurityCheckpoint()
		{
			AssertEquals("Should have correct Security CheckPoint", Env.Security.ReceivablesTransactions, ARModule.SecurityCheckpoint);
		}

		public override void TestImportRemittanceFileSecurityCheckpoint()
		{
			AssertEquals("Should have correct Import Remittance File Security Checkpoint", Env.Security.ImportRemittanceFileReceivables, ARModule.ImportRemittanceFileSecurityCheckpoint_ForTestOnly);
		}

		public void TestMenuStructure()
		{
			MenuAssertion.AssertHasMenu(ARModule.GetNewStandardMenuItems_ForTestOnly(), "&New", ARModule.NewInvoiceMenuText_ForTestOnly);
			MenuAssertion.AssertHasMenu(ARModule.GetNewStandardMenuItems_ForTestOnly(), "&New", ARModule.NewCreditNoteMenuText_ForTestOnly);
			MenuAssertion.AssertHasMenu(ARModule.GetNewStandardMenuItems_ForTestOnly(), "&New", ARModule.NewAdjustmentNoteMenuText_ForTestOnly);
			MenuAssertion.AssertHasMenu(ARModule.GetNewStandardMenuItems_ForTestOnly(), "&New", ARModule.NewJournalMenuText_ForTestOnly);
			MenuAssertion.AssertHasMenu(ARModule.GetNewStandardMenuItems_ForTestOnly(), "&New", ARModule.NewTransferMenuText_ForTestOnly);
			MenuAssertion.AssertHasMenu(ARModule.GetNewStandardMenuItems_ForTestOnly(), "&New", ARModule.NewContraMenuText_ForTestOnly);
			MenuAssertion.AssertHasMenu(ARModule.GetNewStandardMenuItems_ForTestOnly(), "&New", ARModule.NewReceiptMenuText_ForTestOnly);
			MenuAssertion.AssertHasMenu(ARModule.GetNewAdditionalMenuItems_ForTestOnly(), "&Print", ARModule.MarkAsNotPrintedMenuText_ForTestOnly);
			MenuAssertion.AssertHasMenu(ARModule.GetNewAdditionalMenuItems_ForTestOnly(), "&Actions", ARModule.NewBulkReceiptsMenuText_ForTestOnly);
			MenuAssertion.AssertHasMenu(ARModule.GetNewAdditionalMenuItems_ForTestOnly(), "&Actions", ARModule.BadDebtWriteOffMenuText_ForTestOnly);
			MenuAssertion.AssertHasMenu(ARModule.GetNewAdditionalMenuItems_ForTestOnly(), "&Actions", ARModule.OverrideTransactionDescriptionMenuText_ForTestOnly);
		}

		public void TestGetActionMenuItemsSignInvoiceWithDigitalSignatureMenuVisibility()
		{
			Action<bool> assertMenuVisibility = (menuVisibility) =>
			{
				var menus = ARModule.GetActionMenuItems_ForTestOnly();
				var visibility = menus.FindByText(ARModule.SignInvoiceWithDigitalSignatureMenuText_ForTestOnly) != null;
				AssertEquals(visibility, menuVisibility);
			};

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				assertMenuVisibility(false);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				assertMenuVisibility(true);
			}
		}

		public override void TestSignElectronicInvoiceActionMenuItemVisibility()
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countries = Factory.Load<RefCountry>(countriesQuery);

			foreach (var country in countries)
			{
				if (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(country.Code) is IEInvoicingSignatureBuilderProvider)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.Code))
					{
						using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
						{
							var actionMenuItems = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
							var menuItem = actionMenuItems.FindByText("Sign Electronic Invoice");
							AssertNull("'Sign Electronic Invoice' menu item will not appear if EnableEInvoicingFunctionality registry is not enabled", menuItem);
						}

						using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
						{
							var actionMenuItems = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
							var menuItem = actionMenuItems.FindByText("Sign Electronic Invoice");
							AssertNotNull($"'Sign Electronic Invoice' menu item Should exist in {country.Code}; AR only", menuItem);
						}
					}
				}
				else
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.Code))
					{
						var actionMenuItems = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
						var menuItem = actionMenuItems.FindByText("Sign Electronic Invoice");
						AssertNull($"{country.Code} does not support e-Invoicing signatures", menuItem);
					}
				}
			}
		}

		#region Mark Issued Invoice as Reversed

		const string markIssuedInvoiceAsReversed = "Mark Issued Invoice as Reversed";

		public void TestMarkIssuedInvoiceAsReversedMenuVisibility()
		{
			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				var countriesQuery = new ZQuery { OrderBy = RefCountrySchema.Constants.RN_Code };
				var countries = Factory.Load<RefCountry>(countriesQuery);

				foreach (var country in countries)
				{
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.Code))
					{
						if (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(country.Code) is IMarkIssuedInvoiceAsReversed)
						{
							using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
							{
								var actionMenuItems = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
								var menuItem = actionMenuItems.FindByText(markIssuedInvoiceAsReversed);
								AssertNull("'Mark Issued Invoice as Reversed' menu item will not appear if EnableEInvoicingFunctionality registry is not enabled", menuItem);
							}

							using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
							{
								var actionMenuItems = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
								var menuItem = actionMenuItems.FindByText(markIssuedInvoiceAsReversed);
								AssertNotNull($"'Mark Issued Invoice as Reversed' menu item Should exist in {country.Code}; AR only", menuItem);
							}
						}
						else
						{
							using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.Code))
							{
								var actionMenuItems = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
								var menuItem = actionMenuItems.FindByText(markIssuedInvoiceAsReversed);
								AssertNull($"{country.Code} does not support mark issued invoice as reversed", menuItem);
							}
						}
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestMarkIssuedInvoiceAsReverse_NoEligibleTransaction()
		{
			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				{
					using (var form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1, TestObjectCreator.DebtorTR);
						TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1, 100);
						arInvoice1.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;
						Factory.Save();

						module.PerformSearch_ForTest();

						var collection = module.GridCollection as BusinessObjectCollection;
						collection.Load();
						AssertEquals("Collection should contain new invoices", 1, collection.Count);
						module.DisplayGrid.SelectAllElements();

						var actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
						var menuItem = actionMenuItems.FindByText(markIssuedInvoiceAsReversed);

						menuItem.PerformClick();
						AssertEquals("No eligible transaction(s) can be mark as reversed.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestMarkIssuedInvoiceAsReverse_Messages()
		{
			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				{
					using (var form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1, TestObjectCreator.DebtorTR);
						TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1, 100);
						arInvoice1.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;

						var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("002", TestObjectCreator.AUD, 1, TestObjectCreator.DebtorTR);
						TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.AUD, 1, 100);
						arInvoice2.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;

						var reference2 = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
						reference2.AH1_AH = arInvoice2.PK;
						reference2.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS;
						reference2.AH1_Reference = ComplianceDocumentStatusTypes.CDI.Code;

						var arInvoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("003", TestObjectCreator.AUD, 1, TestObjectCreator.DebtorTR);
						TestObjectCreator.CreateInvoiceLine(arInvoice3, TestObjectCreator.AUD, 1, 100);
						arInvoice3.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;

						var reference3 = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
						reference3.AH1_AH = arInvoice3.PK;
						reference3.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS;
						reference3.AH1_Reference = ComplianceDocumentStatusTypes.CDI.Code;

						var arInvoice4 = TestObjectCreator.CreateARInvoice<ARInvoice>("004", TestObjectCreator.AUD, 1, TestObjectCreator.DebtorTR);
						TestObjectCreator.CreateInvoiceLine(arInvoice4, TestObjectCreator.AUD, 1, 100);
						arInvoice4.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;

						var reference4 = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
						reference4.AH1_AH = arInvoice4.PK;
						reference4.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS;
						reference4.AH1_Reference = ComplianceDocumentStatusTypes.CDN.Code;

						Factory.Save();

						module.PerformSearch_ForTest();

						var collection = module.GridCollection as BusinessObjectCollection;
						collection.Load();
						module.DisplayGrid.SelectAllElements();

						var actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
						var menuItem = actionMenuItems.FindByText(markIssuedInvoiceAsReversed);

						menuItem.PerformClick();

						var transactionNums = string.Join(", ", new List<string>() { arInvoice2.AH_TransactionNum, arInvoice3.AH_TransactionNum }.OrderBy(x => x));
						var expectedText = $@"You are trying to reset the compliance document status to CDR, do you want to proceed?

Transaction number: {transactionNums}

Do you wish to continue?";
						Assert("Popup should be question", UnitTestUserNotification.Instance.PreviousMessages[1].WasQuestion);
						AssertEquals(expectedText, UnitTestUserNotification.Instance.PreviousMessages[1].Text);

						expectedText = $@"The following transaction(s) has been reset to CDR.

Transaction number: {transactionNums}";

						Assert("Popup should be information", UnitTestUserNotification.Instance.LastMessage.WasInformation);
						AssertEquals(expectedText, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestMarkIssuedInvoiceAsReverse_ReferenceUpdated()
		{
			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				{
					using (var form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1, TestObjectCreator.DebtorTR);
						arInvoice1.AH_GSTAmount = 5m;
						var line1 = TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.AUD, 1, 100);
						line1.AL_GSTVAT = 5m;
						arInvoice1.AH_ComplianceSubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXA;

						var reference1 = Factory.NewWithValidTestData<AccTransactionHeaderReference>();
						reference1.AH1_AH = arInvoice1.PK;
						reference1.AH1_Type = AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS;
						reference1.AH1_Reference = ComplianceDocumentStatusTypes.CDI.Code;

						Factory.Save();

						module.PerformSearch_ForTest();

						var collection = module.GridCollection as BusinessObjectCollection;
						collection.Load();
						module.DisplayGrid.SelectAllElements();

						var actionMenuItems = module.GetNewActionMenuItems_ForTestOnly();
						var menuItem = actionMenuItems.FindByText(markIssuedInvoiceAsReversed);

						menuItem.PerformClick();

						var referenceInNewFactory = new BusinessObjectFactory().Load<AccTransactionHeaderReference>(reference1.PK);
						AssertEquals(ComplianceDocumentStatusTypes.CDR.Code, referenceInNewFactory.AH1_Reference);
						AssertEquals(105m, referenceInNewFactory.AH1_Amount);

						AssertEquals(AutoEvents.EditedARecord.ToString(), arInvoice1.Logs.MostRecentLog.SL_SE_NKEvent);
						AssertEquals("Compliance document status was manually set to 'CDR', origin value was 'CDI'.", arInvoice1.Logs.MostRecentLog.SL_Reference);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		[TestDate(2016, 12, 1)]
		public void TestGetActionMenuItemsReinstateDailyInvoiceDateIncrementingMenuVisibility()
		{
			Action<DateTime> registryInstatedDateSetter = (registryValue) =>
			{
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryValue);
			};

			Action<string> registryDefaultingBehaviourSetter = (registryValue) =>
			{
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryValue);
			};

			Action<bool> securitySetter = (securityValue) =>
			{
				Env.Security.ReinstateDailyInvoiceDateIncrementing.IsAllowed = securityValue;
			};

			Action<bool> assertMenuVisibility = (menuVisibility) =>
			{
				var menus = ARModule.GetActionMenuItems_ForTestOnly();
				var visibility = menus.FindByText(ARModule.ReinstateDailyInvoiceDateIncrementingMenuText_ForTestOnly) != null;
				AssertEquals(visibility, menuVisibility);
			};

			var menuText = ARModule.ReinstateDailyInvoiceDateIncrementingMenuText_ForTestOnly;

			registryDefaultingBehaviourSetter(AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
			registryInstatedDateSetter(DateTime.MinValue);
			securitySetter(true);
			assertMenuVisibility(false);

			registryDefaultingBehaviourSetter(AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			registryInstatedDateSetter(DateTime.MinValue);
			securitySetter(false);
			assertMenuVisibility(false);

			registryDefaultingBehaviourSetter(AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
			registryInstatedDateSetter(DateTime.MinValue);
			securitySetter(false);
			assertMenuVisibility(false);

			registryDefaultingBehaviourSetter(AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			registryInstatedDateSetter(ZDateTime.Now.ToDateTime());
			securitySetter(true);
			assertMenuVisibility(false);

			registryDefaultingBehaviourSetter(AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			registryInstatedDateSetter(DateTime.MinValue);
			securitySetter(true);
			assertMenuVisibility(true);
		}

		[TestDate(2016, 12, 1)]
		public void TestHandleReinstateDailyInvoiceDateIncrementing()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			Action<ZDateTime> registrySetter = (registryValue) =>
			{
				AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, registryValue.ToDateTime());
			};

			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			Env.Security.ReinstateDailyInvoiceDateIncrementing.IsAllowed = true;
			registrySetter(ZDateTime.MinSmallDateTimeValue);

			var menus = ARModule.GetActionMenuItems_ForTestOnly();
			var menu = menus.FindByText(ARModule.ReinstateDailyInvoiceDateIncrementingMenuText_ForTestOnly);

			Action<ZDateTime> assertRegistryValue = (registryValueExpected) =>
			{
				var regValue = AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.Value;
				AssertEquals(regValue.Year, registryValueExpected.Year);
				AssertEquals(regValue.Month, registryValueExpected.Month);
			};

			var staffGroup = Factory.New<GlbGroup>();
			var curStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staffGroup.Staff.Add(curStaff);
			curStaff.GS_EmailAddress = "a@a.com";
			Factory.Save();
			AccountingConfigurationRegistry.Instance.InvoiceDateIncrementingSuspensionNotifyGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, staffGroup.PK.ToGuid());

			//same time zones
			foreach (var branch in GlbCompany.CurrentCompany.Branches)
			{
				branch.GB_RL_NKHomePort = "AUSYD";
			}

			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code).AddToFilter(StmALogSchema.SL_Reference, "Daily Invoice Date Incrementing Has Been Re-instated");
			var currentPeriodManagement = new AccountingPeriodCalculator(Factory).GetPeriodManagementFromDate(ZDateTime.Now, Env.CurrentCompany.PK);
			Assert("PreCondition: No EDT log for Daily Invoice Date Incrementing reinstated", !currentPeriodManagement.Logs.DatabaseHasLogs(logQuery));
			Env.OutgoingMailManager.EmailsCreated.Clear();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			registrySetter(ZDateTime.MinSmallDateTimeValue);
			menu.PerformClick();
			var log = currentPeriodManagement.Logs.GetAllLogs().Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EditedARecord.Code));
			Assert("EDT log for Daily Invoice Date Incrementing reinstated", currentPeriodManagement.Logs.DatabaseHasLogs(logQuery));
			var previousMessages = UnitTestUserNotification.Instance.PreviousMessages;
			AssertEquals(previousMessages[0].Text, "Daily Invoice Date Incrementing reinstated.");
			assertRegistryValue(ZDateTime.Now);
			AssertEquals(Env.OutgoingMailManager.EmailsCreated.Count, 1);
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].FromAddress, "a@a.com");
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].Subject, "The Invoice Date Incrementing Suspension has been lifted.");
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].Body, string.Format(@"<p>The Invoice Date Incrementing Suspension has been lifted by {0} for Company {1}.  Local operation time: {2}. UTC operation time: {3}.</p>

<p>The ""Current Invoice Date"" has been reverted to Today and will resume daily incrementing.</p>", curStaff.GS_FullName, Env.CurrentCompany.Code, ZDateTime.Now, ZDateTime.UtcNow));

			//different time zones
			Env.OutgoingMailManager.EmailsCreated.Clear();
			registrySetter(ZDateTime.MinSmallDateTimeValue);
			var newBranch = GlbCompany.CurrentCompany.Branches.AddNew();
			newBranch.GB_BranchName = "USA2D";
			newBranch.GB_RL_NKHomePort = "USA2D";

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();

			menu.PerformClick();
			previousMessages = UnitTestUserNotification.Instance.PreviousMessages;
			AssertEquals(previousMessages[1].Text, @"You are about to Reinstate Daily Invoice Date Incrementing. Reinstating Daily Invoice Dates cannot be reversed.

Note: USA2D is currently in a different time zone.
");
			AssertEquals(previousMessages[0].Text, "Daily Invoice Date Incrementing reinstated.");
			assertRegistryValue(ZDateTime.Now);
			AssertEquals(Env.OutgoingMailManager.EmailsCreated.Count, 1);
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].FromAddress, "a@a.com");
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].Subject, "The Invoice Date Incrementing Suspension has been lifted.");
			AssertEquals(Env.OutgoingMailManager.EmailsCreated[0].Body, string.Format(@"<p>The Invoice Date Incrementing Suspension has been lifted by {0} for Company {1}.  Local operation time: {2}. UTC operation time: {3}.</p>

<p>The ""Current Invoice Date"" has been reverted to Today and will resume daily incrementing.</p>", curStaff.GS_FullName, Env.CurrentCompany.Code, ZDateTime.Now, ZDateTime.UtcNow));

			//different time zones //user click no
			Env.OutgoingMailManager.EmailsCreated.Clear();
			registrySetter(ZDateTime.MinSmallDateTimeValue);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			menu.PerformClick();
			previousMessages = UnitTestUserNotification.Instance.PreviousMessages;
			AssertEquals(previousMessages[0].Text, @"You are about to Reinstate Daily Invoice Date Incrementing. Reinstating Daily Invoice Dates cannot be reversed.

Note: USA2D is currently in a different time zone.
");
			assertRegistryValue(ZDateTime.MinSmallDateTimeValue);
			AssertEquals(Env.OutgoingMailManager.EmailsCreated.Count, 0);
		}

		#region TestBatchedInvoiceDoesNotAllowReversing

		public virtual void TestBatchedInvoiceDoesNotAllowReversing()
		{
			ZForm reverseForm = null;
			using (ZForm parentForm = new ZForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				try
				{
					TestTransactionModule.SetFormsModalTo(parentForm);

					//SetupTransactionFilter();
					ARInv = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
					ARInv.AH_LocalExTaxAmount = 100m;
					ARInv.AH_FullyPaidDate = ZDateTime.Empty;

					InvoiceBatchHeader invoiceBatch = Factory.NewWithValidTestData(typeof(InvoiceBatchHeader)) as InvoiceBatchHeader;
					ARInv.AH_AH_InvoiceStatement = invoiceBatch.PK;
					Factory.Save();

					Assert("Precondition: no messages shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
					((ARTransactionModuleStrip)TestTransactionModule).ShowDeleteForm_ForTestOnly(ARInv);

					reverseForm = (ZForm)ZFormModaliser.ActiveForm;

					Assert("Message should be shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("The information should read as follows: ", "This transaction cannot be reversed because it is in Invoice Batch.",
						UnitTestUserNotification.Instance.LastMessage.Text);

					ARInv.AH_AH_InvoiceStatement = ZGuid.Empty;
					Factory.Save();
					((ARTransactionModuleStrip)TestTransactionModule).ShowDeleteForm_ForTestOnly(ARInv);

					reverseForm = (ZForm)ZFormModaliser.ActiveForm;
					AssertNotNull("ReversingForm should pop up", reverseForm);
					Assert("ReverseForm should be the CreditNoteForm", reverseForm is CreditNoteForm);
				}
				finally
				{
					TestTransactionModule.Dispose();
					if (reverseForm != null)
					{
						reverseForm.Dispose();
					}
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		#endregion

		#region TestHandleCreateNewCollectionBatch

		public void TestMenuItemOfNewCollectionBatch()
		{
			using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				MenuItem createBatchMenuItem = module.GetNewAdditionalMenuItems_ForTestOnly().FindByText("Actions");
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var mainMenuItem = createBatchMenuItem.MenuItems.FindByText(module.NewCollectionBatchMenuText_ForTestOnly);
					AssertNotNull(mainMenuItem);

					var subMenuItem1 = mainMenuItem.MenuItems.FindByText("Group By Debtor and Due Date");
					AssertNotNull(subMenuItem1);
					var subMenuItem2 = mainMenuItem.MenuItems.FindByText("Group By Debtor");
					AssertNotNull(subMenuItem2);
				}
			}
		}

		public void TestNewCollectionBatchMenuItem_GroupAllSelectedOption()
		{
			var groupAllSelectedMenuItemOption = GetNewCollectionBatchMenuItem_GroupAllSelectedOption(CountryCodes.Australia);
			AssertNull(groupAllSelectedMenuItemOption);

			groupAllSelectedMenuItemOption = GetNewCollectionBatchMenuItem_GroupAllSelectedOption(CountryCodes.Brazil);
			AssertNotNull(groupAllSelectedMenuItemOption);

			MenuItem GetNewCollectionBatchMenuItem_GroupAllSelectedOption(string countryCode)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				{
					using (var form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						return module.GetNewAdditionalMenuItems_ForTestOnly().FindByText("Actions").MenuItems.FindByText(module.NewCollectionBatchMenuText_ForTestOnly).MenuItems.FindByText("Group All Selected");
					}
				}
			}
		}

		public void TestHandleCreateNewCollectionBatch()
		{
			using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				MenuItem createBatchMenuItem = module.GetNewAdditionalMenuItems_ForTestOnly().FindByText("Actions");
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					var userNotif = UnitTestUserNotification.Instance;

					var collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("Collection should be empty", 0, collection.Count);

					var subMenuItem = createBatchMenuItem.MenuItems.FindByText(module.NewCollectionBatchMenuText_ForTestOnly).MenuItems.FindByText("Group By Debtor and Due Date");
					subMenuItem.PerformClick();
					Assert("Popup should be info", userNotif.LastMessage.WasInformation);
					AssertEquals("Please select Invoices first", userNotif.LastMessage.Text);

					var accountDetail = TestObjectCreator.AALSHI.CompanyData.ARAccountDetailsCollection.AddNew();
					accountDetail.A1_IsDefaultAccount = true;
					accountDetail.A1_PaymentMethod = "CRQ";
					accountDetail.A1_RX_NKAccountCurrency = "AUD";
					TestObjectCreator.AALSHI.CompanyData.OB_ARCreditAgreedPaymentMethod = "CRQ";

					var testInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					testInvoice1.AH_OH = TestObjectCreator.AALSHI.PK;
					var testInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					testInvoice2.AH_OH = TestObjectCreator.AALSHI.PK;

					AssertInvoicesCount(2);

					Assert("Popup should be none", userNotif.LastMessage.WasNone);
					AssertEquals(typeof(AccCollectionBatchForm), module.FormShownInTest_ForTestOnly.GetType());
					module.FormShownInTest_ForTestOnly.Dispose();

					var testInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					testInvoice3.AH_OH = TestObjectCreator.AALSHI.PK;

					var line = CreateCollectionBatchOrderLineForInvoice(testInvoice3, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testInvoice3.AH_OH);

					AssertInvoicesCount(3);
					Assert("Popup should be info", userNotif.LastMessage.WasInformation);
					AssertEquals("Some transactions are already included in active batches. Please revise transactions selection before creating the batch.", userNotif.LastMessage.Text);

					line.IsCancelled = true;
					Factory.Save();

					var testJournal = TestObjectCreator.CreateJournal<ARJournal>(10, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					testJournal.AH_AgreedPaymentMethodOverride = "CRQ";
					AssertInvoicesCount(4);
					Assert("Popup should be none", userNotif.LastMessage.WasNone);
					AssertEquals(typeof(AccCollectionBatchForm), module.FormShownInTest_ForTestOnly.GetType());
					module.FormShownInTest_ForTestOnly.Dispose();

					testJournal.AH_AgreedPaymentMethodOverride = "CCD";
					AssertInvoicesCount(4);
					Assert("Popup should be info", userNotif.LastMessage.WasInformation);
					AssertEquals("Collection batch only supports INV, CRD, ADJ, JNL with ‘CRQ’ agreed payment method. Please revise transactions selection before creating the batch.", userNotif.LastMessage.Text);
					module.FormShownInTest_ForTestOnly.Dispose();

					var paymentMethod = OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.Value;
					paymentMethod.Set("CRQ", false);
					using (OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, paymentMethod))
					{
						AssertInvoicesCount(4);
						Assert("Popup should be info", userNotif.LastMessage.WasInformation);
						AssertEquals(@"There are no valid agreed payment methods configured for Collection Batches, it is not possible to add transactions to the batch.

Please check the registry setting at Master Data -> Organizations -> Code Lists -> Receivables Credit Agreed Payment Methods.", userNotif.LastMessage.Text);
						module.FormShownInTest_ForTestOnly.Dispose();
					}

					paymentMethod.Set("CCD", true);
					paymentMethod.Set("TRF", true);
					paymentMethod.Set("CRQ", true);
					using (OrganisationsDataRegistry.Instance.ReceivablesCreditAgreedPaymentMethodsList.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, paymentMethod))
					{
						AssertInvoicesCount(4);
						Assert("Popup should be none", userNotif.LastMessage.WasNone);
						AssertEquals(typeof(AccCollectionBatchForm), module.FormShownInTest_ForTestOnly.GetType());
						module.FormShownInTest_ForTestOnly.Dispose();

						testInvoice1.AH_AgreedPaymentMethodOverride = "EPA";
						AssertInvoicesCount(4);
						Assert("Popup should be info", userNotif.LastMessage.WasInformation);
						AssertEquals("Collection batch only supports INV, CRD, ADJ, JNL with ‘CCD', 'CRQ', 'TRF’ agreed payment methods. Please revise transactions selection before creating the batch.", userNotif.LastMessage.Text);
						module.FormShownInTest_ForTestOnly.Dispose();

						testInvoice1.AH_AgreedPaymentMethodOverride = "TRF";
						testInvoice3.AH_AgreedPaymentMethodOverride = "TRF";
						var testJournal2 = TestObjectCreator.CreateJournal<ARJournal>(10, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
						testJournal2.AH_AgreedPaymentMethodOverride = "CRQ";
						AssertInvoicesCount(5);
						Assert("Popup should be none", userNotif.LastMessage.WasNone);
						AssertEquals(typeof(AccCollectionBatchForm), module.FormShownInTest_ForTestOnly.GetType());
						module.FormShownInTest_ForTestOnly.Dispose();

						var testReceipt = TestObjectCreator.CreateReceiptOrPayment(ReceiptTypes.Cheque, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100, TestObjectCreator.AUDBankAccount.PK);
						testReceipt.AH_OH = TestObjectCreator.ABIGAS.PK;
						AssertInvoicesCount(6);
						Assert("Popup should be info", userNotif.LastMessage.WasInformation);
						AssertEquals("Collection batch only supports type INV, CRD, ADJ, JNL. Please revise transactions selection before creating the batch.", userNotif.LastMessage.Text);
					}

					void AssertInvoicesCount(int number)
					{
						Factory.Save();
						userNotif.ClearMessages();
						module.PerformSearch_ForTest();
						collection.Load();
						AssertEquals("Collection should contain new invoices", number, collection.Count);
						module.DisplayGrid.SelectAllElements();
						subMenuItem.PerformClick();
					}
				}
			}
		}

		public void TestHandleCreateNewCollectionBatchDebtorsNoBank()
		{
			using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				MenuItem createBatchMenuItem = module.GetNewAdditionalMenuItems_ForTestOnly().FindByText("Actions");
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					BusinessObjectCollection collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("Collection should be empty", 0, collection.Count);

					createBatchMenuItem.MenuItems.FindByText(module.NewCollectionBatchMenuText_ForTestOnly).MenuItems.FindByText("Group By Debtor and Due Date").PerformClick();
					Assert("Popup should be info", UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertEquals("Please select Invoices first", UnitTestUserNotification.Instance.LastMessage.Text);
					var accountDetail = TestObjectCreator.AALSHI.CompanyData.ARAccountDetailsCollection.AddNew();
					accountDetail.A1_IsDefaultAccount = true;
					accountDetail.A1_PaymentMethod = "CRQ";
					accountDetail.A1_RX_NKAccountCurrency = "EUR";
					accountDetail.A1_AccountName = "111";
					accountDetail.A1_BankName = "Bank Test";
					accountDetail.A1_BankAccount = "Bank Test";
					accountDetail.A1_IBANNumber = "123456789";
					TestObjectCreator.AALSHI.CompanyData.OB_ARCreditAgreedPaymentMethod = "CRQ";

					var testInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					testInvoice1.AH_OH = TestObjectCreator.AALSHI.PK;
					testInvoice1.AH_AgreedPaymentMethodOverride = "CRQ";
					testInvoice1.AH_RX_NKTransactionCurrency = "EUR";
					Factory.Save();

					//NO Debtors without Bank Data
					UnitTestUserNotification.Instance.ClearMessages();
					module.PerformSearch_ForTest();
					collection.Load();
					AssertEquals("Collection should contain new invoices", 1, collection.Count);
					module.DisplayGrid.SelectAllElements();
					createBatchMenuItem.MenuItems.FindByText(module.NewCollectionBatchMenuText_ForTestOnly).MenuItems.FindByText("Group By Debtor and Due Date").PerformClick();
					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertEquals(typeof(AccCollectionBatchForm), module.FormShownInTest_ForTestOnly.GetType());
					module.FormShownInTest_ForTestOnly.Dispose();//

					//Some Debtors, < 5, without Bank Data. Display normal message
					testInvoice1.AH_RX_NKTransactionCurrency = "AUD";

					var accountDetail2 = TestObjectCreator.ABIGAS.CompanyData.ARAccountDetailsCollection.AddNew();
					accountDetail2.A1_IsDefaultAccount = false;
					accountDetail2.A1_PaymentMethod = "CRQ";
					accountDetail2.A1_RX_NKAccountCurrency = "EUR";
					accountDetail2.A1_AccountName = "222";
					accountDetail2.A1_BankName = "Bank Test 2";
					accountDetail2.A1_IBANNumber = "12345678922";
					TestObjectCreator.ABIGAS.CompanyData.OB_ARCreditAgreedPaymentMethod = "CRQ";

					var testInvoice1_2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					testInvoice1_2.AH_OH = TestObjectCreator.ABIGAS.PK;
					testInvoice1_2.AH_AgreedPaymentMethodOverride = "CRQ";
					var testInvoice2_2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					testInvoice2_2.AH_OH = TestObjectCreator.AALSHI.PK;
					testInvoice2_2.AH_AgreedPaymentMethodOverride = "CRQ";
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessages();
					module.PerformSearch_ForTest();
					collection.Load();
					AssertEquals("Collection should contain new invoices", 3, collection.Count);
					module.DisplayGrid.SelectAllElements();
					createBatchMenuItem.MenuItems.FindByText(module.NewCollectionBatchMenuText_ForTestOnly).MenuItems.FindByText("Group By Debtor and Due Date").PerformClick();
					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertEquals(@"The following debtors do not have a bank account setup with payment method 'CRQ-Collection Request' for the collection batch currency 'EUR'.

Please revise transactions selection or setup bank account against the debtor's organizations record before creating the collection batch.

Code        	Name
AALSHI	A.A.L. SHIPPING AGENCIES P/L
ABIGAS	ABI GAS & TOOLS", UnitTestUserNotification.Instance.LastMessage.Text);
					module.FormShownInTest_ForTestOnly.Dispose();

					//Many Debtors, > 5, without Bank Data. Display large message
					for (int i = 112; i < 500; i += 27)
					{
						var nameDebtorTest = "DEBTOR_" + i.ToString().Substring(2, 1) + i.ToString();
						nameDebtorTest = nameDebtorTest.Replace('0', 'X');
						OrgHeader orgHeaderTest = TestObjectCreator.CreateOrgHeader("CO" + i.ToString(), false, true);
						orgHeaderTest.OH_FullName = nameDebtorTest;
						orgHeaderTest.ExportersBankAccount = "Bank Test " + i.ToString();
						orgHeaderTest.CompanyData.OB_ARCreditAgreedPaymentMethod = "CRQ";

						var accountDetailTemp = orgHeaderTest.CompanyData.ARAccountDetailsCollection.AddNew();
						accountDetailTemp.A1_IsDefaultAccount = true;
						accountDetailTemp.A1_PaymentMethod = "CRQ";
						accountDetailTemp.A1_RX_NKAccountCurrency = "EUR";
						accountDetailTemp.A1_AccountName = "111";
						accountDetailTemp.A1_BankName = "Bank Test";
						accountDetailTemp.A1_BankAccount = "Bank Test";
						accountDetailTemp.A1_IBANNumber = "123456789";

						var testInvoiceTemp = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
						testInvoiceTemp.AH_OH = orgHeaderTest.PK;
						testInvoiceTemp.AH_AgreedPaymentMethodOverride = "CRQ";
						testInvoiceTemp.AH_RX_NKTransactionCurrency = "EUR";
						Factory.Save();
					}

					UnitTestUserNotification.Instance.ClearMessages();
					module.PerformSearch_ForTest();
					collection.Load();
					AssertEquals("Collection should contain new invoices", 18, collection.Count);
					module.DisplayGrid.SelectAllElements();
					createBatchMenuItem.MenuItems.FindByText(module.NewCollectionBatchMenuText_ForTestOnly).MenuItems.FindByText("Group By Debtor and Due Date").PerformClick();
					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasInformation);
					AssertEquals(@"The following debtors do not have a bank account setup with payment method 'CRQ-Collection Request' for the collection batch currency 'EUR'.

Please revise transactions selection or setup bank account against the debtor's organizations record before creating the collection batch.

Code        	Name
AALSHI	A.A.L. SHIPPING AGENCIES P/L
ABIGAS	ABI GAS & TOOLS
ZCO301	DEBTOR_13X1
ZCO112	DEBTOR_2112
ZCO382	DEBTOR_2382

Note:	Unable to display the full list as it contains more than 5 organizations.
	There are others 12 organizations outside the list.", UnitTestUserNotification.Instance.LastMessage.Text);
					module.FormShownInTest_ForTestOnly.Dispose();
				}
			}
		}

		public void TestHandleCreateBatchGroupAllSelected()
		{
			using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				var currentCompany = GlbCompany.CurrentCompany;
				using (currentCompany.TemporarilySetCountry(CountryCodes.Brazil))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					AssertEquals("Precondition:", CountryCodes.Brazil, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

					var userNotification = UnitTestUserNotification.Instance;

					var actionMenuItem = module.GetNewAdditionalMenuItems_ForTestOnly().FindByText("Actions");
					var createCollectionOrdersBatchMenuItem = actionMenuItem.MenuItems.FindByText(module.NewCollectionBatchMenuText_ForTestOnly);
					var subMenuItem_GroupAllSelected = createCollectionOrdersBatchMenuItem.MenuItems.FindByText("Group All Selected");
					AssertNotNull("Precondition:", subMenuItem_GroupAllSelected);

					var collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals(0, collection.Count);

					var companyOrgProxy = TestObjectCreator.CreateOrgHeader("DebtorCOP", false, true);
					currentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;
					currentCompany.OrgProxy.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "1#2345,67.89%", Core.Constants.CountryCodes.Brazil);

					var arCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "ARCRD01", TestObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
					var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV01", TestObjectCreator.AUD, 1.0m, 100m, 0m, 100m, 0m);
					arCreditNote.AH_OH = TestObjectCreator.AALSHI.PK;
					arInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
					Factory.Save();

					string errorText = "Collection Orders in Brazil only support INV Transactions with ‘CRQ’ agreed payment method and with an allocated Invoice Remittance Reference. Please review selected transactions before creating the order.";

					Assert("Precondition: IsUsedByActiveCollectionOrderLine", !arCreditNote.IsUsedByActiveCollectionOrderLine);
					AssertEquals("Precondition: AH_Ledger", LedgerTypes.AccountsReceivable, arCreditNote.AH_Ledger);
					AssertNotEquals("Precondition: AH_TransactionType", TransactionTypes.Invoice, arCreditNote.AH_TransactionType);
					AssertHasError_HandleCreateCollectionBatchGroupAllSelected("Popup Error when not INV transaction", TransactionTypes.CreditNote, true, errorText);

					Assert("Precondition: IsUsedByActiveCollectionOrderLine", !arInvoice.IsUsedByActiveCollectionOrderLine);
					AssertEquals("Precondition: AH_Ledger", LedgerTypes.AccountsReceivable, arInvoice.AH_Ledger);
					AssertEquals("Precondition: AH_TransactionType", TransactionTypes.Invoice, arInvoice.AH_TransactionType);
					AssertNotEquals("Precondition: AH_AgreedPaymentMethodOverride", OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest, arInvoice.AH_AgreedPaymentMethodOverride);
					AssertHasError_HandleCreateCollectionBatchGroupAllSelected("Popup Error when INV transaction Not have CRQ Payment Method", TransactionTypes.Invoice, true, errorText);

					arInvoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;
					Factory.Save();

					AssertEquals("Precondition: AH_AgreedPaymentMethodOverride", OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest, arInvoice.AH_AgreedPaymentMethodOverride);
					Assert("Precondition: InvoiceRemittanceReference.IsEmpty", arInvoice.InvoiceRemittanceReference.IsEmpty);
					AssertHasError_HandleCreateCollectionBatchGroupAllSelected("Popup Error when Invoice Remittance Reference is empty", TransactionTypes.Invoice, true, errorText);

					arInvoice.AH_AgreedPaymentMethodOverride = ZString.Empty;
					arInvoice.InvoiceRemittanceReference = "TESTINV1";
					Factory.Save();

					AssertNotEquals("Precondition: AH_AgreedPaymentMethodOverride", OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest, arInvoice.AH_AgreedPaymentMethodOverride);
					Assert("Precondition: InvoiceRemittanceReference", !arInvoice.InvoiceRemittanceReference.IsEmpty);
					AssertHasError_HandleCreateCollectionBatchGroupAllSelected("Popup Error when INV transaction Not have CRQ Payment Method", TransactionTypes.Invoice, true, errorText);

					arInvoice.AH_AgreedPaymentMethodOverride = OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest;
					Factory.Save();

					AssertEquals("Precondition: AH_AgreedPaymentMethodOverride", OrgConstants.CreditAgreedPaymentMethods.Code.CollectionRequest, arInvoice.AH_AgreedPaymentMethodOverride);
					AssertNoError_HandleCreateCollectionBatchGroupAllSelected("No error", TransactionTypes.Invoice, false, "");

					CreateCollectionBatchOrderLineForInvoice(arInvoice, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, null, DebtorValidation.DebtorShouldBeEmpty);

					errorText = "Some transactions are already included in active batches. Please revise transactions selection before creating the batch.";
					Assert("Precondition: IsUsedByActiveCollectionOrderLine", arInvoice.IsUsedByActiveCollectionOrderLine);
					AssertHasError_HandleCreateCollectionBatchGroupAllSelected("Popup Error when INV transaction Not allocated with Invoice Remittance Reference", TransactionTypes.Invoice, true, errorText);

					void AssertHasError_HandleCreateCollectionBatchGroupAllSelected(string message, ZString transactionType, bool hasError, string expectedErrorMessage)
					{
						SelectElementsFromDisplayGridAndPerformClick(transactionType);

						Assert(message, userNotification.LastMessage.WasInformation);
						AssertEquals(expectedErrorMessage, userNotification.LastMessage.Text);
					}

					void AssertNoError_HandleCreateCollectionBatchGroupAllSelected(string message, ZString transactionType, bool hasError, string expectedErrorMessage)
					{
						SelectElementsFromDisplayGridAndPerformClick(transactionType);

						Assert(message, userNotification.LastMessage.WasNone);
						AssertType<AccCollectionBatchForm>(module.FormShownInTest_ForTestOnly);

						var collectionBatch = (AccCollectionBatch)module.FormShownInTest_ForTestOnly.BusinessEntityForPersistingForm;
						AssertEquals(CollectionFileFormatList.Codes.itauBank, collectionBatch.ACB_CollectionFileFormat);
						AssertEquals(1, collectionBatch.CollectionOrders.Count);
						AssertEquals(ZGuid.Empty, collectionBatch.CollectionOrders[0].ACO_OH_Debtor);
						AssertEquals(DebtorValidation.DebtorShouldBeEmpty, collectionBatch.CollectionOrders[0].DebtorValidationType);

						module.FormShownInTest_ForTestOnly.Dispose();
					}

					void SelectElementsFromDisplayGridAndPerformClick(ZString transactionType)
					{
						userNotification.ClearMessages();
						module.PerformSearch_ForTest();
						collection.Load();
						AssertEquals("Collection should contain new invoices", 2, collection.Count);
						module.DisplayGrid.SelectAllElements(x => x is InvoicingBase invoicingBase && invoicingBase.AH_TransactionType == transactionType);
						subMenuItem_GroupAllSelected.PerformClick();
					}
				}
			}
		}

		#endregion

		[TestDate(2012, 05, 15, 10, 44, 03)]
		public void TestOverrideInvoiceRemittanceType()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));

			using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				MenuItem overrideReferenceMenu = module.GetOverrideDetailsMenuItems_ForTestOnly().FindByText(module.OverrideInvoicePaymentReferenceCodeText_ForTestOnly);
				using (ZForm form1 = new ZForm())
				{
					form1.Controls.Add(module.EmbeddedControl);
					form1.Show();

					BusinessObjectCollection collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("Collection should be empty", 0, collection.Count);

					overrideReferenceMenu.PerformClick();
					AssertEquals("You have not selected any Transactions.", UnitTestUserNotification.Instance.LastMessage.Text);

					var org = TestObjectCreator.AALSHI;
					org.OH_IsDebtor = true;
					org.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					invoice.AH_OH = org.PK;
					var creditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "INV002", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					creditNote.AH_OH = org.PK;
					var arPayment = TestObjectCreator.CreateARPayment(1, 10, ZDateTime.Today, ZDateTime.Today, org.PK, TestObjectCreator.AUDBankAccount.PK);
					Factory.Save();

					var configurationCollection = InvoiceRemittanceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
					AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationCollection);
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessages();
					module.PerformSearch_ForTest();
					collection.Load();
					AssertEquals("Collection should contain 3 transactions", 3, collection.Count);
					module.DisplayGrid.SelectAllElements();
					overrideReferenceMenu.PerformClick();
					AssertEquals("The 'Override Invoice Remittance Type' Action menu can only be run for INV, CRD and ADJ transaction types only.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					module.PerformSearch_ForTest();
					collection.Load();
					AssertEquals("Collection should contain 3 transactions", 3, collection.Count);
					module.DisplayGrid.SelectAllElements(x => x is InvoicingBase invoicingBase && (invoicingBase.AH_TransactionType == TransactionTypes.Invoice || invoicingBase.AH_TransactionType == TransactionTypes.CreditNote));
					AssertEquals(2, module.DisplayGrid.SelectedElements.Length);

					overrideReferenceMenu.PerformClick();

					AssertType("LastFormShownDialogForTest", typeof(OverrideInvoiceRemittanceTypeForm), ZFormModaliser.LastFormShownForTest);
					var form = (OverrideInvoiceRemittanceTypeForm)ZFormModaliser.LastFormShownForTest;
					var bizo = (OverrideInvoiceRemittanceTypeHelper)form.BusinessEntity;

					AssertEquals("2 business object in grid for OverrideInvoiceRemittanceType", 2, bizo.WrappedObjects.Count);

					bizo.WrappedObjects.Cast<InvoicingBase>().First(x => x.AH_TransactionType == TransactionTypes.Invoice).AH_InvoicePaymentReferenceCode = "BBB";
					bizo.WrappedObjects.Cast<InvoicingBase>().First(x => x.AH_TransactionType == TransactionTypes.CreditNote).AH_InvoicePaymentReferenceCode = "BBB";

					ContinueWithSave saveResult = form.FireSaveButton();

					AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);

					var newFactory = new BusinessObjectFactory();
					var invoiceInNewFactory = newFactory.Load<InvoicingBase>(invoice.PK);
					var creditNoteInNewFactory = newFactory.Load<InvoicingBase>(creditNote.PK);

					AssertEquals("Invoice Remittance Type should be updated", "BBB", invoiceInNewFactory.AH_InvoicePaymentReferenceCode);
					AssertEquals("Invoice Remittance Type should be updated", "BBB", creditNoteInNewFactory.AH_InvoicePaymentReferenceCode);

					AssertEquals("Invoice Remittance Reference should be updated", "XX123456", invoiceInNewFactory.InvoiceRemittanceReference);
					AssertEquals("Invoice Remittance Reference should be updated", "XX123456", creditNoteInNewFactory.InvoiceRemittanceReference);
				}
			}
		}

		[TestDate(2020, 12, 01)]
		public void TestHandleBulkAllocateComplianceNumberForVietnamCompany()
		{
			// REFACTOR ME: The behaviour of queuing an EInvoice Pivot when allocated a compliance number is controlled by IComplianceInfoElectronicInvoicingEligibleSubType
			//              These tests should be applied to all countries.

			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequenceBook.XD_Code = "ABC";
				sequenceBook.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequenceBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
				sequenceBook.XD_Prefix = "AA";
				sequenceBook.XD_StartNumber = 1;
				sequenceBook.XD_NextNumber = 2;
				sequenceBook.XD_EndNumber = 100;
				sequenceBook.XD_MaximumNumberDigits = 8;
				sequenceBook.XD_StartDate = new ZDate(2020, 11, 01);
				sequenceBook.XD_ExpiryDate = new ZDate(2020, 12, 20);
				Factory.Save();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					MenuItem allocateComplianceNumberMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Number");
					AssertNotNull(allocateComplianceNumberMenuItem);

					var arCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD001", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK);
					var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "2", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK);
					var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "3", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK);
					arInvoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					arInvoice2.AH_TransactionReference = "AA00000001";

					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(3, module.DisplayGrid.SelectedElements.Length);
					allocateComplianceNumberMenuItem.PerformClick();
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToAllocate, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					arInvoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					arInvoice2.AH_TransactionReference = string.Empty;

					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(3, module.DisplayGrid.SelectedElements.Length);
					allocateComplianceNumberMenuItem.PerformClick();
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToAllocate, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.ABIGAS, TestObjectCreator.FRT.PK);
					arInvoice3.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					var arInvoice4 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.ABIGAS, TestObjectCreator.FRT.PK);
					arInvoice4.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;

					Factory.Save();

					var orgFilter = (ModuleGuidFilter)((ARTransactionFilterStripBusinessObject)module.FilterBusinessObject)["Debtor"];
					orgFilter.IsActive = true;
					orgFilter.Property = TestObjectCreator.ABIGAS.PK;

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(2, module.DisplayGrid.SelectedElements.Length);
					allocateComplianceNumberMenuItem.PerformClick();
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2020, 12, 01)]
		public void TestHandleBulkAllocateComplianceNumberForVietnamCompany_InvoiceIsCancelled()
		{
			// REFACTOR ME: The behaviour of queuing an EInvoice Pivot when allocated a compliance number is controlled by IComplianceInfoElectronicInvoicingEligibleSubType
			//              These tests should be applied to all countries.

			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequenceBook.XD_Code = "ABC";
				sequenceBook.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequenceBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
				sequenceBook.XD_Prefix = "AA";
				sequenceBook.XD_StartNumber = 1;
				sequenceBook.XD_NextNumber = 2;
				sequenceBook.XD_EndNumber = 100;
				sequenceBook.XD_MaximumNumberDigits = 8;
				sequenceBook.XD_StartDate = new ZDate(2020, 11, 01);
				sequenceBook.XD_ExpiryDate = new ZDate(2020, 12, 20);
				Factory.Save();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var allocateComplianceNumberMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Number");
					AssertNotNull(allocateComplianceNumberMenuItem);

					UnitTestUserNotification.Instance.ClearMessages();
					var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "1", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.ABIGAS, TestObjectCreator.FRT.PK);
					arInvoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "2", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.ABIGAS, TestObjectCreator.FRT.PK);
					arInvoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;

					var reversingFactory = new ReversingFactory();
					var reversing = reversingFactory.NewReversing(arInvoice1);
					reversing.Reverse();
					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(3, module.DisplayGrid.SelectedElements.Length);
					allocateComplianceNumberMenuItem.PerformClick();
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToAllocate, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					arInvoice2.AH_TransactionReference = string.Empty;
					var reversing2 = reversingFactory.NewReversing(arInvoice2);
					reversing2.Reverse();
					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(4, module.DisplayGrid.SelectedElements.Length);
					allocateComplianceNumberMenuItem.PerformClick();
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToAllocate, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2020, 12, 01)]
		public void TestHandleBulkAllocateComplianceNumberForVietnamCompany_AllLinesWithCMTCharge()
		{
			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequenceBook.XD_Code = "ABC";
				sequenceBook.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequenceBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
				sequenceBook.XD_Prefix = "AA";
				sequenceBook.XD_StartNumber = 1;
				sequenceBook.XD_NextNumber = 2;
				sequenceBook.XD_EndNumber = 100;
				sequenceBook.XD_MaximumNumberDigits = 8;
				sequenceBook.XD_StartDate = new ZDate(2020, 11, 01);
				sequenceBook.XD_ExpiryDate = new ZDate(2020, 12, 20);
				Factory.Save();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var allocateComplianceNumberMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Number");
					AssertNotNull(allocateComplianceNumberMenuItem);

					UnitTestUserNotification.Instance.ClearMessages();

					var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1M, TestObjectCreator.AALSHI);
					arInvoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					var arInvLine1 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
					arInvLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine1.AL_OSExTaxAmount = 100m;
					arInvLine1.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					arInvLine1.AL_AT = ZGuid.Empty;
					var arInvLine2 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
					arInvLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine2.AL_OSExTaxAmount = 200m;
					arInvLine2.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					arInvLine2.AL_AT = TestObjectCreator.GST1.PK;

					var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV002", TestObjectCreator.VND, 1M, TestObjectCreator.AALSHI);
					arInvoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					var arInvLine3 = (ARInvoiceLine)arInvoice1.Lines.AddNew();
					arInvLine3.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine3.AL_OSExTaxAmount = 300m;
					arInvLine3.AL_AC = TestObjectCreator.FRT.PK;
					arInvLine3.AL_AT = TestObjectCreator.GST1.PK;

					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(2, module.DisplayGrid.SelectedElements.Length);
					allocateComplianceNumberMenuItem.PerformClick();
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.SomeTransactionsNotSatisfyToAllocate, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					var arInvoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("INV003", TestObjectCreator.VND, 1M, TestObjectCreator.ABIGAS);
					arInvoice3.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					var arInvLine4 = (ARInvoiceLine)arInvoice3.Lines.AddNew();
					arInvLine4.AL_AG = TestObjectCreator.GLHeader1.PK;
					arInvLine4.AL_OSExTaxAmount = 300m;
					arInvLine4.AL_AC = TestObjectCreator.CommentChargeCode.PK;
					arInvLine4.AL_AT = TestObjectCreator.GST1.PK;

					Factory.Save();

					var orgFilter = (ModuleGuidFilter)((ARTransactionFilterStripBusinessObject)module.FilterBusinessObject)["Debtor"];
					orgFilter.IsActive = true;
					orgFilter.Property = TestObjectCreator.ABIGAS.PK;

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(1, module.DisplayGrid.SelectedElements.Length);
					allocateComplianceNumberMenuItem.PerformClick();
					AssertEquals(AccountingConstants.VietnamEInvoicingMessage.AllTransactionsNotSatisfyToAllocate, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2020, 12, 01)]
		public void TestHandleBulkAllocateComplianceNumberForVietnamCompany_ErrorMessage()
		{
			// REFACTOR ME: The behaviour of queuing an EInvoice Pivot when allocated a compliance number is controlled by IComplianceInfoElectronicInvoicingEligibleSubType
			//              These tests should be applied to all countries.

			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				var sequenceBook = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequenceBook.XD_Code = "ABC";
				sequenceBook.XD_SequenceClass = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequenceBook.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
				sequenceBook.XD_Prefix = "AA";
				sequenceBook.XD_StartNumber = 1;
				sequenceBook.XD_NextNumber = 2;
				sequenceBook.XD_EndNumber = 100;
				sequenceBook.XD_MaximumNumberDigits = 8;
				sequenceBook.XD_StartDate = new ZDate(2020, 11, 01);
				sequenceBook.XD_ExpiryDate = new ZDate(2020, 12, 20);
				Factory.Save();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					MenuItem allocateComplianceNumberMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Number");
					AssertNotNull(allocateComplianceNumberMenuItem);

					var arCreditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "1", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK);
					var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "2", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK);
					var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "3", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK);
					var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "4", TestObjectCreator.VND, 1.0m, 100m, 100m, 100m, 100m, TestObjectCreator.AALSHI, TestObjectCreator.FRT.PK);
					arInvoice2.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					arInvoice2.AH_TransactionReference = "AA00000001";

					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(4, module.DisplayGrid.SelectedElements.Length);
					allocateComplianceNumberMenuItem.PerformClick();
					AssertEquals("AllTransactionsNotSatisfyToAllocate_DisableEInvoicingAdjustment", @"Compliance Number cannot be allocated to the selected transactions due to one of the following reasons:
1. No matching compliance invoice book found.
2. Compliance number has already been allocated.
3. Selected INV transactions do not have a compliance sub type assigned.
4. Allocation of compliance number to CRD and ADJ transactions is not supported.
5. INV transactions have been reversed.
6. Allocation of compliance number to transactions that contains comment charge lines (Charge Type: CMT) only is not supported.

No transaction will be updated.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						UnitTestUserNotification.Instance.ClearMessages();
						allocateComplianceNumberMenuItem.PerformClick();
						AssertEquals("AllTransactionsNotSatisfyToAllocate_EnableEInvoicingAdjustment", @"Compliance Number cannot be allocated to the selected transactions due to one of the following reasons:
1. No matching compliance invoice book found.
2. Compliance number has already been allocated.
3. Selected INV transactions do not have a compliance sub type assigned.
4. Allocation of compliance number is only allowed for INV and CRD posted via 'Amend with Credit Note' function.
5. INV transactions have been reversed.
6. Allocation of compliance number to transactions that contains comment charge lines (Charge Type: CMT) only is not supported.

No transaction will be updated.", UnitTestUserNotification.Instance.LastMessage.Text);
					}

					UnitTestUserNotification.Instance.ClearMessages();
					arInvoice1.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
					arInvoice2.AH_TransactionReference = string.Empty;

					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(4, module.DisplayGrid.SelectedElements.Length);
					allocateComplianceNumberMenuItem.PerformClick();
					AssertEquals("SomeTransactionsNotSatisfyToAllocate_DisableEInvoicingAdjustment", @"Compliance Number cannot be allocated to some of the selected transactions due to one of the following reasons:
1. No matching compliance invoice book found.
2. Compliance number has already been allocated.
3. Selected INV transactions do not have a compliance sub type assigned.
4. Allocation of compliance number to CRD and ADJ transactions is not supported.
5. INV transactions have been reversed.
6. Allocation of compliance number to transactions that contains comment charge lines (Charge Type: CMT) only is not supported.

Only INV transactions with a compliance sub type will be updated.", UnitTestUserNotification.Instance.LastMessage.Text);

					using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						arInvoice3.AH_ComplianceSubType = VietnamComplianceInfo.ComplianceSubTypeCodes.TXI;
						Factory.Save();

						UnitTestUserNotification.Instance.ClearMessages();
						allocateComplianceNumberMenuItem.PerformClick();
						AssertEquals("SomeTransactionsNotSatisfyToAllocate_EnableEInvoicingAdjustment", @"Compliance Number cannot be allocated to some of the selected transactions due to one of the following reasons:
1. No matching compliance invoice book found.
2. Compliance number has already been allocated.
3. Selected INV transactions do not have a compliance sub type assigned.
4. Allocation of compliance number is only allowed for INV and CRD posted via 'Amend with Credit Note' function.
5. INV transactions have been reversed.
6. Allocation of compliance number to transactions that contains comment charge lines (Charge Type: CMT) only is not supported.

Only INV transactions with a compliance sub type will be updated.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			else
			{
				Assert(true);
			}
		}

		[TestDate(2020, 12, 01)]
		public void TestHandleBulkAllocateComplianceNumberForVietnamCompany_NotInvoicingBase()
		{
			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var allocateComplianceNumberMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Allocate Compliance Number");
					AssertNotNull(allocateComplianceNumberMenuItem);

					var arJournal = TestObjectCreator.CreateJournal<ARJournal>(10m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					Factory.Save();

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(1, module.DisplayGrid.SelectedElements.Length);
					AssertNoExceptionThrown(() => allocateComplianceNumberMenuItem.PerformClick());
					AssertEquals("AllTransactionsNotSatisfyToAllocate_DisableEInvoicingAdjustment", @"Compliance Number cannot be allocated to the selected transactions due to one of the following reasons:
1. No matching compliance invoice book found.
2. Compliance number has already been allocated.
3. Selected INV transactions do not have a compliance sub type assigned.
4. Allocation of compliance number to CRD and ADJ transactions is not supported.
5. INV transactions have been reversed.
6. Allocation of compliance number to transactions that contains comment charge lines (Charge Type: CMT) only is not supported.

No transaction will be updated.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public new void TestComplianceSubTypeAndNumberAllocationMenuItem()
		{
			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				var countryComplianceFactoryMock = GetICountryComplianceFactory(true, string.Empty);

				using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.VietNam))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var message = AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber;

					MenuItem updateComplianceInfoMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Update Compliance Sub Type and/or Number");
					AssertNotNull(updateComplianceInfoMenuItem);
					var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("INV001", TestObjectCreator.VND, 1m, TestObjectCreator.AALSHI);
					Factory.Save();
					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(1, module.DisplayGrid.SelectedElements.Length);
					updateComplianceInfoMenuItem.PerformClick();
					AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					var arCreditNote = TestObjectCreator.CreateARCreditNote("CRD001", TestObjectCreator.ABIGAS, TestObjectCreator.VND);
					Factory.Save();
					ModuleGuidFilter orgFilter = (ModuleGuidFilter)((ARTransactionFilterStripBusinessObject)module.FilterBusinessObject)["Debtor"];
					orgFilter.IsActive = true;
					orgFilter.Property = TestObjectCreator.ABIGAS.PK;
					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(1, module.DisplayGrid.SelectedElements.Length);
					updateComplianceInfoMenuItem.PerformClick();
					AssertEquals(AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					var arAdjustment = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("ADJ001", 100m, 5m, ZDateTime.Today, TestObjectCreator.Debtor.PK);
					Factory.Save();
					orgFilter.Property = TestObjectCreator.Debtor.PK;
					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(1, module.DisplayGrid.SelectedElements.Length);
					updateComplianceInfoMenuItem.PerformClick();
					AssertEquals(AccountingConstants.DisallowToUpdateComplianceSubTypeAndNumber, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				countryComplianceFactoryMock = GetICountryComplianceFactory(false, "error message");

				using (ObjectFactory.Substitute(countryComplianceFactoryMock.Object))
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form = new ZForm())
				{
					var complianceSubTypeRules = AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
					if (complianceSubTypeRules.Count == 0)
					{
						var rule = complianceSubTypeRules.AddNew();
						rule.Country = CountryCodes.China;
						rule.SubType = ChinaComplianceInfo.ComplianceSubTypeCodes.TXB;
						rule.LedgerType = LedgerTypes.AccountsReceivable;
						rule.InvoiceType = TransactionTypes.Invoice;
						rule.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax;
						rule.DisbursementRule = DisbursementRuleCodes.AllTransactions;
						rule.OriginalRule = OriginalRuleCodes.AllTransactions;
						rule.TaxRegistrationType = TaxRegistrationTypeCodes.Recoverable;
					}

					AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceSubTypeRules);

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					UnitTestUserNotification.Instance.ClearMessages();
					var updateComplianceInfoMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Update Compliance Sub Type and/or Number");
					AssertNotNull(updateComplianceInfoMenuItem);
					var orgFilter = (ModuleGuidFilter)((ARTransactionFilterStripBusinessObject)module.FilterBusinessObject)["Debtor"];
					orgFilter.IsActive = true;
					orgFilter.Property = TestObjectCreator.ABIGAS.PK;
					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();
					AssertEquals(1, module.DisplayGrid.SelectedElements.Length);
					updateComplianceInfoMenuItem.PerformClick();
					AssertEquals("error message", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			else
			{
				Assert(true);
			}
		}

		Mock<ICountryComplianceFactory> GetICountryComplianceFactory(bool isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowedValue, string errorMessageForARComplianceSubTypeAndNumberUpdate)
		{
			var countryComplianceFactoryMock = new Mock<ICountryComplianceFactory>();

			var complianceSubTypeAndNumberUpdateRulesMock = new Mock<IComplianceSubTypeAndNumberUpdateRules>();
			complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.IsARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowed).Returns(isARComplianceSubTypeAndNumberManualUpdateToAnyValueDisallowedValue);
			complianceSubTypeAndNumberUpdateRulesMock.Setup(x => x.GetErrorMessageForARComplianceSubTypeAndNumberUpdate(It.IsAny<AccTransactionHeader[]>())).Returns(errorMessageForARComplianceSubTypeAndNumberUpdate);

			countryComplianceFactoryMock.Setup(c => c.GetIComplianceSubTypeAndNumberUpdateRules(It.IsAny<ZString>())).Returns(complianceSubTypeAndNumberUpdateRulesMock.Object);

			return countryComplianceFactoryMock;
		}

		public void TestOverrideInvoiceRemittanceTypeChangeInvoicePaymentReferenceCode()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));
			var configurationCollection = InvoiceRemittanceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
			AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationCollection);
			Factory.Save();

			using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				MenuItem overrideReferenceMenu = module.GetOverrideDetailsMenuItems_ForTestOnly().FindByText(module.OverrideInvoicePaymentReferenceCodeText_ForTestOnly);
				using (ZForm form1 = new ZForm())
				{
					form1.Controls.Add(module.EmbeddedControl);
					form1.Show();

					BusinessObjectCollection collection = module.GridCollection as BusinessObjectCollection;
					collection.Load();
					AssertEquals("Collection should be empty", 0, collection.Count);

					overrideReferenceMenu.PerformClick();
					AssertEquals("You have not selected any Transactions.", UnitTestUserNotification.Instance.LastMessage.Text);

					var org = TestObjectCreator.AALSHI;
					org.OH_IsDebtor = true;
					org.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					var creditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "INV001", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
					creditNote.AH_OH = org.PK;
					Factory.Save();

					AssertEquals("Invoice Remittance Type should be saved", configurationCollection[1].Code, creditNote.AH_InvoicePaymentReferenceCode);
					AssertEquals("Invoice Remittance Reference should be saved", "XX123456", creditNote.InvoiceRemittanceReference);

					module.PerformSearch_ForTest();
					collection.Load();
					AssertEquals("Collection should contain 1 transactions", 1, collection.Count);
					module.DisplayGrid.SelectAllElements();
					overrideReferenceMenu.PerformClick();

					var form = (OverrideInvoiceRemittanceTypeForm)ZFormModaliser.LastFormShownForTest;
					var bizo = (OverrideInvoiceRemittanceTypeHelper)form.BusinessEntity;

					AssertEquals("1 business object in grid for OverrideInvoiceRemittanceType", 1, bizo.WrappedObjects.Count);

					bizo.WrappedObjects[0].AH_InvoicePaymentReferenceCode = configurationCollection[0].Code;
					ContinueWithSave saveResult = form.FireSaveButton();

					AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);

					var newFactory = new BusinessObjectFactory();
					var creditNoteInNewFactory = newFactory.Load<InvoicingBase>(creditNote.PK);

					AssertEquals("Invoice Remittance Type should be updated", configurationCollection[0].Code, creditNoteInNewFactory.AH_InvoicePaymentReferenceCode);
					AssertEquals("Invoice Remittance Reference should be updated", "NumberForAAA", creditNoteInNewFactory.InvoiceRemittanceReference);

					bizo.WrappedObjects[0].AH_InvoicePaymentReferenceCode = configurationCollection[1].Code;
					saveResult = form.FireSaveButton();

					AssertEquals("Precondition: form should be saved correctly", ContinueWithSave.Yes, saveResult);

					newFactory = new BusinessObjectFactory();
					creditNoteInNewFactory = newFactory.Load<InvoicingBase>(creditNote.PK);

					AssertEquals("Invoice Remittance Type should be updated", configurationCollection[1].Code, creditNoteInNewFactory.AH_InvoicePaymentReferenceCode);
					AssertEquals("Invoice Remittance Reference should be updated", "XX123456", creditNoteInNewFactory.InvoiceRemittanceReference);
				}
			}
		}

		public void TestMarkAsNotPrinted()
		{
			bool defaultSecurityValue = Env.Security.MarkInvoiceCreditAsNotPrinted.IsAllowed;

			try
			{
				using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				{
					MenuItem printMenuItem = module.GetNewAdditionalMenuItems_ForTestOnly().FindByText("Print");
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						BusinessObjectCollection collection = module.GridCollection as BusinessObjectCollection;
						collection.Load();
						AssertEquals("Collection should be empty", 0, collection.Count);

						Env.Security.MarkInvoiceCreditAsNotPrinted.IsAllowed = false;
						printMenuItem.MenuItems.FindByText(module.MarkAsNotPrintedMenuText_ForTestOnly).PerformClick();
						Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
						Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

						Env.Security.MarkInvoiceCreditAsNotPrinted.IsAllowed = true;
						printMenuItem.MenuItems.FindByText(module.MarkAsNotPrintedMenuText_ForTestOnly).PerformClick();

						Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("You have not selected any invoices", UnitTestUserNotification.Instance.LastMessage.Text);

						ARInvoice testInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
						testInvoice1.AH_InvoicePrinted = ZBool.False;
						ARInvoice testInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
						testInvoice2.AH_InvoicePrinted = ZBool.False;

						Factory.Save();

						module.PerformSearch_ForTest();
						collection.Load();
						AssertEquals("Collection should contain new invoices", 2, collection.Count);
						module.DisplayGrid.SelectAllElements();
						printMenuItem.MenuItems.FindByText(module.MarkAsNotPrintedMenuText_ForTestOnly).PerformClick();

						Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
						ZString errorMessage = "There are no invoices or credit notes to mark as not printed in your selection";
						AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

						testInvoice1.AH_InvoicePrinted = ZBool.True;
						Assert("TestInvoice1 should be marked as printed", testInvoice1.AH_InvoicePrinted);
						Factory.Save();
						printMenuItem.MenuItems.FindByText(module.MarkAsNotPrintedMenuText_ForTestOnly).PerformClick();
						Assert("Pop up should be information", UnitTestUserNotification.Instance.LastMessage.WasInformation);
						AssertEquals("Invoice/Credit Note marked as not printed", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert("TestInvoice1 should be marked as not printed", !testInvoice1.AH_InvoicePrinted);
						Assert("TestInvoice2 should be marked as not printed", !testInvoice1.AH_InvoicePrinted);

						testInvoice1.AH_InvoicePrinted = ZBool.True;
						testInvoice2.AH_InvoicePrinted = ZBool.True;
						Factory.Save();
						printMenuItem.MenuItems.FindByText(module.MarkAsNotPrintedMenuText_ForTestOnly).PerformClick();
						Assert("Pop up should be information", UnitTestUserNotification.Instance.LastMessage.WasInformation);
						AssertEquals("Invoice/Credit Note marked as not printed", UnitTestUserNotification.Instance.LastMessage.Text);
						Assert("TestInvoice1 should be marked as not printed", !testInvoice1.AH_InvoicePrinted);
						Assert("TestInvoice2 should be marked as not printed", !testInvoice1.AH_InvoicePrinted);
					}
				}
			}
			finally
			{
				Env.Security.MarkInvoiceCreditAsNotPrinted.IsAllowed = defaultSecurityValue;
			}
		}

		public void TestMarkAsNotPrintedWorksOnlyOnEligibleTransactions()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				MenuItem printMenuItem = module.GetNewAdditionalMenuItems_ForTestOnly().FindByText("Print");
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var collection = module.GridCollection as BusinessObjectCollection;
					Env.Security.MarkInvoiceCreditAsNotPrinted.IsAllowed = true;

					var invoice = Factory.NewWithValidTestData<ARInvoice>();
					invoice.AH_InvoicePrinted = ZBool.True;

					Factory.Save();

					module.PerformSearch_ForTest();
					collection.Load();
					AssertEquals("Collection should contain new invoice", 1, collection.Count);
					module.DisplayGrid.SelectAllElements();
					printMenuItem.MenuItems.FindByText(module.MarkAsNotPrintedMenuText_ForTestOnly).PerformClick();

					Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
					var expected = @"You cannot mark following transaction(s) as 'Not Printed', as they were printed at least once before. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV 00001000";
					AssertEquals(expected, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Invoice should be marked as not printed", invoice.AH_InvoicePrinted);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public override void TestPrintingToolBarButtons()
		{
			ToolBarButton[] buttons = TestTransactionModule.ToolBarButtons;
			ToolBarButton printButton = buttons.FindByText("Print");
			AssertNotNull("There should be a Button named Print", printButton);
			AssertNotNull("The Print button should have a dropdown menu", printButton.DropDownMenu);

			var menuItems = printButton.DropDownMenu.MenuItems;
			AssertNotNull(menuItems.FindByText("Print Transaction"));
			AssertNotNull(menuItems.FindByText("Print &Match Doc"));
			AssertNotNull(menuItems.FindByText("Mark as Not Printed"));
		}

		public void TestHandlePrintCannotPrintTransactionsARDiscount() =>
			AssertHandlePrintCannotPrintTransactions<ARDiscount>("Discount");

		public void TestHandlePrintCannotPrintTransactionsARContra() =>
			AssertHandlePrintCannotPrintTransactions<ARContraRow>("Contra");

		public void TestHandlePrintCannotPrintTransactionsARExchangeDifference() =>
			AssertHandlePrintCannotPrintTransactions<ARExchangeDifference>("Exchange Difference");

		public void TestHandlePrintCannotPrintTransactionsARJournal() =>
			AssertHandlePrintCannotPrintTransactions<ARJournal>("Journal");

		public void TestHandlePrintCannotPrintTransactionsAROverpayment() =>
			AssertHandlePrintCannotPrintTransactions<AROverpayment>("Overpayment");

		public void TestHandlePrintCannotPrintTransactionsARTransfer() =>
			AssertHandlePrintCannotPrintTransactions<ARTransferFromRow>("Transfer");

		void AssertHandlePrintCannotPrintTransactions<T>(string message) where T : AccTransactionHeader
		{
			using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (ZForm form = new ZForm())
			{
				var testHeader = Factory.NewWithValidTestData<T>();
				testHeader.AH_OH = TestObjectCreator.CreateOrgHeader("TSTREGORG", true, true).PK;
				Factory.Save();

				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();

				BusinessObjectCollection testCollection = (BusinessObjectCollection)module.GetNewGridCollection_ForTestOnly();
				testCollection.Load();
				AssertEquals(1, testCollection.Count);

				if (module.CurrentBusinessObjectInGrid_ForTestOnly != null)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					module.HandlePrint_ForTestOnly(this, new EventArgs());
					AssertEquals(GetExpectedPrintErrorMessage(testHeader.AH_TransactionNum, message, false), UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandlePrintPrintsOnlyEligibleTransactions()
		{
			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var creator = new TestObjectCreator(Factory);
				var invoice = creator.CreateInvoiceWithLine(typeof(ARInvoice), "10001", creator.AUD, 1.0m, 100.0m, 0.0m, 100.0m, 0.0m, creator.AALSHI, creator.CC10.PK);
				Factory.Save();

				using (var module1 = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form1 = new ZForm())
				{
					form1.Controls.Add(module1.EmbeddedControl);
					form1.Show();
					module1.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)module1.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals("Should load AR Invoice", 1, testCollection.Count);

					module1.HandlePrint_ForTestOnly(this, new EventArgs());
					AssertNull("No errors should be reported", UnitTestUserNotification.Instance.LastMessage.Text);
				}

				invoice.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
				Assert("Attach INV document to EDocs", InvoicePrintTask.AttachARInvoiceToEdocs_ForTestOnly(invoice, new NotificationBuffer()));
				Factory.Save();
				AssertEquals("AR Invoice should have invoice attached in EDocs", 1, invoice.DocManagerInfo.AllEDocs.Count);
				var eDoc = invoice.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);
				Factory.Save();

				using (var module2 = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form2 = new ZForm())
				{
					form2.Controls.Add(module2.EmbeddedControl);
					form2.Show();
					module2.PerformSearch_ForTest();

					BusinessObjectCollection testCollection = (BusinessObjectCollection)module2.GetNewGridCollection_ForTestOnly();
					testCollection.Load();
					AssertEquals("Should load AR Invoice", 1, testCollection.Count);

					module2.HandlePrint_ForTestOnly(this, new EventArgs());
					var expected = @"Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, hence re-printing of following transactions is not allowed through this module. You can go to the eDocs tab of a transaction and re-print the first invoice version stored there.
AR INV 00001000";
					AssertEquals("Expect a 'not eligible to print' error", expected, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestSignInvoiceWithDigitalSignature_Succeed()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				{
					Menu = module.GetNewActionMenuItems_ForTestOnly();
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
						TestObjectCreator.AALSHI.OH_RL_NKClosestPort = uNLOCO.RL_Code;

						AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
						ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
						AccComplianceSequence sequence = TestObjectCreator.SetupComplianceSequence(menuPK, PeruComplianceInfo.ComplianceSubTypeCodes.TXI, "abc", 1, 100, 1);
						Factory.Save();

						var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR003", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
						invoice1.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
						invoice1.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
						TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.GLHeader1.PK, 100);
						Factory.Save();
						var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR004", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
						invoice2.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
						invoice2.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
						TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.GLHeader1.PK, 100);
						Factory.Save();
						AssertNotNullOrEmpty(invoice1.AH_TransactionReference);
						AssertNotNullOrEmpty(invoice2.AH_TransactionReference);

						invoice1.AH_DigitalSignature_COMPRESSED = null;
						invoice2.AH_DigitalSignature_COMPRESSED = null;
						Factory.Save();

						module.PerformSearch_ForTest();
						var collection = module.GridCollection as BusinessObjectCollection;
						collection.Load();
						AssertEquals("TestCollection should contain new invoices", 2, collection.Count);
						module.DisplayGrid.SelectSingleElement(invoice2);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						Menu.FindByText(module.SignInvoiceWithDigitalSignatureMenuText_ForTestOnly).PerformClick();
						Assert("Popup should be Info", UnitTestUserNotification.Instance.LastMessage.WasInformation);
						AssertEquals("Signed 2 invoice(s) successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertNotNull(invoice1.AH_DigitalSignature_COMPRESSED);
						AssertNotNull(invoice2.AH_DigitalSignature_COMPRESSED);
					}
				}
			}
		}

		public void TestSignInvoiceWithDigitalSignature_FailedWithVariousErrors()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				{
					Menu = module.GetNewActionMenuItems_ForTestOnly();
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						var collection = module.GridCollection as BusinessObjectCollection;
						collection.Load();
						AssertNotNull(collection);
						collection.Load();
						AssertEquals("TestCollection should be empty", 0, collection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						Menu.FindByText(module.SignInvoiceWithDigitalSignatureMenuText_ForTestOnly).PerformClick();
						Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("Please choose one transaction to sign.", UnitTestUserNotification.Instance.LastMessage.Text);

						AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post);
						ZGuid menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Cost Confirmation Document")).PK;
						AccComplianceSequence sequence = TestObjectCreator.SetupComplianceSequence(menuPK, PeruComplianceInfo.ComplianceSubTypeCodes.TXI, "-", 1, 100, 1);
						Factory.Save();

						var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
						invoice1.AH_InvoiceDate = new ZDateTime(2018, 09, 11);
						invoice1.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 11, 05, 30, 59);
						Factory.Save();

						var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
						invoice2.AH_InvoiceDate = new ZDateTime(2018, 09, 11);
						invoice2.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 11, 05, 30, 59);
						Factory.Save();

						invoice1.AH_DigitalSignature_COMPRESSED = null;
						invoice2.AH_DigitalSignature_COMPRESSED = null;
						Factory.Save();

						module.PerformSearch_ForTest();
						collection.Load();
						AssertEquals("TestCollection should contain new invoices", 2, collection.Count);
						module.DisplayGrid.SelectAllElements();

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						Menu.FindByText(module.SignInvoiceWithDigitalSignatureMenuText_ForTestOnly).PerformClick();
						Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("Please choose one transaction to sign.", UnitTestUserNotification.Instance.LastMessage.Text);

						module.DisplayGrid.SelectSingleElement(invoice2);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						Menu.FindByText(module.SignInvoiceWithDigitalSignatureMenuText_ForTestOnly).PerformClick();
						Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("This transaction does not have a compliance number, please choose another one.", UnitTestUserNotification.Instance.LastMessage.Text);

						invoice1.AH_DigitalSignature_COMPRESSED = new ZBlob(new byte[] { 0, 1, 2, 3 });
						Factory.Save();
						module.DisplayGrid.SelectSingleElement(invoice1);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						Menu.FindByText(module.SignInvoiceWithDigitalSignatureMenuText_ForTestOnly).PerformClick();
						Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("This transaction has been signed already, please choose another one.", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		#region SignElectronicInvoice Tests

		public void TestSignElectronicInvoice_ShowsPopup_WhenNoTransactionsSelected()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Egypt))
			using (var module = new ARTransactionModuleStrip_ForSignElectronicInvoiceTests())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var collection = module.GridCollection as BusinessObjectCollection;
				collection.Load();
				AssertEquals("TestCollection should be empty", 0, collection.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var menuItems = module.GetNewActionMenuItems_ForTestOnly();
				menuItems.FindByText(module.SignElectronicInvoiceMenuText_ForTestOnly).PerformClick();
				Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Please choose one or more transactions to sign.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Actual signing logic should not be called", !module.SignElectronicInvoiceCore_WasCalled);
			}
		}

		public void TestSignElectronicInvoice_ShowsConfirmation_WhenTransactionsSelected()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Egypt))
			using (var module = new ARTransactionModuleStrip_ForSignElectronicInvoiceTests())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR003", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				invoice1.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
				invoice1.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
				TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.GLHeader1.PK, 100);
				var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR004", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				invoice2.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
				invoice2.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
				TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.GLHeader1.PK, 100);
				Factory.Save();

				var collection = module.GridCollection as BusinessObjectCollection;
				module.PerformSearch_ForTest();
				collection.Load();
				AssertEquals("TestCollection should contain new invoices", 2, collection.Count);
				module.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var menuItems = module.GetNewActionMenuItems_ForTestOnly();
				menuItems.FindByText(module.SignElectronicInvoiceMenuText_ForTestOnly).PerformClick();
				Assert("Popup should be question", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				var expectedText = @"About to sign 2 transaction(s) for Electronic Invoicing.
Please insert your hardware token now.

Do you wish to continue?";
				AssertEquals(expectedText, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Actual signing logic should not be called", !module.SignElectronicInvoiceCore_WasCalled);
			}
		}

		public void TestSignElectronicInvoice_ShowsFinalSummary_WhenConfirmedYes()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Egypt))
			using (var module = new ARTransactionModuleStrip_ForSignElectronicInvoiceTests())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR003", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				invoice1.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
				invoice1.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
				TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.GLHeader1.PK, 100);
				var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR004", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				invoice2.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
				invoice2.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
				TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.GLHeader1.PK, 100);
				Factory.Save();

				var collection = module.GridCollection as BusinessObjectCollection;
				module.PerformSearch_ForTest();
				collection.Load();
				AssertEquals("TestCollection should contain new invoices", 2, collection.Count);
				module.DisplayGrid.SelectAllElements();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var menuItems = module.GetNewActionMenuItems_ForTestOnly();
				menuItems.FindByText(module.SignElectronicInvoiceMenuText_ForTestOnly).PerformClick();
				Assert("Popup should be information", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				var expectedText = @"Processing complete.
2 transaction(s) were signed successfully.
1 transaction(s) had errors.";
				AssertEquals(expectedText, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Actual signing logic should be called", module.SignElectronicInvoiceCore_WasCalled);
			}
		}

		public void TestSignElectronicInvoice_Aborts_OnCancelOrError()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Egypt))
			using (var module = new ARTransactionModuleStrip_ForSignElectronicInvoiceTests())
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR003", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				invoice1.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
				invoice1.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
				TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.GLHeader1.PK, 100);
				var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR004", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				invoice2.AH_InvoiceDate = new ZDateTime(2018, 09, 12);
				invoice2.AH_SystemCreateTimeUtc = new ZDateTime(2018, 09, 12, 11, 36, 45);
				TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.GLHeader1.PK, 100);
				Factory.Save();

				var collection = module.GridCollection as BusinessObjectCollection;
				module.PerformSearch_ForTest();
				collection.Load();
				AssertEquals("TestCollection should contain new invoices", 2, collection.Count);
				module.DisplayGrid.SelectAllElements();
				var menuItems = module.GetNewActionMenuItems_ForTestOnly();

				// Failure signing via CryptographicException (wrong PIN)
				module.SignElectronicInvoiceCore_Exception = new System.Security.Cryptography.CryptographicException("Incorrect token PIN");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItems.FindByText(module.SignElectronicInvoiceMenuText_ForTestOnly).PerformClick();
				Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
				var expectedMessage = @"Error signing transactions using hardware token: CryptographicException - Incorrect token PIN

Please refer to Help > Diagnostics > Test Hardware Token Signature.";
				AssertEquals("Last pop-up should be exception error during signing", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Actual signing logic should be called", module.SignElectronicInvoiceCore_WasCalled);
				module.Reset_ForTest();

				// Failure signing via IOException (wrong chipset DLLs)
				module.SignElectronicInvoiceCore_Exception = new System.IO.IOException("Error with token DLLs");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menuItems.FindByText(module.SignElectronicInvoiceMenuText_ForTestOnly).PerformClick();
				Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
				expectedMessage = @"Error signing transactions using hardware token: IOException - Error with token DLLs

Please refer to Help > Diagnostics > Test Hardware Token Signature.";
				AssertEquals("Last pop-up should be exception error during signing", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Actual signing logic should be called", module.SignElectronicInvoiceCore_WasCalled);
				module.Reset_ForTest();
			}
		}

		class ARTransactionModuleStrip_ForSignElectronicInvoiceTests : ARTransactionModuleStrip
		{
			public bool SignElectronicInvoiceCore_WasCalled;
			public Exception SignElectronicInvoiceCore_Exception;

			public void Reset_ForTest()
			{
				SignElectronicInvoiceCore_WasCalled = false;
				SignElectronicInvoiceCore_Exception = null;
			}

			protected override (DialogResult result, int successCount, int errorCount, Exception ex) SignElectronicInvoiceCore(object sender, EventArgs e)
			{
				SignElectronicInvoiceCore_WasCalled = true;
				return (DialogResult.OK, 2, 1, SignElectronicInvoiceCore_Exception);
			}
		}

		#endregion

		#region GetLedgerSpecificEInvoicingActionMenuItems: Status Request Menu Item

		public void TestStatusRequestMenuItem_ForKoreaSouthCompany_Succeed()
		{
			var expectedMessage = "The request is sent.";
			AccEInvoicingTransactionPivot pivotSUB1, pivotSUB2;

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-1)))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				PrepareData();
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var query = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.StatusCheck);
				AssertEquals("Precondition: StatusCheck pivot should not exist", 0, Factory.Load<AccEInvoicingTransactionPivot>(query).Length);

				(testModule.GridCollection as BusinessObjectCollection).Load();
				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition", 2, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				AssertStatusRequestMenuItem(testModule, expectedMessage, () =>
				{
					var pivotSTA = Factory.Load<AccEInvoicingTransactionPivot>(query);
					AssertEquals("One StatusCheck pivot should be created", 1, pivotSTA.Length);
					AssertEquals("AIP_Status of pivotSTA should be the default value: Queued", EInvoicingPivotState.Queued, pivotSTA[0].AIP_Status);
					AssertEquals($"Transaction Number {pivotSTA[0].ParentTransactionHeader.AH_TransactionNum}: {expectedMessage}", UnitTestUserNotification.Instance.LastMessage.Text);

					pivotSUB1.Reload();
					pivotSUB2.Reload();
					AssertEquals($"AIP_Status should be updated from {EInvoicingPivotState.Failed} to DLV", EInvoicingPivotState.Delivered, pivotSUB1.AIP_Status);
					AssertEquals($"AIP_Status should be updated from {EInvoicingPivotState.Failed} to DLV", EInvoicingPivotState.Delivered, pivotSUB2.AIP_Status);
					AssertNullOrEmpty("AIP_ErrorDescription should be updated from 'Error Description' to empty", pivotSUB1.AIP_ErrorDescription);
					AssertNullOrEmpty("AIP_ErrorDescription should be updated from 'Error Description' to empty", pivotSUB2.AIP_ErrorDescription);
				});
			}

			void PrepareData()
			{
				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice1, TestObjectCreator.KRW, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice1.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));

				var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("Test001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice2, TestObjectCreator.KRW, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice2.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				pivotSUB1 = arInvoice1.GetMostRecentEInvoicingTransactionPivot();
				pivotSUB2 = arInvoice2.GetMostRecentEInvoicingTransactionPivot();
				var batchSUB = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				pivotSUB1.AIP_AIB = batchSUB.PK;
				pivotSUB2.AIP_AIB = batchSUB.PK;
				batchSUB.UpdatePivotsStatus(EInvoicingPivotState.Failed, new List<ZGuid> { arInvoice1.PK, arInvoice2.PK }, "Error Description", null);
				Factory.Save();
			}
		}

		public void TestStatusRequestMenuItem_ForKoreaSouthCompany_WhenEReportingStatusIsNotFAL()
		{
			var expectedMessage = "Transaction Number 00001000: The transaction is not eligible for requests because the E-Reporting Status of the transaction is not 'FAL - Failed'.";

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-1)))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				PrepareData();
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				(testModule.GridCollection as BusinessObjectCollection).Load();
				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition", 1, testModule.SelectedBusinessObjects_ForTestOnly.Length);
				AssertStatusRequestMenuItem(testModule, expectedMessage);
			}

			void PrepareData()
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("Test001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice, TestObjectCreator.KRW, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
				Assert("PreCondition", arInvoice.Lines.Cast<AccTransactionLines>().Any(x => x.TaxRate.AT_Type == AccTaxRate.Types.Rated));
				Factory.Save();

				var pivotSUB = arInvoice.GetMostRecentEInvoicingTransactionPivot();
				var batchSUB = TestObjectCreator.CreateEInvoicingBatch(10001, EInvoicingBatchState.Sent, GlbCompany.CurrentCompany);
				pivotSUB.AIP_AIB = batchSUB.PK;
				Factory.Save();
			}
		}

		public void TestStatusRequestMenuItem_ForKoreaSouthCompany_WhenTransactionIsNotEligibleForElectronicInvoicing()
		{
			var expectedMessage = "Transaction Number 00001000: Transaction is not eligible for Electronic Invoicing.";

			var arInvoiceNotEligible = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "4", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			arInvoiceNotEligible.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				(testModule.GridCollection as BusinessObjectCollection).Load();
				testModule.DisplayGrid.SelectAllElements();
				AssertEquals("Precondition", 1, testModule.SelectedBusinessObjects_ForTestOnly.Length);

				AssertStatusRequestMenuItem(testModule, expectedMessage);
			}
		}

		public void TestStatusRequestMenuItem_ForKoreaSouthCompany_WhenNoSelectedBusinessObjects()
		{
			var expectedMessage = @"Your request is being processed.

Please note:
1. Only transactions that have an E-Reporting Status of 'FAL - Failed' are eligible for 'Request e-Invoice Status'.
2. This action will update the E-Reporting Status of transactions in the same batch as the currently selected transaction(s) to 'DLV - Delivered' and submit a request to NTS to check on the invoice submission status.";

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				AssertStatusRequestMenuItem(testModule, expectedMessage);
			}
		}

		public void TestStatusRequestMenuItem_DisplayCondition()
		{
			if (Equals(GetModuleID(), ModuleIDs.AREnquiry))
			{
				Assert("Not applicable", true);

				return;
			}

			GlbStaff.CurrentUser.GS_LoginName = "test_notsupport";
			Assert("Precondition: should not be support user", !Env.CurrentUser.IsSupportUser);
			using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				var koreaStatusRequestMenuName = new KoreaSouthComplianceInfoEInvoicingExtension().StatusRequestMenuName;

				using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, true))
				{
					var menuItems = testModule.GetNewActionMenuItems_ForTestOnly();
					AssertNotNull(menuItems.FindByText(koreaStatusRequestMenuName));
					AssertEquals("Request e-Invoice Status menu count should be 1", menuItems.Where(m => m.Text == koreaStatusRequestMenuName).ToList().Count, 1);
				}

				using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.KoreaSouth, false))
				{
					AssertNull(testModule.GetNewActionMenuItems_ForTestOnly().FindByText(koreaStatusRequestMenuName));
				}

				using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.Turkey, true))
				{
					AssertNull(testModule.GetNewActionMenuItems_ForTestOnly().FindByText(new TurkeyComplianceInfo().StatusRequestMenuName));
				}
			}
		}

		void AssertStatusRequestMenuItem(ARTransactionModuleStrip module, string expectedMessage, Action additionalAssert = null)
		{
			var requestMenuItem = module.GetNewActionMenuItems_ForTestOnly().FindByText("Request e-Invoice Status");
			AssertNotNull("Precondition", requestMenuItem);

			AssertNullOrEmpty("Precondition", UnitTestUserNotification.Instance.LastMessage.Text);
			requestMenuItem.PerformClick();
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			additionalAssert?.Invoke();
		}

		#endregion

		public void TestWriteOffBadDebt()
		{
			// Test DB will not have the StmData base data, which contains the default value for bad debt for the registry.
			// So add bad debt into the registry for this test
			AccGLHeader badDebtAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "3980.00.00"));
			AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, badDebtAccount.PK.ToGuid());

			bool defaultSecurityValue = Env.Security.BadDebtWriteOffReceivablesInvoice.IsAllowed;
			try
			{
				using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				{
					Menu = module.GetNewActionMenuItems_ForTestOnly();
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						BusinessObjectCollection collection = module.GridCollection as BusinessObjectCollection;
						collection.Load();
						AssertNotNull(collection);
						collection.Load();
						AssertEquals("TestCollection should be empty", 0, collection.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						Menu.FindByText(module.BadDebtWriteOffMenuText_ForTestOnly).PerformClick();

						Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
						AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

						ARInvoice testInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
						testInvoice1.AH_InvoiceAmount = 21M;
						testInvoice1.AH_GSTAmount = 2.1M;
						testInvoice1.AH_OutstandingAmount = 23.1M;
						ARInvoice testInvoice2 = Factory.NewWithValidTestData<ARInvoice>();

						Factory.Save();

						module.PerformSearch_ForTest();
						collection.Load();
						AssertEquals("TestCollection should contain new invoices", 2, collection.Count);
						module.DisplayGrid.SelectSingleElement(testInvoice1);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						Env.Security.BadDebtWriteOffReceivablesInvoice.IsAllowed = false;
						Menu.FindByText(module.BadDebtWriteOffMenuText_ForTestOnly).PerformClick();
						Assert("Popup should be error", UnitTestUserNotification.Instance.LastMessage.WasError);
						Assert("Popup should be security error", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
					}
				}
			}
			finally
			{
				Env.Security.BadDebtWriteOffReceivablesInvoice.IsAllowed = defaultSecurityValue;
			}
		}

		public void TestWriteOffWhenReversalOfInvoiceIsPrevented()
		{
			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfReversalTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccGLHeader badDebtAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "3980.00.00"));
			AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, badDebtAccount.PK.ToGuid());

			bool defaultSecurityValue = Env.Security.BadDebtWriteOffReceivablesInvoice.IsAllowed;

			try
			{
				using (ARTransactionModuleStrip module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				{
					Menu = module.GetNewActionMenuItems_ForTestOnly();
					using (ZForm form = new ZForm())
					{
						form.Controls.Add(module.EmbeddedControl);
						form.Show();

						BusinessObjectCollection collection = module.GridCollection as BusinessObjectCollection;

						var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.Debtor, TestObjectCreator.GLHeader1.PK.ToGuid());

						Factory.Save();

						module.PerformSearch_ForTest();
						collection.Load();
						AssertEquals("TestCollection should contain new invoices", 1, collection.Count);

						module.DisplayGrid.SelectSingleElement(arInvoice);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						Menu.FindByText(module.BadDebtWriteOffMenuText_ForTestOnly).PerformClick();

						Assert("Should be in error", UnitTestUserNotification.Instance.LastMessage.WasError);

						var expectedMessage = "Write Off As Bad Debt is not permitted. Receivables Invoice Transactions cannot be reversed. This is controlled by the registry setting Accounting -> Receivable Defaults -> Default Settings -> Prevent Reversal of Invoice Transactions.";
						Assert("Write off as Bad debt should be prevented", UnitTestUserNotification.Instance.LastMessage.Contains(expectedMessage));
					}
				}
			}
			finally
			{
				Env.Security.BadDebtWriteOffReceivablesInvoice.IsAllowed = defaultSecurityValue;
			}
		}

		[TestDate(2012, 05, 15, 10, 44, 03)]
		public void TestPeriodicInvoiceBulkFormStartPostingWhenSaved()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));

			Action<OrgHeader> setupOrganisationForPeriodicInvoicePosting = (organisation) =>
			{
				var type = organisation.CompanyData.InvoiceTypes.AddNew();
				type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
				type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
				type.PI_RS_NKServiceLevel = "STD";
			};

			setupOrganisationForPeriodicInvoicePosting(TestObjectCreator.Agent);
			setupOrganisationForPeriodicInvoicePosting(TestObjectCreator.Agent2);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001"));
			job.LocalChargesPK = TestObjectCreator.LocalClient.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.Agent);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, TestObjectCreator.LocalClient2, TestObjectCreator.AUD, 100M, TestObjectCreator.Agent2);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var gLHeader1 = TestObjectCreator.GLHeader1;
			gLHeader1.AG_Description = "GLH" + TestObjectCreator.GetRandomString(3);
			gLHeader1.Factory.Save();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingBase miscInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M);
			miscInvoice.AH_OH = TestObjectCreator.Agent.PK;
			miscInvoice.AH_PostDate = ZDateTime.Today.AddDays(-10);
			miscInvoice.IsDisbursementOrFinal = false;
			TestObjectCreator.CreateInvoiceLine(miscInvoice, TestObjectCreator.AUD, 1M, 50M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			Factory.Save();

			Action<PeriodicInvoiceBulk> prepareBulkInvoice = (bulkInvoice) =>
			{
				bulkInvoice.PostDate = ZDateTime.Today;
				bulkInvoice.CurrencyNK = TestObjectCreator.AUD.RX_Code;
				bulkInvoice.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code))).ToList().ForEach(x => x.Value = true);
				bulkInvoice.LoadJobs();
				bulkInvoice.LoadMiscInvoices();
				AssertEquals("PeriodicInvoices.Count", 3, bulkInvoice.PeriodicInvoices.Count);
				bulkInvoice.RunPreSaveValidation();
				AssertNoErrors("Precondition: form bizo should allow to call Save method.", bulkInvoice);
			};

			Action<bool> assertInvoicePosted = (shouldInvoicesBePosted) =>
			{
				var filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, ZDateTime.Today);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				var invoices = Factory.Load<ARInvoice>(filter);
				var periodicInvoices = invoices.Where(x => x.IsPeriodicInvoice);
				AssertEquals("Periodic invoice should be posted.", shouldInvoicesBePosted ? 3 : 0, periodicInvoices.Count());

				foreach (var invoice in periodicInvoices)
				{
					AssertEquals("AH_TransactionType", InvoiceTypesList.Codes.FinalInvoice_Batching, invoice.AH_TransactionCategory);
					AssertEquals("Lines count", 1, invoice.Lines.Count);
				}
			};

			ZFormModaliser.LastFormShownForTest = null;
			using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				var menu = module.GetNewStandardMenuItems_ForTestOnly();
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					menu.FindByText("New").MenuItems.FindByText(module.NewPeriodicInvoiceBulkMenuText_ForTestOnly).PerformClick();
					var bulkInvoiceForm = module.LastPeriodicInvoiceBulkForm;
					AssertNotNull("PeriodicInvoicingBulkForm", bulkInvoiceForm);
					prepareBulkInvoice(bulkInvoiceForm.BusinessEntity);
					bulkInvoiceForm.Close();
					AssertNull("Posting process should not be started.", ZFormModaliser.LastFormShownForTest as ProgressForm);
					assertInvoicePosted(false);
				}
			}
			using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				var menu = module.GetNewStandardMenuItems_ForTestOnly();
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					menu.FindByText("New").MenuItems.FindByText(module.NewPeriodicInvoiceBulkMenuText_ForTestOnly).PerformClick();
					var bulkInvoiceForm = module.LastPeriodicInvoiceBulkForm;
					AssertNotNull("PeriodicInvoicingBulkForm", bulkInvoiceForm);
					prepareBulkInvoice(bulkInvoiceForm.BusinessEntity);
					var postButton = (ZButton)bulkInvoiceForm.Controls.Find("PostButton", true).FirstOrDefault();
					AssertNotNull("PostButton in not found in bulkInvoiceForm", postButton);
					postButton.PerformClick();
					AssertEquals("Periodic Bulk invoice Form should be disposed at this stage for performance reasons.", true, bulkInvoiceForm.IsDisposed);
					AssertNotNull("Posting process controller has been created.", module.LastPostController);
					AssertNotNull("Posting process has been started.", module.LastPostController.LastProgressFormForTestOnly);
					AssertEquals("Progress form has been shown.", true, module.LastPostController.LastProgressFormForTestOnly.Visible);
					assertInvoicePosted(false);
					Application.DoEvents();
					AssertEquals("Progress form should stay visible to allow user to analyse log.", true, module.LastPostController.LastProgressFormForTestOnly.Visible);
					AssertType("Progress form type.", typeof(ProgressWithDetailesForm), module.LastPostController.LastProgressFormForTestOnly);
					var progressForm = (ProgressWithDetailesForm)module.LastPostController.LastProgressFormForTestOnly;
					string expectedLog =
@"
15-May-12 10:44: Posting is started.
15-May-12 10:44: Success - Invoice ZAgent, AUD, FID: AR INV 00001001 was posted.
15-May-12 10:44: Success - Invoice ZAgent2, AUD, FID: AR INV 00001002 was posted.
15-May-12 10:44: Success - Invoice ZAgent, AUD, FID: AR INV 00001003 was posted.
15-May-12 10:44: Posting is finished. Posted 3 of 3 periodic invoices.
Elapsed time: 00:00:00";
					AssertMultilineASCIIEquals("Posting process should be logged.", expectedLog, progressForm.Log);
					module.LastPostController.LastProgressFormForTestOnly.Close();
					assertInvoicePosted(true);
					string periodicInvoiceBulkFormPostButtonClick = "ZForm.OnPostButtonClick";
					AssertContains("StratPosting should be called from PeriodicInvoiceBulkForm Post button click", periodicInvoiceBulkFormPostButtonClick, module.LastPostController.LastStackTraceInStartPosting_ForTestOnly.ToString());
					AssertNotContains("PeriodicInvoiceBulkPoster should be called outside PeriodicInvoiceBulkForm Post button click to allow PeriodicInvoiceBulkForm to be GC collected",
						periodicInvoiceBulkFormPostButtonClick, module.LastPostController.GetPoster_ForTestOnly().LastStackTraceInPost_ForTestOnly.ToString());
				}
			}
		}

		[TestDate(2012, 05, 15, 10, 44, 03)]
		public void TestPeriodicInvoiceBulkFormPostingWhenErrorsExist()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));

			Action<OrgHeader> setupOrganisationForPeriodicInvoicePosting = (organisation) =>
			{
				var type = organisation.CompanyData.InvoiceTypes.AddNew();
				type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
				type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
				type.PI_RS_NKServiceLevel = "STD";
			};

			setupOrganisationForPeriodicInvoicePosting(TestObjectCreator.Agent);
			setupOrganisationForPeriodicInvoicePosting(TestObjectCreator.Agent2);
			setupOrganisationForPeriodicInvoicePosting(TestObjectCreator.LocalClient);

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001"));
			job.LocalChargesPK = TestObjectCreator.LocalClient.PK;
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.Agent);
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 200M, TestObjectCreator.Agent2);
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			var charge3 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 300M, TestObjectCreator.LocalClient);
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var gLHeader1 = TestObjectCreator.GLHeader1;
			gLHeader1.AG_Description = "GLH" + TestObjectCreator.GetRandomString(3);
			gLHeader1.Factory.Save();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingBase miscInvoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M);
			miscInvoice1.AH_OH = TestObjectCreator.Agent.PK;
			miscInvoice1.AH_PostDate = ZDateTime.Today.AddDays(-10);
			miscInvoice1.IsDisbursementOrFinal = false;
			var miscInvoice1Line = TestObjectCreator.CreateInvoiceLine(miscInvoice1, TestObjectCreator.AUD, 1M, 50M, 0M, 0M, TestObjectCreator.GLHeader1.PK);

			Factory.Save();

			Action<PeriodicInvoiceBulk> prepareBulkInvoice = (bulkInvoice) =>
			{
				bulkInvoice.PostDate = ZDateTime.Today;
				bulkInvoice.CurrencyNK = TestObjectCreator.AUD.RX_Code;
				bulkInvoice.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code))).ToList().ForEach(x => x.Value = true);
				bulkInvoice.LoadJobs();
				bulkInvoice.LoadMiscInvoices();
				AssertEquals("PeriodicInvoices.Count", 4, bulkInvoice.PeriodicInvoices.Count);
				bulkInvoice.RunPreSaveValidation();
				AssertNoErrors("Precondition: form bizo should allow to call Save method.", bulkInvoice);
			};

			ZFormModaliser.LastFormShownForTest = null;
			using (var module = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
			{
				var menu = module.GetNewStandardMenuItems_ForTestOnly();
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();
					menu.FindByText("New").MenuItems.FindByText(module.NewPeriodicInvoiceBulkMenuText_ForTestOnly).PerformClick();
					var bulkInvoiceForm = module.LastPeriodicInvoiceBulkForm;
					AssertNotNull("PeriodicInvoicingBulkForm", bulkInvoiceForm);
					prepareBulkInvoice(bulkInvoiceForm.BusinessEntity);
					bulkInvoiceForm.FireSaveButton();

					//Chages are changed during posting
					var newFactory = new BusinessObjectFactory();
					newFactory.RefreshEnabled = false;
					newFactory.Load<Charge>(charge1.PK).JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
					newFactory.Load<Charge>(charge3.PK).Delete();
					newFactory.Save();

					bulkInvoiceForm.Close();
					Application.DoEvents();
					AssertEquals("Progress form should stay visible to allow user to analyze log.", true, module.LastPostController.LastProgressFormForTestOnly.Visible);
					AssertType("Progress form type.", typeof(ProgressWithDetailesForm), module.LastPostController.LastProgressFormForTestOnly);
					var progressForm = (ProgressWithDetailesForm)module.LastPostController.LastProgressFormForTestOnly;
					string expectedLog =
@"
15-May-12 10:44: Posting is started.
15-May-12 10:44: ERROR - Invoice ZAgent, AUD, FID: No AR transaction was posted.
15-May-12 10:44: Success - Invoice ZAgent2, AUD, FID: AR INV 00001001 was posted.
15-May-12 10:44: ERROR - Invoice ZLOCCLT, AUD, FID: 
	Error - record: Charge count for the invoice was changed from 1 to 0.
	Error - record: Total Local Ex Tax Amount of this invoice has changed from $300.00 to $0.00.
	Error - record: Total Local Tax Amount of this invoice has changed from $30.00 to $0.00.
15-May-12 10:44: Success - Invoice ZAgent, AUD, FID: AR INV 00001002 was posted.
15-May-12 10:44: Posting is finished. Posted 2, failed 2 of 4 periodic invoices.
Elapsed time: 00:00:00

Errors:
15-May-12 10:44: ERROR - Invoice ZAgent, AUD, FID: No AR transaction was posted.
15-May-12 10:44: ERROR - Invoice ZLOCCLT, AUD, FID: 
	Error - record: Charge count for the invoice was changed from 1 to 0.
	Error - record: Total Local Ex Tax Amount of this invoice has changed from $300.00 to $0.00.
	Error - record: Total Local Tax Amount of this invoice has changed from $30.00 to $0.00.
";
					AssertMultilineASCIIEquals("Posting process should be logged.", expectedLog, progressForm.Log);
					module.LastPostController.LastProgressFormForTestOnly.Close();

					var filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, ZDateTime.Today);
					filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
					var invoices = Factory.Load<ARInvoice>(filter);
					var periodicInvoices = invoices.Where(x => x.IsPeriodicInvoice);
					AssertEquals("Only periodic invoices without errors should be posted.", 2, periodicInvoices.Count());
				}
			}
		}

		public virtual void TestRegenerateJournalEntriesActionMenuItem()
		{
			var module = (ARTransactionModuleStrip)TestTransactionModule;
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var actionMenu = module.GetNewActionMenuItems_ForTestOnly();
			var regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
			AssertNotNull("Menu item should exist", regenerateJournalEntriesMenuItem);

			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			actionMenu = module.GetNewActionMenuItems_ForTestOnly();
			regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
			AssertNull("Menu item should not exist", regenerateJournalEntriesMenuItem);

			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var nonSupportStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, User.SupportUserName));
			using (Env.SetTemporaryUserContext(nonSupportStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				actionMenu = module.GetNewActionMenuItems_ForTestOnly();
				regenerateJournalEntriesMenuItem = actionMenu.FindByText("Regenerate Journal Entries (CWSupport Only)");
				AssertNull("Menu item should not exist", regenerateJournalEntriesMenuItem);
			}
		}

		[TestDate(2022, 01, 01)]
		public virtual void TestRegenerateJournalEntries()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.CreateARSuspenseControlAccount().PK.ToGuid());
			var mockDataRecover = new Mock<IGeneralLedgerDataRecover>();

			using (ObjectFactory.Substitute(mockDataRecover.Object))
			using (var module = ZModuleFactory.Instance.Create(ModuleID) as ARTransactionModuleStrip)
			{
				using (var form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var arInvoice = (ARInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
					var line = arInvoice.Lines[0];
					line.AL_AT = TestObjectCreator.GST1.PK;
					line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;

					var matchLinkGroup = new TransactionMatchLinkGroup(Factory);
					var matchLink = matchLinkGroup.AddNew();
					matchLink.AP_MatchDate = ZDateTime.Today;
					matchLink.AP_MatchGroupNum = "M1";
					matchLink.AP_AH = arInvoice.PK;

					var cashBasisVAT = TestObjectCreator.CreateCashBasisVAT(line, 100, 10, matchLink);

					var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
					var taxTransaction = Factory.New<AccTaxTransaction>();
					taxTransaction.ATT_ETC = taxConfiguration.PK;
					taxTransaction.ATT_GC = GlbCompany.CurrentCompany.PK;
					taxTransaction.ATT_GB = GlbBranch.CurrentBranch.PK;
					taxTransaction.ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
					taxTransaction.ATT_Ledger = LedgerTypes.AccountsReceivable;
					taxTransaction.ATT_Basis = "MAT";
					taxTransaction.ATT_RX_NKOSTaxCurrency = "AUD";
					taxTransaction.ATT_LocalTaxAmount = 10m;
					taxTransaction.ATT_OSTaxAmount = 200m;
					taxTransaction.ATT_TaxSuperType = TaxSuperTypeList.Perceptions.Code;
					taxTransaction.ATT_RateNumerator = 2;
					taxTransaction.ATT_RateDenominator = 1;
					taxTransaction.ATT_A9_TaxMessage = Factory.NewWithValidTestData<AccInvMsg>().PK;
					taxTransaction.ATT_PostDate = ZDate.Today;
					taxTransaction.ATT_TaxDate = ZDate.Today;
					taxTransaction.ATT_TaxSystemCode = "DNC";
					taxTransaction.ATT_AH = arInvoice.PK;
					taxTransaction.ATT_AT_TaxID = Factory.NewWithValidTestData<AccTaxRate>().PK;
					taxTransaction.ATT_AG_LedgerControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;
					taxTransaction.ATT_AG_TaxPendingControlAccount = Factory.NewWithValidTestData<AccGLHeader>().PK;

					var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxTransaction.PK));
					if (pivots.Length == 0)
					{
						var pivot = Factory.New<AccTaxRecordTransactionLinePivot>();
						pivot.ATP_ATT = taxTransaction.PK;
						pivot.FillWithValidTestData();
					}

					var arJournal = TestObjectCreator.CreateJournal<ARJournal>(100m, ZDateTime.Today, TestObjectCreator.ABIGAS.PK);
					Factory.Save();

					var taxGLMovement = Factory.New<AccTaxGLMovement>();

					taxGLMovement.ATM_ATT_TaxTransaction = taxTransaction.PK;
					taxGLMovement.ATM_Period = 202201;
					taxGLMovement.ATM_Date = ZDate.Today;
					taxGLMovement.ATM_Amount = 100m;
					taxGLMovement.ATM_Type = TaxGLMovementTypeList.Realised.Code;

					var glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
					var glHeader2 = Factory.NewWithValidTestData<AccGLHeader>();

					taxGLMovement.ATM_AG_DebitAccount = glHeader1.PK;
					taxGLMovement.ATM_AG_CreditAccount = glHeader2.PK;

					Factory.Save();

					var taxGLMovements = Factory.Load<AccTaxGLMovement>(new ZQuery(AccTaxGLMovementSchema.ATM_ATT_TaxTransaction, taxTransaction.PK));

					module.PerformSearch_ForTest();
					module.DisplayGrid.SelectAllElements();

					AssertEquals(2, module.SelectedBusinessObjects_ForTestOnly.Length);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					module.HandleRegenerateJournalEntries_ForTestOnly(this, new EventArgs());
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader, It.Is<BusinessObject[]>(y => y.Length == 1 && y.FirstOrDefault().PK == arJournal.PK)), Times.Once);
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_AL_TransactionLine, It.Is<BusinessObject[]>(y => y.Length == arInvoice.Lines.Count && y.All(z => arInvoice.Lines.Any(a => a.PK == z.PK)))), Times.Once);
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_YC_CashBasisVAT, It.Is<BusinessObject[]>(y => y.Length == 1 && y.FirstOrDefault().PK == cashBasisVAT.PK)), Times.Once);
					mockDataRecover.Verify(x => x.RecoverPartOfGLD(AccGeneralLedgerDataSchema.GLD_ATM_TaxGLMovement, It.Is<BusinessObject[]>(y => y.Length == taxGLMovements.Length && y.All(z => taxGLMovements.Any(a => a.PK == z.PK)))), Times.Once);
				}
			}
		}

		public void TestQueueInvoiceForTransmissionMenuItem()
		{
			if (Equals(GetModuleID(), ModuleIDs.ARTransaction))
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (var testModule = (ARTransactionModuleStrip)ZModuleFactory.Instance.Create(ModuleID))
				using (var form = new ZForm())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					var queueInvoiceForTransmissionMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText("Queue Invoice For Transmission");
					AssertNotNull("Queue Invoice For Transmission Menu Item should exist", queueInvoiceForTransmissionMenuItem);
				}
			}
			else
			{
				Assert(true);
			}
		}

		#region MatchTransactionsMenuItem

		protected override InvoicingBase CreateTransactionsForMatchTransactionPopup()
		{
			TestObjectCreator.CreateARInvoice<ARInvoice>("AR100001", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			TestObjectCreator.CreateARInvoice<ARInvoice>("AR100002", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
			return TestObjectCreator.CreateARInvoice<ARInvoice>("AR100003", TestObjectCreator.AUD, 1.0m, TestObjectCreator.Debtor);
		}

		protected override Type ExpectedModuleTypeForMatchTransactionPopup => typeof(ARMatchingModule);

		protected override SecurityCheckpoint SecurityCheckpointForMatchTransactions => Env.Security.MatchReceivablesTransactions;

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ARTransaction;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		protected override Journal CreateJournal()
		{
			return Factory.NewWithValidTestData<ARJournal>();
		}

		protected override InvoicingBase CreateInvoice()
		{
			return Factory.NewWithValidTestData<ARInvoice>();
		}

		protected override InvoicingBase CreateCreditNote()
		{
			return Factory.NewWithValidTestData<ARCreditNote>();
		}

		protected override Receipt CreateReceipt()
		{
			return Factory.NewWithValidTestData<ARReceipt>();
		}

		protected override Payment CreatePayment()
		{
			return Factory.NewWithValidTestData<ARPayment>();
		}

		protected ARTransactionModuleStrip ARModule
		{
			get { return (ARTransactionModuleStrip)TestTransactionModule; }
		}

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			APContraRow apContraRow = Factory.NewWithValidTestData<APContraRow>();
			ARContraRow arContraRow = Factory.NewWithValidTestData<ARContraRow>();
			arContraRow.AH_TransactionNum = apContraRow.AH_TransactionNum;
			apContraRow.AH_TransactionBelongsToGroup = arContraRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			arContraRow.AH_OH = TestObjectCreator.ABIGAS.PK;
			apContraRow.AH_OH = TestObjectCreator.AALSHI.PK;

			ARTransferFromRow transferFrom1 = Factory.NewWithValidTestData<ARTransferFromRow>();
			ARTransferToRow transferTo1 = Factory.NewWithValidTestData<ARTransferToRow>();
			transferTo1.AH_TransactionCount = 2;
			transferTo1.AH_TransactionNum = transferFrom1.AH_TransactionNum;
			transferFrom1.AH_TransactionBelongsToGroup = transferTo1.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			transferTo1.AH_OH = TestObjectCreator.LocalClient.PK;
			transferFrom1.AH_OH = TestObjectCreator.LocalClient2.PK;
			transferFrom1.AH_OSExTaxAmount = transferTo1.AH_OSExTaxAmount = 10M;

			ARTransferFromRow transferFrom2 = Factory.NewWithValidTestData<ARTransferFromRow>();
			ARTransferToRow transferTo2 = Factory.NewWithValidTestData<ARTransferToRow>();
			transferTo2.AH_TransactionCount = 2;
			transferTo2.AH_TransactionNum = transferFrom2.AH_TransactionNum;
			transferFrom2.AH_TransactionBelongsToGroup = transferTo2.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
			transferTo2.AH_OH = TestObjectCreator.LocalClient.PK;
			transferFrom2.AH_OH = TestObjectCreator.LocalClient2.PK;
			transferFrom2.AH_OSExTaxAmount = transferTo2.AH_OSExTaxAmount = 10M;

			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();

			ARInvoice jobRelatedInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			jobRelatedInvoice1.AH_JH = testJob.PK;
			jobRelatedInvoice1.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(jobRelatedInvoice1, testJob);

			ARInvoice jobRelatedInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			jobRelatedInvoice2.AH_JH = testJob.PK;
			jobRelatedInvoice2.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(jobRelatedInvoice2, testJob);

			AssertEquals("Precondition: It must be created invoices with the same job to test concurrency issue in multiple reversing.",
				jobRelatedInvoice1.AH_JH, jobRelatedInvoice2.AH_JH);

			return new BusinessObject[] { arContraRow,
												transferFrom1,
												transferTo2,
												Factory.NewWithValidTestData(typeof(ARInvoice)),
												Factory.NewWithValidTestData(typeof(ARCreditNote)),
												Factory.NewWithValidTestData(typeof(ARAdjustmentNote)),
												Factory.NewWithValidTestData(typeof(ARJournal)),
												Factory.NewWithValidTestData(typeof(ARPayment)),
												Factory.NewWithValidTestData(typeof(ARReceipt)),
												jobRelatedInvoice1,
												jobRelatedInvoice2 };
		}

		public override void TestPromptToPrintComplianceDocumentWithRollup()
		{
			PromptToPrintComplianceDocumentCore(AssertForPromptToPrintComplianceDocument, true, true);
			PromptToPrintComplianceDocumentCore(AssertForNotPromptToPrintComplianceDocument, true, false);
		}

		public override void TestPromptToPrintComplianceDocumentWithoutRollup()
		{
			PromptToPrintComplianceDocumentCore(AssertForPromptToPrintComplianceDocument, false, true);
			PromptToPrintComplianceDocumentCore(AssertForNotPromptToPrintComplianceDocument, false, false);
		}

		protected override ControllerID InvoiceControllerID
		{
			get { return ControllerIDs.ARInvoice; }
		}

		protected override ControllerID ExchangeDifferenceControllerID
		{
			get { return ControllerIDs.ARExchangeDifference; }
		}

		protected override ControllerID DiscountControllerID
		{
			get { return ControllerIDs.ARDiscount; }
		}

		protected override ControllerID OverpaymentControllerID
		{
			get { return ControllerIDs.AROverpayment; }
		}

		protected override AccTransactionHeader GetInvoiceToTestControllerID()
		{
			return Factory.NewWithValidTestData<ARInvoice>();
		}

		protected override Invoice GetExistingInvoiceForTest
		{
			get { return ARInv; }
		}

		TaxSystemsConfigurationCollection CreateTaxSystemConfigurationForBrazil()
		{
			var helper = new AccountingTestObjectCreator(Factory);
			var taxAuthority = helper.CreateTaxAuthority("TA");
			taxAuthority.Country = Core.Constants.CountryCodes.Brazil;
			taxAuthority.TaxAuthorityType = AccountingMasterFilesTaxFrameworkConstants.TaxAuthorityTypeList.National.Code;

			var taxSystemISS = helper.CreateTaxSystem("ISS");
			var taxSystemINSS = helper.CreateTaxSystem("INSS");

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemISS.Code = "ISS";
			taxSystemISS.Name = "PROVISÃO DE ISS";
			taxSystemISS.Country = Core.Constants.CountryCodes.Brazil;
			taxSystemsConfigCollection.Add(taxSystemISS);
			taxSystemINSS.Code = "INSS";
			taxSystemINSS.Name = "RETENÇÃO DE INSS";
			taxSystemINSS.Country = Core.Constants.CountryCodes.Brazil;
			taxSystemsConfigCollection.Add(taxSystemINSS);

			return taxSystemsConfigCollection;
		}

		AccCollectionOrderLine CreateCollectionBatchOrderLineForInvoice(InvoicingBase invoice, ZString currency, ZGuid? debtor = null, DebtorValidation debtorValidationType = DebtorValidation.DebtorIsRequired)
		{
			var collectionBatch = CreateCollectionBatch(invoice, currency);
			var collectionOrder = CreateCollectionOrder(collectionBatch, debtor, debtorValidationType);
			var collectionLine = CreateCollectionOrderLine(collectionOrder, invoice);
			Factory.Save();

			return collectionLine;
		}

		AccCollectionBatch CreateCollectionBatch(InvoicingBase invoice, ZString currency)
		{
			var batch = Factory.New<AccCollectionBatch>();
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			batch.ACB_GC = invoice.AH_GC;
			batch.IsCancelled = false;
			batch.ACB_BatchNumber = ZString.Empty;
			batch.ACB_AB = bankAccount.PK;
			batch.ACB_RX_NKCurrency = currency;
			batch.ACB_TotalAmount = 100m;

			return batch;
		}

		AccCollectionOrder CreateCollectionOrder(AccCollectionBatch batch, ZGuid? debtor = null, DebtorValidation debtorValidationType = DebtorValidation.DebtorIsRequired)
		{
			var order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDateTime.Today.Date;
			order.ACO_IsCancelled = false;
			order.ACO_OrderNumber = "00001000";
			order.ACO_Amount = 100m;
			order.ACO_OH_Debtor = debtor ?? ZGuid.Empty;
			order.DebtorValidationType = debtorValidationType;

			return order;
		}

		AccCollectionOrderLine CreateCollectionOrderLine(AccCollectionOrder order, InvoicingBase invoice)
		{
			var line = Factory.New<AccCollectionOrderLine>();
			line.AOL_ACO = order.PK;
			line.AOL_AH = invoice.PK;
			line.IsCancelled = false;

			return line;
		}

		#endregion
	}
}
