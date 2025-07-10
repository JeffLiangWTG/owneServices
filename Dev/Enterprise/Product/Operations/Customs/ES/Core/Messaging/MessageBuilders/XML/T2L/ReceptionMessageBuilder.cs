using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LrecepcionV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class ReceptionMessageBuilder : T2LCommonMessageBuilder<IReceptionMessageDataProvider, T2LrecepcionV1Ent>
	{
		public ReceptionMessageBuilder(IReceptionMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override T2LrecepcionV1Ent GenerateXMLMessage()
		{
			return GetPopulatedT2LReceptionCommon<T2LrecepcionV1Ent, SegmentosDeServicioTipo, PartidasDeOrdenTipo>(provider.Header, provider.Lines);
		}
	}
}
