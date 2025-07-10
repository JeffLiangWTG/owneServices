using System;
using System.Web.Http.ExceptionHandling;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Utilities.Exceptions.Testing
{
	internal class EnterpriseWebApiExceptionHandlerTest : TestCase
	{
		readonly DisposableList disposables = new DisposableList(3);

		protected override void SetUp()
		{
			disposables.AddRange(new IDisposable[] { Globals.SetIsUnitTestingProductionFunctionality(), Globals.SetIsUserInteractiveForTest(false), WebExceptionReporterThrowForTest.EnableForTest() });
			base.SetUp();
		}

		protected override void TearDown()
		{
			disposables.Dispose();
			base.TearDown();
		}

		public void TestDatabaseUpgradedExceptionNotReported()
		{
			var context = new ExceptionHandlerContext(new ExceptionContext(new DatabaseUpgradedException(), new ExceptionContextCatchBlock("whatever", true, true)));
			AssertNoExceptionThrown(() => new EnterpriseWebApiExceptionHandler().Handle(context));
		}

		public void TestSqlLoginDisabledExceptionNotReported()
		{
			var error = SqlExceptionBuilder.CreateSqlError(18470, 1, 1, Db.Connection.ServerName, "Login failed for user. Reason: The account is disabled.", "", 1);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors);
			var context = new ExceptionHandlerContext(new ExceptionContext(exception, new ExceptionContextCatchBlock("whatever", true, true)));
			AssertNoExceptionThrown(() => new EnterpriseWebApiExceptionHandler().Handle(context));
		}

		[ExpectException(typeof(InvalidOperationException))]

		public void TestInvalidOperationExceptionIsReported()
		{
			var context = new ExceptionHandlerContext(new ExceptionContext(new InvalidOperationException(), new ExceptionContextCatchBlock("whatever", true, true)));
			new EnterpriseWebApiExceptionHandler().Handle(context);
		}

		public void TestDatabaseUpgradeInProgressExceptionNotReported()
		{
			var context = new ExceptionHandlerContext(new ExceptionContext(new DatabaseUpgradeInProgressException(), new ExceptionContextCatchBlock("whatever", true, true)));
			AssertNoExceptionThrown(() => new EnterpriseWebApiExceptionHandler().Handle(context));
		}

		public void TestDatabaseUpgradedInnerExceptionNotReported()
		{
			var context = new ExceptionHandlerContext(new ExceptionContext(new ApplicationException("fail", new InvalidOperationException("fail", new DatabaseUpgradedException())), new ExceptionContextCatchBlock("whatever", true, true)));
			AssertNoExceptionThrown(() => new EnterpriseWebApiExceptionHandler().Handle(context));
		}

		class WebExceptionReporterThrowForTest : WebExceptionReporter
		{
			protected override void DoReportException(Exception ex, string key, string message)
			{
				throw ex;
			}

			public static IDisposable EnableForTest()
			{
				new WebExceptionReporterThrowForTest().Enable();
				return new DisposableAction(() => ExceptionReporter.DisableExposed());
			}
		}
	}
}
