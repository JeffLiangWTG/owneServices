using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE014InvalidationTypeProviderTest : Customs.Business.Testing.DataProviderTestCase<IE014InvalidationTypeProvider>
	{
		[TestDate(2023, 1, 10, 12, 15, 30)]
		public void TestRequestDateTime()
		{
			AssertEquals("RequestDateTime", new ZDateTime(2023, 1, 10, 12, 15, 30), Provider.RequestDateTime);
		}

		public void TestJustification()
		{
			AssertEquals("Justification", "Justification Text", Provider.Justification);
		}

		public void TestInitiatedByCustoms()
		{
			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			AssertEquals("InitiatedByCustoms", false, GetProvider().InitiatedByCustoms);

			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested;
			AssertEquals("InitiatedByCustoms", true, GetProvider().InitiatedByCustoms);
		}

		protected override IE014InvalidationTypeProvider GetProvider() => new IE014InvalidationTypeProvider(nctsHeader, "Justification Text");

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}

		NctsHeader nctsHeader;
	}
}
