using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class StaffReportRequestMessageActionTest : TestCaseWithFactory
	{
		public void TestExecuteAction()
		{
			SystemDataRegistry.Instance.IsFirstTimeSendingStaffReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			string requestXml = "<StaffReportRequest />";
			var interchange = SystemMessage.CreateInterchange(Factory, requestXml);
			var message = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			Factory.Save();

			IMessageAction action = new StaffReportRequestMessageAction(new BusinessObjectFactoryProvider(Factory));
			var logger = new NotificationBuffer();
			action.ExecuteAction(message, logger, out List<ITransactionParticipant> participants);

			AssertEquals("Flag should have been reset", true, SystemDataRegistry.Instance.IsFirstTimeSendingStaffReport.Value);
		}
	}
}
