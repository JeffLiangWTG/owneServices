using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class UnitMeasurementTextOverrideRegistryItem : StronglyTypedRegistryItem<UnitMeasurementTextOverrideCollection>
	{
		public UnitMeasurementTextOverrideRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new UnitMeasurementTextOverrideRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.UnitMeasurementTextOverrideRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class UnitMeasurementTextOverrideRegistryDataType : NonPersistentBusinessObjectRegistryDataType<UnitMeasurementTextOverrideCollection>
	{
		public UnitMeasurementTextOverrideRegistryDataType()
		{
		}
	}
}
