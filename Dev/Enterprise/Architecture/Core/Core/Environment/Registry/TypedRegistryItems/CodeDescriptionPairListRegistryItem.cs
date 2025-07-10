using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class CodeDescriptionPairListRegistryItem : TranslatableRegistryItem<ReadOnlyCodeDescriptionPairList, ReadOnlyCodeDescriptionPairList>
	{
		public CodeDescriptionPairListRegistryItem(RegistryItemImpl inner, bool isLocalizable, ReadOnlyCodeDescriptionPairList allDefaultValues)
			: base(inner)
		{
			this.isLocalizable = isLocalizable;
			this.allDefaultValues = allDefaultValues;
			ValidateLocalizable();
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage)
			: this(name, category, caption, hint, maxCodeLength, storage, RegistryOptions.Default)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, params MultilingualString[] categories)
			: this(name, caption, hint, maxCodeLength, new CodeDescriptionPairListEditorInfo(), storage, RegistryOptions.Default, new ReadOnlyCodeDescriptionPairList(), false, categories)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, ReadOnlyCodeDescriptionPairList defaultValue, params MultilingualString[] categories)
			: this(name, caption, hint, maxCodeLength, new CodeDescriptionPairListEditorInfo(), storage, RegistryOptions.Default, defaultValue, false, categories)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, bool isLocalizable, ReadOnlyCodeDescriptionPairList defaultValue, params MultilingualString[] categories)
			: this(name, caption, hint, maxCodeLength, new CodeDescriptionPairListEditorInfo(), storage, isLocalizable, RegistryOptions.Default, defaultValue, false, categories)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, maxCodeLength, storage, options, new ReadOnlyCodeDescriptionPairList(), false)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, ReadOnlyCodeDescriptionPairList defaultValue)
			: this(name, category, caption, hint, maxCodeLength, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, bool isLocalizable, ReadOnlyCodeDescriptionPairList defaultValue)
			: this(name, category, caption, hint, maxCodeLength, storage, isLocalizable, RegistryOptions.Default, defaultValue)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, RegistryOptions options, bool isLocalizable, ReadOnlyCodeDescriptionPairList defaultValue)
			: this(name, category, caption, hint, maxCodeLength, storage, isLocalizable, options, defaultValue)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue)
			: this(name, category, caption, hint, maxCodeLength, storage, options, defaultValue, false)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, bool isLocalizable, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue)
			: this(name, category, caption, hint, maxCodeLength, storage, isLocalizable, options, defaultValue, false)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool useDefaultDefaultValue)
			: this(name, category, caption, hint, maxCodeLength, new CodeDescriptionPairListEditorInfo(), storage, options, defaultValue, useDefaultDefaultValue)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, RegistryStorageFlags storage, bool isLocalizable, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool useDefaultDefaultValue)
			: this(name, category, caption, hint, maxCodeLength, new CodeDescriptionPairListEditorInfo(), storage, isLocalizable, options, defaultValue, useDefaultDefaultValue)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, CodeDescriptionPairListEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool useDefaultDefaultValue)
			: this(name, caption, hint, maxCodeLength, editorInfo, storage, options, defaultValue, useDefaultDefaultValue, category)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, CodeDescriptionPairListEditorInfo editorInfo, RegistryStorageFlags storage, bool isLocalizable, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool useDefaultDefaultValue)
			: this(name, caption, hint, maxCodeLength, editorInfo, storage, isLocalizable, options, defaultValue, useDefaultDefaultValue, category)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString caption, MultilingualString hint, int maxCodeLength, CodeDescriptionPairListEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool useDefaultDefaultValue, params MultilingualString[] categories)
			: this(name, caption, hint, maxCodeLength, editorInfo, storage, true, options, defaultValue, useDefaultDefaultValue, categories)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, int maxCodeLength, CodeDescriptionPairListEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options,
			RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter)
			: this(new RegistryItemImplWithDynamicDefaultValue(name, new MultilingualString[] { category }, caption, hint, new CodeDescriptionPairListRegistryDataType(maxCodeLength), editorInfo, storage, options, defaultValueGetter))
		{
		}

		protected CodeDescriptionPairListRegistryItem(IRegistryItem item)
			: base(item)
		{
		}

		public CodeDescriptionPairListRegistryItem(string name, MultilingualString caption, MultilingualString hint, int maxCodeLength, CodeDescriptionPairListEditorInfo editorInfo, RegistryStorageFlags storage, bool isLocalizable, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool useDefaultDefaultValue, params MultilingualString[] categories)
			: base(new RegistryItemImpl(name, caption, hint, new CodeDescriptionPairListRegistryDataType(maxCodeLength), editorInfo, storage, options, defaultValue, useDefaultDefaultValue, categories))
		{
			this.isLocalizable = isLocalizable;
			allDefaultValues = defaultValue;
			ValidateLocalizable();
		}

		void ValidateLocalizable()
		{
			foreach (CodeDescriptionPair pair in allDefaultValues)
			{
				if (isLocalizable && !HasResString(pair))
				{
					throw new InvalidCastException("The default value for a localizable CodeDescriptionPairListRegistryItem must have codes or descriptions constructed using ResString.GetMultilingualString(). Name=" + Name);
				}
				else if (!isLocalizable && HasResString(pair))
				{
					throw new InvalidCastException("The default value for a non localizable CodeDescriptionPairListRegistryItem cannot be localizable. Name=" + Name);
				}
				else if (pair.MultilingualCode is ResourceString && !string.IsNullOrEmpty(pair.MultilingualDescription.GetUnresolvedString()))
				{
					throw new InvalidCastException("A CodeDescriptionPairListRegistryItem with localizable codes must have empty descriptions");
				}
			}
		}

		bool HasResString(CodeDescriptionPair pair)
		{
			return pair.MultilingualDescription is ResourceString || (string.IsNullOrEmpty(pair.MultilingualDescription.GetUnresolvedString()) && pair.MultilingualCode is ResourceString);
		}

		public override bool IsTranslatable
		{
			get { return isLocalizable; }
		}

		protected override ReadOnlyCodeDescriptionPairList Convert(ReadOnlyCodeDescriptionPairList value)
		{
			if (IsTranslatable)
			{
				var list = new CodeDescriptionPairList();
				foreach (CodeDescriptionPair pair in value)
				{
					if (string.IsNullOrEmpty(pair.MultilingualDescription.GetUnresolvedString()))
					{
						list.AddPair(GetMultilingualString(pair.Code));
					}
					else
					{
						list.Add(new CodeDescriptionPair(pair.Code, GetMultilingualString(pair.MultilingualDescription.GetUnresolvedString())));
					}
				}
				return list;
			}
			else
			{
				return value;
			}
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (CodeDescriptionPair pair in allDefaultValues)
				{
					yield return string.IsNullOrEmpty(pair.MultilingualDescription.GetUnresolvedString()) ? (ResourceString)pair.MultilingualCode : (ResourceString)pair.MultilingualDescription;
				}
			}
		}

		public override IEnumerable<string> GetCaptions(ReadOnlyCodeDescriptionPairList value)
		{
			foreach (CodeDescriptionPair pair in value)
			{
				yield return string.IsNullOrEmpty(pair.MultilingualDescription.GetUnresolvedString()) ? pair.Code : pair.MultilingualDescription.GetUnresolvedString();
			}
		}

		public override int MaxLength
		{
			get { return -1; }
		}

		readonly bool isLocalizable;

		readonly ReadOnlyCodeDescriptionPairList allDefaultValues;
	}
}
