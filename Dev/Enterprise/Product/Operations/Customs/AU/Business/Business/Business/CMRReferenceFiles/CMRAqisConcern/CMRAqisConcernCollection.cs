
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisConcernCollection : BusinessObjectCollection<CMRAqisConcern>
	{
		public CMRAqisConcernCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
