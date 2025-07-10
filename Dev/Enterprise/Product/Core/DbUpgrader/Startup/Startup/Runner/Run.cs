using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Startup
{
	/// <summary>
	/// This class is the entry point of DbUpgrader.
	/// The Upgrader class constructor is internal to avoid direct use.
	/// </summary>
	public class Run : IDbUpgraderRunner
	{
		/// <summary>
		/// Performs
		///   - Full Upgrade
		///   - Using UpgraderForm (NOT Silent)
		/// </summary>
		/// <returns>Succeded ? true : false</returns>
		public ValidationResponse FullUpgrade(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade, UpgraderEvent upgradeEventHandler, Func<BaseUpgradeManager, bool> runUpgradeGui)
		{
			var result = new ValidationResponse();
			UpgradeManager guiUpgrader = new UpgradeManager(versionInfo, softwareUpgrade);
			if (upgradeEventHandler != null)
			{
				guiUpgrader.UpgradeEvent += upgradeEventHandler;
			}
			using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
			{
				result.Successful = runUpgradeGui(guiUpgrader);
				return result;
			}
		}

		/// <summary>
		/// Performs
		///   - Full Upgrade
		///   - Silent
		/// </summary>
		/// <returns>Succeded ? true : false</returns>
		public ValidationResponse FullSilentUpgrade(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade, UpgraderEvent upgradeEventHandler)
		{
			var silentUpgrader = new UnattendedUpgradeManager(versionInfo, softwareUpgrade);
			silentUpgrader.UpgradeEvent += upgradeEventHandler;
			using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
			{
				return silentUpgrader.Run();
			}
		}

		/// <summary>
		/// Performs
		///   - SCHEMA & SCRIPT only Upgrade
		///   - Silent
		///   - Forcing Upgrade even if Version is the same
		/// </summary>
		/// <returns>Succeded ? true : false</returns>
		public static bool ForceSchemaScriptSilentUpgrade(UpgraderEvent upgradeEventHandler)
		{
			return new Run().FullSilentUpgrade(new ForceSchemaScriptUpgradeVersionInfo(), null, upgradeEventHandler).Successful;
		}

		/// <summary>
		/// Performs
		///   - SCHEMA, SCRIPT & DATA only Upgrade
		///   - Silent
		///   - Forcing Upgrade even if Version is the same
		/// </summary>
		/// <returns>Succeded ? true : false</returns>
		public static bool ForceSchemaScriptDataSilentUpgrade(UpgraderEvent upgradeEventHandler)
		{
			return new Run().FullSilentUpgrade(new ForceSchemaScriptDataUpgradeVersionInfo(), null, upgradeEventHandler).Successful;
		}

		public void RemoveOldUpgradePackages()
		{
			SoftwareUpgrader.Instance.RemoveOldUpgradePackages();
		}
	}
}
