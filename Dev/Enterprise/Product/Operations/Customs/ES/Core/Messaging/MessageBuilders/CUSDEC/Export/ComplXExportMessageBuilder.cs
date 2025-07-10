using CargoWise.Types;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public class ComplXExportMessageBuilder : EDIFACTMessageBuilder<IComplXExportMessageDataProvider, CUSDECMessage>
	{
		public ComplXExportMessageBuilder(IComplXExportMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override void PopulateEdifactMessage()
		{
			ComplXExportMessageTextBuilder.PopulateCUSDECMessage(edifactMessage, provider);
		}
	}
}
