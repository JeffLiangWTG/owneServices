using System.Xml.Linq;
using CargoWise.Application;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public interface IItemDetailEInvoiceXmlBuilder
	{
		XStreamingElement BuildXml(TransactionInfo transaction, AccEInvoicingBatch accBatch);
	}

	class ItemDetailEInvoiceXmlBuilder : IItemDetailEInvoiceXmlBuilder
	{
		readonly IArgentinaEInvoicingDependencyFactory EInvoicingDependencies;

		public ItemDetailEInvoiceXmlBuilder()
		{
			EInvoicingDependencies = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetArgentinaEInvoicingDependencyFactory();
		}

		XStreamingElement IItemDetailEInvoiceXmlBuilder.BuildXml(TransactionInfo transaction, AccEInvoicingBatch accBatch)
		{
			#region SuppressResourceStringsCheckRegion

			XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
			XNamespace ser = "http://impl.service.wsmtxca.afip.gov.ar/service/";

			return new XStreamingElement(soapenv + "Envelope",
					new XAttribute(XNamespace.Xmlns + "soapenv", soapenv.NamespaceName),
					new XAttribute(XNamespace.Xmlns + "ser", ser.NamespaceName),
					new XElement(soapenv + "Header"),
					BuildItemDetailEnvoiceXml(transaction, soapenv));

			XStreamingElement BuildItemDetailEnvoiceXml(TransactionInfo transactioInfo, XNamespace soap)
			{
				return new XStreamingElement(soap + "Body",
					new XElement(ser + "autorizarComprobanteRequest",
						EInvoicingDependencies.GetAuthRequestBuilder().BuildXML(transactioInfo),
						EInvoicingDependencies.GetComprobanteCAERequestBuilder().BuildXML(transactioInfo, accBatch.Factory)));
			}
		}

		#endregion
	}
}
