using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Module.Testing
{
	[TestedType(typeof(DFDOrganisationModuleForTest))]
	public class DFDOrganisationModuleTest : OrganisationModuleTest
	{
		public void TestImportMenuItems()
		{
			using (var module = new DFDOrganisationModuleForTest())
			using (var control = module.GetNewEmbeddedControl())
			{
				AssertNull(module.FormActionMenu.FindByText("Import DSV U.S. Organisation Data Update", true));
				AssertNull(module.FormActionMenu.FindByText("Import DSV U.S. Supplier Data", true));
			}

			var uSCompany = Factory.New<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var branch = uSCompany.Branches.AddNew();
			Factory.Save();
			using (branch.SetAsTemporaryContext())
			{
				using (var module = new DFDOrganisationModuleForTest())
				using (var control = module.GetNewEmbeddedControl())
				{
					AssertNotNull(module.FormActionMenu.FindByText("Import DSV U.S. Organisation Data Update", true));
					AssertNotNull(module.FormActionMenu.FindByText("Import DSV U.S. Supplier Data", true));
				}
			}
		}

		#region Implementation
		public class DFDOrganisationModuleForTest : DFDOrganisationModule
		{
			public new Control GetNewEmbeddedControl()
			{
				return base.GetNewEmbeddedControl();
			}
		}
		#endregion
	}
}
