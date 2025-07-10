using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class AlphanumericStringPropertyComparerTest : TestCaseWithFactory
	{
		#region IComparer Members

		public void TestCompare()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy4 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy5 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy6 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Description = "2";
			dummy2.Z0_Description = "1";
			dummy3.Z0_Description = "20";
			dummy4.Z0_Description = "100";
			dummy5.Z0_Description = "33AA";
			dummy6.Z0_Description = "4B";

			List<DummyBusinessObject> list = new List<DummyBusinessObject> { dummy6, dummy4, dummy2, dummy5, dummy1, dummy3 };

			AlphanumericStringPropertyComparer comparer_ascending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Ascending);
			list.Sort(comparer_ascending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy2, dummy1, dummy6, dummy3, dummy5, dummy4 }, list);

			AlphanumericStringPropertyComparer comparer_descending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Descending);
			list.Sort(comparer_descending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy4, dummy5, dummy3, dummy6, dummy1, dummy2 }, list);
		}

		public void TestCompare_WithEmptyString()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Description = "10";
			dummy2.Z0_Description = "2";
			dummy3.Z0_Description = "";

			List<DummyBusinessObject> list = new List<DummyBusinessObject> { dummy2, dummy1, dummy3 };

			AlphanumericStringPropertyComparer comparer_ascending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Ascending);
			list.Sort(comparer_ascending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy3, dummy2, dummy1 }, list);

			AlphanumericStringPropertyComparer comparer_descending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Descending);
			list.Sort(comparer_descending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy1, dummy2, dummy3 }, list);
		}

		public void TestCompare_WithNullString()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Description = "10";
			dummy2.Z0_Description = "2";
			dummy3.Z0_Description = null;

			List<DummyBusinessObject> list = new List<DummyBusinessObject> { dummy2, dummy1, dummy3 };

			AlphanumericStringPropertyComparer comparer_ascending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Ascending);
			list.Sort(comparer_ascending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy3, dummy2, dummy1 }, list);

			AlphanumericStringPropertyComparer comparer_descending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Descending);
			list.Sort(comparer_descending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy1, dummy2, dummy3 }, list);
		}

		public void TestCompare_WithStringContainingLineBreaks()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy4 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy5 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Description = "\r\n10";
			dummy2.Z0_Description = "\r\n2";
			dummy3.Z0_Description = "3\r\n";
			dummy4.Z0_Description = "A\r\n\r\n3\r\n";
			dummy5.Z0_Description = "A\r\n20\r\n\r\n";

			List<DummyBusinessObject> list = new List<DummyBusinessObject> { dummy1, dummy2, dummy3, dummy4, dummy5 };

			AlphanumericStringPropertyComparer comparer_ascending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Ascending);
			list.Sort(comparer_ascending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy2, dummy1, dummy3, dummy5, dummy4 }, list);

			AlphanumericStringPropertyComparer comparer_descending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Descending);
			list.Sort(comparer_descending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy4, dummy5, dummy3, dummy1, dummy2 }, list);
		}

		public void TestCompare_WithStringContainingWhitespace()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy4 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy5 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy6 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Description = " 10";
			dummy2.Z0_Description = " 2";
			dummy3.Z0_Description = "3 ";
			dummy4.Z0_Description = "A  3 ";
			dummy5.Z0_Description = "A 20  ";
			dummy6.Z0_Description = "A 10\t";

			List<DummyBusinessObject> list = new List<DummyBusinessObject> { dummy1, dummy2, dummy3, dummy4, dummy5, dummy6 };

			AlphanumericStringPropertyComparer comparer_ascending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Ascending);
			list.Sort(comparer_ascending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy2, dummy1, dummy3, dummy6, dummy5, dummy4 }, list);

			AlphanumericStringPropertyComparer comparer_descending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Descending);
			list.Sort(comparer_descending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy4, dummy5, dummy6, dummy3, dummy1, dummy2 }, list);
		}

		public void TestCompare_WithStringContainingNonASCIIChars()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy4 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy5 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy6 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Description = "2睡觉";
			dummy2.Z0_Description = "10餐饮";
			dummy3.Z0_Description = "2식품";
			dummy4.Z0_Description = "10자다";
			dummy5.Z0_Description = "2नींद";
			dummy6.Z0_Description = "10भोजन";

			List<DummyBusinessObject> list = new List<DummyBusinessObject> { dummy1, dummy2, dummy3, dummy4, dummy5, dummy6 };

			AlphanumericStringPropertyComparer comparer_ascending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Ascending);
			list.Sort(comparer_ascending);

			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy5, dummy1, dummy3, dummy6, dummy2, dummy4 }, list);

			AlphanumericStringPropertyComparer comparer_descending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Descending);
			list.Sort(comparer_descending);
			AssertDummyBizoCollectionEqualsByElementDescription(new[] { dummy4, dummy2, dummy6, dummy3, dummy1, dummy5 }, list);
		}

		[ExpectNoExceptions]
		public void TestComparerWithHugeNumber()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Description = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			dummy2.Z0_Description = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567891";
			AlphanumericStringPropertyComparer comparer_ascending = new AlphanumericStringPropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description.Name, ListSortDirection.Ascending);

			comparer_ascending.Compare(dummy1, dummy2);
		}

		#endregion

		#region Implementation

		public void AssertDummyBizoCollectionEqualsByElementDescription(IEnumerable<DummyBusinessObject> expected, IEnumerable<DummyBusinessObject> actual)
		{
			AssertDummyBizoCollectionEqualsByElementDescription("", expected, actual);
		}

		public void AssertDummyBizoCollectionEqualsByElementDescription(string message, IEnumerable<DummyBusinessObject> expected, IEnumerable<DummyBusinessObject> actual)
		{
			AssertArrayEqualsByElements(message, expected.Select(e => e.Z0_Description).ToArray(), actual.Select(e => e.Z0_Description).ToArray());
		}

		#endregion
	}
}
