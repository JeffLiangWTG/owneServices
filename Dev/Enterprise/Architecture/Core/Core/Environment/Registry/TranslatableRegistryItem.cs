using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class TranslatableRegistryItem<TGet, TSet> : StronglyTypedRegistryItem<TGet, TSet>, ITranslatableRegistryItemCaptionSource, IRegistryItemCaptionSource
	{
		public TranslatableRegistryItem(IRegistryItem inner)
			: base(inner)
		{ }

		string ICustomizableDataCaptionSource.Description
		{
			get { return Res.GetString("f1ab535c-e22f-4282-86be-6a43ab96f1f7", "Registry Item {0}", Caption); }
		}

		public virtual string GetKey(object context, string caption)
		{
			DefaultStringsReverseLookup.TryGetValue(caption, out ResourceString defaultString);
			return defaultString != null ? defaultString.ResourceKey : CustomizableDataResourceStrings.GetCustomizableDataKey(ResourceStringsKeyPrefix, caption); // Resource string key prefix
		}

		public virtual string ResourceStringsKeyPrefix
		{
			get { return "R!" + Name; } // This is a resource string key prefix
		}

		ushort ICustomizableDataCaptionSource.Asmid
		{
			get
			{
				var oneDefaultString = DefaultStrings.FirstOrDefault();
				return oneDefaultString != null ? oneDefaultString.Asmid : ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId;
			}
			set
			{ }
		}

		IEnumerable<IResString> ICustomizableDataCaptionSource.GetCompileTimeSystemCaptions()
		{
			throw new NotImplementedException();
		}

		IEnumerable<IResString> ICustomizableDataCaptionSource.GetRuntimeCaptions(IResString userCaption, object context)
		{
			throw new NotImplementedException();
		}

		protected CustomizableDataResourceStrings CustomizableDataResourceStrings
		{
			get { return new CustomizableDataResourceStrings(this); }
		}

		protected MultilingualString GetMultilingualString(string caption)
		{
			caption = caption.TrimEnd();
			if (string.IsNullOrEmpty(caption))
			{
				return (NoResString)"";
			}
			else
			{
				ResourceString defaultString;
				if (DefaultStringsReverseLookup.TryGetValue(caption, out defaultString))
				{
					return defaultString;
				}
				else
				{
					return CustomizableDataResourceStrings.GetMultilingualString(null, caption);
				}
			}
		}

		Dictionary<string, ResourceString> DefaultStringsReverseLookup
		{
			get
			{
				if (defaultStringsReverseLookup == null)
				{
					// Note: Must be thread safe. Doesn't matter if two threads race to set the dictionary, so no locking is used.
					// Reference updates are guaranteed to be atomic in the language spec.
					var threadLocalDictionary = new Dictionary<string, ResourceString>();
					foreach (var resString in DefaultStrings)
					{
						var key = resString.GetUnresolvedString();
						ResourceString other;
						if (!threadLocalDictionary.TryGetValue(key, out other) || other.ResourceKey != resString.ResourceKey)
						{
							threadLocalDictionary[key] = resString;
						}
					}

					defaultStringsReverseLookup = threadLocalDictionary;
				}
				return defaultStringsReverseLookup;
			}
		}

		Dictionary<string, ResourceString> defaultStringsReverseLookup;

		public abstract bool IsTranslatable { get; }

		public abstract IEnumerable<ResourceString> DefaultStrings { get; }

		public abstract IEnumerable<string> GetCaptions(TGet value);

		public abstract int MaxLength { get; }

		public IEnumerable<string> GetCaptions(object value)
		{
			if (value is string)
			{
				value = ((string)value).TrimEnd();
			}

			if (value is TSet)
			{
				value = Convert((TSet)value);
			}
			return GetCaptions((TGet)value);
		}

		public IEnumerable<MultilingualString> GetMultilingualCaptions(object value)
		{
			var stringValue = value as string;
			if (stringValue != null)
			{
				value = stringValue.TrimEnd();
			}

			if (value is TSet)
			{
				value = Convert((TSet)value);
			}
			return GetMultilingualCaptions((TGet)value);
		}

		protected virtual IEnumerable<MultilingualString> GetMultilingualCaptions(TGet value)
		{
			foreach (var item in GetCaptions(value))
			{
				yield return (NoResString)item;
			}
		}
	}
}
