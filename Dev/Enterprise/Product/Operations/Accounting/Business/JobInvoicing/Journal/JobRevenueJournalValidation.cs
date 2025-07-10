using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRevenueJournalValidation : TransactionHeaderWithLinesValidation
	{
		public JobRevenueJournalValidation(JobRevenueJournal parent)
			: base(parent)
		{
		}

		JobRevenueJournal ParentJournal
		{
			get { return (JobRevenueJournal)Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateBranchPKFrom();
			ValidateBranchPKTo();
			ValidateDepartmentPKFrom();
			ValidateDepartmentPKTo();
			ValidateDefaultSharing();
			ValidateCostRevenueTypeFrom();
			ValidateCostRevenueTypeTo();
		}

		protected override void CheckAH_OSExTaxAmount()
		{
			base.CheckAH_OSExTaxAmount();

			if (ParentJournal.AH_OSExTaxAmount != 0)
			{
				ParentJournal.AH_OSExTaxAmountInfo.AddError(Res.GetString("0733008F-5CDB-444E-90DB-2CF2B8EE2B96", "Journal balance should be zero."));
			}
			else if (ParentJournal.Lines.Count == 0)
			{
				ParentJournal.AH_OSExTaxAmountInfo.AddError(Res.GetString("F5D0733C-3A4D-4173-A086-CBEB8EB4862A", "Journal should contain at least one line."));
			}
		}

		protected override void CheckAH_Desc()
		{
			base.CheckAH_Desc();
			MandatoryValidation.CheckEntered(Parent.AH_DescInfo);
		}

		protected override void CheckAH_PostDate()
		{
			base.CheckAH_PostDate();

			if (AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Value == Guid.Empty)
			{
				ParentJournal.AH_PostDateInfo.AddError(Res.GetString("00582D14-58A4-492F-9B7D-F30968902C93", "You cannot post this journal until the ‘Job Revenue Journal Control Account’ has been populated in the system registry.\r\nThis registry setting is available at:\r\nAccounting > General Ledger Defaults > Control Account > Job Revenue Journal Control Account"));
			}
		}

		protected override void CheckAH_GE()
		{
			if (ShouldValidateBranchDepartmentCombination)
			{
				if ((ShouldValidateBranchDepartmentCombinationForParentInDatabase || !ParentJournal.IsInDatabase)
					|| ParentJournal.AH_GBInfo.HasChanges || ParentJournal.AH_GEInfo.HasChanges)
				{
					ZGuid newAH_JH;
					ZGuid newAH_GB;
					ZGuid newAH_GE;
					ParentJournal.RecalculateAH_JH_AH_GB_AH_GE(out newAH_JH, out newAH_GB, out newAH_GE);

					var branch = ParentJournal.Factory.Load<GlbBranch>(newAH_GB);
					var department = ParentJournal.Factory.Load<GlbDepartment>(newAH_GE);
					GlbBranchCombinationValidation.CheckBranchDepartmentCombination(ParentJournal.AH_GEInfo, branch, department, NotificationTypeForBranchDepartmentCombination);
				}
			}
		}

		public void ValidateBranchPKFrom()
		{
			ValidateCalculatedProperty(ParentJournal.BranchPKFromInfo);
		}

		protected void CheckBranchPKFrom()
		{
			if (ParentJournal.IsInSimpleEntryMode)
			{
				MandatoryValidation.CheckEntered(ParentJournal.BranchPKFromInfo);
				ListValidation.ErrorIfInvalidPK(ParentJournal.BranchPKFromInfo, ParentJournal.Lookups.Branches);
				CheckIfFromToBranchAndDepartmentTheSame(ParentJournal.BranchPKFromInfo);
			}
		}

		public void ValidateBranchPKTo()
		{
			ValidateCalculatedProperty(ParentJournal.BranchPKToInfo);
		}

		protected void CheckBranchPKTo()
		{
			if (ParentJournal.IsInSimpleEntryMode)
			{
				MandatoryValidation.CheckEntered(ParentJournal.BranchPKToInfo);
				ListValidation.ErrorIfInvalidPK(ParentJournal.BranchPKToInfo, ParentJournal.Lookups.Branches);
				CheckIfFromToBranchAndDepartmentTheSame(ParentJournal.BranchPKToInfo);
			}
		}

		public void ValidateDepartmentPKFrom()
		{
			ValidateCalculatedProperty(ParentJournal.DepartmentPKFromInfo);
		}

		protected void CheckDepartmentPKFrom()
		{
			if (ParentJournal.IsInSimpleEntryMode)
			{
				MandatoryValidation.CheckEntered(ParentJournal.DepartmentPKFromInfo);
				ListValidation.ErrorIfInvalidPK(ParentJournal.DepartmentPKFromInfo, ParentJournal.Lookups.Departments);
				CheckIfFromToBranchAndDepartmentTheSame(ParentJournal.DepartmentPKFromInfo);
				CheckIfFMiscDepartment(ParentJournal.DepartmentPKFrom, ParentJournal.DepartmentPKFromInfo);
			}
		}

		public void ValidateDepartmentPKTo()
		{
			ValidateCalculatedProperty(ParentJournal.DepartmentPKToInfo);
		}

		protected void CheckDepartmentPKTo()
		{
			if (ParentJournal.IsInSimpleEntryMode)
			{
				MandatoryValidation.CheckEntered(ParentJournal.DepartmentPKToInfo);
				ListValidation.ErrorIfInvalidPK(ParentJournal.DepartmentPKToInfo, ParentJournal.Lookups.Departments);
				CheckIfFromToBranchAndDepartmentTheSame(ParentJournal.DepartmentPKToInfo);
				CheckIfFMiscDepartment(ParentJournal.DepartmentPKTo, ParentJournal.DepartmentPKToInfo);
			}
		}

		public void ValidateDefaultSharing()
		{
			ValidateCalculatedProperty(ParentJournal.DefaultSharingInfo);
		}

		protected void CheckDefaultSharing()
		{
			if (ParentJournal.IsInSimpleEntryMode)
			{
				MandatoryValidation.CheckNotNegative(ParentJournal.DefaultSharingInfo);
			}
		}

		void CheckIfFromToBranchAndDepartmentTheSame(ZPropertyInfo property)
		{
			if (ParentJournal.BranchPKTo == ParentJournal.BranchPKFrom &&
				ParentJournal.DepartmentPKTo == ParentJournal.DepartmentPKFrom)
			{
				property.AddWarning(Res.GetString("9F73034E-9F38-48C9-A8E0-8FC1B3E5989D", "The From Branch/Department and To Branch/Department is the same, that is possibly wrong."));
			}
		}

		void CheckIfFMiscDepartment(ZGuid deptPK, ZPropertyInfo property)
		{
			if (deptPK.IsValid && !deptPK.IsEmpty)
			{
				GlbDepartment dept = ParentJournal.Factory.Load<GlbDepartment>(deptPK);
				if (dept != null && dept.GE_Misc)
				{
					property.AddError(Res.GetString("DB970707-F8F7-4C78-9590-B7A657A8CF46", "Cannot issue job charges for a miscellaneous department."));
				}
			}
		}

		public void ValidateCostRevenueTypeTo()
		{
			ValidateCalculatedProperty(ParentJournal.CostRevenueTypeToInfo);
		}

		protected void CheckCostRevenueTypeTo()
		{
			CheckCostRevenueTypeCore(ParentJournal.CostRevenueTypeToInfo);
		}

		public void ValidateCostRevenueTypeFrom()
		{
			ValidateCalculatedProperty(ParentJournal.CostRevenueTypeFromInfo);
		}

		protected void CheckCostRevenueTypeFrom()
		{
			CheckCostRevenueTypeCore(ParentJournal.CostRevenueTypeFromInfo);
		}

		void CheckCostRevenueTypeCore(ZPropertyInfo propertyInfo)
		{
			if (ParentJournal.IsInSimpleEntryMode)
			{
				MandatoryValidation.CheckEntered(propertyInfo);
				ListValidation.ErrorIfInvalidCode(propertyInfo, ParentJournal.CostRevenueTypeList);
				if (!ParentJournal.IsReverseTransaction)
				{
					var registrySetting = AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);
					if (registrySetting != AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code && (ZString)propertyInfo.Value != registrySetting)
					{
						propertyInfo.AddError(Res.GetString("74cc750c-63c9-4ef0-adba-69f2114377ef", "When '{0}' Registry is set to {2}, {1} must be {2}.",
							AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.Caption, propertyInfo.Name, registrySetting));
					}
				}
			}
		}
	}
}
