using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Module
{
	public class EUH7BillFilterBusinessObjectLookups
	{
		public EUH7BillFilterBusinessObjectLookups(EUH7BillFilterBusinessObject parent)
		{
			Parent = parent;
		}

		protected readonly FilterStripBusinessObject Parent;

		protected BusinessObjectFactory Factory => Parent.Factory;

		public virtual CodeDescriptionPairList CustomsStatusList => Factory.GetCachedValue<AISEntryStatusList>();
	}
}
