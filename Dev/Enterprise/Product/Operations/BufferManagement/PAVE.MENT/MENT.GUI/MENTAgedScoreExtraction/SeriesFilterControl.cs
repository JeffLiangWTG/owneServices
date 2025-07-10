using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class SeriesFilterControl : ZUserControl
	{
		public SeriesFilterControl()
		{
			InitializeComponent();
		}

		public void UpdateBindings(StmModuleFilter filter)
		{
			filterStripWrapperControl.SetDataBinding(filter, string.Empty);
		}
	}
}
