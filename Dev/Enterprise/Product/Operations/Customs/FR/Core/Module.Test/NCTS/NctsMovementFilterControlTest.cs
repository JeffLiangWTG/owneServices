using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Module.NCTS.Testing
{
	class NctsMovementFilterControlTest : TestCaseWithFactory
	{
		public void TestFRAdditionalColumnsExist()
		{
			using (var userControl = new NctsMovementFilterControl(new EU.NCTS.Business.NctsHeaderCollection(Factory), new NctsMovementFilterStripBusinessObject()))
			{
				var grid = userControl.Grid;
				AssertNotNull(grid.GetColumnStyle(FR.Business.NCTS.NctsHeader.Schema.DetailedDepartureStatusCode));
			}
		}
	}
}
