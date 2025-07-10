using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DpsConfidenceThresholdsRegistryItem : StronglyTypedRegistryItem<DpsConfidenceThresholdsBusinessObject, DpsConfidenceThresholdsBusinessObject>
	{
		public DpsConfidenceThresholdsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DpsConfidenceThresholdsBusinessObject defaultValue)
			: base(new DpsConfidenceThresholdsRegistryItemImpl(name, category, caption, hint, storage, options, defaultValue))
		{
		}

		public void SetValue(DpsConfidenceThresholdsBusinessObject value)
		{
			SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		class DpsConfidenceThresholdsRegistryItemImpl : RegistryItemImpl
		{
			public DpsConfidenceThresholdsRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DpsConfidenceThresholdsBusinessObject defaultValue)
				: base(name, category, caption, hint, new DpsConfidenceThresholdsRegistryDataType(), storage, options, defaultValue)
			{
			}
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.DpsConfidenceThresholdsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class DpsConfidenceThresholdsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DpsConfidenceThresholdsBusinessObject>
	{
	}
}
