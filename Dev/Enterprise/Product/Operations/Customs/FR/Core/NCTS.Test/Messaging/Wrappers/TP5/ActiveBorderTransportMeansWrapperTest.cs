using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class ActiveBorderTransportMeansWrapperTest : Customs.Business.Testing.DataProviderTestCase<ActiveBorderTransportMeansWrapper>
	{
		public void TestCustomsOfficeAtBorderReference()
		{
			AssertEquals("CustomsOfficeAtBorderReference should equal BM_CustomsOfficeAtBorder.", "FR000040", Provider.CustomsOfficeAtBorderReference);
		}

		public void TestTypeOfIdentification()
		{
			AssertEquals("TypeOfIdentification should equal BM_ActiveBorderIdentificationType.", "AA", Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal BM_TOLCarrierID.", "LE MAURENSOIS", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			AssertEquals("Nationality should equal BM_RN_NKTOLCarrierNationality.", "GT", Provider.Nationality);
		}

		public void TestConveyenceReferenceNumber()
		{
			AssertEquals("ConveyenceReferenceNumber should equal BM_ConveyanceNumber.", "CONVEYANCE1", Provider.ConveyenceReferenceNumber);
		}

		protected override ActiveBorderTransportMeansWrapper GetProvider()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.BM_CustomsOfficeAtBorder = "FR000040";
			movementHeader.BM_ActiveBorderIdentificationType = "AA";
			movementHeader.BM_TOLCarrierID = "LE MAURENSOIS";
			movementHeader.BM_RN_NKTOLCarrierNationality = Enterprise.Core.Constants.CountryCodes.Guatemala;
			movementHeader.BM_ConveyanceNumber = "CONVEYANCE1";

			return ActiveBorderTransportMeansWrapper.New(movementHeader);
		}
	}
}
