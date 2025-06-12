using System;
using System.ServiceModel;
using System.Web;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Runtime.InteropServices;
using System.ServiceModel.Channels;
using CargoWise.eHub.Common;
using System.Configuration;

namespace CargoWise.eHub.Gateway.Tests
{
	[TestClass]
	public class eHubErrorHandlerTests
	{
		[TestMethod]
		public void TestProvideFault_Logging_0x80070057()
		{
			var exception0x80070057 = CreateCommunicationException("The remote host closed the connection. The error code is 0x80070057.", -2147024809);

			var testLogger = MockRepository.GenerateMock<ILog>();
			var threadContext = MockRepository.GenerateMock<IVariablesContext>();
			testLogger.Expect(_ => _.Error(exception0x80070057));
			testLogger.Expect(_ => _.ThreadVariablesContext).Return(threadContext);
			threadContext.Expect(_ => _.Set(Arg<string>.Is.Equal("ExceptionID"), Arg<string>.Is.Anything));
			threadContext.Expect(_ => _.Remove("ExceptionID"));

			var errorHandler = new eHubErrorHandler();
			errorHandler.SetLogger(testLogger);

			Message message = null;
			errorHandler.ProvideFault(exception0x80070057, MessageVersion.CreateVersion(EnvelopeVersion.Soap11), ref message);

			testLogger.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestProvideFault_Logging_0x800703E3InErrorCode()
		{
			var cOMException = CreateCOMException("The I/O operation has been aborted because of either a thread exit or an application request. The error code is 0x800703E3.", -2147023901, true);
			var httpException = new HttpException("An error occurred while communicating with the remote host. The error code is 0x800703E3.", cOMException);
			var exception0x800703E3 = new CommunicationException("An error occurred while communicating with the remote host. The error code is 0x800703E3.", httpException);

			var testLogger = MockRepository.GenerateMock<ILog>();
			var threadContext = MockRepository.GenerateMock<IVariablesContext>();
			testLogger.Expect(_ => _.Error(exception0x800703E3));
			testLogger.Expect(_ => _.ThreadVariablesContext).Return(threadContext);
			threadContext.Expect(_ => _.Set(Arg<string>.Is.Equal("ExceptionID"), Arg<string>.Is.Anything));
			threadContext.Expect(_ => _.Remove("ExceptionID"));

			var errorHandler = new eHubErrorHandler();
			errorHandler.SetLogger(testLogger);

			Message message = null;
			errorHandler.ProvideFault(exception0x800703E3, MessageVersion.CreateVersion(EnvelopeVersion.Soap11), ref message);

			testLogger.VerifyAllExpectations();
		}

		[TestMethod]
		public void TestProvideFault_Logging_0x800703E3InHResult()
		{
			var cOMException = CreateCOMException("The I/O operation has been aborted because of either a thread exit or an application request.", -2147023901, false);
			var httpException = new HttpException("An error occurred while communicating with the remote host. The error code is 0x800703E3.", cOMException);
			var exception0x800703E3 = new CommunicationException("An error occurred while communicating with the remote host. The error code is 0x800703E3.", httpException);

			var testLogger = MockRepository.GenerateMock<ILog>();
			var threadContext = MockRepository.GenerateMock<IVariablesContext>();
			testLogger.Expect(_ => _.Error(exception0x800703E3));
			testLogger.Expect(_ => _.ThreadVariablesContext).Return(threadContext);
			threadContext.Expect(_ => _.Set(Arg<string>.Is.Equal("ExceptionID"), Arg<string>.Is.Anything));
			threadContext.Expect(_ => _.Remove("ExceptionID"));

			var errorHandler = new eHubErrorHandler();
			errorHandler.SetLogger(testLogger);

			Message message = null;
			errorHandler.ProvideFault(exception0x800703E3, MessageVersion.CreateVersion(EnvelopeVersion.Soap11), ref message);

			testLogger.VerifyAllExpectations();
		}

		[TestMethod]
		public void AddExceptionIDToError()
		{
			var exceptionText = @"This is a reasonably lenghty error description to test large amount of exception data being returned via the service.
				Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor.
				Aenean massa.Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.
				Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem.Nulla consequat massa quis enim.Donec pede justo, fringilla vel, aliquet nec, vulputate.
				Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.
				Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.
				Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?
				Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?";


			var exceptionReportTextSizeLimit = int.TryParse(ConfigurationManager.AppSettings["ExceptionReportTextSizeLimit"], out var textLimit) ? textLimit : 307200;
			var errorHandler = new eHubErrorHandler();
			var errorMessage = errorHandler.AddExceptionIDToError(new Exception(exceptionText));

			Assert.AreEqual(2000, exceptionReportTextSizeLimit);
			Assert.IsTrue(exceptionText.Length < exceptionReportTextSizeLimit);
			Assert.IsTrue(errorMessage.Length < exceptionReportTextSizeLimit);
			Assert.IsTrue(errorMessage.Contains($"voluptas nulla pariatur? ExceptionID:"));
		}

		[TestMethod]
		public void AddExceptionIDToError_StringLimitReached()
		{
			var exceptionText = @"This is a reasonably lenghty error description to test large amount of exception data being returned via the service.
				Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor.
				Aenean massa.Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus.
				Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem.Nulla consequat massa quis enim.Donec pede justo, fringilla vel, aliquet nec, vulputate.
				Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo.
				Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt.
				Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur?
				Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?";

			var errorHandler = new eHubErrorHandler();
			var errorMessage = errorHandler.AddExceptionIDToError(new Exception(exceptionText + exceptionText));


			var exceptionReportTextSizeLimit = int.TryParse(ConfigurationManager.AppSettings["ExceptionReportTextSizeLimit"], out var textLimit) ? textLimit : 307200;

			Assert.AreEqual(2000, exceptionReportTextSizeLimit);
			Assert.IsTrue((exceptionText + exceptionText).Length > exceptionReportTextSizeLimit);
			Assert.IsFalse(errorMessage.Length > exceptionReportTextSizeLimit);
			Assert.IsTrue(errorMessage.Contains($"... Batch error text limit reached. Error text truncated\r\n ExceptionID:"));
		}

		[TestCleanup]
		public void ResetLogger()
		{
			var errorHandler = new eHubErrorHandler();
			errorHandler.SetLogger(LogManager.GetLogger(typeof(eHubErrorHandler)));
		}


		CommunicationException CreateCommunicationException(string message, int hResult)
		{
			var innerException = new HttpException(message);
			SetHResult(innerException, hResult);
			return new CommunicationException(message, innerException);
		}

		COMException CreateCOMException(string message, int hResult, bool setErrorCode)
		{
			if (setErrorCode)
				return new COMException(message, hResult);

			var exception = new COMException(message);
			SetHResult(exception, hResult);
			return exception;
		}

		void SetHResult(Exception exception, int hResult)
		{
			var propertyInfo = exception.GetType().GetProperty("HResult");
			propertyInfo.SetValue(exception, hResult, null);
		}
	}
}

