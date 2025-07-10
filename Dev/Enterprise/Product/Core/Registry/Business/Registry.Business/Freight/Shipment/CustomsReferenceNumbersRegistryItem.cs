using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CustomsReferenceNumberTypesRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CustomsReferenceNumberTypeCollection, CustomsReferenceNumberTypeCollection>
	{
		public CustomsReferenceNumberTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CustomsReferenceNumberTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CustomsReferenceNumberTypesDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}

		public CustomsReferenceNumberTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CustomsReferenceNumberTypeCollection defaultValue, RegistryOptions registryOptions)
			: base(new RegistryItemImpl(name, category, caption, hint, new CustomsReferenceNumberTypesDataType(), storage, registryOptions, defaultValue))
		{
		}

		protected override object GetValueWithoutFallbackCore(System.Guid companyPK, System.Guid branchPK, System.Guid departmentPK)
		{
			var value = base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as CustomsReferenceNumberTypeCollection;

			if (value != null)
			{
				var defaults = DefaultValue;
				if (defaults != null)
				{
					FixMissingDefaultsAndSetSystemDefined(value, defaults);
				}
			}

			return value;
		}

		static void FixMissingDefaultsAndSetSystemDefined(CustomsReferenceNumberTypeCollection value, CustomsReferenceNumberTypeCollection defaults)
		{
			foreach (CustomsReferenceNumberType defaultItem in defaults.Cast<CustomsReferenceNumberType>().Where(d => d.SystemDefined))
			{
				var item = (CustomsReferenceNumberType)value.FindByCode(defaultItem.Code);

				if (item == null || defaultItem.Description != item.Description || defaultItem.IsUnique != item.IsUnique)
				{
					if (item != null)
					{
						value.Remove(item);
					}
					item = value.Add(defaultItem.Code, defaultItem.Description, defaultItem.IsUnique);
				}

				item.SystemDefined = defaultItem.SystemDefined;
			}
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.CustomsReferenceNumberTypesRegistryItemEditor, Enterprise.Registry.GUI")]
	public class CustomsReferenceNumberTypesDataType : NonPersistentBusinessObjectRegistryDataType<CustomsReferenceNumberTypeCollection>
	{
	}
}
