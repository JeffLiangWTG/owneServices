using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;

namespace CargoWiseOne.WebInfrastructure.Integration.Strong.Test
{
	class InstallSites
	{
		InstallSites()
		{
			InstallSiteItems = new List<InstallSiteItem>();
		}

		public List<InstallSiteItem> InstallSiteItems { get; }

		public static InstallSites LoadSitesFromConfig()
		{
			var document = LoadConfigFile();
			var namespaceManager = new XmlNamespaceManager(document.NameTable);
			namespaceManager.AddNamespace("Build", "http://www.edi.com.au/build.xsd");

			var sites = new InstallSites();
			foreach (XmlNode node in document.SelectSingleNode("//Build:Sites", namespaceManager).ChildNodes)
			{
				sites.InstallSiteItems.Add(DeserializeInstallSiteItem(node));
			}

			return sites;
		}

		static InstallSiteItem DeserializeInstallSiteItem(XmlNode node)
		{
			bool.TryParse(node?.Attributes?["Install"].Value, out var install);

			return new InstallSiteItem
			(
				folderName: node?.Attributes?["FolderName"].Value,
				defaultDomainPrefix: node?.Attributes?["DefaultDomainPrefix"].Value,
				defaultApplicationPath: node?.Attributes?["DefaultApplicationPath"].Value,
				description: node?.Attributes?["Description"].Value
			)
			{
				Install = install,
			};
		}

		static XmlDocument LoadConfigFile()
		{
			var assemblyFilePath = Assembly.GetExecutingAssembly().Location;
			var binPath = Path.GetDirectoryName(assemblyFilePath);
			var appConfig = ConfigurationManager.OpenExeConfiguration(assemblyFilePath);
			var webDeployXmlFileName =
				appConfig.AppSettings.Settings[WebDeployXmlFileConfig]?.Value
				?? throw new NotSupportedException("You need to specify the test web deploy xml file path in the app.Config file.");
			var webDeployXmlFilePath = Path.Combine(binPath, webDeployXmlFileName);
			if (!File.Exists(webDeployXmlFilePath))
			{
				throw new FileNotFoundException($"{webDeployXmlFileName} file not found.", webDeployXmlFilePath);
			}

			var document = new XmlDocument();
			document.Load(webDeployXmlFilePath);

			return document;
		}

		const string WebDeployXmlFileConfig = "webDeployXmlFile";
	}
}
