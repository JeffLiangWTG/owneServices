using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Testing
{
	class StandardXmlServiceTaskTestBase : TestCaseWithFactory
	{
		protected NotificationBuffer ProcessMessages(bool isVerboseLoggingForBatchReprocessing = true, bool isVerboseLoggingForMemory = false, bool returnNullMessageAction = false, Exception messageActionTrowException = null)
		{
			var processor = new MockStandardXMLMessageProcessor(returnNullMessageAction, messageActionTrowException);
			processor.SetIsVerboseLoggingForBatchReprocessingProblemForTesting(isVerboseLoggingForBatchReprocessing);
			processor.SetIsVerboseLoggingForMemoryProblemForTesting(isVerboseLoggingForMemory);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			Factory.ReloadAll<EDIMessage>();
			return notifications;
		}

		protected IEDIMessage GetQueuedXMSShipmentMessage(string filePath)
		{
			var message = GetQueuedXMSDataMessage(filePath);
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Shipments;
			return message;
		}

		protected IEDIMessage GetQueuedXMSConsolMessage(string filePath)
		{
			var message = GetQueuedXMSDataMessage(filePath);
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Consols;
			return message;
		}

		protected IEDIMessage GetQueuedXMSAgencyMessage(string filePath)
		{
			var message = GetQueuedXMSDataMessage(filePath);
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.AgencyBillsOfLading;
			return message;
		}

		protected IEDIMessage GetQueuedXMSOrderMessage(string filePath)
		{
			var message = GetQueuedXMSDataMessage(filePath);
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Orders;
			return message;
		}

		protected IEDIMessage GetQueuedXMSDataMessage(string filePath)
		{
			var message = Factory.New<IEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.XMS;
			message.EM_MessageType = EDIMessageTypeList.Codes.XMS;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_MessageText = GetFileText(filePath);
			return message;
		}

		protected void AssertLogs(Regex regExp, string actual)
		{
			AssertEquals(String.Format("Actual text: \r\n '{0}' \r\n\r\n doesn't match Regular Expression  \r\n\r\n '{1}' \r\n", actual, regExp.ToString()), true, regExp.IsMatch(actual));
		}

		protected string GetFileText(string filePath)
		{
			return GetFileResourceString("TestFiles." + filePath);
		}

		static Stream GetFileResource(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			return assembly.GetManifestResourceStream(assembly.GetName().Name + "." + resourceName);
		}

		static string GetFileResourceString(string resourceName)
		{
			return new StreamReader(GetFileResource(resourceName)).ReadToEnd();
		}
	}
}
