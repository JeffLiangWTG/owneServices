using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class HouseWaybillWrapperTest : TestCaseWithFactory
	{
		public void TestHouseWaybillWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			IHouseWaybill wrapper = new HouseWaybillWrapper(bill, MessageSubTypeCodes.Codes.Original);
			CombineAssertions(() =>
			{
				AssertNotNull(wrapper.MessageHeader);
				AssertNotNull(wrapper.BusinessHeader);
				AssertNotNull(wrapper.MasterConsignment);
			});
		}
	}
}
