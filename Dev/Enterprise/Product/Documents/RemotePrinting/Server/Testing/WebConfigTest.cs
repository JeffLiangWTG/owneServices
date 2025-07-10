using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	public class WebConfigTest : TestCase
	{
		string webConfigPath
		{
			get { return GetSupplementaryContentPath("Enterprise", "Product", "Documents", "RemotePrinting", "Server", "Server", "Web.config"); }
		}

		public void TestWebConfigIsPresent()
		{
			var webConfigXml = XDocument.Load(webConfigPath);
			AssertNotNull(webConfigXml);
		}

		public void TestCustomErrorsModeOff()
		{
			var webConfigXml = XDocument.Load(webConfigPath);
			var customErrors = webConfigXml.XPathSelectElement("//system.web/customErrors");
			AssertEquals("Off", (string)customErrors.Attribute("mode"));
		}

		public void TestSoapExtensions()
		{
			var webConfigXml = XDocument.Load(webConfigPath);
			var soapExtensions = webConfigXml.XPathSelectElement("//system.web/webServices/soapExtensionTypes");
			AssertNotNull(soapExtensions);
			Assert(soapExtensions.HasElements);

			var expectedExtensionName = typeof(ExceptionHandlingExtension).AssemblyQualifiedName;
			var versionIndex = expectedExtensionName.IndexOf(", Version", StringComparison.OrdinalIgnoreCase);
			if (versionIndex > 0)
			{
				expectedExtensionName = expectedExtensionName.Substring(0, versionIndex);
			}

			var expectedExtensionElement = soapExtensions.Elements().FirstOrDefault(x =>
				x.Attributes().Any(attribute =>
					attribute.Name.LocalName.Equals("type") && attribute.Value.Equals(expectedExtensionName)));
			AssertNotNull(expectedExtensionElement);
		}

		public void TestClientRequirements()
		{
			var webConfigXml = XDocument.Load(webConfigPath);

			var osRequirement = webConfigXml.XPathSelectElement($"//appSettings/add[@key='{AppSettingsHelper.Keys.ClientRequirementOS}']");
			AssertNotNull("Should have ClientRequirementOS setting", osRequirement);
			AssertEquals("No OS requirement.", "", (string)osRequirement.Attribute("value"));

			var dotnetRequirement = webConfigXml.XPathSelectElement($"//appSettings/add[@key='{AppSettingsHelper.Keys.ClientRequirementDotnet}']");
			AssertNotNull("Should have ClientRequirementDotnet setting", dotnetRequirement);
			AssertEquals("Requires .Net 4.8 or better", "4.8", (string)dotnetRequirement.Attribute("value"));

			var intermediateInstall = webConfigXml.XPathSelectElement($"//appSettings/add[@key='{AppSettingsHelper.Keys.ClientIntermediateInstallationFile}']");
			AssertNotNull("Should have ClientIntermediateInstallationFile setting", intermediateInstall);
		}

		public void TestRoleConfigurationForSupportPage()
		{
			string expectedLocationName = "Support";
			var webConfigXml = XDocument.Load(webConfigPath);

			var locationXml = webConfigXml.XPathSelectElement("/configuration/location[contains(@path,'" + expectedLocationName + "')]/system.web/authorization");
			AssertNotNull(locationXml);

			string allowRoles = locationXml.Element("allow")?.Attribute("roles")?.Value;
			AssertEquals("support", allowRoles);

			string denyUsers = locationXml.Element("deny")?.Attribute("users")?.Value;
			AssertEquals("*", denyUsers);
		}

		public void TestProtocols()
		{
			var webConfigXml = XDocument.Load(webConfigPath);
			var protocols = webConfigXml.XPathSelectElement("//system.web/webServices/protocols").Elements().ToArray();

			AssertEquals(1, protocols.Length);

			AssertEquals("add", protocols[0].Name.LocalName);
			AssertEquals("HttpPost", protocols[0].Attribute("name").Value);
		}
	}
}
