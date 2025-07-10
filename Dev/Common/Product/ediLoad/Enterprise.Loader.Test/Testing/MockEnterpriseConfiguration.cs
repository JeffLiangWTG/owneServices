using System;
using System.IO;
using System.Reflection;
using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using Enterprise.Upgrades;

namespace Enterprise.Loader.Testing
{
	sealed class MockEnterpriseConfiguration : EnterpriseConfiguration
	{
		IAppManager appManager;
		public UpgradeManager MockUgradeManager { get; set; }

		protected override void InitializeCore(string[] args)
		{
			base.InitializeCore(args);
			TargetVersion = new Version(18, 1, 1, 1);
		}

		public new string LegacyInstanceName
		{
			get { return base.LegacyInstanceName; }
			set { base.LegacyInstanceName = value; }
		}

		public new string TargetDirectoryName
		{
			get { return base.TargetDirectoryName; }
			set { base.TargetDirectoryName = value; }
		}

		public new UILevel UILevel
		{
			get { return base.UILevel; }
			set { base.UILevel = value; }
		}

		public override string CurrentPackage
		{
			get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
		}

		public override IAppManager GetNewAppManagerClient()
		{
			return appManager ?? new MockAppManager();
		}

		public override UpgradeManager NewUpgradeManager()
		{
			return MockUgradeManager ?? base.NewUpgradeManager();
		}

		public void SetAppManagerClient(IAppManager value)
		{
			appManager = value;
		}
	}
}
