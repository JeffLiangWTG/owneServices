using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class TransactionMatchLinkCollection : BusinessObjectCollection<TransactionMatchLink>
	{
		public TransactionMatchLinkCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TransactionMatchLinkCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
