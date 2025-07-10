using Enterprise.Security.ActiveDirectory.GUI;
using Enterprise.ZArchitecture.Core;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class StartupADPasswordExpiryChecker : IPostLoginTask
	{
		public string TaskDescription
		{
			get { return Res.GetString("77cf01ed-02b4-432f-b248-e89f510dea2d", "Reminder for AD password expiry"); }
		}

		public bool ShouldExecute()
		{
			return true;
		}

		public void Execute()
		{
			ADPasswordExpiryChecker.Instance.Enable();
		}
	}
}
