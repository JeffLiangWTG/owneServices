using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common.ErrorManagement;
using Moq;
using NUnit.Framework;

namespace CargoWise.Common
{
	public class ErrorReporterTest : TestCase
	{
		public void TestReportErrorOnlySendsOnceForSameIssueByKey()
		{
			for (int i = 0; i <= 1; i++)
			{
				ErrorReporter.ReportOnce("Hi Mum");
				if (i == 0)
				{
					AssertEquals("Hi Mum", ErrorReporter.LastMessageReported);
					ErrorReporter.LastMessageReported = null;
				}
				else
				{
					AssertNull(ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestDeferredReporting()
		{
			const string errorText = "This is an error!";
			using (ErrorReporter.DeferReportingTemporarily())
			{
				ErrorReporter.ReportOnce(errorText);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}

			AssertEquals(errorText, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDeferredReportingWithAdditionalInfo()
		{
			var reporterMock = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(reporterMock.Object))
			using (ErrorReporter.DeferReportingTemporarily())
			using (ErrorReporter.GatherAdditionalInformation())
			{
				ErrorReporter.SetAdditionalInfo("category1", "key1", "value1");
				ErrorReporter.SetAdditionalInfo("category1", "key2", "value2");
				ErrorReporter.SetAdditionalInfo("category2", "key3", "value3");
				ErrorReporter.ReportOnceWithAdditionalInfo("message_key", "There was an error", "category1"); // Skipping "category2"

				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}

			AssertEquals("There was an error\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: category1\r\nkey1:\r\nvalue1\r\nkey2:\r\nvalue2", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDeferredReportingAcrossMultipleThreads()
		{
			const string errorText = "Error on main thread";
			const string acrossThreadError = "This is on another thread";

			Task.Run(async () =>
			{
				using (ErrorReporter.DeferReportingTemporarily())
				{
					await Task.Delay(TimeSpan.FromSeconds(1));
					ErrorReporter.ReportOnce("secondary error", acrossThreadError);
				}
				//deferred errors should not be processed until scopes are closed
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			});

			using (ErrorReporter.DeferReportingTemporarily())
			{
				ErrorReporter.ReportOnce("main error", errorText);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);

				Task.Delay(TimeSpan.FromSeconds(5)).Wait();
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}

			AssertEquals(acrossThreadError, ErrorReporter.LastMessageReported);
			AssertEquals(2, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		void FunctionThatReportsAnExceptionWithNoKeyAndNoStackTrace()
		{
			ErrorReporter.ReportOnce("Some message with no key specified", new Exception("Not thrown exception"));
		}

		public void TestDeferredReporting_NoKeyAndNoStackTrace_GeneratesKeyBasedOnStackTrace()
		{
			// Arrange
			var reporterMock = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(reporterMock.Object))
			using (ErrorReporter.DeferReportingTemporarily())
			{
				// Act
				FunctionThatReportsAnExceptionWithNoKeyAndNoStackTrace();

				// Assert
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
				AssertEquals(string.Empty, ErrorReporter.LastKeyReported);
			}

			AssertEquals("Some message with no key specified", ErrorReporter.LastMessageReported);
			AssertContains("at CargoWise.Common.ErrorReporter.ReportOnce(System.String message, System.Exception exception)", ErrorReporter.LastKeyReported);
			AssertContains("at CargoWise.Common.ErrorReporterTest.FunctionThatReportsAnExceptionWithNoKeyAndNoStackTrace()", ErrorReporter.LastKeyReported);
			AssertContains("at CargoWise.Common.ErrorReporterTest.TestDeferredReporting_NoKeyAndNoStackTrace_GeneratesKeyBasedOnStackTrace()", ErrorReporter.LastKeyReported);
		}

		public void TestReportErrorReportAggregateException()
		{
			var exceptions = new Exception[3];
			exceptions[0] = new ArgumentNullException("Argument");
			exceptions[1] = new NotSupportedException("AggregateException");
			exceptions[2] = new IndexOutOfRangeException("IndexOutOfRangeException");
			var aggregateException = new AggregateException(exceptions);
#if NETFRAMEWORK
			var expectedMessage = @"One or more errors occurred
Value cannot be null.
Parameter name: Argument
AggregateException
IndexOutOfRangeException";
#else
			var expectedMessage = @"One or more errors occurred
Value cannot be null. (Parameter 'Argument')
AggregateException
IndexOutOfRangeException";
#endif
			ErrorReporter.ReportOnce("One or more errors occurred", aggregateException);
			AssertEquals(expectedMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestReportErrorOnlySendsOnceForSameIssueByException()
		{
			AssertReportError(new Exception("hello"));
			bool shouldReportAlwaysInReportOnce = false;
			var exception = new ExceptionImplementingReporterExtender(shouldReportAlwaysInReportOnce);
			AssertReportError(exception);
			shouldReportAlwaysInReportOnce = true;
			exception = new ExceptionImplementingReporterExtender(shouldReportAlwaysInReportOnce);
			AssertReportError(exception, shouldReportAlwaysInReportOnce);
		}

		[ExpectNoExceptions]
		public void TestReportOnceOnSameCallStackWhenCallStackIsMissingFromTheReportedException()
		{
			// Arrange
			const string sameErrorMessage = "Same Error Message";
			var reporterMock = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(reporterMock.Object))
			{
				// Act
				TestWithCallstack1();
				TestWithCallstack1();
				TestWithCallstack2();
			}

			// Assert
			reporterMock.Verify(x => x.Report(It.IsAny<string>(), sameErrorMessage, It.IsAny<InvalidOperationException>()), Times.Exactly(2));

			void TestWithCallstack1()
			{
				ErrorReporter.ReportOnce(sameErrorMessage, new InvalidOperationException());
			}

			void TestWithCallstack2()
			{
				ErrorReporter.ReportOnce(sameErrorMessage, new InvalidOperationException());
			}
		}

		public class ReportDeveloperExceptionOnceTest : TestCase
		{
			[ExpectNoExceptions]
			public void TestWithoutKey()
			{
				// Arrange
				const string errorMessage = "Error Message";

				var reporterMock = new Mock<IErrorReporter>();

				using (ErrorReporter.SetTemporaryInstanceForTest(reporterMock.Object))
				{
					// Act
					ErrorReporter.ReportDeveloperExceptionOnce(errorMessage, new InvalidOperationException());
				}

				// Assert
				reporterMock.Verify(x => x.ReportDeveloperExceptionOrHandleSilently(null, errorMessage, It.IsAny<InvalidOperationException>()), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestWithKey()
			{
				const string errorMessage = "Error Message";

				CombineAssertions(() =>
				{
					Test(errorKey: null, errorMessage, new InvalidOperationException());
					Test(errorKey: "", errorMessage, new ArithmeticException());
					Test(errorKey: "Error Key", errorMessage, new ArgumentException());
				});

				void Test(string errorKey, string errorMessage, Exception exception)
				{
					// Arrange
					var reporterMock = new Mock<IErrorReporter>();

					using (ErrorReporter.SetTemporaryInstanceForTest(reporterMock.Object))
					{
						// Act
						ErrorReporter.ReportDeveloperExceptionOnce(errorKey, errorMessage, exception);
					}

					// Assert
					reporterMock.Verify(x => x.ReportDeveloperExceptionOrHandleSilently(errorKey, errorMessage, It.Is<Exception>(e => e.GetType() == exception.GetType())), Times.Once);
				}
			}
		}

		public void TestLastExceptionsReportedReturnsEmptyListWhenExceptionsThrownIsNull()
		{
			//Assert
			AssertNoExceptionThrown(() =>
			{
				//Act
				var exceptions = ErrorReporter.LastExceptionsReported();
				AssertNotNull(exceptions);
			});
		}

		public void TestLastExceptionsReportedReturnsEmptyListWhenExceptionsThrownIsEmpty()
		{
			//Arrange
			ErrorReporter.Clear();
			var exceptions = ErrorReporter.ExceptionsThrown; // Sets _exceptionsThrown from null to new Queue<string>(15)

			//Act
			var lastExceptionsReported = ErrorReporter.LastExceptionsReported();

			//Assert
			AssertNotNull(lastExceptionsReported);
			AssertEquals(0, lastExceptionsReported.Count);
		}

		public void TestPopulateExceptionsBufferWhenExceptionsThrownIsNull()
		{
			//Arrange
			var testException = new Exception("Test Exception");

			//Assert
			AssertNoExceptionThrown(() =>
			{
				//Act
				ErrorReporter.PopulateExceptionsBuffer(testException);
			});

			AssertContains(testException.Message, ErrorReporter.LastExceptionsReported().FirstOrDefault());
		}

		public void TestPopulateExceptionsBufferWhenExceptionsThrownIsNotNull()
		{
			//Arrange
			ErrorReporter.Clear();
			var exceptions = ErrorReporter.ExceptionsThrown; // Sets _exceptionsThrown from null to new Queue<string>(15)

			var testException1 = new Exception("Test Exception 1");
			var testException2 = new Exception("Test Exception 2");

			//Act
			ErrorReporter.PopulateExceptionsBuffer(testException1);
			ErrorReporter.PopulateExceptionsBuffer(testException2);

			//Assert
			AssertContains(testException1.Message, ErrorReporter.LastExceptionsReported().FirstOrDefault());
			AssertContains(testException2.Message, ErrorReporter.LastExceptionsReported().Skip(1).FirstOrDefault());
		}

		static void AssertReportError(Exception exception, bool errorReportAlwaysExpected = false)
		{
			for (int i = 0; i <= 1; i++)
			{
				ErrorReporter.ReportOnce("Hi Mum", exception);
				if (i == 0 || errorReportAlwaysExpected)
				{
					AssertEquals("Hi Mum", ErrorReporter.LastMessageReported);
					ErrorReporter.LastMessageReported = null;
				}
				else
				{
					AssertNull(ErrorReporter.LastMessageReported);
				}
			}

			ErrorReporter.Clear();
		}

		[ExpectNoExceptions]
		public void TestReportOnce_CustomizedErrorReporterInstance()
		{
			// Arrange
			var reporter = new Mock<IErrorReporter>();
			reporter.Setup(x => x.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()))
				.Verifiable(Times.Once());

			// Act
			reporter.Object.ReportOnce("key", "message", new Exception());

			// Assert
			Mock.VerifyAll(reporter);

			// Cleanup
			ErrorReporter.Clear();
		}

		[SuppressMessage("Microsoft.Design", "CA1064:Exceptions should be public", Justification = "For test only")]
		[Serializable]
		class ExceptionImplementingReporterExtender : Exception, IErrorReporterExtender
		{
			internal ExceptionImplementingReporterExtender(bool shouldReportAlwaysInReportOnce)
			{
				ShouldReportAlwaysInReportOnce = shouldReportAlwaysInReportOnce;
			}

#if NETFRAMEWORK
			protected ExceptionImplementingReporterExtender(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
#endif

			readonly bool ShouldReportAlwaysInReportOnce;
			bool IErrorReporterExtender.ShouldReportAlwaysInReportOnce => ShouldReportAlwaysInReportOnce;
		}
	}
}
