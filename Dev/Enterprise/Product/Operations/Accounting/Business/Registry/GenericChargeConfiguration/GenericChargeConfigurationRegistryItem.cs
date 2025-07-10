using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class GenericChargeConfigurationRegistryItem : StronglyTypedRegistryItem<GenericChargeConfigurationCollection>
	{
		public GenericChargeConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new RegistryItemImpl(name, category, caption, hint, new GenericChargeConfigurationRegistryDataType(), storage, option))
		{
		}

		public GenericChargeConfigurationRegistryItem(IRegistryItem inner)
			: base(inner)
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.GenericChargeConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class GenericChargeConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<GenericChargeConfigurationCollection>
	{
		protected override bool ValuesAreEqualCore(GenericChargeConfigurationCollection a, GenericChargeConfigurationCollection b)
		{
			//Implementation inferred through reading the test
			if (a.Count != b.Count)
			{
				return false;
			}

			for (var i = 0; i < a.Count; i++)
			{
				var itemA = a[i];
				var itemB = b[i];

				if (itemA.ChargePK != itemB.ChargePK)
				{
					return false;
				}
			}

			return true;
		}
	}
}
