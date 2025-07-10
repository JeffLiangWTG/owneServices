using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LrecepcionModificaV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class ReceptionAmendmentMessageBuilder : T2LCommonMessageBuilder<IReceptionAmendmentMessageDataProvider, T2LrecepcionModificaV1Ent>
	{
		public ReceptionAmendmentMessageBuilder(IReceptionAmendmentMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override T2LrecepcionModificaV1Ent GenerateXMLMessage()
		{
			return GetPopulatedT2LReceptionCommon<T2LrecepcionModificaV1Ent, SegmentosDeServicioTipo, PartidasDeOrdenTipo>(provider.Header, provider.Lines);
		}
	}
}
