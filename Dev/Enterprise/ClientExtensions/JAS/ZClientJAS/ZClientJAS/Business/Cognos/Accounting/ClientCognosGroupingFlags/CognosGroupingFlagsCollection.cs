
using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosGroupingFlagsCollection : BusinessObjectCollection<CognosGroupingFlags>
	{
		public CognosGroupingFlagsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CognosGroupingFlagsCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
