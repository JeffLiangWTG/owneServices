using System.Linq;
using System.Text;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public abstract class WindowsInstallerAppManagerInvoker : AppManagerInvoker, IAppManagerInvocable
	{
		protected WindowsInstallerAppManagerInvoker(Installation installation)
			: base(installation)
		{
		}

		protected abstract string Arguments { get; }
		public abstract string InstallerAbsolutePath { get; }
		protected abstract string NameOfComponentBeingInstalled { get; }
		protected virtual bool WaitForInstallerToComplete => true;

		public static AppManagerResult GetResult(InstallationResultCollection results)
		{
			Argument.NotNull(results, nameof(results));
			StringBuilder sb = new StringBuilder();
			AppManagerResultStatus status = AppManagerResultStatus.Success;
			foreach (InstallationResult result in results)
			{
				if (result.IsError)
				{
					status = AppManagerResultStatus.Error;
				}
				if (!string.IsNullOrEmpty(result.Message))
				{
					if (sb.Length > 0)
					{
						sb.AppendLine();
						sb.AppendLine();
					}
					sb.Append(result.Message);
				}
			}
			return new AppManagerResult(status, sb.ToString());
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			ChangeCurrentTaskDescription(TaskDescription);
			var type = GetType();
			InstallationResult result = InvokeAppManager(type, GetAppManagerState(), CreateMutexRequest());
			return result;
		}

		protected virtual MutexRequest CreateMutexRequest()
		{
			return MutexRequest.Create(NameOfComponentBeingInstalled);
		}

		protected virtual string TaskDescription
		{
			get { return "Installing " + NameOfComponentBeingInstalled; }
		}

		protected abstract Configuration GetNewConfiguration();

		protected InstallationResultCollection InvokeInstaller(string fullPathOfProgramToRun)
		{
			Argument.NotNullOrEmpty(fullPathOfProgramToRun, nameof(fullPathOfProgramToRun));
			var installation = new Installation(GetNewConfiguration());
			var container = new InstallationItemContainer(installation);
			var installer = CreateWindowsInstallerProgram(container, NameOfComponentBeingInstalled);
			installer.SetFullPathOfProgramToRun(fullPathOfProgramToRun);
			installer.Arguments = Arguments;
			installer.CreateNoWindow = true;
			installer.SilentlyIgnoreRebootRequired = true;
			var result = new InstallationResultCollection();
			installer.Install(result);
			ProcessResult(ref result);
			return result;
		}

		protected virtual WindowsInstallerProgram CreateWindowsInstallerProgram(InstallationItem parentInstallationItem, string nameOfComponent)
		{
			Argument.NotNull(parentInstallationItem, nameof(parentInstallationItem));

			return new WindowsInstallerProgram(parentInstallationItem, nameOfComponent, WaitForInstallerToComplete);
		}

		protected virtual void ProcessResult(ref InstallationResultCollection result)
		{
			Argument.NotNull(result, nameof(result));
		}

		protected string fullPathOfProgramToRun;

		protected virtual object GetAppManagerState()
		{
			return InstallerAbsolutePath;
		}

		protected virtual void SetAppManagerState(object state)
		{
			fullPathOfProgramToRun = (string)state;
		}

		#region IAppManagerInvocable Members

		AppManagerResult IAppManagerInvocable.Invoke(bool waitedForMutex, object state)
		{
			if (!IsStateValid(state)) //Can't be a contract as it implements a more generic interface.
			{
				return new AppManagerResult(AppManagerResultStatus.Error, "A null or empty string was passed to WindowsInstallerAppManagerInvoker.Invoke");
			}
			SetAppManagerState(state);
			return GetResult(InvokeInstaller(fullPathOfProgramToRun));
		}

		static public bool IsStateValid(object state)
		{
			var str = state as string;
			if (str != null)
			{
				return !string.IsNullOrEmpty(str);
			}

			var strArray = state as string[];
			return strArray != null && strArray.All(currStr => !string.IsNullOrEmpty(currStr));
		}

		#endregion
	}
}
