using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ManifestBase.Testing
{
	[TestsSubclassesOf(typeof(AsycudaBill))]
	public abstract class AsycudaBillTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEnsurePackedItemRelationshipSettingIsCorrect()
		{
			var bizObj = (AsycudaBill)GetNewBusinessObject();
			var pack = bizObj.Packs.AddNew();
			AssertEquals("Bill.PackedItemRelationship should match Pack.PackedItemRelationship", pack.PackedItemRelationship, bizObj.PackedItemRelationship);
		}
	}

	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillBaseOnlyTest : AsycudaBillTest
	{
		public void TestPackedItems()
		{
			var bill = Factory.New<AsycudaBill>();
			var packedItems = bill.PackedItems;
			CombineAssertions(() =>
			{
				AssertType<AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>>("Type of collection", packedItems);
				AssertEquals("IsLoaded", true, packedItems.IsLoaded);
				AssertEquals("IsRegisteredEditableChildObject", true, bill.IsRegisteredEditableChildObject(packedItems));
			});
		}

		public void TestPackedItemsRelationshipTypeOne()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			bill.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			var pack1 = bill.Packs.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedBill = newFactory.Load<AsycudaBill>(bill.PK);
			loadedBill.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;

			CombineAssertions(() =>
			{
				AssertEquals("Initially there's 1 pack item", 1, loadedBill.PackedItems.Count);
				AssertEquals("Line number", (ZInt)1, loadedBill.PackedItems[0].API_LineNo);

				var pack2 = loadedBill.Packs.AddNew();
				AssertEquals("Now there are 2 pack items", 2, loadedBill.PackedItems.Count);
				AssertContainsExactElementsInAnyOrder("Line numbers of pack items",
					new ZInt[] { 1, 2 }, loadedBill.PackedItems.Select(x => x.API_LineNo));

				var pack3 = loadedBill.Packs.AddNew();
				AssertEquals("Now there are 3 pack items", 3, loadedBill.PackedItems.Count);
				AssertContainsExactElementsInAnyOrder("Line numbers of pack items",
					new ZInt[] { 1, 2, 3 }, loadedBill.PackedItems.Select(x => x.API_LineNo));

				var billClusterKey = loadedBill.ABL_ClusterKey;
				Assert("Before saved, ClusterKeys of pack items are equal to that of bill", loadedBill.PackedItems.All(x => x.API_ClusterKey == billClusterKey));
				AssertNoExceptionThrown(() => newFactory.Save());
				Assert("After saved, ClusterKeys of pack items are equal to that of bill", loadedBill.PackedItems.All(x => x.API_ClusterKey == billClusterKey));
			});
		}

		public void TestPackedItemRelationship()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertEquals("IsManyPackedItemRelationship", false, bill.IsManyPackedItemRelationship);
			AssertEquals("IsNonePackedItemRelationship", true, bill.IsNonePackedItemRelationship);
			AssertEquals("IsOnePackedItemRelationship", false, bill.IsOnePackedItemRelationship);
			bill.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			AssertEquals("IsManyPackedItemRelationship", false, bill.IsManyPackedItemRelationship);
			AssertEquals("IsNonePackedItemRelationship", false, bill.IsNonePackedItemRelationship);
			AssertEquals("IsOnePackedItemRelationship", true, bill.IsOnePackedItemRelationship);
			bill.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			AssertEquals("IsManyPackedItemRelationship", true, bill.IsManyPackedItemRelationship);
			AssertEquals("IsNonePackedItemRelationship", false, bill.IsNonePackedItemRelationship);
			AssertEquals("IsOnePackedItemRelationship", false, bill.IsOnePackedItemRelationship);
		}

		public void TestPackedItemRelationshipRelatedHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.PackedItemRelationshipOverrideForTesting = AsycudaPackPackedItemPivotCollection.RelationshipType.One;
			Assert(!header.IsNonePackedItemRelationship);
			Assert(header.IsOnePackedItemRelationship);
			Assert(!header.IsManyPackedItemRelationship);
			var bill = header.Bills.AddNew();
			Assert(!bill.IsNonePackedItemRelationship);
			Assert(bill.IsOnePackedItemRelationship);
			Assert(!bill.IsManyPackedItemRelationship);
			var package = bill.Packs.AddNew();
			Assert(!package.IsNonePackedItemRelationship);
			Assert(package.IsOnePackedItemRelationship);
			Assert(!package.IsManyPackedItemRelationship);
		}

		public void TestDeletionInCorrectOrder()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			bill.ContainerPK = container.PK;
			var pivot = bill.Pivot;
			var pack = bill.Packs.AddNew();
			var packedItems = bill.PackedItems.AddNew();
			Factory.Save();

			pack.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (bill.IsDeleted)
				{
					Assert("This is wrong Pack should not be deleted after the Bill", false);
				}
			};

			bill.Delete();
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(Factory.Save);
				AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaBill)));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaContainerBillOrPackageLink)));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaPack)));
				AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaPackedItem)));
			});
		}

		public void TestHeaderAndClusterKey()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertNull(bill.Header);
			AssertEquals(0, bill.ABL_ClusterKey);

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			bill.ABL_AMA = manifestHeader.PK;
			AssertEquals(0, bill.ABL_ClusterKey);
			AssertEquals(manifestHeader.PK, bill.Header.PK);

			Factory.Save();
			AssertEquals("ClusterKey generated OnSaving for manifestHeader.", 1, manifestHeader.AMA_ClusterKey);
			AssertEquals("ClusterKey generated OnSaving and same as manifestHeader.", 1, bill.ABL_ClusterKey);
		}

		public void TestContainer()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "XXX";
			bill.ContainerPK = container.PK;
			var linkQry = new ZQuery(AsycudaContainerBillOrPackageLinkSchema.APC_ABL_Bill, bill.PK);
			var link = Factory.Load<AsycudaContainerBillOrPackageLink>(linkQry)[0];
			CombineAssertions(() =>
			{
				AssertEquals("bill link", link.APC_ABL_Bill, bill.PK);
				AssertEquals("container link", link.APC_ACN_Container, container.PK);
				AssertEquals("container header", container.ACN_AMA_Manifest, header.PK);
				AssertEquals("container name", container.ACN_ContainerNumber, "XXX");
			});
			bill.ContainerPK = ZGuid.Empty;
			AssertNull(bill.Pivot);
		}

		public void TestIsCancelled()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertNull(bill.Header);
			AssertEquals(false, bill.IsCancelled);

			bill.IsCancelled = true;
			AssertEquals(true, bill.IsCancelled);

			bill.IsCancelled = false;
			AssertEquals(false, bill.IsCancelled);

			var header = Factory.New<AsycudaManifestHeader>();
			AssertEquals(false, header.IsCancelled);

			var bill1 = header.Bills.AddNew();
			AssertEquals(false, bill1.IsCancelled);
			var bill2 = header.Bills.AddNew();
			AssertEquals(false, bill2.IsCancelled);

			header.IsCancelled = true;
			AssertEquals(true, header.IsCancelled);
			// - All attached Bills are deactivated
			AssertEquals(true, bill1.IsCancelled);
			AssertEquals(true, bill2.IsCancelled);

			header.IsCancelled = false;
			AssertEquals(false, header.IsCancelled);
			// - All attached Bills are activated
			AssertEquals(false, bill1.IsCancelled);
			AssertEquals(false, bill2.IsCancelled);
		}

		public void TestIsCancelled_CanReactivate()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertNull(bill.Header);
			AssertEquals(false, bill.IsCancelled);
			AssertNull(bill.CanReactivate());

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "MAN00123";
			bill.ABL_AMA = header.PK;

			AssertEquals(false, header.IsCancelled);
			AssertNull(bill.CanReactivate());

			header.IsCancelled = true;
			AssertEquals("Cannot activate.  This Bill is on Canceled Manifest MAN00123.", bill.CanReactivate());
		}

		public void TestShipperABLAddress()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ShipperName = "SHIPPER";
			bill.ABL_ShipperStreet1 = "STREET 1";
			bill.ABL_ShipperStreet2 = "STREET 2";
			bill.ABL_ShipperCity = "CT";
			bill.ABL_ShipperState = "ST";
			bill.ABL_ShipperPostcode = "4343";
			bill.ABL_RN_NKShipperCountry = "NZ";
			bill.ABL_ShipperPhone = "+4234232";
			CombineAssertions(() => AssertABLAddress(bill.ShipperABLAddress, "SHIPPER", "STREET 1", "STREET 2", "CT", "ST", "4343", "NZ", "+4234232"));
		}

		public void TestConsigneeABLAddress()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeName = "CONSIGNEE";
			bill.ABL_ConsigneeStreet1 = "STREET 1";
			bill.ABL_ConsigneeStreet2 = "STREET 2";
			bill.ABL_ConsigneeCity = "CT";
			bill.ABL_ConsigneeState = "ST";
			bill.ABL_ConsigneePostcode = "4343";
			bill.ABL_RN_NKConsigneeCountry = "NZ";
			bill.ABL_ConsigneePhone = "+4234232";
			CombineAssertions(() => AssertABLAddress(bill.ConsigneeABLAddress, "CONSIGNEE", "STREET 1", "STREET 2", "CT", "ST", "4343", "NZ", "+4234232"));
		}

		public void TestNotifyPartyABLAddress()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_NotifyPartyName = "NOTIFY PARTY";
			bill.ABL_NotifyPartyStreet1 = "STREET 1";
			bill.ABL_NotifyPartyStreet2 = "STREET 2";
			bill.ABL_NotifyPartyCity = "CT";
			bill.ABL_NotifyPartyState = "ST";
			bill.ABL_NotifyPartyPostcode = "4343";
			bill.ABL_RN_NKNotifyPartyCountry = "NZ";
			bill.ABL_NotifyPartyPhone = "+4234232";
			CombineAssertions(() => AssertABLAddress(bill.NotifyPartyABLAddress, "NOTIFY PARTY", "STREET 1", "STREET 2", "CT", "ST", "4343", "NZ", "+4234232"));
		}

		void AssertABLAddress(AsycudaBillAddress address, string name, string address1, string address2, string city, string state, string postcode, string country, string phone)
		{
			AssertEquals("Name", name, address.CompanyName);
			AssertEquals("Address1", address1, address.Address1);
			AssertEquals("Address2", address2, address.Address2);
			AssertEquals("City", city, address.City);
			AssertEquals("State", state, address.State);
			AssertEquals("Postcode", postcode, address.Postcode);
			AssertEquals("RN_NKCountryCode", country, address.RN_NKCountryCode);
			AssertEquals("Phone", phone, address.Phone);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "X";
			header.AMA_RN_NKCountry = "SB";
			header.AMA_Nature = "ABC";
			var bill = header.Bills.AddNew();
			bill.ABL_RL_NKOrigin = "SBHIR";
			return bill;
		}
	}
}
