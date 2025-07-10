using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	#region Registry Item

	public class ShipmentInspectionTypeRegistryItem : TranslatableRegistryItem<ShipmentInspectionTypes, ShipmentInspectionTypes>, ICodeDescriptionPairListProvider
	{
		public ShipmentInspectionTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ShipmentInspectionTypes defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public ShipmentInspectionTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ShipmentInspectionTypes defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ShipmentInspectionTypeRegistryDataType(defaultValue), storage, options, defaultValue))
		{
			this.defaultValue = defaultValue;
		}

		#region ICodeDescriptionPairListProvider

		CodeDescriptionPairList ICodeDescriptionPairListProvider.CodeDescriptionPairList
		{
			get { return this.Value.Types.GetCodeDescriptionPairList(); }
		}

		#endregion

		public override bool IsTranslatable
		{
			get { return true; }
		}

		protected override ShipmentInspectionTypes Convert(ShipmentInspectionTypes value)
		{
			foreach (ShipmentInspectionType item in value.Types)
			{
				using (item.GetValidationSuspender())
				{
					item.Description = GetMultilingualString(item.EnglishDescription);
				}
			}

			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (ShipmentInspectionType item in defaultValue.Types)
				{
					yield return (ResourceString)item.Description;
				}
			}
		}

		public override IEnumerable<string> GetCaptions(ShipmentInspectionTypes value)
		{
			foreach (ShipmentInspectionType item in value.Types)
			{
				yield return item.EnglishDescription;
			}
		}

		public override int MaxLength
		{
			get { return 256; }
		}

#if DEBUG
	public 
#endif
		readonly ShipmentInspectionTypes defaultValue;
	}

	#endregion

	#region Data Type

	[RegistryEditor("Enterprise.Registry.GUI.ShipmentInspectionTypeRegistryItemEditor, Enterprise.Registry.GUI")]
	public class ShipmentInspectionTypeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ShipmentInspectionTypes>
	{
		public ShipmentInspectionTypeRegistryDataType()
		{
		}

		public ShipmentInspectionTypeRegistryDataType(ShipmentInspectionTypes defaultValue)
			: base(defaultValue)
		{
		}
	}

	#endregion
}
