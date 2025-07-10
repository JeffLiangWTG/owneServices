using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class GlowTempOrgRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<GlowTempOrgRequiredFieldCollection, GlowTempOrgRequiredFieldCollection>
	{
		public GlowTempOrgRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, GlowTempOrgRequiredFieldCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GlowTempOrgRegistryDataType(defaultValue), storage, option, defaultValue))
		{
		}

		public override int MaxLength => 256;
	}

	[RegistryEditor("Enterprise.Registry.GUI.GlowTempOrgRequiredFieldsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class GlowTempOrgRegistryDataType : NonPersistentBusinessObjectRegistryDataType<GlowTempOrgRequiredFieldCollection>
	{
		public GlowTempOrgRegistryDataType(GlowTempOrgRequiredFieldCollection defaultValue)
			: base(defaultValue)
		{
		}
	}
}
