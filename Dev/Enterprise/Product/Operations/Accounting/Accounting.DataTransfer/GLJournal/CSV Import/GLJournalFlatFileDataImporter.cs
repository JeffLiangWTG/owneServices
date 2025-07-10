using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public partial class GLJournalFlatFileDataImporter : FlatFileDataImporter, Accounting.Integration.IGLJournalFlatFileDataImporter
	{
		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new GLJournalFlatFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.GLJournal();
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CsvFlatFileFormat(false); }
		}

		protected override bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
		{
			fLastImportedJournal = CreateGLJournal();

			ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, notifications);
			Adapter.ImportFromValueObject(fLastImportedJournal, (Xsd.GLJournal)xsd, importContext);
			return false;
		}

		protected virtual GLJournal CreateGLJournal()
		{
			return FactoryProvider.Current.New<GLJournal>();
		}

		public GLJournal LastImportedJournal
		{
			get { return fLastImportedJournal; }
		}

		GLJournal fLastImportedJournal;

		protected virtual GLJournalDataAdapter Adapter => new GLJournalDataAdapter();
	}
}
