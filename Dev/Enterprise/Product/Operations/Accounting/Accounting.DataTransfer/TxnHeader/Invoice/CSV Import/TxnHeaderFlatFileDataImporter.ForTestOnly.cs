#if DEBUG

using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile
{
	public partial class TxnHeaderFlatFileDataImporter
	{
		public IFlatFileConverter CreateConverter_ForTestOnly(INotifications notificationSubscriber)
		{
			return CreateConverter(notificationSubscriber);
		}

		public IValueObject CreateXsd_ForTestOnly()
		{
			return CreateXsd();
		}

		public IFlatFileFormat FlatFileFormat_ForTestOnly => FlatFileFormat;

		public bool ExtractToDataAdapter_ForTestOnly(IValueObject xSD, INotifications notifications)
		{
			return ExtractToDataAdapter(xSD, notifications);
		}

		public Business.ARAP.Invoicing.InvoicingBase FLastImportedInvoice_ForTestOnly
		{
			get { return fLastImportedInvoice; }
			set { fLastImportedInvoice = value; }
		}
	}
}

#endif
