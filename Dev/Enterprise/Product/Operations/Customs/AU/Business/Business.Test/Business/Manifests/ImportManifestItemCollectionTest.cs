using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ImportManifestItemCollection))]
	public class ImportManifestItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportManifestItemCollection>
	{
		#region TestItemsNotSaveWhenPortHasMessages

		public void TestItemsNotSaveWhenPortHasMessages()
		{
			CusSeaManTranHead manifest = Factory.New<CusSeaManTranHead>();
			manifest.BT_VoyageNum = "VGNUM1";
			manifest.BT_VesselName = "VesselName";

			CusSeaManArrivalPort port = manifest.Arrivals.AddNew();
			CusSeaManOBLHeaderCargoLine cargoLine1 = port.CargoLines.AddNew();
			cargoLine1.BO_OceanBill = "BL0001";
			cargoLine1.BO_RL_NKOriginPort = "HKHKG";
			cargoLine1.BO_RL_NKDestinationPort = "AUSYD";
			EDIMessage message = port.Messages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_MessageText = "Message";
			message.EM_LinkTable = "CusSeaManArrivalPort";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = "RCV";
			message.EM_MessageType = "CST";
			message.EM_MessageSubType = "CST";

			Factory.Save();

			CusSeaManOBLHeaderCargoLine cargoLine2 = port.CargoLines.AddNew();
			cargoLine1.BO_OceanBill = "BL0002";
			cargoLine1.BO_RL_NKOriginPort = "HKHKG";
			cargoLine1.BO_RL_NKDestinationPort = "AUSYD";

			ImportManifestItemCollection itemCollection = new ImportManifestItemCollection(manifest, sailing, true);
			ImportManifestItem item = itemCollection[0];
			AssertHasWarning(item.BillNumberInfo, "Cargolist is already reported to Customs. Any additions to Cargolist must be done via Customs Import Manifest");

			Factory.Save();

			AssertEquals(0, itemCollection.Count);
		}

		public void TestSARDoesNotCauseWarning()
		{
			CusSeaManTranHead manifest = Factory.New<CusSeaManTranHead>();
			manifest.BT_VoyageNum = "VGNUM1";
			manifest.BT_VesselName = "VesselName";

			CusSeaManArrivalPort port = manifest.Arrivals.AddNew();
			CusSeaManOBLHeaderCargoLine cargoLine1 = port.CargoLines.AddNew();
			cargoLine1.BO_OceanBill = "BL0001";
			cargoLine1.BO_RL_NKOriginPort = "HKHKG";
			cargoLine1.BO_RL_NKDestinationPort = "AUSYD";
			EDIMessage message = port.Messages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_MessageText = "Message";
			message.EM_LinkTable = "CusSeaManArrivalPort";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = "RCV";
			message.EM_MessageType = "SAR";
			message.EM_MessageSubType = "SAR";

			Factory.Save();

			CusSeaManOBLHeaderCargoLine cargoLine2 = port.CargoLines.AddNew();
			cargoLine1.BO_OceanBill = "BL0002";
			cargoLine1.BO_RL_NKOriginPort = "HKHKG";
			cargoLine1.BO_RL_NKDestinationPort = "AUSYD";

			ImportManifestItemCollection itemCollection = new ImportManifestItemCollection(manifest, sailing, true);
			ImportManifestItem item = itemCollection[0];
			AssertNoWarning(item.BillNumberInfo, "Cargolist is already reported to Customs. Any additions to Cargolist must be done via Customs Import Manifest");

			message = port.Messages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			message.EM_MessageText = "Message";
			message.EM_LinkTable = "CusSeaManArrivalPort";
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = "RCV";
			message.EM_MessageType = "CST";
			message.EM_MessageSubType = "CST";

			Factory.Save();

			CusSeaManOBLHeaderCargoLine cargoLine3 = port.CargoLines.AddNew();
			cargoLine1.BO_OceanBill = "BL0003";
			cargoLine1.BO_RL_NKOriginPort = "HKHKG";
			cargoLine1.BO_RL_NKDestinationPort = "AUSYD";

			itemCollection = new ImportManifestItemCollection(manifest, sailing, true);
			item = itemCollection[0];
			AssertHasWarning(item.BillNumberInfo, "Cargolist is already reported to Customs. Any additions to Cargolist must be done via Customs Import Manifest");
		}

		#endregion

		#region TestParentManifest

		public void TestParentManifest()
		{
			AssertEquals(manifest, collection.Manifest);
		}

		public void TestSelectItems()
		{
			CusSeaManOBLHeader oceanBill1 = manifest.OceanBills.AddNew();
			oceanBill1.BO_RL_NKDischargePort = "AUSYD";
			CusSeaManOBLHeader oceanBill2 = manifest.OceanBills.AddNew();
			oceanBill2.BO_RL_NKDischargePort = "AUBNE";

			manifest.Lookups.SelectedPort = "AUSYD";

			ImportManifestItem item1 = new ImportManifestItem(oceanBill1, sailing, collection, Factory);
			ImportManifestItem item2 = new ImportManifestItem(oceanBill2, sailing, collection, Factory);

			collection.Add(item1);
			collection.Add(item2);

			Assert(!item1.ShouldSave);
			Assert(!item2.ShouldSave);

			collection.ItemsView.SelectItems(manifest.Lookups.SelectedPort, true);

			Assert(item1.ShouldSave);
			Assert(!item2.ShouldSave);

			collection.ItemsView.SelectItems(manifest.Lookups.SelectedPort, false);

			Assert(!item1.ShouldSave);
			Assert(!item2.ShouldSave);
		}

		#endregion

		#region TestSelectAll

		public void TestSelectAll()
		{
			CusSeaManOBLHeader oceanBill1 = manifest.OceanBills.AddNew();
			oceanBill1.BO_RL_NKDischargePort = "AUSYD";
			CusSeaManOBLHeader oceanBill2 = manifest.OceanBills.AddNew();
			oceanBill2.BO_RL_NKDischargePort = "AUBNE";

			manifest.Lookups.SelectedPort = "ALL";

			ImportManifestItem item1 = new ImportManifestItem(oceanBill1, sailing, collection, Factory);
			ImportManifestItem item2 = new ImportManifestItem(oceanBill2, sailing, collection, Factory);

			collection.Add(item1);
			collection.Add(item2);

			Assert(!item1.ShouldSave);
			Assert(!item2.ShouldSave);

			collection.ItemsView.SelectAll(true);

			Assert(item1.ShouldSave);
			Assert(item2.ShouldSave);

			collection.ItemsView.SelectAll(false);

			Assert(!item1.ShouldSave);
			Assert(!item2.ShouldSave);
		}

		#endregion

		#region TestHasItemsToSave

		public void TestHasItemsToSave()
		{
			CusSeaManTranHead manifest = Factory.New<CusSeaManTranHead>();

			ImportManifestItemCollection collection = new ImportManifestItemCollection(manifest, sailing, true);

			Assert(!collection.HasItemsToSave);

			ImportManifestItem item1 = new ImportManifestItem(null, sailing, collection, Factory);
			ImportManifestItem item2 = new ImportManifestItem(null, sailing, collection, Factory);

			collection.Add(item1);
			collection.Add(item2);

			item1.ShouldSave = true;

			collection.RefineItemsToSave();

			Assert(collection.HasItemsToSave);
		}

		#endregion

		#region RefreshItemsView

		public void RefreshItemsView()
		{
			CusSeaManTranHead manifest = Factory.New<CusSeaManTranHead>();

			CusSeaManOBLHeader oceanBill1 = manifest.OceanBills.AddNew();
			oceanBill1.BO_RL_NKDischargePort = "AUSYD";
			CusSeaManOBLHeader oceanBill2 = manifest.OceanBills.AddNew();
			oceanBill2.BO_RL_NKDischargePort = "AUBNE";

			manifest.Lookups.SelectedPort = "AUSYD";

			ImportManifestItemCollection collection = new ImportManifestItemCollection(manifest, sailing, true);

			AssertEquals(0, collection.ItemsView.Count);

			ImportManifestItem item1 = new ImportManifestItem(oceanBill1, sailing, collection, Factory);
			ImportManifestItem item2 = new ImportManifestItem(oceanBill2, sailing, collection, Factory);

			collection.Add(item1);
			collection.Add(item2);

			collection.RefreshItemsView();

			AssertEquals(1, collection.ItemsView.Count);
			Assert(collection.ItemsView.Contains(item1));
			Assert(!collection.ItemsView.Contains(item2));
		}

		#endregion

		#region TestLookups

		public void TestLookups()
		{
			CusSeaManTranHead manifest = Factory.New<CusSeaManTranHead>();

			CusSeaManOBLHeader oceanBill1 = manifest.OceanBills.AddNew();
			oceanBill1.BO_RL_NKDischargePort = "AUSYD";
			CusSeaManOBLHeader oceanBill2 = manifest.OceanBills.AddNew();
			oceanBill2.BO_RL_NKDischargePort = "AUBNE";

			ImportManifestItemCollection collection = new ImportManifestItemCollection(manifest, sailing, false);

			AssertEquals(0, collection.ItemsView.Count);

			ImportManifestItem item1 = new ImportManifestItem(oceanBill1, sailing, collection, Factory);
			ImportManifestItem item2 = new ImportManifestItem(oceanBill2, sailing, collection, Factory);

			collection.Add(item1);
			collection.Add(item2);

			collection.RefreshItemsView();

			AssertEquals(2, collection.ItemsView.Count);
			Assert(collection.ItemsView.Contains(item1));
			Assert(collection.ItemsView.Contains(item2));
		}

		#endregion
		#region Implementation

		protected override ImportManifestItemCollection GetCollectionToTest()
		{
			CusSeaManTranHead manifest = Factory.New<CusSeaManTranHead>();

			return new ImportManifestItemCollection(manifest, sailing, true);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ImportManifestItem(null, sailing, collection, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "HKHKC";
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUSYD";

			manifest = Factory.New<CusSeaManTranHead>();
			sailing = voyage.Sailings[0];
			collection = new ImportManifestItemCollection(manifest, sailing, true);
		}

		CusSeaManTranHead manifest;
		JobSailing sailing;
		ImportManifestItemCollection collection;

		#endregion
	}
}
