using CargoWise.Customs.ES.MessageDefinitions.Version1.H7.ConsultaH7V1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class QueryH7MessageBuilder : H7CommonMessageBuilder<IQueryH7MessageDataProvider, ConsultaH7V1Ent>
	{
		public QueryH7MessageBuilder(IQueryH7MessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ConsultaH7V1Ent GenerateXMLMessage()
		{
			return new ConsultaH7V1Ent
			{
				Message = GetPopulatedMessage(),
				Declaration = new DeclarationMrnTd()
				{
					MrnH7 = provider.DeclarationMRN,
					MrnG3 = provider.G3DeclarationMRN,
					MrnH7Next = provider.NextH7DeclarationMRN
				}
			};
		}
	}
}
