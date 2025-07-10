using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionWithThreeGroupsRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CodeDescriptionWithThreeGroupsCollection, CodeDescriptionWithThreeGroupsCollection>
	{
		public CodeDescriptionWithThreeGroupsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int codeMaxLength, RegistryStorageFlags storage,
			RegistryOptions options, CodeDescriptionWithThreeGroupsRegistryEditorInfo editorInfo, RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter,
			ReadOnlyCodeDescriptionPairList fiscalOutputNetCodeList, ReadOnlyCodeDescriptionPairList fiscalOutputTaxCodeList, ReadOnlyCodeDescriptionPairList fiscalInputTaxCodeList)
			: base(new RegistryItemImplWithDynamicDefaultValue(name, category, caption, hint, new CodeDescriptionWithThreeGroupsRegistryDataType(codeMaxLength), storage, options, defaultValueGetter), editorInfo)
		{
			if (DefaultValue != null)
			{
				DefaultValue.GroupLookup = fiscalOutputNetCodeList ?? new ReadOnlyCodeDescriptionPairList();
				DefaultValue.Group2Lookup = fiscalOutputTaxCodeList ?? new ReadOnlyCodeDescriptionPairList();
				DefaultValue.Group3Lookup = fiscalInputTaxCodeList ?? new ReadOnlyCodeDescriptionPairList();
				DefaultValue.CodeMaxLength = ((CodeDescriptionWithThreeGroupsRegistryDataType)Inner.DataType).CodeMaxLength;
			}
		}

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var value = new CodeDescriptionWithThreeGroupsCollection(base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as CodeDescriptionWithThreeGroupsCollection);

			if (DefaultValue != null)
			{
				var defaults = DefaultValue;

				if (value.CodeMaxLength == 0)
				{
					value.CodeMaxLength = DefaultValue.CodeMaxLength;
				}
				if (value.GroupLookup == null || value.GroupLookup.Count == 0)
				{
					value.GroupLookup = DefaultValue.GroupLookup;
				}
				if (value.Group2Lookup == null || value.Group2Lookup.Count == 0)
				{
					value.Group2Lookup = DefaultValue.Group2Lookup;
				}
				if (value.Group3Lookup == null || value.Group3Lookup.Count == 0)
				{
					value.Group3Lookup = DefaultValue.Group3Lookup;
				}
			}

			return value;
		}

		public override int MaxLength => 256;

		public override bool IsTranslatable => false;
	}

	class CodeDescriptionWithThreeGroupsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionWithThreeGroupsCollection>
	{
		public CodeDescriptionWithThreeGroupsRegistryDataType(int codeMaxLength)
		{
			CodeMaxLength = codeMaxLength;
		}
		public int CodeMaxLength { get; set; }
	}
}
