using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public class TIRMessageBuilder : EDIFACTMessageBuilder<ITIRMessageDataProvider, CUSDECMessage>
	{
		public TIRMessageBuilder(ITIRMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}
		protected override void PopulateEdifactMessage() => TIRMessageTextBuilder.PopulateCUSDECMessage(edifactMessage, provider);
	}
}
