using System;
using System.Linq;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(MultilingualStringRegistryItem))]
	public class MultilingualStringRegistryItemTest : StronglyTypedRegistryItemTestCase<MultilingualString, string>
	{
		public void TestDefaultValueIsCarriedOver()
		{
			using (IMockResourceStringCache grmMockData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Russian).UseMockData())
			{
				grmMockData.Put("x", new ResourceStringData("x", "Preved"));
				string testing = "Testing";

				MultilingualStringRegistryItem item1 = new MultilingualStringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.All, ResString.GetMultilingualString("x", testing));
				AssertEquals("Preved", item1.DefaultValue.GetLocalizedValue(Enterprise.Core.SharedConstants.Languages.Russian));

				MultilingualStringRegistryItem item2 = new MultilingualStringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.All, RegistryOptions.Default, ResString.GetMultilingualString("x", testing));
				AssertEquals("Preved", item2.DefaultValue.GetLocalizedValue(Enterprise.Core.SharedConstants.Languages.Russian));
			}
		}

		public void TestNewMultilingualStringRegistryItem()
		{
			MultilingualStringRegistryItem stringRegistryItem = new MultilingualStringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", new StringRegistryDataType(CharacterCase.Upper), RegistryStorageFlags.Company, RegistryOptions.CannotCallParameterlessValueGetter);
			AssertEquals("Name", stringRegistryItem.Name);
			AssertEquals("Category", stringRegistryItem.Category);
			AssertEquals("Caption", stringRegistryItem.Caption);
			AssertEquals("Hint", stringRegistryItem.Hint);
			AssertEquals(CharacterCase.Upper, ((StringRegistryDataType)stringRegistryItem.DataType).CharacterCase);
			AssertEquals(RegistryStorageFlags.Company, stringRegistryItem.Storage);
			AssertEquals(RegistryOptions.CannotCallParameterlessValueGetter, stringRegistryItem.Options);
			AssertType(typeof(TextRegistryEditorInfo), stringRegistryItem.EditorInfo);
		}

		public void TestValueLocalization()
		{
			using (IMockResourceStringCache grmMockData = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.German).UseMockData())
			{
				grmMockData.Put("x", new ResourceStringData("x", "Prufung"));

				string testing = "Testing";

				MultilingualStringRegistryItem registryItem = new MultilingualStringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.All, ResString.GetMultilingualString("x", testing));
				AssertEquals("Testing", registryItem.Value);
				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
				{
					AssertEquals("Prufung", registryItem.Value);
				}
				AssertEquals("Testing", registryItem.Value);

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Testing ");
				AssertEquals("Testing", registryItem.Value);
				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
				{
					AssertEquals("Prufung", registryItem.Value);
				}

				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "foo");
				AssertEquals("foo", registryItem.Value);
				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
				{
					AssertEquals("foo", registryItem.Value);
				}

				string key = ((ResourceString)registryItem.Value).ResourceKey;
				grmMockData.Put(key, new ResourceStringData(key, "oof"));
				AssertEquals("foo", registryItem.Value);
				using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
				{
					AssertEquals("oof", registryItem.Value);
				}
			}
		}

		public void TestCaptionSource()
		{
			var x = ResString._GetMultilingualString(55, "x", "This is the default value");
			var registryItem = new MultilingualStringRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.All, x);
			var captionSource = (ITranslatableRegistryItemCaptionSource)registryItem;
			AssertEquals(55, captionSource.Asmid);
			AssertContainsExactElementsInAnyOrder(new MultilingualString[] { x }, captionSource.DefaultStrings);
			AssertEquals("Registry Item Caption", captionSource.Description);
			AssertContainsExactElementsInAnyOrder(new string[] { "something" }, captionSource.GetCaptions((NoResString)"something"));
			AssertContainsExactElementsInAnyOrder(new string[] { "something" }, captionSource.GetCaptions("something"));
			AssertContainsExactElementsInAnyOrder(new string[] { "something" }, new TranslatableRegistryItemValueCaptionSource(captionSource, (NoResString)"something").GetRuntimeCaptions().Select(res => res.ToString()));
			AssertContainsExactElementsInAnyOrder(new string[] { "something" }, captionSource.GetCaptions("something "));
			AssertEquals("x", CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "This is the default value").ResourceKey);
			AssertEquals(CustomizableDataResourceStrings.GetCustomizableDataKey("R!Name", "Some other value"), CustomizableDataResourceStrings.GetMultilingualString(captionSource, null, "Some other value").ResourceKey);
		}

		protected override StronglyTypedRegistryItem<MultilingualString, string> GetNewRegistryItem()
		{
			return new MultilingualStringRegistryItem("", null, null, null, RegistryStorageFlags.All);
		}

		protected override string ValidValue
		{
			get { return "This is a valid value"; }
		}
	}
}
