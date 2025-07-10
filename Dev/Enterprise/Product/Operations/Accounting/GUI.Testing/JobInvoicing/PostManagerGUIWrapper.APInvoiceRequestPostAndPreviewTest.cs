using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	class PostManagerGUIWrapper_APInvoiceRequestPostAndPreviewTest : TestCaseWithFactory
	{
		public void TestGetInvoiceToPreviewRequestCostConfirmationDocument_JobRelated_UserWithRights()
		{
			SetupSecurityRight(true);
			AssertPreview(isConsolRelated: false);
		}

		public void TestGetInvoiceToPreviewRequestCostConfirmationDocument_ConsolRelated_UserWithRights()
		{
			SetupSecurityRight(true);
			AssertPreview(isConsolRelated: true);
		}

		public void TestPostRequest_JobRelated_UserWithRights()
		{
			SetupSecurityRight(true);
			AssertPost(isConsolRelated: false);
		}

		public void TestPostRequest_ConsolRelated_UserWithRights()
		{
			SetupSecurityRight(true);
			AssertPost(isConsolRelated: true);
		}

		public void TestGetInvoiceToPreviewRequestCostConfirmationDocument_JobRelated()
		{
			using (Env.SetTemporaryUserContext(LoginUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SetupSecurityRight(false);
				AssertPreview(isConsolRelated: false);
			}
		}

		public void TestGetInvoiceToPreviewRequestCostConfirmationDocument_ConsolRelated()
		{
			using (Env.SetTemporaryUserContext(LoginUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SetupSecurityRight(false);
				AssertPreview(isConsolRelated: true);
			}
		}

		public void TestPostRequest_JobRelated()
		{
			using (Env.SetTemporaryUserContext(LoginUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SetupSecurityRight(false);
				AssertPost(isConsolRelated: false);
			}
		}

		public void TestPostRequest_ConsolRelated()
		{
			using (Env.SetTemporaryUserContext(LoginUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				SetupSecurityRight(false);
				AssertPost(isConsolRelated: true);
			}
		}

		void AssertPreview(bool isConsolRelated)
		{
			ForwardingConsol consol = null;
			if (isConsolRelated)
			{
				consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			}

			var shipment = TestObjectCreator.CreateShipment("S001", "", "", consol);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, null, 0);
			Factory.Save();

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			request.XP_ParentTableCode = "AH";
			var expectedErrorMessage = "Request reference type 'Transaction' is not supported.";
			var result = PostManagerGUIWrapper.GetInvoiceToPreviewRequestCostConfirmationDocument(request);
			AssertNull("InvoiceToPreview", result.InvoiceToPreview);
			AssertEquals("ErrorMessage", expectedErrorMessage, result.ErrorMessage);
			AssertNoWindowsShown();

			var invoiceNumber = "INV2";
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, invoiceNumber, null, 0, null);
			charge.JR_APInvoiceDate = ZDateTime.Now;
			if (isConsolRelated)
			{
				var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);
				consolCost.E6_InvoiceNum = charge.JR_APInvoiceNum;
				consolCost.E6_InvoiceDate = charge.JR_APInvoiceDate;
			}
			var invoiceCharges = new APInvoiceCharges(charge.CostAccount.OH_Code, charge.JR_APInvoiceNum, ZGuid.Empty, "", null);
			invoiceCharges.Charges.Add(charge);
			var tablePrefix = consol != null ? consol.TablePrefix : job.TablePrefix;
			request.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), tablePrefix);
			expectedErrorMessage = (consol != null ? "Consol" : "Job") + " was not found.";
			result = PostManagerGUIWrapper.GetInvoiceToPreviewRequestCostConfirmationDocument(request);
			AssertNull("InvoiceToPreview", result.InvoiceToPreview);
			AssertEquals("ErrorMessage", expectedErrorMessage, result.ErrorMessage);
			AssertNoWindowsShown();

			request.InitializeJobRelated(invoiceCharges, consol != null ? consol.PK : job.PK, tablePrefix);
			expectedErrorMessage = "Please save job S001 before posting costs and/or charges.\n";
			result = PostManagerGUIWrapper.GetInvoiceToPreviewRequestCostConfirmationDocument(request);
			AssertNull("InvoiceToPreview", result.InvoiceToPreview);
			AssertEquals("ErrorMessage", expectedErrorMessage, result.ErrorMessage);
			AssertNoWindowsShown();

			Factory.Save();
			if (isConsolRelated)
			{
				charge.ParentConsolCost.E6_InvoiceNum += "_1";
			}
			else
			{
				charge.JR_APInvoiceNum += "_1";
			}
			Factory.Save();

			result = PostManagerGUIWrapper.GetInvoiceToPreviewRequestCostConfirmationDocument(request);
			AssertNull("InvoiceToPreview", result.InvoiceToPreview);
			expectedErrorMessage = "No preview available as there are no charges valid for posting with the request Creditor and Invoice Number.";
			AssertEquals("ErrorMessage", expectedErrorMessage, result.ErrorMessage);
			AssertNoWindowsShown();

			if (isConsolRelated)
			{
				charge.ParentConsolCost.E6_InvoiceNum = invoiceNumber;
				charge.ParentConsolCost.E6_OSCostAmount *= -1;
			}
			else
			{
				charge.JR_APInvoiceNum = invoiceNumber;
				charge.JR_OSCostAmt *= -1;
			}
			Factory.Save();
			result = PostManagerGUIWrapper.GetInvoiceToPreviewRequestCostConfirmationDocument(request);
			AssertNull("InvoiceToPreview", result.InvoiceToPreview);
			AssertEquals("ErrorMessage", expectedErrorMessage, result.ErrorMessage);
			AssertNoWindowsShown();

			if (isConsolRelated)
			{
				charge.ParentConsolCost.E6_OSCostAmount = 150;
			}
			else
			{
				charge.JR_OSCostAmt = 150;
			}
			Factory.Save();
			result = PostManagerGUIWrapper.GetInvoiceToPreviewRequestCostConfirmationDocument(request);
			AssertNull("InvoiceToPreview", result.InvoiceToPreview);
			AssertEquals("ErrorMessage", "Can't preview this request because source details have been modified since then.", result.ErrorMessage);
			AssertNoWindowsShown();

			if (isConsolRelated)
			{
				charge.ParentConsolCost.E6_OSCostAmount = 100;
			}
			else
			{
				charge.JR_OSCostAmt = 100;
			}
			Factory.Save();
			result = PostManagerGUIWrapper.GetInvoiceToPreviewRequestCostConfirmationDocument(request);
			AssertEquals("ErrorMessage", "", result.ErrorMessage);
			AssertNotNull("InvoiceToPreview", result.InvoiceToPreview);
			Assert("IsInDatabase", !result.InvoiceToPreview.IsInDatabase);
			Assert("HasContext", result.InvoiceToPreview.HasContext(BusinessContext.UnapprovedAPInvoiceCreatedForRequestPreview));
			AssertEquals("AH_OH", TestObjectCreator.Creditor1.PK, result.InvoiceToPreview.AH_OH);
			AssertEquals("AH_TransactionNum", "INV2", result.InvoiceToPreview.AH_TransactionNum);
			AssertEquals("AH_OSExTaxAmount", 100m, result.InvoiceToPreview.AH_OSExTaxAmount);
			Assert("IsCostPosted", !charge.IsCostPosted);
			AssertNoWindowsShown();
		}

		void AssertPost(bool isConsolRelated)
		{
			ForwardingConsol consol = null;
			if (isConsolRelated)
			{
				consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001");
			}

			var shipment = TestObjectCreator.CreateShipment("S001", "", "", consol);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, null, 0);
			Factory.Save();

			var request = Factory.New<APInvoiceChargesApprovalRequest>();
			request.XP_ParentTableCode = "AH";
			var expectedErrorMessage = "Request reference type 'Transaction' is not supported.";
			var errorMessage = PostManagerGUIWrapper.PostRequest(request);
			AssertEquals("ErrorMessage", expectedErrorMessage, errorMessage);
			AssertNoWindowsShown();

			var invoiceNumber = "INV2";
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "", null, 100, TestObjectCreator.Creditor1, invoiceNumber, null, 0, null);
			charge.JR_APInvoiceDate = ZDateTime.Now;
			if (isConsolRelated)
			{
				var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 100, TestObjectCreator.Creditor1);
				consolCost.E6_InvoiceNum = charge.JR_APInvoiceNum;
				consolCost.E6_InvoiceDate = charge.JR_APInvoiceDate;
			}
			var invoiceCharges = new APInvoiceCharges(charge.CostAccount.OH_Code, charge.JR_APInvoiceNum, ZGuid.Empty, "", null);
			invoiceCharges.Charges.Add(charge);
			var tablePrefix = consol != null ? consol.TablePrefix : job.TablePrefix;
			request.InitializeJobRelated(invoiceCharges, ZGuid.NewZGuid(), tablePrefix);
			expectedErrorMessage = (consol != null ? "Consol" : "Job") + " was not found.";
			errorMessage = PostManagerGUIWrapper.PostRequest(request);
			AssertEquals("ErrorMessage", expectedErrorMessage, errorMessage);
			AssertNoWindowsShown();

			request.InitializeJobRelated(invoiceCharges, consol != null ? consol.PK : job.PK, tablePrefix);
			expectedErrorMessage = "Please save job S001 before posting costs and/or charges.\n";
			errorMessage = PostManagerGUIWrapper.PostRequest(request);
			AssertEquals("ErrorMessage", "", errorMessage);
			AssertNoWindowsShown(expectedErrorMessage);

			Factory.Save();
			if (isConsolRelated)
			{
				charge.ParentConsolCost.E6_InvoiceNum += "_1";
			}
			else
			{
				charge.JR_APInvoiceNum += "_1";
			}
			Factory.Save();

			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			Factory.Save();

			errorMessage = PostManagerGUIWrapper.PostRequest(request);
			AssertEquals("ErrorMessage", "", errorMessage);
			expectedErrorMessage = "Nothing to post as there are no charges valid for posting with the request Creditor and Invoice Number.";
			AssertNoWindowsShown(expectedErrorMessage);

			if (isConsolRelated)
			{
				charge.ParentConsolCost.E6_InvoiceNum = invoiceNumber;
				charge.ParentConsolCost.E6_OSCostAmount *= -1;
			}
			else
			{
				charge.JR_APInvoiceNum = invoiceNumber;
				charge.JR_OSCostAmt *= -1;
			}
			Factory.Save();
			errorMessage = PostManagerGUIWrapper.PostRequest(request);
			AssertEquals("ErrorMessage", "", errorMessage);
			AssertNoWindowsShown(expectedErrorMessage);

			if (isConsolRelated)
			{
				charge.ParentConsolCost.E6_OSCostAmount = 150;
			}
			else
			{
				charge.JR_OSCostAmt = 150;
			}
			Factory.Save();
			errorMessage = PostManagerGUIWrapper.PostRequest(request);
			AssertEquals("ErrorMessage", "", errorMessage);
			AssertNoWindowsShown("Can't post this request because source details have been modified since then.");

			if (isConsolRelated)
			{
				charge.ParentConsolCost.E6_OSCostAmount = 100;
			}
			else
			{
				charge.JR_OSCostAmt = 100;
			}
			Factory.Save();
			errorMessage = PostManagerGUIWrapper.PostRequest(request);
			AssertEquals("ErrorMessage", "", errorMessage);
			Assert("IsCostPosted", charge.IsCostPosted);
			AssertEquals("ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Posted, request.XP_ApprovalStatus);
			AssertNoWindowsShown();
		}

		static void SetupSecurityRight(bool hasRight)
		{
			Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = hasRight;
			AssertEquals("Precondition: User right", hasRight, new UnapprovedTransactionValidationHelper().GetSecurityCheckPoint(1).IsAllowed);
		}

		static void AssertNoWindowsShown(string expectedMessage = null)
		{
			AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("PreviousMessages", expectedMessage != null ? 1 : 0, UnitTestUserNotification.Instance.PreviousMessages.Count(x => x.Text != null));
			UnitTestUserNotification.Instance.ClearMessages();

			AssertNull("LastFormShownDialogForTest", ZFormModaliser.LastFormShownDialogForTest);
			AssertNull("LastFormShownForTest", ZFormModaliser.LastFormShownForTest);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		string LoginUserName { get { return "User1"; } }

		protected override void SetUp()
		{
			base.SetUp();

			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 0;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));

			SecurityTestObject.CreateTestUser(true, Env.Security.Maintain.Code, "USR", LoginUserName, "pass");
		}
	}
}
