using Enterprise.Dat.Implementation;
using NUnit.Framework;

namespace Enterprise.Dat.Adapter.Testing
{
	sealed class InstalledSoftwareDetectionTest : TestCase
	{
		public void TestOlapServer()
		{
			AssertEquals(true, installedSoftware.HasFlag(RequiredSoftware.OlapServer));
		}

		protected override void SetUp()
		{
			base.SetUp();
			installedSoftware = InstalledSoftwareDetection.GetInstalledSoftware();
		}
		RequiredSoftware installedSoftware;
	}
}
