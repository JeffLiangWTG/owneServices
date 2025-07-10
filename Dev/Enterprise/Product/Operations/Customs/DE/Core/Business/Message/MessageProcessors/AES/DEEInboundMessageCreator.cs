using System;
using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class DEEInboundMessageCreator : DEInboundMessageCreator<AesEDIMessage>
	{
		protected override IInterchangeCustomsDataProvider GetInterchangeDataProvider(BusinessObjectFactory factory, XmlNode customsMessageRootNode, string applicationReference)
		{
			if (AesResponseMessageDetails.Instance.AesVersion3_0ResponseMessages.ContainsKey(applicationReference))
			{
				return new Messaging.AESVersion3_0.AesInterchangeCustomsDataProvider(factory, customsMessageRootNode);
			}
			else
			{
				throw new NotImplementedException($"Message type '{applicationReference}' not implemented");
			}
		}
	}
}
