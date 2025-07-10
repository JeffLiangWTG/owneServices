using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBSendChileWrapperLocationTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestAWBSendChileWrapperLocation()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			CreateAndPopulateMasterBill();

			IAWBRequest wrapper = new AWBSendChileWrapper(bill, WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Mot);

			IDocLocations location1 = wrapper.DocLocations.ElementAt(0);
			IDocLocations location2 = wrapper.DocLocations.ElementAt(1);

			Factory.Save();

			CombineAssertions(() =>
			{
				Assert(wrapper.DocLocations.IsCountEqualTo(2));

				AssertEquals(WrappersConstants.LocationName.Pe, location1.Name);
				AssertEquals("UYMVD", location1.Code);
				AssertEquals(WrappersConstants.LocationName.Pd, location2.Name);
				AssertEquals("CLSCL", location2.Code);
			});
		}

		void CreateAndPopulateMasterBill()
		{
			AsycudaBill bill = (AsycudaBill)header.MasterBill;
			bill.ABL_RL_NKPortOfDischarge = "CLSCL";
			bill.ABL_RL_NKPortOfLoading = "UYMVD";
		}
	}
}
