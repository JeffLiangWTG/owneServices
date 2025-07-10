using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.ServiceTasks.Tests
{
	[TestedType(typeof(CustomerMessageServiceTask))]
	class CustomerMessageServiceTaskTest : ServiceTaskTestCase<CustomerMessageServiceTask>
	{
		[ExpectNoExceptions]
		public void TestServiceTaskCanRunInAnyBranch()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.SYS;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = SystemMessageList.Codes.CustomerServiceRequest;
			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			Factory.Save();

			var getPKMethod = typeof(CustomerMessageProcessor).GetMethod("GetEDIMessagePKs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			AssertNotNull(getPKMethod);
			var pks = getPKMethod.Invoke(new CustomerMessageProcessor(), Array.Empty<object>()) as List<ZGuid>;
			AssertEquals(1, pks.Count);
			AssertEquals(message.PK, pks[0]);

			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
			var serviceTask = new CustomerMessageServiceTask();
			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;
			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(serviceTask.GetType().Name, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		public void TestGetMessageAction()
		{
			var processor = new CustomerMessageProcessorForTest();

			var action = processor.GetMessageAction_Exposed(SystemMessage.MessageType, SystemMessageList.Codes.CustomerServiceRequest);
			AssertEquals("action type", typeof(SupportRequestMessageAction), action.GetType());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"Customer Service Request",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.CustomerServiceRequest),
				};
			}
		}
	}

	class CustomerMessageProcessorForTest : CustomerMessageProcessor
	{
		public IMessageAction GetMessageAction_Exposed(ZString messageType, ZString messageSubType)
		{
			return base.GetMessageAction(messageType, messageSubType, null);
		}
	}
}
