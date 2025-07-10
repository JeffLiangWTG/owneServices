using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSBorderTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSBorderTransportMeansProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(NCTSBorderTransportMeansProvider.NewOrNull((NctsCommonMovementHeader)null));
		}

		public void TestNewOrNull_CusTransportMeans()
		{
			AssertNull(NCTSBorderTransportMeansProvider.NewOrNull((DepartureCusTransportMeans)null));
		}

		public void TestTypeOfIdentification()
		{
			AssertEquals(NctsTransportTypeOfIdList.Codes._21, Provider.TypeOfIdentification);
		}

		public void TestTypeOfIdentification_CusTransportMeans()
		{
			var provider = NCTSBorderTransportMeansProvider.NewOrNull(GetCusTransportMeans());
			AssertEquals(NctsTransportTypeOfIdList.Codes._10, provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("DE1234567", Provider.IdentificationNumber);
		}

		public void TestIdentificationNumber_CusTransportMeans()
		{
			var provider = NCTSBorderTransportMeansProvider.NewOrNull(GetCusTransportMeans());
			AssertEquals("DE2345678", provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			AssertEquals(Core.Constants.CountryCodes.Germany, Provider.Nationality);
		}

		public void TestNationality_CusTransportMeans()
		{
			var provider = NCTSBorderTransportMeansProvider.NewOrNull(GetCusTransportMeans());
			AssertEquals(Core.Constants.CountryCodes.France, provider.Nationality);
		}

		public void TestConveyanceReferenceNumber()
		{
			AssertEquals("CN001", Provider.ConveyanceReferenceNumber);
		}

		public void TestConveyanceReferenceNumber_CusTransportMeans()
		{
			var provider = NCTSBorderTransportMeansProvider.NewOrNull(GetCusTransportMeans());
			AssertEquals("RN001", provider.ConveyanceReferenceNumber);
		}

		public void TestCustomsOfficeAtBorder()
		{
			AssertEquals("DE001", Provider.CustomsOfficeAtBorder);
		}

		public void TestCustomsOfficeAtBorder_CusTransportMeans()
		{
			var provider = NCTSBorderTransportMeansProvider.NewOrNull(GetCusTransportMeans());
			AssertEquals("DE002", provider.CustomsOfficeAtBorder);
		}

		protected override NCTSBorderTransportMeansProvider GetProvider() => NCTSBorderTransportMeansProvider.NewOrNull(GetMovementHeader());

		NctsCommonMovementHeader GetMovementHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_ActiveBorderIdentificationType = NctsTransportTypeOfIdList.Codes._21;
			movementHeader.BM_TOLCarrierID = "DE1234567";
			movementHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Germany;
			movementHeader.BM_ConveyanceNumber = "CN001";
			movementHeader.BM_CustomsOfficeAtBorder = "DE001";
			return movementHeader;
		}

		DepartureCusTransportMeans GetCusTransportMeans()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var transportMeans = nctsHeader.MovementHeader.AdditionalTransportAtBorderList.AddNew();
			transportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._10;
			transportMeans.TPM_IdentificationNumber = "DE2345678";
			transportMeans.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.France;
			transportMeans.TPM_ReferenceNumber = "RN001";
			transportMeans.TPM_CustomsOffice = "DE002";
			return transportMeans;
		}
	}
}
