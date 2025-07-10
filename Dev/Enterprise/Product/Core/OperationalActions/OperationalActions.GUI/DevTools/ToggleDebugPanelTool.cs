using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Services.OperationalActions.GUI.DevTools
{
	internal sealed class ToggleDebugPanelTool : IDevTool
	{
		public bool AddAsButton
		{
			get { return true; }
		}

		public string Name
		{
			get { return (NoResString)"Tgl Dbg"; }
		}

		public void Show(Form form)
		{
			OperationalActionRunnerForm runnerForm = form as OperationalActionRunnerForm;

			if (runnerForm != null)
			{
				runnerForm.EnableDebugLogging = !runnerForm.EnableDebugLogging;
			}
			else
			{
				Globals.Message.Show((NoResString)"Approperate form not found");
			}
		}
	}
}
