using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestMessageSendingObject))]
	public sealed class AsycudaManifestMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<AsycudaManifestMessageSendingObjectLookups>(sendingObject.Lookups);
		}

		public void TestMessageSubType()
		{
			AssertEquals("Max length of MessageSubType must be 3", 3, sendingObject.MessageSubTypeInfo.MaxLength);
		}

		public void TestMessageSubTypeDescription()
		{
			AssertEquals("MessageSubTypeDescription should be get only", false, sendingObject.MessageSubTypeDescriptionInfo.HasSetter);
			sendingObject.MessageSubType = "170";

			AssertEquals("MessageSubTypeDescription should return description of the code set in MessageSubType", "Forwarder Manifest Request", sendingObject.MessageSubTypeDescription);
		}

		public void Test_ReadOnly_Members()
		{
			AssertEquals("MessageType should be read only", true, sendingObject.MessageTypeInfo.ReadOnly);
			AssertEquals("MessageSubType should be read only", true, sendingObject.MessageSubTypeInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AsycudaManifestMessageSendingObject(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			sendingObject = new AsycudaManifestMessageSendingObject(header);
		}

		AsycudaManifestHeader header;
		AsycudaManifestMessageSendingObject sendingObject;
	}
}
