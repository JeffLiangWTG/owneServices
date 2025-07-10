using System;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Customs.DE.Messaging.Testing
{
	sealed class DEMessagingSolutionTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoReferenceToBusinessInMessagingProject()
		{
			var baseDESourcePath = BaseSourcePath + @"Enterprise\Product\Operations\Customs\DE\Core\";
			var filename = baseDESourcePath + "DE.sln";
			var solution = new SolutionFile(filename);

			var projectInfo = solution.Projects.Where(x => x.ProjectName == "Enterprise.Customs.DE.Messaging").Single();
			var projectPath = baseDESourcePath + projectInfo.ProjectPath;

			var projectDefinition = XDocument.Load(projectPath);
			var references = projectDefinition
				.Element("Project")
				.Elements("ItemGroup")
				.Elements("Reference")
				.Select(refElem => refElem.Element("HintPath")?.Value);
			var allowedBusinessReferences = new[] { @"..\..\..\..\..\..\..\Bin\Enterprise.Messaging.Business.dll", @"..\..\..\..\..\..\..\Bin\Enterprise.ZArchitecture.Business.dll" };
			var extraBusinessReferences = references.Where(x => !string.IsNullOrEmpty(x) && x.IndexOf("Business.dll", StringComparison.CurrentCultureIgnoreCase) >= 0).Except(allowedBusinessReferences).ToArray();
			if (extraBusinessReferences.Length > 0)
			{
				CombineAssertions(() =>
				{
					foreach (string reference in extraBusinessReferences)
					{
						Assert($"Do not add Business reference to Messaging business logic should go through the provider. Reference: {reference}", false);
					}
				});
			}
			else
			{
				Assert("No added Business References", true);
			}
		}
	}
}
