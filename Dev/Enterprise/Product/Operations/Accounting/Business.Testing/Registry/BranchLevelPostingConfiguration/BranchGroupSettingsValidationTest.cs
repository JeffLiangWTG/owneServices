using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class BranchGroupSettingsValidationTest : TestCaseWithFactory
	{
		public void TestBranch()
		{
			var branch = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "AA");

			var config = new BranchLevelPostingConfiguration();
			config.EnableBranchLevelPosting = true;
			var settings1 = config.BranchGroupSettingsCollection.AddNew();

			settings1.Validation.ValidateBranchPK();

			AssertHasError("Branch must be unique in the grid", settings1.BranchPKInfo, "Please enter a Branch.");

			settings1.BranchPK = branch.PK;

			AssertNoError("Branch must be unique in the grid", settings1.BranchPKInfo, "Please enter a Branch.");

			var settings2 = config.BranchGroupSettingsCollection.AddNew();
			settings2.BranchPK = branch.PK;

			var settings3 = config.BranchGroupSettingsCollection.AddNew();
			settings3.BranchPK = GlbBranch.CurrentBranch.PK;

			AssertHasError("Branch must be unique in the grid", settings2.BranchPKInfo, "Same Branch not allowed more than once. Please select another Branch.");
			AssertNoError("Error should not be shown on row without duplicate branch", settings3.BranchPKInfo, "Same Branch not allowed more than once. Please select another Branch.");

			settings2.BranchPK = TestObjectCreator.NonCurrentBranch.PK;

			AssertNoError("No error as Branches are unique", settings2.BranchPKInfo, "Same Branch not allowed more than once. Please select another Branch.");
		}

		public void TestPostingGorupNumber()
		{
			var config = new BranchLevelPostingConfiguration();
			config.EnableBranchLevelPosting = true;
			var settings1 = config.BranchGroupSettingsCollection.AddNew();
			settings1.GroupNumber = 0;

			AssertNoError(settings1.GroupNumberInfo, "Please enter a Posting Group number between 0 to 9.");

			settings1.GroupNumber = 10;

			AssertHasError(settings1.GroupNumberInfo, "Please enter a Posting Group number between 0 to 9.");

			settings1.GroupNumber = 9;

			AssertNoError(settings1.GroupNumberInfo, "Please enter a Posting Group number between 0 to 9.");
		}

		public void TestIsParentBranch()
		{
			var branch1 = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "AA");
			var branch2 = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "BB");
			var branch3 = TestObjectCreator.CreateNewBranch(GlbCompany.CurrentCompany, "CC");

			var config = new BranchLevelPostingConfiguration();
			config.EnableBranchLevelPosting = true;

			var settings1 = config.BranchGroupSettingsCollection.AddNew();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = false;

			var settings2 = config.BranchGroupSettingsCollection.AddNew();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = false;

			var settings3 = config.BranchGroupSettingsCollection.AddNew();
			settings3.BranchPK = branch3.PK;
			settings3.GroupNumber = 2;
			settings3.IsParentBranch = true;

			config.RunPreSaveValidation();

			AssertHasError(settings1.IsParentBranchInfo, "A single branch in each Posting Group must be set as the Branch to fallback for Transaction Header");
			AssertHasError(settings2.IsParentBranchInfo, "A single branch in each Posting Group must be set as the Branch to fallback for Transaction Header");
			AssertNoError(settings3.IsParentBranchInfo, "A single branch in each Posting Group must be set as the Branch to fallback for Transaction Header");

			//make all the branches belong to the same posting group
			settings3.GroupNumber = 1;

			config.RunPreSaveValidation();

			AssertNoError(settings1.IsParentBranchInfo, "A single branch in each Posting Group must be set as the Branch to fallback for Transaction Header");
			AssertNoError(settings2.IsParentBranchInfo, "A single branch in each Posting Group must be set as the Branch to fallback for Transaction Header");
			AssertNoError(settings3.IsParentBranchInfo, "A single branch in each Posting Group must be set as the Branch to fallback for Transaction Header");

			//make two branches in a posting group as parent
			settings2.IsParentBranch = true;

			config.RunPreSaveValidation();

			AssertNoError(settings1.IsParentBranchInfo, "Only one branch in a Posting Group can be set as the Branch to fallback for Transaction Header");
			AssertHasError(settings2.IsParentBranchInfo, "Only one branch in a Posting Group can be set as the Branch to fallback for Transaction Header");
			AssertHasError(settings3.IsParentBranchInfo, "Only one branch in a Posting Group can be set as the Branch to fallback for Transaction Header");

			settings2.IsParentBranch = false;

			config.RunPreSaveValidation();

			AssertNoError(settings1.IsParentBranchInfo, "Only one branch in a Posting Group can be set as the Branch to fallback for Transaction Header");
			AssertNoError(settings2.IsParentBranchInfo, "Only one branch in a Posting Group can be set as the Branch to fallback for Transaction Header");
			AssertNoError(settings3.IsParentBranchInfo, "Only one branch in a Posting Group can be set as the Branch to fallback for Transaction Header");
		}

		TestObjectCreator TestObjectCreator => new TestObjectCreator(Factory);
	}
}
