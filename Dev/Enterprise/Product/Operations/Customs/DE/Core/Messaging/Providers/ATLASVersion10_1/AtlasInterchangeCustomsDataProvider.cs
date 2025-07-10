using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class AtlasInterchangeCustomsDataProvider : IInterchangeCustomsDataProvider
	{
		public AtlasInterchangeCustomsDataProvider(BusinessObjectFactory factory, XmlNode customsMessageRootNode)
		{
			this.factory = factory;
			this.customsMessageRootNode = customsMessageRootNode;
		}

		public string MessageSubType
		{
			get
			{
				var messageGroupNote = customsMessageRootNode.SelectSingleNode($"./MetaData/MessageGroup") ?? customsMessageRootNode.SelectSingleNode($"./messageGroup");
				return messageGroupNote?.ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.NodeType == XmlNodeType.Text)?.InnerText.ToUpper(CultureInfo.InvariantCulture);
			}
		}

		public string MessageType
		{
			get
			{
				var result = EDIInterchange.ApplicationCodes.Unknown;
				var messageSubType = MessageSubType;
				if (!string.IsNullOrEmpty(messageSubType))
				{
					if (factory.GetCachedValue<TemporaryStorageMessageSubTypeList>().ContainsCode(messageSubType))
					{
						result = EDIMessageTypeList.Codes.TemporaryStorage;
					}
					else if (factory.GetCachedValue<NctsMessageSubTypeList>().ContainsCode(messageSubType))
					{
						result = EDIMessageTypeList.Codes.NCTS;
					}
					else if (factory.GetCachedValue<ImportMessageSubTypeList>().ContainsCode(messageSubType) || factory.GetCachedValue<MonthlyClosingMessageSubTypeList>().ContainsCode(messageSubType))
					{
						result = EDIMessageTypeList.Codes.Import;
					}
				}
				return result;
			}
		}

		public string InterchangeRecipientEORIBranch => customsMessageRootNode.SelectSingleNode("./MetaData/InterchangeRecipient/Identification/SubsidiaryNumber")?.InnerText
			?? customsMessageRootNode.SelectSingleNode("./MessageRecipient/subsidiaryNumber")?.InnerText;

		public string LocalReferenceNumber => customsMessageRootNode.SelectSingleNode("./Header/LRN")?.InnerText ?? customsMessageRootNode.SelectSingleNode("./Header/LocalReferenceNumber")?.InnerText;

		readonly BusinessObjectFactory factory;
		readonly XmlNode customsMessageRootNode;
	}
}
