using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory) => factory.GetCachedValue<CustomsStatusList>();
	}
}
