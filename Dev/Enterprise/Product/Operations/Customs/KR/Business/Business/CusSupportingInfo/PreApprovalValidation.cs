using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class PreApprovalValidation : CusSupportingInfoValidation
	{
		public PreApprovalValidation(PreApproval parent)
			: base(parent)
		{
		}

		public new PreApproval Parent => (PreApproval)base.Parent;
	}
}
