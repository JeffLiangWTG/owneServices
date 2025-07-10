using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class ArrayToTextConverterTest : TestCase
	{
		public void TestConvertBusinessObjectCollection()
		{
			PKDescriptionCollection collection = new PKDescriptionCollection();
			collection.AddNew().Description = "Item 1";
			collection.AddNew().Description = "";
			collection.AddNew().Description = "Item 2";

			AssertEquals("Item 1, Item 2", ArrayToTextConverter.ConvertToCommaSeparatedText(collection, "Description"));
			AssertEquals(string.Format("Item 1, {0}Item 2", System.Environment.NewLine), ArrayToTextConverter.ConvertToCommaSeparatedMultilineText(collection, "Description"));
		}

		public void TestConvertStringArray()
		{
			AssertEquals("Item 1, Item 2", ArrayToTextConverter.ConvertToCommaSeparatedText("Item 1", "", null, "Item 2"));
			AssertEquals(string.Format("Item 1, {0}Item 2", System.Environment.NewLine), ArrayToTextConverter.ConvertToCommaSeparatedMultilineText("Item 1", "", null, "Item 2"));
		}
	}
}
