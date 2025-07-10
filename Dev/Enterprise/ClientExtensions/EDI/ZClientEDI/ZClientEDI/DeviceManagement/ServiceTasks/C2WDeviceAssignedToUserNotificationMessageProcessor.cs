using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.ServiceTasks
{
	class C2WDeviceAssignedToUserNotificationMessageProcessor : IMessageProcessor
	{
		#region IMessageProcessor

		public EHubMessageType MessageType
		{
			get { return EHubMessageType.C2WDeviceAssignedToUserNotification; }
		}

		public void Process(BusinessObjectFactory factory, string from, EHubMessageContainer messageContainer)
		{
			var message = messageContainer.GetInternalMessage<C2WDeviceAssignedToUserNotificationMessage>();

			var query = new ZQuery(DmgDeviceHeaderSchema.CDH_Identifier, SQLComparisonOperator.Equal, message.device_friendly_identifier);
			var device = factory.Load<ClientDeviceHeader>(query).SingleOrDefault();
			if (device == null)
			{
				return;
			}

			if (!string.IsNullOrEmpty(message.parent_code) || !string.IsNullOrEmpty(message.parent_description))
			{
				device.CDH_ClientParentID = string.Format("{0} ({1})", message.parent_description, message.parent_code);
			}
			else
			{
				device.CDH_ClientParentID = string.Empty;
			}
			device.CDH_ClientParentType = message.parent_type;
		}

		#endregion
	}
}
