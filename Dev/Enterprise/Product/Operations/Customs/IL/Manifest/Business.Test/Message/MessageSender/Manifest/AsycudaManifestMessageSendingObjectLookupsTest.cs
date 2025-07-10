using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	public class AsycudaManifestMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageSubTypes()
		{
			var messageSubTypes = sendingObject.Lookups.MessageSubTypes;
			AssertEquals(1, messageSubTypes.Count);
			AssertEquals("170", messageSubTypes.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			sendingObject = new AsycudaManifestMessageSendingObject(header);
		}

		AsycudaManifestMessageSendingObject sendingObject;
	}
}
