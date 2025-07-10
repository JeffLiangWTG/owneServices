using System;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public abstract class MessageDataObjectBase
	{
		protected MessageDataObjectBase(ILEDIMessage message)
		{
			EDIMessage = message;
		}

		protected readonly ILEDIMessage EDIMessage;
		public BusinessObjectFactory Factory => EDIMessage.Factory;

		public ILEDIMessagePrettierBase Prettier => prettier ??= CreatePrettierCore();
		ILEDIMessagePrettierBase prettier;

		protected abstract ILEDIMessagePrettierBase CreatePrettierCore();

		public bool CanSetMessageInterpretation => CanSetMessageInterpretationCore;

		protected virtual bool CanSetMessageInterpretationCore => false;

		public ZString MessageText => EDIMessage.EM_MessageText;
	}

	public abstract class MessageDataObject<TMessage> : MessageDataObjectBase
		where TMessage : class
	{
		protected MessageDataObject(ILEDIMessage message) : base(message)
		{
		}

		public TMessage MessageData => messageData ??= GetMessageData(EDIMessage.EM_MessageText);
		TMessage messageData;

		protected virtual TMessage GetMessageData(ZString messageText) =>
			new MessageDeserializer<TMessage>().DeserializeMessage(messageText);
	}

	public class MessageDeserializer<TMessage>
		where TMessage : class
	{
		public TMessage DeserializeMessage(ZString messageText)
		{
			var hasDefaultNamespace = Extensions.HasDefaultNamespace<TMessage>(messageText);
			return hasDefaultNamespace
				? Extensions.Deserialize<TMessage>(messageText)
				: Extensions.Deserialize<TMessage>(messageText, GetElementName());
		}

		static string GetElementName()
		{
			var type = typeof(TMessage);
			var xmlRootAttribute = (XmlRootAttribute)Attribute.GetCustomAttribute(type, typeof(XmlRootAttribute));
			return xmlRootAttribute?.ElementName;
		}
	}
}
