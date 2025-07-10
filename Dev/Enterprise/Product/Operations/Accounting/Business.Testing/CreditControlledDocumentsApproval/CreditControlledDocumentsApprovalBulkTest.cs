using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using NUnit.Framework;
using AccountingBusiness = Enterprise.Accounting.Business;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CreditControlledDocumentsApprovalBulk))]
	public class CreditControlledDocumentsApprovalBulkTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CreditControlledDocumentsApprovalBulk(Factory);
		}

		public void TestSingleLevelAuthorizationIsSetWhileApproving_RequestsThatAllowMoreThanOneLevelAuthorization()
		{
			Env.Security.OnCreditHoldControllerFirstLevel.IsAllowed = false;
			Env.Security.OnCreditHoldControllerSecondLevel.IsAllowed = true;
			Env.Security.OnCreditHoldControllerThirdLevel.IsAllowed = false;

			var request = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request.XP_PrivledgeRequired = "1, 2, 3";

			var bulk = new CreditControlledDocumentsApprovalBulk(Factory, request);

			var result = bulk.ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved);
			Assert("Approval request status is changed successfully", result);
			AssertEquals("Privilege required is updated", "2", request.XP_PrivledgeRequired);
		}

		public void TestChangeStatusSecurity()
		{
			//user does not have any approval right
			Env.Security.OnCreditHoldControllerFirstLevel.IsAllowed = false;
			Env.Security.OnCreditHoldControllerSecondLevel.IsAllowed = false;
			Env.Security.OnCreditHoldControllerThirdLevel.IsAllowed = false;

			var request_1st = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_1st.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_1st.XP_PrivledgeRequired = "1";

			var request_2nd = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_2nd.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_2nd.XP_PrivledgeRequired = "2";

			var request_3rd = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_3rd.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_3rd.XP_PrivledgeRequired = "3";

			var bulk = new CreditControlledDocumentsApprovalBulk(Factory, request_1st, request_2nd, request_3rd);

			var result = bulk.ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved);
			Assert("No change of status was successful.", !result);

			AssertEquals("No approval to process", 0, bulk.CreditControlledDocumentsApprovalsAllowedToProcess.Count);

			//user only have 1st level approval right
			Env.Security.OnCreditHoldControllerFirstLevel.IsAllowed = true;
			Env.Security.OnCreditHoldControllerSecondLevel.IsAllowed = false;
			Env.Security.OnCreditHoldControllerThirdLevel.IsAllowed = false;

			request_1st = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_1st.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_1st.XP_PrivledgeRequired = "1";

			request_2nd = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_2nd.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_2nd.XP_PrivledgeRequired = "2";

			request_3rd = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_3rd.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_3rd.XP_PrivledgeRequired = "3";

			bulk = new CreditControlledDocumentsApprovalBulk(Factory, request_1st, request_2nd, request_3rd);

			result = bulk.ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved);
			Assert("At least one status update was successful.", result);

			AssertEquals("One approval to process", 1, bulk.CreditControlledDocumentsApprovalsAllowedToProcess.Count);
			AssertEquals("1", bulk.CreditControlledDocumentsApprovalsAllowedToProcess[0].XP_PrivledgeRequired);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, bulk.CreditControlledDocumentsApprovalsAllowedToProcess[0].XP_ApprovalStatus);

			//user only have 2nd level approval right
			Env.Security.OnCreditHoldControllerFirstLevel.IsAllowed = true;
			Env.Security.OnCreditHoldControllerSecondLevel.IsAllowed = true;
			Env.Security.OnCreditHoldControllerThirdLevel.IsAllowed = false;

			request_1st = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_1st.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_1st.XP_PrivledgeRequired = "1";

			request_2nd = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_2nd.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_2nd.XP_PrivledgeRequired = "2";

			request_3rd = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_3rd.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_3rd.XP_PrivledgeRequired = "3";

			bulk = new CreditControlledDocumentsApprovalBulk(Factory, request_1st, request_2nd, request_3rd);

			result = bulk.ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved);
			Assert("At least one status update was successful.", result);

			AssertEquals("Two approvals to process", 2, bulk.CreditControlledDocumentsApprovalsAllowedToProcess.Count);
			AssertEquals("1", bulk.CreditControlledDocumentsApprovalsAllowedToProcess[0].XP_PrivledgeRequired);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, bulk.CreditControlledDocumentsApprovalsAllowedToProcess[0].XP_ApprovalStatus);
			AssertEquals("2", bulk.CreditControlledDocumentsApprovalsAllowedToProcess[1].XP_PrivledgeRequired);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, bulk.CreditControlledDocumentsApprovalsAllowedToProcess[1].XP_ApprovalStatus);

			//user has 3rd level approval right
			Env.Security.OnCreditHoldControllerFirstLevel.IsAllowed = true;
			Env.Security.OnCreditHoldControllerSecondLevel.IsAllowed = true;
			Env.Security.OnCreditHoldControllerThirdLevel.IsAllowed = true;

			request_1st = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_1st.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_1st.XP_PrivledgeRequired = "1";

			request_2nd = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_2nd.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_2nd.XP_PrivledgeRequired = "2";

			request_3rd = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request_3rd.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request_3rd.XP_PrivledgeRequired = "3";

			bulk = new CreditControlledDocumentsApprovalBulk(Factory, request_1st, request_2nd, request_3rd);

			result = bulk.ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved);
			Assert("All request was successfule.", result);

			AssertEquals("All three approvals to process", 3, bulk.CreditControlledDocumentsApprovalsAllowedToProcess.Count);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, bulk.CreditControlledDocumentsApprovalsAllowedToProcess[0].XP_ApprovalStatus);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, bulk.CreditControlledDocumentsApprovalsAllowedToProcess[1].XP_ApprovalStatus);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, bulk.CreditControlledDocumentsApprovalsAllowedToProcess[2].XP_ApprovalStatus);
		}

		[TestDate(2014, 03, 05, 14, 42, 00)]
		public void TestChangeStatus()
		{
			Env.Security.OnCreditHoldControllerFirstLevel.IsAllowed = true;
			var request1 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			var request2 = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request1.XP_PrivledgeRequired = "1";
			request2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Cancelled;
			request2.XP_PrivledgeRequired = "1";

			var bulk = new CreditControlledDocumentsApprovalBulk(Factory, request1, request2);
			bool result = bulk.ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Approved);
			AssertEquals(true, result);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request1.XP_ApprovalStatus);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request2.XP_ApprovalStatus);
			AssertEquals(new ZDateTime(2014, 03, 05, 14, 42, 00), request1.XP_ApprovalDate);
			AssertEquals(new ZDateTime(2014, 03, 05, 14, 42, 00), request2.XP_ApprovalDate);
			AssertEquals("E", request1.XP_GS_NKApprovingUser1.ToString());
			AssertEquals("E", request2.XP_GS_NKApprovingUser1.ToString());
			result = bulk.ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled);
			AssertEquals(true, result);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request1.XP_ApprovalStatus);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Cancelled, request2.XP_ApprovalStatus);
			AssertEquals(ZDateTime.Empty, request1.XP_ApprovalDate);
			AssertEquals(ZDateTime.Empty, request2.XP_ApprovalDate);
			AssertEquals("", request1.XP_GS_NKApprovingUser1.ToString());
			AssertEquals("", request2.XP_GS_NKApprovingUser1.ToString());
			result = bulk.ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Rejected);
			AssertEquals(true, result);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Rejected, request1.XP_ApprovalStatus);
			AssertEquals(Constants.GenApprovalRequestApprovalStatus.Rejected, request2.XP_ApprovalStatus);
			AssertEquals(new ZDateTime(2014, 03, 05, 14, 42, 00), request1.XP_ApprovalDate);
			AssertEquals(new ZDateTime(2014, 03, 05, 14, 42, 00), request2.XP_ApprovalDate);
			AssertEquals("E", request1.XP_GS_NKApprovingUser1.ToString());
			AssertEquals("E", request2.XP_GS_NKApprovingUser1.ToString());
		}

		public void TestChangeStatusToApprovedWhenPriviledgeIsBlank()
		{
			AssertChangeStatusWhenPriviledgeIsBlank(Constants.GenApprovalRequestApprovalStatus.Approved);
		}

		public void TestChangeStatusToRejectededWhenPriviledgeIsBlank()
		{
			AssertChangeStatusWhenPriviledgeIsBlank(Constants.GenApprovalRequestApprovalStatus.Rejected);
		}

		public void TestChangeStatusToCancelledWhenPriviledgeIsBlank()
		{
			AssertChangeStatusWhenPriviledgeIsBlank(Constants.GenApprovalRequestApprovalStatus.Cancelled);
		}

		void AssertChangeStatusWhenPriviledgeIsBlank(string status)
		{
			SetSecurityPermissionsForHighestPermission(false);
			SetSecurityPermissionsForLowerLevelPermissions(true);

			var request = CreateApprovalRequest("", false);
			var bulk = new CreditControlledDocumentsApprovalBulk(Factory, request);
			var result = bulk.ChangeStatus(status);
			AssertEquals("Approval status is not updated", Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);

			SetSecurityPermissionsForHighestPermission(true);
			SetSecurityPermissionsForLowerLevelPermissions(false);

			result = bulk.ChangeStatus(status);
			Assert("ChangeStatus result", result);
			AssertEquals("Approval status is updated", status, request.XP_ApprovalStatus);
			AssertEquals("Privilege required is updated", "3", request.XP_PrivledgeRequired);
		}

		public void TestChangeStatusToCancelledWhenCreatedByCurrentUserWithoutRights()
		{
			AssertCancelledStatusWhenUserIsCreateUserWithoutRights("1");
		}

		public void TestChangeStatusToCancelledWhenCreatedByCurrentUserWithoutRightsAndPriviledgeIsBlank()
		{
			AssertCancelledStatusWhenUserIsCreateUserWithoutRights("");
		}

		void AssertCancelledStatusWhenUserIsCreateUserWithoutRights(string privilegeRequired)
		{
			SetSecurityPermissionsForHighestPermission(false);
			SetSecurityPermissionsForLowerLevelPermissions(false);

			var request = CreateApprovalRequest(privilegeRequired, false);
			var bulk = new CreditControlledDocumentsApprovalBulk(Factory, request);
			var result = bulk.ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled);
			AssertEquals("Approval status is not updated", Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);

			request = CreateApprovalRequest(privilegeRequired, true);
			bulk = new CreditControlledDocumentsApprovalBulk(Factory, request);
			result = bulk.ChangeStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled);
			Assert("ChangeStatus result", result);
			AssertEquals("Approval status is updated", Constants.GenApprovalRequestApprovalStatus.Cancelled, request.XP_ApprovalStatus);
		}

		void SetSecurityPermissionsForHighestPermission(bool hasPermission)
		{
			Env.Security.OnCreditHoldControllerThirdLevel.IsAllowed = hasPermission;
		}

		void SetSecurityPermissionsForLowerLevelPermissions(bool hasPermission)
		{
			Env.Security.OnCreditHoldControllerFirstLevel.IsAllowed = hasPermission;
			Env.Security.OnCreditHoldControllerSecondLevel.IsAllowed = hasPermission;
		}

		AccountingBusiness.CreditControlledDocumentsApproval CreateApprovalRequest(string privilegeRequired, bool isCreatedByCurrentUser)
		{
			var request = Factory.NewWithValidTestData<AccountingBusiness.CreditControlledDocumentsApproval>();
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			request.XP_PrivledgeRequired = privilegeRequired;
			if (isCreatedByCurrentUser)
			{
				request.XP_SystemCreateUser = Env.CurrentUser.Initials;
			}
			return request;
		}
	}
}
