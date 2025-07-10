using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.VersionReporting.BatchProcessor;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.BatchProcessor
{
	public class SupportRequestMessageAction : IMessageAction
	{
		public SupportRequestMessageAction(BusinessObjectFactoryProvider factoryProvider)
		{
		}

		bool ExecuteActionCore(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants)
		{
			participants = new List<ITransactionParticipant>(0);
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceRequest));
			var request = (Xsd.CustomerServiceRequest)SystemMessage.Deserialize(message, serializer);
			var processor = CreateProcessor(notifications);
			processor.Process(request, true);
			return true;
		}

		protected virtual ISupportRequestProcessor CreateProcessor(INotifications notifications)
		{
			return new SupportRequestProcessor(NotificationLogger.CreateLogger(notifications));
		}

		#region IMessageAction implementation

		bool IMessageAction.ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participants)
		{
			try
			{
				return ExecuteActionCore(message, notifications, out participants);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.AddError(this.GetType().Name + System.Environment.NewLine + "Message: " + message.EM_MessageTextShort + System.Environment.NewLine + ex.ToString());
				participants = new List<ITransactionParticipant>(0);
				return false;
			}
		}

		void IMessageAction.SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
		{
			if (!onSuccess)
			{
				try
				{
					var email = new EmailDef();
					email.Subject = subject;
					email.Body = body;
					Env.OutgoingMailManager.CreateAndSave(email, EDIDataRegistry.Instance.InternalNotificationGroup.Value, GroupSourceLocator.GetFromRegistryItem(EDIDataRegistry.Instance.InternalNotificationGroup));
				}
				catch (EmailSendFailedException)
				{
				}
			}
		}

		#endregion
	}
}
