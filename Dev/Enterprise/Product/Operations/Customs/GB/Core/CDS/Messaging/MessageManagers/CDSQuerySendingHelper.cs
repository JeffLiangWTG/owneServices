using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XmlWriting;
using Enterprise.ZArchitecture.Schema;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageManagers
{
	public static class CDSQuerySendingHelper
	{
		public static bool Deliver(BusinessObjectFactory factory, UniversalEvent universalEvent, BusinessObject parentBO, ZString deliveryDestination, MessageSendingNotificationCollection notificationCollection, EDIMessage message)
		{
			var isDeliverySuccessful = false;

			if (universalEvent != null)
			{
				var delNotifications = new DeliveryNotifications(notificationCollection);

				var context = new DeliveryContext(factory)
				{
					ParentInfo = parentBO != null ? EntityInfo.New(parentBO) : EntityInfo.Empty,
					ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging,
					MessageTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
					MessageSubTypeCode = EDIMessageSubTypeList.Codes.XmlUniversalEvent,
					Notifications = delNotifications
				};

				var delivery = new CDSQueryEHubDelivery(universalEvent);
				var mode = new NonPersistentEDICommunicationMode
				{
					EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML,
					EK_Destination = deliveryDestination
				};

				Func<IEDIMessage> getMessageFunc = message != null ? new Func<IEDIMessage>(() => message) : null;

				var result = delivery.Deliver(context, mode, new DeliveryStreamWrapperUXML(context.ParentInfo, universalEvent, new XmlWriter(), UniversalXmlInfo.Namespace_2011_11), getMessageFunc);

				if (result.Succeeded)
				{
					SetOutgoingUniversalEventInterpretation(delivery.InterchangeCreated.PK, factory);
					isDeliverySuccessful = true;
				}
			}

			return isDeliverySuccessful;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
		static void SetOutgoingUniversalEventInterpretation(ZGuid interchangePK, BusinessObjectFactory factory)
		{
			var query = new ZQuery(EDIMessageSchema.EM_EI, interchangePK);
			query.FetchOnlyFromLocalCache = true;
			var msg = factory.LoadTop1<EDIMessage>(query);

			if (msg != null)
			{
				var accessGetterToConvertXmlToHtml = msg.EM_MessageInterpretation;
				factory.Save();
			}
		}

		class DeliveryNotifications : INotifications
		{
			public DeliveryNotifications(MessageSendingNotificationCollection notificationCollection)
			{
				this.notificationCollection = notificationCollection;
			}

			public void Add(INotification notification)
			{
				if (notification.Type.IsFatal)
				{
					notificationCollection.AddError(notification.Message);
				}
				else
				{
					notificationCollection.AddInformation(notification.Message);
				}
			}

			readonly MessageSendingNotificationCollection notificationCollection;
		}
	}
}
