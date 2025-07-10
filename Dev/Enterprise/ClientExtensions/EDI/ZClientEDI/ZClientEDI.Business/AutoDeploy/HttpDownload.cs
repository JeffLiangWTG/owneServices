using System.Globalization;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI
{
	/// <summary>
	/// Summary description for HttpDownload.
	/// </summary>
	public class HttpDownload : FirstSupportingVersion
	{
		public HttpDownload() : base()
		{
		}

		public override bool IsSupportingVersion(ReleaseBuild build)
		{
			if (build != null)
			{
				return IsSupportingVersion(build.HL_MajorVersion, build.HL_MinorVersion, build.HL_Release)
					|| IsSupportingPatch(StdVersion, build.VersionNumber)
					|| IsSupportingPatch(GprVersion, build.VersionNumber);
			}
			else
			{
				return false;
			}
		}

		static public readonly VersionNumber GprVersion = new VersionNumber(1, 3, 3371, 982);
		static public readonly VersionNumber StdVersion = new VersionNumber(1, 4, 3532, 653);

		const int MajorSecureDownloadVersion = 1;
		const int MinorSecureDownloadVersion = 4;
		const int ReleaseSecureDownloadVersion = 3597; // 6 Nov 2009
		public const int SecureDownloadVersion = MajorSecureDownloadVersion * 100000 + MinorSecureDownloadVersion * 10000 + ReleaseSecureDownloadVersion;

		public static string SecureDownloadVersionString
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.0", MajorSecureDownloadVersion, MinorSecureDownloadVersion, ReleaseSecureDownloadVersion); }
		}

		#region Implementation

		protected override int GetVersionComposedNumber()
		{
			// If there is no secure download user then fall back to
			// the original anonymous HTTP download
			return string.IsNullOrEmpty(UpgradeConstants.HttpDownloadUserName)
				? 112083
				: SecureDownloadVersion;
		}
		#endregion
	}
}
