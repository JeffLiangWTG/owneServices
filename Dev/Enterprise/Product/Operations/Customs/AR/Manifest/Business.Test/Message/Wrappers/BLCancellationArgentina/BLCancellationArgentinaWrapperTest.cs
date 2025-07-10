using System.Linq;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class BLCancellationArgentinaWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestBLCancellationArgentinaWrapper()
		{
			CreateAndPopulateManifestHeader();

			Factory.Save();

			ICancellation wrapper = new BLCancellationArgentinaWrapper(header.Bills[0]);
			IPort portWrapper = wrapper.HBLPorts.FirstOrDefault();

			CombineAssertions(() =>
			{
				AssertEquals("2021080000000010", wrapper.VoyageID);

				AssertEquals(1, wrapper.HBLPorts.Count);

				AssertEquals("ARBUE", portWrapper.LoadingPort);
				AssertEquals("(H)QRHW20050055C", portWrapper.HBLNumber);
				AssertEquals("", wrapper.Authentication.AgentType);
				AssertEquals("", wrapper.Authentication.CompanyCUIT);
				AssertEquals("", wrapper.Authentication.CompanyRol);
			});
		}

		void CreateAndPopulateManifestHeader()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.AMA_CustomsLoadPort = "ARBUE";
			header.RegistrationNumber = "2021080000000010";

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "(H)QRHW20050055C";
		}
	}
}
