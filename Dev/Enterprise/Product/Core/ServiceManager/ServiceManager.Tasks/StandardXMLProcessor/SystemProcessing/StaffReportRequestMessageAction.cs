using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	class StaffReportRequestMessageAction : IMessageAction
	{
		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "factoryProvider")]
		public StaffReportRequestMessageAction(BusinessObjectFactoryProvider factoryProvider)
		{
		}

		bool IMessageAction.ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants)
		{
			participants = new List<ITransactionParticipant>(0);

			try
			{
				using (var stringReader = new StringReader(message.EM_MessageText))
				{
					var reader = new XmlTextReader(stringReader);
					reader.Read();
					reader.ReadStartElement(SystemMessageList.Descriptions.StaffReportRequest);

					SystemDataRegistry.Instance.IsFirstTimeSendingStaffReport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					notifications.Add(new InfoNotification("Staff report sent flag has been reset"));
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.AddWarning("Invalid Staff Report Request");
			}

			return true;
		}

		void IMessageAction.SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
		{
		}
	}
}
