using System;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class ILGPM130MessageBuilderTest : ILMessageBuilderBaseTest
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("messageProvider is must", () => new ILGPM130MessageBuilder(null, null));
			AssertExceptionThrown<ArgumentNullException>("gatePassMovementDocDataSendingObject is must", () => new ILGPM130MessageBuilder(Factory.New<ForwardingShipment>().GatePassMovementProvider, null));
		}

		protected override IMessageBuilder GetMessageBuilder()
		{
			var gatePassMovementProvider = shipment.GatePassMovementProvider;
			var gatePassMovementDocDataObject = new GatePassMovementBuilder(gatePassMovementProvider).Build();
			var gatePassMovementDocDataSendingObject = new GatePassMovementDocDataSendingObject(gatePassMovementDocDataObject, true);

			return new ILGPM130MessageBuilder(gatePassMovementProvider, gatePassMovementDocDataSendingObject);
		}

		protected override string GetExpectedMessageSubType()
		{
			return "130";
		}

		protected override string GetExpectedMessageText()
		{
			return new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("GatepassRequestMessage1030.xml")).Replace("<gatepassNumber>0</gatepassNumber>", "<gatepassNumber>10000000</gatepassNumber>");
		}

		protected override string GetExpectedMessageType()
		{
			return "GPM";
		}

		protected override BusinessObject GetLinkedObject() => shipment;

		protected override IBusinessObjectCollection GetMessageOwnerCollection() => shipment.Messages;

		protected override void SetUp()
		{
			base.SetUp();
			var factory = Factory;
			shipment = factory.New<ForwardingShipment>();

			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Israel);
			helper.CreateCusMapType("CRGTY", "OUT", "Cargo Types", true);
			helper.CreateCusMap("CRGTY", "FCL", "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("CRGTY", "LCL", "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("CRGTY", "ROR", "2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("CRGTY", "BLK", "3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			helper.CreateCusMap("CRGTY", "LQD", "4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, Core.Constants.CountryCodes.Israel);
			factory.Save();
		}

		ForwardingShipment shipment;
	}
}
