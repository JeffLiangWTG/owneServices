using System;
using System.Diagnostics;
using System.Linq;
using CargoWise.BuildTools;
using WTG.DevTools.Definitions;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Core
{
	[Immutable]
	public struct VersionNumber : IComparable<VersionNumber>, IEquatable<VersionNumber>
	{
		public VersionNumber(string version)
		{
			var numberOfVersionFields = 4;
			var parts = version.Split('.');

			if (parts.Length < numberOfVersionFields)
			{
				parts = parts.Concat(Enumerable.Repeat("0", numberOfVersionFields - parts.Length)).ToArray();
			}

			this = new VersionNumber(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]));
		}

		public VersionNumber(FileVersionInfo fileVersionInfo)
		{
			this = new VersionNumber(fileVersionInfo.FileMajorPart, fileVersionInfo.FileMinorPart, fileVersionInfo.FileBuildPart, fileVersionInfo.FilePrivatePart);
		}

		public VersionNumber(Version version)
		{
			this = new VersionNumber(version.Major, version.Minor, version.Build, version.Revision);
		}

		public VersionNumber(int major, int minor, int release, int patch)
		{
			if (major < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(major));
			}

			if (minor < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(minor));
			}

			if (release < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(release));
			}

			if (patch < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(patch));
			}

			this.major = major;
			this.minor = minor;
			this.release = release;
			this.patch = patch;
		}

		public bool IsEmpty
		{
			get { return (Major == 0) && (Minor == 0) && (Release == 0) && (Patch == 0); }
		}

		public override bool Equals(object obj)
		{
			return (obj != null) && (obj is VersionNumber) && Equals((VersionNumber)obj);
		}

		public DateTime GetReleaseDate()
		{
			return Major > 2 ? new DateTime(2000 + Major, Minor, Release) :
					Major > 1 ? BuildConstants.BaseReleaseDateCw1Obsolete.AddDays(Release) :
						ReleaseRings.BaseReleaseDateEdi.AddDays(Release);
		}

		public override int GetHashCode()
		{
			int result = 0;
			result |= (Major & 0xf) << 28;
			result |= (Minor & 0xff) << 20;
			result |= (Release & 0xff) << 12;
			return (result | (Patch & 0xfff));
		}

		public override string ToString()
		{
			return Major + "." + Minor + "." + Release + "." + Patch;
		}

		public Version ToVersion()
		{
			return new Version(Major, Minor, Release, Patch);
		}

		public static bool TryParse(string version, out VersionNumber result)
		{
			string[] parts;
			if (version != null &&
				(parts = version.Split('.')).Length == 4 &&
				int.TryParse(parts[0], out var major) &&
				int.TryParse(parts[1], out var minor) &&
				int.TryParse(parts[2], out var release) &&
				int.TryParse(parts[3], out var patch))
			{
				result = new VersionNumber(major, minor, release, patch);
				return true;
			}
			else
			{
				result = new VersionNumber();
				return false;
			}
		}

		#region Add

		public VersionNumber Add(int major, int minor, int release, int patch)
		{
			return new VersionNumber(Major + major, Minor + minor, Release + release, Patch + patch);
		}

		public VersionNumber AddMajor(int value)
		{
			return new VersionNumber(Major + value, Minor, Release, Patch);
		}

		public VersionNumber AddMinor(int value)
		{
			return new VersionNumber(Major, Minor + value, Release, Patch);
		}

		public VersionNumber AddRelease(int value)
		{
			return new VersionNumber(Major, Minor, Release + value, Patch);
		}

		public VersionNumber AddPatch(int value)
		{
			return new VersionNumber(Major, Minor, Release, Patch + value);
		}

		#endregion

		#region Operators

		public static bool operator ==(VersionNumber v1, VersionNumber v2)
		{
			return v1.Equals(v2);
		}

		public static bool operator !=(VersionNumber v1, VersionNumber v2)
		{
			return !v1.Equals(v2);
		}

		public static bool operator >(VersionNumber v1, VersionNumber v2)
		{
			return v1.CompareTo(v2) > 0;
		}

		public static bool operator >=(VersionNumber v1, VersionNumber v2)
		{
			return v1.CompareTo(v2) >= 0;
		}

		public static bool operator <(VersionNumber v1, VersionNumber v2)
		{
			return v1.CompareTo(v2) < 0;
		}

		public static bool operator <=(VersionNumber v1, VersionNumber v2)
		{
			return v1.CompareTo(v2) <= 0;
		}

		#endregion

		#region Version Parts

		public int Major
		{
			get { return major; }
		}

		public int Minor
		{
			get { return minor; }
		}

		public int Release
		{
			get { return release; }
		}

		public int Patch
		{
			get { return patch; }
		}

		readonly int major;
		readonly int minor;
		readonly int release;
		readonly int patch;

		#endregion

		#region IComparable<VersionNumber> Members

		public int CompareTo(VersionNumber other)
		{
			if (Major != other.Major)
			{
				if (Major > other.Major)
				{
					return 1;
				}
				return -1;
			}

			if (Minor != other.Minor)
			{
				if (Minor > other.Minor)
				{
					return 1;
				}
				return -1;
			}

			if (Release != other.Release)
			{
				if (Release > other.Release)
				{
					return 1;
				}
				return -1;
			}

			if (Patch == other.Patch)
			{
				return 0;
			}

			if (Patch > other.Patch)
			{
				return 1;
			}

			return -1;
		}

		#endregion

		#region IEquatable<VersionNumber> Members

		public bool Equals(VersionNumber other)
		{
			return (Patch == other.Patch) && (Release == other.Release) && (Minor == other.Minor) && (Major == other.Major);
		}

		#endregion
	}
}
