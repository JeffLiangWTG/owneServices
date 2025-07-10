using System;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AR.Manifest.Business
{
	public abstract class BaseMessageSender
	{
		protected BaseMessageSender(AsycudaBill bill)
		{
			this.bill = bill;
		}
		protected readonly AsycudaBill bill;

		BusinessObjectFactory Factory => bill.Factory;

		protected abstract ZString MessageType { get; }

		protected virtual ZString MessageSubType { get; }

		protected abstract ZString MessageText { get; }

		public string SendMessage()
		{
			var result = ZString.Empty;

			var message = CreateEDIMessage();
			if (message != null)
			{
				try
				{
					bill.Messages.Add(message);
					SetStatus();
					Factory.Save();
					result = FormattableString.Invariant($"{ARMessageConstants.MessageSendSuccessful}");
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
					result = FormattableString.Invariant($"{ARMessageConstants.MessageCreateFailure}\n{e.Message}");
					message.Delete();
					RollBackStatus();
				}
			}
			return result;
		}

		EDIMessage CreateEDIMessage()
		{
			var message = Factory.New<ARMessage>();

			message.EM_ApplicationReference = bill.ABL_BillNumber;
			message.EM_IsTestMessage = ARCustomsDataRegistry.Instance.ARTestingSystem.Value;
			message.EM_LinkUniqueID = bill.PK;
			message.EM_MessageText = MessageText;
			message.EM_MessageType = MessageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;

			return message;
		}

		protected abstract void SetStatus();

		protected virtual void RollBackStatus() { }

		protected static ZString BuildMessageText(IXmlMessageBuilder builder)
		{
			return builder.GenerateXmlMessage().GetSerializedString();
		}
	}
}
