namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsBillAdditionalDocumentValidationDecider : INctsAdditionalInfoValidationDecider
	{
	}

	public interface INctsBillAdditionalDocumentPhase5ValidationDecider : INctsBillAdditionalDocumentValidationDecider
	{
		bool IsRuleR3062Active { get; }
	}
}
