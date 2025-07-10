using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class ArrivalTransportMeansWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalTransportMeansWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal TPM_IdentificationNumber.", "510PZ47", Provider.IdentificationNumber);
		}

		public void TestTypeOfIdentification()
		{
			AssertEquals("TypeOfIdentification should equal TPM_TypeOfIdentification.", (byte)99, Provider.TypeOfIdentification);
		}

		protected override ArrivalTransportMeansWrapper GetProvider()
		{
			var arrivalTransportMeans = Factory.New<CusTransportMeans>();
			arrivalTransportMeans.TPM_IdentificationNumber = "510PZ47";
			arrivalTransportMeans.TPM_TypeOfIdentification = "99";
			return ArrivalTransportMeansWrapper.New(arrivalTransportMeans);
		}
	}
}
