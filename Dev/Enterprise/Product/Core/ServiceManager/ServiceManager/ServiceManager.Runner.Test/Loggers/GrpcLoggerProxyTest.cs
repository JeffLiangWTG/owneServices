using System;
using Enterprise.ServiceManager.Runner;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Loggers
{
	public class GrpcLoggerProxyTest
	{
		[SetUp]
		public void SetUp()
		{
			runnerLoggerMock = new Mock<IRunnerLogger>();
			grpcLoggerProxy = new GrpcLoggerProxy(runnerLoggerMock.Object);
			testException1 = new Exception("someException");
			testException2 = new Exception("someException2");
		}

		[Test]
		public void TestInfo() => AssertLogMessage(() => grpcLoggerProxy.Info("someMessage"), LogLevel.Debug, "Grpc severity:[Information]:someMessage");

		[Test]
		public void TestInfoFormat() => AssertLogMessage(() => grpcLoggerProxy.Info("{0}{1}", "some", "Message2"), LogLevel.Debug, "Grpc severity:[Information]:someMessage2");

		[Test]
		public void TestDebug() => AssertLogMessage(() => grpcLoggerProxy.Debug("someMessage3"), LogLevel.Debug, "Grpc severity:[Debug]:someMessage3");

		[Test]
		public void TestDebugFormat() => AssertLogMessage(() => grpcLoggerProxy.Debug("{0}{1}", "some", "Message4"), LogLevel.Debug, "Grpc severity:[Debug]:someMessage4");

		[Test]
		public void TestWarning() => AssertLogMessage(() => grpcLoggerProxy.Warning("someMessage5"), LogLevel.Debug, "Grpc severity:[Warning]:someMessage5");

		[Test]
		public void TestWarningFormat() => AssertLogMessage(() => grpcLoggerProxy.Warning("{0}{1}", "some", "Message6"), LogLevel.Debug, "Grpc severity:[Warning]:someMessage6");

		[Test]
		public void TestWarningException() => AssertLogMessage(() => grpcLoggerProxy.Warning(testException1, "someMessage7"), LogLevel.Debug, "Grpc severity:[Warning]:someMessage7", testException1);

		[Test]
		public void TestError() => AssertLogMessage(() => grpcLoggerProxy.Error("someMessage8"), LogLevel.Error, "Grpc severity:[Error]:someMessage8");

		[Test]
		public void TestErrorFormat() => AssertLogMessage(() => grpcLoggerProxy.Error("{0}{1}", "some", "Message9"), LogLevel.Error, "Grpc severity:[Error]:someMessage9");

		[Test]
		public void TestErrorException() => AssertLogMessage(() => grpcLoggerProxy.Error(testException2, "someMessage10"), LogLevel.Error, "Grpc severity:[Error]:someMessage10", testException2);

		[Test]
		public void TestForTypeInfo() => AssertLogMessage(() => grpcLoggerProxy.ForType<string>().Info("someMessage"), LogLevel.Debug, "Grpc severity:[Information]|type:[System.String]:someMessage");

		[Test]
		public void TestForTypeInfoFormat() => AssertLogMessage(() => grpcLoggerProxy.ForType<double>().Info("{0}{1}", "some", "Message2"), LogLevel.Debug, "Grpc severity:[Information]|type:[System.Double]:someMessage2");

		[Test]
		public void TestForTypeDebug() => AssertLogMessage(() => grpcLoggerProxy.ForType<string>().Debug("someMessage3"), LogLevel.Debug, "Grpc severity:[Debug]|type:[System.String]:someMessage3");

		[Test]
		public void TestForTypeDebugFormat() => AssertLogMessage(() => grpcLoggerProxy.ForType<double>().Debug("{0}{1}", "some", "Message4"), LogLevel.Debug, "Grpc severity:[Debug]|type:[System.Double]:someMessage4");

		[Test]
		public void TestForTypeWarning() => AssertLogMessage(() => grpcLoggerProxy.ForType<string>().Warning("someMessage5"), LogLevel.Debug, "Grpc severity:[Warning]|type:[System.String]:someMessage5");

		[Test]
		public void TestForTypeWarningFormat() => AssertLogMessage(() => grpcLoggerProxy.ForType<double>().Warning("{0}{1}", "some", "Message6"), LogLevel.Debug, "Grpc severity:[Warning]|type:[System.Double]:someMessage6");

		[Test]
		public void TestForTypeWarningException() => AssertLogMessage(() => grpcLoggerProxy.ForType<string>().Warning(testException1, "someMessage7"), LogLevel.Debug, "Grpc severity:[Warning]|type:[System.String]:someMessage7", testException1);

		[Test]
		public void TestForTypeError() => AssertLogMessage(() => grpcLoggerProxy.ForType<double>().Error("someMessage8"), LogLevel.Error, "Grpc severity:[Error]|type:[System.Double]:someMessage8");

		[Test]
		public void TestForTypeErrorFormat() => AssertLogMessage(() => grpcLoggerProxy.ForType<string>().Error("{0}{1}", "some", "Message9"), LogLevel.Error, "Grpc severity:[Error]|type:[System.String]:someMessage9");

		[Test]
		public void TestForTypeErrorException() => AssertLogMessage(() => grpcLoggerProxy.ForType<double>().Error(testException2, "someMessage10"), LogLevel.Error, "Grpc severity:[Error]|type:[System.Double]:someMessage10", testException2);

		void AssertLogMessage(Action action, LogLevel expectedType, string expectedMessage, Exception expectedException = null)
		{
			runnerLoggerMock.Reset();
			Assert.DoesNotThrow(() => action());
			if (expectedException == null)
			{
				runnerLoggerMock.Verify(l => l.Log(expectedType, expectedMessage));
			}
			else
			{
				runnerLoggerMock.Verify(l => l.Log(expectedType, expectedMessage, expectedException));
			}
		}

		GrpcLoggerProxy grpcLoggerProxy;
		Mock<IRunnerLogger> runnerLoggerMock;
		Exception testException1;
		Exception testException2;
	}
}
