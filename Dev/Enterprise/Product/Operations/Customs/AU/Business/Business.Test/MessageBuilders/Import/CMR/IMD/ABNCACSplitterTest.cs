using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ABNCACSplitterTest : TestCase
	{
		public void TestSplitByLength()
		{
			AssertSplittable(TestABN + TestCAC);
			AssertSplittable("     " + TestABN + TestCAC + "     ");
			AssertSplittable(TestABNWithSpaces + TestCACWithSpaces);
			AssertSplittable("     " + TestABNWithSpaces + TestCACWithSpaces + "     ");
		}

		public void TestSplitByDelimiter()
		{
			AssertSplittable(TestABN + "/" + TestCAC);
			AssertSplittable("   " + TestABN + "  / " + TestCAC + " ");
			AssertSplittable(TestABNWithSpaces + "/" + TestCACWithSpaces);
			AssertSplittable("   " + TestABNWithSpaces + "  / " + TestCACWithSpaces + " ");
		}

		public void TestSplitABNOnly()
		{
			AssertSplittableABNOnly(TestABN);
			AssertSplittableABNOnly("    " + TestABN + "    ");
			AssertSplittableABNOnly(TestABNWithSpaces);
			AssertSplittableABNOnly("    " + TestABNWithSpaces + "    ");
		}

		public void TestValidity()
		{
			AssertNotSplittable("foo");
			AssertNotSplittable("/");
			AssertNotSplittable("/aorh");
			AssertNotSplittable("1234567890aoeuidhtn");
			AssertNotSplittable("1234567890a/a");
			AssertNotSplittable(ZString.Empty);
		}

		void AssertNotSplittable(ZString stringToSplit)
		{
			ABNCACSplitter splitter = new ABNCACSplitter(stringToSplit);
			AssertEquals("isvalid when invalid: " + stringToSplit, false, splitter.IsValid);
			AssertEquals("abn when invalid: " + stringToSplit, ZString.Empty, splitter.ABN);
			AssertEquals("cac when invalid: " + stringToSplit, ZString.Empty, splitter.CAC);
		}

		void AssertSplittable(ZString stringToSplit)
		{
			ABNCACSplitter splitter = new ABNCACSplitter(stringToSplit);
			AssertEquals("IsValid when splittable: " + stringToSplit, true, splitter.IsValid);
			AssertEquals("abn when splittable: " + stringToSplit, TestABN, splitter.ABN);
			AssertEquals("cac when splittable: " + stringToSplit, TestCAC, splitter.CAC);
		}

		void AssertSplittableABNOnly(ZString stringToSplit)
		{
			ABNCACSplitter splitter = new ABNCACSplitter(stringToSplit);
			AssertEquals("IsValid when only one abn and splittable: " + stringToSplit, true, splitter.IsValid);
			AssertEquals("abn when only one abn and splittable: " + stringToSplit, TestABN, splitter.ABN);
			AssertEquals("cac when only one abn and splittable: " + stringToSplit, ZString.Empty, splitter.CAC);
		}

		const string TestABN = "123456789AB";
		const string TestABNWithSpaces = "123 456 789 AB";
		const string TestCAC = "CDE";
		const string TestCACWithSpaces = "CDE";
	}
}
