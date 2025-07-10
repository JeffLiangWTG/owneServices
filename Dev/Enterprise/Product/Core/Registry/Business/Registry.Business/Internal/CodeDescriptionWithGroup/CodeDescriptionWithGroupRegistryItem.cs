using System.Linq;
using System.Text;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionWithGroupRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CodeDescriptionWithGroupCollection, CodeDescriptionWithGroupCollection>
	{
		public CodeDescriptionWithGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString groupColumnCaption)
			: this(name, category, caption, hint, storage, groupColumnCaption, ZString.Empty)
		{
		}

		public CodeDescriptionWithGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionWithGroupRegistryEditorInfo editorInfo)
			: this(name, category, caption, hint, storage, editorInfo, new CodeDescriptionWithGroupCollection())
		{
		}

		public CodeDescriptionWithGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString groupColumnCaption, string defaultGroupColumnValue)
			: this(name, category, caption, hint, storage, groupColumnCaption, new CodeDescriptionWithGroupCollection(new ReadOnlyCodeDescriptionPairList(), defaultGroupColumnValue))
		{
		}

		public CodeDescriptionWithGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString groupColumnCaption, CodeDescriptionWithGroupCollection defaultValue)
			: this(name, category, caption, hint, storage, new CodeDescriptionWithGroupRegistryEditorInfo(groupColumnCaption), defaultValue)
		{
		}

		public CodeDescriptionWithGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionWithGroupRegistryEditorInfo editorInfo, CodeDescriptionWithGroupCollection defaultValue)
			: this(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionWithGroupRegistryDataType(), storage, defaultValue), editorInfo)
		{
		}

		public CodeDescriptionWithGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionWithGroupRegistryEditorInfo editorInfo, RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter)
			: this(new RegistryItemImplWithDynamicDefaultValue(name, category, caption, hint, new CodeDescriptionWithGroupRegistryDataType(), storage, defaultValueGetter), editorInfo)
		{
		}

		public CodeDescriptionWithGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionWithGroupRegistryEditorInfo editorInfo, RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter defaultValueGetter)
			: this(new RegistryItemImplWithDynamicDefaultValue(name, category, caption, hint, new CodeDescriptionWithGroupRegistryDataType(), storage, options, defaultValueGetter), editorInfo)
		{
		}

		public CodeDescriptionWithGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionWithGroupRegistryEditorInfo editorInfo, CodeDescriptionWithGroupCollection defaultValue,
			bool includeMissingDefaults, bool removeNonDefaults)
			: this(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionWithGroupRegistryDataType(), storage, defaultValue), editorInfo)
		{
			IncludeMissingDefaults = includeMissingDefaults;
			RemoveNonDefaults = removeNonDefaults;
		}

		public CodeDescriptionWithGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionWithGroupRegistryEditorInfo editorInfo, CodeDescriptionWithGroupCollection defaultValue)
			: this(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionWithGroupRegistryDataType(), storage, options, defaultValue), editorInfo)
		{
		}

		protected CodeDescriptionWithGroupRegistryItem(IRegistryItem item, IRegistryEditorInfo editorInfo)
			: base(item, editorInfo)
		{
		}

		public bool IncludeMissingDefaults { get; internal set; }
		public bool RemoveNonDefaults { get; internal set; }

		protected override object GetValueWithoutFallbackCore(System.Guid companyPK, System.Guid branchPK, System.Guid departmentPK)
		{
			var baseValue = base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK) as CodeDescriptionWithGroupCollection;
			baseValue.CodeMaxLength = DefaultValue?.CodeMaxLength ?? 0;

			var value = new CodeDescriptionWithGroupCollection(baseValue);

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
				if (value.DefaultGroupForNewChild.IsEmpty)
				{
					value.DefaultGroupForNewChild = defaults.DefaultGroupForNewChild;
				}

				int initialCount = value.Count;
				if (initialCount != 0 && (IncludeMissingDefaults || RemoveNonDefaults))
				{
					if (IncludeMissingDefaults)
					{
						foreach (CodeDescriptionWithGroup item in defaults)
						{
							if (!value.ContainsCode(item.Code))
							{
								value.Add(item.Code, item.Description, item.Group);
							}
						}
					}

					if (RemoveNonDefaults)
					{
						for (int i = initialCount; --i >= 0;)
						{
							CodeDescriptionWithGroup item = value[i];
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

		public override int MaxLength => 256;

		static public ZString BuildLogReference(BuildLogReferenceArgs args)
		{
			StringBuilder result = new StringBuilder();
			ICodeDescriptionWithGroupList originalList = args.OriginalValue as ICodeDescriptionWithGroupList;
			ICodeDescriptionWithGroupList newList = args.NewValue as ICodeDescriptionWithGroupList;
			if (originalList != null && newList != null)
			{
				for (int i = 0; i < originalList.Count; ++i)
				{
					var originalItem = originalList[i];
					var newItem = newList.Cast<ICodeDescriptionWithGroup>().FirstOrDefault(x => x.Code == originalItem.Code);
					if (newItem != null && originalItem.Group != newItem.Group)
					{
						if (result.Length > 0)
						{
							result.AppendLine();
						}
						result.Append(newItem.Description + " " + newItem.Group);
					}
				}
			}

			return result.ToString();
		}
	}

	class CodeDescriptionWithGroupRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionWithGroupCollection>
	{
		public CodeDescriptionWithGroupRegistryDataType()
		{
		}
	}
}
