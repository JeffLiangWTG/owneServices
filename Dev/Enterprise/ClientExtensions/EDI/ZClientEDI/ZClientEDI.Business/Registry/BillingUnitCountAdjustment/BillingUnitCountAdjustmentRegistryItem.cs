using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class BillingUnitCountAdjustmentRegistryItem : StronglyTypedRegistryItem<BillingUnitCountAdjustmentCollection, BillingUnitCountAdjustmentCollection>
	{
		public BillingUnitCountAdjustmentRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, BillingUnitCountAdjustmentRegistryEditorInfo editorInfo, RegistryStorageFlags storage)
			: this(name, category, caption, hint, editorInfo, storage, new BillingUnitCountAdjustmentCollection())
		{
		}

		public BillingUnitCountAdjustmentRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, BillingUnitCountAdjustmentRegistryEditorInfo editorInfo, RegistryStorageFlags storage, BillingUnitCountAdjustmentCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new BillingUnitCountAdjustmentRegistryDataType(), editorInfo, storage, RegistryOptions.Default, defaultValue))
		{
		}
	}

	public class BillingUnitCountAdjustmentRegistryDataType : NonPersistentBusinessObjectRegistryDataType<BillingUnitCountAdjustmentCollection>
	{
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.BillingUnitCountAdjustmentRegistryEditor, ZClientEDI")]
	public class BillingUnitCountAdjustmentRegistryEditorInfo : IRegistryEditorInfo
	{
		public Type BaseDataTypeToBeEdited => typeof(BillingUnitCountAdjustmentCollection);
	}
}
