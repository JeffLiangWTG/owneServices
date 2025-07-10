using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever
{
	class BigDataScript : MockScript
	{
		public override string ScriptText => @"
SELECT *
FROM #TempBigBillingData";

		public override int TimeoutSecs => 5;

		public override IEnumerable<ZSqlParameter> GetInputParameters(IDateTimeRange dateTimeRange) => Array.Empty<ZSqlParameter>();
	}

	internal class ScriptRunnerTests : TransactionedTestCase
	{
		public void TestScriptRunnerTimesOutOnBigData()
		{
			TestConnection.ExecuteNonQuery(@"
create table #TempBigBillingData
(
	CompanyCode char(3),
	BranchCode char(3),
	TransactionDateUtc DateTime,
	UserCode char(3),
	TransactionReference01 char(100),
	TransactionReference02 char(100),
	TransactionReference03 char(100),
	TransactionReference04 char(100),
	TransactionGuidReference char(100),
	ItemCount int,
	AdditionalRefs varbinary(max)
)

DECLARE @TransactionDateUtc DateTime = GetDate()
DECLARE @Counter INT = 1
WHILE ( @Counter <= 1000000)
BEGIN
	INSERT INTO #TempBigBillingData Values('DEM', '~BR', @TransactionDateUtc, 'USR', 'DAILY#1', 'DAILY#2', '', '', '448AC110-9B18-497F-8F6E-CA80AC82E098', 1, 0x)
	SET @Counter  = @Counter  + 1
END");
			try
			{
				var startTime = DateTime.UtcNow.AddHours(-2);
				var endTime = startTime.AddHours(1);
				Exception ex = null;
				try
				{
					ScriptRunner.Run(new BigDataScript(), new RecurringRange(startTime, endTime), new BillingTransactionFactory());
				}
				catch (Exception e)
				{
					ex = e;
				}

				AssertNotNull("There should have been an exception", ex);
				var isSqlTimeoutException = ex is SqlException sqlEx && new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.TimeoutExpired;
				Assert("Timeout exception should have been thrown", ex is TimeoutException || isSqlTimeoutException);
			}
			finally
			{
				TestConnection.ExecuteNonQuery("DROP TABLE #TempBigBillingData");
			}
		}
	}
}
