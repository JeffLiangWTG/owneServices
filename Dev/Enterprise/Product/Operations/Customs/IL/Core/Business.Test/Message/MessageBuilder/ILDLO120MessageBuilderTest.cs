using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILDLO120MessageBuilderTest : ILMessageBuilderBaseTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("messageProvider is must", () => new ILDLO120MessageBuilder(null, null));
			AssertExceptionThrown<ArgumentNullException>("deliveryOrderDocDataSendingObject is must", () => new ILDLO120MessageBuilder(Factory.New<ForwardingShipment>().DeliveryOrderProvider, null));
		}

		public void TestPopulateMessages_ShouldIncludeNote_WhenDigitalSignatureDisabled()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var messageBuilder = GetMessageBuilder();
				var messageBuilderResult = messageBuilder.PopulateMessages();
				AssertEquals("IsSuccess = true", true, messageBuilderResult.IsSuccess);
				var buildResults = messageBuilderResult.GetBuilderResults();
				AssertEquals("One build result expected", 1, buildResults.Count());
				var buildResult = buildResults.First();
				var message = buildResult.Message;
				AssertNotNull("Build result still should have message", message);
				var notes = (StmNoteCollection)message.Notes.GetAllNotes();
				AssertEquals("The message have note", 1, notes.Count);
				AssertEquals("The message have note with text", "Digital Signature is disabled by Registry", notes[0].ST_NoteText);
			}
		}

		public void TestPopulateMessages_ShouldFailWithError_WhenDigitalSignatureEnabled()
		{
			using (ILCustomsDataRegistry.Instance.SendILCustomsMessagesWithoutDigitalSignature.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var messageBuilder = GetMessageBuilder();
				var messageBuilderResult = messageBuilder.PopulateMessages();
				AssertEquals("Populating should fail", false, messageBuilderResult.IsSuccess);
				var buildResults = messageBuilderResult.GetBuilderResults();
				AssertEquals("One build result expected", 1, buildResults.Count());
				var buildResult = buildResults.First();
				AssertContainsExactElementsInAnyOrder("There should be 1 error", new string[] { "No Valid digital sign certificate found for signing this message – please review your staff or company configuration" }, buildResult.Errors);
				AssertNotNull("Build result still should have message", buildResult.Message);
			}
		}

		protected override IMessageBuilder GetMessageBuilder()
		{
			var deliveryOrderDocDataObject = new DeliveryOrderBuilder(shipment).Build();
			deliveryOrderDocDataObject.ReceiverType.Code = "3";
			var deliveryOrderDocDataSendingObject = new DeliveryOrderDocDataSendingObject(deliveryOrderDocDataObject, true);

			return new ILDLO120MessageBuilder(shipment.DeliveryOrderProvider, deliveryOrderDocDataSendingObject);
		}

		protected override string GetExpectedMessageSubType()
		{
			return "120";
		}

		protected override string GetExpectedMessageText()
		{
			return new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderRequest_1200.xml")).Replace("<deliveryOrderNumber>0</deliveryOrderNumber>", "<deliveryOrderNumber>10000000</deliveryOrderNumber>");
		}

		protected override string GetExpectedMessageType()
		{
			return "DLO";
		}

		protected override BusinessObject GetLinkedObject() => shipment;

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => shipment.Messages;

		protected override void SetUp()
		{
			base.SetUp();
			var factory = Factory;
			shipment = factory.New<ForwardingShipment>();
			factory.Save();
		}

		ForwardingShipment shipment;
	}
}
