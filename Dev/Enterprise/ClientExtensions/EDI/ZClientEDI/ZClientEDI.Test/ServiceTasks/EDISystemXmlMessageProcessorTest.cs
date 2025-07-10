using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.LogsReporting.BatchProcessor;
using Enterprise.Client.EDI.VersionReporting.BatchProcessor;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Client.EDI.ServiceTasks.Tests
{
	class EDISystemXmlMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEnsureHostedServiceBusinessObjectBindingAttributesAdded()
		{
			var matchCount = AssemblyMetaDataReader.GetAttributes<HostedServiceBusinessObjectBindingAttribute>().Count(x => x.ServiceTaskCode == "SYS" && x.Table == EDIMessageSchema.Constants.TableName && x.ClientSpecificCode == Clients.EDI);
			AssertEquals(5, matchCount);
		}

		public void TestGetMessageAction()
		{
			var processor = new EDISystemXmlMessageProcessorForTest();
			var action = processor.GetMessageAction_Exposed(SystemMessage.MessageType, SystemMessageList.Codes.CustomerServiceRequest);
			AssertNull("action", action);
			action = processor.GetMessageAction_Exposed(SystemMessage.MessageType, SystemMessageList.Codes.CurrentVersionReport);
			AssertEquals("action type", typeof(CurrentVersionReportMessageAction), action.GetType());
			action = processor.GetMessageAction_Exposed(SystemMessage.MessageType, SystemMessageList.Codes.DeliveredVersionReport);
			AssertEquals("action type", typeof(DeliveredVersionReportMessageAction), action.GetType());
			action = processor.GetMessageAction_Exposed(SystemMessage.MessageType, SystemMessageList.Codes.LogsReport);
			AssertEquals("action type", typeof(LogsReportMessageAction), action.GetType());
			action = processor.GetMessageAction_Exposed(SystemMessage.MessageType, SystemMessageList.Codes.ERequestDocument);
			AssertEquals("action type", typeof(ERequestDocumentMessageAction), action.GetType());
			action = processor.GetMessageAction_Exposed(SystemMessage.MessageType, SystemMessageList.Codes.UserAccountReport);
			AssertEquals("action type", typeof(UserAccountReportMessageAction), action.GetType());
		}
	}
}
