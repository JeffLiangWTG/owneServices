using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebCustomsCssCollectionWrapper))]
	sealed class WebCustomsCssCollectionWrapperTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestAddDefaultValues()
		{
			var inUrlsButNotInValues = new WebTrackerCustomCss("inUrlsButNotInValues", "inUrlsButNotInValues");
			var inValuesButNotInUrls = new WebTrackerCustomCss("inValuesButNotInUrls", "inValuesButNotInUrls");
			var inBothValuesAndUrls = new WebTrackerCustomCss("inBothValuesAndUrls", "inBothValuesAndUrls");

			var wrapper = new WebCustomsCssCollectionWrapper(new[] { inUrlsButNotInValues.Url, inBothValuesAndUrls.Url }, new[] { inValuesButNotInUrls, inBothValuesAndUrls });

			AssertEquals("Should be three - The two from values + other from urls", 4, wrapper.Collection.Count);
			AssertEquals("string.Empty should always be present", WebCustomCssBusinessObject.DefaultDataValue, wrapper.Collection[string.Empty].Data);
			AssertEquals("When the item wasn't in the array, it should use the default's value", WebCustomCssBusinessObject.DefaultDataValue, wrapper.Collection[inUrlsButNotInValues.Url].Data);
			AssertEquals("When the item was in the array, it should use the provided value", inBothValuesAndUrls.Data, wrapper.Collection[inBothValuesAndUrls.Url].Data);
			AssertEquals("The given items should always be added, even if they arent in the defaults", inValuesButNotInUrls.Data, wrapper.Collection[inValuesButNotInUrls.Url].Data);
		}

		public void TestToWebTrackerCustomCssArray()
		{
			var wrapper = new WebCustomsCssCollectionWrapper(System.Array.Empty<string>(), new[] {
				new WebTrackerCustomCss("notempty", "notempty"),
				new WebTrackerCustomCss("empty", string.Empty),
				new WebTrackerCustomCss("defaultvalue", WebCustomCssControl.CssImportStatement)
			});

			var result = wrapper.ToWebTrackerCustomCssArray();
			AssertEquals("We should only export the non empty & non default values", 1, result.Length);
			AssertEquals("The correct one was identified as not empty", "notempty", result.Single().Url);
		}

		protected override BusinessObject GetNewBusinessObject() => new WebCustomsCssCollectionWrapper(new[] { "foo.com" }, new[] { new WebTrackerCustomCss("bar.com", "Some not empty value") });
	}
}
