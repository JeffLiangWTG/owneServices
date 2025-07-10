using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class PreApprovalCollection : CusSupportingInfoCollection<PreApproval>
	{
		public PreApprovalCollection(JobComInvoiceLine parent)
			: base(parent, CusSupportingInfoTypeList.Codes.PreApproval)
		{
		}
	}
}
