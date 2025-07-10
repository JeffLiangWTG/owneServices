using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.AutoDeploy.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class HttpPackageSender
	{
		public HttpPackageSender(string licenceEnterpriseCode, string packagePath, ILogger logger, params UpgradesToClient[] upgradeRequests)
		{
			if (upgradeRequests == null || upgradeRequests.Length == 0)
			{
				throw new ArgumentException("upgradeRequests cannot be null or empty.");
			}
			this.LicenceEnterpriseCode = licenceEnterpriseCode;
			this.PackagePath = packagePath;
			this.UpgradeRequests = upgradeRequests;
			this.logger = logger;
		}

		public HttpPackageSender(string licenceEnterpriseCode, string packagePath, params UpgradesToClient[] upgradeRequests)
			: this(licenceEnterpriseCode, packagePath, null, upgradeRequests)
		{
		}

		readonly string LicenceEnterpriseCode;
		readonly UpgradesToClient[] UpgradeRequests;
		readonly string PackagePath;
		readonly ILogger logger;
		IPackagePublishService packagePublish;

		IPackagePublishService PackagePublish
		{
			get { return packagePublish ?? (packagePublish = new PackagePublishService()); }
		}

		internal void SetPackagePublishForTest(IPackagePublishService service)
		{
			this.packagePublish = service;
		}

		public virtual bool Run()
		{
			var result = false;
			string targetDirectory = GetTargetDirectory();
			string packageFileName = Path.GetFileName(PackagePath);
			if (PackagePublish.IsPackageAlreadyPublished(packageFileName, LicenceEnterpriseCode) == Customs.Business.TriState.True
					|| PackagePublish.PublishPackage(PackagePath, targetDirectory))
			{
				SendMessageToClient();
				result = true;
			}
			else
			{
				HandleError();
			}
			return result;
		}

		void HandleError()
		{
			if (logger != null)
			{
				logger.Log(LogType.Error, PackagePublish.LastErrorMessage);
			}
		}

		protected void SendMessageToClient()
		{
			var upgradeRequestsWithDownloadOptimization = new List<UpgradesToClient>();
			var upgradeRequests = new List<UpgradesToClient>();

			foreach (var upgradesToClient in UpgradeRequests)
			{
				if (upgradesToClient.LicDatabase.LD_EnablePackageDownloadOptimization && upgradesToClient.Build.HL_IsRolledOut)
				{
					upgradeRequestsWithDownloadOptimization.Add(upgradesToClient);
				}
				else
				{
					upgradeRequests.Add(upgradesToClient);
				}
			}

			if (upgradeRequestsWithDownloadOptimization.Count > 0)
			{
				SendEmailAndEdiMessage(upgradeRequestsWithDownloadOptimization.ToArray(), GetVersionInfo(Path.GetFileName(PackagePath), true));
			}

			if (upgradeRequests.Count > 0)
			{
				SendEmailAndEdiMessage(upgradeRequests.ToArray(), GetVersionInfo(Path.GetFileName(PackagePath)));
			}
		}

		void SendEmailAndEdiMessage(UpgradesToClient[] upgradeRequests, EdiPackageDownloadInfo info)
		{
			var notificationEnabledDatabases = upgradeRequests.Where(x => x.L1_NotifyUser).ToArray();

			string[] emailRecipients = notificationEnabledDatabases.Where(x => x.LicDatabase != null && x.LicDatabase.VersionCanReceiveAllSystemMessages != Customs.Business.TriState.True)
				.Select(x => (string)x.LicDatabase.LD_PublicEmailAddressForUpdate)
				.ToArray();

			BusinessObjectFactory factory = new BusinessObjectFactory() { RefreshEnabled = false };

			if (emailRecipients.Length > 0)
			{
				var email = info.GetVersionInfoEmail(emailRecipients);
				Env.OutgoingMailManager.Create(factory, email);
			}

			foreach (var request in notificationEnabledDatabases.Where(x => x.LicDatabase != null && x.LicDatabase.VersionCanReceiveAllSystemMessages != Customs.Business.TriState.False))
			{
				var licenceCode = request.LicDatabase.LicenceCodeForSystemMessage;
				var xml = info.BuildXmlFragment(SystemMessageList.Descriptions.UpgradeDownload);
				var messageCreator = ObjectFactory.Get<IOutgoingSystemMessage>();
				messageCreator.Create(factory, xml, licenceCode);
			}

			factory.Save();
		}

		public EdiPackageDownloadInfo GetVersionInfo(string fileName, bool enableDownloadOptimization = false)
		{
			if (enableDownloadOptimization)
			{
				return new EdiPackageDownloadInfo(fileName, "The package has been rolled out to CargoWise Cloud", ZBool.False, string.Empty);
			}

			string targetUrl = PackagePathGenerator.GenerateHttpPath(LicenceEnterpriseCode, fileName);
			return new EdiPackageDownloadInfo(fileName, "Deployed via Web", ZBool.True, targetUrl);
		}

		public virtual string GetTargetDirectory()
		{
			return PackagePathGenerator.GenerateWebServerRootPath(LicenceEnterpriseCode);
		}
	}
}
