using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEOrganisationModule))]
	public class UPEOrganisationModuleTest : ZModuleBasherTest
	{
		public void TestGetNewFilterControl()
		{
			AssertEquals("Correct ZFilterControl type", typeof(UPEOrganisationFilterControl), Module.EmbeddedControl.GetType());
		}

		public void TestGetNewFilterBusinessObject()
		{
			AssertEquals("Correct FilterBusinessObject type", typeof(UPEOrganisationFilterBusinessObject), Module.FilterBusinessObject.GetType());
		}

		public void TestGetNewActionMenu()
		{
			MenuItem[] menuItemCollection = Module.GetNewActionMenuItems();
			AssertNotNull("Menu item exists", menuItemCollection.FindByText("&Update Classifier Role"));
		}

		#region Implementation
		TestUPEOrganisationModule Module;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Module = new TestUPEOrganisationModule();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Module.Dispose();
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Organisation;
		}

		class TestUPEOrganisationModule : UPEOrganisationModule
		{
			public new FilterBusinessObject FilterBusinessObject
			{
				get
				{
					return base.FilterBusinessObject;
				}
			}

			public new MenuItem[] GetNewActionMenuItems()
			{
				return base.GetNewActionMenuItems();
			}
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			// The test db will be full of OrgHeaders, so an additional identifier is needed to filter by in order to avoid test problems.
			var org = (OrgHeader)factory.NewWithValidTestData(businessObjectType);
			org.OH_FullName = filterStripHelperTestOrgName;
			return org;
		}

		protected override void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
			base.CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);
			var filter = (ModuleTextFilter)filterBusinessObject["Name"];
			filter.IsActive = true;
			filter.Property = filterStripHelperTestOrgName;
		}

		const string filterStripHelperTestOrgName = "MODULE BASHER TEST";
		#endregion
	}
}
