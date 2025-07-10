using System;
using System.Collections;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal abstract class JXCMessageProcessorBaseTest : TestCaseWithFactory
	{
		protected BusinessObjectCreatedOrUpdatedNotification[] GetBusinessObjectCreatedOrUpdatedNotification(NotificationBuffer notificationBuffer, params Type[] businessObjectTypes)
		{
			ArrayList result = new ArrayList();
			INotification[] notifications = notificationBuffer.GetEventsByType(NotificationSubscriberType.BusinessObjectCreatedOrUpdated);
			foreach (BusinessObjectCreatedOrUpdatedNotification notification in notifications)
			{
				if (businessObjectTypes.Length == 0 || ((IList)businessObjectTypes).Contains(notification.BusinessEntity.GetType()))
				{
					result.Add(notification);
				}
			}

			return (BusinessObjectCreatedOrUpdatedNotification[])result.ToArray(typeof(BusinessObjectCreatedOrUpdatedNotification));
		}

		protected void AssertBusinessObjectCreatedOrUpdatedNotification(BusinessObjectCreatedOrUpdatedNotification notification, BusinessObject expectedBusinessObject)
		{
			AssertEquals("Expected a " + nameof(BusinessObjectCreatedOrUpdatedNotification) + " with the correct business object", expectedBusinessObject.PK, notification.BusinessEntity.PK);
		}
	}
}
