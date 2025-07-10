using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_5
{
	public class EmcsInterchangeCustomsDataProvider : IInterchangeCustomsDataProvider
	{
		public EmcsInterchangeCustomsDataProvider(BusinessObjectFactory factory, XmlNode customsMessageRootNode)
		{
			this.factory = factory;
			this.customsMessageRootNode = customsMessageRootNode;
		}

		public string MessageSubType
		{
			get
			{
				var messageGroupNote = customsMessageRootNode.SelectSingleNode($"./Header/MessageGroup");
				return messageGroupNote?.ChildNodes
					.Cast<XmlNode>()
					.FirstOrDefault(x => x.NodeType == XmlNodeType.Text)?.InnerText
					.ToUpper(CultureInfo.InvariantCulture);
			}
		}

		public string MessageType
		{
			get
			{
				var result = EDIInterchange.ApplicationCodes.Unknown;
				var messageSubType = MessageSubType;
				if (!string.IsNullOrEmpty(messageSubType) && factory.GetCachedValue<EmcsMessageSubTypeList>().ContainsCode(messageSubType))
				{
					result = EDIMessageTypeList.Codes.EMCS;
				}
				return result;
			}
		}

		public string InterchangeRecipientEORIBranch => string.Empty;

		public string LocalReferenceNumber => string.Empty;

		readonly BusinessObjectFactory factory;
		readonly XmlNode customsMessageRootNode;
	}
}
