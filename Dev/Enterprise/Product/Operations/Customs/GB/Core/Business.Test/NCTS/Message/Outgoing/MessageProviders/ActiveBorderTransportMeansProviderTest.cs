using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class ActiveBorderTransportMeansProviderTest : DataProviderTestCase<ActiveBorderTransportMeansProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestCustomsOfficeAtBorderReferenceNumber()
		{
			AssertEquals("GB000058", Provider.CustomsOfficeAtBorderReferenceNumber);
		}

		public void TestCustomsOfficeAtBorderReferenceNumber_Blank()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var provider = new ActiveBorderTransportMeansProvider(header.MovementHeader, 1);

			AssertEquals(null, provider.CustomsOfficeAtBorderReferenceNumber);
		}

		public void TestTypeOfIdentification()
		{
			AssertEquals(41, Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("ABC1234", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			AssertEquals(Core.Constants.CountryCodes.Portugal, Provider.Nationality);
		}

		public void TestConveyanceReferenceNumber()
		{
			AssertEquals("CN000", Provider.ConveyanceReferenceNumber);
		}

		public void TestCustomsOfficeAtBorderReferenceNumber_Additional()
		{
			var provider = GetActiveBorderTransportMeansProviderFromAdditionalTransportAtBorder();
			AssertEquals("GB000057", provider.CustomsOfficeAtBorderReferenceNumber);
		}

		public void TestTypeOfIdentification_Additional()
		{
			var provider = GetActiveBorderTransportMeansProviderFromAdditionalTransportAtBorder();
			AssertEquals(42, provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber_Additional()
		{
			var provider = GetActiveBorderTransportMeansProviderFromAdditionalTransportAtBorder();
			AssertEquals("ABCDEFG", provider.IdentificationNumber);
		}

		public void TestNationality_Additional()
		{
			var provider = GetActiveBorderTransportMeansProviderFromAdditionalTransportAtBorder();
			AssertEquals(Core.Constants.CountryCodes.France, provider.Nationality);
		}

		public void TestConveyanceReferenceNumber_Additional()
		{
			var provider = GetActiveBorderTransportMeansProviderFromAdditionalTransportAtBorder();
			AssertEquals("CN001", provider.ConveyanceReferenceNumber);
		}

		ActiveBorderTransportMeansProvider GetActiveBorderTransportMeansProviderFromAdditionalTransportAtBorder()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var depHeader1 = header.MovementHeader;

			var transport = depHeader1.AdditionalTransportAtBorderList.AddNew();
			transport.TPM_CustomsOffice = "GB000057";
			transport.TPM_ReferenceNumber = "CN001";
			transport.TPM_TypeOfIdentification = "42";
			transport.TPM_IdentificationNumber = "ABCDEFG";
			transport.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.France;

			return new ActiveBorderTransportMeansProvider(transport, 1);
		}

		protected override ActiveBorderTransportMeansProvider GetProvider()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var depHeader = header.MovementHeader;
			depHeader.BM_CustomsOfficeAtBorder = "GB000058";
			depHeader.BM_ConveyanceNumber = "CN000";
			depHeader.BM_ActiveBorderIdentificationType = "41";
			depHeader.BM_TOLCarrierID = "ABC1234";
			depHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Portugal;

			return new ActiveBorderTransportMeansProvider(depHeader, 1);
		}
	}
}
