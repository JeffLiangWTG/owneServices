using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Data;
using CargoWise.Shared;
using NUnit.Framework;
using WTG.DevTools.Configuration.Networking;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Web.Shared.Test
{
	#pragma warning disable CA1813 //Supressed the warning as another class inherits from this class
	public class WithEnterpriseTestWebApplicationAttribute : TestSetupAttribute
	#pragma warning restore CA1813
	{
		public static WithEnterpriseTestWebApplicationAttribute TestWebApplication => Properties[ApplicationNameValue];

		public WithEnterpriseTestWebApplicationAttribute(string rapidFailProtectionInterval = "00:05:00", int rapidFailProtectionMaxCrashes = 5)
		{
			this.rapidFailProtectionInterval = rapidFailProtectionInterval;
			this.rapidFailProtectionMaxCrashes = rapidFailProtectionMaxCrashes;
		}

		public override void SetUp(TestCase testCase)
		{
			DeploymentPath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
				"WiseTech Global",
				"CargoWiseOneWeb",
				ApplicationName,
				Guid.NewGuid().ToString());
			Directory.CreateDirectory(DeploymentPath);

			DeployProjectFiles();
			DeployAssemblyFiles();

			InitializeWebConfig();
			SetupApplication();

			Properties[ApplicationName] = this;

			void DeployProjectFiles()
			{
				var appConfig = ConfigurationManager.OpenExeConfiguration(testCase.TestAssembly);
				var projectFilePath = Path.GetFullPath(appConfig.AppSettings.Settings[ApplicationName].Value);
				if (!string.IsNullOrWhiteSpace(projectFilePath))
				{
					var projectFileDir = Path.GetDirectoryName(projectFilePath);
					var webSiteFiles = ProjectFileParser.GetDeployableFiles(projectFilePath);
					foreach (var webSiteFile in webSiteFiles)
					{
						var sourceFile = Path.Combine(projectFileDir, webSiteFile);
						var destinationFile = Path.Combine(DeploymentPath, webSiteFile);
						var destinationDir = Path.GetDirectoryName(destinationFile);
						if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
						{
							Directory.CreateDirectory(destinationDir);
						}

						if (File.Exists(sourceFile) && !File.Exists(destinationFile))
						{
							File.Copy(sourceFile, destinationFile);
						}
					}
				}
			}

			void DeployAssemblyFiles()
			{
				var binDir = Path.Combine(DeploymentPath, "bin");
				if (!string.IsNullOrEmpty(binDir) && !Directory.Exists(binDir))
				{
					Directory.CreateDirectory(binDir);
				}

				foreach (var assemblyFileName in GetAssembliesListFromAppConfig(testCase.TestAssembly))
				{
					var sourceFile = Path.Combine(TestContext.CurrentContext.TestDirectory, RelativeBuildPath, assemblyFileName);
					var destinationFile = Path.Combine(binDir, assemblyFileName);
					if (File.Exists(sourceFile) && !File.Exists(destinationFile))
					{
						File.Copy(sourceFile, destinationFile);
					}
				}
			}

			IEnumerable<string> GetAssembliesListFromAppConfig(string assemblyFilePath)
			{
				var appConfig = ConfigurationManager.OpenExeConfiguration(assemblyFilePath);
				var xmlDocument = XDocument.Load(appConfig.FilePath);
				var assembliesPath = $"/configuration/{ApplicationName}/assemblies/add";
				var assemblies = new HashSet<string>
				(
					from element in xmlDocument.XPathSelectElements(assembliesPath)
					select element?.Attribute("key")?.Value
				);

				return assemblies;
			}
		}

		public override void TearDown(TestCase testCase)
		{
			Properties.TryRemove(ApplicationName, out _);

			WebApplicationUninstaller.ExecuteOrInvoke(SiteName, appPoolName);

			if (!string.IsNullOrEmpty(DeploymentPath))
			{
				FileIO.DeleteDirectory(DeploymentPath);
			}
		}

		public void StopAppPool()
		{
			WebApplicationStartStopAppPool.ExecuteOrInvoke(SiteName, appPoolName, WebApplicationStartStopAppPool.ActionStop);
		}

		public void StartAppPool()
		{
			WebApplicationStartStopAppPool.ExecuteOrInvoke(SiteName, appPoolName, WebApplicationStartStopAppPool.ActionStart);
		}

		void SetupApplication()
		{
			port = PortAuthority.GetRandomPort();
			tlsPort = TlsCertificateHash is null ? 0 : port + 1;
			WebApplicationInstaller
				.ExecuteOrInvoke(
					DeploymentPath,
					port,
					tlsPort,
					TlsCertificateHash,
					SiteName,
					appPoolName,
					EnableWindowsAuthentication,
					rapidFailProtectionInterval,
					rapidFailProtectionMaxCrashes);
		}

		public string GetUrl(string path, string query = "")
		{
			return GetHttpUri(path, query).AbsoluteUri;
		}

		public virtual Uri GetHttpUri(string path, string query = "")
		{
			return new UriBuilder("http", ServerName, port, path) { Query = query }.Uri;
		}

		public virtual Uri GetHttpsUri(string path, string query = "")
		{
			return TlsCertificateHash != null
				? new UriBuilder("https", ServerName, tlsPort, path) { Query = query }.Uri
				: throw new NotSupportedException($"Set '{nameof(TlsCertificateHash)}' to support HTTPS for the deployment.");
		}

		public virtual string SiteName { get; } = Guid.NewGuid().ToString();

		public virtual string ServerName => "localhost";

		protected virtual string RelativeBuildPath => "";

		protected virtual string ApplicationName => ApplicationNameValue;

		void InitializeWebConfig()
		{
			UpdateWebConfig(InitializeWebConfig);
		}

		public void UpdateWebConfig(Action<XDocument> updateAction)
		{
			UpdateConfigFile(updateAction, "Web");
		}

		protected virtual void InitializeWebConfig(XDocument doc)
		{
			UpdateAppSettings(doc, "ServerName", Db.ServerName);
			UpdateAppSettings(doc, "DatabaseName", Db.DatabaseName);
		}

		void UpdateConfigFile(Action<XDocument> updateAction, string configFileType)
		{
			var configPath = Path.Combine(DeploymentPath, configFileType + ".config");
			var doc = XDocument.Load(configPath);
			updateAction(doc);
			doc.Save(configPath);
		}

		static void UpdateAppSettings(XDocument doc, string key, string value)
		{
			var appSettingsElement = GetAppSettingsElement(doc);
			var appSetting = GetAppSettingElement(appSettingsElement, key);
			appSetting.SetAttributeValue("value", value);
		}

		static XElement GetAppSettingsElement(XDocument doc) => doc.Element("configuration").Element("appSettings");

		static XElement GetAppSettingElement(XElement appSettingsElement, string key) => appSettingsElement.Elements("add").SingleOrDefault(e => e.Attribute("key")?.Value == key);

		protected virtual bool EnableWindowsAuthentication => true;

		public byte[] TlsCertificateHash { get; set; }

		public string DeploymentPath { get; private set; }

		[ThreadSafe]
		protected static readonly ConcurrentDictionary<string, WithEnterpriseTestWebApplicationAttribute> Properties
			= new ConcurrentDictionary<string, WithEnterpriseTestWebApplicationAttribute>();
		readonly string appPoolName = Guid.NewGuid().ToString();
		int port;
		int tlsPort;

		readonly string rapidFailProtectionInterval;
		readonly int rapidFailProtectionMaxCrashes;

		const string ApplicationNameValue = "EnterpriseTestWebApplication";
	}
}
