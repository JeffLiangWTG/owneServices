using System;
using CargoWise.Application;
using Enterprise.Accounting.Business.Testing.ScriptTests;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.Integration.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	class ReportErrorHandlerTest : TestCase
	{
		public void TestShouldSkipReportError()
		{
			var suspendSqlErrorList = new[] {
				ChinaGLAccountBalanceTest.ErrorMessageForInvalidMixedTiers
			};

			var reportSqlErrorSuspender = new ReportErrorHandler();

			AssertExceptionThrown<ArgumentNullException>(() => reportSqlErrorSuspender.ShouldSkipReportError(null));

			foreach (var errorMessage in suspendSqlErrorList)
			{
				AssertException(errorMessage, true);

				AssertException($@"{errorMessage}
Warning: Null value is eliminated by an aggregate or other SET operation.
Warning: Null value is eliminated by an aggregate or other SET operation.
Warning: Null value is eliminated by an aggregate or other SET operation.
Warning: Null value is eliminated by an aggregate or other SET operation.", true);

				AssertException($@"{errorMessage.Substring(0, errorMessage.Length - 1)}
Warning: Null value is eliminated by an aggregate or other SET operation.
Warning: Null value is eliminated by an aggregate or other SET operation.
Warning: Null value is eliminated by an aggregate or other SET operation.
Warning: Null value is eliminated by an aggregate or other SET operation.", false);
			}

			void AssertException(string errorMessage, bool expectedResultToSQLExecutionException)
			{
				var exception = new Exception(errorMessage);

				AssertEquals(false, reportSqlErrorSuspender.ShouldSkipReportError(exception).ShouldSkip);

#pragma warning disable SYSLIB0050, SYSLIB0051
				var info = new System.Runtime.Serialization.SerializationInfo(typeof(Exception), new System.Runtime.Serialization.FormatterConverter());
				var streamContext = new System.Runtime.Serialization.StreamingContext();
				exception.GetObjectData(info, streamContext);
#pragma warning restore SYSLIB0050, SYSLIB0051

#pragma warning disable CS0612 // Type or member is obsolete
				var (shouldSkip, displayMessage) = reportSqlErrorSuspender.ShouldSkipReportError(new DummySQLExecutionException(info, streamContext));
#pragma warning restore CS0612 // Type or member is obsolete
				AssertEquals(expectedResultToSQLExecutionException, shouldSkip);

				if (expectedResultToSQLExecutionException)
				{
					AssertEquals(errorMessage, displayMessage);
				}
			}
		}

		public void TestRegisterObjectFactory()
		{
			AssertEquals(typeof(ReportErrorHandler), ObjectFactory.Get<IReportErrorHandler>().GetType());
		}

		[Serializable]
		class DummySQLExecutionException : SQLExecutionException
		{
#if NET
			[Obsolete]
#endif
			public DummySQLExecutionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{
			}
		}
	}
}
