using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class JobDeclarationInlandTransportResDataHelperTest : TestCaseWithFactory
	{
		public void TestGetInlandTransactionRes()
		{
			CombineAssertions(() =>
			{
				var resourceStringData = JobDeclarationInlandTransportResDataHelper.GetInlandTransactionRes(TransportTypeList.Codes.Sea, TransportMeansList.Codes.ImoShipIdentificationNumber);
				AssertEquals("SEA - 10 - Caption", "IMO No.", resourceStringData.Caption);
				AssertEquals("SEA - 10 - FullDescription", "Lloyds / IMO Number", resourceStringData.FullDescription);

				resourceStringData = JobDeclarationInlandTransportResDataHelper.GetInlandTransactionRes(TransportTypeList.Codes.Sea, TransportMeansList.Codes.NameOfTheSeaGoingVessel);
				AssertEquals("SEA - 11 - Caption", "Vessel Name", resourceStringData.Caption);

				resourceStringData = JobDeclarationInlandTransportResDataHelper.GetInlandTransactionRes(TransportTypeList.Codes.Air, TransportMeansList.Codes.ImoShipIdentificationNumber);
				AssertEquals("Other - Caption", "Transport ID", resourceStringData.Caption);
			});
		}
	}
}
