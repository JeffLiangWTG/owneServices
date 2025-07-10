using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Test
{
	public abstract class SimplePAVEServiceTestCase : TransactionedTestCase
	{
		#region VerifyProcessTransferRulesParameters

		public void TestProcessTransferRules_ShouldNotRunProcessorAndLog_WhenBufferManagement_IsDisabled()
		{
			BMSTestHelper.DisableBMSInRegistry();

			var service = GetService();

			CallServiceMethod(service, null, loggerMock.Object);

			AssertCollectionContains((LogType.Information, $"Attempt to run {ExpectedClassNameInLogs}.{ExpectedMethodNameInLogs} with Registry bufferManagement disabled"), logs);
		}

		public void TestProcessTransferRules_ShouldNotRunProcessorAndLog_WhenNoWorkflowPKs()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var service = GetService();

			CallServiceMethod(service, null, loggerMock.Object);
			AssertCollectionContains((LogType.Information, $"Attempt to run {ExpectedClassNameInLogs}.{ExpectedMethodNameInLogs} without processed item PKs"), logs);

			logs.Clear();

			CallServiceMethod(service, Array.Empty<Guid>(), loggerMock.Object);
			AssertCollectionContains((LogType.Information, $"Attempt to run {ExpectedClassNameInLogs}.{ExpectedMethodNameInLogs} without processed item PKs"), logs);
		}

		#endregion

		public void TestShouldLog_WhenThrowsException()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var service = new ServiceThatThrowsException
			{
				ExceptionToThrow = new Exception("Oh no something went wrong!")
			};

			var dummyTransferablePKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			AssertNoExceptionThrown("Should not throw exception", () => ((IPAVEService)service).Process(dummyTransferablePKs, loggerMock.Object));

			Assert(logs.Any());
			AssertEquals((LogType.Information, $"Error when call {nameof(ServiceThatThrowsException)}.{nameof(TestShouldLog_WhenThrowsException)}, transferablePKs: {dummyTransferablePKs[0]},{dummyTransferablePKs[1]} Exception: Oh no something went wrong!"), logs.Last());
		}

		#region Abstract Methods

		protected abstract IPAVEService GetService();

		protected abstract void CallServiceMethod(IPAVEService service, IReadOnlyCollection<Guid> processedPKs, ILogger logger);

		protected abstract string ExpectedClassNameInLogs { get; }

		protected abstract string ExpectedMethodNameInLogs { get; }

		#endregion

		#region Test Classes

		class ServiceThatThrowsException : PAVEService
		{
			public Exception ExceptionToThrow { get; set; }

			protected override void ProcessCore(IEnumerable<Guid> processedPKs, ILogger logger)
			{
				throw ExceptionToThrow;
			}
		}

		#endregion

		#region Setup

		protected Mock<ILogger> loggerMock;
		protected List<(LogType type, string message)> logs;

		protected override void SetUp()
		{
			base.SetUp();

			logs = new List<(LogType, string)>();
			loggerMock = new Mock<ILogger>();
			loggerMock.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType type, string message) => logs.Add((type, message)));
		}

		#endregion
	}
}
