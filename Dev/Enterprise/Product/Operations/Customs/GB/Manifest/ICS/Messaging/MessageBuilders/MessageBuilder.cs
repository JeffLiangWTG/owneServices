using CargoWise.Types;

namespace Enterprise.Customs.GB.ICS.Messaging
{
	public interface IMessageBuilder
	{
		ZString Build();
	}

	public abstract class MessageBuilder : IMessageBuilder
	{
		public ZString Build()
		{
			var messageObject = GetPopulatedMessageObject();
			var message = GenerateMessage(messageObject);
			return message;
		}

		protected abstract object GetPopulatedMessageObject();

		protected ZString GenerateMessage(object messageObject)
		{
			if (messageObject != null)
			{
				var type = messageObject.GetType();
#if NET48
				if (type.IsSerializable)
				{
					var result = Enterprise.Customs.GB.Business.SerializationHelper.Serialize(type, messageObject);
					return CargoWise.Customs.Shared.MessageContracts.XmlMessageHelper.RemoveEmptyXmlElements(result);
				}
#else
				// No need to check for serializability in .NET Core and later, as serialization is handled differently.
				var result = Enterprise.Customs.GB.Business.SerializationHelper.Serialize(type, messageObject);
				return CargoWise.Customs.Shared.MessageContracts.XmlMessageHelper.RemoveEmptyXmlElements(result);
#endif
			}

			return ZString.Empty;
		}
	}
}
