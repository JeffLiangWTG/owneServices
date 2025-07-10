using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DbHealth.Check;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class StartupDbHealthRestoreWarningChecker : IPostLoginTask
	{
		public string TaskDescription
		{
			get { return Res.GetString("64F0C900-9AD1-4BFF-9BA8-F8D801DEB4F4", "Startup Database Health Restore Warning Checker"); }
		}

		public bool ShouldExecute()
		{
			return Env.CurrentCompany?.Country?.Code == CountryCodes.Portugal
				&& !EnvProxy.IsHostedWithCargowise;
		}

		public void Execute()
		{
			var registryValue = SystemDataRegistry.Instance.LastDbHealthCheckWarningList.Value;
			var allWarnings = registryValue.Cast<DbHealthWarningRegistryElement>();
			var restoreWarnings = allWarnings.Where(x => x.WarningType == DatabaseWarning.RestoreWarning && !x.IsAcknowledged);

			if (restoreWarnings.Any())
			{
				var builder = new ZStringBuilder();
				restoreWarnings.ForEach(x => builder.AppendLine(x.Description));
				builder.AppendLine(string.Empty);
				builder.AppendLine(Res.GetString("94CD2DA8-A543-4159-9354-8F07435D5E16", "Do you want to mark above warnings as 'Acknowledged' so that these warnings will not be reminded again?"));

				var caption = Res.GetString("A1FB03A9-6E67-4005-A205-77E3A308D0E3", "Database Restore Warning");

				if (DialogResult.Yes == Globals.Message.Show(builder.ToString(), caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes))
				{
					foreach (DbHealthWarningRegistryElement warning in registryValue)
					{
						if (warning.WarningType == DatabaseWarning.RestoreWarning && !warning.IsAcknowledged)
						{
							warning.IsAcknowledged = true;
						}
					}
					SystemDataRegistry.Instance.LastDbHealthCheckWarningList.SetValue(registryValue);
				}
			}
		}
	}
}

