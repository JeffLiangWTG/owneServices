using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business.Testing
{
	internal class RefundManagerDummy<T> : RefundManager<T> where T : BusinessObject, IRefundEnquiry
	{
		public RefundManagerDummy(T owner) : base(owner)
		{
		}

		protected override ClientRefund NewRefund()
		{
			return Enterprise.Client.UPE.Business.Testing.ClientRefundTest.TestHelper.PopulatedRefund(Factory);
		}
	}
}
