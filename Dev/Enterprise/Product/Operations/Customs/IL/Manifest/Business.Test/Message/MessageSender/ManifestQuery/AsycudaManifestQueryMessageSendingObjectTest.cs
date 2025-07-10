using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestQueryMessageSendingObject))]
	public sealed class AsycudaManifestQueryMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<AsycudaManifestQueryMessageSendingObjectLookups>(sendingObject.Lookups);
		}

		public void TestMessageSubType()
		{
			AssertEquals("Max length of MessageSubType must be 3", 3, sendingObject.MessageSubTypeInfo.MaxLength);
		}

		public void TestMessageSubTypeDescription()
		{
			AssertEquals("MessageSubTypeDescription should be get only", false, sendingObject.MessageSubTypeDescriptionInfo.HasSetter);
			sendingObject.MessageSubType = "820";
			AssertEquals("MessageSubTypeDescription should return description of the code set in MessageSubType", "Manifest Query Request", sendingObject.MessageSubTypeDescription);
		}

		public void Test_ReadOnly_Members()
		{
			AssertEquals("MessageType should be read only", true, sendingObject.MessageTypeInfo.ReadOnly);
			AssertEquals("MessageSubType should be read only", true, sendingObject.MessageSubTypeInfo.ReadOnly);
			AssertEquals("ManifestNumber should be read only", true, sendingObject.ManifestNumberInfo.ReadOnly);
			AssertEquals("ParentDealNumber should be read only", true, sendingObject.ParentDealNumberInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AsycudaManifestQueryMessageSendingObject(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestNumber = "123";
			var asycudaBill = header.Bills.AddNew();
			var asycudaTransportDocumentInfo = asycudaBill.TransportDocuments.AddNew();
			asycudaTransportDocumentInfo.CSI_Code = TransportDocsTypeList.Codes.IL2;
			asycudaTransportDocumentInfo.CSI_ReferenceNumber = "456";
			sendingObject = new AsycudaManifestQueryMessageSendingObject(header);
		}

		AsycudaManifestHeader header;
		AsycudaManifestQueryMessageSendingObject sendingObject;
	}
}
