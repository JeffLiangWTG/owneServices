using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ReferenceDLLTest : TestCase
	{
		// TODO: rewrite this test to check the dependency chain for possible circular references
		// System assemblies are removed as to allow for the slight differences between net48 and netcore
		public void TestReferencedDLLNotAdded()
		{
			var complianceRiskPluginBusinessAssembly = Assembly.Load("Enterprise.ComplianceRisk.Business");
			var referencedDLLs = complianceRiskPluginBusinessAssembly.GetReferencedAssemblies()
				.Where(a => !a.Name.StartsWith("System.") && !a.Name.Equals("System") && !a.Name.StartsWith("mscorlib"))
				.ToArray();
			var referencedDLLsNames = referencedDLLs.OrderBy(o => o.Name).Select(o => o.Name);
			CombineAssertions("For Compliance Risk Plugin can being applied in other modules without additional dll references, meanwhile escaping potential reference circle, try NOT to add new DLL references, if you do need to add a new reference, please change this UT. If just reduce the reference number, thank you for your contribution and please change this UT directly.", () =>
			{
				AssertEquals(28, referencedDLLs.Length);
				AssertEquals(@"CargoWise.ApplicationContext
CargoWise.Common
CargoWise.ComponentModel
CargoWise.Data
CargoWise.EntityFramework
CargoWise.EventReference
CargoWise.Integration
CargoWise.Odyssey.Schema
CargoWise.ResourceStrings
CargoWise.ResourceStrings.Cache
CargoWise.Schema
CargoWise.Shared.40
CargoWise.Types
Enterprise.ComplianceRisk.Integration
Enterprise.DeniedPartyScreening.Integration
Enterprise.Environment
Enterprise.Freight.Integration
Enterprise.Integration
Enterprise.MasterFiles.Business
Enterprise.MasterFiles.Integration
Enterprise.Messaging.Business
Enterprise.Messaging.Integration
Enterprise.Registry.Business
Enterprise.ZArchitecture.Business
Enterprise.ZArchitecture.Core
Newtonsoft.Json
Resources
WTG.Foundation.Http", string.Join(System.Environment.NewLine, referencedDLLsNames));
			});
		}
	}
}
