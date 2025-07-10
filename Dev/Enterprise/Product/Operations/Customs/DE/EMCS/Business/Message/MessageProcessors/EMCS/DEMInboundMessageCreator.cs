using System.Xml;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class DEMInboundMessageCreator : DEInboundMessageCreator<EmcsEDIMessage>
	{
		protected override IInterchangeCustomsDataProvider GetInterchangeDataProvider(BusinessObjectFactory factory, XmlNode customsMessageRootNode, string applicationReference)
		{
			if (EmcsResponseMessageDetails.Instance.Version2_5ResponseMessages.ContainsKey(applicationReference))
			{
				return new Messaging.Version2_5.EmcsInterchangeCustomsDataProvider(factory, customsMessageRootNode);
			}
			else
			{
				return new Messaging.Version2_4.EmcsInterchangeCustomsDataProvider(factory, customsMessageRootNode);
			}
		}
	}
}
