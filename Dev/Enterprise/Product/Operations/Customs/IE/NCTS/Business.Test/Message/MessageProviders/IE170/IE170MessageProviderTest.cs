using System;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE170MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE170MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE170MessageProvider(null));

				var header = Factory.New<NctsHeader>();
				AssertExceptionThrown<ArgumentException>("MovementHeader missing", () => new IE170MessageProvider(header));
			});
		}

		public void TestTransitOperation()
		{
			AssertType<IE170TransitOperationProvider>("TransitOperation", Provider.TransitOperation);
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
			AssertType<HolderOfTransitProcedureProvider>("Holder of the Transit Procedure", Provider.HolderOfTheTransitProcedure);
		}

		public void TestRepresentative()
		{
			AssertType<IE170RepresentativeProvider>("Holder of the Transit Procedure", Provider.Representative);
		}

		public void TestConsignment()
		{
			AssertType<IE170ConsignmentProvider>("Consignment", Provider.Consignment);
		}

		protected override IE170MessageProvider GetProvider() => new IE170MessageProvider(nctsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			SetPrincipal(string.Empty);
		}

		void SetPrincipal(string id)
		{
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", id, "TIR123");
		}

		NctsHeader nctsHeader;
	}
}
