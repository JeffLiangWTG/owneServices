using System;
using CargoWise.Data.Testing;
using Enterprise.ProductRegistration.Common;
using Moq;
using NUnit.Framework;

namespace CargoWise.ProductRegistration.Service.Test
{
	public class RegistrationRepositoryWithRetryTest : TestCase
	{
		public void TestGetStatus_RetryAfterException()
		{
			var mockConnectionManager = new Mock<IConnectionManager>();
			var mockRepo = new Mock<IRegistrationRepository>();
			var ex = CreateSqlException(18470);
			mockRepo.SetupSequence(m => m.GetStatus("ABCDEF"))
				.Throws(ex)
				.ReturnsAsync(new DbStatus() { Status = "REG" });
			mockRepo.Setup(m => m.Connection).Returns(mockConnectionManager.Object);
			mockConnectionManager.Setup(m => m.Reset());

			var repo = new RegistrationRepositoryWithRetry(mockRepo.Object);
			AssertEquals("REG", repo.GetStatus("ABCDEF").Result.Status);
			mockRepo.Verify(m => m.GetStatus("ABCDEF"), Times.Exactly(2));
			mockRepo.VerifyAll();
			mockConnectionManager.VerifyAll();
		}

		public void TestGetStatus_ExceptionRethrownAfterOneRetry()
		{
			var mockConnectionManager = new Mock<IConnectionManager>();
			var mockRepo = new Mock<IRegistrationRepository>();
			var ex = CreateSqlException(18470);
			mockRepo.SetupSequence(m => m.GetStatus("ABCDEF"))
				.Throws(ex)
				.Throws(ex)
				.ReturnsAsync(new DbStatus() { Status = "REG" });
			mockRepo.Setup(m => m.Connection).Returns(mockConnectionManager.Object);
			mockConnectionManager.Setup(m => m.Reset());

			var repo = new RegistrationRepositoryWithRetry(mockRepo.Object);
			AssertExceptionThrown(typeof(AggregateException), () => { repo.GetStatus("ABCDEF").Wait(); });
			mockRepo.Verify(m => m.GetStatus("ABCDEF"), Times.Exactly(2));
			mockRepo.VerifyAll();
			mockConnectionManager.VerifyAll();
		}

		public void TestRegister()
		{
			var mockConnectionManager = new Mock<IConnectionManager>();
			var mockRepo = new Mock<IRegistrationRepository>();
			var ex = CreateSqlException(18470);
			var request = new RegisterRequest();
			mockRepo.SetupSequence(m => m.Register(request, "pwd"))
				.Throws(ex)
				.ReturnsAsync(new DbRegisterResult() { ReturnCode = 1 });
			mockRepo.Setup(m => m.Connection).Returns(mockConnectionManager.Object);
			mockConnectionManager.Setup(m => m.Reset());

			var repo = new RegistrationRepositoryWithRetry(mockRepo.Object);
			AssertEquals(1, repo.Register(request, "pwd").Result.ReturnCode);
			mockRepo.Verify(m => m.Register(request, "pwd"), Times.Exactly(2));
			mockRepo.VerifyAll();
			mockConnectionManager.VerifyAll();
		}

		public void TestVerify()
		{
			var mockConnectionManager = new Mock<IConnectionManager>();
			var mockRepo = new Mock<IRegistrationRepository>();
			var ex = CreateSqlException(18470);
			var request = new VerifyRequest();
			mockRepo.SetupSequence(m => m.Verify(1, "{password}", null))
				.Throws(ex)
				.ReturnsAsync(new DbRegisterResult() { ReturnCode = 1 });
			mockRepo.Setup(m => m.Connection).Returns(mockConnectionManager.Object);
			mockConnectionManager.Setup(m => m.Reset());

			var repo = new RegistrationRepositoryWithRetry(mockRepo.Object);
			AssertEquals(1, repo.Verify(1, "{password}", null).Result.ReturnCode);
			mockRepo.Verify(m => m.Verify(1, "{password}", null), Times.Exactly(2));
			mockRepo.VerifyAll();
			mockConnectionManager.VerifyAll();
		}

		public void TestUnregister()
		{
			var mockConnectionManager = new Mock<IConnectionManager>();
			var mockRepo = new Mock<IRegistrationRepository>();
			var ex = CreateSqlException(18470);
			var request = new RegisterRequest();
			mockRepo.SetupSequence(m => m.Unregister(1, "pwd"))
				.Throws(ex)
				.ReturnsAsync(1);
			mockRepo.Setup(m => m.Connection).Returns(mockConnectionManager.Object);
			mockConnectionManager.Setup(x => x.Reset());

			var repo = new RegistrationRepositoryWithRetry(mockRepo.Object);
			AssertEquals(1, repo.Unregister(1, "pwd").Result);
			mockRepo.Verify(m => m.Unregister(1, "pwd"), Times.Exactly(2));
			mockRepo.VerifyAll();
			mockConnectionManager.VerifyAll();
		}

		SqlException CreateSqlException(int sqlErrorNumber)
		{
			var error = SqlExceptionBuilder.CreateSqlError(sqlErrorNumber, 1, 1, "", "Something to see here", "", 1);
			var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errorCollection);
			return exception;
		}
	}
}

