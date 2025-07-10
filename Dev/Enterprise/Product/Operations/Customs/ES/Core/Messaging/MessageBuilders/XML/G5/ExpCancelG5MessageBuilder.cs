using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.DE;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.DG;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5ExpCancelV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class ExpCancelG5MessageBuilder : G5CommonMessageBuilder<IExpCancelG5MessageDataProvider, G5ExpCancelV1Ent>
	{
		public ExpCancelG5MessageBuilder(IExpCancelG5MessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override G5ExpCancelV1Ent GenerateXMLMessage()
		{
			return new G5ExpCancelV1Ent()
			{
				EnvelopeG5 = GetPopulatedEnvelope(),
				MrnG5 = provider.MRN,
				HeaderSimp = GetPopulatedSimplifiedHeader(provider.Header)
			};
		}

		protected HeaderSimplifiedDg GetPopulatedSimplifiedHeader(IG5SimplifiedHeader header)
		{
			return header == null ? null : new HeaderSimplifiedDg
			{
				Lrn = header.LRN,
				Declarant = GetPopulatedActor<ActorDe>(header.Declarant),
				Representative = GetPopulatedRepresentative(header.Representative),
				AdditionalInformation = header.AdditionalInfos.ConvertToCollection(GetPopulatedDocumentCommon<AdditionalInformationDe>),
			};
		}
	}
}
