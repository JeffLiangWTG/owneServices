using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE014MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE014MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE014MessageProvider(null, null));

				var header = Factory.New<NctsHeader>();
				AssertExceptionThrown<ArgumentException>("MovementHeader missing", () => new IE014MessageProvider(header, "Justification Text"));
			});
		}

		public void TestTransitOperation()
		{
			AssertType<IE014TransitOperationProvider>("TransitOperation", Provider.TransitOperation);
		}

		public void TestInvalidationType()
		{
			AssertType<IE014InvalidationTypeProvider>("InvalidationType", Provider.InvalidationType);
		}

		public void TestDepartureOffice()
		{
			AssertEquals("Departure office", "IEDUB100", Provider.DepartureOffice);
		}

		public void TestHolderOfTheTransit()
		{
			AssertType<HolderOfTransitProcedureProvider>("HolderOfTheTransit", Provider.HolderOfTheTransit);
		}

		protected override IE014MessageProvider GetProvider() => new IE014MessageProvider(nctsHeader, "Justification Text");

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsHeader.MovementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "IEDUB100", ZDateTime.Empty, true);
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "0123456789000", "TIR123");
		}

		NctsHeader nctsHeader;
	}
}
