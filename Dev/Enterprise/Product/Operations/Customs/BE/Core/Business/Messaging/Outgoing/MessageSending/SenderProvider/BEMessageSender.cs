using System.IO;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business;

public abstract class BEMessageSender<TEdiMessage, TProvider> : MessageSender
	where TEdiMessage : BEMessage
	where TProvider : IMessageHeader
{
	protected BEMessageSender(BusinessObject parent, string messageName, bool sendWithErrors, bool isTestMessage)
	{
		MessageObject = CargoWise.Common.Argument.NotNull(parent, nameof(parent));
		SendWithErrors = sendWithErrors;
		MessageSubType = messageName;
		this.isTestMessage = isTestMessage;
	}

	public override object DataProvider
	{
		get
		{
			if (dataProvider == null)
			{
				dataProvider = GetDataProvider(MessageObject);
			}
			return dataProvider;
		}
	}
	TProvider dataProvider;

	protected abstract TProvider GetDataProvider(BusinessObject messageObject);

	Stream MessageContent => GetProducer((TProvider)DataProvider).GenerateXmlMessage().GetSerializedStream();

	protected abstract IXmlMessageBuilder GetProducer(TProvider dataProvider);

	protected readonly bool SendWithErrors;

	public override bool IsTestMessage => isTestMessage;
	readonly bool isTestMessage;

	public sealed override void Send()
	{
		PreSend();

		var newMessage = MessageObject.Factory.New<TEdiMessage>();
		newMessage.EM_LinkedObject = MessageObject;
		newMessage.EM_MessageSubType = MessageSubType;
		newMessage.SetEM_MessageTextOrDataSource(MessageContent);
		newMessage.EM_SendWithMessageErrors = SendWithErrors;
		newMessage.EM_IsTestMessage = IsTestMessage;

		SendCore(newMessage);
		PostSend(newMessage);
	}

	void PreSend()
	{
		PreSendCore();
	}

	void PostSend(BEMessage newMessage)
	{
		PostSendCore(newMessage);
	}

	protected virtual void SendCore(BEMessage newMessage)
	{
	}

	protected virtual void PreSendCore()
	{
	}

	protected virtual void PostSendCore(BEMessage newMessage)
	{
	}
}
