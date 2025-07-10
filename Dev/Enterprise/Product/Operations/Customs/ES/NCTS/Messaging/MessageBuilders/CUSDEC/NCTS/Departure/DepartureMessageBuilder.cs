using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public class DepartureMessageBuilder : EDIFACTMessageBuilder<IDepartureMessageDataProvider, CUSDECMessage>
	{
		public DepartureMessageBuilder(IDepartureMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override void PopulateEdifactMessage() => DepartureMessageTextBuilder.PopulateCUSDECMessage(edifactMessage, provider);
	}
}
