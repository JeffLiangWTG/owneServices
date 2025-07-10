using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WCB
{
	public class WCBDataImporter : FlatFileDataImporter
	{
		public WCBDataImporter(JobDeclaration jobDec)
			: base(jobDec)
		{
			Format = FileFormat.Unknown;
		}

		public void SetFileFormat(FileFormat format)
		{
			this.Format = format;
		}

		protected JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)BusinessEntity; }
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			Xsd.InvoiceHeader invoiceHeader = xSD as Xsd.InvoiceHeader;

			AUInvoiceValueObjectDataAdapter dataAdapter = new AUInvoiceValueObjectDataAdapter(JobDeclaration);
			ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, notifications);
			((IValueObjectDataAdapter)dataAdapter).ImportFromValueObject(JobDeclaration.Invoices.AddNew(), invoiceHeader, importContext);

			return true;
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new WCBFlatFileFormat(); }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			FlatFileConverter result = null;
			switch (Format)
			{
				case FileFormat.Mercedes:
					result = new DaimlerDataConverter(notifications, FactoryProvider.Current);
					break;

				case FileFormat.Freightliner:
					result = new FreightlinerDataConverter(notifications, FactoryProvider.Current);
					break;

				default:
					result = new WCBDataConverter(notifications, FactoryProvider.Current);
					break;
			}
			return result;
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.InvoiceHeader();
		}

		protected override (bool Result, System.IO.TextReader ReaderOut, byte[] Hash) IsValidDataBeforeImport(System.IO.TextReader reader, INotifications notifications)
		{
			var result = (Result: false, ReaderOut: reader, Hash: (byte[])null);

			if (Format == FileFormat.Unknown)
			{
				notifications.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, "The Input File cannot be determined as either a Freightliner invoice or a Daimler invoice."));
			}
			else
			{
				result = base.IsValidDataBeforeImport(reader, notifications);
			}

			return result;
		}

		FileFormat Format;
	}
}
