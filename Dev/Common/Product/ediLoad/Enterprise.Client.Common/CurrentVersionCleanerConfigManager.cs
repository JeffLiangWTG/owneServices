using System;
using System.IO;
using System.Xml.Serialization;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;

namespace Enterprise.Client.Common
{
	public class CurrentVersionCleanerConfigManager : BaseAppManagerInvocable, ICurrentVersionCleanerConfigManager
	{
		public virtual ICurrentVersionCleanerConfigWithLogs LoadConfiguration(string baseInstallationPath)
		{
			Argument.NotNullOrEmpty(baseInstallationPath, nameof(baseInstallationPath));

			var configFile = Path.Combine(baseInstallationPath, CurrentVersionConfigFileName);

			try
			{
				if (File.Exists(configFile))
				{
					using (var reader = new StreamReader(configFile))
					{
						return (CurrentVersionCleanerConfig)new XmlSerializer(typeof(CurrentVersionCleanerConfig)).Deserialize(reader);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// ignored
			}

			return new CurrentVersionCleanerConfig
			{
				VersionInactiveDurationInDays = TimeSpan.FromDays(14),
				CurrentVersionCleanupIntervalInDays = TimeSpan.FromDays(1),
				LastStartTime = DateTime.MinValue,
				NextRuntime = DateTime.MinValue,
				LastSuccessTime = DateTime.MinValue,
				CurrentVersionFile = Path.Combine(baseInstallationPath, "CurrentVersion")
			};
		}

		public AppManagerResult SaveConfigurationViaAppManager(string baseInstallationPath, ICurrentVersionCleanerConfigWithLogs config)
		{
			Argument.NotNullOrEmpty(baseInstallationPath, nameof(baseInstallationPath));
			Argument.NotNull(config, nameof(config));

			installationPath = baseInstallationPath;

			using (var stringWriter = new StringWriter())
			{
				new XmlSerializer(typeof(CurrentVersionCleanerConfig)).Serialize(stringWriter, config);

				configXml = stringWriter.ToString();
				return ExecuteOrInvoke();
			}
		}
		protected override object[] GetState()
		{
			return new object[] { installationPath, configXml };
		}

		protected override void RestoreState(object[] state)
		{
			installationPath = (string)state[0];
			configXml = (string)state[1];
		}

		protected override void DoExecute()
		{
			var path = Path.Combine(installationPath, CurrentVersionConfigFileName);

			File.WriteAllText(path, configXml);
		}

		public const string CurrentVersionConfigFileName = "CurrentVersionConfig.xml";
		string installationPath;
		string configXml;
	}
}
