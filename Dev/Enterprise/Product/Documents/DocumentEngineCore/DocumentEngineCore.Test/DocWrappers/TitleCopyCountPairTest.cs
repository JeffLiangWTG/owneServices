using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	sealed class TitleCopyCountPairTest : TestCase
	{
		public void TestDefaultConstructor()
		{
			TitleCopyCountPair testPair = new TitleCopyCountPair();
			AssertEquals("Title", "", testPair.Title);
			AssertEquals("CopyCount", (short)1, testPair.CopyCount);
		}

		public void TestOneArgConstructor()
		{
			TitleCopyCountPair testPair = new TitleCopyCountPair("Boo");
			AssertEquals("Title", "Boo", testPair.Title);
			AssertEquals("CopyCount", (short)1, testPair.CopyCount);
		}

		public void TestTwoArgConstructor()
		{
			TitleCopyCountPair testPair = new TitleCopyCountPair("Boo", 42);
			AssertEquals("Title", "Boo", testPair.Title);
			AssertEquals("CopyCount", (short)42, testPair.CopyCount);
		}

		public void TestTitle()
		{
			TitleCopyCountPair testPair = new TitleCopyCountPair();
			testPair.Title = "test";
			AssertEquals("Title", "test", testPair.Title);

			testPair.Title = "another";
			AssertEquals("Title", "another", testPair.Title);
		}

		public void TestCopyCount()
		{
			TitleCopyCountPair testPair = new TitleCopyCountPair();
			testPair.CopyCount = 12;
			AssertEquals("CopyCount", (short)12, testPair.CopyCount);

			testPair.CopyCount = 99;
			AssertEquals("CopyCount", (short)99, testPair.CopyCount);
		}
	}
}
