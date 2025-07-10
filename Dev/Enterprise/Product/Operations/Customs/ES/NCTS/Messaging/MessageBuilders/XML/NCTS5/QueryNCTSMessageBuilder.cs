using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTRAC_v515.CCTRACV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class QueryNCTSMessageBuilder : NCTSCommonMessageBuilder<IQueryNCTSMessageDataProvider, Cctracv1Ent>
	{
		public QueryNCTSMessageBuilder(IQueryNCTSMessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CCTRAC";

		protected override Cctracv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Cctracv1Ent>();
			if (declaration != null)
			{
				declaration.Cctrac = GetPopulatedCCTRACType();
			}
			return declaration;
		}

		CctracType GetPopulatedCCTRACType()
		{
			var cCTRACv515 = GetPopulatedMessage<CctracType>();
			if (cCTRACv515 != null)
			{
				cCTRACv515.TransitOperation = GetPopulatedCommonTransitOperationMRN<TransitOperationTypeTrac>(provider.TransitOperation);
			}
			return cCTRACv515;
		}
	}
}
