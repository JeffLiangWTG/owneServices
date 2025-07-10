
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBCollectionNonDependent : BusinessObjectCollection<CusHAWB>
	{
		public CusHAWBCollectionNonDependent(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
