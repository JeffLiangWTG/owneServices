using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class AFRReporterIDRegistryItem : StronglyTypedRegistryItem<AFRReporterID>
	{
		public AFRReporterIDRegistryItem(ZString name, MultilingualString category, MultilingualString caption, MultilingualString hint)
		: base(new RegistryItemImpl(name, category, caption, hint, new AFRReporterIDRegistryDataType(), RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.IsOnlyForController))
		{
		}
	}
}
