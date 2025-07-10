using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

public abstract class EDIFACTMessageProviderBase<TMessageParent> : IEDIFACTMessageProvider
	where TMessageParent : IEDIMessageCollectionProvider
{
	public EDIFACTMessageProviderBase(TMessageParent messageParent)
	{
		this.messageParent = Argument.NotNull(messageParent, nameof(messageParent));
	}
	protected readonly TMessageParent messageParent;

	public abstract IMessageHeaderProvider MessageHeader { get; }

	public EDIMessageCollection Messages => messageParent.Messages;

	public BusinessObjectFactory Factory => messageParent.Factory;
}
