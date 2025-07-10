using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.CommissionManagement.Business.Testing
{
	internal class AccCommissionApprovalRequestValidationTest : BusinessObjectValidationTestCase
	{
		#region Properties

		public void TestCheckCRQ_BatchNumber_EmptyIsOk()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_BatchNumber = "";
			request.Validation.ValidateCRQ_BatchNumber();

			AssertNoErrors("Empty batch number is ok - will be populated on save", request.CRQ_BatchNumberInfo);
		}

		public void TestCRQ_GS_NKApprovingStaff1_ErrorIfInvalidCode()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";

			var request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff1 = "ADL";
			AssertListValidationInvalidCodeError(request.CRQ_GS_NKApprovingStaff1Info, false);

			request.CRQ_GS_NKApprovingStaff1 = "XXX";
			AssertListValidationInvalidCodeError(request.CRQ_GS_NKApprovingStaff1Info, true);
		}

		public void TestCRQ_GS_NKApprovingStaff1_IsMandatoryDependingOnRegistry()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();
			{
				OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

				request.CRQ_GS_NKApprovingStaff1 = "ADL";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff1Info, false);

				request.CRQ_GS_NKApprovingStaff1 = "";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff1Info, true);
			}

			{
				OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

				request.CRQ_GS_NKApprovingStaff1 = "ADL";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff1Info, false);

				request.CRQ_GS_NKApprovingStaff1 = "";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff1Info, true);
			}

			{
				OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

				request.CRQ_GS_NKApprovingStaff1 = "ADL";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff1Info, false);

				request.CRQ_GS_NKApprovingStaff1 = "";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff1Info, false);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCRQ_GS_NKApprovingStaff1_MustHaveSecurityRight()
		{
			// Bug found in Work Item WI00112341, if you dont clear the context then suspected leaks from previous tests will make the securities DisplayText have then incorrect value (DAT only). Will be resolved soon
			var ctx = Env.CurrentUserContext;
			Env.ClearUserContext();
			Env.SetUserContext(ctx);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff1Security = Factory.New<GlbSecurity>();
			staff1Security.GU_GS = staff1.PK;
			staff1Security.GU_SecurityRight = Env.Security.CommissionAuthorizationLevel1.Code;
			staff1Security.GU_SecurityItemIsAllowed = true;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";
			var staff2Security = Factory.New<GlbSecurity>();
			staff2Security.GU_GS = staff2.PK;
			staff2Security.GU_SecurityRight = Env.Security.CommissionAuthorizationLevel1.Code;
			staff2Security.GU_SecurityItemIsAllowed = false;

			Factory.Save();

			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			var expectedNoLevel1SecurityAccessMessage = string.Format(@"Staff does not have the appropriate security rights to approve this.

If this staff requires access to this function, ask your system administrator to change their Staff or Group Security Rights to allow access to:

{0}",
					Env.Security.CommissionAuthorizationLevel1.DisplayTextPathToSecurityRight);

			approvalRequest.CRQ_GS_NKApprovingStaff1 = "ADL";
			AssertNoError(approvalRequest.CRQ_GS_NKApprovingStaff1Info, expectedNoLevel1SecurityAccessMessage);

			approvalRequest.CRQ_GS_NKApprovingStaff1 = "SCW";
			AssertHasError(approvalRequest.CRQ_GS_NKApprovingStaff1Info, expectedNoLevel1SecurityAccessMessage);
		}

		public void TestCRQ_GS_NKApprovingStaff1_IsNotEqualStaff2()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff2 = "SCW";

			request.CRQ_GS_NKApprovingStaff1 = "ADL";
			AssertNoError(request.CRQ_GS_NKApprovingStaff1Info, "The 1st Authorization Staff and 2nd Authorization Staff cannot be the same.");

			request.CRQ_GS_NKApprovingStaff1 = "SCW";
			AssertHasError(request.CRQ_GS_NKApprovingStaff1Info, "The 1st Authorization Staff and 2nd Authorization Staff cannot be the same.");

			request.CRQ_GS_NKApprovingStaff2 = "";
			request.CRQ_GS_NKApprovingStaff1 = "";
			AssertNoError(request.CRQ_GS_NKApprovingStaff1Info, "The 1st Authorization Staff and 2nd Authorization Staff cannot be the same.");
		}

		public void TestCRQ_GS_NKApprovingStaff1_MustHaveValidEmailAddress()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_EmailAddress = "";

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.CommissionAuthorizationLevel1.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			var request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff1 = "ADL";
			request.Validation.ValidateCRQ_GS_NKApprovingStaff1();
			AssertHasError(request.CRQ_GS_NKApprovingStaff1Info, "Staff does not have an email address.");

			staff.GS_EmailAddress = "invalid email address";
			Factory.Save();
			request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff1 = "ADL";
			request.Validation.ValidateCRQ_GS_NKApprovingStaff1();
			AssertHasError(request.CRQ_GS_NKApprovingStaff1Info, "Staff email address is not valid.");

			staff.GS_EmailAddress = "andrew.luong@wisetechglobal.com";
			Factory.Save();
			request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff1 = "ADL";
			request.Validation.ValidateCRQ_GS_NKApprovingStaff1();
			AssertNoErrors(request.CRQ_GS_NKApprovingStaff1Info);
		}

		public void TestCRQ_GS_NKApprovingStaff2_ErrorIfInvalidCode()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";

			var request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff2 = "ADL";
			AssertListValidationInvalidCodeError(request.CRQ_GS_NKApprovingStaff2Info, false);

			request.CRQ_GS_NKApprovingStaff2 = "XXX";
			AssertListValidationInvalidCodeError(request.CRQ_GS_NKApprovingStaff2Info, true);
		}

		public void TestCRQ_GS_NKApprovingStaff2_IsMandatoryDependingOnRegistry()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();
			{
				OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

				request.CRQ_GS_NKApprovingStaff2 = "ADL";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff2Info, false);

				request.CRQ_GS_NKApprovingStaff2 = "";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff2Info, true);
			}

			{
				OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

				request.CRQ_GS_NKApprovingStaff2 = "ADL";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff2Info, false);

				request.CRQ_GS_NKApprovingStaff2 = "";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff2Info, false);
			}

			{
				OrganisationsDataRegistry.Instance.CommissionApprovalLevelRequired.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);

				request.CRQ_GS_NKApprovingStaff2 = "ADL";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff2Info, false);

				request.CRQ_GS_NKApprovingStaff2 = "";
				AssertMandatoryValidationError(request.CRQ_GS_NKApprovingStaff2Info, false);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCRQ_GS_NKApprovingStaff2_MustHaveSecurityRight()
		{
			// Bug found in Work Item WI00112341, if you dont clear the context then suspected leaks from previous tests will make the securities DisplayText have then incorrect value (DAT only). Will be resolved soon
			var ctx = Env.CurrentUserContext;
			Env.ClearUserContext();
			Env.SetUserContext(ctx);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ADL";
			var staff1Security = Factory.New<GlbSecurity>();
			staff1Security.GU_GS = staff1.PK;
			staff1Security.GU_SecurityRight = Env.Security.CommissionAuthorizationLevel2.Code;
			staff1Security.GU_SecurityItemIsAllowed = true;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "SCW";
			var staff2Security = Factory.New<GlbSecurity>();
			staff2Security.GU_GS = staff2.PK;
			staff2Security.GU_SecurityRight = Env.Security.CommissionAuthorizationLevel2.Code;
			staff2Security.GU_SecurityItemIsAllowed = false;

			Factory.Save();

			var approvalRequest = Factory.New<AccCommissionApprovalRequest>();
			var expectedNoLevel2SecurityAccessMessage = string.Format(@"Staff does not have the appropriate security rights to approve this.

If this staff requires access to this function, ask your system administrator to change their Staff or Group Security Rights to allow access to:

{0}",
					Env.Security.CommissionAuthorizationLevel2.DisplayTextPathToSecurityRight);

			approvalRequest.CRQ_GS_NKApprovingStaff2 = "ADL";
			AssertNoError(approvalRequest.CRQ_GS_NKApprovingStaff2Info, expectedNoLevel2SecurityAccessMessage);

			approvalRequest.CRQ_GS_NKApprovingStaff2 = "SCW";
			AssertHasError(approvalRequest.CRQ_GS_NKApprovingStaff2Info, expectedNoLevel2SecurityAccessMessage);
		}

		public void TestCRQ_GS_NKApprovingStaff2_IsNotEqualStaff1()
		{
			var request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff1 = "SCW";

			request.CRQ_GS_NKApprovingStaff2 = "ADL";
			AssertNoError(request.CRQ_GS_NKApprovingStaff2Info, "The 2nd Authorization Staff and 1st Authorization Staff cannot be the same.");

			request.CRQ_GS_NKApprovingStaff2 = "SCW";
			AssertHasError(request.CRQ_GS_NKApprovingStaff2Info, "The 2nd Authorization Staff and 1st Authorization Staff cannot be the same.");

			request.CRQ_GS_NKApprovingStaff1 = "";
			request.CRQ_GS_NKApprovingStaff2 = "";
			AssertNoError(request.CRQ_GS_NKApprovingStaff2Info, "The 2nd Authorization Staff and 1st Authorization Staff cannot be the same.");
		}

		public void TestCRQ_GS_NKApprovingStaff2_MustHaveValidEmailAddress()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "ADL";
			staff.GS_EmailAddress = "";

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = Env.Security.CommissionAuthorizationLevel2.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;

			Factory.Save();

			var request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff2 = "ADL";

			request.Validation.ValidateCRQ_GS_NKApprovingStaff2();
			AssertHasError(request.CRQ_GS_NKApprovingStaff2Info, "Staff does not have an email address.");

			staff.GS_EmailAddress = "invalid email address";
			Factory.Save();

			request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff2 = "ADL";
			request.Validation.ValidateCRQ_GS_NKApprovingStaff2();
			AssertHasError(request.CRQ_GS_NKApprovingStaff2Info, "Staff email address is not valid.");

			staff.GS_EmailAddress = "andrew.luong@wisetechglobal.com";
			Factory.Save();

			request = Factory.New<AccCommissionApprovalRequest>();
			request.CRQ_GS_NKApprovingStaff2 = "ADL";
			request.Validation.ValidateCRQ_GS_NKApprovingStaff2();
			AssertNoErrors(request.CRQ_GS_NKApprovingStaff2Info);
		}

		#endregion
	}
}
