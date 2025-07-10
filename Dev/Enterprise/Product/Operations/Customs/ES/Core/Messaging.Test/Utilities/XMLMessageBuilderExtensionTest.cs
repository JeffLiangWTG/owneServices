using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.Testing
{
	public class XMLMessageBuilderExtensionTest : TestCaseWithFactory
	{
		public void TestConvert()
		{
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("Convert returns an empty array when input collection is empty", System.Array.Empty<ZString>(), new List<ZString>() { }.Convert(ConvertMethodToTest));
				AssertArrayEqualsByElements("Convert returns a converted array when input collection is correct", expectedCollectionToTest, collectionToTest.Convert(ConvertMethodToTest));
			});
		}

		readonly IEnumerable<ZString> collectionToTest = new List<ZString>() { "A", "B", "C" };
		readonly ZString[] expectedCollectionToTest = new ZString[] { "AA", "BB", "CC" };

		ZString ConvertMethodToTest(ZString testString)
		{
			return testString + testString;
		}
	}
}
