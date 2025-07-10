using System;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE054MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE054MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE054MessageProvider(null, "Y"));

				var header = Factory.New<NctsHeader>();
				AssertExceptionThrown<ArgumentException>("MovementHeader missing", () => new IE054MessageProvider(header, "Y"));
			});
		}

		public void TestCustomsOfficeOfDeparture()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Office of departure has not been filled", string.Empty, Provider.CustomsOfficeOfDeparture);
				var departureOffice = nctsHeader.MovementHeader.CustomsOffices.Find((NctsEuOfficeCode x) => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture).First();
				departureOffice.CY_Data = "COD";
				AssertEquals("Office of departure has been filled", "COD", Provider.CustomsOfficeOfDeparture);
			});
		}

		public void TestHolderOfTheTransitProcedure()
		{
			SetPrincipal("IE12345678");
			AssertType<HolderOfTransitProcedureProvider>("HolderOfTheTransitProcedure", Provider.HolderOfTheTransitProcedure);
		}

		public void TestTransitOperation()
		{
			AssertType<IE054TransitOperationProvider>("TransitOperation", Provider.TransitOperation);
		}

		protected override IE054MessageProvider GetProvider() => new IE054MessageProvider(nctsHeader, "Y");

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}

		void SetPrincipal(string id)
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", id, "TIR123");
		}

		NctsHeader nctsHeader;
	}
}
