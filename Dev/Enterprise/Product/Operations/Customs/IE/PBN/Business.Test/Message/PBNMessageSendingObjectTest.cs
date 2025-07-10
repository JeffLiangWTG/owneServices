using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNMessageSendingObject))]
	sealed class PBNMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMessageType()
		{
			var propertyInfo = MessageSendingObject.MessageTypeInfo;
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(propertyInfo);
			AssertEquals("Message Type", resourceStringData.Caption);
			AssertEquals("Lookups.MessageTypes", propertyInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestJobNumber()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(MessageSendingObject.JobNumberInfo);
			AssertEquals("PBN ID/Job Number", resourceStringData.Caption);
		}

		public void TestMessageStatus()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(MessageSendingObject.MessageStatusInfo);
			AssertEquals("Message Status", resourceStringData.Caption);
		}

		public void TestLookups()
		{
			var lookups = MessageSendingObject.Lookups;
			AssertSame("Cached", lookups, messageSendingObject.Lookups);
		}

		public void TestCreateSender()
		{
			var sender = MessageSendingObject.CreateSender();
			AssertType<PBNMessageSender>(sender);
		}

		protected override BusinessObject GetNewBusinessObject() => new PBNMessageSendingObject(Factory.New<AsycudaManifestHeader>());

		PBNMessageSendingObject MessageSendingObject => messageSendingObject ??= new PBNMessageSendingObject(Factory.New<AsycudaManifestHeader>());
		PBNMessageSendingObject messageSendingObject;
	}
}
