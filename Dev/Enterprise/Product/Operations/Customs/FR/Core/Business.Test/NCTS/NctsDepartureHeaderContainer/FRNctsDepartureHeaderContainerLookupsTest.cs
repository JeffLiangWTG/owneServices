using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	public class FRNctsDepartureHeaderContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestModes()
		{
			AssertEquals("LCL, FCL, LSE, ULD, BBK, BLK, LQD, ROR, LTL, FTL, OBC, UNA, CNT, NCT", headerContainer.Lookups.Modes.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			headerContainer = header.DepartureHeaderContainers.AddNew();
		}
		NctsHeader header;
		FRNctsDepartureHeaderContainer headerContainer;
	}
}
