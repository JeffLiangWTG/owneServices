using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Dash.Business;
using Enterprise.Dash.Integration.Services;
using Enterprise.Dash.ServiceTasks.MessageProcessors;
using Enterprise.Dash.ServiceTasks.ServiceTasks;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Dash.ServiceTasks.Tests.ServiceTasks
{
	public class ProductCodeMatchingServiceTaskTests : TestCaseWithFactory
	{
		[TestedType(typeof(ProductCodeMatchingServiceTask))]
		public class ProductCodeMatchingServiceTaskTest : ServiceTaskTestCase<ProductCodeMatchingServiceTask>
		{
			protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
			{
				get
				{
					return
					[
						new TaskNudgeInformationForTest(
							EDIMessageSchema.Constants.TableName,
							"DashProductCodeMatching Queue",
							EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.DashDocumentDataProcessing,
							EDIMessageSchema.Constants.EM_MessageType + "=" + WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.ProductCodeMatching,
							EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
							EDIMessageSchema.Constants.EM_IsActive + "=Y",
							EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Internal)
					];
				}
			}

			public void TestRunTask_Calls_DashMessageProcessor()
			{
				using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// arrange
					var factoryMock = new Mock<IFactory>();
					var loggerMock = new Mock<ILogger>();
					var shipamaxService = new Mock<IShipamaxService>();
					var dashGlowService = new Mock<IDashGlowService>();
					var dashErrorReporter = new Mock<IDashErrorReporter>();

					var messageProcessor = new ProductCodeMatchingMessageProcessor(factoryMock.Object, loggerMock.Object, dashGlowService.Object, shipamaxService.Object, dashErrorReporter.Object);
					var dashMessageProcessorTask = new ProductCodeMatchingServiceTask(messageProcessor);

					dashMessageProcessorTask.ServiceLogger = loggerMock.Object;

					// act
					AssertNoExceptionThrown(() => dashMessageProcessorTask.RunTask(CancellationToken.None));

					// assert
					loggerMock.Verify(x => x.Log(LogType.Information, "Task was started successfully."));
					loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."));
					loggerMock.Verify(x => x.Log(LogType.Information, "Total 0 message(s) successfully processed."));
					loggerMock.Verify(x => x.Log(LogType.Information, "Task completed"));
				}
			}
		}
	}
}
