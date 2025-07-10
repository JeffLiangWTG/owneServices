using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class NctsInterchangeCustomsDataProvider : IInterchangeCustomsDataProvider
	{
		public NctsInterchangeCustomsDataProvider(BusinessObjectFactory factory, XmlNode customsMessageRootNode)
		{
			this.factory = factory;
			this.customsMessageRootNode = customsMessageRootNode;
		}

		public string MessageSubType
		{
			get
			{
				var messageGroupNote = customsMessageRootNode.SelectSingleNode("./messageGroup");
				return messageGroupNote?.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.NodeType == XmlNodeType.Text)?.InnerText.ToUpper(CultureInfo.InvariantCulture);
			}
		}

		public string MessageType => MessageSubType != null && factory.GetCachedValue<NctsMessageSubTypeList>().ContainsCode(MessageSubType)
			? EDIMessageTypeList.Codes.NCTS
			: EDIInterchange.ApplicationCodes.Unknown;

		public string InterchangeRecipientEORIBranch => customsMessageRootNode.SelectSingleNode("./MessageRecipient/subsidiaryNumber")?.InnerText;

		public string LocalReferenceNumber => customsMessageRootNode.SelectSingleNode("./TransitOperation/LRN")?.InnerText;

		readonly BusinessObjectFactory factory;
		readonly XmlNode customsMessageRootNode;
	}
}
