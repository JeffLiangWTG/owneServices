using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class ConsolidateByBranchStrategy : ConsolidationStrategy
	{
		#region Constructors

		public ConsolidateByBranchStrategy(IConsolidationOptionsWrapper wrapper)
			: base(wrapper)
		{
		}

#if DEBUG
		public ConsolidateByBranchStrategy(IConsolidationOptionsWrapper wrapper, bool hasAcknowledged)
			: base(wrapper, hasAcknowledged)
		{
		}
#endif

		#endregion

		#region IConsolidationStrategy Members

		public override void AddMatchingFilter(ZQuery query)
		{
			if (HasAcknowledged)
			{
				var branch = Wrapper.BranchPK;
				if (!branch.IsEmpty)
				{
					query.AddToFilter(JobDeclarationSchema.JE_GB, branch);
				}
			}
		}

		public override bool IsMatching(JobDeclaration declaration)
		{
			var branch = Wrapper.BranchPK;
			return !HasAcknowledged || branch.IsEmpty || declaration.JE_GB.IsEmpty || branch == declaration.JE_GB;
		}

		public override void FillDataForNewDeclaration(JobDeclaration declaration)
		{
			if (HasAcknowledged && declaration.JE_GB.IsEmpty)
			{
				var branch = Wrapper.BranchPK;
				if (!branch.IsEmpty)
				{
					declaration.JE_GB = branch;
				}
			}
			if (declaration.JE_GB.IsEmpty)
			{
				declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			}
		}

		#endregion

		#region GetEffectiveSetting

		protected override bool GetSettingFromRegistry()
		{
			return CACustomsDataRegistry.Instance.ConsolidateByBranch.GetFallBackValueAtAllLevels(Wrapper.CompanyPK.ToGuid(), Wrapper.BranchPK.ToGuid(), Guid.Empty);
		}

		protected override bool GetSettingOfImporterCore(OrgImpAddInfo importerAddInfo)
		{
			return importerAddInfo.ZO_IsConsolidateByBranch;
		}

		#endregion
	}
}
