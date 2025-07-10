using System.Linq;
using System.Xml;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	public class InventoryLinkingControlResponse : InventoryLinkingResponse
	{
		public InventoryLinkingControlResponse(ZString xml) : base(xml)
		{
		}

		protected override ZString RootNode => "inventoryLinkingControlResponse";

		public ZString ActionCode => SelectSingleNode(ActionCodeNodeXPath);

		public ZString UCR => SelectSingleNode(UCRNodeXPath);

		public ZString UCRType => SelectSingleNode(UCRTypeNodeXPath);

		public ZString[] ErrorCodes => SelectNodes(ErrorCodeNodeXPath).Cast<XmlNode>()
			.Select(node => (ZString)node.InnerText)
			.Where(error => !error.IsEmpty)
			.ToArray();

		ZString ActionCodeNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='actionCode']");
		ZString UCRNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='ucr']/*[local-name()='ucr']");
		ZString UCRTypeNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='ucr']/*[local-name()='ucrType']");
		ZString ErrorCodeNodeXPath => System.FormattableString.Invariant($"//*[local-name()='{RootNode}']/*[local-name()='error']/*[local-name()='errorCode']");
	}
}
