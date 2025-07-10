using System;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class BaseExceptionBuilderTest : TransactionedTestCase
	{
		public void TestExeDateTime()
		{
			AssertEquals("ExeDateTimeCreated", EnvProxy.Instance.Time.FormatDateTimeWithSeconds(ReleaseInfo.Instance.ExeDate), TestBuilder.ExeDateTimeCreated);
		}

		public void TestVersionNumber()
		{
			AssertEquals("VersionNumber", ReleaseInfo.Instance.VersionNumber.ToString(), TestBuilder.VersionNumber);
		}

		#region Implementation

		protected virtual ExceptionBuilder TestBuilder
		{
			get
			{
				if (exceptionBuilder == null)
				{
					exceptionBuilder = new DummyExceptionBuilder(ExceptionReportArgsForTest, TestErrorTime);
				}
				return exceptionBuilder;
			}
		}

		ExceptionReportArgs ExceptionReportArgsForTest
		{
			get
			{
				if (exceptionReportArgs == null)
				{
					exceptionReportArgs = new ExceptionReportArgs(new Exception("This is a test exception"), TestErrorReportID, "This is a test key", "This is a test description");
				}
				return exceptionReportArgs;
			}
		}

		string TestErrorReportID
		{
			get
			{
				if (errorReportID == null)
				{
					errorReportID = System.DateTime.Now.ToString("yyyMMddHHmmss");
				}
				return errorReportID;
			}
		}

		string TestErrorTime
		{
			get
			{
				if (errorTime == null)
				{
					errorTime = System.DateTime.Now.ToString();
				}
				return errorTime;
			}
		}

		ExceptionReportArgs exceptionReportArgs;
		string errorReportID;
		string errorTime;
		DummyExceptionBuilder exceptionBuilder;

		#endregion Implementation
	}
}
