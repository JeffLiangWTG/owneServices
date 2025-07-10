namespace Enterprise.Customs.ES.NCTS.Business
{
	public class CommonPreviousDocumentValidation : EU.NCTS.Business.CommonPreviousDocumentValidation
	{
		public CommonPreviousDocumentValidation(CommonPreviousDocument parent) : base(parent)
		{
		}

		protected override bool ShouldCheckCSI_ReferenceNumber2Mandatory => false;
	}
}
