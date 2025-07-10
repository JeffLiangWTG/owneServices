using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Common.ErrorManagement;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public abstract class ExceptionFullTracerTest : TestCase
	{
		public void TestBuildTrace()
		{
			var tracer = new TestExceptionFullTracer();
			tracer.AssertTrace();
		}

		public void TestStripLineNoAndFilename()
		{
			var tracer = new TestExceptionFullTracer();
			AssertEquals("   at bla.bla(bla bla)\r\n", tracer.StripLineNoAndFilename("   at bla.bla(bla bla) in c:\bla\bla \r\n")); // Test data bla bla
			AssertEquals("   at (object)\r\n", tracer.StripLineNoAndFilename("   at (object) in d:\bla\bla \r\n"));
		}

		#region TestRemoteExceptionHasFullStackTrace

		public void TestRemoteExceptionHasFullStackTrace()
		{
			try
			{
				MockRemotePassingMethod();
			}
			catch (Exception ex)
			{
				var tracer = new TestExceptionFullTracer();
				Assert("Precondition: Has ReportException in trace", ex.StackTrace.Contains("ReportException(Int32 counter)"));
				AssertEquals(ExpectedStackTrace, tracer.BuildTraceExposed(ex).TrimEnd());
			}
		}

		void MockRemotePassingMethod()
		{
			try
			{
				SuperDuperMethod(5);
			}
			catch (Exception ex)
			{
				typeof(Exception).GetMethod("PrepForRemoting", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ex, Array.Empty<object>());
#pragma warning disable CA2200
				throw ex; // Here should be "throw ex;" for clearing current stack trace and simulate passing between app domains
#pragma warning restore CA2200
			}
		}

		void SuperDuperMethod(int counter)
		{
			if (counter <= 0)
			{
				HandleThreadException();
			}
			SuperDuperMethod(counter - 1);
		}

		void HandleThreadException()
		{
			ReportException(3);
		}

		void ReportException(int counter)
		{
			if (counter <= 0)
			{
				throw new InvalidOperationException();
			}
			ReportException(counter - 1);
		}

		const string ExpectedStackTrace =
@"   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.ReportException(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.ReportException(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.ReportException(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.ReportException(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.HandleThreadException()
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.SuperDuperMethod(Int32 counter)
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.MockRemotePassingMethod()
----- Exception rethrown at [0]: -----
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.MockRemotePassingMethod()
   at Enterprise.ZArchitecture.Core.Testing.ExceptionFullTracerTest.TestRemoteExceptionHasFullStackTrace()";

		#endregion

		#region TestStackTraceWithILOffSet

		public void TestStackTraceWithILOffSet()
		{
			try
			{
				SuperDuperMethod(2);
			}
			catch (Exception ex)
			{
				var tracer = new TestExceptionFullTracer();
				var result = tracer.BuildTraceExposed(ex).TrimEnd();
				AssertEquals(true, result.Contains(" [Assembly="));
				AssertEquals(true, result.Contains(" [Type="));
				AssertEquals(true, result.Contains(" [Method="));
				AssertEquals(true, result.Contains(" [ILOffset="));
				AssertEquals(true, result.Contains(" [Parameters="));
			}
		}

		public void TestStackTraceWithWithRootCauseStackTrace()
		{
			try
			{
				throw new ExceptionWithRootCauseStackTrace(GetRootCauseStackTrace());
			}
			catch (Exception ex)
			{
				var tracer = new TestExceptionFullTracer();
				AssertContains("GetRootCauseStackTrace", tracer.BuildTraceExposed(ex));
			}
		}

		StackTrace GetRootCauseStackTrace()
		{
			return new StackTrace();
		}

		[Serializable]
		class ExceptionWithRootCauseStackTrace : Exception, IWithRootCauseStackTrace
		{
			public ExceptionWithRootCauseStackTrace(StackTrace stackTrace)
			{
				rootCauseStackTrace = stackTrace;
			}

#if NETFRAMEWORK
			protected ExceptionWithRootCauseStackTrace(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
				: base(info, context)
			{
			}
#endif

			public StackTrace RootCauseStackTrace => rootCauseStackTrace;

			[NonSerialized]
			readonly StackTrace rootCauseStackTrace;
		}

		#endregion

		#region TestExceptionFullTracer

		class TestExceptionFullTracer : ExceptionFullTracer
		{
			public void AssertTrace()
			{
				string trace = BuildTrace(new StackTrace(), true);
				var lines = Regex.Split(trace, "\r\n|\r|\n");
				trace = "";

				foreach (var line in lines)
				{
					string newLine = line;
					int index = newLine.IndexOf(" [Assembly=");

					if (index > 0)
					{
						newLine = newLine.Substring(0, index);
					}

					if (newLine.Length > 0)
					{
						trace += newLine + System.Environment.NewLine;
					}
				}

				string oldTrace = MassageLegacyTraceForTest(System.Environment.StackTrace);
				AssertEquals(oldTrace, trace);
			}

			string MassageLegacyTraceForTest(string trace)
			{
				trace = StripLineNoAndFilenameFromWholeCallStack(trace);
				trace = trace.Replace("at ", "   at ");
				trace = trace.Replace("[T]", "");
				trace = trace.Replace(oldHeader, "");
				return trace;
			}

			public new string StripLineNoAndFilename(string frame)
			{
				return base.StripLineNoAndFilename(frame);
			}

			string StripLineNoAndFilenameFromWholeCallStack(string stack)
			{
				string result = "";
				string[] frames = stack.Split('\r');

				foreach (string frame in frames)
				{
					result += base.StripLineNoAndFilename(frame).Trim() + "\r\n";
				}

				return result;
			}

			public string BuildTraceExposed(Exception ex)
			{
				return BuildTrace(ex);
			}

			const string oldHeader = @"   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()
";
		}

		#endregion
	}
}
