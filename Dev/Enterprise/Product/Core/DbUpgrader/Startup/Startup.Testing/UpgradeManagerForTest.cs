using System.Collections.Generic;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class UpgradeManagerForTest : UpgradeManager
	{
		public UpgradeManagerForTest(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade, bool forceBiDatabaseUpgrade, bool overrideAcquireApplicationLockoutAndRun = false)
			: base(versionInfo, softwareUpgrade)
		{
			this.forceBiDatabaseUpgrade = forceBiDatabaseUpgrade;
			this.overrideRunOfflineUpgrade = overrideAcquireApplicationLockoutAndRun;
			this.OverrideShouldAcquireLockoutInTransaction = false;
			DisableRefreshDependentScripts_ForTest = true;
		}

		public bool? BiDisableChangeDataCapture_OverrideValueForTest;

		protected override bool IsCdcDisabledOnRegistry()
		{
			if (BiDisableChangeDataCapture_OverrideValueForTest.HasValue)
			{
				return BiDisableChangeDataCapture_OverrideValueForTest.Value;
			}
			else
			{
				return base.IsCdcDisabledOnRegistry();
			}
		}

		internal override bool GetConfirmationToKillOtherDbConnections()
		{
			if (UserChooseToKillOtherDbConnections_ForTest)
			{
				return GetConfirmationToDisconnectUsers(new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("test", "test") });
			}
			if (UserHasCancelledToKillOtherDbConnections_ForTest)
			{
				return false;
			}

			return base.GetConfirmationToKillOtherDbConnections();
		}

		internal bool UserHasCancelledToKillOtherDbConnections_ForTest;

		internal bool UserChooseToKillOtherDbConnections_ForTest;

		public void InitialiseRequiredUpgraders_Exposed()
		{
			InitialiseRequiredUpgraders();
		}

		public UpgradeActionList OfflineActionList_Exposed
		{
			get
			{
				return offlineActionList;
			}
		}

		protected override bool ForceBiDatabaseUpgrade
		{
			get { return forceBiDatabaseUpgrade; }
		}
		readonly bool forceBiDatabaseUpgrade;

		internal override void RunOfflineUpgrade()
		{
			if (!overrideRunOfflineUpgrade)
			{
				base.RunOfflineUpgrade();
			}
		}
		readonly bool overrideRunOfflineUpgrade;

		protected override void DoNonTransactionalStepsAfterFirstMainDbLockout()
		{
			if (!override_DoNonTransactionalStepsAfterFirstMainDbLockout)
			{
				base.DoNonTransactionalStepsAfterFirstMainDbLockout();
			}
		}
		public bool override_DoNonTransactionalStepsAfterFirstMainDbLockout;

		internal protected override bool ShouldAcquireLockoutInTransaction()
		{
			if (OverrideShouldAcquireLockoutInTransaction.HasValue)
			{
				return OverrideShouldAcquireLockoutInTransaction.Value;
			}
			return base.ShouldAcquireLockoutInTransaction();
		}

		internal bool? OverrideShouldAcquireLockoutInTransaction { get; set; }

		protected override bool EnsureExclusiveLockOfResourcesIfRequired()
		{
			if (EnsureExclusiveLockOfResourcesIfRequired_ReturnValueForTest.HasValue)
			{
				return EnsureExclusiveLockOfResourcesIfRequired_ReturnValueForTest.Value;
			}
			return base.EnsureExclusiveLockOfResourcesIfRequired();
		}
		public bool? EnsureExclusiveLockOfResourcesIfRequired_ReturnValueForTest;

		protected override void KillUserConnections()
		{
			if (!override_KillUserConnections)
			{
				base.KillUserConnections();
			}
		}
		public bool override_KillUserConnections;

		protected override void CleanupAfterFirstKillUserConnections()
		{
			if (!override_KillUserConnections)
			{
				base.CleanupAfterFirstKillUserConnections();
			}
		}

		protected override void DoOfflinePreparationBeforeMainTransaction()
		{
			if (!override_DoOfflinePreparationBeforeMainTransaction)
			{
				base.DoOfflinePreparationBeforeMainTransaction();
			}
		}

		public bool override_DoOfflinePreparationBeforeMainTransaction;

		public IEnumerable<string> GetAllDatabases()
		{
			return base.AllDatabases;
		}

		protected override IUpgradePreparation CreateUpgradePreparation()
			=> MockUpgradePreparer ?? base.CreateUpgradePreparation();
		public IUpgradePreparation MockUpgradePreparer { get; set; }

		public BaseUpgrader OverrideAssembliesUpgrader { get; set; }
		protected override BaseUpgrader GetAssembliesUpgrader()
			=> OverrideAssembliesUpgrader ?? base.GetAssembliesUpgrader();
	}
}
