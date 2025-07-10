using System;
using System.Collections.Generic;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	public class TranslatableRegistryItemValueCaptionSource : ICustomizableDataCaptionSource
	{
		public TranslatableRegistryItemValueCaptionSource(ITranslatableRegistryItemCaptionSource source, object value)
		{
			this.source = source;
			this.value = value;
		}

		public ushort Asmid
		{
			get
			{
				return source.Asmid;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public string Description
		{
			get { return source.Description; }
		}

		public IEnumerable<IResString> GetCompileTimeSystemCaptions()
		{
			throw new NotImplementedException();
		}

		public IEnumerable<IResString> GetRuntimeCaptions(IResString userCaption = null, object context = null)
		{
			ResourceString result;
			foreach (var item in source.GetMultilingualCaptions(value))
			{
				if (item is ResourceString)
				{
					result = (ResourceString)item;
				}
				else
				{
					var defaultText = item.ToString(Res.DefaultLanguage);
					result = ResString._GetMultilingualString(source.Asmid, GetKey(null, defaultText), defaultText);
				}
				yield return result;
			}
		}

		public string GetKey(object context, string caption)
		{
			return source.GetKey(context, caption);
		}

		int ICustomizableDataCaptionSource.MaxLength
		{
			get { return source.MaxLength; }
		}

		public int GetMaxLength(ResourceString caption)
		{
			int result;
			var variantLengthCaptionSource = source as IRegistryItemVariantLengthCaptionSource;
			if (variantLengthCaptionSource != null)
			{
				result = variantLengthCaptionSource.GetMaxLength(caption);
			}
			else
			{
				result = source.MaxLength;
			}
			return result;
		}

		readonly ITranslatableRegistryItemCaptionSource source;
		readonly object value;
	}
}
