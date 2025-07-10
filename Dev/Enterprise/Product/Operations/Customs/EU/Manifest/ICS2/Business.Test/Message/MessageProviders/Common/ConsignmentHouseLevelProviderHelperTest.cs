using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class ConsignmentHouseLevelProviderHelperTest : TestCaseWithFactory
	{
		public void TestGoodsItems_ShouldBeInOrder()
		{
			var bill = Factory.New<AsycudaBill>();
			AddPack(3);
			AddPack(2);
			AddPack(4);
			AddPack(1);

			var helper = new ConsignmentHouseLevelProviderHelper(bill);
			var goodsItemsCollection = helper.GoodsItems;

			AssertContainsExactElementsInExactOrder([1, 2, 3, 4], goodsItemsCollection.Select(i => i.GoodsItemNumber));

			void AddPack(ZShort lineNumber)
			{
				var pack = bill.Packs.AddNew();
				pack.APA_LineNo = lineNumber;
			}
		}

		public void TestCarrierIdentificationNumberWithIgnoredCountry()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var carrierOrg = Factory.New<OrgHeader>();

			carrierOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", CountryCodes.Germany);
			carrierOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "654321", CountryCodes.UnitedKingdom);

			var carrier = carrierOrg.MainAddress;
			bill.Header.AMA_OA_Carrier = carrier.PK;

			var helper = new ConsignmentHouseLevelProviderHelper(bill);
			AssertEquals("CarrierIdentificationNumber", "DE123456", helper.CarrierIdentificationNumber);
		}

		public void TestContainersAddedToHeaderButNotPack()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var container1 = header.Containers.AddNew();
			container1.ACN_ContainerNumber = "C1";
			var container2 = header.Containers.AddNew();
			container2.ACN_ContainerNumber = "C2";

			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container1.PK;

			var helper = new ConsignmentHouseLevelProviderHelper(bill);
			var transportEquipmentCollection = helper.TransportEquipmentCollection;
			AssertContainsExactElementsInExactOrder(["C1"], transportEquipmentCollection.Select(i => i.ContainerIdentificationNumber));
		}
	}
}
