using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPackedItemsForBinding()
		{
			var pack = Factory.New<AsycudaPack>();
			var packedItems = pack.PackedItemsForBinding;
			CombineAssertions(() =>
			{
				AssertType<AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaPack>>(packedItems);
				AssertEquals("IsRegisteredEditableChildObject", true, pack.IsRegisteredEditableChildObject(packedItems));
			});
		}

		public void TestSetContainer()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CON1";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			pack.SetContainer("CON1", header);
			AssertEquals("Pack line's container PK is set", container.PK, pack.ContainerPK);
		}

		public void TestPackedItems()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals(pack.PackedItems.GetType(), typeof(AsycudaPackPackedItemPivotCollection));
		}

		public void TestHasManifestBeenSubmittedToCustomsIncludingChildren()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals(false, pack.HasManifestBeenSubmittedToCustomsIncludingChildren);
			var packedItem1 = pack.PackedItemForTesting();
			var packedItem2 = pack.CreatePackedItemForTesting();
			AssertEquals(false, pack.HasManifestBeenSubmittedToCustomsIncludingChildren);
			packedItem2.API_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals(false, pack.HasManifestBeenSubmittedToCustomsIncludingChildren);
			packedItem1.API_MessageStatus = MessageStatusCodeList.Codes.Awaiting;
			AssertEquals(true, pack.HasManifestBeenSubmittedToCustomsIncludingChildren);
		}

		public void TestDeletionInCorrectOrder()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var undg = pack.UNDGs.AddNew();
			undg.DI_DG = ZGuid.Empty;
			Factory.Save();

			EventHandler<NotificationsChangedEventArgs> handler = (object sender, NotificationsChangedEventArgs e) =>
			{
				if (pack.IsDeleted)
				{
					Assert("This is wrong UNDGs should not be deleted after the Pack", false);
				}
			};
			undg.NotificationsChanged += handler;
			pack.Delete();

			// For UNDGDataItem, an even NotificationsChanged will be triggered when "pack.Delete()" was called.
			// The event NotificationsChanged also will be triggered when UNDGSubstancePivotCollection is cleared.
			// Now we have added code to delete dbo.UNDGSubstancePivot items when their parent UNDGDataItem is deleted.
			// And becasue DelayListChangedEvents, the event will be triggered during Factory.Save instead of in "pack.Delete()" function.
			// So here need to remove the event handler before Factory.Save,
			// otherwise a false assert failure "This is wrong UNDGs should not be deleted after the Pack" would happen.
			// If there was a event which could tell us when a BusinessObject is deleted,
			// we could have a better way to handle this assertion.
			undg.NotificationsChanged -= handler;
			AssertNoExceptionThrown(Factory.Save);
			AssertEquals(0, Factory.GetDatabaseCount(typeof(AsycudaPack)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(UNDGDataItem)));
		}

		public void TestAttachToCustomsNumbers()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			header.FillWithValidTestData();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = header.Bills.AddNew();
			bill.CustomsEntryNumber = "1234";
			bill.CustomsEntryNumberType = CusEntryNumberTypes.Standard.LocalReferenceNumber;

			AssertEquals("No packs exist as yet", 0, bill.CustomsEntryNumbers[0].PackPivots.Count);

			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			AssertEquals("Auto default packs since there is only one bill", 2, bill.CustomsEntryNumbers[0].PackPivots.Count);
		}

		public void TestConsignmentReference()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("pack.ConsignmentReferenceInfo.ReadOnly", true, pack.ConsignmentReferenceInfo.ReadOnly);
			pack.ConsignmentReference = 293;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var packInDiffFactory = newFactory.Load<AsycudaPack>(pack.PK);
			AssertEquals("packInDiffFactory.ConsignmentReference", 293, packInDiffFactory.ConsignmentReference);
		}
		public void TestUNDGs()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = header.Bills.AddNew().Packs.AddNew();
			pack.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			AssertEquals("0004a", pack.UNDGs.FirstItemForBinding[0].UNDGSubstance.DG_Code);
			Factory.Save();
			var packReloaded = new BusinessObjectFactory().Load<AsycudaPack>(pack.PK);
			AssertEquals("0004a", packReloaded.UNDGs.FirstItemForBinding[0].UNDGSubstance.DG_Code);
		}

		public void TestBill()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals(bill, pack.Bill);
		}

		public void TestPopulateDefaults()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BAG", "BAG 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var pack = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaPack)bill.Packs.AddNew();
			pack.APA_GoodsDescription = "Description";
			pack.APA_PackQty = 5;
			pack.APA_PackUQ = "BAG";
			pack.LinePrice = 4.0;
			header.AMA_RL_NKPortOfLoading = "SGAPI";

			var packedItem = pack.PackedItem;

			AssertEquals("Goods Description", pack.APA_GoodsDescription, packedItem.API_GoodsDescription);
			AssertEquals("Customs Qty", (ZDecimal)pack.APA_PackQty, packedItem.API_CustomsQty);
			AssertEquals("(Customs) UQ", pack.APA_PackUQ, packedItem.API_CustomsUQ);
		}

		public void TestAssignConsignmentReferenceNumbers()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill1Packs = bill1.Packs;

			var pack1 = bill1Packs.AddNew();
			Factory.Save();
			AssertEquals(1, pack1.ConsignmentReference);

			var pack2 = bill1Packs.AddNew();
			Factory.Save();
			AssertEquals(2, pack2.ConsignmentReference);

			pack2.Delete();
			Factory.Save();

			var pack3 = bill1Packs.AddNew();
			Factory.Save();
			AssertEquals(3, pack3.ConsignmentReference);

			var bill2 = header.Bills.AddNew();
			var bill2Packs = bill2.Packs;

			var pack4 = bill2Packs.AddNew();
			Factory.Save();
			AssertEquals(4, pack4.ConsignmentReference);
		}

		public void TestCodeProperty()
		{
			var pack = Factory.New<AsycudaPack>();
			pack.APA_PackQty = 69;
			pack.APA_GoodsDescription = "Meal for Two";
			AssertEquals("69 Meal for Two", pack.CodeProperty);
		}

		public void TestLinePriceUpdatesPackedItem()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			pack.LinePrice = 1000m;
			pack.LinePriceCurrency = Core.Constants.CurrencyCodes.Singapore;

			var packedItem = pack.GetSGPackedItemForTesting();
			AssertEquals(pack.LinePrice, packedItem.API_CustomsValue);

			pack.LinePriceCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals(402.88m, packedItem.API_CustomsValue);

			pack.LinePrice = 0m;
			AssertEquals(0m, packedItem.API_CustomsValue);
		}

		public void TestGoodsDescriptionUpdatesPackedItem()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var pack = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaPack)bill.Packs.AddNew();

			pack.APA_GoodsDescription = "TEST TO PACK COUNTRY";
			var packedItem = pack.PackedItem;
			AssertEquals("TEST TO PACK COUNTRY", packedItem.API_GoodsDescription);

			pack.APA_GoodsDescription = "CHANGED GOODS DESCRIPTION";
			AssertEquals("TEST TO PACK COUNTRY", packedItem.API_GoodsDescription);
		}

		public void TestQtyAndUQUpdatesPackedItem()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "BAG", "BAG 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var bill = header.Bills.AddNew();
			var pack = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaPack)bill.Packs.AddNew();

			pack.APA_PackQty = 10;
			var packedItem = pack.PackedItem;
			AssertEquals(10m, packedItem.API_CustomsQty);
			AssertEquals("NMB", packedItem.API_CustomsUQ);

			pack.APA_PackUQ = "BAG";
			AssertEquals(10m, packedItem.API_CustomsQty);
			AssertEquals("BAG", packedItem.API_CustomsUQ);

			packedItem.API_CustomsQty = ZDecimal.Zero;
			packedItem.API_CustomsUQ = ZString.Empty;

			pack.APA_PackUQ = "ZZZ";
			AssertEquals(10m, packedItem.API_CustomsQty);
			AssertEquals("NMB", packedItem.API_CustomsUQ);

			pack.APA_PackUQ = "BAG";
			AssertEquals(10m, packedItem.API_CustomsQty);
			AssertEquals("BAG", packedItem.API_CustomsUQ);

			pack.APA_PackQty = 20;
			AssertEquals(20m, packedItem.API_CustomsQty);
			AssertEquals("BAG", packedItem.API_CustomsUQ);
			pack.APA_PackUQ = "MMM";
			AssertEquals(20m, packedItem.API_CustomsQty);
			AssertEquals("NMB", packedItem.API_CustomsUQ);
		}

		public void TestAPA_LineNo()
		{
			var header = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			var pack3 = bill.Packs.AddNew();
			var pack4 = bill.Packs.AddNew();
			var pack5 = bill.Packs.AddNew();
			AssertEquals((ZShort)1, pack1.APA_LineNo);
			AssertEquals((ZShort)2, pack2.APA_LineNo);
			AssertEquals((ZShort)3, pack3.APA_LineNo);
			AssertEquals((ZShort)4, pack4.APA_LineNo);
			AssertEquals((ZShort)5, pack5.APA_LineNo);
			pack2.Delete();
			AssertEquals((ZShort)1, pack1.APA_LineNo);
			AssertEquals((ZShort)2, pack3.APA_LineNo);
			AssertEquals((ZShort)3, pack4.APA_LineNo);
			AssertEquals((ZShort)4, pack5.APA_LineNo);
			var pack6 = bill.Packs.AddNew();
			pack6.APA_LineNo = 6;
			AssertEquals((ZShort)5, pack6.APA_LineNo);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			return pack;
		}
	}
}
