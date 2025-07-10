using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ReferenceDLLTest : TestCase
	{
		public void TestReferencedDLLNotAdded()
		{
			string[] expectedDllNames = [
				"CargoWise.ApplicationContext",
				"CargoWise.ComponentModel",
				"CargoWise.EntityFramework",
				"CargoWise.Integration",
				"CargoWise.Odyssey.Schema",
				"CargoWise.ResourceStrings",
				"CargoWise.ResourceStrings.Cache",
				"CargoWise.Schema",
				"CargoWise.Shared.40",
				"CargoWise.Types",
				"CargoWise.Windows.UI",
				"Enterprise.ComplianceRisk.Business",
				"Enterprise.ComplianceRisk.Integration",
				"Enterprise.Customs.Common",
				"Enterprise.Customs.Common.GUI",
				"Enterprise.DeniedPartyScreening.Business",
				"Enterprise.DeniedPartyScreening.Common",
				"Enterprise.DeniedPartyScreening.Integration",
				"Enterprise.Environment",
				"Enterprise.Freight.Integration",
				"Enterprise.Integration",
				"Enterprise.Licensing.Core",
				"Enterprise.MasterFiles.Business",
				"Enterprise.MasterFiles.Integration",
				"Enterprise.Registry.Business",
				"Enterprise.Security.Core",
				"Enterprise.ZArchitecture.Business",
				"Enterprise.ZArchitecture.Core",
				"Enterprise.ZArchitecture.GUI",
				"Enterprise.ZArchitecture.GUI.UserControls",
				"Enterprise.ZArchitecture.Modules",
				"mscorlib",
				"Resources",
				"System",
				"System.Core",
				"System.Drawing",
				"System.Windows.Forms"
			];
			var complianceRiskPluginBusinessAssembly = Assembly.Load("Enterprise.ComplianceRisk.GUI");
			var referencedDLLs = complianceRiskPluginBusinessAssembly.GetReferencedAssemblies();
			var referencedDLLsNames = referencedDLLs.Select(o => o.Name).OrderBy(o => o).ToArray();
			CombineAssertions("For Compliance Risk Plugin can being applied in other modules without additional dll references, meanwhile escaping potential reference circle, try NOT to add new DLL references, if you do need to add a new reference, please change this UT. If just reduce the reference number, thank you for your contribution and please change this UT directly.", () =>
			{
				//Would it be better if this test allowed for any of the references listed to be removed and only
				//prevented other references being added?
				AssertEquals(expectedDllNames.Length, referencedDLLs.Length);

				AssertArrayEqualsByElements(expectedDllNames, referencedDLLsNames);
			});
		}
	}
}
