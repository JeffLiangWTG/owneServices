using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5RecNotifV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class ReceptionG5MessageBuilder : G5CommonMessageBuilder<IReceptionG5MessageDataProvider, G5RecNotifV1Ent>
	{
		public ReceptionG5MessageBuilder(IReceptionG5MessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override G5RecNotifV1Ent GenerateXMLMessage()
		{
			return new G5RecNotifV1Ent()
			{
				EnvelopeG5 = GetPopulatedEnvelope(),
				MrnG5 = provider.MRN,
				Header = GetPopulatedHeader(provider.Header),
				GoodsItem = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};
		}
	}
}
