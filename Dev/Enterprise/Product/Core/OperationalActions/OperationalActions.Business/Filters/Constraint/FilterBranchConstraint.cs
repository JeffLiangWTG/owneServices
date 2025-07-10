using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FilterBranchConstraint : IFilterConstraint
	{
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		string IFilterConstraint.Name
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return FilterConstants.Branch; }
		}

		public string SingularValueName
		{
			get { return Res.GetString("f32e14f5-36fb-49c3-89ea-3abd08738b1a", "branch"); }
		}

		public string PluralValueName
		{
			get { return Res.GetString("c55ff25e-d46a-4cc6-bc82-8b043ea6f5fa", "branches"); }
		}

		public string Description
		{
			get
			{
				return Res.GetString("OperationalActionsFilter|BranchConstraint|Description",
					"Matches on the branch code of the current login branch.\r\ne.g. 'Branch == \"BNE\"' will only match if the code of the current login branch is 'BNE'.");
			}
		}

		public object GetValue()
		{
			return GetDefaultStringValue();
		}

		public string GetDefaultStringValue()
		{
			GlbBranch branch = GlbBranch.CurrentBranch;
			return branch == null ? null : branch.GB_Code;
		}
	}
}
