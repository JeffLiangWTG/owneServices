using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface IInvoicingBaseToXmlConverter
	{
		(bool Success, string Xml) ConvertToXml(InvoicingBase invoicingBase, INotifications notifications);
	}
}
