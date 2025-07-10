using CargoWise.EntityFramework;
using Enterprise.UniversalCopy.Business;

namespace Enterprise.UniversalCopy.Module
{
	public class StmUniversalCopyCollection : BusinessObjectCollection<StmUniversalCopy>
	{
		public StmUniversalCopyCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }
	}
}
