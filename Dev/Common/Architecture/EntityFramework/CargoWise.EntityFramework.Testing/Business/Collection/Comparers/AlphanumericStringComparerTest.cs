using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class AlphanumericStringComparerTest : TestCaseWithFactory
	{
		#region IComparer Members

		public void TestCompare()
		{
			ZString testString1 = "2";
			ZString testString2 = "1";
			ZString testString3 = "20";
			ZString testString4 = "100";
			ZString testString5 = "33AA";
			ZString testString6 = "4B";

			List<ZString> list = new List<ZString> { testString6, testString4, testString2, testString5, testString1, testString3 };

			AlphanumericStringComparer comparer = new AlphanumericStringComparer();
			list.Sort(comparer);
			AssertArrayEqualsByElements(new[] { testString2, testString1, testString6, testString3, testString5, testString4 }, list.ToArray());
		}

		public void TestCompare_WithEmptyString()
		{
			ZString testString1 = "10";
			ZString testString2 = "2";
			ZString testString3 = "";

			List<ZString> list = new List<ZString> { testString2, testString1, testString3 };

			AlphanumericStringComparer comparer = new AlphanumericStringComparer();
			list.Sort(comparer);
			AssertArrayEqualsByElements(new[] { testString3, testString2, testString1 }, list.ToArray());
		}

		public void TestCompare_WithNullString()
		{
			ZString testString1 = "10";
			ZString testString2 = "2";
			ZString testString3 = null;

			List<ZString> list = new List<ZString> { testString2, testString1, testString3 };

			AlphanumericStringComparer comparer = new AlphanumericStringComparer();
			list.Sort(comparer);
			AssertArrayEqualsByElements(new[] { testString3, testString2, testString1 }, list.ToArray());
		}

		public void TestCompare_WithStringContainingLineBreaks()
		{
			ZString testString1 = "\r\n10";
			ZString testString2 = "\r\n2";
			ZString testString3 = "3\r\n";
			ZString testString4 = "A\r\n\r\n3\r\n";
			ZString testString5 = "A\r\n20\r\n\r\n";

			List<ZString> list = new List<ZString> { testString1, testString2, testString3, testString4, testString5 };

			AlphanumericStringComparer comparer = new AlphanumericStringComparer();
			list.Sort(comparer);
			AssertArrayEqualsByElements(new[] { testString2, testString1, testString3, testString5, testString4 }, list.ToArray());
		}

		public void TestCompare_WithStringContainingWhitespace()
		{
			ZString testString1 = " 10";
			ZString testString2 = " 2";
			ZString testString3 = "3 ";
			ZString testString4 = "A  3 ";
			ZString testString5 = "A 20  ";
			ZString testString6 = "A 10\t";

			List<ZString> list = new List<ZString> { testString1, testString2, testString3, testString4, testString5, testString6 };

			AlphanumericStringComparer comparer = new AlphanumericStringComparer();
			list.Sort(comparer);
			AssertArrayEqualsByElements(new[] { testString2, testString1, testString3, testString6, testString5, testString4 }, list.ToArray());
		}

		public void TestCompare_WithStringContainingNonASCIIChars()
		{
			ZString testString1 = "2睡觉";
			ZString testString2 = "10餐饮";
			ZString testString3 = "2식품";
			ZString testString4 = "10자다";
			ZString testString5 = "2नींद";
			ZString testString6 = "10भोजन";

			List<ZString> list = new List<ZString> { testString1, testString2, testString3, testString4, testString5, testString6 };

			AlphanumericStringComparer comparer = new AlphanumericStringComparer();
			list.Sort(comparer);

			AssertArrayEqualsByElements(new[] { testString5, testString1, testString3, testString6, testString2, testString4 }, list.ToArray());
		}

		[ExpectNoExceptions]
		public void TestComparerWithHugeNumber()
		{
			ZString testString1 = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			ZString testString2 = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567891";
			AlphanumericStringComparer comparer_ascending = new AlphanumericStringComparer();

			comparer_ascending.Compare(testString1, testString2);
		}

		#endregion
	}
}
