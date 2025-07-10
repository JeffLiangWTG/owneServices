using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(ABLEntryNum))]
	sealed class ABLEntryNumTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDeletionInCorrectOrder()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var ablEntryNum = bill.CustomsEntryNumbers.AddNew();
			var pivot = ablEntryNum.PackPivots.AddPivotFor(pack);
			Factory.Save();

			pivot.NotificationsChanged += (object sender, NotificationsChangedEventArgs e) =>
			{
				if (!ablEntryNum.IsDeleted && pivot.IsDeleted)
				{
					var newPivot = Factory.New<ABLEntryNumRelatedPacksGenPivot>();
					newPivot.Relation1Object = ablEntryNum;
				}
			};
			ablEntryNum.Delete();
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestWeightInKilos()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack1 = bill.Packs.AddNew();
			pack1.APA_Weight = 10m;
			pack1.APA_WeightUQ = "KG";
			var pack2 = bill.Packs.AddNew();
			pack2.APA_Weight = 10m;
			pack2.APA_WeightUQ = "T";

			var entryNum = (ABLEntryNum)GetNewBusinessObject();
			AssertEquals(0m, entryNum.WeightInKilos);

			entryNum.PackPivots.AddPivotFor(pack1);
			AssertEquals(10m, entryNum.WeightInKilos);

			entryNum.PackPivots.AddPivotFor(pack2);
			AssertEquals(10010m, entryNum.WeightInKilos);
		}

		public void TestNumberOfPackages()
		{
			var pack1 = Factory.New<AsycudaPack>();
			pack1.APA_PackQty = 10;
			var pack2 = Factory.New<AsycudaPack>();
			pack2.APA_PackQty = 2;

			var entryNum = (ABLEntryNum)GetNewBusinessObject();
			AssertEquals(0m, entryNum.NumberOfPackages);

			entryNum.PackPivots.AddPivotFor(pack1);
			AssertEquals(10m, entryNum.NumberOfPackages);

			entryNum.PackPivots.AddPivotFor(pack2);
			AssertEquals(12m, entryNum.NumberOfPackages);
		}

		public void TestFirstPackUQ()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack1 = bill.Packs.AddNew();
			pack1.APA_PackUQ = "BAG";
			var pack2 = bill.Packs.AddNew();
			pack2.APA_PackUQ = "BOX";

			var entryNum = (ABLEntryNum)GetNewBusinessObject();
			AssertEquals(ZString.Empty, entryNum.FirstPackUQ);

			entryNum.PackPivots.AddPivotFor(pack1);
			AssertEquals("BAG", entryNum.FirstPackUQ);

			entryNum.PackPivots.AddPivotFor(pack2);
			AssertContains(entryNum.FirstPackUQ, "BAGBOX");
		}

		public void TestDelete()
		{
			var entryNum = (ABLEntryNum)GetNewBusinessObject();
			var pivot = entryNum.PackPivots.AddNew();
			entryNum.Delete();
			Assert(pivot.IsDeleted);
		}

		public void TestDefaultWhenOnlyOnePack()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var entryNum = bill.CustomsEntryNumbers.AddNew();

			AssertEquals("Pack Defaulted to ABLEntryNum", pack.PK, entryNum.PackPivots[0].XX_Relation2ID);
		}

		public void TestDefaultPacksOnFactorySaving()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "RFM");
			header.FillWithValidTestData();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;

			var bill = header.Bills.AddNew();
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryNum = "ABC123";
			var entryNum2 = bill.CustomsEntryNumbers.AddNew();
			AssertEquals("Precondition: No Packs", 0, entryNum.PackPivots.Count);

			var pack = bill.Packs.AddNew();
			AssertEquals("Still no Packs", 0, entryNum.PackPivots.Count);

			Factory.Save();
			AssertEquals("Pack Defaulted to ABLEntryNum", pack, entryNum.PackPivots[0].Relation2Object);
		}

		public void TestNumberOfPackagesException()
		{
			var headerOut = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "RFM");
			headerOut.FillWithValidTestData();
			headerOut.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var billOut = headerOut.Bills.AddNew();
			var entryNumOut = billOut.CustomsEntryNumbers.AddNew();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "RFM");
			header.FillWithValidTestData();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;

			var bill = header.Bills.AddNew();
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			var entryNum2 = bill.CustomsEntryNumbers.AddNew();
			AssertEquals("Precondition: No Packs", 0, entryNum.PackPivots.Count);

			var pack1 = entryNum.Bill.Packs.AddNew();
			var pack2 = entryNum.Bill.Packs.AddNew();

			var pivot1 = entryNum.PackPivots.AddPivotFor(pack1);
			var pivot2 = entryNum.PackPivots.AddPivotFor(pack2);

			AssertEquals("EntryNumber.Packs", 2, entryNum.PackPivots.Count);

			var newTobeDelPack = billOut.Packs.AddNew();
			newTobeDelPack.APA_PackQty = 4;
			newTobeDelPack.APA_GoodsDescription = "OUT";

			entryNum.PackPivots[0].Relation2Object = newTobeDelPack;
			billOut.Delete();

			AssertNull(entryNum.PackPivots[0].Relation2Object);
			AssertNoExceptionThrown(() => { var test = entryNum.NumberOfPackages; });
		}

		public void TestHumanReadableNameCore()
		{
			EntryNum.CE_EntryNum = "LRN1234";
			EntryNum.CE_EntryType = CusEntryNumberTypes.Standard.LocalReferenceNumber;
			AssertEquals("LRN1234 (LRN)", EntryNum.HumanReadableName);
		}

		public void TestCountryCodeIsPopulated()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			AssertEquals("CusEntryNum’s CE_RN_NKCountryCode is populated", header.AMA_RN_NKCountry, entryNum.CE_RN_NKCountryCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = header.Bills.AddNew();
			bill.FillWithValidTestData();
			var entryNum = bill.CustomsEntryNumbers.AddNew();
			entryNum.CE_EntryNum = "LRN1234";
			return entryNum;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		ABLEntryNum entryNum;
		ABLEntryNum EntryNum => entryNum ?? (entryNum = (ABLEntryNum)GetNewBusinessObject());
	}
}
