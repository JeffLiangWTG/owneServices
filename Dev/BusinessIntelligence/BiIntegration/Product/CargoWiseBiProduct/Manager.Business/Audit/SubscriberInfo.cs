using CargoWise.EntityFramework;
using CargoWise.Types;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class SubscriberInfo : NonPersistentBusinessObject
	{
		public SubscriberInfo(ZString code)
		{
			Code = code;
		}

		public ZString Code { get; set; }
		public ZString Description { get; set; }
		public ZString LsnHighWaterMark { get; set; }
		public ZString SeqValHighWaterMark { get; set; }
		public ZDateTime TransactionDateUtc { get; set; }
		public ZDateTime TransactionDateLocal { get; set; }
		public ZInt PeriodHighWatermark { get; set; }
	}
}
