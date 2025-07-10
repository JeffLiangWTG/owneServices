using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Environment.Registry.DataTypesAndValidators;

namespace Enterprise.ZArchitecture.Environment
{
	public class DeleteExpiredRatesRegistryItem : StronglyTypedRegistryItem<DeleteExpiredRates>
	{
		public DeleteExpiredRatesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DeleteExpiredRates deleteExpiredRates)
			: base(new RegistryItemImpl(name, category, caption, hint, new DeleteExpiredRatesRegistryDataType(deleteExpiredRates), new DeleteExpiredRatesEditorInfo(), storage, options, deleteExpiredRates, true))
		{
		}
	}
}
