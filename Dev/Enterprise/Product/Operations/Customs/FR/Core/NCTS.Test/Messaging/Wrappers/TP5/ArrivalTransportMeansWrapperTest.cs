using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class ArrivalTransportMeansWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalTransportMeansWrapper>
	{
		public void TestTypeOfIdentification()
		{
			AssertEquals("TypeOfIdentification should be mapped to TPM_TypeOfIdentification.", "12", Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should be mapped to TPM_IdentificationNumber.", "510PZ47", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			AssertEquals("Nationality should be mapped to TPM_RN_NKTransportNationality.", Core.Constants.CountryCodes.France, Provider.Nationality);
		}

		protected override ArrivalTransportMeansWrapper GetProvider()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;

			var arrivalTransportInfos = movementHeader.ArrivalTransportInfos.AddNew();
			arrivalTransportInfos.TPM_TypeOfIdentification = "12";
			arrivalTransportInfos.TPM_IdentificationNumber = "510PZ47";
			arrivalTransportInfos.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.France;

			return ArrivalTransportMeansWrapper.New(arrivalTransportInfos);
		}
	}
}
