using CargoWise.Application;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class ForceCW1HomeScreenTask : IPostLoginTask
	{
		readonly IProgramRestarter programRestarter;
		public ForceCW1HomeScreenTask() : this(ObjectFactory.Get<IProgramRestarter>()) { }
		public ForceCW1HomeScreenTask(IProgramRestarter programRestarter) => this.programRestarter = programRestarter;
		public string TaskDescription => Res.GetString("D335541A-B786-49CE-826E-E5742D2EB2A6", "Force old CW1 Home Screen");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWise", "CW1078:DoNotUseProcessStart")]
		public void Execute()
		{
			var arguments = CommandLineArguments.UsedToLaunchApplication.Clone();
			arguments.OptionalArgs[ApplicationArguments.OptionForceCW1HomeScreen] = true;
			programRestarter.Restart(arguments: arguments);
		}

		public bool ShouldExecute()
		{
			if (!CWNextFeatureHelper.IsCWNextEnabled())
			{
				return false;
			}

			using (Db.DisposableActionForDbConnection())
			{
				return GlbStaff.CurrentUser.CheckForceCW1HomeScreenEnabledForUser();
			}
		}
	}
}
