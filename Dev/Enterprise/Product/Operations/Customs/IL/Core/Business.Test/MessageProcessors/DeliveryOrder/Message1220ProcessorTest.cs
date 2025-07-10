using CargoWise.Customs.IL.MessageDefinitions.DLO.RES_121.MN_NG_1220_MSG22_DeliveryOrderFeedBack_Message;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IL.Business.MessageProcessors;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business.Testing
{
	sealed class Message1220ProcessorTest
		: BaseMessageProcessorForSingleNumberTest<Message1220Processor, MnNg1220Msg22DeliveryOrderFeedBackMessage, ILDLO122ResponseMessage>
	{
		protected override Message1220Processor CreateProcessor(LoggingInformation loggingInformation) => new Message1220Processor(new LoggingInformation());

		protected override string ExpectedMessageFriendlyName => "IL Delivery Order Response Message";

		protected override string ExpectedMessageTypesToInclude => "DLO";

		protected override string ExpectedMessageSubTypesToInclude => ZString.Empty;

		protected override string BasicSuccessfulMessageText => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderResponse_1220.xml"));

		protected override string WithdrawCancelMessageText => BasicSuccessfulMessageText;

		protected override string NoReferenceMessageText => new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("DeliveryOrderResponse_1220_Interchange.xml"));

		protected override string NumberType => "DON";

		protected override string CouldNotLocateMessage => "Could not locate Shipment by Delivery Order Number #";

		protected override string MoreThanOneMessage => "Found more than one shipment with the same Delivery Order Number – please check";

		protected override string EntryNum => "222197";

		protected override bool ExpectedSupportsConsol => false;

		protected override string DocumentName => ILMessageEventParameter.DeliveryOrderDocumentName;

		protected override string GetExpectedCustomsEntryStatusLogReferenceFreeText => "222197 Accepted";

		protected override EDIMessage GetRequestMessage(ForwardingShipment forwardingShipment)
		{
			var message = Factory.New<ILDLO120RequestMessage>();
			message.EM_Status = "SNT";
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = forwardingShipment.PK;
			return message;
		}
	}
}
