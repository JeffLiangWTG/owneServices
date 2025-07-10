using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingConsolidationRequestEDIMessagePrettier : CDSEDIMessagePrettier<CDSInventoryLinkingConsolidationRequestEDIMessage>
	{
		public CDSInventoryLinkingConsolidationRequestEDIMessagePrettier(CDSInventoryLinkingConsolidationRequestEDIMessage message) : base(message)
		{
			request = message.MessageDataObject;
		}

		public override ZString MakeHumanReadable()
		{
			var interpretation = MessagePrettierCss.CSS
				+ ToH3IfNotEmpty(Invariant($"Inventory Linking Consolidation Request to CDS:"))
				+ ToKeyValuePairSection(new (ZString key, ZString value)[]
				{
					("Close consol ", request.masterUCR)
				});
			return interpretation;
		}
		readonly CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking.inventoryLinkingConsolidationRequest request;
	}
}
