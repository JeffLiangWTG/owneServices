using CargoWise.Types;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class ArrivalAtExitExportMessageBuilder : EDIFACTMessageBuilder<IArrivalAtExitExportMessageDataProvider, CUSDECMessage>
	{
		public ArrivalAtExitExportMessageBuilder(IArrivalAtExitExportMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override void PopulateEdifactMessage()
		{
			ArrivalAtExitExportMessageTextBuilder.PopulateCUSDECMessage(edifactMessage, provider);
		}
	}
}
