
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class ViewMatchGroupCollection : BusinessObjectCollection<ViewMatchGroup>
	{
		public ViewMatchGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ViewMatchGroupCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
