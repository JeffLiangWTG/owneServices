using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.BuildTools;
using NUnit.Framework;

namespace Enterprise.ReflectionTest
{
	class WebConfigTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWebConfigs()
		{
			var buildDoc = XDocument.Load(BuildXml.CreateFromSourceCodeDirectory().BuildXmlFileName);
			var namespaceManager = new XmlNamespaceManager(new NameTable());
			namespaceManager.AddNamespace("namespace", buildDoc.Root.GetDefaultNamespace().NamespaceName);
			var webSolutions = buildDoc.XPathSelectElements("//namespace:WebSolution", namespaceManager);

			CombineAssertions(() =>
			{
				foreach (var solution in webSolutions)
				{
					var path = BaseSourcePath + solution.Attribute("Path").Value;
					var webConfigFiles = Directory.GetFiles(path, "Web.config", SearchOption.AllDirectories);

					foreach (var webConfigFile in webConfigFiles)
					{
						var webConfigDoc = XDocument.Load(webConfigFile);

						var sysWeb = webConfigDoc.XPathSelectElement("//system.web");
						AssertSystemWeb(webConfigFile, sysWeb);

						var appSettings = webConfigDoc.XPathSelectElement("//appSettings");

						AssertAppSettings(webConfigFile, appSettings);
					}
				}
			});
		}

		#region Implementation

		void AssertSystemWeb(string webConfigFile, XElement sysWeb)
		{
			const string missingElement = "<hostingEnvironment shadowCopyBinAssemblies=\"false\" />";

			AssertNotNull(string.Format("{0} does not have a <system.web> element. Please add one with a {1} child element.", webConfigFile, missingElement), sysWeb);

			if (sysWeb == null)
			{
				return;
			}

			var hostingEnvironment = sysWeb.XPathSelectElement("hostingEnvironment");
			AssertNotNull(string.Format("{0} does not contain a <hostingEnvironment> element. Please add {1} as a child element of the <system.web> element.", webConfigFile, missingElement), hostingEnvironment);

			if (hostingEnvironment == null)
			{
				return;
			}

			var shadowCopyAttribute = hostingEnvironment.Attribute("shadowCopyBinAssemblies");
			AssertNotNull(string.Format("{0} has a <hostingEnvironment> element, but the element doesn't have a shadowCopyBinAssemblies attribute. Please add the attribute and set its value to false.", webConfigFile), shadowCopyAttribute);

			if (shadowCopyAttribute == null)
			{
				return;
			}

			AssertEquals(string.Format("{0} has a <hostingEnvironment> element with a shadowCopyBinAssemblies attribute, but its value is set to true and should be false.", webConfigFile), "false", shadowCopyAttribute.Value);
		}

		void AssertAppSettings(string webConfigFile, XElement appSettings)
		{
			const string missingElement = "<add key=\"owin:AutomaticAppStartup\" value=\"false\" />";
			AssertNotNull($"{webConfigFile} does not have <appSettings> element. Please add one with a {missingElement} child element.", appSettings);

			var owinAppStartup = appSettings.XPathSelectElement("add[@key='owin:appStartup']");
			var owinAutomaticAppStartup = appSettings.XPathSelectElement("add[@key='owin:AutomaticAppStartup']");

			if (owinAppStartup != null)
			{
				var owinAppStartupValue = owinAppStartup.Attribute("value").Value;
				AssertNotNull($"{webConfigFile} appSettings has owin:appStartup element, but the value, '{owinAppStartupValue}', is not a valid AssemblyQualifiedName. This is required to short-circuit scanning all DLLs which is unnecessary and may result in errors.",
					Type.GetType(owinAppStartupValue));
			}
			else if (owinAutomaticAppStartup != null)
			{
				var owinAutomaticAppStartupValue = owinAutomaticAppStartup.Attribute("value");
				AssertEquals($"{webConfigFile} appSettings has owin:AutomaticAppStartup element, but its value is set to true and should be false.", "false", owinAutomaticAppStartupValue.Value);
			}
			else
			{
				Fail($"{webConfigFile} appSettings doesn't have owin:appStartup or owin:AutomaticAppStartup element. Please add some.");
			}
		}

		#endregion
	}
}
