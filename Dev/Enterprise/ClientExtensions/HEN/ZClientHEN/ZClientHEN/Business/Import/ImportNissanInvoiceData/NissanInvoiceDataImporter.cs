
using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.HEN.Nissan
{
	public class NissanInvoiceDataImporter : FlatFileDataImporter
	{
		public NissanInvoiceDataImporter(JobDeclaration jobDec)
			: base(jobDec)
		{
			this.JobDec = jobDec;
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new NissanInvoiceConverter(notifications, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.InvoiceHeader();
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new NissanInvoiceFlatFileFormat(); }
		}

		protected override bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
		{
			Xsd.InvoiceHeader invoiceHeader = (Xsd.InvoiceHeader)xsd;

			InvoiceValueObjectDataAdapter dataAdapter = new InvoiceValueObjectDataAdapter(JobDec);
			ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, notifications);
			((IValueObjectDataAdapter)dataAdapter).ImportFromValueObject(JobDec.Invoices.AddNew(), invoiceHeader, importContext);

			return true;
		}

		protected readonly JobDeclaration JobDec;
	}
}
