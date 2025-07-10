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
	public class ERequestDocumentMessageAction : IMessageAction
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "provider")] //constructor signature used by reflection
		public ERequestDocumentMessageAction(BusinessObjectFactoryProvider provider)
		{
		}

		public bool ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participant)
		{
			participant = new List<ITransactionParticipant>(0);

			try
			{
				var serializer = ZXmlSerializer.New(typeof(Xsd.ERequestDocument));
				var requestDoc = (Xsd.ERequestDocument)SystemMessage.Deserialize(message, serializer);
				new ERequestDocumentProcessor(NotificationLogger.CreateLogger(notifications)).Process(requestDoc);

				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				notifications.AddError(this.GetType().Name + System.Environment.NewLine + "Message: " + message.EM_MessageTextShort + System.Environment.NewLine + ex.ToString());
				return false;
			}
		}

		public void SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
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
	}
}
