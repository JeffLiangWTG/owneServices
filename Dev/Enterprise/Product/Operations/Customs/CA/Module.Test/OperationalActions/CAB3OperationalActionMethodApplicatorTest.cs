using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.OperationalActions.Testing
{
	[TestedType(typeof(CAB3OperationalActionMethodApplicator))]
	sealed class CAB3OperationalActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestDefaultScheduleActionCode()
		{
			var testApplicator = new CAB3OperationalActionMethodApplicator();
			AssertEquals(4, testApplicator.DefaultScheduleActionCodeList.Count);

			AssertEquals("NON", testApplicator.DefaultScheduleActionCode);
			AssertNoErrors(testApplicator.DefaultScheduleActionCodeInfo);

			testApplicator.DefaultScheduleActionCode = string.Empty;
			AssertEquals(string.Empty, testApplicator.DefaultScheduleActionCode);
			AssertHasErrorContaining(testApplicator.DefaultScheduleActionCodeInfo, "Please enter a default action.");

			testApplicator.DefaultScheduleActionCode = "XXX";
			AssertEquals("XXX", testApplicator.DefaultScheduleActionCode);
			AssertHasErrorContaining(testApplicator.DefaultScheduleActionCodeInfo, "Enter a valid default action");

			testApplicator.DefaultScheduleActionCode = "CAN";
			AssertEquals("CAN", testApplicator.DefaultScheduleActionCode);
			AssertNoErrors(testApplicator.DefaultScheduleActionCodeInfo);

			testApplicator.DefaultScheduleActionCode = "NOW";
			AssertEquals("NOW", testApplicator.DefaultScheduleActionCode);
			AssertNoErrors(testApplicator.DefaultScheduleActionCodeInfo);

			testApplicator.DefaultScheduleActionCode = "DFR";
			AssertEquals("DFR", testApplicator.DefaultScheduleActionCode);
			AssertNoErrors(testApplicator.DefaultScheduleActionCodeInfo);
		}
	}
}
