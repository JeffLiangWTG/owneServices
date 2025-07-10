using System;
using Enterprise.DbUpgrader.Resource.Version;
using NUnit.Framework;

namespace CargoWise.Database.Abstractions.Test
{
	public class VersionLabelTest
	{
		public VersionLabelTest()
			: base()
		{
		}

		[Test]
		public void CompareTo()
		{
			var r = new Random();
			var major1 = r.Next();
			var minor1 = r.Next();
			var major2 = r.Next();
			var minor2 = r.Next();

			int expectedResult;
			if (major1 != major2)
			{
				expectedResult = major1 - major2;
			}
			else
			{
				expectedResult = minor1 - minor2;
			}

			var testLabel1 = new VersionLabel(major1, minor1);
			Assert.That(testLabel1.CompareTo(major2, minor2), Is.EqualTo(expectedResult), "Compare to given major/minor numbers");
			var testLabel2 = new VersionLabel(major2, minor2);
			Assert.That(testLabel1.CompareTo(testLabel2), Is.EqualTo(expectedResult), "Compare to other version label");
		}

		[Test]
		public void IsMajorDiff()
		{
			var r = new Random();
			var major1 = r.Next();
			var minor1 = r.Next();
			var major2 = r.Next();

			var expectedResult = (major1 != major2);

			var testLabel1 = new VersionLabel(major1, minor1);
			Assert.That(testLabel1.IsMajorDiff(major2), Is.EqualTo(expectedResult));
		}

		[Test]
		public void ToStringValue()
		{
			var r = new Random();
			var major1 = r.Next();
			var minor1 = r.Next();

			var expectedResult = major1.ToString() + "." + minor1.ToString();

			var testLabel1 = new VersionLabel(major1, minor1);
			Assert.That(testLabel1.ToString(), Is.EqualTo(expectedResult));
		}

		[Test]
		public void IsBetweenOrEqualToTopVersion()
		{
			var current = new VersionLabel(1, 0);
			var versionToTest = new VersionLabel(1, 1);
			var to = new VersionLabel(1, 1);

			Assert.That(versionToTest.IsBetweenOrEqualToTopVersion(current, to), Is.EqualTo(true), "Should return true for a test version that is equal to the version upgrading to");

			current = new VersionLabel(0, 1);
			versionToTest = new VersionLabel(1, 3);
			to = new VersionLabel(1, 6);
			Assert.That(versionToTest.IsBetweenOrEqualToTopVersion(current, to), Is.EqualTo(true), "Should return true for a test version where major and minor versions are different");

			current = new VersionLabel(2, 1);
			versionToTest = new VersionLabel(3, 1);
			to = new VersionLabel(4, 1);
			Assert.That(versionToTest.IsBetweenOrEqualToTopVersion(current, to), Is.EqualTo(true), "Should return true for a test version where only the major versions are different");

			current = new VersionLabel(2, 1);
			versionToTest = new VersionLabel(2, 2);
			to = new VersionLabel(2, 3);
			Assert.That(versionToTest.IsBetweenOrEqualToTopVersion(current, to), Is.EqualTo(true), "Should return true for a test version where only the minor versions are different");

			current = new VersionLabel(0, 1);
			versionToTest = new VersionLabel(0, 1);
			to = new VersionLabel(1, 1);
			Assert.That(versionToTest.IsBetweenOrEqualToTopVersion(current, to), Is.EqualTo(false), "Should return false for a test version that is equal to the existing");

			current = new VersionLabel(2, 2);
			versionToTest = new VersionLabel(2, 1);
			to = new VersionLabel(2, 3);
			Assert.That(versionToTest.IsBetweenOrEqualToTopVersion(current, to), Is.EqualTo(false), "Should return false for a version that is below the existing");
		}

		[Test]
		public void DocManagerVersionConsidersMajorAndMinorVersionsInComparison()
		{
			var current = new VersionLabel(1465, 68);
			var to = new VersionLabel(1465, 75);

			var docManager = new VersionLabel(1462, 74);
			Assert.That(docManager.IsBetweenOrEqualToTopVersion(current, to), Is.EqualTo(false), "Should NOT return that it is between the version");

			docManager = new VersionLabel(1465, 74);
			Assert.That(docManager.IsBetweenOrEqualToTopVersion(current, to), Is.EqualTo(true), "Should return that it is between the version");

			docManager = new VersionLabel(1462, 75);
			Assert.That(docManager.IsBetweenOrEqualToTopVersion(current, to), Is.EqualTo(false), "Should NOT return that it is between the version");

			docManager = new VersionLabel(1465, 75);
			Assert.That(docManager.IsBetweenOrEqualToTopVersion(current, to), Is.EqualTo(true), "Should return that it is between the version");
		}
	}
}
