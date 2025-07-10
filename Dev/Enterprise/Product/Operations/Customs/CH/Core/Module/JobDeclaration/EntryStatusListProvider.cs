using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.Module;

class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
{
	protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory) => CommonLookups.CustomsStatusList(factory);
}
