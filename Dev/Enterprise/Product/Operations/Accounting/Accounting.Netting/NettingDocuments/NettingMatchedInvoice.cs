using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingMatchedInvoice : NonPersistentBusinessObject
	{
		public NettingMatchedInvoice(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZString Issuer { get; set; }
		public ZString Recipient { get; set; }
		public ZString ARInvoiceReference { get; set; }
		public ZString APInvoiceReference { get; set; }
	}
}
