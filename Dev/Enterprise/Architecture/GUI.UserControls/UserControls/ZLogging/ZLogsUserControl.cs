using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	internal partial class ZLogsUserControl : ZUserControl
	{
		public ZLogsUserControl(GetStmALogFilterStripBusinessObject getStmALogFilterStripBusinessObject = null)
		{
			InitializeComponent();
			ChangeLogsTabPage.GetStmALogFilterStripBusinessObject = getStmALogFilterStripBusinessObject;

			if (!EnvProxy.Instance.Registry.UserEventTrackingEnterprise && !EnvProxy.Instance.Registry.UserEventTrackingExternal)
			{
				ActivityLogsTabPage.TabVisible = false;
			}
		}

		public LogsToShow LogsToShow
		{
			get { return ChangeLogsTabPage.LogsToShow; }
			set { ChangeLogsTabPage.LogsToShow = value; }
		}
	}
}
