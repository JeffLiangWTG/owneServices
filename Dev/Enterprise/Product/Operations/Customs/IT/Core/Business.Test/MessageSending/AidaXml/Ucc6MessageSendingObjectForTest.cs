using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Testing;

public class Ucc6MessageSendingObjectForTest : Ucc6JobDeclarationMessageSendingObject
{
	public Ucc6MessageSendingObjectForTest(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
		: base(header, jobDeclarationMessageSendingObjectParent)
	{
	}

	protected override Ucc6JobDeclarationMessageSendingObjectLookups GetNewLookups() => new MessageSendingObjectLookupsForTest(this);

	protected override ZString ServiceTypeNamespace { get; }
	protected override ZString ServiceTypePrefix { get; }

	protected override IXmlMessageBuilder GetMessageBuilder() => null;

	protected override IXmlMessageBuilder GetCancellationXmlMessageBuilder(ICancellation cancellationInfo) => null;
}

class MessageSendingObjectLookupsForTest : Ucc6JobDeclarationMessageSendingObjectLookups
{
	public MessageSendingObjectLookupsForTest(Ucc6JobDeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	protected override CodeDescriptionPairList CancellationAndAmendmentLegislativeReferenceListCore => null;

	protected override CodeDescriptionPairList GetCancellationReasonList() => null;
}
