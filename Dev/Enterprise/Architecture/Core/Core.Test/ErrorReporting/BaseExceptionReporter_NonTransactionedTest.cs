using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class BaseExceptionReporter_NonTransactionedTest : TestCase
	{
		public void TestDatabaseOfflineError_ShouldNotReport()
		{
			var error = SqlExceptionBuilder.CreateSqlError(-2, 1, 1, "", "Test SQL Exception should have been caught", "", 1);
			var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);
			Db.Connection.Dispose();
			AssertEquals(ConnectionState.Closed, Db.Connection.State);
			AssertEquals(true, ExceptionReporter.Instance.exceptionHandler.HandleSqlException(exception, new InvalidOperationException()));
			AssertEquals(ConnectionState.Closed, Db.Connection.State);
		}
	}
}
