using System.Linq;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class BLSendChileWrapperLocationTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestBLSendChileWrapperLocation()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CreateAndPopulateHouseBill();
			CreateAndPopulateMasterBill();

			IBLRequest wrapper = new BLSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A);

			IDocumentLocation location1 = wrapper.Locations.ElementAt(0);
			IDocumentLocation location2 = wrapper.Locations.ElementAt(1);
			IDocumentLocation location3 = wrapper.Locations.ElementAt(2);
			IDocumentLocation location4 = wrapper.Locations.ElementAt(3);
			IDocumentLocation location5 = wrapper.Locations.ElementAt(4);
			IDocumentLocation location6 = wrapper.Locations.ElementAt(5);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(WrappersConstants.LocationName.Le, location1.Name);
				AssertEquals("UYMVD", location1.Code);
				AssertEquals("Montevideo", location1.Description);
				AssertEquals(WrappersConstants.LocationName.Pe, location2.Name);
				AssertEquals("UYMVD", location2.Code);
				AssertEquals("Montevideo", location2.Description);
				AssertEquals(WrappersConstants.LocationName.Pd, location3.Name);
				AssertEquals("CLSCL", location3.Code);
				AssertEquals("Santiago", location3.Description);
				AssertEquals(WrappersConstants.LocationName.Ld, location4.Name);
				AssertEquals("CLSCL", location4.Code);
				AssertEquals("Santiago", location4.Description);
				AssertEquals(WrappersConstants.LocationName.Lem, location5.Name);
				AssertEquals("CLSCL", location5.Code);
				AssertEquals("Santiago", location5.Description);
				AssertEquals(WrappersConstants.LocationName.Lrm, location6.Name);
				AssertEquals("UYMVD", location6.Code);
				AssertEquals("Montevideo", location6.Description);
			});
		}

		void CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			bill.ABL_GrossWeight = 127.000m;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_ManifestQty = 1;
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			bill.ABL_RL_NKFinalDestination = "CLSCL";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RoRo = true;
			bill.ABL_Volume = 1000m;
			bill.ABL_VolumeUQ = "M3";
		}

		void CreateAndPopulateMasterBill()
		{
			AsycudaBill bill = (AsycudaBill)header.MasterBill;
			bill.ABL_BillNumber = "21061996";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RL_NKPortOfLoading = "UYMVD";
		}
	}
}
