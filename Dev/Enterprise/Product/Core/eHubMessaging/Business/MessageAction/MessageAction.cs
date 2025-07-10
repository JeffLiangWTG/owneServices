using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.eHubMessaging.Business
{
	public abstract class MessageAction : IMessageAction
	{
		protected MessageAction(BusinessObjectFactoryProvider factoryProvider)
		{
			FactoryProvider = factoryProvider;
		}

		public static IMessageAction New(BusinessObjectFactoryProvider factoryProvider, ZString messageType, ZString messageSubType)
		{
			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.AgencyBillsOfLading))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("AgencyBillOfLadingMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.Consols))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("ForwardingConsolMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.ContainerMovements))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("ContainerMovementMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.Events))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("EventMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.FinancialTransactions))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("FinancialInvoiceMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.NettingClearingJournals))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("NettingClearingJournalMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.BankStatements))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("BankStatementMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.Orders))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("OrderMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.Products))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("ProductMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.Schedules))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("ScheduleMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.Shipments))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("ForwardingShipmentMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.ShipmentBookings))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("BookingMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.ISFs))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("ISFMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.WhsDockets))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("WhsDocketMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.Brokerage))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("DeclarationMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.Invoices))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("InvoiceMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.LocalCartageBooking))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("CartageBookingMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.LocalCartageStatus))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("CartageStatusMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.CFSLoadList))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("CFSLoadListMessageAction"), factoryProvider);
			}

			if (messageType.Equals(EDIMessageTypeList.Codes.XMS) && messageSubType.Equals(EDIMessageSubTypeXMLElementList.Codes.Organizations))
			{
				return (IMessageAction)Activator.CreateInstance(ObjectFactory.GetType("OrganizationMessageAction"), factoryProvider);
			}

			return null;
		}

		#region IMessageAction

		public bool ExecuteAction(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participant)
		{
			try
			{
				return ExecuteActionCore(message, notifications, out participant);
			}
			catch (Exception exception) when (!exception.IsCriticalException() && !exception.IsOutOfDiskSpaceException() && !exception.IsUnableToCreateTempFileException())
			{
				var errorDetails = Res.GetString("b6ddd226-da55-461c-8962-d6b1a437178a", "Could not execute Message Action.\r\n\r\n{0}", exception.ToString());
				notifications.Notify(new ErrorNotification(ErrorType.Error, errorDetails));
				participant = new List<ITransactionParticipant>();
				return false;
			}
		}

		public void SendNotificationEmail(ZString subject, ZString body, INotifications notifications, bool onSuccess)
		{
			if (!onSuccess)
			{
				SendNotificationEmail(subject, body, notifications);
			}
			else if (IsEmailToBeSentOnSuccess)
			{
				SendNotificationEmail(subject, body, notifications);
			}
		}

		#endregion

		void SendNotificationEmail(ZString subject, ZString body, INotifications notifications)
		{
			if (NotificationGroup.IsValidForOutgoingMail(FactoryProvider.Current))
			{
				var email = new EmailDef
				{
					Subject = subject,
					Body = body
				};
				try
				{
					CreateEmail(email);
				}
				catch (EmailSendFailedException ex)
				{
					notifications.Notify(new WarningNotification(Res.GetString("3f973f38-3ab5-4b2a-afda-e39f530ff368", "Notification could not be sent: {0}", ex.Message)));
				}
			}
			else
			{
				notifications.Notify(new WarningNotification(Res.GetString("8626ed5e-b900-489d-95be-f47804608822", "Notification group is not specified or invalid. Please check '{0}'", NotificationGroupRegistryPath)));
			}
		}

		internal virtual void CreateEmail(EmailDef email)
		{
			Env.OutgoingMailManager.Create(FactoryProvider.Current, email, NotificationGroup.PK.ToGuid(), GroupSourceLocator.GetFromGroup(NotificationGroup));
		}

		bool IsEmailToBeSentOnSuccess
		{
			get { return !SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.Value; }
		}

		protected internal abstract bool ExecuteActionCore(EDIMessage message, INotifications notifications, out List<ITransactionParticipant> participant);
		protected internal abstract IGlbGroup NotificationGroup { get; }
		protected internal abstract ZString NotificationGroupRegistryPath { get; }
		protected internal readonly BusinessObjectFactoryProvider FactoryProvider;
	}
}