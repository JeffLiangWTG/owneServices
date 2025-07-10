using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.Module;

public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
{
	public EntryStatusListProvider()
	{
	}

	protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory) => factory.GetCachedValue<AEEntryStatusList>();
}
