using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Data
{
	public class ClientDocumentsUpgrader : BaseUpgrader
	{
		public ClientDocumentsUpgrader(IUpgradeManager manager, DbConnection upgConnection, VersionLabel versionBeforeUpgrade, UpgradeInfo softwareUpgrade)
			: base(manager, upgConnection, versionBeforeUpgrade)
		{
			this.softwareUpgrade = softwareUpgrade;
			latestVersion = -1;
		}

		readonly UpgradeInfo softwareUpgrade;
		readonly int latestVersion;

		internal ClientDocumentsUpgradeTask ClientDocumentsUpgradeTask { get; private set; }

		public void FetchAndExtractDocuments()
		{
			using var tempDirectory = new TempDirectory();
			var tempFile = Temp.GetTempFileName();
			try
			{
				var upgradeManager = MakeUpgradeManager(upgConnection);
				Manager.ShowInfoMessage("Downloading client documents");
				upgradeManager.DownloadUpgradePackageFile(softwareUpgrade.PK, tempFile);
				Manager.ShowInfoMessage("Extracting client documents");
				EdpFile.Unpack(tempFile, tempDirectory.DirectoryName, ".*documents\\.xml$");
			}
			finally
			{
				File.Delete(tempFile);
			}

			var clientDocumentsFileName = DbRegistry.ClientDocumentName.LoadValue(upgConnection) + "Documents.xml";
			var clientDocumentsFullPath = Path.Combine(tempDirectory.DirectoryName, clientDocumentsFileName);

			ClientDocumentsUpgradeTask = new ClientDocumentsUpgradeTask(clientDocumentsFullPath);

			Manager.ShowInfoMessage("Loading client documents");
			_ = ClientDocumentsUpgradeTask.ResourceFile.DataSet;
		}

		protected override void DoUpgrade()
		{
			if (ClientDocumentsUpgradeTask == null)
			{
				ReportError("Failed to upgrade client documents because the documents are not loaded");
				return;
			}

			Manager.ShowInfoMessage("Applying client documents upgrade");
			ClientDocumentsUpgradeTask.Run();
		}

		protected internal virtual void ReportError(string errorMessage)
		{
			ErrorReporter.ReportOnce(errorMessage);
		}

		protected internal virtual UpgradeManager MakeUpgradeManager(DbConnection connection)
		{
			var sqlConnection = (SqlConnection)((IDbConnectionInternals)connection).InternalDbConnection;
			var sqlTransaction = (SqlTransaction)((IDbConnectionInternals)connection).InternalDbTransaction;

			return new SqlUpgradeManager(new UpgradeSqlContext(sqlConnection, sqlConnection.DataSource, sqlConnection.Database, sqlTransaction));
		}

		public override int EstimatedNumberOfTasks
		{
			get { return 4; }
		}

		protected override VersionLabel LatestVersion
		{
			get
			{
				return new VersionLabel(latestVersion, 0);
			}
		}

		public override string Name
		{
			get { return "Client Documents and Reports Upgrade"; }
		}

		public override IEnumerable<string> SecondaryDatabasesToUpgrade
		{
			get { return Array.Empty<string>(); }
		}

		public override bool RequiresApplicationLockout
		{
			get
			{
				return false;
			}
		}
	}
}
