using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class _202000LineTest : TestCase
	{
		public void TestWeight()
		{
			AssertEquals(12M, _202000Line.Weight);
		}

		public void TestPiecesManifested()
		{
			AssertEquals(9, _202000Line.PiecesManifested);
		}

		public void TestPackageTrackingNumber()
		{
			AssertEquals("1ZAT78726777734406", _202000Line.PackageTrackingNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_202000Line = new _202000Line("CA1399AU9639040422              DAT7872Q3D9K2020001ZAT78726777734406                 12     1      9    N                                                            4527199087E                AU09639  S1AU9639TC.113B2004-04-20                              Y                                                                             AUDNNNNNBI                                  ");
		}

		_202000Line _202000Line;
	}
}
