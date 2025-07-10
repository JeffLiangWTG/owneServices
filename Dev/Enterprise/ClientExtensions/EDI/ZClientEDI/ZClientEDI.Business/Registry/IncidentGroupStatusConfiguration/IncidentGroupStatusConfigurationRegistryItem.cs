using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class IncidentGroupStatusConfigurationRegistryItem : StronglyTypedRegistryItem<IncidentGroupTypeCollection>
	{
		public IncidentGroupStatusConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IncidentGroupTypeCollection defaultValues)
			: base(new RegistryItemImpl(
				name,
				category,
				caption,
				hint,
				new IncidentGroupStatusConfigurationDataType(),
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				defaultValues))
		{ }
	}
}
