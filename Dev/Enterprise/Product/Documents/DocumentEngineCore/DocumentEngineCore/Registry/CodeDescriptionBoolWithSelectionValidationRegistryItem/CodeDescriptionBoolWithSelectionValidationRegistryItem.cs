using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class CodeDescriptionBoolWithSelectionValidationRegistryItem : CodeDescriptionBoolRegistryItem
	{
		public CodeDescriptionBoolWithSelectionValidationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionBoolRegistryEditorInfo editorInfo, RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter)
			: base(new RegistryItemImplWithDynamicDefaultValue(name, category, caption, hint, new CodeDescriptionBoolWithSelectionValidationRegistryDataType(), storage, options, defaultValueGetter), editorInfo)
		{
		}
	}

	class CodeDescriptionBoolWithSelectionValidationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionBoolCollection>
	{
		public CodeDescriptionBoolWithSelectionValidationRegistryDataType()
		{
		}

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, CodeDescriptionBoolCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var defaultValues = (CodeDescriptionBoolCollection)((IRegistryItemInternals)registryItem).GetDefaultValue(companyPK, branchPK, departmentPK);
			foreach (CodeDescriptionBool value in defaultValues)
			{
				if (proposedValue.GetBoolFromCode(value.Code) != value.Bool)
				{
					return;
				}
			}
			throw new RegistryValidationException(Res.GetString("0e21ac01-2cf2-48ae-8337-384ef0548759", "Invalid Input. Please select at least one option to override."));
		}
	}
}
