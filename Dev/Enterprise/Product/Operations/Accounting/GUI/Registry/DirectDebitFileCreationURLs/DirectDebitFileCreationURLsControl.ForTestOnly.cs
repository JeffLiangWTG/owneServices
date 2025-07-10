#if DEBUG

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class DirectDebitFileCreationURLsControl
	{
		public ZArchitecture.ZGrid DirectDebitFileURLGrid_ForTestOnly
		{
			get { return DirectDebitFileURLGrid; }
			set { DirectDebitFileURLGrid = value; }
		}
	}
}

#endif
