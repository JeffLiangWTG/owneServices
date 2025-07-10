using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class ActiveTransportMeansProviderTest : DataProviderTestCase<ActiveTransportMeansProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsDepartureMovementHeader missing", () => new ActiveTransportMeansProvider(null));
			});
		}

		public void TestCustomsOfficeAtBorderReferenceNumber()
		{
			AssertEquals(string.Empty, Provider.CustomsOfficeAtBorderReferenceNumber);
			movementHeader.BM_CustomsOfficeAtBorder = "IE432";
			AssertEquals("IE432", Provider.CustomsOfficeAtBorderReferenceNumber);
		}

		public void TestIdentificationType()
		{
			AssertEquals(string.Empty, Provider.IdentificationType);
			movementHeader.BM_ActiveBorderIdentificationType = "40";
			AssertEquals("40", Provider.IdentificationType);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals(string.Empty, Provider.IdentificationNumber);
			movementHeader.BM_TOLCarrierID = "aBc123";
			AssertEquals("ABC123", Provider.IdentificationNumber);
		}

		public void TestNationality()
		{
			AssertEquals(string.Empty, Provider.Nationality);
			movementHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Ireland;
			AssertEquals(Core.Constants.CountryCodes.Ireland, Provider.Nationality);
		}

		public void TestConveyanceReferenceNumber()
		{
			AssertEquals(string.Empty, Provider.ConveyanceReferenceNumber);
			movementHeader.BM_ConveyanceNumber = "CONVNR";
			AssertEquals("CONVNR", Provider.ConveyanceReferenceNumber);
		}

		public void TestGetSingleProviderReadOnlyCollection()
		{
			AssertExceptionThrown<ArgumentException>("MovementHeader missing", () => new ActiveTransportMeansProvider(null).AsReadOnlyCollection());

			movementHeader.BM_ConveyanceNumber = "CONVNR";
			var collection = new ActiveTransportMeansProvider(movementHeader).AsReadOnlyCollection();
			AssertEquals(1, collection.Count);
			AssertEquals("CONVNR", collection.ElementAt(0).ConveyanceReferenceNumber);
		}

		public void TestMultipleTransportMeans()
		{
			var addTransport1 = movementHeader.AdditionalTransportAtBorderList.AddNew();
			addTransport1.TPM_CustomsOffice = "IEDUB100";
			addTransport1.TPM_IdentificationNumber = "12345";
			addTransport1.TPM_RN_NKTransportNationality = "IE";

			var addTransport2 = movementHeader.AdditionalTransportAtBorderList.AddNew();
			addTransport2.TPM_CustomsOffice = "IEDUB200";
			addTransport2.TPM_IdentificationNumber = "67890";
			addTransport2.TPM_RN_NKTransportNationality = "IE";

			var provider = GetProvider();
			var transports = provider.AsReadOnlyCollection();
			AssertEquals(3, transports.Count);
		}

		protected override ActiveTransportMeansProvider GetProvider() => new ActiveTransportMeansProvider(movementHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
		}
		NctsDepartureMovementHeader movementHeader;
		NctsHeader nctsHeader;
	}
}
