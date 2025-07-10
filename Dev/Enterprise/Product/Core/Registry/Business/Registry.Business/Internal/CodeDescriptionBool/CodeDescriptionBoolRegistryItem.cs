using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionBoolRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CodeDescriptionBoolCollection, CodeDescriptionBoolCollection>
	{
		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString boolColumnCaption)
			: this(name, category, caption, hint, storage, boolColumnCaption, false)
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionBoolRegistryEditorInfo editorInfo)
			: this(name, category, caption, hint, storage, editorInfo, false, new ReadOnlyCodeDescriptionPairList())
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString boolColumnCaption, bool defaultBoolColumnValue)
			: this(name, category, caption, hint, storage, boolColumnCaption, defaultBoolColumnValue, new ReadOnlyCodeDescriptionPairList())
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString boolColumnCaption, ReadOnlyCodeDescriptionPairList defaultValue)
			: this(name, category, caption, hint, storage, boolColumnCaption, false, defaultValue)
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString boolColumnCaption, CodeDescriptionBoolCollection defaultValue)
			: this(name, category, caption, hint, storage, new CodeDescriptionBoolRegistryEditorInfo(boolColumnCaption), defaultValue)
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString boolColumnCaption, bool defaultBoolColumnValue, ReadOnlyCodeDescriptionPairList defaultValue)
			: this(name, category, caption, hint, storage, new CodeDescriptionBoolRegistryEditorInfo(boolColumnCaption), defaultBoolColumnValue, defaultValue)
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionBoolRegistryEditorInfo editorInfo, bool defaultBoolColumnValue, ReadOnlyCodeDescriptionPairList defaultValue)
			: this(name, category, caption, hint, storage, editorInfo, new CodeDescriptionBoolCollection(defaultValue, defaultBoolColumnValue))
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolCollection defaultValue)
			: this(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolRegistryDataType(), storage, defaultValue), editorInfo)
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionBoolRegistryEditorInfo editorInfo, RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter)
			: this(new RegistryItemImplWithDynamicDefaultValue(name, category, caption, hint, new CodeDescriptionBoolRegistryDataType(), storage, defaultValueGetter), editorInfo)
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionBoolRegistryEditorInfo editorInfo, RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter)
			: this(new RegistryItemImplWithDynamicDefaultValue(name, category, caption, hint, new CodeDescriptionBoolRegistryDataType(), storage, options, defaultValueGetter), editorInfo)
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolCollection defaultValue)
			: this(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolRegistryDataType(), storage, options, defaultValue), editorInfo)
		{
		}

		public CodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolCollection defaultValue)
			: this(new RegistryItemImpl(name, category, caption, hint, dataType, storage, options, defaultValue), editorInfo)
		{
		}

		protected CodeDescriptionBoolRegistryItem(IRegistryItem item, IRegistryEditorInfo editorInfo)
			: base(item, editorInfo)
		{
		}

		public bool IncludeMissingDefaults { get; internal set; }
		public bool RemoveNonDefaults { get; internal set; }

		protected override object GetValueWithoutFallbackCore(System.Guid companyPK, System.Guid branchPK, System.Guid departmentPK)
		{
			var value = base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as CodeDescriptionBoolCollection;

			if (value != null)
			{
				if (DefaultValue != null)
				{
					value.InitializeFromDefaultValue(DefaultValue);
				}

				if (value.Count != 0 && (IncludeMissingDefaults || RemoveNonDefaults))
				{
					var defaults = DefaultValue;
					int initialCount = value.Count;
					if (IncludeMissingDefaults && defaults != null)
					{
						foreach (CodeDescriptionBool item in defaults)
						{
							if (!value.ContainsCode(item.Code))
							{
								value.Add(item.Code, item.Description, item.Bool);
							}
						}
					}

					if (RemoveNonDefaults && defaults != null)
					{
						for (int i = initialCount; --i >= 0;)
						{
							CodeDescriptionBool item = value[i];
							if (!defaults.ContainsCode(item.Code))
							{
								value.Remove(item);
							}
						}
					}
				}
			}

			return value;
		}

		public override int MaxLength
		{
			get { return 256; }
		}

		static public ZString BuildLogReference(BuildLogReferenceArgs args, string trueCaption, string falseCaption)
		{
			StringBuilder result = new StringBuilder();
			ICodeDescriptionBoolList originalList = args.OriginalValue as ICodeDescriptionBoolList;
			ICodeDescriptionBoolList newList = args.NewValue as ICodeDescriptionBoolList;
			if (originalList != null && newList != null)
			{
				for (int i = 0; i < originalList.Count; ++i)
				{
					var originalItem = originalList[i];
					var newItem = newList.Cast<ICodeDescriptionBool>().FirstOrDefault(x => x.Code == originalItem.Code);
					if (newItem != null && originalItem.Bool != newItem.Bool)
					{
						if (result.Length > 0)
						{
							result.AppendLine();
						}
						result.Append(newItem.Description + " " + (newItem.Bool ? trueCaption : falseCaption));
					}
				}
			}

			return result.ToString();
		}
	}

	class CodeDescriptionBoolRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionBoolCollection>
	{
		public CodeDescriptionBoolRegistryDataType()
		{
		}
	}
}
