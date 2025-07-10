using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;

namespace CargoWise.Loader.Client.Testing
{
	public class MockClientConfiguration : ClientConfiguration
	{
		IAppManager appManagerClient;
		string applicationName;
		string programArguments;
		string programFileName;

		public MockClientConfiguration()
		{
		}

		public MockClientConfiguration(string startupPathOverride)
		{
			StartupPathOverride = startupPathOverride;
		}

		public MockClientConfiguration(string startupPathOverride, string targetPath)
			: this(startupPathOverride)
		{
			TargetPath = targetPath;
		}

		public override string CurrentPackage
		{
			get { return TargetPath; }
		}

		public override string ApplicationName
		{
			get
			{
				return applicationName;
			}
		}

		public string FileListResourceContents { get; set; }

		public override IAppManager GetNewAppManagerClient()
		{
			return appManagerClient ?? new MockAppManager();
		}

		public override string ProgramFileName
		{
			get { return programFileName; }
		}

		public override string ProgramArguments
		{
			get { return programArguments ?? base.ProgramArguments; }
		}

		public bool ReturnRealAppManagerClient { get; set; }

		public new string TargetPath
		{
			get { return base.TargetPath; }
			set { BaseTargetPath = value; }
		}

		public new UILevel UILevel
		{
			get { return base.UILevel; }
			set { base.UILevel = value; }
		}

		public void SetApplicationName(string value)
		{
			applicationName = value;
		}

		public void SetAppManagerClient(IAppManager value)
		{
			appManagerClient = value;
		}

		public void SetProgramArguments(string value)
		{
			programArguments = value;
		}

		public void SetProgramFileName(string value)
		{
			programFileName = value;
		}
	}
}
