using System.Linq;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBSendChileWrapperObservationTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestAWBSendChileWrapperObservation()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CreateAndPopulateHouseBill();

			IAWBRequest wrapper = new AWBSendChileWrapper(header.Bills[0], WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Mot, "ACL: SE MODIFICA TIPO SERVICIO DE FCL/LCL A FCL/FCL");
			IDocObservations observation = wrapper.DocObservations.ElementAt(0);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(WrappersConstants.ObservationName.Mot, observation.Name);
				AssertEquals("ACL: SE MODIFICA TIPO SERVICIO DE FCL/LCL A FCL/FCL", observation.Content);
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
			bill.ABL_RL_NKFinalDestination = "CLSCL";
			bill.ABL_RL_NKOrigin = "UYMVD";
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RoRo = true;
			bill.ABL_Volume = 1000m;
			bill.ABL_VolumeUQ = "M3";
		}
	}
}
