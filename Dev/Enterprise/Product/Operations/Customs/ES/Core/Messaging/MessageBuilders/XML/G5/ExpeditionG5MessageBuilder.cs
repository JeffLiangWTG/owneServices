using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5ExpNotifV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class ExpeditionG5MessageBuilder : G5CommonMessageBuilder<IExpeditionG5MessageDataProvider, G5ExpNotifV1Ent>
	{
		public ExpeditionG5MessageBuilder(IExpeditionG5MessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override G5ExpNotifV1Ent GenerateXMLMessage()
		{
			return new G5ExpNotifV1Ent()
			{
				EnvelopeG5 = GetPopulatedEnvelope(),
				Header = GetPopulatedHeader(provider.Header),
				GoodsItem = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};
		}
	}
}
