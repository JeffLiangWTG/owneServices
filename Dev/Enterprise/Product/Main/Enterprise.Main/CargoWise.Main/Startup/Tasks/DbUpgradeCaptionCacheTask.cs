using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Startup.Tasks
{
	class DbUpgradeCaptionCacheTask : IPostLoginTask
	{
		public string TaskDescription => Res.GetString("BE598E0A-377D-451F-BB19-A7EBA6287662", "Updating Cache data");

		public void Execute()
		{
			Env.Instance.DbUpgradeCaptions.Refresh();
		}

		public bool ShouldExecute() => true;
	}
}
