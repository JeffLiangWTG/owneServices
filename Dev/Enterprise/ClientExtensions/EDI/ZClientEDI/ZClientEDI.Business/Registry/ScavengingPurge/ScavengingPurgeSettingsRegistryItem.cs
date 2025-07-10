using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public sealed class ScavengingPurgeSettingsRegistryItem : StronglyTypedRegistryItem<ScavengingPurgeSettings>
	{
		public ScavengingPurgeSettingsRegistryItem(string category)
			: base(new RegistryItemImpl(
				"ScavengingPurgeSettings",
				(NoResString)category,
				(NoResString)"Scavenging Purge Settings",
				(NoResString)"Specify purge periods for scavenging elements",
				new ScavengingPurgeSettingsRegistryDataType(),
				RegistryStorageFlags.System,
				ScavengingPurgeSettings.GetDefaults()))
		{
		}
	}
}

