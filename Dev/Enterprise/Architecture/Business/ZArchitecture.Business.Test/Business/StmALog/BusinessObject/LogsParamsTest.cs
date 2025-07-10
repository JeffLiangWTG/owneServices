using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class LogsParamsTest : TestCaseWithFactory
	{
		public void TestParams_StmALogIsPassed_ReturnParamsFromEvent()
		{
			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.Parameters.Add("FRM", "UADOK");
				log.Parameters.Add("TO", "RUPKV");
				log.Parameters.Add("WHT", "FLOWERS");
			}

			var parameters = (dynamic)log.Params;

			AssertEquals("FROM param value", "UADOK", parameters.FRM);
			AssertEquals("TO param value", "RUPKV", parameters.TO);
			AssertEquals("WHAT param value", "FLOWERS", parameters.WHT);
		}

		public void TestLogsParams_REF_ShouldBeInitializedCorrectly()
		{
			var expectedRef = "FreeText";

			var logsParams = new LogsParams(expectedRef);

			AssertEquals(expectedRef, logsParams.REF);
		}
	}
}
