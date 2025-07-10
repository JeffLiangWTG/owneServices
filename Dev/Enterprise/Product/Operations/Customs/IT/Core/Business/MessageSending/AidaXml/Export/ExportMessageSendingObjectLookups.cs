using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Export;

public sealed class ExportMessageSendingObjectLookups : Ucc6JobDeclarationMessageSendingObjectLookups
{
	public ExportMessageSendingObjectLookups(ExportMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	protected override CodeDescriptionPairList GetCancellationReasonList()
		=> Factory.GetCachedValue<Ucc6ExportCancellationReasonList>();

	protected override CodeDescriptionPairList CancellationAndAmendmentLegislativeReferenceListCore
	{
		get
		{
			if (SendingObject.IsAmend)
			{
				return Factory.GetCachedValue<Ucc6ExportAmendmentLegislativeReferenceList>();
			}

			if (SendingObject.IsCancel)
			{
				return Factory.GetCachedValue<Ucc6ExportCancellationLegislativeReferenceList>();
			}

			return new CodeDescriptionPairList();
		}
	}
}
