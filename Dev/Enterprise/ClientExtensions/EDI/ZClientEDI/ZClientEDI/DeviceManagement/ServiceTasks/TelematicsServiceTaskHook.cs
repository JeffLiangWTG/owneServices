using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using Enterprise.Telematics.ServiceTasks.MessageTypeProcessors;

namespace Enterprise.Client.EDI.DeviceManagement.ServiceTasks
{
	class TelematicsServiceTaskHook
	{
		public static void Hook()
		{
			ProtobufMessageTypeProcessor.SetClientExtensionHook(new IMessageProcessor[]
			{
				new C2WDeviceAssignedToUserNotificationMessageProcessor(),
				new C2WDeviceRegistrationRequestMessageProcessor(),
				new C2WDeviceDeregistrationRequestMessageProcessor()
			});
		}
	}
}
