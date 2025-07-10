using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Client.Test
{
	public abstract class PAVEServiceClientTestCase : TransactionedTestCase
	{
		public void TestProcessTransferRules_ShouldLog_WhenNoPKsToProcess()
		{
			GetClient().Process(null, loggerMock.Object);
			var expectedLog = (LogType.Information, $"{ExpectedClassNameInLogs}|No PKs to process");

			AssertEquals(expectedLog, logs.Single());

			logs.Clear();

			GetClient().Process(Array.Empty<Guid>(), loggerMock.Object);

			AssertEquals(expectedLog, logs.Single());
		}

		#region Abstract Methods

		protected abstract IPAVEService GetClient();

		protected abstract string ExpectedClassNameInLogs { get; }

		#endregion

		#region SetUp

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
