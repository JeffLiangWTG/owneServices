using System;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WiseCloud.Shared.Security.ParameterVerification;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public class ValidateDbArguments : AbstractApplicationStartupTask
	{
		public override string TaskDescription => Res.GetString("508972D9-0963-46EB-9FF5-05364321724F", "Checking parameters signature");

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			var dbArgsSignature = (string)arguments[ApplicationArguments.OptionDbArgsSignature];
			var dbServer = arguments.ServerName;
			var dbName = arguments.DatabaseName;

			var cwService = new CargoWiseParameterSecurity();

			if (
#if DEBUG
				MockValidationForTest &&
#endif
				(!cwService.ShouldSign(new Version(ReleaseInfo.Instance.VersionNumber.ToString())) || !cwService.ShouldVerify() || cwService.Verify(dbServer, dbName, dbArgsSignature)))
			{
				return true;
			}

			Globals.Message.ShowError(Res.GetString("666EC4EB-D10E-4F90-BBB3-7EF740794F85", "Your database parameter signature check failed"));

			return false;
		}

#if DEBUG
		internal virtual bool MockValidationForTest => true;
#endif
		public override int FailureExitCode => ExitCodes.ValidateDbArgumentsError;
	}
}
