using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business
{
	[TestedType(typeof(AsycudaManifestHeaderSynchroniser))]
	sealed class AsycudaManifestHeaderSynchroniserTest : TestCaseWithFactory
	{
		public void TestLastForeignPort()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			consol.JK_RL_NKLastForeignPort = "MXACA";
			AssertEquals("MXACA", manifestHeader.LastForeignPort);
		}
	}
}
