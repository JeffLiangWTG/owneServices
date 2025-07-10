namespace Enterprise.Customs.FR.Business.Declaration
{
	public class UCC6ImportPreviousDocumentValidationDecider : IPreviousDocumentValidationDecider
	{
		public bool IsRuleNAT_088Active => true;

		public bool IsRuleNAT_130Active => true;

		public bool ISRuleNAT_259Active => true;
	}
}
