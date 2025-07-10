using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Core;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class StartupEnableActivityLogger : IPostLoginTask
	{
		public string TaskDescription
		{
			get { return Res.GetString("8017b70c-dc6e-42de-ba21-b8c2dfcb25ca", "Activity Logger"); }
		}

		public bool ShouldExecute()
		{
			return true;
		}

		public void Execute()
		{
			ZFormActivityLogger.Instance.EnableActivityLogger();
			KToolStrip.IsTrackingEnabled = SystemDataRegistry.Instance.EnableToolStripTracking.Value;
		}
	}
}
