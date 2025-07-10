using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0
{
	public class AesInterchangeCustomsDataProvider : IInterchangeCustomsDataProvider
	{
		public AesInterchangeCustomsDataProvider(BusinessObjectFactory factory, XmlNode customsMessageRootNode)
		{
			this.factory = factory;
			this.customsMessageRootNode = customsMessageRootNode;
		}

		public string MessageSubType => customsMessageRootNode.SelectSingleNode("./messageGroup")?.InnerText;

		public string MessageType
		{
			get
			{
				var result = EDIInterchange.ApplicationCodes.Unknown;
				var messageSubType = MessageSubType;
				if (!string.IsNullOrEmpty(messageSubType) && factory.GetCachedValue<ExportMessageSubTypeList>().ContainsCode(messageSubType))
				{
					result = EDIMessageTypeList.Codes.AES;
				}
				return result;
			}
		}

		public string InterchangeRecipientEORIBranch => customsMessageRootNode.SelectSingleNode("./MessageRecipient/subsidiaryNumber")?.InnerText;

		public string LocalReferenceNumber => customsMessageRootNode.SelectSingleNode("./ExportOperation/LRN")?.InnerText;

		readonly BusinessObjectFactory factory;
		readonly XmlNode customsMessageRootNode;
	}
}
