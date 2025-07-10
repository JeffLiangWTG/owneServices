using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Assemblies
{
	public sealed class AssembliesUpgrader : BaseUpgrader
	{
		internal AssembliesUpgrader(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade, params DatabaseAssembliesUpgrader[] databaseAssembliesUpgraders)
			: base(manager, upgConnection, versionBeforeUpgrade)
		{
			this.databaseAssembliesUpgraders = databaseAssembliesUpgraders;
		}

		public AssembliesUpgrader(IUpgradeManager manager, DbConnection upgConnection, DbConnection auditConnection, DbConnection dataWarehouseConnection, VersionLabel versionBeforeUpgrade)
			: this(manager, upgConnection, versionBeforeUpgrade,
				new MainDbAssembliesUpgrader(manager, upgConnection, versionBeforeUpgrade),
				new AuditDatabaseAssembliesUpgrader(manager, upgConnection, auditConnection, versionBeforeUpgrade),
				new EdwDatabaseAssembliesUpgrader(manager, upgConnection, dataWarehouseConnection, versionBeforeUpgrade))
		{
		}

		public override bool IsUpgradeRequired => base.IsUpgradeRequired && (versionBeforeUpgrade.CompareTo(LatestVersion) != 0 || databaseAssembliesUpgraders.Any(upgrader => upgrader.IsUpgradeRequired));

		public override int EstimatedNumberOfTasks => 3 + databaseAssembliesUpgraders.Sum(upgrader => upgrader.EstimatedNumberOfTasks);

		protected override VersionLabel LatestVersion => SqlClrAssembliesVersion.Application;

		public override string Name => "SQL Assemblies Upgrade";

		public override IEnumerable<string> SecondaryDatabasesToUpgrade => Array.Empty<string>();

		public override bool RequiresApplicationLockout => false;

		protected override void DoUpgrade()
		{
			foreach (var databaseAssembliesUpgrader in databaseAssembliesUpgraders.Where(upgrader => upgrader.IsUpgradeRequired))
			{
				databaseAssembliesUpgrader.RunUpgrade();
			}

			UpdateDataVersion();
		}

		void UpdateDataVersion()
		{
			DbRegistry.DatabaseMajorClrAssembliesVersion.SaveValue(LatestVersion.Major, upgConnection);
			DbRegistry.DatabaseMinorClrAssembliesVersion.SaveValue(LatestVersion.Minor, upgConnection);
		}

		readonly DatabaseAssembliesUpgrader[] databaseAssembliesUpgraders;
	}
}