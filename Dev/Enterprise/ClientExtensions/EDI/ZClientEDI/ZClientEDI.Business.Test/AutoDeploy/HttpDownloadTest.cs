using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Test
{
	public class HttpDownloadTest : FirstSupportingVersionTest
	{
		public void TestGprVersion()
		{
			AssertEquals(HttpDownload.GprVersion, new VersionNumber(1, 3, 3371, 982));
		}

		public void TestStdVersion()
		{
			AssertEquals(HttpDownload.StdVersion, new VersionNumber(1, 4, 3532, 653));
		}

		public void TestGetVersionComposedNumber()
		{
			EDIDataRegistry.Instance.HttpDownloadUserName = "";
			HttpDownload httpDownload = new HttpDownload();
			AssertEquals(httpDownload.VersionComposedNumber, 112083);
			EDIDataRegistry.Instance.HttpDownloadUserName = "SecureUpdates";
			AssertEquals(httpDownload.VersionComposedNumber, HttpDownload.SecureDownloadVersion);
		}

		public new void TestIsSupportingVersion()
		{
			EDIDataRegistry.Instance.HttpDownloadUserName = "Dummy";

			HttpDownload httpDownload = new HttpDownload();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var build = factory.New<ReleaseBuild>();
			build.VersionNumber = HttpDownload.GprVersion;
			Assert(httpDownload.IsSupportingVersion(build));

			build.VersionNumber = new VersionNumber(1, 3, 3371, 981);
			Assert(!httpDownload.IsSupportingVersion(build));

			build.VersionNumber = new VersionNumber(1, 3, 3371, 983);
			Assert(httpDownload.IsSupportingVersion(build));

			build.VersionNumber = new VersionNumber(1, 4, 3371, 654);
			Assert(!httpDownload.IsSupportingVersion(build));

			build.VersionNumber = HttpDownload.StdVersion;
			Assert(httpDownload.IsSupportingVersion(build));

			build.VersionNumber = new VersionNumber(1, 4, 3532, 652);
			Assert(!httpDownload.IsSupportingVersion(build));

			build.VersionNumber = new VersionNumber(1, 4, 3532, 654);
			Assert(httpDownload.IsSupportingVersion(build));

			build.VersionNumber = new VersionNumber(1, 4, 3596, 0);
			Assert(!httpDownload.IsSupportingVersion(build));

			build.VersionNumber = new VersionNumber(1, 4, 3597, 0);
			Assert(httpDownload.IsSupportingVersion(build));
		}

		#region Implementeation

		protected override FirstSupportingVersion GetTstVersion()
		{
			return new HttpDownload();
		}

		#endregion
	}
}
