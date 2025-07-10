using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class MultilingualStringRegistryItem : TranslatableRegistryItem<MultilingualString, string>
	{
		public MultilingualStringRegistryItem(IRegistryItem inner)
			: base(inner)
		{
		}

		public MultilingualStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, new StringRegistryDataType(), null, storage, RegistryOptions.Default, null, true)
		{
		}

		public MultilingualStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, RegistryStorageFlags storage)
			: this(name, category, caption, hint, dataType, null, storage, RegistryOptions.Default, null, true)
		{
		}

		public MultilingualStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, dataType, null, storage, options, null, true)
		{
		}

		public MultilingualStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, new StringRegistryDataType(), null, storage, options, null, true)
		{
		}

		public MultilingualStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MultilingualString defaultValue)
			: this(name, category, caption, hint, new StringRegistryDataType(), null, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public MultilingualStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, MultilingualString defaultValue)
			: this(name, category, caption, hint, new StringRegistryDataType(), null, storage, options, defaultValue)
		{
		}

		public MultilingualStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, TextRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, MultilingualString defaultValue)
			: base(new MultilingualStringRegistryItemImpl(name, category, caption, hint, dataType, editorInfo ?? new TextRegistryEditorInfo(TextEditorType.TextBox), storage, options, defaultValue != null ? defaultValue.GetUnresolvedString() : ""))
		{
			this.defaultValue = ValidateDefaultValue(defaultValue);
		}

		public MultilingualStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, TextRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, MultilingualString defaultValue, bool useDefaultDefaultValue)
			: base(new MultilingualStringRegistryItemImpl(name, category, caption, hint, dataType, editorInfo ?? new TextRegistryEditorInfo(TextEditorType.TextBox), storage, options, defaultValue != null ? defaultValue.GetUnresolvedString() : "", useDefaultDefaultValue))
		{
			this.defaultValue = ValidateDefaultValue(defaultValue);
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		protected override MultilingualString Convert(string value)
		{
			return GetMultilingualString(value);
		}

		public override int MaxLength
		{
			get { return ((StringRegistryDataType)Inner.DataType).MaxLength; }
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get { return defaultValue != null ? new ResourceString[] { defaultValue } : Array.Empty<ResourceString>(); }
		}

		static ResourceString ValidateDefaultValue(MultilingualString value)
		{
			var resString = value as ResourceString;
			if (resString != null && resString.Parameters != null && resString.Parameters.Length > 0)
			{
				throw new InvalidOperationException("Default resource string is parameterized. Do not use a paramterized default value or change to ParameterizedStringRegistryItem");
			}
			if (value != null && resString == null && !(value is NoResString))
			{
				throw new InvalidOperationException("Default resource string of type " + value.GetType() + " not supported");
			}
			return resString;
		}

		readonly ResourceString defaultValue;

		public override IEnumerable<string> GetCaptions(MultilingualString value)
		{
			yield return value.GetUnresolvedString();
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			if (newValue is MultilingualString)
			{
				newValue = ((MultilingualString)newValue).GetUnresolvedString();
			}
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}
	}
}
