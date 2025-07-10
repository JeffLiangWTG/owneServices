using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobRevenueJournalValidationTest : TransactionHeaderWithLinesValidationTest
	{
		public void TestAH_PostDateSensitiveToJobRevenueJournalControlAccount()
		{
			string expectedError = "You cannot post this journal until the ‘Job Revenue Journal Control Account’ has been populated in the system registry.\r\nThis registry setting is available at:\r\nAccounting > General Ledger Defaults > Control Account > Job Revenue Journal Control Account";

			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			Journal.Validation.ValidateAH_PostDate();
			AssertHasError(Journal.AH_PostDateInfo, expectedError);

			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
			Journal.Validation.ValidateAH_PostDate();
			AssertNoError(Journal.AH_OSExTaxAmountInfo, expectedError);
		}

		public void TestCheckAH_OSExTaxAmount()
		{
			Journal.AH_OSExTaxAmount = 10M;
			AssertHasError(Journal.AH_OSExTaxAmountInfo, "Journal balance should be zero.");

			Journal.AH_OSExTaxAmount = 0.1M;
			AssertHasError(Journal.AH_OSExTaxAmountInfo, "Journal balance should be zero.");

			Journal.AH_OSExTaxAmount = 0M;
			AssertHasError(Journal.AH_OSExTaxAmountInfo, "Journal should contain at least one line.");

			Journal.Lines.AddNew();
			AssertNoError(Journal.AH_OSExTaxAmountInfo, "Journal balance should be zero.");
		}

		public void TestCheckAH_Desc()
		{
			Journal.AH_Desc = "";
			AssertHasError(Journal.AH_DescInfo, "Please enter a Description.");

			Journal.AH_Desc = "Test";
			AssertNoError(Journal.AH_DescInfo, "Please enter a Description.");
		}

		public void TestCheckBranchPKFrom()
		{
			AssertSimpleEntryModeGuidProperties((ZPropertyInfoGuid)Journal.BranchPKFromInfo, GlbBranch.CurrentBranch.PK);
		}

		public void TestCheckBranchPKTo()
		{
			AssertSimpleEntryModeGuidProperties((ZPropertyInfoGuid)Journal.BranchPKToInfo, GlbBranch.CurrentBranch.PK);
		}

		public void TestCheckDepartmentPKFrom()
		{
			AssertSimpleEntryModeGuidProperties((ZPropertyInfoGuid)Journal.DepartmentPKFromInfo, NonMiscDepartment.PK);
		}

		public void TestCheckDepartmentPKTo()
		{
			AssertSimpleEntryModeGuidProperties((ZPropertyInfoGuid)Journal.DepartmentPKToInfo, NonMiscDepartment.PK);
		}

		public void TestCheckDefaultSharing()
		{
			Journal.DefaultSharing = -1M;
			AssertNoErrors(Journal.DefaultSharingInfo);

			Journal.ActivateSimpleEntry(Factory.NewJobForTesting<Job>());
			Journal.RunPreSaveValidation();
			AssertHasError(Journal.DefaultSharingInfo, "value cannot be negative.");

			Journal.DefaultSharing = -1M;
			AssertHasError(Journal.DefaultSharingInfo, "value cannot be negative.");

			Journal.DefaultSharing = 0M;
			AssertNoErrors(Journal.DefaultSharingInfo);
		}

		public void TestCheckIfFromToBranchAndDepartmentTheSame()
		{
			string expectedWarning = "The From Branch/Department and To Branch/Department is the same, that is possibly wrong.";
			Journal.BranchPKFrom = GlbBranch.CurrentBranch.PK;
			Journal.BranchPKTo = Journal.BranchPKFrom;
			Journal.DepartmentPKFrom = GlbDepartment.CurrentDepartment.PK;
			Journal.DepartmentPKTo = Journal.DepartmentPKFrom;
			Journal.RunPreSaveValidation();
			AssertNoWarning(Journal.BranchPKFromInfo, expectedWarning);
			AssertNoWarning(Journal.BranchPKToInfo, expectedWarning);
			AssertNoWarning(Journal.DepartmentPKFromInfo, expectedWarning);
			AssertNoWarning(Journal.DepartmentPKToInfo, expectedWarning);

			Journal.ActivateSimpleEntry(Factory.NewJobForTesting<Job>());
			Journal.BranchPKFrom = GlbBranch.CurrentBranch.PK;
			Journal.BranchPKTo = ZGuid.Empty;
			Journal.DepartmentPKFrom = GlbDepartment.CurrentDepartment.PK;
			Journal.DepartmentPKTo = Journal.DepartmentPKFrom;
			Journal.RunPreSaveValidation();
			AssertNoWarning(Journal.BranchPKFromInfo, expectedWarning);
			AssertNoWarning(Journal.BranchPKToInfo, expectedWarning);
			AssertNoWarning(Journal.DepartmentPKFromInfo, expectedWarning);
			AssertNoWarning(Journal.DepartmentPKToInfo, expectedWarning);

			Journal.BranchPKFrom = GlbBranch.CurrentBranch.PK;
			Journal.BranchPKTo = Journal.BranchPKFrom;
			Journal.DepartmentPKFrom = GlbDepartment.CurrentDepartment.PK;
			Journal.DepartmentPKTo = ZGuid.Empty;
			Journal.RunPreSaveValidation();
			AssertNoWarning(Journal.BranchPKFromInfo, expectedWarning);
			AssertNoWarning(Journal.BranchPKToInfo, expectedWarning);
			AssertNoWarning(Journal.DepartmentPKFromInfo, expectedWarning);
			AssertNoWarning(Journal.DepartmentPKToInfo, expectedWarning);

			Journal.DepartmentPKTo = Journal.DepartmentPKFrom;
			Journal.RunPreSaveValidation();
			AssertHasWarning(Journal.BranchPKFromInfo, expectedWarning);
			AssertHasWarning(Journal.BranchPKToInfo, expectedWarning);
			AssertHasWarning(Journal.DepartmentPKFromInfo, expectedWarning);
			AssertHasWarning(Journal.DepartmentPKToInfo, expectedWarning);
		}

		public void TestCheckIfFMiscDepartment()
		{
			string expectedWarning = "Cannot issue job charges for a miscellaneous department.";
			Journal.ActivateSimpleEntry(Factory.NewJobForTesting<Job>());
			Journal.RunPreSaveValidation();
			Journal.DepartmentPKFrom = NonMiscDepartment.PK;
			Journal.DepartmentPKTo = NonMiscDepartment.PK;
			AssertNoError(Journal.DepartmentPKFromInfo, expectedWarning);
			AssertNoError(Journal.DepartmentPKToInfo, expectedWarning);

			Journal.DepartmentPKFrom = MiscDepartment.PK;
			Journal.DepartmentPKTo = MiscDepartment.PK;
			Journal.RunPreSaveValidation();
			AssertHasError(Journal.DepartmentPKFromInfo, expectedWarning);
			AssertHasError(Journal.DepartmentPKToInfo, expectedWarning);
		}

		public void TestCheckCostRevenueTypeTo()
		{
			AssertCheckCostRevenueType(Journal.CostRevenueTypeToInfo);
		}

		public void TestCheckCostRevenueTypeFrom()
		{
			AssertCheckCostRevenueType(Journal.CostRevenueTypeFromInfo);
		}

		void AssertCheckCostRevenueType(ZPropertyInfo property)
		{
			Journal.ActivateSimpleEntry(Factory.NewJobForTesting<Job>());

			property.Value = ZString.Empty;
			AssertHasError(property, "Please enter a value.");

			property.Value = new ZString("ABZ");
			AssertHasError(property, "Enter a valid selection.");

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Revenue.Code))
			{
				property.Value = new ZString(TransactionLineTypes.Revenue);
				AssertNoErrors(property);

				Journal.IsReverseTransaction = true;
				property.Value = new ZString(TransactionLineTypes.Cost);
				AssertNoErrors(property);

				Journal.IsReverseTransaction = false;
				property.Value = new ZString(TransactionLineTypes.Cost);
				var expectedError = $"When 'Job Revenue Journal GL Account Defaulting Rules' Registry is set to REV, {property.Name} must be REV.";
				AssertHasError(property, expectedError);
			}

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Cost.Code))
			{
				property.Value = new ZString(TransactionLineTypes.Cost);
				AssertNoErrors(property);

				Journal.IsReverseTransaction = true;
				property.Value = new ZString(TransactionLineTypes.Revenue);
				AssertNoErrors(property);

				Journal.IsReverseTransaction = false;
				property.Value = new ZString(TransactionLineTypes.Revenue);
				var expectedError = $"When 'Job Revenue Journal GL Account Defaulting Rules' Registry is set to CST, {property.Name} must be CST.";
				AssertHasError(property, expectedError);
			}

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty,
				AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				property.Value = new ZString(TransactionLineTypes.Cost);
				AssertNoErrors(property);

				property.Value = new ZString(TransactionLineTypes.Revenue);
				AssertNoErrors(property);
			}
		}

		protected GlbDepartment NonMiscDepartment
		{
			get { return Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, false)); }
		}

		protected GlbDepartment MiscDepartment
		{
			get { return Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Misc, true)); }
		}

		JobRevenueJournal Journal
		{
			get { return (JobRevenueJournal)Header; }
		}

		protected override Type HeaderType
		{
			get { return typeof(JobRevenueJournal); }
		}

		void AssertSimpleEntryModeGuidProperties(ZPropertyInfoGuid property, ZGuid validValue)
		{
			Journal.RunPreSaveValidation();
			AssertEquals("Precondtition: value is empty.", ZGuid.Empty, property.Value);
			AssertNoErrors(property);

			Journal.ActivateSimpleEntry(Factory.NewJobForTesting<Job>());
			property.Value = ZGuid.Empty;
			AssertHasError(property, "Please enter a value.");

			property.Value = ZGuid.NewZGuid();
			AssertHasError(property, "Enter a valid selection.");

			property.Value = validValue;
			AssertNoErrors(property);
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.NewGuid());
		}
	}
}
