using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LexpedicionV2Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class ExpeditionMessageBuilder : T2LCommonMessageBuilder<IExpeditionMessageDataProvider, T2LexpedicionV2Ent>
	{
		public ExpeditionMessageBuilder(IExpeditionMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override T2LexpedicionV2Ent GenerateXMLMessage()
		{
			return GetPopulatedT2LExpeditionCommon<T2LexpedicionV2Ent, SegmentosDeServicioTipo, ExpedidorTipo, DestinatarioTipo, PartidasDeOrdenTipo>(provider.Header, provider.Lines);
		}
	}
}
