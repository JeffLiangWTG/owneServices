using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class NonActiveCommissionAgreementOverallItemCollection : BusinessObjectCollection<ViewCommissionAgreementOverallItem>
	{
		public NonActiveCommissionAgreementOverallItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

