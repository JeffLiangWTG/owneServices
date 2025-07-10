using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingQueryRequestEDIMessagePrettier : CDSEDIMessagePrettier<CDSInventoryLinkingQueryRequestEDIMessage>
	{
		public CDSInventoryLinkingQueryRequestEDIMessagePrettier(CDSInventoryLinkingQueryRequestEDIMessage message) : base(message)
		{
			request = message.MessageDataObject;
		}

		public override ZString MakeHumanReadable()
		{
			var interpretation = MessagePrettierCss.CSS
				+ ToH3IfNotEmpty(Invariant($"Inventory Linking Query Request to CDS:"))
				+ ToKeyValuePairSection(UCRHelper.GetUCRKeyValuePairs(request.queryUCR));
			return interpretation;
		}

		readonly CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking.inventoryLinkingQueryRequest request;
	}
}
