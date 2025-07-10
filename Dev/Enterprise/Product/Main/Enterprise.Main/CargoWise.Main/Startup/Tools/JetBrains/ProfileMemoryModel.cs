using JetBrains.Profiler.SelfApi;

namespace CargoWise.Main.Startup.Tools.JetBrains
{
	public class ProfileMemoryModel : ProfileModel
	{
		public ProfileMemoryModel(IDialogService dialogService) : base(dialogService)
		{
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
			"https://myaccount-portal.cargowise.com/my-account/public/downloads/JetBrains.dotMemory.Console.windows-x64.2022.2.2.zip";

		protected override string PrerequisiteCheckSum => "49MOMEfKmWvoMb+cyfxC2vAWDjWIwMQNx99YPUeyWxcl5cP9b5g41VFEcuHzI931z/XkGKru4d16rmHsr1BiWg==";

		protected override string ProfilerName => "dotMemory";

		protected override bool DoStartProfiling()
		{
			var config = new DotMemory.Config();
			config.SaveToDir(SnapshotsPath);
			config.UseLogLevelTrace();

			var success = RunSafe(() => DotMemory.Attach(config), Res.GetString("23e4e32b-9d47-4581-87e2-5f4b0e25e46e", "Attaching to the process"));
			if (success)
			{
				Log(Res.GetString("ce1bddd1-e0a5-4587-9e31-e57f9419797f", "Please reproduce the issue and make snapshot(s). Click Finish when you are done and attach the created file(s) to the incident."));
			}

			return success;
		}

		protected override bool DoStopProfiling()
		{
			return RunSafe(() => DotMemory.Detach(), Res.GetString("8e3f8d54-6cfd-45e5-b4b6-194832d355d3", "Detaching from the process"));
		}

		public void TakeSnapshot()
		{
			RunSafe(() => DotMemory.GetSnapshot(), Res.GetString("109ed2d3-7fb7-418e-9803-7de8c666b0a6", "Taking snapshot"));
		}

		protected override void EnsurePrerequisite()
		{
			DotMemory.EnsurePrerequisite(downloadTo: PrerequisitePath);
		}

		protected override void ShowForm()
		{
			DialogService.ProfileMemory(this);
		}
	}
}
