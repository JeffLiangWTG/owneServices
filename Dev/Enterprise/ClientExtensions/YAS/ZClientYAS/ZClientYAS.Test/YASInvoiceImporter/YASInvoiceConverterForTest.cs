using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.YAS.YASInvoiceImporter.Testing
{
	public class YASInvoiceConverterForTest : YASInvoiceConverter
	{
		public YASInvoiceConverterForTest(INotifications subscribeNotification, BusinessObjectFactory factory) : base(subscribeNotification, factory)
		{
		}

		public void MapImportForTest(Xsd.InvoiceHeaderCollection invoices, FlatFileDataRowCollection fileLines)
		{
			base.MapImport(invoices, fileLines);
		}
	}
}
