using System;
using System.Linq;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ParameterizedStringRegistryItem))]
	sealed class ParameterizedStringRegistryItemTest : StronglyTypedRegistryItemTestCase<ResourceString, ResourceString>
	{
		protected override StronglyTypedRegistryItem<ResourceString, ResourceString> GetNewRegistryItem()
		{
			return new ParameterizedStringRegistryItem("test", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.All, RegistryOptions.Default,
				ResString.GetMultilingualString("k", "Test {0}", ResString.GetMultilingualString("y", "Value")), ResString.GetMultilingualString("p", "parameter"));
		}

		public void TestStringValues()
		{
			var item = (ParameterizedStringRegistryItem)GetNewRegistryItem();
			AssertEquals("Test Value", (string)item.Value);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, item.Deserialise("{0} Testing"));
			AssertEquals("Value Testing", (string)item.Value);

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, item.Deserialise("Nothing"));
			AssertEquals("Nothing", (string)item.Value);

			//if the registry item value becomes null, then instead of crashing, we use default value
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, item.Deserialise(null));
			AssertEquals("Test Value", (string)item.Value);
		}

		public void TestCaptionSource()
		{
			var item = (ParameterizedStringRegistryItem)GetNewRegistryItem();
			var captionSource = (ITranslatableRegistryItemCaptionSource)item;
			AssertContainsExactElementsInAnyOrder(new[] { "Test {0}" }, item.GetCaptions(item.Value));
			AssertEquals("k", captionSource.GetKey(null, "Test {0}"));
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create("k", "Test {0}") }, new TranslatableRegistryItemValueCaptionSource(captionSource, item.Value).GetRuntimeCaptions(item.Value).Cast<ResourceString>().Select(o => Tuple.Create(o.ResourceKey, o.ToStringWithParameters(Res.DefaultLanguage))));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, item.Deserialise("Test Value"));
			AssertContainsExactElementsInAnyOrder(new[] { "Test Value" }, item.GetCaptions(item.Value));
			string key = CustomizableDataResourceStrings.GetCustomizableDataKey("R!test", "Test Value");
			AssertEquals(key, captionSource.GetKey(null, "Test Value"));
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(key, "Test Value") }, new TranslatableRegistryItemValueCaptionSource(captionSource, item.Value).GetRuntimeCaptions(item.Value).Cast<ResourceString>().Select(o => Tuple.Create(o.ResourceKey, o.ToStringWithParameters(Res.DefaultLanguage))));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, item.Deserialise("{0} Testing"));
			AssertContainsExactElementsInAnyOrder(new[] { "{0} Testing" }, item.GetCaptions(item.Value));
			key = CustomizableDataResourceStrings.GetCustomizableDataKey("R!test", "{0} Testing");
			AssertEquals(key, captionSource.GetKey(null, "{0} Testing"));
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(key, "{0} Testing") }, new TranslatableRegistryItemValueCaptionSource(captionSource, item.Value).GetRuntimeCaptions(item.Value).Cast<ResourceString>().Select(o => Tuple.Create(o.ResourceKey, o.ToStringWithParameters(Res.DefaultLanguage))));

			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, item.Deserialise("Nothing"));
			AssertContainsExactElementsInAnyOrder(new[] { "Nothing" }, item.GetCaptions(item.Value));
			key = CustomizableDataResourceStrings.GetCustomizableDataKey("R!test", "Nothing");
			AssertEquals(key, captionSource.GetKey(null, "Nothing"));
			AssertContainsExactElementsInAnyOrder(new[] { Tuple.Create(key, "Nothing") }, new TranslatableRegistryItemValueCaptionSource(captionSource, item.Value).GetRuntimeCaptions(item.Value).Cast<ResourceString>().Select(o => Tuple.Create(o.ResourceKey, o.ToStringWithParameters(Res.DefaultLanguage))));
		}

		public void TestToStringWithParametersWorksOnReconstructedObject()
		{
			var item = (ParameterizedStringRegistryItem)GetNewRegistryItem();
			var captionSource = (IRegistryItemCaptionSource)item;
			AssertEquals("Test {0}", ResString.GetMultilingualString(captionSource.GetKey(null, "Test {0}"), "Test {0}").ToStringWithParameters(Res.DefaultLanguage));
			AssertEquals("Test {0}", ResString.GetMultilingualString(captionSource.GetKey(null, "Test {0}"), "Test {0}").ToStringWithParameters(Enterprise.Core.SharedConstants.Languages.ChineseSimplified));
		}
	}
}
