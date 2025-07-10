using System;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.Dash.ServiceTasks.MessageProcessors;
using Enterprise.Dash.ServiceTasks.ServiceTasks;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.Integration;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Dash.ServiceTasks.Tests.ServiceTasks
{
	[TestedType(typeof(DashMessageProcessingTask))]
	public class DashMessageProcessorTaskTest : TestCaseWithFactory
	{
		public void Test_RunTask_Calls_DashMessageProcessor()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var loggerMock = new Mock<ILogger>();

				var dashMessageProcessorMock = new Mock<DashMessageProcessor>();
				var dashMessageProcessorTaskMock = new Mock<DashMessageProcessingTask>();
				dashMessageProcessorTaskMock.CallBase = true;
				dashMessageProcessorTaskMock.Protected().SetupGet<DashMessageProcessor>("DashMessageProcessor").Returns(dashMessageProcessorMock.Object);

				dashMessageProcessorTaskMock.Object.ServiceLogger = loggerMock.Object;

				// act
				AssertNoExceptionThrown(() => dashMessageProcessorTaskMock.Object.RunTask(CancellationToken.None));

				// assert
				loggerMock.Verify(x => x.Log(LogType.Information, "Task was started successfully."));
				loggerMock.Verify(x => x.Log(LogType.Information, "Task completed"));

				dashMessageProcessorMock.Verify(x => x.ProcessMessages(It.IsAny<CancellationToken>()), Times.Once);
			}
		}

		public void Test_RunTask_DoesNotCall_DashMessageProcessor_When_ParsingIsNotEnabled()
		{
			using (DocManagerRegistry.Instance.SetTemporaryDocParsingRegistryValues(false))
			{
				// arrange
				var loggerMock = new Mock<ILogger>();

				var dashMessageProcessorMock = new Mock<DashMessageProcessor>();
				var dashMessageProcessorTaskMock = new Mock<DashMessageProcessingTask>();
				dashMessageProcessorTaskMock.CallBase = true;
				dashMessageProcessorTaskMock.Protected().SetupGet<DashMessageProcessor>("DashMessageProcessor").Returns(dashMessageProcessorMock.Object);

				dashMessageProcessorTaskMock.Object.ServiceLogger = loggerMock.Object;

				// act
				AssertNoExceptionThrown(() => dashMessageProcessorTaskMock.Object.RunTask(CancellationToken.None));

				// assert
				loggerMock.Verify(x => x.Log(LogType.Warning, "Failed to run service task. Please check if registry for at least one of the parsing types is enabled at System -> DocManager -> Document Ingestion -> Parse Types."));

				dashMessageProcessorMock.Verify(x => x.ProcessMessages(It.IsAny<CancellationToken>()), Times.Never);
			}
		}
	}
}
