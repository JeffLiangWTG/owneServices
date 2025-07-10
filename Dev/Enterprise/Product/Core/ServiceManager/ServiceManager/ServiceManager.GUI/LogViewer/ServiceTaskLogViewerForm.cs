using System;
using System.Windows.Forms;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	public partial class ServiceTaskLogViewerForm : ZChildForm
	{
		protected ServiceTaskLogViewerForm()
		{ }

		public ServiceTaskLogViewerForm(ServiceTaskLogViewer logViewer)
			: base(logViewer)
		{
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void RefreshButton_Click(object sender, EventArgs e)
		{
			LogViewer.StrategyLogViewerControl.RefreshLogs();
		}

		void ServiceTaskLogViewerForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F5)
			{
				LogViewer.StrategyLogViewerControl.RefreshLogs();
			}
		}
	}
}

