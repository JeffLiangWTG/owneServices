using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccQueryClaims.Testing
{
	[TestedType(typeof(AccQueryClaimReassignAction))]
	public class AccQueryClaimReassignActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSycnhronisePassesValuesToClaim()
		{
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			AccQueryClaimReassignAction action = new AccQueryClaimReassignAction(claim);
			action.StaffList[0].GS_Code = "BBB";
			action.BranchList.Load();
			action.StaffCode = action.StaffList[0].GS_Code;
			action.BranchPK = action.BranchList[0].PK;
			action.Comment = "Some cool comment";
			action.Sycnhronise();
			AssertEquals("Staff", action.Staff.GS_Code, claim.AY_GS_NKStaffAssignedTo);
			AssertEquals("Branch", action.BranchPK, claim.AY_GB);
			AssertContains("Some cool comment", claim.Details);
			Assert("IsReassigned", claim.IsReassigned);
			action.StaffCode = "";
			action.Sycnhronise();
			AssertHasErrors(action.StaffCodeInfo);
			Assert("IsReassigned", !claim.IsReassigned);
		}

		public void TestValidateStaffPK()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_IsActive = ZBool.False;
			Factory.Save();
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			AccQueryClaimReassignAction action = new AccQueryClaimReassignAction(claim);
			action.RunPreSaveValidation();
			action.StaffCode = ZString.Empty;
			AssertHasErrors(action.StaffCodeInfo);
			action.StaffCode = "CCC";
			AssertHasErrors(action.StaffCodeInfo);
			action.StaffCode = staff2.GS_Code;
			AssertHasErrors(action.StaffCodeInfo);
			action.StaffCode = staff1.GS_Code;
			AssertNoErrors(action.StaffCodeInfo);
		}

		public void TestValidateStaffEmail()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_EmailAddress = ZString.Empty;
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_EmailAddress = "Test@DP.com";
			Factory.Save();
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			AccQueryClaimReassignAction action = new AccQueryClaimReassignAction(claim);
			action.RunPreSaveValidation();
			var expectedWarning = @"The selected staff member does not have an email address entered in the Staff and Resources record.
No notification email will be sent to the staff member.

Alternatively, enter the email address of the staff member under Maintain > System > Staff and Resources before reassigning the claim.";
			action.StaffCode = staff1.GS_Code;
			AssertHasWarning(action.StaffCodeInfo, expectedWarning);
			action.StaffCode = staff2.GS_Code;
			AssertNoWarnings(action.StaffCodeInfo);
		}

		public void TestValidateBranchPK()
		{
			GlbBranch inactiveBranch = Factory.NewWithValidTestData<GlbBranch>();
			inactiveBranch.GB_IsActive = ZBool.False;
			Factory.Save();
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			AccQueryClaimReassignAction action = new AccQueryClaimReassignAction(claim);
			action.BranchList.Load();
			action.RunPreSaveValidation();
			action.BranchPK = ZGuid.Empty;
			AssertHasErrors(action.BranchPKInfo);
			action.BranchPK = ZGuid.NewZGuid();
			AssertHasErrors(action.BranchPKInfo);
			action.BranchPK = inactiveBranch.PK;
			AssertHasErrors(action.BranchPKInfo);
			action.BranchPK = action.BranchList[0].PK;
			AssertNoErrors(action.BranchPKInfo);
		}

		public void TestSetDefaultsFromClaim()
		{
			GlbBranch someBranch = Factory.New<GlbBranch>();
			GlbStaff someStaff = Factory.New<GlbStaff>();
			someStaff.GS_Code = "AAA";
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			claim.AY_GB = someBranch.PK;
			claim.AY_GS_NKStaffAssignedTo = someStaff.GS_Code;
			AccQueryClaimReassignAction action = new AccQueryClaimReassignAction(claim);
			AssertEquals("Before: BranchPK", ZGuid.Empty, action.BranchPK);
			AssertEquals("Before: StaffCode", ZString.Empty, action.StaffCode);
			action.SetDefaultsFromClaim();
			AssertEquals("After: BranchPK", someBranch.PK, action.BranchPK);
			AssertEquals("After: StaffCode", someStaff.GS_Code, action.StaffCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			APAccQueryClaim claim = Factory.New<APAccQueryClaim>();
			return new AccQueryClaimReassignAction(claim);
		}
	}
}
