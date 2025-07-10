
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisEntityCollection : BusinessObjectCollection<CMRAqisEntity>
	{
		public CMRAqisEntityCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
