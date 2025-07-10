using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MailManager;
using Enterprise.MailManager.MailFilters;

namespace Enterprise.DataTransfer.GUI.Testing
{
	sealed class DataImporterFormFileFilterTest : TestCaseWithFactory
	{
		public void TestImportFileFilter()
		{
			using (DataImporterFormForTest form = DataImporterFormForTest.Create(BillingInterfaceName.Test))
			{
				form.Importer = new DataImporterForTest();
				Assert("Plain filter should contain *.txt by default", form.ImportFileFilter.Contains("*.txt"));
			}
		}

		public void TestImportFileFilterUsingFlatFileDataImporter()
		{
			using (DataImporterFormForTest form = DataImporterFormForTest.Create(BillingInterfaceName.Test))
			{
				form.Importer = new FlatFileImporterForTest();
				Assert("Filter is now Csv files (FlatFileImporterForTest class uses CSV files)", form.ImportFileFilter.Contains("*.csv"));
			}
		}

		public void TestImportFileFilterUsingSubclass()
		{
			using (DataImportFormWithImportFileFilter form = DataImportFormWithImportFileFilter.Create(BillingInterfaceName.Test))
			{
				form.Importer = new FlatFileImporterForTest();
				Assert("Even though a flat file importer is supplied, we are using the subclass' import file filter -> backwards compatibility", form.ImportFileFilterForTest.Contains("*.abc"));
			}
		}

		class DataImportFormWithImportFileFilter : DataImporterForm
		{
			public DataImportFormWithImportFileFilter(DataImporterBusinessObject businessEntity, string formCaption, BillingInterfaceName interfaceName)
				: base(businessEntity, formCaption, interfaceName) { }

			public new static DataImportFormWithImportFileFilter Create(BillingInterfaceName interfaceName)
			{
				return new DataImportFormWithImportFileFilter(new DataImporterBusinessObject(new BusinessObjectFactory()), null, interfaceName);
			}

			protected override string ImportFileFilter
			{
				get { return "abc files (*.abc)|*.abc"; }
			}

			public ZString ImportFileFilterForTest
			{
				get { return ImportFileFilter; }
			}
		}

		class FlatFileImporterForTest : FlatFileDataImporter
		{
			protected override Enterprise.DataTransfer.Xml.IValueObject CreateXsd()
			{
				return null;
			}

			protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
			{
				return false;
			}

			protected override IFlatFileFormat FlatFileFormat
			{
				get { return new CsvFlatFileFormat(); }
			}

			protected override IFlatFileConverter CreateConverter(INotifications notifications)
			{
				return null;
			}

			protected override IMailFilter GetMailItemFilter() => QueryMailFilter.AllQueuedItems_ForTesting;
		}
	}
}
