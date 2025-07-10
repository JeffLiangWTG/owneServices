
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRRefundReasonCollection : BusinessObjectCollection<CMRRefundReason>
	{
		public CMRRefundReasonCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
