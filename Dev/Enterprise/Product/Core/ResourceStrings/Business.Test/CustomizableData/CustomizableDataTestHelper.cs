using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business.Testing
{
	public class CustomizableDataTestHelper : Disposable
	{
		public CustomizableDataTestHelper(int sourceMaxLength = 50)
		{
			mockSources = ResourceStringsFactory.MockSources();
			EN = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.English);
			ZH_CN = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.ChineseSimplified);
			FR_FR = ResourceStringsFactory.GetMockSource(Core.SharedConstants.Languages.French);

			customizableDataResourceStrings = new CustomizableDataResourceStrings(new CustomizableDataCaptionSourceForTest(sourceMaxLength));
			var englishNumbers = customizableDataResourceStrings.Source.GetRuntimeCaptions().ToArray();
			for (int i = 0; i < englishNumbers.Length; i++)
			{
				var key = englishNumbers[i].ResourceKey;
				EN.Put(key, new ResourceStringData(key, (ResourceString)englishNumbers[i]));
				ZH_CN.Put(key, new ResourceStringData(key, ChineseNumbers[i]));
				FR_FR.Put(key, new ResourceStringData(key, FrenchNumbers[i]));
			}
		}

		public ResourceString GetMultilingualString(string caption)
		{
			return CustomizableDataResourceStrings.GetMultilingualString(null, caption);
		}

		protected override void Dispose(bool isDisposing)
		{
			mockSources.Dispose();
		}

		public readonly string[] ChineseNumbers = new string[] { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十" };
		public readonly string[] FrenchNumbers = new string[] { "zéro", "un", "deux", "trois", "quatre", "cinq", "six", "sept", "huit", "neuf", "dix" };

		public CustomizableDataResourceStrings CustomizableDataResourceStrings
		{
			get { return customizableDataResourceStrings; }
		}
		readonly CustomizableDataResourceStrings customizableDataResourceStrings;

		readonly IDisposable mockSources;
		protected IMockResourceStringCache EN;
		protected IMockResourceStringCache ZH_CN;
		protected IMockResourceStringCache FR_FR;
	}
}
