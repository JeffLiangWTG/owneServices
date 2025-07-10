using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	public abstract class FirstSupportingVersionTest : TransactionedTestCase
	{
		public void TestIsSupportingVersion()
		{
			AssertEquals("Old Version", false, TstVersion.IsSupportingVersion(TstVersionMajorNumber, TstVersionMinorNumber, TstVersionReleaseNumber - 1));
			AssertEquals("Same Version", true, TstVersion.IsSupportingVersion(TstVersionMajorNumber, TstVersionMinorNumber, TstVersionReleaseNumber));
			AssertEquals("Later Version", true, TstVersion.IsSupportingVersion(TstVersionMajorNumber, TstVersionMinorNumber, TstVersionReleaseNumber + 1));

			AssertEquals("Empty Version", false, TstVersion.IsSupportingVersion(null));

			var factory = new BusinessObjectFactory();
			var build = factory.New<ReleaseBuild>();
			build.HL_MajorVersion = TstVersionMajorNumber;
			build.HL_MinorVersion = TstVersionMinorNumber;
			build.HL_Release = TstVersionReleaseNumber;

			AssertEquals("Same Version", true, TstVersion.IsSupportingVersion(build));
		}

		public void TestVersionMajorNumber()
		{
			AssertEquals("Major Version Number", TstVersionMajorNumber, TstVersion.VersionMajorNumber);
		}

		public void TestVersionMinorNumber()
		{
			AssertEquals("Minor Version Number", TstVersionMinorNumber, TstVersion.VersionMinorNumber);
		}

		public void TestVersionReleaseNumber()
		{
			AssertEquals("Release Version Number", TstVersionReleaseNumber, TstVersion.VersionReleaseNumber);
		}

		public void TestCalculateComposedNumber()
		{
			AssertEquals("Composed Version Number", fVersionComposedNumber, TstVersion.CalculateComposedNumber(TstVersionMajorNumber, TstVersionMinorNumber, TstVersionReleaseNumber));
		}

		public void TestIsSupportingPatch()
		{
			VersionNumber first = new VersionNumber(1, 2, 3000, 500);
			Assert(HttpDownload.IsSupportingPatch(first, new VersionNumber(1, 2, 3000, 500)));
			Assert(HttpDownload.IsSupportingPatch(first, new VersionNumber(1, 2, 3000, 501)));
			Assert(!HttpDownload.IsSupportingPatch(first, new VersionNumber(1, 2, 3000, 499)));
			Assert(!HttpDownload.IsSupportingPatch(first, new VersionNumber(1, 2, 4000, 500)));
			Assert(!HttpDownload.IsSupportingPatch(first, new VersionNumber(1, 2, 2000, 500)));
			Assert(!HttpDownload.IsSupportingPatch(first, new VersionNumber(1, 3, 3000, 500)));
			Assert(!HttpDownload.IsSupportingPatch(first, new VersionNumber(1, 1, 3000, 500)));
			Assert(!HttpDownload.IsSupportingPatch(first, new VersionNumber(2, 2, 3000, 500)));
			Assert(!HttpDownload.IsSupportingPatch(first, new VersionNumber(0, 2, 3000, 500)));
		}

		#region Implementation

		protected int fVersionComposedNumber;

		protected FirstSupportingVersion TstVersion
		{
			get
			{
				if (fTstVersion == null)
				{
					fTstVersion = GetTstVersion();
				}

				return fTstVersion;
			}
		}
		FirstSupportingVersion fTstVersion;

		protected abstract FirstSupportingVersion GetTstVersion();

		protected int TstVersionMajorNumber
		{
			get { return fVersionComposedNumber / 100000; }
		}

		protected int TstVersionMinorNumber
		{
			get { return ((fVersionComposedNumber / 10000) - (TstVersionMajorNumber * 10)); }
		}

		protected int TstVersionReleaseNumber
		{
			get { return (fVersionComposedNumber % 10000); }
		}

		protected override void SetUp()
		{
			base.SetUp();
			fVersionComposedNumber = TstVersion.VersionComposedNumber;
		}

		#endregion
	}
}
