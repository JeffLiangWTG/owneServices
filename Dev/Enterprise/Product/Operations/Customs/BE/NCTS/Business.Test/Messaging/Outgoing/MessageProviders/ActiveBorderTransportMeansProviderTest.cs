
using NUnit.Framework;
namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(ActiveBorderTransportMeansProvider))]
	sealed class ActiveBorderTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<ActiveBorderTransportMeansProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestCustomsOfficeAtBorderReferenceNumber()
		{
			depHeader.BM_CustomsOfficeAtBorder = "BE112233";
			AssertEquals("BE112233", Provider.CustomsOfficeAtBorderReferenceNumber);
		}

		public void TestTypeOfIdentification()
		{
			depHeader.BM_ActiveBorderIdentificationType = "40";
			AssertEquals(40, Provider.TypeOfIdentification);
		}

		public void TestIdentificationNumber()
		{
			depHeader.BM_TOLCarrierID = "ABC123";
			AssertEquals("ABC123", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			depHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Belgium;
			AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.Nationality);
		}

		public void TestConveyanceReferenceNumber()
		{
			depHeader.BM_ConveyanceNumber = "CONVNR";
			AssertEquals("CONVNR", Provider.ConveyanceReferenceNumber);
		}

		public void TestCustomsOfficeAtBorderReferenceNumber_Additional()
		{
			var provider = GetActiveBorderTransportMeansProviderFromAdditionalTransportAtBorder();
			AssertEquals("BE010203", provider.CustomsOfficeAtBorderReferenceNumber);
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
			var transport = depHeader.AdditionalTransportAtBorderList.AddNew();
			transport.TPM_CustomsOffice = "BE010203";
			transport.TPM_ReferenceNumber = "CN001";
			transport.TPM_TypeOfIdentification = "42";
			transport.TPM_IdentificationNumber = "ABCDEFG";
			transport.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.France;

			return new ActiveBorderTransportMeansProvider(transport, 1);
		}

		protected override ActiveBorderTransportMeansProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			depHeader = header.MovementHeader;
			provider = new ActiveBorderTransportMeansProvider(depHeader, 1);
		}

		ActiveBorderTransportMeansProvider provider;
		NctsHeader header;
		NctsDepartureMovementHeader depHeader;
	}
}
