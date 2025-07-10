using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using CargoWise.Common;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class DigestReporterTest : TestCase
	{
		public void TestShouldReportImmediately_IfThereIsNoEventOfSuchKind()
		{
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Event should be reported immediately", 1000, "Wow!", 1, CurrentUtcDateTime);
			ReportAndAssertEventReported("Event should be reported immediately as it has a different key", 2000, "Fantastic!", 1, CurrentUtcDateTime);
		}

		public void TestShouldNotReportImmediately_IfReportFirstIsFalse()
		{
			reportFirst = false;
			AssertEventNotReported("Precondition");
			ReportAndAssertEventNotReported("Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Report should be deferred as reported recently", 1000, "Wow!");
			timeProvider.Sleep(InitialAccumulateTime);
			AssertEventReported("Should report with the Wow", 1000, "Wow!", 2, SomeTimeInThePast);
		}

		public void TestShouldDeferReporting_IfReportedRecently()
		{
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);

			DateTime startTime = CurrentUtcDateTime;
			ReportAndAssertEventNotReported("Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Report should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime);
			AssertEventReported("Should report about two occurrences", 1000, "Wow!", 2, startTime);
		}

		public void TestShouldReportOnlyTheFirstMessage_InEverySession_ForOccurrencesWithTheSameKey()
		{
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "The first message");
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "The second message - see I am different!");

			timeProvider.Sleep(InitialAccumulateTime);
			AssertEventReported("Should report with the first message", 1000, "The first message", 2, SomeTimeInThePast);
		}

		public void TestShouldReportImmediately_IfReportedAWhileAgo()
		{
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime);
			AssertEventReported("Precondition: Should report about two occurrences", 1000, "Wow!", 2, SomeTimeInThePast);

			timeProvider.Sleep(InitialReleaseTime);
			ReportAndAssertEventReported("Precondition: Report should be reported immediately", 2000, "Fantastic!", 1, SomeTimeInThePast);

			ReportAndAssertEventReported("Event should be reported immediately as the last occurrence with the same key was a while ago", 1000, "Wow!", 1, CurrentUtcDateTime);
		}

		public void TestShoulReportImmediately_IfThresholdIsOne()
		{
			reportFirst = false;
			threshold = 1;
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Event should be reported immediately", 1000, "Wow!", 1, CurrentUtcDateTime);
			ReportAndAssertEventReported("Event should be reported immediately as it has a different key", 2000, "Fantastic!", 1, CurrentUtcDateTime);
		}

		public void TestShoulReport_IfReachThreshold()
		{
			reportFirst = false;
			threshold = 3;
			AssertEventNotReported("Precondition");
			ReportAndAssertEventNotReported("Report should not be reported", 1000, "Ops!");
			ReportAndAssertEventNotReported("Report should not be reported", 1000, "Ops!");
			ReportAndAssertEventNotReported("Report should not be reported", 1000, "Ops!");

			timeProvider.Sleep(InitialAccumulateTime);
			AssertEventReported("Event should be reported as the threshold was reached", 1000, "Ops!", 3, SomeTimeInThePast);
		}

		public void TestShoulNeverReport_IfNotReachThresholdInReleaseTime()
		{
			reportFirst = false;
			threshold = 4;
			AssertEventNotReported("Precondition");
			ReportAndAssertEventNotReported("Report should not be reported", 1000, "Ops!");
			ReportAndAssertEventNotReported("Report should not be reported", 1000, "Ops!");
			ReportAndAssertEventNotReported("Report should not be reported", 1000, "Ops!");

			timeProvider.Sleep(InitialReleaseTime);
			AssertEventNotReported("Event should not be reported as the threshold was not reached");

			threshold = 2;
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialReleaseTime);
			AssertEventReported("Only two events should be reported as the last occurrences where released", 1000, "Wow!", 2, SomeTimeInThePast);
		}

		public void TestShouldNotReportImmediately_IfReportedAWhileAgo()
		{
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime);
			AssertEventReported("Precondition: Should report about two occurrences", 1000, "Wow!", 2, SomeTimeInThePast);
		}

		public void TestShouldAcceptZeroReleaseTime()
		{
			AssertEventNotReported("Precondition");
			releaseTimeSpan = TimeSpan.Zero;

			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime);
			AssertEventReported("Precondition: Should report about two occurrences", 1000, "Wow!", 2, SomeTimeInThePast);

			timeProvider.Sleep(InitialAccumulateTime);
			ReportAndAssertEventReported("Should be reported immediately as release time is zero", 1000, "Wow!", 1, SomeTimeInThePast);
			ReportAndAssertEventNotReported("Report should be deferred as reported recently", 1000, "Wow!");
		}

		public void TestShouldBeAbleToChangeItsAccumulateTime()
		{
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime);
			AssertEventReported("Precondition: Should report about the occurrence", 1000, "Wow!", 2, SomeTimeInThePast);

			int shorterAccumulateTime = InitialAccumulateTime / 2;
			accumulateTimeSpan = TimeSpan.FromMilliseconds(shorterAccumulateTime); //this will change the timer at the next iteration of reporting
			DateTime startTime = CurrentUtcDateTime;

			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			AssertEquals("Precondition: The timer should accept the new interval", shorterAccumulateTime, reporter.TimerInterval_ExposedForTesting);

			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			timeProvider.Sleep(shorterAccumulateTime);

			AssertEventReported("Should report about two occurrence", 1000, "Wow!", 2, startTime);
		}

		public void TestShouldReportImmediately_IfAccumulationIsSwitchedOff()
		{
			AssertEventNotReported("Precondition");
			accumulateTimeSpan = TimeSpan.Zero;
			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);

			ReportAndAssertEventReported("Should report immediately", 1000, "Wow!", 1, CurrentUtcDateTime);
		}

		public void TestShouldBeAbleToSwitchAccumulationOnAndOff()
		{
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);

			DateTime startTime = CurrentUtcDateTime;
			ReportAndAssertEventNotReported("Precondition: Reports should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Reports should be deferred as reported recently", 1000, "Wow!");

			//switch off
			accumulateTimeSpan = TimeSpan.FromMilliseconds(0);
			ReportAndAssertEventReported("Should report about three occurrences", 1000, "Wow!", 3, CurrentUtcDateTime);

			ReportAndAssertEventReported("Report should be reported immediately as accumulation is switched off", 1000, "Wow!", 1, CurrentUtcDateTime);
			ReportAndAssertEventReported("Report should be reported immediately as accumulation is switched off", 1000, "Wow!", 1, CurrentUtcDateTime);

			//switch on
			accumulateTimeSpan = TimeSpan.FromMilliseconds(InitialAccumulateTime);

			startTime = CurrentUtcDateTime;
			ReportAndAssertEventNotReported("Reports should be deferred as the same events happened before switching off", 1000, "Wow!");
			ReportAndAssertEventNotReported("Reports should be deferred as the same events happened before switching off", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime);
			AssertEventReported("Should report about two occurrences", 1000, "Wow!", 2, startTime);
		}

		public void TestShouldNotThrow_WhenSwitchingOffRealTimer()
		{
			var timeProviderWithRealTimer = new TimeProvider();
			var reporterWithRealTimer = GetReporter(timeProviderWithRealTimer);

			//start timer
			reporterWithRealTimer.Report(100, "Wow!");

			//instruct to stop timer on next report
			accumulateTimeSpan = TimeSpan.FromMilliseconds(0);

			//stop timer
			AssertNoExceptionThrown(() => reporterWithRealTimer.Report(100, "Wow!"));
		}

		public void TestShouldBeAbleToChangeReportFirst()
		{
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);

			timeProvider.Sleep(releaseTimeSpan);

			reportFirst = false;

			DateTime startTime = CurrentUtcDateTime;
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime);

			AssertEventReported("Should report about two occurrences", 1000, "Wow!", 2, startTime);
		}

		public void TestShouldBeAbleToChangeThreshold()
		{
			AssertEventNotReported("Precondition");

			reportFirst = false;

			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime);

			AssertEventReported("Should report about two occurrences", 1000, "Wow!", 2, SomeTimeInThePast);

			timeProvider.Sleep(releaseTimeSpan);

			threshold = 4;
			DateTime startTime = CurrentUtcDateTime;
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime);

			AssertEventNotReported("Do not reach the threshold");

			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Report should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime);

			AssertEventReported("Should report about four occurrences", 1000, "Wow!", 4, startTime);
		}

		public void TestDigestReporterProviderShouldClearAllReportAfterTest()
		{
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);
			ReportAndAssertEventNotReported("Report should be deferred as reported recently", 1000, "Wow!");

			Assert("We don't test anything here and just defer some report to ensure that this test does not have side effects on other tests and thus tests for any functionality based on DigestReporter are stable", true);
		}

		public void TestShouldNotRestartTimerIfNotChangeAccumulateTime()
		{
			AssertEventNotReported("Precondition");
			ReportAndAssertEventReported("Precondition", 1000, "Wow!", 1, SomeTimeInThePast);

			DateTime startTime = CurrentUtcDateTime;
			ReportAndAssertEventNotReported("Precondition: Reports should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Reports should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime / 2);

			ReportAndAssertEventNotReported("Precondition: Reports should be deferred as reported recently", 1000, "Wow!");
			ReportAndAssertEventNotReported("Precondition: Reports should be deferred as reported recently", 1000, "Wow!");

			timeProvider.Sleep(InitialAccumulateTime / 2);

			AssertEventReported("Should report about two occurrence", 1000, "Wow!", 4, startTime);
		}

		#region Implementation

		static MockTimeProvider timeProvider;
		static DigestReporter<int, string> reporter;

		static TimeSpan accumulateTimeSpan;
		static TimeSpan releaseTimeSpan;
		static bool reportFirst;
		static readonly Func<bool> reportFirstFunc = () => reportFirst;
		static int threshold;
		static readonly Func<int> threshouldFunc = () => threshold;

		const int InitialAccumulateTime = 100;
		const int InitialReleaseTime = 200;

		static internal bool eventReportedWithinASession;
		static internal int lastReportedKey;
		static internal string lastReportedMessage;
		static internal IEnumerable<DateTime> lastReportedOccurrences;

		protected override void SetUp()
		{
			base.SetUp();

			timeProvider = new MockTimeProvider(DateTime.UtcNow, DateTime.Now); // Test time does not require to be accurate
			reporter = GetReporter(timeProvider);

			accumulateTimeSpan = TimeSpan.FromMilliseconds(InitialAccumulateTime);
			releaseTimeSpan = TimeSpan.FromMilliseconds(InitialReleaseTime);

			eventReportedWithinASession = false;
			lastReportedKey = 0;
			lastReportedMessage = null;
			lastReportedOccurrences = null;
			reportFirst = true;
			threshold = 2;
		}

		DigestReporter<int, string> GetReporter(ITimeProvider timeProvider)
		{
			return DigestReporterProvider.GetDigestReporter<int, string>(
				processDigestHandler: (key, message, occurrences) =>
				{
					eventReportedWithinASession = true;
					lastReportedKey = key;
					lastReportedMessage = message;
					lastReportedOccurrences = occurrences;
				},
				accumulateTimeGetter: () => accumulateTimeSpan,
				releaseTimeGetter: () => releaseTimeSpan,
				timeProvider: timeProvider,
				reportFirst: reportFirstFunc,
				threshold: threshouldFunc
				);
		}

		protected override void TearDown()
		{
			reporter.Dispose();
			base.TearDown();
		}

		DateTime CurrentUtcDateTime => timeProvider.GetCurrentUtcDateTime();
		DateTime SomeTimeInThePast => CurrentUtcDateTime.AddMinutes(-1);

		void ReportAndAssertEventReported(string errorMessage, int key, string message, int expectedOccurrencesCount, DateTime startTime)
		{
			PrepareToTheNextReport();
			reporter.Report(key, message);
			AssertEventReported(errorMessage, key, message, expectedOccurrencesCount, startTime);
		}

		void ReportAndAssertEventNotReported(string errorMessage, int key, string message)
		{
			PrepareToTheNextReport();
			reporter.Report(key, message);
			AssertEventNotReported(errorMessage);
		}

		void PrepareToTheNextReport()
		{
			eventReportedWithinASession = false;
		}

		void AssertEventReported(string errorMessage, int expectedKey, string expectedMessage, int expectedOccurrencesCount, DateTime startTime)
		{
			CombineAssertions(errorMessage, () =>
			{
				AssertEquals("Should be reported", true, eventReportedWithinASession);
				AssertEquals("Key", expectedKey, lastReportedKey);
				AssertEquals("Message", expectedMessage, lastReportedMessage);
				AssertEquals("Occurrences count", expectedOccurrencesCount, lastReportedOccurrences.Count());

				DateTime endTime = CurrentUtcDateTime;
				foreach (var occurrence in lastReportedOccurrences)
				{
					Assert("Event should occur within the designated time frame", occurrence >= startTime);
					Assert("Event should occur within the designated time frame", occurrence <= endTime);
				}
			});
		}

		void AssertEventNotReported(string errorMessage)
		{
			AssertEquals(errorMessage, false, eventReportedWithinASession);
		}

		#endregion
	}

	public class DigestReporterTestWithTimeProvider : TestCase
	{
		#region Nested MockTimeProvider/Adaptor

		class MockTimerAdaptor_WithThreading : MockTimerAdaptor
		{
			public MockTimerAdaptor_WithThreading(ITimeProvider timeProvider)
				: base(timeProvider)
			{
			}

			protected override void OnElapsed(ElapsedEventArgs e)
			{
				Task.Factory.StartNew(() => base.OnElapsed(e)).Wait();
			}
		}

		class MockTimeProvider_WithThreading : MockTimeProvider
		{
			public MockTimeProvider_WithThreading()
				: base(DateTime.UtcNow, DateTime.Now)
			{
			}

			protected override MockTimerAdaptor CreateAdaptor()
			{
				return new MockTimerAdaptor_WithThreading(this);
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestDBConnectionThrowsNoExceptions()
		{
			int timesCalled = 0;

			var timeProvider = new MockTimeProvider_WithThreading();
			var reporter = DigestReporterProvider.GetDigestReporter<string, bool>(
				processDigestHandler: (key, connect, occurrences) =>
				{
					if (connect)
					{
						Db.Connection.ExecuteNonQuery("SELECT 1");
					}
					timesCalled++;
				},
				accumulateTimeGetter: () => TimeSpan.FromMilliseconds(1),
				releaseTimeGetter: () => TimeSpan.FromMilliseconds(1),
				timeProvider: timeProvider);

			AssertEquals(0, timesCalled);

			reporter.Report("Key", false);
			reporter.Report("Key", true);
			AssertEquals(1, timesCalled);

			timeProvider.Sleep(1);

			AssertEquals(2, timesCalled);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
		}
	}
}
