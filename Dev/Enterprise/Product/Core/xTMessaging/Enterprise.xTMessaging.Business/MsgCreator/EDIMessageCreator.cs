using System;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.xTMessaging.Business
{
	public abstract class EDIMessageCreator : IEDIMessageCreator
	{
		protected EDIMessageCreator(EDIInterchange interchange, ILogger logger)
		{
			Interchange = Argument.NotNull(interchange, nameof(interchange));
			Logger = Argument.NotNull(logger, nameof(logger));
		}
		protected EDIInterchange Interchange;
		protected ILogger Logger;

		public ZString CreateEDIMessagesForInterchange(Stream payload, BusinessObjectFactory factory)
		{
			return CreateEDIMessagesForInterchangeCore(payload, factory);
		}

		protected abstract ZString CreateEDIMessagesForInterchangeCore(Stream payload, BusinessObjectFactory factory);

		protected virtual EDIMessage CreateEDIMessage(BusinessObjectFactory factory, Func<ZString> getApplicationCode = null, Func<ZString> getMessageType = null, Func<ZString> getMessageSubType = null)
		{
			var newMessage = (EDIMessage)factory.New(EDIMessageType);
			newMessage.EM_IsTestMessage = false;
			newMessage.EM_GB = Interchange.EI_GB;
			newMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			newMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			newMessage.EM_ApplicationCode = (getApplicationCode != null) ? getApplicationCode() : Interchange.EI_ApplicationCode;
			newMessage.EM_MessageType = (getMessageType != null) ? getMessageType() : Interchange.EI_InterchangeType;
			newMessage.EM_MessageSubType = (getMessageSubType != null) ? getMessageSubType() : Interchange.EI_InterchangeType;
			newMessage.EM_GE = GlbDepartment.CurrentDepartment.PK;
			Interchange.ContainedMessages.Add(newMessage);

			return newMessage;
		}

		public ZString MessageProcessNoteType => MessageProcessNoteTypeCore;
		protected virtual Type EDIMessageType => typeof(EDIMessage);
		protected virtual ZString MessageProcessNoteTypeCore => Res.GetString("f1faf93f-b878-487-957a-51f2bbf2bbfb", "xT Message Process Log");
	}
}
