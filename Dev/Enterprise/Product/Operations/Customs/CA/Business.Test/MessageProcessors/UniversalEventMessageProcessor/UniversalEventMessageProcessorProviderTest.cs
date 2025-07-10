using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class UniversalEventMessageProcessorProviderTest : TestCaseWithFactory
	{
		public void TestGetProcessor()
		{
			AssertProcessorType(AutoEvents.CustomsManifestStatusCode, typeof(CustomsManifestStatusMessageProcessor));
			AssertProcessorType(AutoEvents.MessageAcceptedCode, typeof(MessageAcceptedMessageProcessor));
			AssertProcessorType(AutoEvents.MessageRejectedCode, typeof(MessageRejectedMessageProcessor));
			AssertProcessorType(AutoEvents.MessageStatusChangeCode, typeof(MessageStatusChangeMessageProcessor));
			AssertProcessorType(AutoEvents.MessageValidationFailedCode, typeof(MessageValidationFailedMessageProcessor));
			AssertProcessorType(AutoEvents.MessageValidationPassedCode, typeof(MessageValidationPassedMessageProcessor));
			AssertProcessorType(AutoEvents.MiscellaneousEventCode, typeof(MiscellaneousEventMessageProcessor));
		}

		void AssertProcessorType(ZString eventType, Type type)
		{
			message.EM_MessageText = ZString.Format(@"
<UniversalEvent>
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Type>CAeManifestStatusNotice</Type>
					<Key>12345000000011</Key>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventType>{0}</EventType>
	</Event>
</UniversalEvent>
", eventType);
			var universalEvent = message.GetEM_MessageTextReader().Parse<UniversalEvent>();
			var processor = UniversalEventMessageProcessorProvider.GetProcessor(new XmlSessionTracker(new ServiceTaskLogForTesting()), universalEvent, message, entryHeader);
			AssertType("Processor Type", type, processor);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader.CH_BGMReference = "12345000000011";
			message = Factory.New<UniversalEventMessage>();
			message.EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			message.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message.EM_Status = XmlEDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		}

		CusEntryHeader entryHeader;
		UniversalEventMessage message;
	}
}
