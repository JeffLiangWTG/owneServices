using System;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class SeriesConfigurationControl : ZUserControl
	{
		public SeriesConfigurationControl()
		{
			InitializeComponent();
		}

		public void UpdateBinding(StmModuleFilter filter)
		{
			this.BeginInvoke(new Action(() => seriesFilterControl.UpdateBindings(filter)));
		}
	}
}
