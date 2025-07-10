using System;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public class DEAInboundMessageCreator : DEInboundMessageCreator<AtlasEDIMessage>
	{
		protected override IInterchangeCustomsDataProvider GetInterchangeDataProvider(BusinessObjectFactory factory, XmlNode customsMessageRootNode, string applicationReference)
		{
			if (ATLASResponseMessageDetails.Instance.NctsVersion10_2ResponseMessages.ContainsKey(applicationReference))
			{
				return new Messaging.ATLASVersion10_1.NctsInterchangeCustomsDataProvider(factory, customsMessageRootNode);
			}
			if (ATLASResponseMessageDetails.Instance.NctsVersion10_1ResponseMessages.ContainsKey(applicationReference))
			{
				return new Messaging.ATLASVersion10_1.NctsInterchangeCustomsDataProvider(factory, customsMessageRootNode);
			}
			if (ATLASResponseMessageDetails.Instance.TemporaryStorageVersion10_2ResponseMessages.ContainsKey(applicationReference) ||
					 ATLASResponseMessageDetails.Instance.ImportAndCollectiveVersion10_2ResponseMessages.ContainsKey(applicationReference))
			{
				throw new NotImplementedException("ATLAS version 10.2 is not implemented yet");
			}
			else
			{
				return new Messaging.ATLASVersion10_1.AtlasInterchangeCustomsDataProvider(factory, customsMessageRootNode);
			}
		}
	}
}
