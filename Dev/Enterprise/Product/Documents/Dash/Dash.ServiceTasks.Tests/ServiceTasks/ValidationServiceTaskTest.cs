using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Dash.Business;
using Enterprise.Dash.Integration;
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
	[TestedType(typeof(ValidationServiceTask))]
	public class ValidationServiceTaskTest : ServiceTaskTestCase<ValidationServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return
				[
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"Dash Validation Queue",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.DashDocumentDataProcessing,
						EDIMessageSchema.Constants.EM_MessageType + "=" + WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.Validation,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Internal)
				];
			}
		}

		public void Test_RunTask_Calls_DashMessageProcessor()
		{
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				// arrange
				var factoryMock = new Mock<IFactory>();
				var loggerMock = new Mock<ILogger>();
				var shipamaxServiceMock = new Mock<IShipamaxService>();
				var dashErrorReporter = new Mock<IDashErrorReporter>();
				var validatorMock = new Mock<IValidator<DashCommercialInvoice>>();

				var messageProcessor = new ValidationProcessor(factoryMock.Object, loggerMock.Object, shipamaxServiceMock.Object, dashErrorReporter.Object, validatorMock.Object);
				var dashMessageProcessorTask = new ValidationServiceTask(messageProcessor);

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
