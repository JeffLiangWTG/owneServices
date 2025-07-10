#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class BranchLevelPostingConfigurationControl
	{
		public ZArchitecture.ZGrid BranchGroupSettingsGrid_ForTestOnly
		{
			get { return BranchGroupSettingsGrid; }
			set { BranchGroupSettingsGrid = value; }
		}
	}
}

#endif
