using System;
using System.ServiceProcess;
using CargoWise.Loader.Common;
using JetBrains.Profiler.SelfApi;

namespace CargoWise.Main.Startup.Tools.JetBrains
{
	public class ProfilePerformanceModel : ProfileModel
	{
		static readonly String JetBrainsETWServiceName = "JetBrainsEtwHost.16";

		public static bool IsETWServiceRunning()
		{
			ServiceControllerStatus serviceControllerStatus;
			ServiceControllerProxy serviceControllerProxy = new ServiceControllerProxy();
			if (serviceControllerProxy.TryGetStatus(JetBrainsETWServiceName, out serviceControllerStatus))
			{
				return serviceControllerStatus == ServiceControllerStatus.Running;
			}
			else
			{
				return false;
			}
		}

		public ProfilePerformanceModel(IDialogService dialogService, ProfilingType profilingType)
		: base(dialogService)
		{
			this.profilingType = profilingType;
		}

		/// <summary>
		///		Things to keep in mind when you update the version:
		///
		///		1. Create a request for IS to upload a new file myaccount
		///		2. Don't rename the file, keep it as it is, i.e. with version. We need versioning on the client machine so that
		///		   we download and install a new version of tools if there is an old version there.
		///		3. Update license.txt file from the license file in the installation. Manually remove html tags so that the file is readable
		///		   as a plain text rather than as an html.
		/// </summary>
		protected override string PrerequisiteDownloadUrl =>
			"https://myaccount-portal.cargowise.com/my-account/public/downloads/JetBrains.dotTrace.CommandLineTools.windows-x64.2022.2.2.zip";

		protected override string PrerequisiteCheckSum => "/KlxbZVh18ewY7E1YlgNL+XFD/PTE2g/UTuCZVMZrcSZ9VeBKIr+AVMxXPP7EUAEuSWddLnnzdfB6fjfPFsVNA==";

		protected override string ProfilerName => "dotTrace";

		protected override bool DoStartProfiling()
		{
			var config = new DotTrace.Config();
			config.SaveToDir(SnapshotsPath);
			if (profilingType == ProfilingType.TIMELINE)
			{
				config.UseTimelineProfilingType();
			}

			var success = RunSafe(() => DotTrace.Attach(config), Res.GetString("23e4e32b-9d47-4581-87e2-5f4b0e25e46e", "Attaching to the process"));
			if (!success)
			{
				return false;
			}

			success = RunSafe(DotTrace.StartCollectingData, Res.GetString("a247996f-9cf7-434d-b1c1-3e852c43a1a4", "Starting the profiling session"));
			if (!success)
			{
				return false;
			}

			Log(Res.GetString("1ba09ea5-17ca-44ea-87b6-da5bfd6267c9", "Please reproduce the problem and click Stop to stop profiling. Then attach the created file(s) to the incident."));
			return true;
		}

		protected override bool DoStopProfiling()
		{
			var successfullyCollected = RunSafe(DotTrace.StopCollectingData, Res.GetString("18c9b328-f51e-4887-a961-13a189596b30", "Stopping the profiling session"));
			if (successfullyCollected)
			{
				successfullyCollected = RunSafe(DotTrace.SaveData, Res.GetString("4d167b98-08e8-4b4d-8030-bd8664c5aab2", "Saving the data"));
			}

			var successfullyDetached = RunSafe(DotTrace.Detach, Res.GetString("8e3f8d54-6cfd-45e5-b4b6-194832d355d3", "Detaching from the process"));

			return successfullyCollected && successfullyDetached;
		}

		protected override void EnsurePrerequisite()
		{
			DotTrace.EnsurePrerequisite(downloadTo: PrerequisitePath);
		}

		protected override void ShowForm()
		{
			DialogService.ProfilePerformance(this);
		}

		readonly ProfilingType profilingType;
	}

	public enum ProfilingType
	{
		SAMPLING,
		TIMELINE
	}
}
