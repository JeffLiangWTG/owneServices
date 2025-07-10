using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeRuleSetLookupsTest : BarcodeParsingLookupsTestCase
	{
		#region TestBuyers

		public void TestBuyers()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.Buyers = new DepotCollection(Factory);
			AssertEquals(typeof(DepotCollection), ruleSet.Lookups.Buyers.GetType());

			ruleSet.BRS_Module = "";
			AssertEquals(typeof(ConsigneeCollection), ruleSet.Lookups.Buyers.GetType());
		}

		#endregion

		#region TestModuleTypes

		public void TestModuleTypes()
		{
			AssertContainsExactElementsInAnyOrder(new BarcodeModuleTypes(), new BusinessObjectFactory().New<BarcodeRuleSet>().Lookups.ModuleTypes);
		}

		public void TestModuleTypes_DoesNotContainLocationType()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Guid.Empty, Guid.Empty))
			{
				AssertEquals(false, new BusinessObjectFactory().New<BarcodeRuleSet>().Lookups.ModuleTypes.ContainsCode("LOC"));
			}
		}

		#endregion

		#region TestRelatedEntityList

		public void TestRelatedEntityList()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.RelatedEntityList = new GlbCompanyCollection(Factory);

			var relatedEntityLookup1 = ruleSet.Lookups.RelatedEntityList;
			AssertEquals(typeof(GlbCompanyCollection), relatedEntityLookup1.GetType());

			dummy.RelatedEntityList = new GlbBranchCollection(Factory);
			AssertEquals("Related Entity list should be cached by Buyer & Supplier.", relatedEntityLookup1, ruleSet.Lookups.RelatedEntityList);

			ruleSet.BRS_OH_Buyer = Helper.CreateOrg("Buyer").PK;
			var relatedEntityLookup2 = ruleSet.Lookups.RelatedEntityList;
			AssertEquals("Related Entity list should be cached by Buyer & Supplier.", typeof(GlbBranchCollection), relatedEntityLookup2.GetType());

			dummy.RelatedEntityList = new GlbDepartmentCollection(Factory);
			AssertEquals("Related Entity list should be cached by Buyer & Supplier.", relatedEntityLookup2, ruleSet.Lookups.RelatedEntityList);

			ruleSet.BRS_OH_Supplier = Helper.CreateOrg("Supplier").PK;
			AssertEquals("Related Entity list should be cached by Buyer & Supplier.", typeof(GlbDepartmentCollection), ruleSet.Lookups.RelatedEntityList.GetType());

			ruleSet.BRS_Module = "";
			AssertNull("Default Module has no Related Entity.", ruleSet.Lookups.RelatedEntityList);
		}

		#endregion

		#region TestSuppliers

		public void TestSuppliers()
		{
			var ruleSet = Helper.CreateRuleSet();
			ruleSet.BRS_Module = DummyBarcodeParsingConsumer.Module;

			var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
			dummy.Suppliers = new CTOCollection(Factory);
			AssertEquals(typeof(CTOCollection), ruleSet.Lookups.Suppliers.GetType());

			ruleSet.BRS_Module = "";
			AssertEquals(typeof(ConsignorCollection), ruleSet.Lookups.Suppliers.GetType());
		}

		#endregion
	}
}
