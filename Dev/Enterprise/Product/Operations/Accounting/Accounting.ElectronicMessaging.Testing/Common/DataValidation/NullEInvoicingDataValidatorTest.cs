using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing
{
	public class NullEInvoicingDataValidatorTest : BaseEInvoicingDataValidatorTest
	{
		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()
		{
			return new NullEInvoicingDataValidator(GlbCompany.CurrentCompany);
		}

		public void TestValidatorRuns()
		{
			var logger = new DetailedLoggerForTest();
			GetEInvoicingDataValidatorForTest().Run(logger);

			var expectedLogs = new []
			{
				Tuple.Create<LogType, string, Exception>(LogType.Debug, "Data Validation started for the batched transactions.", null),
				Tuple.Create<LogType, string, Exception>(LogType.Debug, "Data Validation completed for the batched transactions.", null),
			};
			AssertArrayEqualsByElements("Just the begin & end logs should be present.", expectedLogs, logger.Logs.ToArray());
		}
	}
}
