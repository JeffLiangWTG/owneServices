using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ImportManifestItem))]
	public class ImportManifestItemTest : NonPersistentBusinessObjectTestCase
	{
		#region TestReadOnly

		public void TestReadOnly()
		{
			Assert(!collection[0].ReadOnly);
		}

		#endregion

		#region TestParentCollection

		public void TestParentCollection()
		{
			AssertEquals(collection, importManifestItem.ParentCollection);
		}

		#endregion

		#region TestRelatedBusinessObjectList

		public void TestRelatedBusinessObjectList()
		{
			AssertEquals(2, collection.Count);
			AssertEquals(oceanBill, collection[0].RelatedBusinessObjectList[0]);
			AssertEquals(cargoLine1, collection[1].RelatedBusinessObjectList[0]);
			AssertEquals(cargoLine2, collection[1].RelatedBusinessObjectList[1]);
		}

		#endregion

		#region TestLookups

		public void TestLookups()
		{
			manifest.Lookups.SelectedPort = "ALL";

			CusSeaManArrivalPort port1 = manifest.Arrivals.AddNew();
			port1.BA_RL_NKArrivalPort = "AUSYD";
			CusSeaManArrivalPort port2 = manifest.Arrivals.AddNew();
			port2.BA_RL_NKArrivalPort = "AUBNE";

			ImportManifestItem importManifestItem = collection[0];

			AssertEquals("ALL", importManifestItem.SelectedPort);
			Assert(importManifestItem.ArrivalPorts.ContainsCode("AUSYD"));
			Assert(importManifestItem.ArrivalPorts.ContainsCode("AUBNE"));
			Assert(importManifestItem.ArrivalPorts.ContainsCode("ALL"));
			Assert(importManifestItem.ItemsView.Contains(importManifestItem));
		}

		#endregion

		#region TestCountSavedBills

		public void TestCountSavedBills()
		{
			CusSeaManOBLHeader oceanBill1 = manifest.OceanBills.AddNew();
			oceanBill1.BO_OceanBill = "OceanBill1";
			CusSeaManOBLHeader oceanBill2 = manifest.OceanBills.AddNew();
			oceanBill2.BO_OceanBill = "OceanBill2";
			CusSeaManOBLHeader oceanBill3 = manifest.OceanBills.AddNew();
			oceanBill3.BO_OceanBill = "OceanBill3";
			CusSeaManOBLHeader oceanBill4 = manifest.OceanBills.AddNew();
			oceanBill4.BO_OceanBill = "OceanBill4";

			ImportManifestItem importManifestItem1 = new ImportManifestItem(oceanBill1, sailing, collection, Factory);
			ImportManifestItem importManifestItem2 = new ImportManifestItem(oceanBill2, sailing, collection, Factory);

			AssertEquals(0, collection.SavedBillsCount);

			collection.Add(importManifestItem1);
			collection.Add(importManifestItem2);
			importManifestItem1.ShouldSave = false;
			importManifestItem2.ShouldSave = false;

			Factory.Save();
			AssertEquals(0, collection.SavedBillsCount);

			ImportManifestItem importManifestItem3 = new ImportManifestItem(oceanBill3, sailing, collection, Factory);
			collection.Add(importManifestItem3);
			collection.IsRefinedForSave = false;
			importManifestItem3.ShouldSave = true;
			importManifestItem3.BillNumberInfo.AddWarning("Cargolist is already reported to Customs. Any additions to Cargolist must be done via Customs Import Manifest");
			Factory.Save();

			AssertEquals(0, collection.SavedBillsCount);

			ImportManifestItem importManifestItem4 = new ImportManifestItem(oceanBill4, sailing, collection, Factory);
			collection.Add(importManifestItem4);
			collection.IsRefinedForSave = false;
			importManifestItem4.ShouldSave = true;
			Factory.Save();

			AssertEquals(1, collection.SavedBillsCount);
		}

		#endregion

		#region TestOnFactorySaving

		public void TestOnFactorySaving()
		{
			CusSeaManOBLHeader oceanBill1 = manifest.OceanBills.AddNew();
			oceanBill1.BO_OceanBill = "OceanBill1";
			CusSeaManOBLHeader oceanBill2 = manifest.OceanBills.AddNew();
			oceanBill2.BO_OceanBill = "OceanBill2";

			ImportManifestItem importManifestItem1 = new ImportManifestItem(oceanBill1, sailing, collection, Factory);
			ImportManifestItem importManifestItem2 = new ImportManifestItem(oceanBill2, sailing, collection, Factory);

			collection.Add(importManifestItem1);
			collection.Add(importManifestItem2);

			importManifestItem1.ShouldSave = true;

			Assert(manifest.OceanBills.Contains(oceanBill1));
			Assert(manifest.OceanBills.Contains(oceanBill2));

			Factory.Save();

			Assert(manifest.OceanBills.Contains(oceanBill1));
			Assert(!manifest.OceanBills.Contains(oceanBill2));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			manifest = Factory.New<CusSeaManTranHead>();
			oceanBill = manifest.OceanBills.AddNew();
			CusSeaManArrivalPort port = manifest.Arrivals.AddNew();
			cargoLine1 = port.CargoLines.AddNew();
			cargoLine2 = port.CargoLines.AddNew();
			cargoLine1.BO_OceanBill = "Bill1";
			cargoLine2.BO_OceanBill = "Bill1";

			VoyageOrigin voyOrigin = Factory.New<VoyageOrigin>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			sailing = Factory.New<JobSailing>();
			VoyageDestination voyDestination = Factory.New<VoyageDestination>();

			voyOrigin.JA_JV = voyage.PK;
			voyDestination.JB_JV = voyage.PK;
			sailing.JX_JA = voyOrigin.PK;
			sailing.JX_JB = voyDestination.PK;

			collection = new ImportManifestItemCollection(manifest, sailing, true);
			importManifestItem = new ImportManifestItem(null, sailing, collection, Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return importManifestItem;
		}

		CusSeaManOBLHeaderCargoLine cargoLine1;
		CusSeaManOBLHeaderCargoLine cargoLine2;
		CusSeaManTranHead manifest;
		JobSailing sailing;
		CusSeaManOBLHeader oceanBill;
		ImportManifestItemCollection collection;
		ImportManifestItem importManifestItem;

		#endregion
	}
}
