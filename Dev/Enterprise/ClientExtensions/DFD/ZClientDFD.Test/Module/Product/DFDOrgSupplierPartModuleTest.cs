using Enterprise.MasterFiles.Module;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Module.Testing
{
	[TestedType(typeof(OrganisationModule))]
	class DFDOrgSupplierPartModuleTest : OrganisationModuleTest
	{
		public void TestImportMenuItems()
		{
			using (var module = new DFDOrgSupplierPartModule())
			{
				AssertNotNull(module.FormActionMenu.FindByText("Import DSV U.S. Product Data", true));
				AssertNotNull(module.FormActionMenu.FindByText("Import From CSV", true));
				AssertNotNull(module.FormActionMenu.FindByText("Import Last Cost From CSV", true));
			}
		}
	}
}
