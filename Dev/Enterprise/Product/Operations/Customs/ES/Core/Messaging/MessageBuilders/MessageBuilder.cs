using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public abstract class MessageBuilder<TProvider> : IMessageBuilderBase
		where TProvider : IESEDIMessageCollectionProvider
	{
		protected MessageBuilder(TProvider provider, ZString messageType, ZString messageSubType)
		{
			Provider = Argument.NotNull(provider, nameof(provider));
			this.provider = provider;
			MessageType = messageType;
			MessageSubType = messageSubType;
		}
		protected readonly TProvider provider;

		public ZString GetSignedMessageText() => SignMessageText(UnsignedMessageText);

		protected abstract ZString SignMessageText(ZString messageText);

		public virtual ZString UnsignedMessageText { get { return unsignedMessageText; } set { unsignedMessageText = value; } }
		protected ZString unsignedMessageText;

		public ZString MessageType { get; }

		public ZString MessageSubType { get; }

		public IESEDIMessageCollectionProvider Provider { get; }
	}
}
