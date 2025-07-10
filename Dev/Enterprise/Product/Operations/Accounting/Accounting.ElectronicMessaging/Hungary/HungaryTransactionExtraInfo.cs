using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public class HungaryTransactionExtraInfo
	{
		public bool IsDebtorPrivatePerson { get; set; }

		public ZDateTime? OriginalTransactionPostDate { get; set; }

		public InvoiceDeliveryMethod? InvoiceDeliveryMethod { get; set; }
	}
}
