using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.PAVE.MENT.Business.Test
{
	public class MENTStmScheduleTaskValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCollectionActiveSecurity()
		{
			Env.Security.MENTAgedScoreQueryChangeCollectionActive.IsAllowed = true;
			var schedule = Factory.NewWithValidTestData<MENTStmScheduleTask>();
			schedule.S5_IsActive = true;
			Factory.Save();

			AssertNoErrors(schedule.S5_IsActiveInfo);

			Env.Security.MENTAgedScoreQueryChangeCollectionActive.IsAllowed = false;

			AssertNoErrors(schedule.S5_IsActiveInfo);

			schedule.S5_IsActive = false;
			AssertHasError(schedule.S5_IsActiveInfo, Env.Security.MENTAgedScoreQueryChangeCollectionActive.ErrorMessageForNotAllowed);

			Env.Security.MENTAgedScoreQueryChangeCollectionActive.IsAllowed = true;
			Factory.Save();

			schedule.S5_IsActive = true;
			AssertNoErrors(schedule.S5_IsActiveInfo);

			schedule.S5_IsActive = false;
			AssertNoErrors(schedule.S5_IsActiveInfo);

			Factory.Save();

			Env.Security.MENTAgedScoreQueryChangeCollectionActive.IsAllowed = false;

			schedule.S5_IsActive = true;
			AssertHasError(schedule.S5_IsActiveInfo, Env.Security.MENTAgedScoreQueryChangeCollectionActive.ErrorMessageForNotAllowed);
		}
	}
}
