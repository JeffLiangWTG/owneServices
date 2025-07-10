using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ResourceStrings.Business.Testing
{
	sealed class CustomizableDataCaptionSourceForTest : ICustomizableDataCaptionSource
	{
		public CustomizableDataCaptionSourceForTest(int maxLength)
		{
			MaxLength = maxLength;
		}

		public string Description
		{
			get { return "Customizable Data Test"; }
		}

		public IEnumerable<IResString> GetRuntimeCaptions(IResString initialValue, object context = null)
		{
			var runtimeCaptions = new string[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten" };
			var list = new List<IResString>(runtimeCaptions.Select(c => CustomizableDataResourceStrings.GetMultilingualString(this, null, c)));
			if (!string.IsNullOrEmpty(initialValue?.EnglishText) && Array.IndexOf(runtimeCaptions, initialValue.EnglishText) == -1)
			{
				list.Add(initialValue);
			}
			return list;
		}

		public string GetKey(object context, string caption)
		{
			return CustomizableDataResourceStrings.GetCustomizableDataKey("CDRSTest", caption);
		}

		public int MaxLength { get; }

		public IEnumerable<IResString> GetCompileTimeSystemCaptions()
		{
			throw new NotImplementedException();
		}

		public ushort Asmid
		{
			get
			{
				return ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId;
			}
			set
			{
				throw new NotImplementedException();
			}
		}
	}
}
