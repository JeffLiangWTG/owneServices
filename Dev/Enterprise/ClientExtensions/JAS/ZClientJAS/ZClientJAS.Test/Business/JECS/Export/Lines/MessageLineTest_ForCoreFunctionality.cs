using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class MessageLineTest_ForCoreFunctionality : TestCase
	{
		public void TestLineIdentifier()
		{
			AssertEquals("TESTLINE3100", Line.LineIdentifier);
			Line.TestLineType = JXCConstants.LineTypes.HEAD;
			AssertEquals("HEAD3100", Line.LineIdentifier);
			Line.TestLineType = JXCConstants.LineTypes.HAWB;
			AssertEquals("HAWB3100", Line.LineIdentifier);
		}

		public void TestLineAsString()
		{
			AssertEquals("TESTLINE3100;LineContent1;AnotherContent2;AnotherValue", Line.LineAsString);
			Line.Field1Value = "Smashing";
			AssertEquals("TESTLINE3100;Smashing;AnotherContent2;AnotherValue", Line.LineAsString);
			Line.Field2Value = "Whatever";
			Line.Field3Value = "Content";
			AssertEquals("TESTLINE3100;Smashing;Whatever;Content", Line.LineAsString);
		}

		public void TestLineAsString_NewlineCharactersAreReplacedWithSpaces()
		{
			AssertEquals("TESTLINE3100;LineContent1;AnotherContent2;AnotherValue", Line.LineAsString);
			Line.Field1Value = "Smashing\r\nWhatever\n Content\r HAHA\rMEH";
			AssertEquals("TESTLINE3100;Smashing Whatever  Content  HAHA MEH;AnotherContent2;AnotherValue", Line.LineAsString);
		}

		public void TestLineAsString_WhenFieldCountIsLessThan1()
		{
			Line.TestFieldCount = 0;
			AssertEquals("TESTLINE3100", Line.LineAsString);
			Line.TestFieldCount = 1;
			AssertEquals("TESTLINE3100;LineContent1", Line.LineAsString);
			Line.TestFieldCount = -1;
			AssertEquals("TESTLINE3100", Line.LineAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Line = new MessageLineForTest();
		}

		MessageLineForTest Line;
	}
}
