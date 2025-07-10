using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.ConsultaDVDH2V1Ent;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DVDT;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class QueryDVDMessageBuilder : DVDCommonMessageBuilder<IQueryDVDMessageDataProvider, ConsultaDvdh2V1Ent>
	{
		public QueryDVDMessageBuilder(IQueryDVDMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ConsultaDvdh2V1Ent GenerateXMLMessage()
		{
			return new ConsultaDvdh2V1Ent()
			{
				Mensaje = GetPopulatedMessage()
			};
		}

		TdMensaje GetPopulatedMessage()
		{
			var message = GetPopulatedCommonMessage<TdMensaje>();
			if (message != null)
			{
				if (provider.RequestATCData)
				{
					message.IndicadorDatosAtc = TdIndicadorAtc.S;
					message.IndicadorDatosAtcValueSpecified = provider.RequestATCData;
				}
			}
			return message;
		}
	}
}
