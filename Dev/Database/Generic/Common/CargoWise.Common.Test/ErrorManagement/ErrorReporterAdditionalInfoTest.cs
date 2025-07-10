using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.Common
{
	public class ErrorReporterAdditionalInfoTest : TestCase
	{
		public void TestAdditionalInformation()
		{
			using (ErrorReporter.GatherAdditionalInformation())
			{
				ErrorReporter.SetAdditionalInfo("KEY", "VALUE");
				ErrorReporter.SetAdditionalInfo("SOME", "SOME_KEY", "some value");
				ErrorReporter.SetAdditionalInfo("UMI", "UMI_KEY", "Hi sir what is your value...");
				ErrorReporter.SetAdditionalInfo("LWK", "LWK_KEY", "I am not french");
				ErrorReporter.ReportOnceWithAdditionalInfo("A", "Hi", "LWK", "UMI");
			}
			AssertEquals("Hi\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: UMI\r\nUMI_KEY:\r\nHi sir what is your value...\r\n\r\nCATEGORY: LWK\r\nLWK_KEY:\r\nI am not french",
				ErrorReporter.LastMessageReported);
			ErrorReporter.ReportOnceWithAdditionalInfo("B", "Hi", "LWK", "UMI");
			AssertEquals(@"Hi", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationInfoFlagSet()
		{
			using (ErrorReporter.GatherAdditionalInformation())
			{
				ErrorReporter.CreateInfoFlag("FLAGSET");

				ErrorReporter.SetAdditionalInfoIfFlagSet("CATEGORY_NAME", "KEY_NAME", "VALUE", "FLAGSET");

				ErrorReporter.ReportOnceWithAdditionalInfo("KEY", "Hi", "CATEGORY_NAME");
			}

			AssertEquals("Hi\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: CATEGORY_NAME\r\nKEY_NAME:\r\nVALUE",
				ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationInfoFlagNotSet()
		{
			using (ErrorReporter.GatherAdditionalInformation())
			{
				ErrorReporter.SetAdditionalInfoIfFlagSet("CATEGORY_NAME", "KEY_NAME", "VALUE", "FLAGSET");
				ErrorReporter.ReportOnceWithAdditionalInfo("KEY", "Hi", "CATEGORY_NAME");
			}

			AssertEquals("Hi", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationInfoFlagStackTrace()
		{
			using (ErrorReporter.GatherAdditionalInformation())
			{
				ErrorReporter.CreateInfoFlag("FLAGSET");

				ErrorReporter.AddStackTraceToAdditionalInfoIfFlagSet("CATEGORY_NAME", "KEY_NAME", "FLAGSET");

				ErrorReporter.ReportOnceWithAdditionalInfo("KEY", "Hi", "CATEGORY_NAME");
			}

			AssertStartsWith("AdditionalInfo StackTrace", "Hi\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: CATEGORY_NAME\r\nKEY_NAME:\r\n",
				ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationLazy()
		{
			var someValue = "SomeValue";
			using (ErrorReporter.GatherAdditionalInformation())
			{
				ErrorReporter.SetAdditionalInfo("KEY", () => someValue);
				ErrorReporter.ReportOnceWithAdditionalInfo("A", "Hi");
			}
			AssertEquals("Hi\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: NONE\r\nKEY:\r\nSomeValue",
				ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationNoCategory()
		{
			using (ErrorReporter.GatherAdditionalInformation())
			{
				ErrorReporter.SetAdditionalInfo("KEY", "VALUE");
				ErrorReporter.ReportOnceWithAdditionalInfo("Hi");
			}
			AssertEquals("Hi\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: NONE\r\nKEY:\r\nVALUE",
					ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationOverridesValue()
		{
			using (ErrorReporter.GatherAdditionalInformation())
			{
				ErrorReporter.SetAdditionalInfo("KEY", "A");
				ErrorReporter.SetAdditionalInfo("KEY", "B");
				ErrorReporter.SetAdditionalInfo("KEY", "C");
				ErrorReporter.ReportOnceWithAdditionalInfo("Hi");
			}
			AssertEquals("Hi\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: NONE\r\nKEY:\r\nC",
				ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationWithException()
		{
			using (ErrorReporter.GatherAdditionalInformation())
			{
				ErrorReporter.SetAdditionalInfo("IMPORTANT", "KEY", "VALUE");
				ErrorReporter.ReportOnceWithAdditionalInfo("Hi", new Exception("Exception"), "IMPORTANT");
			}
			AssertEquals("Hi\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: IMPORTANT\r\nKEY:\r\nVALUE",
				ErrorReporter.LastMessageReported);
			AssertEquals("Exception", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationNested()
		{
			using (ErrorReporter.GatherAdditionalInformation())
			{
				using (ErrorReporter.GatherAdditionalInformation())
				{
					ErrorReporter.SetAdditionalInfo("KEY", "VALUE");
					ErrorReporter.SetAdditionalInfo("SOME", "SOME_KEY", "some value");
					ErrorReporter.SetAdditionalInfo("UMI", "UMI_KEY", "Hi sir what is your value...");
					ErrorReporter.SetAdditionalInfo("LWK", "LWK_KEY", "I am not french");
				}
				ErrorReporter.ReportOnceWithAdditionalInfo("A", "Hi", "LWK", "UMI");
			}
			AssertEquals("Hi\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: UMI\r\nUMI_KEY:\r\nHi sir what is your value...\r\n\r\nCATEGORY: LWK\r\nLWK_KEY:\r\nI am not french",
				ErrorReporter.LastMessageReported);
			ErrorReporter.ReportOnceWithAdditionalInfo("B", "Hi", "LWK", "UMI");
			AssertEquals(@"Hi", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationDoesNotLeakOverThreads()
		{
			var pauseEventOne = new ManualResetEvent(false);
			var pauseEventTwo = new ManualResetEvent(false);

			var threads = new List<Thread>();

			threads.Add(new Thread(() =>
			{
				using (ErrorReporter.GatherAdditionalInformation())
				{
					ErrorReporter.SetAdditionalInfo("KEY", "VALUE");
					pauseEventOne.Set();
					pauseEventTwo.WaitOne();
				}
			}));

			threads.Add(new Thread(() =>
			{
				pauseEventOne.WaitOne();
				ErrorReporter.ReportOnceWithAdditionalInfo("B", "Hi");
				pauseEventTwo.Set();
			}));

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			AssertEquals(@"Hi", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationDoesNotLeakOverThreadsWithMultipleStores()
		{
			var pauseEventOne = new ManualResetEvent(false);
			var pauseEventTwo = new ManualResetEvent(false);
			var pauseEventThree = new ManualResetEvent(false);

			var messagethread1 = "";
			var messagethread2 = "";

			var threads = new List<Thread>();

			threads.Add(new Thread(() =>
			{
				using (ErrorReporter.GatherAdditionalInformation())
				{
					ErrorReporter.SetAdditionalInfo("KEY", "SMITH");
					pauseEventTwo.Set();
					pauseEventOne.WaitOne();
					pauseEventThree.WaitOne();
					ErrorReporter.ReportOnceWithAdditionalInfo("A", "SMITH");
					messagethread1 = ErrorReporter.LastMessageReported;
				}
			}));

			threads.Add(new Thread(() =>
			{
				using (ErrorReporter.GatherAdditionalInformation())
				{
					ErrorReporter.SetAdditionalInfo("KEY", "JOHN");
					pauseEventOne.Set();
					pauseEventTwo.WaitOne();
					ErrorReporter.ReportOnceWithAdditionalInfo("B", "JOHN");
					messagethread2 = ErrorReporter.LastMessageReported;
					pauseEventThree.Set();
				}
			}));

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			AssertEquals($"SMITH\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: NONE\r\nKEY:\r\nSMITH", messagethread1);
			AssertEquals($"JOHN\r\n\r\n-- Additional Information --\r\n\r\nCATEGORY: NONE\r\nKEY:\r\nJOHN", messagethread2);
			AssertEquals(2, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}
	}
}
