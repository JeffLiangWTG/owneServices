using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5ExpAmendV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class ExpAmendmentG5MessageBuilder : G5CommonMessageBuilder<IExpAmendmentG5MessageDataProvider, G5ExpAmendV1Ent>
	{
		public ExpAmendmentG5MessageBuilder(IExpAmendmentG5MessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override G5ExpAmendV1Ent GenerateXMLMessage()
		{
			return new G5ExpAmendV1Ent()
			{
				EnvelopeG5 = GetPopulatedEnvelope(),
				MrnG5 = provider.MRN,
				Header = GetPopulatedHeader(provider.Header),
				GoodsItem = provider.Lines.ConvertToCollection(GetPopulatedLine)
			};
		}
	}
}
