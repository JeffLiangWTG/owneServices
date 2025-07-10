using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class BLSendChileWrapperOpTransportTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestBLSendChileWrapperOpTransport()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();
			CreateAndPopulateHouseBill(true);

			IBLRequest wrapper = new BLSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A);
			IDocumentOpTransport documentOpTransport = wrapper.DocumentOpTransport;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(WrappersConstants.OperationType.I, documentOpTransport.Nature);
				AssertEquals("NKVesselDemo", documentOpTransport.VesselName);
			});
		}

		void PopulateManifestHeader()
		{
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_VesselName = "NKVesselDemo";
		}

		void CreateAndPopulateHouseBill(bool roro)
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
			bill.ABL_RoRo = roro;
			bill.ABL_Volume = 1000m;
			bill.ABL_VolumeUQ = "M3";
		}
	}
}
