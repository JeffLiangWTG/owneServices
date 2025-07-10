using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.AddInfoTransformationBase.Testing
{
	sealed class AddInfoParserTest : TestCase
	{
		public void TestSimilarKeysDoNotGetAMatchOnEachOther()
		{
			string addInfoText = "CMRValueKey=ABC*ValueKey=XYZ";

			AddInfoParser cMRValueKeyParser = new AddInfoParser(addInfoText, "CMRValueKey");
			AssertEquals("CMRValueKeyParser.Value", "ABC", cMRValueKeyParser.Value);
			AssertEquals("CMRValueKeyParser.AddInfoMinusKeyAndValuePair", "ValueKey=XYZ", cMRValueKeyParser.AddInfoMinusKeyAndValuePair);

			AddInfoParser valueKeyParser = new AddInfoParser(addInfoText, "ValueKey");
			AssertEquals("ValueKeyParser.Value", "XYZ", valueKeyParser.Value);
			AssertEquals("ValueKeyParser.AddInfoMinusKeyAndValuePair", "CMRValueKey=ABC", valueKeyParser.AddInfoMinusKeyAndValuePair);
		}

		public void TestGetsValuesOutProperlyFromAllPartsOfAnAddInfo()
		{
			string addInfoText = "Value1=111*Value2=222*Value3=333*Value4=444";

			AddInfoParser parser1 = new AddInfoParser(addInfoText, "Value1");
			AssertEquals("Parser1.Value", "111", parser1.Value);
			AssertEquals("Parser1.AddInfoMinusKeyAndValuePair", "Value2=222*Value3=333*Value4=444", parser1.AddInfoMinusKeyAndValuePair);

			AddInfoParser parser2 = new AddInfoParser(addInfoText, "Value2");
			AssertEquals("Parser2.Value", "222", parser2.Value);
			AssertEquals("Parser2.AddInfoMinusKeyAndValuePair", "Value1=111*Value3=333*Value4=444", parser2.AddInfoMinusKeyAndValuePair);

			AddInfoParser parser3 = new AddInfoParser(addInfoText, "Value3");
			AssertEquals("Parser3.Value", "333", parser3.Value);
			AssertEquals("Parser3.AddInfoMinusKeyAndValuePair", "Value1=111*Value2=222*Value4=444", parser3.AddInfoMinusKeyAndValuePair);

			AddInfoParser parser4 = new AddInfoParser(addInfoText, "Value4");
			AssertEquals("Parser4.Value", "444", parser4.Value);
			AssertEquals("Parser4.AddInfoMinusKeyAndValuePair", "Value1=111*Value2=222*Value3=333", parser4.AddInfoMinusKeyAndValuePair);
		}

		public void TestParseWithEmpty()
		{
			AddInfoParser parser1 = new AddInfoParser("", "Value1");
			AssertEquals("Parser1.Value", "", parser1.Value);
			AssertEquals("Parser1.AddInfoMinusKeyAndValuePair", "", parser1.AddInfoMinusKeyAndValuePair);
		}

		public void TestParseWithSingleKey()
		{
			string addInfoText = "Value1=111";

			AddInfoParser parser1 = new AddInfoParser(addInfoText, "Value1");
			AssertEquals("Parser1.Value", "111", parser1.Value);
			AssertEquals("Parser1.AddInfoMinusKeyAndValuePair", "", parser1.AddInfoMinusKeyAndValuePair);

			parser1 = new AddInfoParser(addInfoText, "Value2");
			AssertEquals("Parser1.Value", "", parser1.Value);
			AssertEquals("Parser1.AddInfoMinusKeyAndValuePair", "Value1=111", parser1.AddInfoMinusKeyAndValuePair);
		}

		public void TestParseHandlesSingleQuoteForSQLByAddingQuoteEscapeCharacter()
		{
			string addInfoText = "Box29Text=UACU3398451 20' FCL";

			AddInfoParser parser1 = new AddInfoParser(addInfoText, "Value2");
			AssertEquals("Parser1.Value", "", parser1.Value);
			AssertEquals("Parser1.AddInfoMinusKeyAndValuePair returns value acceptable for sql statement", "Box29Text=UACU3398451 20'' FCL", parser1.AddInfoMinusKeyAndValuePair);
		}
	}
}
