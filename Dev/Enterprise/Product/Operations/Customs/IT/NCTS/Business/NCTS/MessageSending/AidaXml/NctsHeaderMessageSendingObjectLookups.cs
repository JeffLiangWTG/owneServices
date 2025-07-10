using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public sealed class NctsHeaderMessageSendingObjectLookups : EU.NCTS.Business.NctsHeaderMessageSendingObjectLookups
{
	public NctsHeaderMessageSendingObjectLookups(NctsHeaderMessageSendingObject parent) : base(parent)
	{
	}

	public CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<Phase5DepartureDeclarationTypeList>();

	public CodeDescriptionPairList ReasonList => GetReasonList();

	public CodeDescriptionPairList LegislativeReferenceList => GetLegislativeReferenceList();

	#region Implementation

	CodeDescriptionPairList GetReasonList()
	{
		var parent = Parent;
		if (parent.IsAmend)
		{
			return Factory.GetCachedValue<NctsPhase5AmendmentReasonList>();
		}
		else if (parent.IsCancel)
		{
			return Factory.GetCachedValue<NctsPhase5CancellationReasonList>();
		}
		return new CodeDescriptionPairList();
	}

	CodeDescriptionPairList GetLegislativeReferenceList()
	{
		var parent = Parent;
		if (parent.IsAmend)
		{
			return Factory.GetCachedValue<NctsPhase5AmendmentLegislativeReferenceList>();
		}
		else if (parent.IsCancel)
		{
			return Factory.GetCachedValue<NctsPhase5CancellationLegislativeReferenceList>();
		}
		return new CodeDescriptionPairList();
	}

	#endregion
}
