using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	public class AsycudaManifestQueryMessageSendingObjectLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageSubTypes()
		{
			var messageSubTypes = sendingObject.Lookups.MessageSubTypes;
			AssertEquals(1, messageSubTypes.Count);
			AssertEquals("820", messageSubTypes.CodesAsString);
			AssertSame("Cached", messageSubTypes, sendingObject.Lookups.MessageSubTypes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<AsycudaManifestHeader>();
			sendingObject = new AsycudaManifestQueryMessageSendingObject(header);
		}

		AsycudaManifestQueryMessageSendingObject sendingObject;
	}
}
