using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using WTG.StaticAnalysis.Annotation;

//
// WARNING: You cannot move this class to another namespace without modifying or suspending the $/Dev Special File Handlers.
// Don't even try it unless you're fully prepared.
//

namespace Enterprise.DbUpgrader.Resource.Version
{
	/// <summary>
	/// Object to encapsulate Version Major/Minor numbers.
	/// </summary>
	[ImmutableObject(true)]
	[Immutable]
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public sealed class VersionLabel : IComparable<VersionLabel>
	{
		public VersionLabel(int majorVersion, int minorVersion)
		{
			this.major = majorVersion;
			this.minor = minorVersion;
		}

		public int Major
		{
			get { return major; }
		}
		readonly int major;

		public int Minor
		{
			get { return minor; }
		}
		readonly int minor;

		public int CompareTo(VersionLabel other)
		{
			if (other == null)
			{
				throw new ArgumentNullException(nameof(other), "The VersionLabel 'other' must not be null");
			}

			return CompareTo(other.major, other.minor);
		}

		public int CompareTo(int anotherMajor, int anotherMinor)
		{
			int result = major - anotherMajor;

			if (result == 0)
			{
				result = minor - anotherMinor;
			}

			return result;
		}

		public bool IsMajorDiff(int anotherMajor)
		{
			return major != anotherMajor;
		}

		public bool IsBetweenOrEqualToTopVersion(VersionLabel bottom, VersionLabel top)
		{
			if (!((bottom != null) && (top != null)))
			{
				throw new ArgumentException("Invalid argument.", nameof(bottom));
			}

			return (major > bottom.major && major <= top.major) || (major == top.major && minor > bottom.minor && minor <= top.minor);
		}

		public override string ToString()
		{
			return major.ToString(CultureInfo.InvariantCulture) + "." + minor.ToString(CultureInfo.InvariantCulture);
		}
	}
}
