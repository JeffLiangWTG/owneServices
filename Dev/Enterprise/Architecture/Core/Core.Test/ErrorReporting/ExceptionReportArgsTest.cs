using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ExceptionReportArgsTest : TestCase
	{
		public void TestEx()
		{
			AssertEquals(Ex, Args.Ex);
		}

		public void TestKey()
		{
			AssertEquals("Key", Key, Args.Key);
		}

		public void TestErrorReportID()
		{
			AssertEquals(ErrorReportID, Args.ErrorReportID);
		}

		public void TestErrorDescription()
		{
			AssertEquals(ErrorDescription, Args.ErrorDescription);
		}

		public void TestSubjectPrefix()
		{
			AssertEquals(null, Args.SubjectPrefix);
			string expected = "Prefix";
			Args.SubjectPrefix = expected;
			AssertEquals(expected, Args.SubjectPrefix);
		}

		public void TestShutDownApplication()
		{
			AssertEquals(false, Args.ShutDownApplication);
			Args.ShutDownApplication = true;
			AssertEquals(true, Args.ShutDownApplication);
		}

		#region Implementation

		Exception Ex;
		const string ErrorReportID = "Test Error ID";
		ExceptionReportArgs Args;
		const string Key = "Test Key";
		const string ErrorDescription = "Testing the exception report";

		protected override void SetUp()
		{
			Ex = new ArgumentException("Exception Report Test");
			Args = new ExceptionReportArgs(Ex, ErrorReportID, Key, ErrorDescription);
		}

		#endregion
	}
}
