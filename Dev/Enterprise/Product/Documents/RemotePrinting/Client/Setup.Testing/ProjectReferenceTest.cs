using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Setup.Testing
{
	sealed class ProjectReferenceTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCoreComponent_xTMessagingShared_ReferencesOnlyComponentsAvailableInRemotePrintingSetup()
		{
			var projectFilePath = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Core", "xTMessaging", "Enterprise.xTMessaging.Shared", "Enterprise.xTMessaging.Shared.csproj");
			var setupWxsFilePath = Path.Combine(BaseSourcePath, "Enterprise", "Product", "Documents", "RemotePrinting", "Client", "Setup", "Setup.wxs");

			Assert($"{projectFilePath} is not exist.", File.Exists(projectFilePath));
			Assert($"{setupWxsFilePath} is not exist.", File.Exists(setupWxsFilePath));

			var projectXml = XDocument.Load(projectFilePath);
			var setupWxsXml = XDocument.Load(setupWxsFilePath);

			var projectReferences = projectXml
				.XPathSelectElements("//ItemGroup/Reference[@Include]")
				.Select(x => x.Attribute("Include")?.Value?.Trim() ?? string.Empty)
				.ToArray();

			Assert("Should get all references.", projectReferences.Length > 0);

			var setupWxsReferences = setupWxsXml
				.XPathSelectElements("//*[local-name()='Component'][@Id='CoreComponent']/*[local-name()='File']")
				.Select(x => x.Attribute("Name")?.Value?.Trim() ?? string.Empty)
				.ToArray();

			Assert("Should get all component files.", setupWxsReferences.Length > 0);

			CombineAssertions(() =>
			{
				foreach (var projectReference in projectReferences)
				{
					Assert($@"The project reference ""{projectReference}"" should be in ""{setupWxsFilePath}"".", setupWxsReferences.Any(x => x.Equals(projectReference + ".dll", System.StringComparison.InvariantCultureIgnoreCase)));
				}
			});
		}
	}
}
