using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbUpgrader.Startup
{
	public class SoftwareUpgrader
	{
		public static SoftwareUpgrader Instance
		{
			get { return OverridableInstance.Value; }
		}
		internal static Overridable<SoftwareUpgrader> OverridableInstance = new Overridable<SoftwareUpgrader>(new SoftwareUpgrader());

		public delegate void ErrorHandler(string errorMessage);

		internal void CommitSoftwareUpgradeInDatabase(UpgradeInfo softwareUpgrade, IUpgradeManager parent)
		{
			if (parent != null)
			{
				parent.StartTask("Committing software upgrade");
			}
			Enterprise.Upgrades.UpgradeManager upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
			upgradeManager.SetAsCurrentVersion(softwareUpgrade, Env.LoginController.UpgradeLogonStaffCode ?? User.ServiceUserCode, Env.LoginController.UpgradeLogonName ?? "System Upgrade Service");
		}

#if DEBUG
		virtual
#endif
		public void RunCurrentVersionWriter(UpgradeInfo softwareUpgrade, IUpgradeManager parent)
		{
			Enterprise.Upgrades.UpgradeManager upgradeManager = UpgradeManagerFactory.NewUpgradeManager();
			bool retry;

			do
			{
				retry = false;
				try
				{
					upgradeManager.RunCurrentVersionWriter(Path.Combine(InstallationEnvironment.Instance.BaseInstallPath, softwareUpgrade.Version.ToString()));
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (parent != null)
					{
						retry = parent.ShowErrorWithRetry("Error completing local installation:\r\n" + ex.Message);
						if (!retry)
						{
							throw;
						}
					}
				}
			}
			while (retry);
		}

		public void RemoveOldUpgradePackages()
		{
			const string pksSql =
				@"select SZ_PK from dbo.StmUpgrade 
				  where SZ_Status in ('APL', 'DEL', 'NAP', 'OBS')
				  and SZ_PK not in (select top 2 SZ_PK from dbo.StmUpgrade where SZ_Status = 'APL' order by SZ_StatusTime desc)";

			List<Guid> pks = new List<Guid>();
			using (var reader = Db.Connection.Command(pksSql).ExecuteReader())
			{
				while (reader.Read())
				{
					pks.Add((Guid)reader[0]);
				}
			}

			const string deleteSql =
				@"DELETE dbo.StmUpgrade WITH (READPAST, READCOMMITTEDLOCK) WHERE SZ_PK = @Pk";

			foreach (var pk in pks)
			{
				using (var cmd = Db.Connection.Command(deleteSql, 300))
				{
					cmd.AddParameter("@Pk", SqlDbType.UniqueIdentifier, pk);
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}
