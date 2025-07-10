using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocWrapperUtilitiesTest : TestCase
	{
		public void TestMultiLineStringAlreadyContainThisFullLine()
		{
			string existing = "Test1\nTest2\nTest3";
			AssertEquals(true, DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(existing, "Test1"));
			AssertEquals(true, DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(existing, "Test2"));
			AssertEquals(false, DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(existing, "Test4"));
			AssertEquals(true, DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(existing, "Test1\nTest2\nTest3"));
			AssertEquals(true, DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(existing, "Test1\nTest2"));

			existing = "Single Line";
			AssertEquals(true, DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(existing, "Single Line"));
			AssertEquals(false, DocWrapperUtilities.MultiLineStringAlreadyContainThisFullLine(existing, "Some Crap"));
		}

		public void TestToStringRounded() => CombineAssertions(() =>
		{
			AssertEquals("4 -> 4,00", "4,00", DocWrapperUtilities.ToStringRounded(4, 2));
			AssertEquals("4.562 -> 4,56", "4,56", DocWrapperUtilities.ToStringRounded(4.562, 2));
			AssertEquals("4.562 -> 5", "5", DocWrapperUtilities.ToStringRounded(4.562, 0));
		});
	}
}
