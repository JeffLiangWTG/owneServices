//This Web.config setting only applicable in .NetFramework e.g. <secureWebPages mode = "Off" />
#if NETFRAMEWORK
using System.Collections.Generic;
using System.Xml;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Testing
{
	class AccountingWebConfigTest : BaseWebConfigTopLevelOnlyAssembliesTest
	{
		public void TestSecureWebPagesBeOff()
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.Load(DebugFilePath);
			var xmlNode = xmlDocument.DocumentElement.SelectSingleNode("secureWebPages");

			Assertion.AssertEquals("SecureWebPages should be OFF to prevent security issue when access the service with HTTPS.", "Off", xmlNode.Attributes["mode"].Value);
		}

		protected override string DebugFilePath => GetSupplementaryContentPath("Enterprise", "Product", "Operations", "Accounting", "Web", "Accounting.Web", "Web.config");
		protected override string DebugCompilationAssembliesXPath => "system.web/compilation/assemblies";

		protected override IEnumerable<string> GetExpectedAddedAssembliesForCompilation()
		{
			yield return "System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35";
			yield return "Enterprise.Accounting.Web";
		}
	}
}
#endif
