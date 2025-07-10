#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class JobClosureConfigurationControl
	{
		public ZArchitecture.ZGrid JobClosureConfigGrid_ForTestOnly
		{
			get { return JobClosureConfigGrid; }
			set { JobClosureConfigGrid = value; }
		}
	}
}

#endif
