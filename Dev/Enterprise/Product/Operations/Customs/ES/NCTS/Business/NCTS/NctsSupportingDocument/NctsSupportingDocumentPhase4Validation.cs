namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsSupportingDocumentPhase4Validation : EU.NCTS.Business.NctsSupportingDocumentPhase4Validation
	{
		public NctsSupportingDocumentPhase4Validation(EU.NCTS.Business.NctsSupportingDocument parent) : base(parent)
		{
		}

		protected override bool ShouldCheckConditionC902 => false;
	}
}
