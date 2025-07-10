using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LexpedicionModificaV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class ExpeditionAmendmentMessageBuilder : T2LCommonMessageBuilder<IExpeditionAmendmentMessageDataProvider, T2LexpedicionModificaV1Ent>
	{
		public ExpeditionAmendmentMessageBuilder(IExpeditionAmendmentMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override T2LexpedicionModificaV1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedT2LExpeditionCommon<T2LexpedicionModificaV1Ent, SegmentosDeServicioTipo, ExpedidorTipo, DestinatarioTipo, PartidasDeOrdenTipo>(provider.Header, provider.Lines);
			declaration.NumeroDeReferenciaDelT2LExpedicion = provider.Header.ExpeditionT2LReference;

			return declaration;
		}
	}
}
