
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.TNT
{
	public class ReadOnlyCusMAWBCollection : BusinessObjectCollection<CusMAWB>
	{
		public ReadOnlyCusMAWBCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			return null;
		}
	}
}
