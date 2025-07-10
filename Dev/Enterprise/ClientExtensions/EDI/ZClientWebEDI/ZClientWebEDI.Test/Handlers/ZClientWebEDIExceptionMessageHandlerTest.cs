using System;
using System.Web.Http.ExceptionHandling;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;
using Enterprise.ZClientWebCargoWiseEDI.Handlers;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class ZClientWebEDIExceptionMessageHandlerTest : TestCase
	{
		public void TestWhenExceptionIsNotRelatedToDBUpgrade()
		{
			var exceptionContext = new ExceptionContext(new InvalidOperationException(), new ExceptionContextCatchBlock("", true, true));
			var exceptionHandlerContext = new ExceptionHandlerContext(exceptionContext);

			var result = ZClientWebEDIExceptionMessageHandler.HandleException(exceptionHandlerContext);
			Assert("Only DB upgrade related exceptions are handled", !result);
			AssertNull(exceptionHandlerContext.Result);

			ErrorReporter.Clear();
		}

		public void TestWhenExceptionIsOnDatabaseUpgradeInProgressException()
		{
			var exceptionContext = new ExceptionContext(new DatabaseUpgradedException(), new ExceptionContextCatchBlock("", true, true));
			var exceptionHandlerContext = new ExceptionHandlerContext(exceptionContext);

			var result = ZClientWebEDIExceptionMessageHandler.HandleException(exceptionHandlerContext);
			Assert("DB upgrade related exceptions are handled", result);
			AssertType<EnterpriseWebApiExceptionHandler.DatabaseUpgradeResponse>(exceptionHandlerContext.Result);

			ErrorReporter.Clear();
		}

		public void TestWhenExceptionIsOnDatabaseUpgradedException()
		{
			var exceptionContext = new ExceptionContext(new DatabaseUpgradeInProgressException(), new ExceptionContextCatchBlock("", true, true));
			var exceptionHandlerContext = new ExceptionHandlerContext(exceptionContext);

			var result = ZClientWebEDIExceptionMessageHandler.HandleException(exceptionHandlerContext);
			Assert("DB upgrade related exceptions are handled", result);
			AssertType<EnterpriseWebApiExceptionHandler.DatabaseUpgradeResponse>(exceptionHandlerContext.Result);

			ErrorReporter.Clear();
		}

		public void TestWhenInnerExceptionIsOnDatabaseUpgradedException()
		{
			var testException = new Exception();
			try
			{
				UseInvalidExceptionWrapOnDatabaseUpgradedException();
			}
			catch (Exception ex)
			{
				testException = ex;
			}

			var exceptionContext = new ExceptionContext(testException, new ExceptionContextCatchBlock("", true, true));
			var exceptionHandlerContext = new ExceptionHandlerContext(exceptionContext);
			var result = ZClientWebEDIExceptionMessageHandler.HandleException(exceptionHandlerContext);
			Assert("DB upgrade related exceptions are handled", result);
			AssertType<EnterpriseWebApiExceptionHandler.DatabaseUpgradeResponse>(exceptionHandlerContext.Result);

			ErrorReporter.Clear();
		}

		public void TestWhenInnerExceptionIsOnDatabaseUpgradeInProgressException()
		{
			var testException = new Exception();
			try
			{
				UseInvalidExceptionWrapOnDatabaseUpgradeInProgressException();
			}
			catch (Exception ex)
			{
				testException = ex;
			}

			var exceptionContext = new ExceptionContext(testException, new ExceptionContextCatchBlock("", true, true));
			var exceptionHandlerContext = new ExceptionHandlerContext(exceptionContext);
			var result = ZClientWebEDIExceptionMessageHandler.HandleException(exceptionHandlerContext);
			Assert("DB upgrade related exceptions are handled", result);
			AssertType<EnterpriseWebApiExceptionHandler.DatabaseUpgradeResponse>(exceptionHandlerContext.Result);

			ErrorReporter.Clear();
		}

		public void UseInvalidExceptionWrapOnDatabaseUpgradedException()
		{
			try
			{
				throw new DatabaseUpgradedException();
			}
			catch (DatabaseUpgradedException ex)
			{
				throw new InvalidOperationException("test UseInvalidExceptionWrapOnDatabaseUpgradedException", ex);
			}
		}

		public void UseInvalidExceptionWrapOnDatabaseUpgradeInProgressException()
		{
			try
			{
				throw new DatabaseUpgradeInProgressException();
			}
			catch (DatabaseUpgradeInProgressException ex)
			{
				throw new InvalidOperationException("test UseInvalidExceptionWrapOnDatabaseUpgradeInProgressException", ex);
			}
		}
	}
}
