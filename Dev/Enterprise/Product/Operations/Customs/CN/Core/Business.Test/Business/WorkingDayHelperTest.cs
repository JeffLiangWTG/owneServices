using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	class WorkingDayHelperTest : TestCaseWithFactory
	{
		public void TestGetNearestWorkingdayAfter()
		{
			AssertEquals(new ZDateTime(2020, 1, 19), WorkingDayHelper.GetNearestWorkingdayAfter(new ZDateTime(2020, 1, 17)));
			AssertEquals(new ZDateTime(2020, 1, 19), WorkingDayHelper.GetNearestWorkingdayAfter(new ZDateTime(2020, 1, 18)));
			AssertEquals(new ZDateTime(2020, 1, 20), WorkingDayHelper.GetNearestWorkingdayAfter(new ZDateTime(2020, 1, 19)));
			AssertEquals(new ZDateTime(2020, 2, 3), WorkingDayHelper.GetNearestWorkingdayAfter(new ZDateTime(2020, 1, 24)));
		}
	}
}
