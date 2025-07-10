using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	partial class SharedAvailabilityGroupRefDbPreparationStrategy : SharedDatabasePreparationStrategy
	{
		public SharedAvailabilityGroupRefDbPreparationStrategy(IUpgradeContext context, AdminConnection upgConnection, string mainDbName, RefDbTypeEnum refDbType, string refDbCountry, int versionToUpgradeTo, string availabilityGroupName)
			: base(context, mainDbName, refDbType, refDbCountry, upgConnection, versionToUpgradeTo)
		{
			Argument.NotNullOrEmpty(mainDbName, nameof(mainDbName));

			if (string.IsNullOrWhiteSpace(availabilityGroupName))
			{
				throw new NotSupportedException("To use this strategy, the main database must participate in AlwaysOn Availability Group");
			}
			AvailabilityGroupName = availabilityGroupName;
		}

		protected override string SharedRefDbPrefix => RefDbTableNameResolver.SharedAvailabilityGroupRefDbPrefix;
		protected override string VersionSharedRefDbPrefix => RefDbTableNameResolver.GetSharedAvailabilityGroupRefDbPrefix(AvailabilityGroupName, refDbType, refDbCountry);

		protected override (string DbName, int DbVersion) GetBaseDatabaseForUpgrade(int minimumVersion)
		{
			var sharedAGDatabase = GetBaseDatabaseForUpgradeByPrefix(VersionSharedRefDbPrefix, minimumVersion);
			if (minimumVersion < sharedAGDatabase.DbVersion)
			{
				minimumVersion = sharedAGDatabase.DbVersion;
			}

			var sharedDatabase = GetBaseDatabaseForUpgradeByPrefix(base.VersionSharedRefDbPrefix, minimumVersion);

			return (sharedDatabase.DbVersion > sharedAGDatabase.DbVersion)
				? sharedDatabase
				: sharedAGDatabase;
		}

		#region Implementation

		string AvailabilityGroupName { get; }

		#endregion // Implementation
	}
}

#region Test
#if DEBUG

#region Partial class

namespace Enterprise.DbUpgrader.ReferenceDatabases
{
	partial class SharedAvailabilityGroupRefDbPreparationStrategy
	{
		public SharedAvailabilityGroupRefDbPreparationStrategy(IUpgradeContext context, AdminConnection upgConnection, string mainDbName, string groupName, RefDbTypeEnum refDbType, string refDbCountry, int versionToUpgradeTo)
			: base(context, mainDbName, refDbType, refDbCountry, upgConnection, versionToUpgradeTo)
		{
			Argument.NotNullOrEmpty(mainDbName, nameof(mainDbName));
			Argument.NotNullOrEmpty(groupName, nameof(groupName));

			AvailabilityGroupName = groupName;
		}
	}
}

#endregion // Partial class
#endif
#endregion
