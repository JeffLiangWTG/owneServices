using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	public class AsycudaManifestHeaderSynchroniserTest : SynchroniserTestCase
	{
		public void TestSynchronisersShippingAgent()
		{
			OrgHeader orgReceivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader orgSendingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_OA_ReceivingForwarderAddress = orgReceivingAgent.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = orgSendingAgent.MainAddress.PK;
			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_ManifestType = BRManifestTypes.Codes.MER;
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals(orgSendingAgent.MainAddress.PK, manifestHeader.AMA_OA_ShippingAgent);
		}
	}
}
