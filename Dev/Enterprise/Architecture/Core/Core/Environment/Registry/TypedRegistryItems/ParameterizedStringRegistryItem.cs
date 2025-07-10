using System;
using System.Collections.Generic;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class ParameterizedStringRegistryItem : TranslatableRegistryItem<ResourceString, ResourceString>
	{
		public ParameterizedStringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ResourceString defaultValue, params MultilingualString[] parameterDescriptions)
			: base(new RegistryItemImpl(name, category, caption, hint, new ParameterizedStringDataType(null, defaultValue), new ParameterizedStringRegistryItemEditorInfo(parameterDescriptions), storage, options, defaultValue))
		{
			DataType = new ParameterizedStringDataType(this, defaultValue);
			ValidateParameterDescriptions();
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		public override IEnumerable<string> GetCaptions(ResourceString value)
		{
			yield return value.ToStringWithParameters(Res.DefaultLanguage);
		}

		public override int MaxLength
		{
			get { return int.MaxValue; }
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get { return DefaultValue != null ? new ResourceString[] { DefaultValue } : Array.Empty<ResourceString>(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1178:DoNotInvokeOldResGetStringMethodsAnalyzer", Justification = "Used for core functionality")]
		public ResourceString Deserialise(string caption)
		{
			ResourceString value;
			if (caption == DefaultValue.ToStringWithParameters(Res.DefaultLanguage) || caption == null)
			{
				value = DefaultValue;
			}
			else
			{
				value = CustomizableDataResourceStrings.GetMultilingualString(null, caption);
				value = ResString._GetMultilingualString(value.Asmid, value.ResourceKey, value.GetUnresolvedString(), DefaultValue.Parameters);
			}
			return value;
		}

		public override string GetKey(object context, string caption)
		{
			if (caption == DefaultValue.ToStringWithParameters(Res.DefaultLanguage))
			{
				return DefaultValue.ResourceKey;
			}
			else
			{
				return CargoWise.ResourceStrings.Cache.CustomizableDataResourceStrings.GetCustomizableDataKey(ResourceStringsKeyPrefix, caption);
			}
		}

		void ValidateParameterDescriptions()
		{
			if (DefaultValue.Parameters.Length != ((ParameterizedStringRegistryItemEditorInfo)EditorInfo).ParameterDescriptions.Count)
			{
				throw new InvalidOperationException("Expected the number of parameter descriptions to match the number of string parameters (" + DefaultValue.Parameters.Length + ")");
			}
		}
	}
}
