using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class GroupInvoiceChargeCollection<T> : JobComInvChargeCollection<T> where T : GroupInvoiceCharge
	{
		public GroupInvoiceChargeCollection(JobComInvoiceGroupHeader groupInvoice)
			: base(groupInvoice)
		{
		}
	}
}
