using System.Collections.Generic;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using static Enterprise.ZArchitecture.Web.Modules.WebModuleIDs;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class WebModuleIDsTest : BaseModuleIDsTest
	{
		#region TestGetWebModuleIDFromModuleID

		public void TestGetWebModuleIDFromModuleID()
		{
			AssertEquals(OrganisationTracking, GetWebModuleIDFromModuleID(ModuleIDs.Organisation));
			AssertEquals(Location, GetWebModuleIDFromModuleID(ModuleIDs.Location));
			AssertEquals(RefCountry, GetWebModuleIDFromModuleID(ModuleIDs.RefCountry));
			AssertEquals(RefUNLOCO, GetWebModuleIDFromModuleID(ModuleIDs.RefUNLOCO));
			AssertEquals(RefVessel, GetWebModuleIDFromModuleID(ModuleIDs.RefVessel));
			AssertEquals(RefCommodityCode, GetWebModuleIDFromModuleID(ModuleIDs.RefCommodityCode));
			AssertEquals(OrgSupplierPartTracking, GetWebModuleIDFromModuleID(ModuleIDs.WhsConfigProduct));
			AssertEquals(TrackingWarehouse, GetWebModuleIDFromModuleID(ModuleIDs.WhsConfigWarehouse));
			AssertEquals(RefServiceLevel, GetWebModuleIDFromModuleID(ModuleIDs.ServiceLevel));
			AssertEquals(OrgSupplierPartTracking, GetWebModuleIDFromModuleID(ModuleIDs.SupplierPart));
			AssertEquals(OrgCarrierTracking, GetWebModuleIDFromModuleID(ModuleIDs.Organisation, "CaRriEr"));
			AssertEquals(GlbPerson, GetWebModuleIDFromModuleID(ModuleIDs.GlbPerson));

			AssertEquals(NotAssigned, GetWebModuleIDFromModuleID(ModuleIDs.Registry));
		}

		#endregion

		#region TestNestedModules

		public override void TestNestedModules()
		{
			bool found1 = false;
			bool found2 = false;
			foreach (ModuleIdentifier iD in GetModuleIDs())
			{
				if (iD == WebModuleIDs.Dummy)
				{
					found1 = true;
				}

				if (iD == WebModuleIDs.DummyClass.NestedDummy)
				{
					found2 = true;
				}

				if (found1 && found2)
				{
					Assert(true);
					return;
				}
			}
			Fail("Dummy or NestedDummy module was not found");
		}

		#endregion

		#region Implementation

		protected override IEnumerable<ModuleIdentifier> GetModuleIDs()
		{
			return WebModuleIDs.All;
		}

		#endregion
	}
}
