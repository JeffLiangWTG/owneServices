using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.AUS.Products.Testing
{
	[TestedType(typeof(AUSOrgSupplierPartModule))]
	public class AUSOrgSupplierPartModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.SupplierPart;
		}

		public void TestKNAOrgSupplierPartModuleOverride()
		{
			using (AUSOrgSupplierPartModule module = new AUSOrgSupplierPartModule())
			{
				MenuAssertion.AssertHasMenu("Should find the CSV Import menu", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import From CSV");
				MenuAssertion.AssertHasMenu("Should find the Austin CSV Import menu", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import From &Austin csv-file");
				MenuAssertion.AssertHasMenu("Should find the Austin CSV Export menu", module.FormActionMenu, "&Actions", "D&ata Transfer", "Export To &Austin csv-file");
			}
		}
	}
}
