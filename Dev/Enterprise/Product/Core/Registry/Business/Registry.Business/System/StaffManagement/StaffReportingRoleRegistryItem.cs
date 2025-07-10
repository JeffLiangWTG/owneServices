using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class StaffReportingRoleRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<StaffReportingRoleCollection, StaffReportingRoleCollection>
	{
		public StaffReportingRoleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, StaffReportingRoleCollection defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public StaffReportingRoleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, StaffReportingRoleCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new StaffReportingRoleRegistryDataType(defaultValue), storage, options, defaultValue))
		{
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.StaffReportingRoleRegistryItemEditor, Enterprise.Registry.GUI")]
	public class StaffReportingRoleRegistryDataType : NonPersistentBusinessObjectRegistryDataType<StaffReportingRoleCollection>
	{
		public StaffReportingRoleRegistryDataType(StaffReportingRoleCollection defaultValue)
			: base(defaultValue)
		{
		}
	}
}
