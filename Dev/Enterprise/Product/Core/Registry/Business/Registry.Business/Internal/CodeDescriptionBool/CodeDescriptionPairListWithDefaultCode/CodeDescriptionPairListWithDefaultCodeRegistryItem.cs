using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class CodeDescriptionPairListWithDefaultCodeRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<ICodeDescriptionPairListWithDefaultCode, SystemDefinableCodeDescriptionBoolCollection>, ICodeDescriptionPairListProvider
	{
		public CodeDescriptionPairListWithDefaultCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, bool useDefaultFlag)
			: this(name, category, caption, hint, storage, null, useDefaultFlag)
		{
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ReadOnlyCodeDescriptionPairList defaultValue, bool useDefaultFlag)
			: this(name, category, caption, hint, storage, defaultValue, false, useDefaultFlag)
		{
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ReadOnlyCodeDescriptionPairList defaultValue, bool canEditDefaultValue, bool useDefaultFlag)
			: this(name, category, caption, hint, storage, defaultValue, canEditDefaultValue, useDefaultFlag, 0)
		{
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ReadOnlyCodeDescriptionPairList defaultValue, bool canEditDefaultValue, bool useDefaultFlag, int codeMaxLength)
			: this(name, new[] { category }, caption, hint, storage, defaultValue, canEditDefaultValue, useDefaultFlag, codeMaxLength)
		{
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ReadOnlyCodeDescriptionPairList defaultValue, bool canEditDefaultValue, bool useDefaultFlag, int codeMaxLength)
			: this(name, categories, caption, hint, storage, RegistryOptions.Default, defaultValue, canEditDefaultValue, useDefaultFlag, codeMaxLength)
		{
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool canEditDefaultValue, bool useDefaultFlag, int codeMaxLength)
			: base(new RegistryItemImpl(name, caption, hint, GetDataType(defaultValue, canEditDefaultValue, codeMaxLength), new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("bc0a2422-ba36-4cf9-89c7-c144044c37ed", "Default"), useDefaultFlag), storage, options, GetDefaultValue(defaultValue, canEditDefaultValue, codeMaxLength), false, categories), new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("bc0a2422-ba36-4cf9-89c7-c144044c37ed", "Default"), useDefaultFlag))
		{
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool useDefaultFlag)
			: this(name, category, caption, hint, storage, options, defaultValue, false, useDefaultFlag)
		{
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool canEditDefaultValue, bool useDefaultFlag)
			: this(name, category, caption, hint, storage, options, defaultValue, canEditDefaultValue, useDefaultFlag, 0)
		{
		}

		public CodeDescriptionPairListWithDefaultCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, bool canEditDefaultValue, bool useDefaultFlag, int codeMaxLength, bool isDefaultValueMandatory = true)
			: base(new RegistryItemImpl(name, category, caption, hint, GetDataType(defaultValue, canEditDefaultValue, codeMaxLength, isDefaultValueMandatory), storage, options, GetDefaultValue(defaultValue, canEditDefaultValue, codeMaxLength)), new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("bc0a2422-ba36-4cf9-89c7-c144044c37ed", "Default"), useDefaultFlag))
		{
		}

		static CodeDescriptionPairListWithDefaultCodeRegistryDataType GetDataType(ReadOnlyCodeDescriptionPairList list, bool canEdit, int maxLength, bool isDefaultValueMandatory = true)
		{
			ReadOnlyCodeDescriptionPairList systemDefinedList = canEdit ? null : list;
			return new CodeDescriptionPairListWithDefaultCodeRegistryDataType(systemDefinedList, maxLength, isDefaultValueMandatory);
		}

		static SystemDefinableCodeDescriptionBoolCollection GetDefaultValue(ReadOnlyCodeDescriptionPairList list, bool canEdit, int maxLength)
		{
			SystemDefinableCodeDescriptionBoolCollection result;
			if (list != null)
			{
				result = new SystemDefinableCodeDescriptionBoolCollection(maxLength, list, canEdit);
				result.SetDefaultCode(list.DefaultCode, false);
			}
			else
			{
				result = new SystemDefinableCodeDescriptionBoolCollection(maxLength);
			}
			return result;
		}

		public override int MaxLength
		{
			get { return 256; }
		}

		public override IEnumerable<string> GetCaptions(ICodeDescriptionPairListWithDefaultCode value)
		{
			foreach (RegistryBusinessObject item in value)
			{
				yield return item.EnglishDescription;
			}
		}

		#region ICodeDescriptionPairListProvider

		CodeDescriptionPairList ICodeDescriptionPairListProvider.CodeDescriptionPairList
		{
			get { return this.Value.GetCodeDescriptionPairList(); }
		}

		#endregion
	}

	internal class CodeDescriptionPairListWithDefaultCodeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SystemDefinableCodeDescriptionBoolCollection>
	{
		public CodeDescriptionPairListWithDefaultCodeRegistryDataType(ReadOnlyCodeDescriptionPairList systemDefinedList, int codeMaxLength, bool isDefaultValueMandatory = true)
		{
			this.SystemDefinedList = systemDefinedList;
			this.CodeMaxLength = codeMaxLength;
			this.isDefaultValueMandatory = isDefaultValueMandatory;
		}

		protected override SystemDefinableCodeDescriptionBoolCollection DeserialiseCore(byte[] value)
		{
			SystemDefinableCodeDescriptionBoolCollection result = new SystemDefinableCodeDescriptionBoolCollection(CodeMaxLength, SystemDefinedList, false);
			result.Populate(value);
			return result;
		}

		protected override void ValidateCore(IRegistryItem registryItem, SystemDefinableCodeDescriptionBoolCollection proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (proposedValue.Count == 0)
			{
				throw new RegistryValidationException((NoResString)"Please enter something in the list.");
			}
			else if (proposedValue.DefaultElement == null && isDefaultValueMandatory)
			{
				throw new RegistryValidationException((NoResString)"Please select a default code.");
			}
		}

		public ReadOnlyCodeDescriptionPairList SystemDefinedList { get; }

		readonly int CodeMaxLength;
		readonly bool isDefaultValueMandatory;
	}
}
