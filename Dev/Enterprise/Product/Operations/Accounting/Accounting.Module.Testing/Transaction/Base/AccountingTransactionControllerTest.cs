using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.Base;
using Enterprise.Accounting.Module.Transaction.Base.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class AccountingTransactionControllerTest : TransactionControllerWithLoginCompanyCheckTest
	{
		public void TestShouldShowReverseConfirmationMessage()
		{
			AssertShouldShowReverseConfirmationMessage(false);
			AssertShouldShowReverseConfirmationMessage(true);

			void AssertShouldShowReverseConfirmationMessage(bool isMultiReversing)
			{
				var transaction = ParentTransactionHeaderRow;
				var reversingBaseMock = new Mock<ReversingBase>(transaction);
				reversingBaseMock.Protected().Setup<bool>("CanTransactionBeReversed").Returns(true);
				Assert("Precondition: CanReverseTransaction", reversingBaseMock.Object.CanReverseTransaction);
				reversingBaseMock.Setup(x => x.ShouldShowReverseConfirmationMessage()).Returns(true);
				var expectedQuestion = "Do you want to reverse in this test?";
				reversingBaseMock.Setup(x => x.GetReverseConfirmationMessage()).Returns(expectedQuestion);

				AccountingTransactionController.Reversing_ForTestOnly = reversingBaseMock.Object;
				if (isMultiReversing)
				{
					AccountingTransactionController.MultipleReversingProvider_ForTestOnly = new MultipleReversingProviderForHeader();
				}

				AccountingTransactionController.ReversingErrors_ForTestOnly.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var isOperationDone = AccountingTransactionController.DoBaseReversing_ForTestOnly(transaction);

				Assert(nameof(isOperationDone), !isOperationDone);
				AssertEquals("LastMessage", expectedQuestion, UnitTestUserNotification.Instance.LastMessage.Text);

				if (isMultiReversing)
				{
					AssertEquals("ReversingErrors_ForTestOnly.Count", 1, AccountingTransactionController.ReversingErrors_ForTestOnly.Count);
					var expectedError =
$@"User has answered 'No' to the next question:
{expectedQuestion}";
					AssertEquals("ReversingErrors_ForTestOnly", expectedError, AccountingTransactionController.ReversingErrors_ForTestOnly[0]);
				}
				else
				{
					AssertEquals("ReversingErrors_ForTestOnly.Count", 0, AccountingTransactionController.ReversingErrors_ForTestOnly.Count);
				}
				reversingBaseMock.Protected().Verify("DoReverseTransaction", Times.Never());

				AccountingTransactionController.ReversingErrors_ForTestOnly.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();

				isOperationDone = AccountingTransactionController.DoBaseReversing_ForTestOnly(ParentTransactionHeaderRow);

				Assert(nameof(isOperationDone), isOperationDone);
				AssertEquals("LastMessage", expectedQuestion, UnitTestUserNotification.Instance.LastMessage.Text);
				reversingBaseMock.Protected().Verify("DoReverseTransaction", Times.Once());
				AssertEquals("ReversingErrors_ForTestOnly.Count", 0, AccountingTransactionController.ReversingErrors_ForTestOnly.Count);
			}
		}

		#region Credit Note Reversal Test Cases

		public void TestApprovalLevelRightsAreOnlyCheckedForARInvoiceAndARAdjustmentNoteReversal()
		{
			var controllerID = GetControllerID();
			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
			Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

			if (DeleteShouldShowForm)
			{
				((TransactionHeader)ParentTransactionHeaderRow).AH_InvoiceAmount = 20m;
				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				using (var reverseForm = Controller.ShowDeleteForm(ParentTransactionHeaderRow) as ZForm)
				{
					if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARInvoiceForInterCompanyTransaction || controllerID == ControllerIDs.ARAdjustmentNote)
					{
						AssertNull(reverseForm);
						AssertEquals("Should show login form", typeof(LoginFormWithRequest), ZFormModaliser.LastFormShownDialogForTest.GetType());
					}
					else
					{
						AssertNotNull(reverseForm);
					}
				}
			}
			else
			{
				Assert($"{controllerID.ToString()} does not show a reverse form", true);
			}
		}

		public void TestUserHasApprovalLevelRights_NoExisitingRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(0, approvalRequestFortransaction.Length);

				Assert(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNotNull(reverseForm);
						reverseForm.FireSaveButton();
					}

					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(0, approvalRequestFortransaction.Length);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndProvidesOnTheSpotAuthorization_NoExisitingRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(0, approvalRequestFortransaction.Length);

				SecurityTestObject.CreateTestUser(true, Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "US1", "User1", "pass");

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					SecurityOverrideProviderSource.Get(transaction).Provider = new InvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							loginForm.DoLoginForTest("User1", "pass");
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
						}
					});
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNotNull(reverseForm);
						reverseForm.FireSaveButton();
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(0, approvalRequestFortransaction.Length);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndCancelOutFromSecurityOverrideScreen_NoExisitingRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(0, approvalRequestFortransaction.Length);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					SecurityOverrideProviderSource.Get(transaction).Provider = new InvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel; //user cancel out from login screen
						}
					});
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNull(reverseForm);
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(!transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(0, approvalRequestFortransaction.Length);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndWantsToCreateApprovalRequests_NoExisitingRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(0, approvalRequestFortransaction.Length);

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					SecurityOverrideProviderSource.Get(transaction).Provider = new InvoicingSecurityOverrideProvider();

					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						var approvalBulkForm = form as ARCreditNoteApprovalBulkForm;
						if (loginForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; //user clicks approval request button
						}
						else if (approvalBulkForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user clicks continue button on approval form
						}
					});
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNull(reverseForm);
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(!transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals("New request created", 1, approvalRequestFortransaction.Length);
					AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestFortransaction[0].XP_ApprovalStatus);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserHasApprovalLevelRights_ExistingREQRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				CreateApprovalRequest(transaction, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(1, approvalRequestFortransaction.Length);
				AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestFortransaction[0].XP_ApprovalStatus);

				Assert(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNotNull(reverseForm);
						reverseForm.FireSaveButton();
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(1, approvalRequestFortransaction.Length);
					AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequestFortransaction[0].XP_ApprovalStatus);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndProvidesOnTheSpotAuthorization_ExisitngREQRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				CreateApprovalRequest(transaction, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(1, approvalRequestFortransaction.Length);
				AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestFortransaction[0].XP_ApprovalStatus);

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				SecurityTestObject.CreateTestUser(true, Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "US1", "User1", "pass");

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					SecurityOverrideProviderSource.Get(transaction).Provider = new InvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							loginForm.DoLoginForTest("User1", "pass");
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user provide on the spot authorization
						}
					});
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNotNull(reverseForm);
						reverseForm.FireSaveButton();
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(1, approvalRequestFortransaction.Length);
					AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequestFortransaction[0].XP_ApprovalStatus);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndCancelOutFromSecurityOverrideScreen_ExisitngREQRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				CreateApprovalRequest(transaction, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(1, approvalRequestFortransaction.Length);
				AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestFortransaction[0].XP_ApprovalStatus);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					SecurityOverrideProviderSource.Get(transaction).Provider = new InvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel; //user cancel out from login screen
						}
					});
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNull(reverseForm);
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(!transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(1, approvalRequestFortransaction.Length);
					AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestFortransaction[0].XP_ApprovalStatus);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndWantsToCreateApprovalRequests_ExisitngREQRequest_LikeToCancelExisitngREQRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				CreateApprovalRequest(transaction, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(1, approvalRequestFortransaction.Length);
				var previousApprovalRequestFortransaction = approvalRequestFortransaction[0];

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					SecurityOverrideProviderSource.Get(transaction).Provider = new InvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						var approvalBulkForm = form as ARCreditNoteApprovalBulkForm;
						if (loginForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; //user clicks approval request button
						}
						else if (approvalBulkForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; //user clicks continue button on approval form
						}
					});
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); //user wants to cancel exisitng requests
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNull(reverseForm);
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(!transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(2, approvalRequestFortransaction.Length);
					AssertEquals("Previous request cancelled", previousApprovalRequestFortransaction.PK, approvalRequestFortransaction.First(x => x.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Cancelled).PK);
					AssertNotEquals("New request created", previousApprovalRequestFortransaction.PK, approvalRequestFortransaction.First(x => x.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Requested).PK);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserDoesNotHaveApprovalLevelRightsAndWantsToCreateApprovalRequests_ExisitngREQRequest_DoNotWantToCancelExistingREQRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				CreateApprovalRequest(transaction, Core.Constants.GenApprovalRequestApprovalStatus.Requested);
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(1, approvalRequestFortransaction.Length);

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					SecurityOverrideProviderSource.Get(transaction).Provider = new InvoicingSecurityOverrideProvider();
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						var loginForm = form as LoginForm;
						if (loginForm != null)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore; //user clicks approval request button
						}
					});
					UnitTestUserNotification.Instance.ClearUserResponses();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No); //user does not want to cancel exisitng requests
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNull(reverseForm);
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(!transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(1, approvalRequestFortransaction.Length);
					AssertEquals("Previous request not cancelled", Core.Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequestFortransaction[0].XP_ApprovalStatus);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserHasApprovalLevelRights_ExisitngAPPRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				CreateApprovalRequest(transaction, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<ARCreditNoteApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(1, approvalRequestFortransaction.Length);
				AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestFortransaction[0].XP_ApprovalStatus);
				approvalRequestFortransaction[0].PostingDetails.MaxAuthorisationLevelRequired = 1;
				Factory.Save();

				Assert(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNotNull(reverseForm);
						reverseForm.FireSaveButton();
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<ARCreditNoteApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(1, approvalRequestFortransaction.Length);
					AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequestFortransaction[0].XP_ApprovalStatus);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserDoesNotHaveApprovalLevelRights_ExisitngAPPRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				CreateApprovalRequest(transaction, Core.Constants.GenApprovalRequestApprovalStatus.Approved);
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<ARCreditNoteApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(1, approvalRequestFortransaction.Length);
				AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Approved, approvalRequestFortransaction[0].XP_ApprovalStatus);
				approvalRequestFortransaction[0].PostingDetails.MaxAuthorisationLevelRequired = 1;
				Factory.Save();

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNotNull(reverseForm);
						reverseForm.FireSaveButton();
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<ARCreditNoteApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(1, approvalRequestFortransaction.Length);
					AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequestFortransaction[0].XP_ApprovalStatus);
					AssertEquals(1, approvalRequestFortransaction[0].PostingDetails.MaxAuthorisationLevelRequired);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserHasApprovalLevelRights_ExisitingREJRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				CreateApprovalRequest(transaction, Core.Constants.GenApprovalRequestApprovalStatus.Rejected);
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(1, approvalRequestFortransaction.Length);
				AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Rejected, approvalRequestFortransaction[0].XP_ApprovalStatus);

				Assert(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertNotNull(reverseForm);
						reverseForm.FireSaveButton();
					}
					var newFactory = new BusinessObjectFactory();
					var transactionInNewFactory = newFactory.Load<InvoicingBase>(transaction.PK);
					Assert(transactionInNewFactory.IsReversed);
					approvalRequestFortransaction = newFactory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
					AssertEquals(1, approvalRequestFortransaction.Length);
					AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Rejected, approvalRequestFortransaction[0].XP_ApprovalStatus);
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		public void TestUserDoesNotHaveApprovalLevelRights_ExisitingREJRequest()
		{
			var controllerID = GetControllerID();
			if (controllerID == ControllerIDs.ARInvoice || controllerID == ControllerIDs.ARAdjustmentNote)
			{
				InvoicingBase transaction = null;
				if (controllerID == ControllerIDs.ARInvoice)
				{
					var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
					var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
					Factory.Save();
					transaction = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
					transaction.Lines.Add(TestObjectCreator.CreateRevenueLine(charge1, transaction.PK));
				}
				else
				{
					transaction = TestObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>("AR001", 10m, 0m, ZDateTime.Today, TestObjectCreator.AALSHI.PK);
					TestObjectCreator.CreateAdjusmentNoteLine(transaction as ARAdjustmentNote, TestObjectCreator.CC1.PK, 10m, 0m);
				}
				Factory.Save();

				CreateApprovalRequest(transaction, Core.Constants.GenApprovalRequestApprovalStatus.Rejected);
				Factory.Save();

				var approvalRequestFortransaction = Factory.Load<GenApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, transaction.PK));
				AssertEquals(1, approvalRequestFortransaction.Length);
				AssertEquals(Core.Constants.GenApprovalRequestApprovalStatus.Rejected, approvalRequestFortransaction[0].XP_ApprovalStatus);

				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed);
				Assert(!Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed);

				using (AccountingConfigurationRegistry.Instance.ReceivableReversalAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting()))
				{
					using (var reverseForm = Controller.ShowDeleteForm(transaction) as ZForm)
					{
						AssertEquals("Should show login form", typeof(LoginFormWithRequest), ZFormModaliser.LastFormShownDialogForTest.GetType());
					}
				}
			}
			else
			{
				Assert("Only applicable when reversing AR invoice or AR adjustment note", true);
			}
		}

		protected AuthorizationModeAndSettings GetAuthorisationConfigSetting()
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

		void CreateApprovalRequest(InvoicingBase parent, ZString approvalStatus)
		{
			var approvalRequest = Factory.New<ARCreditNoteApprovalRequest>();
			approvalRequest.ChangeApprovalTypeForInvoiceReversal();
			approvalRequest.Initialize(new[] { parent }, parent.PK, AccTransactionHeaderSchema.Constants.Prefix, JobInvoicingPostingOption.Revenue);
			approvalRequest.XP_ApprovalStatus = approvalStatus;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_cached ?? (TestObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_cached;

		#endregion

		public void TestGetIDForFormCache()
		{
			BusinessObject formBusinessEntity = GetFormBusinessEntity();
			AccountingTransactionController.Reversing_ForTestOnly = null;
			AssertEquals("Should be controller ID", AccountingTransactionController.ID.ToString(), AccountingTransactionController.GetIDForFormCache_ForTestOnly(formBusinessEntity));

			AccountingTransactionController.Reversing_ForTestOnly = new ReversingFactory().NewReversing((IReversing)formBusinessEntity);
			AssertEquals("Should be controller ID + Reversing", AccountingTransactionController.ID.ToString() + "Reversing", AccountingTransactionController.GetIDForFormCache_ForTestOnly(formBusinessEntity));
		}

		public void TestGetFormBusinessEntity()
		{
			BusinessObject formBusinessEntity = GetFormBusinessEntity();
			using (ZForm testForm = AccountingTransactionController.GetForm_ForTestOnly(formBusinessEntity) as ZForm)
			{
				AssertEquals("IsReverseTransaction", formBusinessEntity, testForm.BusinessEntity);
			}

			ReversingFactory revFactory = new ReversingFactory();
			AccountingTransactionController.Reversing_ForTestOnly = revFactory.NewReversing((IReversing)formBusinessEntity);
			if (AccountingTransactionController.Reversing_ForTestOnly != null)
			{
				AccountingTransactionController.Reversing_ForTestOnly.Reverse();
				using (ZForm testForm = AccountingTransactionController.GetForm_ForTestOnly(formBusinessEntity) as ZForm)
				{
					if (ShouldHaveReversedBizoForTest)
					{
						AssertEquals("IsReverseTransaction", ((IReversing)formBusinessEntity).ReverseTransaction, testForm.BusinessEntity);
					}
					else
					{
						AssertEquals("IsReverseTransaction", formBusinessEntity, testForm.BusinessEntity);
					}
				}
			}
			else
			{
				using (ZForm testForm = AccountingTransactionController.GetForm_ForTestOnly(formBusinessEntity) as ZForm)
				{
					AssertEquals("IsReverseTransaction", formBusinessEntity, testForm.BusinessEntity);
				}
			}
		}

		public void TestGetTopLevelBusinessObjectTypeCaching()
		{
			IBusiness originalTransaction = AccountingTransactionController.GetTopLevelBusinessObjectCached_ForTestOnly(ParentTransactionHeaderRow);

			Assert("Factories of source entity and business object returned by subclassed controller should be different",
				originalTransaction.Factory != ParentTransactionHeaderRow.Factory);

			Assert("Factory of business object returned by subclassed controller should be same as controller's factory",
				originalTransaction.Factory == AccountingTransactionController.Factory);

			AssertEquals("Type of transaction returned should be same as type of top level businessobject on controller",
				AccountingTransactionController.TypeOfTopLevelBusinessObject, originalTransaction.GetType());

			IBusiness cachedTransaction = AccountingTransactionController.GetTopLevelBusinessObjectCached_ForTestOnly(ParentTransactionHeaderRow);
			AssertEquals("Original Transaction and cached transactions should be the same objects", originalTransaction, cachedTransaction);
		}

		public void TestShowEditFormShowsEditForm()
		{
			AccountingTransactionController.ShowEditForm(ParentTransactionHeaderRow);
			AssertEquals("Form should be writable", ZArchitecture.Core.ODisplayMode.Browse, AccountingTransactionController.LastShownForm.DisplayMode);
		}

		public override void TestDeleteForm()
		{
			// DeleteForm is tested on TestShowDeleteFormDeleteMode, TestDoReversingBeforeShowDeleteForm and TestDoBadDebtWrittingOffBeforeShowDeleteForm
			Assert(true);
		}

		public void TestShowDeleteFormDeleteMode()
		{
			try
			{
				Controller.ShowDeleteForm(ParentTransactionHeaderRow);
				if (Controller.LastShownForm != null)
				{
					AssertEquals(ODisplayMode.Delete, Controller.LastShownForm.DisplayMode);
				}
			}
			finally
			{
				if (Controller != null && Controller.LastShownForm != null)
				{
					((Form)Controller.LastShownForm).Close();
				}
				else
				{
					if (Controller == null)
					{
						Fail("Controller was null");
					}
					else if (Controller.LastShownForm == null && DeleteShouldShowForm)
					{
						Fail("Controller's Last Shown Form was null");
					}
					else
					{
						Assert(true);
					}
				}
			}
		}

		public void TestDoReversingBeforeShowDeleteForm()
		{
			try
			{
				Controller.ShowDeleteForm(ParentTransactionHeaderRow);
				if (Controller.LastShownForm != null)
				{
					IReversing reversingBizo = AccountingTransactionController.GetLoadedBusinessEntityInLocalFactory_ForTestOnly(ParentTransactionHeaderRow) as IReversing;
					AssertNotNull(reversingBizo);
					IReversing reversedReversingBizo = reversingBizo.ReverseTransaction;
					AssertEquals("It should be reversed transaction.", ShouldHaveReversedBizoForTest, reversedReversingBizo != null);
					AssertEquals("It should be in reversing", true, reversingBizo.IsReversing);
					AssertEquals("It should have reversed state", true, reversingBizo.IsReversed);
				}
			}
			finally
			{
				if (Controller != null && Controller.LastShownForm != null)
				{
					((Form)Controller.LastShownForm).Close();
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestDoBadDebtWrittingOffBeforeShowDeleteForm()
		{
			IBadDebtWritingOff badDebtWritingOffBizo = ParentTransactionHeaderRow as IBadDebtWritingOff;
			if (badDebtWritingOffBizo == null)
			{
				Assert(true);
				return;
			}

			try
			{
				badDebtWritingOffBizo.IsWritingOff = true;
				Controller.ShowDeleteForm(ParentTransactionHeaderRow);
				if (Controller.LastShownForm != null)
				{
					badDebtWritingOffBizo = AccountingTransactionController.GetLoadedBusinessEntityInLocalFactory_ForTestOnly(ParentTransactionHeaderRow) as IBadDebtWritingOff;
					AssertNotNull(badDebtWritingOffBizo);
					IBadDebtWritingOff reversedBadDebtWritingOffBizo = badDebtWritingOffBizo.ReverseTransaction as IBadDebtWritingOff;
					AssertNotNull(reversedBadDebtWritingOffBizo);
					AssertEquals("It should be in writinig off", true, reversedBadDebtWritingOffBizo.IsWritingOff);
					AssertEquals("It should be in writinig off", false, reversedBadDebtWritingOffBizo.IsReversing);
					AssertEquals("It should have reversed state", true, badDebtWritingOffBizo.IsReversing);
					AssertEquals("It should not have reversed state", false, badDebtWritingOffBizo.IsReversed);
				}
			}
			finally
			{
				if (Controller != null && Controller.LastShownForm != null)
				{
					((Form)Controller.LastShownForm).Close();
				}
				else
				{
					Assert(true);
				}
			}
		}

		public void TestMultipleReversing()
		{
			BusinessObject formBusinessEntity = ParentTransactionHeaderRow;
			if (DeleteShouldShowForm)
			{
				IReversing formBusinessEntityAsIReversing = formBusinessEntity as IReversing;
				var multipleReversingProvider = new MultipleReversingProviderForHeader();
				multipleReversingProvider.BizObjectsForReversing.Add(formBusinessEntity);
				AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				Assert("Postcondition: reversed object must be added.", multipleReversingProvider.TransactionsAlreadyReversed.Count > 0);

				if (ShouldHaveReversedBizoForTest)
				{
					Assert("Reverse transaction must created.", multipleReversingProvider.TransactionsAlreadyReversed[0].IsReverseTransaction);
				}
				Assert("Reversed object must created.", multipleReversingProvider.TransactionsAlreadyReversed[0].IsReversed);
				Assert("Reversed object must not have an MultipleReversingErrors.", multipleReversingProvider.TransactionsAlreadyReversed[0].MultipleReversingErrors.Length == 0);
				Assert("Reversed object must have not an validation error.", !multipleReversingProvider.TransactionsAlreadyReversed[0].HasErrors());
				AssertType("For correct binding reversing object must be wrapped.", typeof(IReversingImplicitlyImplementedWrapperForBinding),
					multipleReversingProvider.TransactionsAlreadyReversed[0]);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestMultipleReversingValidation()
		{
			BusinessObject formBusinessEntity = ParentTransactionHeaderRow;
			if (DeleteShouldShowForm)
			{
				IReversing formBusinessEntityAsIReversing = formBusinessEntity as IReversing;
				var multipleReversingProvider = new MultipleReversingProviderForHeader();
				multipleReversingProvider.BizObjectsForReversing.Add(formBusinessEntity);
				AccountingTransactionController.ReversingErrors_ForTestOnly.Add("error");
				AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				Assert("Postcondition: reversed object must be added.", multipleReversingProvider.TransactionsAlreadyReversed.Count > 0);

				Assert("Reversed object must have an MultipleReversingErrors.", multipleReversingProvider.TransactionsAlreadyReversed[0].MultipleReversingErrors.Length > 0);
				Assert("Reversed object must have an row validation error.",
					((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity).HasRowErrors);
				Assert("Reversed object must have an row validation error.",
					multipleReversingProvider.TransactionsAlreadyReversed[0].NotificationsIncludingChildren.ContainsNotificationContaining("error"));
				multipleReversingProvider.TransactionsAlreadyReversed[0].RunPreSaveValidation();
				Assert("Reversed object must still have an row validation error after Validation was run.",
					((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity).HasRowErrors);
				Assert("Reversed object must still have an row validation error after Validation was run.",
					multipleReversingProvider.TransactionsAlreadyReversed[0].NotificationsIncludingChildren.ContainsNotificationContaining("error"));

				multipleReversingProvider.TransactionsAlreadyReversed[0].MultipleReversingErrors = null;
				if (multipleReversingProvider.TransactionsAlreadyReversed[0].TransactionNumber.IsEmpty)
				{
					multipleReversingProvider.TransactionsAlreadyReversed[0].TransactionNumber = "000001";
				}
				multipleReversingProvider.TransactionsAlreadyReversed[0].Factory.Save();

				((Form)Controller.LastShownForm).Close();

				Controller = ZControllerFactory.Create(GetControllerID());
				multipleReversingProvider = new MultipleReversingProviderForHeader();
				multipleReversingProvider.BizObjectsForReversing.Add(formBusinessEntity);
				AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				Assert("Postcondition: reversed object must be added.", multipleReversingProvider.TransactionsAlreadyReversed.Count > 0);

				Assert("Reversed object must have an MultipleReversingErrors as already reversed.", multipleReversingProvider.TransactionsAlreadyReversed[0].MultipleReversingErrors.Length > 0);
				Assert("Reversed object must have an row validation error as already reversed.",
					((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity).HasRowErrors);
				multipleReversingProvider.TransactionsAlreadyReversed[0].RunPreSaveValidation();
				Assert("Reversed object must still have an row validation error after Validation was run.",
					((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity).HasRowErrors);
				AssertNull("There are no messages must be shown during multireversing.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestMultipleReversingSecurityErrorAreShownAsValidationError()
		{
			var formBusinessEntity = ParentTransactionHeaderRow;
			if (DeleteShouldShowForm)
			{
				var formBusinessEntityAsIReversing = formBusinessEntity as IReversing;
				var multipleReversingProvider = new MultipleReversingProviderForHeader();
				var deleteCheckPoint = AccountingTransactionController.GetCheckPointForDelete(formBusinessEntity);
				deleteCheckPoint.IsAllowed = false;
				multipleReversingProvider.BizObjectsForReversing.Add(formBusinessEntity);
				AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				Assert("Postcondition: reversed object must be added.", multipleReversingProvider.TransactionsAlreadyReversed.Count > 0);

				Assert("Reversed object must have security error as MultipleReversingErrors.", multipleReversingProvider.TransactionsAlreadyReversed[0].MultipleReversingErrors.Length > 0);
				Assert("Reversed object must have security error as row validation error.",
					((BusinessObject)multipleReversingProvider.TransactionsAlreadyReversed[0].WrappedBusinessEntity).HasRowErrors);
				Assert("Reversed object must have security error as row validation error.",
					multipleReversingProvider.TransactionsAlreadyReversed[0].NotificationsIncludingChildren.ContainsNotificationContaining(deleteCheckPoint.ErrorMessageForNotAllowed));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestShowDeleteFormForMultipleReversing()
		{
			try
			{
				BusinessObject formBusinessEntity = ParentTransactionHeaderRow;
				IReversing formBusinessEntityAsIReversing = formBusinessEntity as IReversing;
				var multipleReversingProvider = new MultipleReversingProviderForHeader();
				multipleReversingProvider.BizObjectsForReversing.Add(formBusinessEntity);
				AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				if (Controller.LastShownForm != null)
				{
					AssertType(typeof(MultipleReversingForHeaderForm), Controller.LastShownForm);
					AssertEquals(ODisplayMode.Delete, Controller.LastShownForm.DisplayMode);
				}
			}
			finally
			{
				if (Controller != null && Controller.LastShownForm != null)
				{
					((Form)Controller.LastShownForm).Close();
				}
				else
				{
					if (Controller == null)
					{
						Fail("Controller was null");
					}
					else if (Controller.LastShownForm == null && DeleteShouldShowForm)
					{
						Fail("Controller's Last Shown Form was null");
					}
					else
					{
						Assert(true);
					}
				}
			}
		}

		public void TestShowDeleteFormForMultipleReversingTransactionsWhenMultipleReversingProviderIsNull()
		{
			var formBusinessEntity = ParentTransactionHeaderRow;
			var multipleReversingProvider = new MultipleReversingProviderForHeader();
			multipleReversingProvider.BizObjectsForReversing.Add(formBusinessEntity);

			var message = @"When reversing multiple transactions, the MultipleReversingProvider shouldn't be null.
This method should be called using controller's DeleteMultiple() method.";

			AssertExceptionThrown<ArgumentException>(message, () => Controller.ShowDeleteForm(multipleReversingProvider));
		}

		public void TestControllerIDForMultipleReversing()
		{
			BusinessObject formBusinessEntity = ParentTransactionHeaderRow;
			if (DeleteShouldShowForm)
			{
				IReversing formBusinessEntityAsIReversing = formBusinessEntity as IReversing;
				var multipleReversingProvider = new MultipleReversingProviderForHeader();
				multipleReversingProvider.BizObjectsForReversing.Add(formBusinessEntity);
				AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				AssertEquals("ControllerID must be the same for all controllers during multiple reversing to avoid showing several MultipleReversingForm forms.",
					 ControllerIDs.APInvoice, AccountingTransactionController.ID);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestFactoryForMultipleReversing()
		{
			BusinessObject formBusinessEntity = ParentTransactionHeaderRow;
			if (DeleteShouldShowForm)
			{
				IReversing formBusinessEntityAsIReversing = formBusinessEntity as IReversing;
				var multipleReversingProvider = new MultipleReversingProviderForHeader();
				multipleReversingProvider.BizObjectsForReversing.Add(formBusinessEntity);
				AccountingTransactionController.DeleteMultiple(new BusinessObject[] { multipleReversingProvider });
				AssertEquals("Controller Factory must be the same for all controllers during multiple reversing ahd must get from MultipleReversingProvider.",
					 multipleReversingProvider.Factory, AccountingTransactionController.Factory);
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestGetCheckPointForDelete()
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			AssertEquals("Should return CheckPointForDelete_ForTestOnly", AccountingTransactionController.CheckPointForDelete_ForTestOnly, AccountingTransactionController.GetCheckPointForDelete(invoice));
			AssertEquals("No CheckPoint for multiple reversing bizo", Env.Security.None, AccountingTransactionController.GetCheckPointForDelete(new MultipleReversingProviderForHeader()));
		}

		public void TestDeleteMultipleIfMultipleReversingIsNotSupported()
		{
			BusinessObject formBusinessEntity = ParentTransactionHeaderRow;
			if (DeleteShouldShowForm)
			{
				AccountingTransactionController.DeleteMultiple(new BusinessObject[] { formBusinessEntity });
				AssertEquals("A massage must be shown.", "The action for multiple objects is not implemented in this version.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSecurityCheckPoints()
		{
			string message = "{0}. Please check that you have overridden Expected{0} in your test class.";
			AssertEquals(string.Format(message, "CheckPointForView"), ExpectedCheckPointForView, AccountingTransactionController.GetCheckPointForView(ParentTransactionHeaderRow));
			AssertEquals(string.Format(message, "CheckPointForNew"), ExpectedCheckPointForNew, AccountingTransactionController.GetCheckPointForNew(ParentTransactionHeaderRow));
			AssertEquals(string.Format(message, "CheckPointForEdit"), ExpectedCheckPointForEdit, AccountingTransactionController.GetCheckPointForEdit(ParentTransactionHeaderRow));
			AssertEquals(string.Format(message, "CheckPointForDelete"), ExpectedCheckPointForDelete, AccountingTransactionController.GetCheckPointForDelete(ParentTransactionHeaderRow));
		}

		[ExpectNoExceptions]
		public void TestNotThrowExceptionWhenBizoNotSaved()
		{
			var tableName = BusinessObjectFactory.GetTableNameFromType(AccountingTransactionController.TypeOfTopLevelBusinessObject, false);
			if (string.IsNullOrEmpty(tableName))
			{
				AccountingTransactionController.ShowEditForm(ParentTransactionHeaderRowNotSaved);
			}
			else
			{
				AccountingTransactionController.ShowEditForm(new BusinessObjectFactory().New(AccountingTransactionController.TypeOfTopLevelBusinessObject));
			}
		}

		public void TestGetTopLevelBusinessObjectCachedWithoutFactoryLoad()
		{
			var settings = AccountingTransactionController as IBusinessEntityFactorySettings;
			if (settings != null)
			{
				AccountingTransactionController.GetTopLevelBusinessObjectCounter = 0;
				settings.ShouldUseSourceEntityFactory = true;

				AssertNotEquals("LatestSourceEntityIdentifier should be different than source entity PK",
					ParentTransactionHeaderRow.PK, AccountingTransactionController.LatestSourceEntityIdentifier_ForTestOnly);

				var transaction = AccountingTransactionController.GetTopLevelBusinessObjectCached_ForTestOnly(ParentTransactionHeaderRow);

				AssertEquals("Factory of business object returned by subclassed controller should be same as source entity factory",
					ParentTransactionHeaderRow.Factory, transaction.Factory);

				AssertNotEquals("Factory of business object returned by subclassed controller should be differnet than controller's factory",
					AccountingTransactionController.Factory, transaction.Factory);

				AssertEquals("Type of transaction returned should be same as type of top level businessobject on controller",
					AccountingTransactionController.TypeOfTopLevelBusinessObject, transaction.GetType());

				AssertEquals(1, AccountingTransactionController.GetTopLevelBusinessObjectCounter);
				var cachedTransaction = AccountingTransactionController.GetTopLevelBusinessObjectCached_ForTestOnly(ParentTransactionHeaderRow);

				AssertEquals("2nd call should not use cached transaction",
					2, AccountingTransactionController.GetTopLevelBusinessObjectCounter);

				AccountingTransactionController.GetTopLevelBusinessObjectCounter = 0;
				settings.ShouldUseSourceEntityFactory = false;

				AccountingTransactionController.LatestSourceEntityIdentifier_ForTestOnly = ZGuid.Empty;
				AssertNotEquals("LatestSourceEntityIdentifier should be different than source entity PK",
					ParentTransactionHeaderRow.PK, AccountingTransactionController.LatestSourceEntityIdentifier_ForTestOnly);

				transaction = AccountingTransactionController.GetTopLevelBusinessObjectCached_ForTestOnly(ParentTransactionHeaderRow);

				AssertNotEquals("Factory of business object returned by subclassed controller should be different than source entity factory",
					ParentTransactionHeaderRow.Factory, transaction.Factory);

				AssertEquals("Factory of business object returned by subclassed controller should be same as controller's factory",
					AccountingTransactionController.Factory, transaction.Factory);

				AssertEquals("Type of transaction returned should be same as type of top level businessobject on controller",
					AccountingTransactionController.TypeOfTopLevelBusinessObject, transaction.GetType());

				AssertEquals(1, AccountingTransactionController.GetTopLevelBusinessObjectCounter);
				cachedTransaction = AccountingTransactionController.GetTopLevelBusinessObjectCached_ForTestOnly(ParentTransactionHeaderRow);

				AssertEquals("2nd call should use cached transaction",
					1, AccountingTransactionController.GetTopLevelBusinessObjectCounter);
			}
			else
			{
				AccountingTransactionController.GetTopLevelBusinessObjectCounter = 0;

				AssertNotEquals("LatestSourceEntityIdentifier should be different than source entity PK",
					ParentTransactionHeaderRow.PK, AccountingTransactionController.LatestSourceEntityIdentifier_ForTestOnly);

				var transaction = AccountingTransactionController.GetTopLevelBusinessObjectCached_ForTestOnly(ParentTransactionHeaderRow);

				AssertNotEquals("Factory of business object returned by subclassed controller should be different than source entity factory",
					ParentTransactionHeaderRow.Factory, transaction.Factory);

				AssertEquals("Factory of business object returned by subclassed controller should be same as controller's factory",
					AccountingTransactionController.Factory, transaction.Factory);

				AssertEquals("Type of transaction returned should be same as type of top level businessobject on controller",
					AccountingTransactionController.TypeOfTopLevelBusinessObject, transaction.GetType());

				AssertEquals(1, AccountingTransactionController.GetTopLevelBusinessObjectCounter);
				var cachedTransaction = AccountingTransactionController.GetTopLevelBusinessObjectCached_ForTestOnly(ParentTransactionHeaderRow);

				AssertEquals("2nd call should use cached transaction",
					1, AccountingTransactionController.GetTopLevelBusinessObjectCounter);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());

			periodHelper.SetupPeriods();
		}

		protected AccountingTransactionController AccountingTransactionController
		{
			get { return (AccountingTransactionController)Controller; }
		}

		protected UnitTestUserNotification UnitTestNotification
		{
			get { return (UnitTestUserNotification)Globals.Message; }
		}

		protected sealed override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return ParentTransactionHeaderRow;
		}

		protected virtual BusinessObject GetFormBusinessEntity()
		{
			return ParentTransactionHeaderRow;
		}

		protected virtual BusinessObject ParentTransactionHeaderRowNotSaved { get; }

		protected virtual bool ShouldHaveReversedBizoForTest
		{
			get { return true; }
		}

		protected virtual bool DeleteShouldShowForm
		{
			get { return true; }
		}

		protected virtual SecurityCheckpoint ExpectedCheckPointForView
		{
			get { return Env.Security.None; }
		}

		protected virtual SecurityCheckpoint ExpectedCheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected virtual SecurityCheckpoint ExpectedCheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected virtual SecurityCheckpoint ExpectedCheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
