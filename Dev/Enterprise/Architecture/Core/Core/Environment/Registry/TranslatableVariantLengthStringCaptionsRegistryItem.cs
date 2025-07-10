using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public abstract class TranslatableVariantLengthStringCaptionsRegistryItem<TGet, TSet> : TranslatableRegistryItem<TGet, TSet>
		, IRegistryItemVariantLengthCaptionSource
	{
		public TranslatableVariantLengthStringCaptionsRegistryItem(IRegistryItem inner)
			: base(inner)
		{ }

		int IRegistryItemVariantLengthCaptionSource.GetMaxLength(string caption)
		{
			int result = 0;

			foreach (var property in StringCaptionProperties)
			{
				if (property.Value.Equals(caption))
				{
					result = property.MaxLength;
					break;
				}
			}

			if (result == 0)
			{
				result = MaxLength;
			}

			return result;
		}

		public override IEnumerable<string> GetCaptions(TGet value)
		{
			foreach (var property in StringCaptionProperties)
			{
				yield return property.Value;
			}
		}

		public override int MaxLength
		{
			get
			{
				int result = -1;
				var captionWithLongestMaxLength = StringCaptionProperties.OrderByDescending(x => x.MaxLength).FirstOrDefault();
				if (captionWithLongestMaxLength != null)
				{
					result = captionWithLongestMaxLength.MaxLength;
				}
				return result;
			}
		}

		protected abstract IEnumerable<ZPropertyInfoString> StringCaptionProperties { get; }
	}
}
