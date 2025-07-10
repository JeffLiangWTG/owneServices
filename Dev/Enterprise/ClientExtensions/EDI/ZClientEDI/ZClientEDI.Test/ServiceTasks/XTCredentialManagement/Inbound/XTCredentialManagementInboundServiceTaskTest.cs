using System.Collections.Generic;
using System.Text;
using Enterprise.Client.EDI.ServiceTasks.XTCredentialManagement.Inbound;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace ZClientEDI.Test.ServiceTasks.XTCredentialManagement.Inbound
{
	[TestedType(typeof(XTCredentialManagementInboundServiceTask))]
	class XTCredentialManagementInboundServiceTaskTest : ServiceTaskTestCase<XTCredentialManagementInboundServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new TaskNudgeInformationForTest[]
			{
				new TaskNudgeInformationForTest(
					EDIInterchangeSchema.Constants.TableName,
					XTCredentialManagementInboundServiceTask.ServiceTaskDescription,
					EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
					EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
					EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
					EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.XMS)
			};

		public void TestInterchangeStatusUpdateForSuccess()
		{
			var mockLogger = new Mock<ILogger>();
			var log = new StringBuilder();
			mockLogger.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback((LogType logType, string message) =>
			{
				log.AppendLine(message);
			});

			XTCredentialManagementTestUtils.CreateRequestAndAckResponseInterchange(Factory, out var request, out var response);
			var serviceTask = new XTCredentialManagementInboundServiceTask() { Factory = Factory };
			serviceTask.Logger = mockLogger.Object;
			Assert($"Expected {EDIInterchangeStatusList.Codes.Sent} but got {request.EI_Status}", request.EI_Status == EDIInterchangeStatusList.Codes.Sent);
			Assert($"Expected {EDIInterchangeStatusList.Codes.Queued} but got {response.EI_Status}", response.EI_Status == EDIInterchangeStatusList.Codes.Queued);
			InitialiseAndRunTaskSchedule(serviceTask);
			Assert($"Expected {EDIInterchangeStatusList.Codes.Sent} but got {request.EI_Status}", request.EI_Status == EDIInterchangeStatusList.Codes.Sent);
			Assert($"Expected {EDIInterchangeStatusList.Codes.Received} but got {response.EI_Status}", response.EI_Status == EDIInterchangeStatusList.Codes.Received);
			AssertContains("XT Credential Change Response Service Task Started\r\nXT Credential Change Response Service Task Finished\r\n", log.ToString());
		}

		public void TestInterchangeStatusUpdateForFailure()
		{
			XTCredentialManagementTestUtils.CreateRequestAndNackResponseInterchange(Factory, out var request, out var response);
			var serviceTask = new XTCredentialManagementInboundServiceTask() { Factory = Factory };
			Assert($"Expected {EDIInterchangeStatusList.Codes.Sent} but got {request.EI_Status}", request.EI_Status == EDIInterchangeStatusList.Codes.Sent);
			Assert($"Expected {EDIInterchangeStatusList.Codes.Queued} but got {response.EI_Status}", response.EI_Status == EDIInterchangeStatusList.Codes.Queued);
			InitialiseAndRunTaskSchedule(serviceTask);
			Assert($"Expected {EDIInterchangeStatusList.Codes.Queued} but got {request.EI_Status}", request.EI_Status == EDIInterchangeStatusList.Codes.Queued);
			Assert($"Expected {EDIInterchangeStatusList.Codes.Received} but got {response.EI_Status}", response.EI_Status == EDIInterchangeStatusList.Codes.Received);
		}
	}
}

