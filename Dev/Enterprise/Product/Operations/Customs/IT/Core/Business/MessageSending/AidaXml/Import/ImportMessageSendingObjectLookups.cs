using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public sealed class ImportMessageSendingObjectLookups : Ucc6JobDeclarationMessageSendingObjectLookups
{
	public ImportMessageSendingObjectLookups(ImportMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	protected override CodeDescriptionPairList GetCancellationReasonList()
		=> Factory.GetCachedValue<Ucc6ImportCancellationReasonList>();

	protected override CodeDescriptionPairList CancellationAndAmendmentLegislativeReferenceListCore
		=> Factory.GetCachedValue<Ucc6ImportCancellationAndAmendmentLegislativeReferenceList>();
}
