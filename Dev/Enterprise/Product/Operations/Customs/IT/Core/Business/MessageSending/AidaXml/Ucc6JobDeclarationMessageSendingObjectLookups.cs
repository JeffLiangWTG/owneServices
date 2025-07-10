using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public abstract class Ucc6JobDeclarationMessageSendingObjectLookups : ZLookups
{
	protected Ucc6JobDeclarationMessageSendingObjectLookups(Ucc6JobDeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
		SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}

	protected Ucc6JobDeclarationMessageSendingObject SendingObject { get; }

	public CodeDescriptionPairList MessageTypeList => new Ucc6SendingObjectMessageTypeListBuilder(SendingObject.Header).GetMessageTypeList();

	public CodeDescriptionPairList CancellationOrAmendmentReasonList
	{
		get
		{
			if (SendingObject.IsAmend)
			{
				return Factory.GetCachedValue<AmendmentReasonList>();
			}

			if (SendingObject.IsCancel)
			{
				return GetCancellationReasonList();
			}

			return new CodeDescriptionPairList();
		}
	}

	protected abstract CodeDescriptionPairList GetCancellationReasonList();

	public CodeDescriptionPairList CancellationAndAmendmentLegislativeReferenceList => CancellationAndAmendmentLegislativeReferenceListCore;

	protected abstract CodeDescriptionPairList CancellationAndAmendmentLegislativeReferenceListCore { get; }
}
