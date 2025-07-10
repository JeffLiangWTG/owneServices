using System.Collections.Generic;
using CargoWise.ApplicationManager.Common;

namespace CargoWise.Loader.Common.Testing
{
	public class MockConfiguration : Configuration
	{
		string applicationName;
		string[] args;
		List<string> unhandledArguments;
		IAppManager appManagerClient;

		public MockConfiguration()
		{
		}

		public MockConfiguration(string startupPathOverride)
		{
			StartupPathOverride = startupPathOverride;
		}

		public MockConfiguration(string startupPathOverride, string targetPath)
			: this(startupPathOverride)
		{
			TargetPath = targetPath;
		}

		public override string ApplicationName
		{
			get { return applicationName ?? base.ApplicationName; }
		}

		public override string CurrentPackage
		{
			get { return TargetPath; }
		}

		public new string TargetDirectoryName
		{
			get { return base.TargetDirectoryName; }
			set { base.TargetDirectoryName = value; }
		}

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

		public List<string> UnhandledArguments
		{
			get { return unhandledArguments ?? (unhandledArguments = new List<string>()); }
		}

		public string[] GetArgs()
		{
			return (args == null) ? null : (string[])args.Clone();
		}

		protected override void InitializeCore(string[] args)
		{
			this.args = args;
			base.InitializeCore(args);
		}

		protected override void ParseCommandLineArgument(string arg)
		{
			base.ParseCommandLineArgument(arg);
			UnhandledArguments.Add(arg);
		}

		public void SetApplicationName(string value)
		{
			applicationName = value;
		}

		public override IAppManager GetNewAppManagerClient()
		{
			return appManagerClient ?? new MockAppManager();
		}

		public void SetAppManagerClient(IAppManager value)
		{
			appManagerClient = value;
		}

		internal override string StartupPathFromLocation(string location)
		{
			LastLocation = location;
			return base.StartupPathFromLocation(location);
		}

		public string LastLocation;
	}
}
