using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class CusEntryHeaderLookups : EU.Business.Declaration.CusEntryHeaderLookups
	{
		public CusEntryHeaderLookups(CusEntryHeader parent) : base(parent)
		{
		}

		public virtual RefCurrencyCollection CurrencyList => new RefCurrencyCollection(Factory);
	}
}
