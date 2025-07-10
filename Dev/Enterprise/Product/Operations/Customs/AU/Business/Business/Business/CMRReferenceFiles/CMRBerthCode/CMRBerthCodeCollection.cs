using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRBerthCodeCollection : BusinessObjectCollection<CMRBerthCode>
	{
		public CMRBerthCodeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CMRBerthCodeCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
