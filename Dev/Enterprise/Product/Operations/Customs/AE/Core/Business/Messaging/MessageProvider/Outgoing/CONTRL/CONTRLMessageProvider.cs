using CargoWise.EntityFramework;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AE.Business;

public sealed class CONTRLMessageProvider : EDIFACTMessageProviderBase<IEDIMessageCollectionProvider>, ICONTRLMessageProvider
{
	public CONTRLMessageProvider(EDIMessage cUSRESMessage, bool isCUSRESMessageParsed) : base(GetEDIMessageCollectionProvider(cUSRESMessage))
	{
		this.cUSRESMessage = cUSRESMessage;
		this.isCUSRESMessageParsed = isCUSRESMessageParsed;
	}

	readonly EDIMessage cUSRESMessage;

	readonly bool isCUSRESMessageParsed;

	public override IMessageHeaderProvider MessageHeader => messageHeaderProvider ??= new CONTRLMessageHeaderProvider();
	IMessageHeaderProvider messageHeaderProvider;

	public ActionCodedList ActionCoded => actionCoded ??= GetActionCoded();
	ActionCodedList actionCoded;

	ActionCodedList GetActionCoded()
	{
		if (cUSRESMessage.EM_LinkedObject is IMessageAttachee)
		{
			return ActionCodedList.InterchangeReceived;
		}
		else if (isCUSRESMessageParsed)
		{
			return ActionCodedList.ThisLevelAcknowledgedAndAllLowerLevelsAcknowledgedIfNotExplicitlyRejected;
		}
		else
		{
			return ActionCodedList.ThisLevelAndAllLowerLevelsRejected;
		}
	}

	public string InterchangeControlReference => interchangeControlReference ??= GetInterchangeControlReference();
	string interchangeControlReference;

	string GetInterchangeControlReference() => new NAICInterchangeUnpacker().RetrieveInterchangeHeader(cUSRESMessage.Interchange).InterchangeControlReference;

	public EDIMessage RequestMessage => cUSRESMessage;

	public AEEDIMessage AddNewEDIMessage() => (AEEDIMessage)Messages?.AddNew(typeof(AEEDIMessage)) ?? Factory.New<AEEDIMessage>();

	static IEDIMessageCollectionProvider GetEDIMessageCollectionProvider(EDIMessage cUSRESMessage)
	{
		if (cUSRESMessage.EM_LinkedObject is IMessageAttachee and IEDIMessageCollectionProvider provider)
		{
			return provider;
		}
		else
		{
			return new LinkedObjectNotFoundEDIMessageCollectionProvider(cUSRESMessage);
		}
	}

	internal sealed class LinkedObjectNotFoundEDIMessageCollectionProvider : IEDIMessageCollectionProvider
	{
		public LinkedObjectNotFoundEDIMessageCollectionProvider(EDIMessage cUSRESMessage)
		{
			Factory = cUSRESMessage.Factory;
		}

		public EDIMessageCollection Messages => null;
		public BusinessObjectFactory Factory { get; }
	}
}
