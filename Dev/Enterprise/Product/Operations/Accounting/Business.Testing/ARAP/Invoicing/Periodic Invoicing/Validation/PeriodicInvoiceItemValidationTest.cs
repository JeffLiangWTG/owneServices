using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing
{
	public class PeriodicInvoiceItemValidationTest : TransactionHeaderValidationTest
	{
		public void TestLineHasMixtureOfBranches()
		{
			var expectedError = "This Invoice cannot be included in Periodic Invoice as it has lines using branches with different Posting Groups.";
			new AccountingPeriodTestHelper(Factory).SetupPeriods();
			Factory.Save();

			var branch1 = TestObjectCreator.CreateBranch("ABC", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("DEF", GlbCompany.CurrentCompany);

			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M, TestObjectCreator.ABIGAS, TestObjectCreator.GLHeader1.PK);

			var line = invoice1.Lines[0];
			line.AL_Desc = "bla bla";
			line.AL_GB = branch1.PK;

			var line1 = (InvoicingLineBase)invoice1.Lines.AddNew();
			line1.AL_Desc = "bla bla";
			line1.AL_GB = branch2.PK;

			var validation = new PeriodicInvoiceItemValidation(invoice1);
			validation.ValidateAll();

			AssertNoRowError(invoice1, expectedError);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			invoice1.IncludeInThePeriodicInvoice = true;
			validation.ValidateAll();

			AssertHasRowError(invoice1, expectedError);

			invoice1.ClearAllNotifications();
			invoice1.IncludeInThePeriodicInvoice = false;
			validation.ValidateAll();

			AssertNoRowError(invoice1, expectedError);

			invoice1.ClearAllNotifications();
			invoice1.IncludeInThePeriodicInvoice = true;
			validation.ValidateAll();

			AssertHasRowError(invoice1, expectedError);

			branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			var settings1 = new BranchGroupSettings();
			settings1.BranchPK = branch1.PK;
			settings1.GroupNumber = 1;
			settings1.IsParentBranch = false;

			var settings2 = new BranchGroupSettings();
			settings2.BranchPK = branch2.PK;
			settings2.GroupNumber = 1;
			settings2.IsParentBranch = true;

			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings1);
			branchLevelPostingConfiguration.BranchGroupSettingsCollection.Add(settings2);

			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			invoice1.ClearAllNotifications();
			validation.ValidateAll();

			//No error as the two different branches are part of the same posting group
			AssertNoRowError(invoice1, expectedError);
		}

		protected override Type HeaderType => typeof(ARInvoice);
	}
}
