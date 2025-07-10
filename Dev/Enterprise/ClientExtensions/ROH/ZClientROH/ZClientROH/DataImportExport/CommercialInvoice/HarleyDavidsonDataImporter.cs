using CargoWise.ComponentModel;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.Rohlig.HarleyDavidson
{
	public class HarleyDavidsonDataImporter : FlatFileDataImporter
	{
		public HarleyDavidsonDataImporter(JobDeclaration jobDec) : base(jobDec)
		{
			this.JobDec = jobDec;
		}

		protected override IFlatFileConverter CreateConverter(INotifications notifications)
		{
			return new HarleyDavidsonConverter(notifications, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.InvoiceHeader();
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new HarleyDavidsonFlatFileFormat(); }
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			Xsd.InvoiceHeader invoiceHeader = (Xsd.InvoiceHeader)xSD;

			InvoiceValueObjectDataAdapter dataAdapter = new InvoiceValueObjectDataAdapter(JobDec);
			ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, notifications);
			((IValueObjectDataAdapter)dataAdapter).ImportFromValueObject(JobDec.Invoices.AddNew(), invoiceHeader, importContext);

			return true;
		}

		protected readonly JobDeclaration JobDec;
	}
}

#region Setup
#endregion
