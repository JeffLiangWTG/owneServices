using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.IE.PBN.Business.Testing
{
	[TestedType(typeof(PBNMessageSendingObjectParent))]
	sealed class PBNMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTopLevelBusinessObject()
		{
			AssertSame(MessageSendingObjectParent.Header, MessageSendingObjectParent.TopLevelBusinessObject);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			AssertSame(Env.Security.GlobalManifestSendWithMessageErrors, MessageSendingObjectParent.SecurityCheckpointToSendWithMessageError);
		}

		public void TestGetSendingObjectsCollection()
		{
			var sendingObjectsCollection = MessageSendingObjectParent.SendingObjectsCollection;
			AssertType<PBNMessageSendingObjectCollection>(sendingObjectsCollection);
			AssertEquals("Count", 1, sendingObjectsCollection.Count);
			AssertNotNull("Sending object initialized", sendingObjectsCollection[0]);
		}

		public void TestMessageSendingObjectProperties()
		{
			var properties = MessageSendingObjectParent.MessageSendingObjectProperties.ToArray();

			AssertEquals("Property count", 3, properties.Length);
			AssertMessageSendingObjectProperty("Column 1", properties[0], expectedPropertyName: nameof(PBNMessageSendingObject.MessageType), expectedIsMandatory: true, expectedColumnWidth: 85);
			AssertMessageSendingObjectProperty("Column 2", properties[1], expectedPropertyName: nameof(PBNMessageSendingObject.JobNumber), expectedIsMandatory: true, expectedColumnWidth: 110);
			AssertMessageSendingObjectProperty("Column 3", properties[2], expectedPropertyName: nameof(PBNMessageSendingObject.MessageStatus), expectedIsMandatory: true, expectedColumnWidth: 90);

			void AssertMessageSendingObjectProperty(string message, MessageSendingObjectProperty property, string expectedPropertyName, bool expectedIsMandatory, int expectedColumnWidth)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("PropertyName", expectedPropertyName, property.PropertyName);
					AssertEquals("IsMandatory", expectedIsMandatory, property.IsMandatory);
					AssertEquals("ColumnWidth", expectedColumnWidth, property.ColumnWidth);
				});
			}
		}

		protected override BusinessObject GetNewBusinessObject() => MessageSendingObjectParent;

		PBNMessageSendingObjectParent MessageSendingObjectParent
		{
			get
			{
				if (messageSendingObjectParent == null)
				{
					var header = Factory.New<AsycudaManifestHeader>();
					messageSendingObjectParent = new PBNMessageSendingObjectParent(header);
				}

				return messageSendingObjectParent;
			}
		}
		PBNMessageSendingObjectParent messageSendingObjectParent;
	}
}
