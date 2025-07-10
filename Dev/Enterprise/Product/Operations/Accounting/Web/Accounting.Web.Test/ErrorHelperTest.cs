using System;
using CargoWise.Common;
using Enterprise.Accounting.Web.Core;
using Enterprise.Accounting.Web.Exceptions;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Web.Testing
{
	public class ErrorHelperTest : TestCase
	{
		public void TestReportError_ShouldReturnCorrectMessage_AndCallReportOnce()
		{
			// Arrange
			var reporterMock = new Mock<IErrorReporter>();
			var exception = new InvalidOperationException("Test exception");
			var callerName = $"{nameof(ErrorHelperTest)}.{nameof(TestReportError_ShouldReturnCorrectMessage_AndCallReportOnce)}";
			var expectedKey = $"Accounting.Web_{callerName}_{exception.GetType().Name}";
			var expectedMessage = $"An unexpected error occurred. Please try again later.";

			using (ErrorReporter.SetTemporaryInstanceForTest(reporterMock.Object))
			{
				// Act
				var result = ErrorHelper.ReportError(exception, callerName);

				// Assert
				AssertEquals(expectedMessage, result);
				reporterMock.Verify(
					er => er.Report(
						It.Is<string>(s => s.Contains(expectedKey)),
						It.Is<string>(s => s == exception.Message),
						It.IsAny<Exception>()),
					Times.Once);
			}
		}

		public void TestReportError_ShouldReturnCorrectMessage_WithErrorId()
		{
			// Arrange
			var reporterMock = new Mock<IErrorReporter>();
			var exception = new InvalidOperationException("Test exception");
			var errorReportId = "123456";
			var callerName = $"{nameof(ErrorHelperTest)}.{nameof(TestReportError_ShouldReturnCorrectMessage_AndCallReportOnce)}";
			var expectedKey = $"Accounting.Web_{callerName}_{exception.GetType().Name}";
			var expectedMessage = $"An unexpected error occurred. Please try again later. Error Report Id: {errorReportId}.";

			using (ErrorReporter.SetTemporaryInstanceForTest(reporterMock.Object))
			{
				reporterMock.Setup(
					er => er.Report(
						It.IsAny<string>(), It.IsAny<string>(),
						It.IsAny<Exception>())).Callback((string key, string message, Exception ex) =>
							{
								Assert("Exception should be an AccountingWebReportableException", ex is AccountingWebReportableException);
								(ex as AccountingWebReportableException).ErrorReportID = errorReportId;
							}
						);

				// Act
				var result = ErrorHelper.ReportError(exception, callerName);

				// Assert
				AssertEquals("Result message should contain the Error Id", expectedMessage, result);
			}
		}

		public void TestReportError_ShouldNotReportConcurrencyError()
		{
			// Arrange
			var reporterMock = new Mock<IErrorReporter>();
			var exception = GetConcurrencyErrorException();

			var callerName = $"{nameof(ErrorHelperTest)}.{nameof(TestReportError_ShouldNotReportConcurrencyError)}";
			var expectedMessage = "An unexpected error occurred. Please try again later.";

			using (ErrorReporter.SetTemporaryInstanceForTest(reporterMock.Object))
			{
				// Act
				var result = ErrorHelper.ReportError(exception, callerName);

				// Assert
				AssertEquals(expectedMessage, result);
				reporterMock.Verify(
					er => er.Report(
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<Exception>()),
					Times.Never);
			}
		}

		SqlException GetConcurrencyErrorException()
		{
			// The SQL is coming from the failure conditions in UpdatePaymentDetailOldSql
			// and UpdatePaymentDetailNewSql in TransactionPaymentDataAccess class.
			var sqlText = "RAISERROR('ConcurrencyError', 16, 1)";

			using var connection = DbAccess.NewConnection();
#pragma warning disable CW1116 // Use CargoWise.Data.Db.Connection
			using var command = connection.CreateCommand();
#pragma warning restore CW1116 // Use CargoWise.Data.Db.Connection
			command.CommandText = sqlText;

			try
			{
				command.ExecuteScalar();
			}
			catch (SqlException ex)
			{
				if (ex.Message != "ConcurrencyError")
				{
					throw new ArgumentException("Sql Exception does not have the expected message 'ConcurrencyError'");
				}

				return ex;
			}

			throw new ArgumentException("Sql Exception was not generated");
		}
	}
}
