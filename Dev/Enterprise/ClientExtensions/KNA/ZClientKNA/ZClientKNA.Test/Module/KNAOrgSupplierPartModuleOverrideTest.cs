using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.KNA.Module.Testing
{
	public class KNAOrgSupplierPartModuleOverrideTest : TestCase
	{
		public void TestKNAOrgSupplierPartModuleOverride()
		{
			using (Customs.AU.Module.OrgSupplierPartModule module = (Customs.AU.Module.OrgSupplierPartModule)ZModuleFactory.Instance.Create(ModuleIDs.SupplierPart))
			{
				MenuAssertion.AssertHasMenu("Should find the CSV Import menu", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import From CSV");
				MenuAssertion.AssertHasMenu("Should find the KNA CSV Import menu", module.FormActionMenu, "&Actions", "D&ata Transfer", "Import from KNA CSV file");
				MenuAssertion.AssertHasMenu("Should find the KNA CSV Export menu", module.FormActionMenu, "&Actions", "D&ata Transfer", "Export to KNA CSV file");
			}
		}
	}
}
