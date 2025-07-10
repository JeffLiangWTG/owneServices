using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BarcodeParsing.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business
{
	class BarcodeParsingDiagnosticsLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestBuyers

		public void TestBuyers()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			using (BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(Factory))
			{
				diagnostic.ModuleCode = DummyBarcodeParsingConsumer.Module;

				var dummy = DummyBarcodeParsingConsumer.GetDummy(Factory);
				dummy.Buyers = new DepotCollection(Factory);
				AssertEquals(typeof(DepotCollection), diagnostic.Lookups.Buyers.GetType());

				diagnostic.ModuleCode = "";
				AssertEquals(typeof(ConsigneeCollection), diagnostic.Lookups.Buyers.GetType());
			}
		}

		#endregion

		#region TestModuleTypes

		public void TestModuleTypes()
		{
			AssertContainsExactElementsInAnyOrder(new BarcodeModuleTypes(), new BarcodeParsingDiagnostics(Factory).Lookups.ModuleTypes);
		}

		#endregion

		#region TestRelatedEntityList

		public void TestRelatedEntityList()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			var factory = diagnostic.Factory;

			using (BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(factory))
			{
				diagnostic.ModuleCode = DummyBarcodeParsingConsumer.Module;

				var dummy = DummyBarcodeParsingConsumer.GetDummy(factory);
				dummy.RelatedEntityList = new GlbCompanyCollection(factory);

				var relatedEntityLookup1 = diagnostic.Lookups.RelatedEntityList;
				AssertEquals(typeof(GlbCompanyCollection), relatedEntityLookup1.GetType());

				dummy.RelatedEntityList = new GlbBranchCollection(factory);
				AssertEquals("Related Entity list should be cached by Buyer & Supplier.", relatedEntityLookup1, diagnostic.Lookups.RelatedEntityList);

				diagnostic.BuyerPK = Factory.New<OrgHeader>().PK;
				var relatedEntityLookup2 = diagnostic.Lookups.RelatedEntityList;
				AssertEquals("Related Entity list should be cached by Buyer & Supplier.", typeof(GlbBranchCollection), relatedEntityLookup2.GetType());

				dummy.RelatedEntityList = new GlbDepartmentCollection(factory);
				AssertEquals("Related Entity list should be cached by Buyer & Supplier.", relatedEntityLookup2, diagnostic.Lookups.RelatedEntityList);

				diagnostic.SupplierPK = Factory.New<OrgHeader>().PK;
				AssertEquals("Related Entity list should be cached by Buyer & Supplier.", typeof(GlbDepartmentCollection), diagnostic.Lookups.RelatedEntityList.GetType());

				diagnostic.ModuleCode = "";
				AssertNull("Default Module has no Related Entity.", diagnostic.Lookups.RelatedEntityList);
			}
		}

		#endregion

		#region TestSuppliers

		public void TestSuppliers()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			var factory = diagnostic.Factory;

			using (BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(factory))
			{
				diagnostic.ModuleCode = DummyBarcodeParsingConsumer.Module;

				var dummy = DummyBarcodeParsingConsumer.GetDummy(factory);
				dummy.Suppliers = new CTOCollection(factory);
				AssertEquals(typeof(CTOCollection), diagnostic.Lookups.Suppliers.GetType());

				diagnostic.ModuleCode = "";
				AssertEquals(typeof(ConsignorCollection), diagnostic.Lookups.Suppliers.GetType());
			}
		}

		#endregion

		#region TestDiagnosticsTypes

		public void TestDiagnosticsTypes()
		{
			AssertContainsExactElementsInAnyOrder(new DiagnosticsTypes(), new BarcodeParsingDiagnostics(Factory).Lookups.DiagnosticsTypes);
		}

		#endregion

		#region TestTargetFields

		public void TestTargetFields()
		{
			var diagnostic = new BarcodeParsingDiagnostics(Factory);
			var factory = diagnostic.Factory;

			using (BarcodeParsingTestCase.EnableDummyBarcodeParsingConsumer(factory))
			{
				diagnostic.ModuleCode = DummyBarcodeParsingConsumer.Module;
				var targetFields = new DummyTargetFields();
				AssertContainsExactElementsInAnyOrder(targetFields.Cast<CodeDescriptionPair>(), diagnostic.Lookups.TargetFields);

				diagnostic.ModuleCode = "";
				AssertEquals(0, diagnostic.Lookups.TargetFields.Count);
			}
		}

		#endregion
	}
}
