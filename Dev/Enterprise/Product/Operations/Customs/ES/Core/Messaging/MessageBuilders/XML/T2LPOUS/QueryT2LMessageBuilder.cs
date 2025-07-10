using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01CONSV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class QueryT2LMessageBuilder : XMLMessageBuilder<IQueryT2LMessageDataProvider, Iep01ConsEntType>
	{
		public QueryT2LMessageBuilder(IQueryT2LMessageDataProvider provider, ZString messageType, ZString messageSubType) : base(provider, messageType, messageSubType)
		{
		}

		protected override Iep01ConsEntType GenerateXMLMessage() => GetPopulatedIep01ConsEntType();

		Iep01ConsEntType GetPopulatedIep01ConsEntType()
		{
			return new Iep01ConsEntType
			{
				Message = GetPopulatedMessage(),
				Mrnt2L = provider.MRN
			};
		}

		MessageTdC GetPopulatedMessage()
		{
			return new MessageTdC
			{
				MessageIdentification = TransactionId,
				PreparationDateAndTime = CET.ToCustomsFormatDateStringyyyyMMddTHHmmss(),
			};
		}
	}
}
