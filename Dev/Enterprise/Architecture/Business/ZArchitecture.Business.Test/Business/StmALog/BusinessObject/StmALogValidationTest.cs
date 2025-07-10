using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmALogValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDontValidateOldLogs()
		{
			var log = Factory.NewWithValidTestData<StmALog>();
			Factory.Save();
			AssertNoErrors(log);
			((INeedRow)log).Row[StmALogSchema.SL_SE_NKEvent.Name] = "人";
			log.RunPreSaveValidation();
			AssertNoErrors(log);
			log = Factory.NewWithValidTestData<StmALog>();
			((INeedRow)log).Row[StmALogSchema.SL_SE_NKEvent.Name] = "人";
			log.RunPreSaveValidation();
			AssertHasError(log.SL_SE_NKEventInfo, "Event Code only accepts Western European languages characters.");
			ErrorReporter.Clear();
		}

		public void TestDontValidateEventTimeRange()
		{
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_EventTime = ZDateTime.MinSmallDateTimeValue;
				log.Validation.ValidateSL_EventTime();
				AssertNoErrors(log.SL_EventTimeInfo);

				log.SL_EventTime = ZDateTime.Now;
				log.Validation.ValidateSL_EventTime();
				AssertNoErrors(log.SL_EventTimeInfo);

				log.SL_EventTime = ZDateTime.MaxSmallDateTimeValue;
				log.Validation.ValidateSL_EventTime();
				AssertNoErrors(log.SL_EventTimeInfo);
			}
		}
	}
}
