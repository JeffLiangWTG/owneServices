
using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.AUS.Declaration
{
	public class AUSDataImporter : FlatFileDataImporter
	{
		public AUSDataImporter(JobDeclaration jobDec) : base(jobDec)
		{
		}

		protected JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)BusinessEntity; }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			FlatFileConverter result = new DataConverter(notificationSubscriber, FactoryProvider.Current, JobDeclaration.Importer.PK, JobDeclaration.Supplier.PK);
			return result;
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.InvoiceHeaderCollection();
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get
			{
				return new AUSFlatFileFormat();
			}
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			Xsd.InvoiceHeaderCollection invHeaderCollection = xSD as Xsd.InvoiceHeaderCollection;

			AUInvoiceValueObjectDataAdapter dataAdapter = new AUInvoiceValueObjectDataAdapter(JobDeclaration);
			ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, notifications);

			foreach (Xsd.InvoiceHeader invHeader in invHeaderCollection)
			{
				((IValueObjectDataAdapter)dataAdapter).ImportFromValueObject(JobDeclaration.Invoices.AddNew(), invHeader, importContext);
			}

			return true;
		}
	}
}
