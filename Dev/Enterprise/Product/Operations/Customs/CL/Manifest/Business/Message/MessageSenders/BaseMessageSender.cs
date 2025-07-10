using System;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CL.Manifest.Business
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

		protected abstract ZString MessageSubType { get; }

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
					result = FormattableString.Invariant($"{CLMessageConstants.MessageSendSuccessful}");
				}
				catch (ZSaveException e)
				{
					ZExceptionReporting.HandleSaveException(e);
					result = FormattableString.Invariant($"{CLMessageConstants.MessageCreateFailure}{System.Environment.NewLine}{e.Message}");
					message.Delete();
					RollBackStatus();
				}
			}
			return result;
		}

		EDIMessage CreateEDIMessage()
		{
			var message = Factory.New<CLMessage>();

			message.EM_ApplicationCode = ApplicationCodeList.Codes.CLCustoms;
			message.EM_ApplicationReference = bill.ABL_BillNumber;
			message.EM_IsTestMessage = CLCustomsDataRegistry.Instance.CLTestingSystem.Value;
			message.EM_LinkUniqueID = bill.PK;
			message.EM_MessageText = MessageText;
			message.EM_MessageType = MessageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Queued;

			return message;
		}

		protected virtual void SetStatus() { }

		protected virtual void RollBackStatus() { }

		protected static ZString BuildMessageText(IXmlMessageBuilder builder)
		{
			return builder.GenerateXmlMessage().GetSerializedString();
		}
	}
}
