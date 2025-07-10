namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IPreviousDocumentValidationDecider : EU.Business.Declaration.MultiLineAddInfos.IPreviousDocumentValidationDecider
	{
		bool IsRuleNAT_088Active { get; }

		bool IsRuleNAT_130Active { get; }

		bool ISRuleNAT_259Active { get; }
	}
}
