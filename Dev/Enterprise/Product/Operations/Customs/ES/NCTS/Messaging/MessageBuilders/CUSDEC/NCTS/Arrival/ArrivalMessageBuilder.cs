using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This class is used to create the message")]
	public class ArrivalMessageBuilder : EDIFACTMessageBuilder<IArrivalMessageDataProvider, CUSDECMessage>
	{
		public ArrivalMessageBuilder(IArrivalMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override void PopulateEdifactMessage() => ArrivalMessageTextBuilder.PopulateCUSDECMessage(edifactMessage, provider);
	}
}
