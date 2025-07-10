using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.CLManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestBill()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaBill>(pack.Bill);
		}

		public void TestLookups()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaPackLookups>(pack.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			return pack;
		}

		public void TestDelete()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var arrivalLine = arrivalHeader.ArrivalDetails.AddNew();

			arrivalLine.ATL_ABL_AsycudaBill = bill.PK;
			arrivalLine.ATL_APA_AsycudaPack = pack.PK;

			pack.Delete();
			Assert(arrivalLine.IsDeleted);
		}

		public void TestPackCantBeDeleteWhenStatusIsAccepted()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACP";

			var pack = bill.Packs.AddNew();

			AssertEquals("This Bill is already sent and accepted.", pack.ReasonForNotAbleToDelete);
			AssertEquals(false, pack.CanDelete);

			bill.ABL_BillStatus = "";
			AssertEquals(true, pack.CanDelete);
		}

		public void TestPackCantBeDeleteWhenStatusIsWaiting()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";

			var pack = bill.Packs.AddNew();

			AssertEquals("This Bill is already sent and it is awaiting for a response.", pack.ReasonForNotAbleToDelete);
			AssertEquals(false, pack.CanDelete);

			bill.ABL_BillStatus = "";
			bill.ABL_MessageStatus = "";
			AssertEquals(true, pack.CanDelete);
		}

		public void TestBillCantBeDeleteWhenStatusIsCancelled()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "CAN";

			var pack = bill.Packs.AddNew();

			AssertEquals("This bill is canceled. Packs must be kept for history purposes.", pack.ReasonForNotAbleToDelete);
			AssertEquals(false, pack.CanDelete);

			bill.ABL_BillStatus = "";
			AssertEquals(true, pack.CanDelete);
		}

		public void TestPackUQIsNotConverted()
		{
			var cusRefPack = Factory.New<CusRefPacks>();
			cusRefPack.RP_CustomsPack = "BT";
			cusRefPack.RP_Type = "GMB";
			cusRefPack.RP_CustomsCountry = "CL";
			cusRefPack.RP_ConversionFactor = 2;
			cusRefPack.RP_CommercialPack = "PCS";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line1.JL_F3_NKPackType = "PCS";

			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			var bill = manifestHeader.Bills[0];
			var pack = bill.Packs[0];
			CombineAssertions(() =>
			{
				AssertEquals("APA_PackUQ is not converted", 10, pack.APA_PackQty);
				AssertEquals("APA_PackUQ is not converted", "PCS", pack.APA_PackUQ);
			});
		}
	}
}

