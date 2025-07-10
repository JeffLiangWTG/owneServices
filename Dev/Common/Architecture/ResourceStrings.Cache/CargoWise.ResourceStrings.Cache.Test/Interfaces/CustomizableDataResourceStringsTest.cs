using System.Text;
using NUnit.Framework;

namespace CargoWise.ResourceStrings.Cache
{
	public class CustomizableDataResourceStringsTest : TestCase
	{
		public void TestGeneratesDifferentKeysForDifferentLongCaptions()
		{
			var sb = new StringBuilder();
			for (int i = 0; i < 10; i++)
			{
				sb.Append("abcdefghijklmnopqrstuvwxyz");
			}
			var caption1 = sb.ToString();

			sb.Append("1234567890");
			var caption2 = sb.ToString();

			AssertEquals("Should generate consistent key", CustomizableDataResourceStrings.GetCustomizableDataKey("Prefix", caption1), CustomizableDataResourceStrings.GetCustomizableDataKey("Prefix", caption1));
			AssertEquals("Should generate consistent key", CustomizableDataResourceStrings.GetCustomizableDataKey("Prefix", caption2), CustomizableDataResourceStrings.GetCustomizableDataKey("Prefix", caption2));
			AssertNotEquals("Should generate different keys for different captions", CustomizableDataResourceStrings.GetCustomizableDataKey("Prefix", caption1), CustomizableDataResourceStrings.GetCustomizableDataKey("Prefix", caption2));
		}
	}
}
