using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI
{
	/// <summary>
	/// Summary description for FirstSupportedVersion.
	/// </summary>
	public abstract class FirstSupportingVersion
	{
		public virtual bool IsSupportingVersion(ReleaseBuild build)
		{
			if (build != null)
			{
				return IsSupportingVersion(build.HL_MajorVersion, build.HL_MinorVersion, build.HL_Release);
			}
			else
			{
				return false;
			}
		}

		public bool IsSupportingVersion(int majorVersion, int minorVersion, int release)
		{
			return CalculateComposedNumber(majorVersion, minorVersion, release) >= VersionComposedNumber;
		}

		public int VersionMajorNumber
		{
			get { return (VersionComposedNumber / 100000); }
		}

		public int VersionMinorNumber
		{
			get { return ((VersionComposedNumber / 10000) - (VersionMajorNumber * 10)); }
		}

		public int VersionReleaseNumber
		{
			get { return (VersionComposedNumber % 10000); }
		}

		public static bool IsSupportingPatch(VersionNumber firstVersion, VersionNumber build)
		{
			return build.Major == firstVersion.Major
				&& build.Minor == firstVersion.Minor
				&& build.Release == firstVersion.Release
				&& build.Patch >= firstVersion.Patch;
		}

		#region Implementation

		public int VersionComposedNumber
		{
			get { return GetVersionComposedNumber(); }
		}

		protected abstract int GetVersionComposedNumber();

		public int CalculateComposedNumber(int majorVersion, int minorVersion, int release)
		{
			return (majorVersion * 100000 + minorVersion * 10000 + release);
		}

		#endregion
	}
}
