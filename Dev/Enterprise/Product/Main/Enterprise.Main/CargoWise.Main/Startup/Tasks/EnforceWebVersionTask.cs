using System.Windows.Forms;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class EnforceWebVersionTask : IPostLoginTask
	{
		public string TaskDescription => Res.GetString("E17673A7-496F-420A-B8E6-AB903562BB5C", "Enforce Web Version");

		public void Execute()
		{
			if (StartupOpenMainFormTask.MainFormInstance != null && !StartupOpenMainFormTask.MainFormInstance.Visible)
			{
				StartupOpenMainFormTask.MainFormInstance.Shown += (_, _) => ShowShutdownMessage();
			}
			else
			{
				ShowShutdownMessage();
			}
		}

		static void ShowShutdownMessage()
		{
			Globals.Message.Show(
				Res.GetString("6972E064-E81E-4FD6-A296-1DE0031DBF24",
				"You have been added into a Web Version trial. Please use WiseCloud Client to launch the CargoWise Web Version. Look for the CXW icon."),
				Res.GetString("1CD2E4B4-39BA-4269-B43C-749A5E69F056", "Enforce Web Version"),
				MessageBoxButtons.OK, MessageBoxIcon.Information);
			Env.ExitApplication();
		}

		public bool ShouldExecute()
		{
			using (Db.DisposableActionForDbConnection())
			{
				return GlbStaff.CurrentUser.CheckWebVersionEnforcedForUser();
			}
		}
	}
}
