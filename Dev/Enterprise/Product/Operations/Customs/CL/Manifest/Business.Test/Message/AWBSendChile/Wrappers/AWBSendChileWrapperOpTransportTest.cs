using CargoWise.Customs.CL.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class AWBSendChileWrapperOpTransportTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestAWBSendChileWrapperOpTransport()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			AsycudaBill bill = header.Bills.AddNew();
			IAWBRequest wrapper = new AWBSendChileWrapper(bill, WrappersConstants.ActionType.A, WrappersConstants.ObservationName.Mot);
			IDocOpTransport docOpTransport = wrapper.DocOpTransport;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(WrappersConstants.OperationType.I, docOpTransport.Nature);
				AssertEquals("Voyage", docOpTransport.VoyageName);
				AssertEquals("TR", docOpTransport.TransshipmentType);
			});
		}

		void PopulateManifestHeader()
		{
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_TransshipmentType = TransshipmentTypeCodeList.Codes.TR;
			header.AMA_Voyage = "Voyage";
		}
	}
}
