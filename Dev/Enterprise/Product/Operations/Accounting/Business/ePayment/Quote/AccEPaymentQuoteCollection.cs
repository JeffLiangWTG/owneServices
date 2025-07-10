using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business
{
	public class AccEPaymentQuoteCollection : BusinessObjectCollection<AccEPaymentQuote>
	{
		public AccEPaymentQuoteCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : base(factory, sQLFilter)
		{
		}
		public AccEPaymentQuoteCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}

	public abstract class AccEPaymentQuoteCollection<T> : BusinessObjectCollection<T>
		where T : AccEPaymentQuote
	{
		public AccEPaymentQuoteCollection(BusinessObjectFactory factory, ZQuery sQLFilter) : base(factory, sQLFilter)
		{
		}
		public AccEPaymentQuoteCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
