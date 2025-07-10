using System;
using System.Collections.Generic;
using CargoWise.Common.ErrorManagement;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class RetryHandlerTest : TestCase
	{
		#region Helper class
		/// <summary>
		///     Class that provides some target methods to be invoked, to allow testing of RetryHandler.
		///     The name of each method indicates how often it will "succeed" when called.
		///     Methods "fail" by throwing an exception.
		/// </summary>
		internal class MethodAttempt
		{
			int attemptsMade;
			int countSucceedEverySecond;
			int countSucceedEveryThird;
			public int AttemptsMade
			{
				get
				{
					return attemptsMade;
				}
			}

			public bool AlwaysFails()
			{
				attemptsMade++;
				throw new ApplicationException("AlwaysFails has failed");
			}

			public bool AlwaysSucceeds()
			{
				attemptsMade++;
				return true;
			}

			public bool SucceedsEverySecondCall()
			{
				attemptsMade++;
				countSucceedEverySecond++;
				if ((countSucceedEverySecond % 2) == 0)
				{
					return true;
				}

				throw new ApplicationException("SucceedsEverySecondCall has failed");
			}

			public bool SucceedsEveryThirdCall()
			{
				attemptsMade++;
				countSucceedEveryThird++;
				if ((countSucceedEveryThird % 3) == 0)
				{
					return true;
				}

				throw new ApplicationException("SucceedsEveryThirdCall has failed");
			}

			public bool SucceedsEveryThirdCallWithArg(int theArg)
			{
				attemptsMade++;
				countSucceedEveryThird++;
				if (theArg == 42 && (countSucceedEveryThird % 3) == 0)
				{
					return true;
				}

				throw new ApplicationException(String.Format("SucceedsEveryThirdCall has failed (theArg={0}", theArg));
			}
		}

		#endregion // Helper class

		#region Tests for ParseTimeSpanList
		static readonly TimeSpan ts2s = new TimeSpan(0, 0, 0, 2);
		static readonly TimeSpan ts2m = new TimeSpan(0, 0, 2, 0);
		static readonly TimeSpan ts2h = new TimeSpan(0, 2, 0, 0);
		static readonly TimeSpan ts2d = new TimeSpan(2, 0, 0, 0);
		public void TestParseSuccessful()
		{
			List<TimeSpan> retries = RetryHandler.ParseTimeSpanList("2s;2m;2h;2d");
			AssertEquals(4, retries.Count);
			AssertEquals(ts2s, retries[0]);
			AssertEquals(ts2m, retries[1]);
			AssertEquals(ts2h, retries[2]);
			AssertEquals(ts2d, retries[3]);
			// same, using comma separator
			retries = RetryHandler.ParseTimeSpanList("2s,2m,2h,2d");
			AssertEquals(4, retries.Count);
			AssertEquals(ts2s, retries[0]);
			AssertEquals(ts2m, retries[1]);
			AssertEquals(ts2h, retries[2]);
			AssertEquals(ts2d, retries[3]);
		}

		public void TestParseTimeSpanOverflowIgnored()
		{
			var retries = RetryHandler.ParseTimeSpanList("2s,2m,2h,200000000000000000000000000000000000d");
			AssertEquals(3, retries.Count);
			AssertEquals(ts2s, retries[0]);
			AssertEquals(ts2m, retries[1]);
			AssertEquals(ts2h, retries[2]);
			AssertEquals("Parsing overflow", true, ErrorReporter.HasBeenReported("RetryHandler|SpecificationOverflow"));
			if (ErrorReporter.TotalErrorCount == 2)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestParseEmptyString()
		{
			string retriesStr = string.Empty;
			List<TimeSpan> retries = RetryHandler.ParseTimeSpanList(retriesStr);
			AssertEquals(0, retries.Count);
		}

		public void TestParseInvalidOneUnit()
		{
			string retriesStr = "2s;2m;2X;2d";
			List<TimeSpan> retries = RetryHandler.ParseTimeSpanList(retriesStr);
			AssertEquals(3, retries.Count);
			AssertEquals(ts2s, retries[0]);
			AssertEquals(ts2m, retries[1]);
			AssertEquals(ts2d, retries[2]);
			AssertEquals("TimeSpan specification string is not valid and was ignored: 2X", ErrorReporter.LastMessageReported);
			Assert(ErrorReporter.TotalErrorCount == 1);
			if (ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestParseInvalidAllUnits()
		{
			string retriesStr = "2A;2B;2C;2Z";
			List<TimeSpan> retries = RetryHandler.ParseTimeSpanList(retriesStr);
			AssertEquals(0, retries.Count);
			AssertEquals("TimeSpan specification string is not valid and was ignored: 2A", ErrorReporter.LastMessageReported);
			Assert(ErrorReporter.TotalErrorCount == 1);
			if (ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestParseInvalidInputString()
		{
			string retriesStr = "INVALID";
			List<TimeSpan> retries = RetryHandler.ParseTimeSpanList(retriesStr);
			AssertEquals(0, retries.Count);
			AssertEquals("TimeSpan specification string is not valid and was ignored: INVALID", ErrorReporter.LastMessageReported);
			Assert(ErrorReporter.TotalErrorCount == 1);
			if (ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestParseMultipleFormats()
		{
			// all values are the same... just different formats
			List<TimeSpan> one = RetryHandler.ParseTimeSpanList("1d 12h 0m 0s;1.5D;36H,2160M;129600S");
			TimeSpan ts = TimeSpan.FromDays(1.5);
			AssertEquals(5, one.Count);
			AssertEquals(ts, one[0]);
			AssertEquals(ts, one[1]);
			AssertEquals(ts, one[2]);
			AssertEquals(ts, one[3]);
			AssertEquals(ts, one[4]);
		}

		public void TestParseNegativeValues()
		{
			List<TimeSpan> retries = RetryHandler.ParseTimeSpanList("2s;2m;2h;-2d");
			AssertEquals(3, retries.Count);
			AssertEquals(ts2s, retries[0]);
			AssertEquals(ts2m, retries[1]);
			AssertEquals(ts2h, retries[2]);
			AssertEquals(2, ErrorReporter.TotalErrorCount);
			AssertEquals("Parsing negative values", true, ErrorReporter.HasBeenReported("RetryHandler|NegativeValue"));
			if (ErrorReporter.TotalErrorCount == 2)
			{
				ErrorReporter.Clear();
			}
		}

		#endregion // Tests for ParseTimeSpanList

		#region Tests for FormatTimeSpan
		public void TestFormat()
		{
			AssertEquals("1d 12h 30m 30s", RetryHandler.FormatTimeSpan(new TimeSpan(1, 12, 30, 30)));
			AssertEquals("12h", RetryHandler.FormatTimeSpan(new TimeSpan(12, 0, 0)));
			AssertEquals("30m", RetryHandler.FormatTimeSpan(new TimeSpan(0, 30, 0)));
			AssertEquals("30s", RetryHandler.FormatTimeSpan(new TimeSpan(0, 0, 30)));
			// ParseTimeSpanList doesn't accept negative values, but that doesn't mean Format doesn't
			AssertEquals("-5m -5s", RetryHandler.FormatTimeSpan(TimeSpan.Parse("-00:05:05")));
			AssertEquals("-5m", RetryHandler.FormatTimeSpan(new TimeSpan(0, -5, 0)));
		}

		#endregion // Tests for FormatTimeSpan

		#region Tests for Invoke (delegate)
		public void TestInvokeWithPredicate()
		{
			var handler = new RetryHandler("1s;1s");
			var exceptionToCatch = new Exception();
			var exceptions = new Queue<Exception>(new[] { exceptionToCatch, new Exception() });
			AssertExceptionThrown<Exception>("Because we're only supposed to catch the first this should have succeeded", () =>
			{
				handler.Invoke<int, Exception>(() =>
				{
					throw exceptions.Dequeue();
				}, shouldCatch: (ex) => ex == exceptionToCatch);
			});
			AssertEquals("Both exceptions should have been thrown", 0, exceptions.Count);
		}

		public void TestInvokeWithFailureDelegate()
		{
			var attempt = new MethodAttempt();
			var retryHandler = new RetryHandler("1s;1s");
			int catchCount = 0;
			AssertExceptionThrown<ApplicationException>(() => retryHandler.Invoke<Exception>(() => attempt.AlwaysFails(), () => catchCount++));
			AssertEquals("Exception handler should be called every time it fails, except the last", 2, catchCount);
			if (ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestInvokeSuccessIfNoRetries()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler(string.Empty);
			AssertNoExceptionThrown(() => rh.Invoke(delegate
			{
				attempt.AlwaysSucceeds();
			}));
			Assert(attempt.AttemptsMade == 1);
		}

		public void TestInvokeSuccessIfNoRetriesEmptyTimeSpan()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler(new List<TimeSpan> { new TimeSpan() });
			AssertNoExceptionThrown(() => rh.Invoke(delegate
			{
				attempt.AlwaysSucceeds();
			}));
			Assert(attempt.AttemptsMade == 1);
		}

		public void TestInvokeSuccessIfMultipleRetries()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("1s;1s;1s;1s;1s");
			AssertNoExceptionThrown(() => rh.Invoke(delegate
			{
				attempt.AlwaysSucceeds();
			}));
			Assert(attempt.AttemptsMade == 1);
		}

		public void TestInvokeCorrectNumberOfAttemptsIfAlwaysFail()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("1s;1s");
			AssertExceptionThrown<ApplicationException>(() => rh.Invoke(delegate
			{
				attempt.AlwaysFails();
			}));
			Assert(attempt.AttemptsMade == 3);
		}

		public void TestInvokeCorrectNumberOfAttemptsIfAlwaysFailWithZeroDelay()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("0s;0s");
			AssertExceptionThrown<ApplicationException>(() => rh.Invoke(delegate
			{
				attempt.AlwaysFails();
			}));
			Assert(attempt.AttemptsMade == 3);
		}

		public void TestInvokeAttemptsIfAlwaysFailAndNegativeInputs()
		{
			// check logic
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("-1s;-1s");
			AssertExceptionThrown<ApplicationException>(() => rh.Invoke(delegate
			{
				attempt.AlwaysFails();
			}));
			Assert(attempt.AttemptsMade == 1);
			AssertEquals("Parsing negative values", true, ErrorReporter.HasBeenReported("RetryHandler|NegativeValue"));
			if (ErrorReporter.TotalErrorCount == 2)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestInvokePassNullDelegate()
		{
			var rh = new RetryHandler(string.Empty);
			AssertExceptionThrown<ArgumentNullException>(() => rh.Invoke(null));
		}

		public void TestInvokePassNullDelegate2()
		{
			var rh = new RetryHandler(string.Empty);
			RetryMethod<bool> method = null;
			AssertExceptionThrown<ArgumentNullException>(() => rh.Invoke(method));
		}

		public void TestInvokeAnonymousDelegate()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("1s;1s");
			rh.Invoke(delegate
			{
				attempt.SucceedsEverySecondCall();
			});
			Assert(attempt.AttemptsMade == 2);
		}

		public void TestInvokeWithArg()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("1s;1s;1s");
			int arg = 42;
			bool result = rh.Invoke(() => attempt.SucceedsEveryThirdCallWithArg(arg));
			Assert(attempt.AttemptsMade == 3);
			Assert(result);
		}

		/// <summary>
		///     Check that a failed retry throws the expected exception from the no-return variant
		/// </summary>
		public void TestInvokeFail()
		{
			var attempt = new MethodAttempt();
			// make two attempts (i.e. one retry)
			var rh = new RetryHandler("1s");
			AssertExceptionThrown<ApplicationException>(() => rh.Invoke(delegate
			{
				attempt.SucceedsEveryThirdCall();
			}));
		}

		/// <summary>
		///     Check that a failed retry throws the expected exception from the return variant
		/// </summary>
		public void TestInvokeFail2()
		{
			var attempt = new MethodAttempt();
			// make just one attempt (i.e. no retries)
			var rh = new RetryHandler(string.Empty);
			AssertExceptionThrown<ApplicationException>(() => rh.Invoke(attempt.SucceedsEverySecondCall));
		}

		public void TestInvokeTimeoutFail()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("1s;1s;4s", "1s");
			AssertExceptionThrown<ApplicationException>(() => rh.Invoke(delegate
			{
				attempt.AlwaysFails();
			}));
			Assert(attempt.AttemptsMade < 4);
		}

		public void TestInvokeNegativeTimeoutFail()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("1s;1s;4s", "-1s");
			AssertExceptionThrown<ApplicationException>(() => rh.Invoke(delegate
			{
				attempt.AlwaysFails();
			}));
			Assert(attempt.AttemptsMade == 4);
			AssertEquals("Parsing negative values", true, ErrorReporter.HasBeenReported("RetryHandler|NegativeValue"));
			if (ErrorReporter.TotalErrorCount == 1)
			{
				ErrorReporter.Clear();
			}
		}

		public void TestInvokeAlwaysSucceed()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("1s;1s;1s");
			bool result = rh.Invoke(attempt.AlwaysSucceeds);
			Assert(result);
			Assert(attempt.AttemptsMade == 1);
		}

		public void TestInvokeWithExpectedExceptionType()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("1s;1s");
			AssertExceptionThrown<ApplicationException>(() => rh.Invoke<ApplicationException>(() => attempt.AlwaysFails()));
			Assert(attempt.AttemptsMade == 3);
		}

		public void TestInvokeWithUnexpectedExceptionType()
		{
			var attempt = new MethodAttempt();
			var rh = new RetryHandler("1s;1s");
			AssertExceptionThrown<ApplicationException>(() => rh.Invoke<NullReferenceException>(() => attempt.AlwaysFails()));
			Assert(attempt.AttemptsMade == 1);
		}
		#endregion // Tests for Invoke (delegate)

	}
}