using System.Diagnostics.CodeAnalysis;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
#if WINZOR
	public
#endif
	class StartupOpenMainFormTask : AbstractApplicationStartupTask
	{
		public override string TaskDescription => Res.GetString("f14fd383-d425-4d61-8d2d-25ce7fd75370", "Opening Main Form");

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
			return !LoginDirector.Instance.HideUI && (System.Environment.UserInteractive || Globals.IsWinzor);
		}

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			MainFormInstance = new MainForm();
			if (LoginDirector.Instance.AuthenticatedUser.State == LoginAuthenticationInfo.Status.TwoFactorAuthenticationRequired)
			{
				MainFormInstance.ShowLoginUserControl(true);
			}
			else if (LoginDirector.Instance.LoggedInLocation)
			{
				MainForm.InitializeAfterLogin();
			}
			else if (LoginDirector.Instance.AuthenticatedUser.LoginValidated)
			{
				MainFormInstance.ShowLoginLocationControl();
			}
			else
			{
				MainFormInstance.ShowLoginUserControl();
			}
			return true;
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static MainForm MainFormInstance
		{
			get;
			set;
		}

		public override int FailureExitCode => ExitCodes.StartupOpenMainFormTaskError;
	}
}
