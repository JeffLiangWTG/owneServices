using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class CodePairWithAdditionalEventRegistryItem : StronglyTypedRegistryItem<string>
	{
		public CodePairWithAdditionalEventRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpList, bool allowBlank, bool validate, ComboBoxRegistryEditorInfo editorInfo, Func<IRegistryItem, bool> additionalItemForValidation, RegistryStorageFlags storage, RegistryOptions options, string defaultValue, bool useDefaultDefaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodePairRegistryWithAdditionalEventDataType(lookUpList, allowBlank, validate, additionalItemForValidation), editorInfo, storage, options, defaultValue, useDefaultDefaultValue))
		{
		}
	}
}
