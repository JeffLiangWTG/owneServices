using CargoWise.Types;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class DUAExportMessageBuilder : EDIFACTMessageBuilder<IDUAExportMessageDataProvider, CUSDECMessage>
	{
		public DUAExportMessageBuilder(IDUAExportMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override void PopulateEdifactMessage()
		{
			DUAExportMessageTextBuilder.PopulateCUSDECMessage(edifactMessage, provider);
		}
	}
}
