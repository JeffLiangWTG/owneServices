using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;
using Enterprise.Customs.AU.GUI.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirScanForOutturnHostTest : ScanForOutturnHostTest
	{
		public void TestConstructor()
		{
			using (var form = new ZForm())
			{
				AssertExceptionThrown(typeof(ArgumentNullException), () => new AirScanForOutturnHost(form, null));
			}

			CusMAWB cusMAWB = Factory.New<CusMAWB>();
			AssertExceptionThrown(typeof(ArgumentNullException), () => new AirScanForOutturnHost(null, cusMAWB));
			using (var form = new ZForm())
			{
				var manager = new AirScanForOutturnHost(form, cusMAWB);
			}
		}

		protected override ScanForOutturnHost GetNewScanForOutturnHost(ZForm form)
		{
			var cusMAWB = Factory.New<CusMAWB>();
			return new AirScanForOutturnHost(form, cusMAWB);
		}
	}
}
