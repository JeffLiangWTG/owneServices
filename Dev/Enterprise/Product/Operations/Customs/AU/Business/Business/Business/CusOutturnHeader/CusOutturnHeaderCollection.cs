using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderCollection : BusinessObjectCollection<CusOutturnHeader>
	{
		public CusOutturnHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
