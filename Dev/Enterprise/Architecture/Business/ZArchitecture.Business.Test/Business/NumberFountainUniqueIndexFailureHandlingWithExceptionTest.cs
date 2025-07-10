using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class NumberFountainUniqueIndexFailureHandlingWithExceptionTest : TestCaseWithFactory
	{
		public void TestNumberFountainWithNoCommandToFindMaxValueFailWithException()
		{
			var brokenHandler = new BrokenNumberFountainUniqueIndexFailureHandler(new BrokenNumberFountain().PeekPreliminaryFormatted(Factory), Factory.New<DummyEnterpriseBusinessObject>());
			AssertNumberFountainFailWithException(brokenHandler);
		}

		public void TestNumberFountainWithCommandToFindMaxValueFailWithException()
		{
			var brokenHandler = new BrokenNumberFountainUniqueIndexFailureHandlerWithCommandToFindMaxValueInDatabase(new BrokenNumberFountain().PeekPreliminaryFormatted(Factory), Factory.New<DummyEnterpriseBusinessObject>());
			AssertNumberFountainFailWithException(brokenHandler);
		}

		void AssertNumberFountainFailWithException(NumberFountainUniqueIndexFailureHandler brokenHandler)
		{
			try
			{
				brokenHandler.NotifyUserAndAttemptToResolve(new NotificationHandler(), brokenHandler.HandledUniqueIndexNames.Single());

				string reportMask =
					"Number Fountain adjust failed after an unique index violation\r\n" +
					"  Fountain                    : {0}\r\n";

				string expectedDeveloperErrorReportedStart = String.Format(reportMask, typeof(BrokenNumberFountain).FullName);
				Assert("LastMessageReported - Start\r\n" + ErrorReporter.LastMessageReported,
					ErrorReporter.LastMessageReported.StartsWith(expectedDeveloperErrorReportedStart));

				reportMask =
					"  Unique Index Violated       : {0}\r\n" +
					"  BizO causing error          : {1}\r\n";

				string expectedDeveloperErrorReportedEnd = String.Format(reportMask, brokenHandler.HandledUniqueIndexNames.Single(), typeof(DummyEnterpriseBusinessObject).FullName);
				Assert("LastMessageReported - End\r\n" + ErrorReporter.LastMessageReported +
					System.Environment.NewLine + "Should end with :" + expectedDeveloperErrorReportedEnd,
					ErrorReporter.LastMessageReported.EndsWith(expectedDeveloperErrorReportedEnd));
				AssertEquals("Failed to fix fountain conflict.", ErrorReporter.LastExceptionReported.Message);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
	}
}
