using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class VersionNumberTest : TestCase
	{
		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestInvalidMajorPart()
		{
			new VersionNumber(-1, 0, 0, 0);
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestInvalidMinorPart()
		{
			new VersionNumber(0, -1, 0, 0);
		}

		[ExpectException(typeof(ArgumentOutOfRangeException))]
		public void TestInvalidPatchPart()
		{
			new VersionNumber(0, 0, 0, -1);
		}

		public void TestConstructUsingInts()
		{
			VersionNumber versionNumber = new VersionNumber(1, 2, 3, 4);
			AssertEquals("Major", 1, versionNumber.Major);
			AssertEquals("Minor", 2, versionNumber.Minor);
			AssertEquals("Release", 3, versionNumber.Release);
			AssertEquals("Patch", 4, versionNumber.Patch);

			versionNumber = new VersionNumber(4, 3, 2, 1);
			AssertEquals("Major", 4, versionNumber.Major);
			AssertEquals("Minor", 3, versionNumber.Minor);
			AssertEquals("Release", 2, versionNumber.Release);
			AssertEquals("Patch", 1, versionNumber.Patch);
		}

		public void TestConstructUsingString()
		{
			VersionNumber versionNumber = new VersionNumber("1.2.3.4");
			AssertEquals("Major", 1, versionNumber.Major);
			AssertEquals("Minor", 2, versionNumber.Minor);
			AssertEquals("Release", 3, versionNumber.Release);
			AssertEquals("Patch", 4, versionNumber.Patch);

			versionNumber = new VersionNumber("4.3.2.1");
			AssertEquals("Major", 4, versionNumber.Major);
			AssertEquals("Minor", 3, versionNumber.Minor);
			AssertEquals("Release", 2, versionNumber.Release);
			AssertEquals("Patch", 1, versionNumber.Patch);
		}

		public void TestConstructUsingThreePartString()
		{
			VersionNumber versionNumber = new VersionNumber("1.2.3");
			AssertEquals("Major", 1, versionNumber.Major);
			AssertEquals("Minor", 2, versionNumber.Minor);
			AssertEquals("Release", 3, versionNumber.Release);
			AssertEquals("Patch", 0, versionNumber.Patch);

			versionNumber = new VersionNumber("4.3.2");
			AssertEquals("Major", 4, versionNumber.Major);
			AssertEquals("Minor", 3, versionNumber.Minor);
			AssertEquals("Release", 2, versionNumber.Release);
			AssertEquals("Patch", 0, versionNumber.Patch);
		}

		public void TestConstructUsingTwoPartString()
		{
			VersionNumber versionNumber = new VersionNumber("1.2");
			AssertEquals("Major", 1, versionNumber.Major);
			AssertEquals("Minor", 2, versionNumber.Minor);
			AssertEquals("Release", 0, versionNumber.Release);
			AssertEquals("Patch", 0, versionNumber.Patch);

			versionNumber = new VersionNumber("4.3");
			AssertEquals("Major", 4, versionNumber.Major);
			AssertEquals("Minor", 3, versionNumber.Minor);
			AssertEquals("Release", 0, versionNumber.Release);
			AssertEquals("Patch", 0, versionNumber.Patch);
		}

		public void TestConstructUsingOnePartString()
		{
			VersionNumber versionNumber = new VersionNumber("1");
			AssertEquals("Major", 1, versionNumber.Major);
			AssertEquals("Minor", 0, versionNumber.Minor);
			AssertEquals("Release", 0, versionNumber.Release);
			AssertEquals("Patch", 0, versionNumber.Patch);

			versionNumber = new VersionNumber("4");
			AssertEquals("Major", 4, versionNumber.Major);
			AssertEquals("Minor", 0, versionNumber.Minor);
			AssertEquals("Release", 0, versionNumber.Release);
			AssertEquals("Patch", 0, versionNumber.Patch);
		}

		public void TestIsEmpty()
		{
			AssertEquals("IsEmpty", true, new VersionNumber().IsEmpty);
			AssertEquals("IsEmpty", false, new VersionNumber(1, 0, 0, 0).IsEmpty);
		}

		public void TestEquals()
		{
			VersionNumber versionNumber = new VersionNumber(1, 2, 3, 4);
			AssertEquals("Equals()", false, versionNumber.Equals(null));
			AssertEquals("Equals()", false, versionNumber.Equals(""));
			AssertEquals("Equals()", false, versionNumber.Equals(new VersionNumber()));
			AssertEquals("Equals()", true, versionNumber.Equals(new VersionNumber(1, 2, 3, 4)));
		}

		public void TestGetReleaseDate()
		{
			AssertEquals("GetReleaseDate()", new DateTime(2000, 1, 2), new VersionNumber(1, 1, 1, 4).GetReleaseDate());
			AssertEquals("GetReleaseDate()", new DateTime(2001, 3, 2), new VersionNumber(1, 1, 426, 0).GetReleaseDate());
			AssertEquals("GetReleaseDate()", new DateTime(2001, 3, 2), new VersionNumber(1, 4, 426, 0).GetReleaseDate());
			AssertEquals("GetReleaseDate()", new DateTime(2013, 8, 9), new VersionNumber(2, 0, 1, 0).GetReleaseDate());
			AssertEquals("GetReleaseDate()", new DateTime(2013, 8, 18), new VersionNumber(2, 0, 10, 12).GetReleaseDate());
			AssertEquals("GetReleaseDate()", new DateTime(2013, 9, 8), new VersionNumber(2, 0, 31, 500).GetReleaseDate());
			AssertEquals("GetReleaseDate()", new DateTime(2014, 8, 8), new VersionNumber(2, 0, 365, 1000).GetReleaseDate());
			AssertEquals("GetReleaseDate()", new DateTime(2014, 3, 4), new VersionNumber(14, 3, 4, 0).GetReleaseDate());
			AssertEquals("GetReleaseDate()", new DateTime(2014, 3, 5), new VersionNumber(14, 3, 5, 16).GetReleaseDate());
			AssertEquals("GetReleaseDate()", new DateTime(2015, 10, 11), new VersionNumber(15, 10, 11, 4).GetReleaseDate());
		}

		public void TestGetHashCode()
		{
			int hashCode = new VersionNumber(1, 2, 3, 4).GetHashCode();
			AssertEquals("GetHashCode() must not change if VersionNumbers are equal.", hashCode, new VersionNumber(1, 2, 3, 4).GetHashCode());
		}

		public void TestToString()
		{
			AssertEquals("ToString()", "1.2.3.4", new VersionNumber(1, 2, 3, 4).ToString());
			AssertEquals("ToString()", "4.3.2.1", new VersionNumber(4, 3, 2, 1).ToString());
		}

		public void TestCompareTo()
		{
			VersionNumber versionNumber = new VersionNumber(1, 1, 1, 1);
			AssertEquals("Compare()", 0, versionNumber.CompareTo(new VersionNumber(1, 1, 1, 1)));
			AssertEquals("Compare()", 1, versionNumber.CompareTo(new VersionNumber(0, 1, 1, 1)));
			AssertEquals("Compare()", -1, versionNumber.CompareTo(new VersionNumber(2, 1, 1, 1)));
			AssertEquals("Compare()", 1, versionNumber.CompareTo(new VersionNumber(1, 0, 1, 1)));
			AssertEquals("Compare()", -1, versionNumber.CompareTo(new VersionNumber(1, 2, 1, 1)));
			AssertEquals("Compare()", 1, versionNumber.CompareTo(new VersionNumber(1, 1, 0, 1)));
			AssertEquals("Compare()", -1, versionNumber.CompareTo(new VersionNumber(1, 1, 2, 1)));
			AssertEquals("Compare()", 1, versionNumber.CompareTo(new VersionNumber(1, 1, 1, 0)));
			AssertEquals("Compare()", -1, versionNumber.CompareTo(new VersionNumber(1, 1, 1, 2)));
		}

		public void TestTryParse()
		{
			VersionNumber versionNumber;
			AssertEquals("TryParse()", false, VersionNumber.TryParse(null, out versionNumber));
			AssertEquals("TryParse()", false, VersionNumber.TryParse("1.2.3.4.5", out versionNumber));
			AssertEquals("TryParse()", false, VersionNumber.TryParse("X.2.3.4", out versionNumber));
			AssertEquals("TryParse()", false, VersionNumber.TryParse("1.X.3.4", out versionNumber));
			AssertEquals("TryParse()", false, VersionNumber.TryParse("1.2.X.4", out versionNumber));
			AssertEquals("TryParse()", false, VersionNumber.TryParse("1.2.3.X", out versionNumber));
			AssertEquals("TryParse()", true, VersionNumber.TryParse("1.2.3.4", out versionNumber));
			AssertEquals("ToString()", "1.2.3.4", versionNumber.ToString());
		}

		public void TestAdd()
		{
			VersionNumber versionNumber = new VersionNumber(1, 1, 1, 1);

			AssertEquals("Add(1, 2, 3, 4)", new VersionNumber(2, 3, 4, 5), versionNumber.Add(1, 2, 3, 4));
			AssertEquals("Add(4, 3, 2, 1)", new VersionNumber(5, 4, 3, 2), versionNumber.Add(4, 3, 2, 1));
			AssertEquals("Add(-1, -1, -1, -1)", new VersionNumber(), versionNumber.Add(-1, -1, -1, -1));

			AssertEquals("AddMajor(1)", new VersionNumber(2, 1, 1, 1), versionNumber.AddMajor(1));
			AssertEquals("AddMajor(-1)", new VersionNumber(0, 1, 1, 1), versionNumber.AddMajor(-1));
			AssertEquals("AddMinor(1)", new VersionNumber(1, 2, 1, 1), versionNumber.AddMinor(1));
			AssertEquals("AddMinor(-1)", new VersionNumber(1, 0, 1, 1), versionNumber.AddMinor(-1));
			AssertEquals("AddRelease(1)", new VersionNumber(1, 1, 2, 1), versionNumber.AddRelease(1));
			AssertEquals("AddRelease(-1)", new VersionNumber(1, 1, 0, 1), versionNumber.AddRelease(-1));
			AssertEquals("AddPatch(1)", new VersionNumber(1, 1, 1, 2), versionNumber.AddPatch(1));
			AssertEquals("AddPatch(-1)", new VersionNumber(1, 1, 1, 0), versionNumber.AddPatch(-1));
		}

		public void TestOperators()
		{
			VersionNumber v1 = new VersionNumber(1, 1, 1, 1);
			VersionNumber v1b = new VersionNumber(1, 1, 1, 1);
			VersionNumber v2 = new VersionNumber(2, 2, 2, 2);
			VersionNumber v2b = new VersionNumber(2, 2, 2, 2);
			VersionNumber v3 = new VersionNumber(3, 3, 3, 3);

			AssertEquals("v1 == v1", true, v1 == v1b);
			AssertEquals("v1 == v2", false, v1 == v2);
			AssertEquals("v1 == v3", false, v1 == v3);

			AssertEquals("v1 != v1", false, v1 != v1b);
			AssertEquals("v1 != v2", true, v1 != v2);
			AssertEquals("v1 != v3", true, v1 != v3);

			AssertEquals("v1 < v1", false, v1 < v1b);
			AssertEquals("v1 < v2", true, v1 < v2);
			AssertEquals("v1 < v3", true, v1 < v3);

			AssertEquals("v2 < v1", false, v2 < v1);
			AssertEquals("v2 < v2", false, v2 < v2b);
			AssertEquals("v2 < v3", true, v2 < v3);

			AssertEquals("v1 <= v1", true, v1 <= v1b);
			AssertEquals("v1 <= v2", true, v1 <= v2);
			AssertEquals("v1 <= v3", true, v1 <= v3);

			AssertEquals("v2 <= v1", false, v2 <= v1);
			AssertEquals("v2 <= v2", true, v2 <= v2b);
			AssertEquals("v2 <= v3", true, v2 <= v3);

			AssertEquals("v1 > v1", false, v1 > v1b);
			AssertEquals("v1 > v2", false, v1 > v2);
			AssertEquals("v1 > v3", false, v1 > v3);

			AssertEquals("v2 > v1", true, v2 > v1);
			AssertEquals("v2 > v2", false, v2 > v2b);
			AssertEquals("v2 > v3", false, v2 > v3);

			AssertEquals("v1 >= v1", true, v1 >= v1b);
			AssertEquals("v1 >= v2", false, v1 >= v2);
			AssertEquals("v1 >= v3", false, v1 >= v3);

			AssertEquals("v2 >= v1", true, v2 >= v1);
			AssertEquals("v2 >= v2", true, v2 >= v2b);
			AssertEquals("v2 >= v3", false, v2 >= v3);
		}
	}
}
