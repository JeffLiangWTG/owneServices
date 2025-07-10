using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.DevTools
{
	internal class UserIdleWorkerExplorerTool : IDevTool
	{
		public bool AddAsButton
		{
			get { return false; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		public string Name
		{
			get { return "UserIdleWorker Explorer"; }
		}

		public void Show(Form form)
		{
			new UserIdleWorkerExplorerForm().Show();
		}
	}
}