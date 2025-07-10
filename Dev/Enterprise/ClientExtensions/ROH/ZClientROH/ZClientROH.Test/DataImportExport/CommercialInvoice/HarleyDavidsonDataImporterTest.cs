using CargoWise.ComponentModel;
using CargoWise.IO;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.Rohlig.HarleyDavidson.Testing
{
	public class HarleyDavidsonDataImporterTest : FlatFileDataImporterTestCase
	{
		protected override string PathToTestFile
		{
			get
			{
				return (tmpTestFilePath);
			}
		}

		protected override FlatFileDataImporter GetDataImporter()
		{
			return Importer;
		}

		public void TestCreateXsd()
		{
			AssertEquals("Should be Xsd.InvoiceHeader", typeof(Xsd.InvoiceHeader), Importer.CreateXsd().GetType());
		}

		public void TestFlatFileFormat()
		{
			AssertEquals("Should be CsvFlatFileFormat", typeof(HarleyDavidsonFlatFileFormat), Importer.FlatFileFormat.GetType());
		}

		public void TestCreateConverter()
		{
			AssertEquals("Should be HarleyDavidsonConverter", typeof(HarleyDavidsonConverter), Importer.CreateConverter(new NotificationBuffer()).GetType());
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		#region Setup
		protected override void SetUp()
		{
			base.SetUp();
			Importer = new HarleyDavidsonDataImporterTestClass(JobDeclaration.New(Factory));
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			tmpTestFilePath = resourceRetriever.SaveResourceToFile("HarleyCommercialInvoice.csv");
		}

		string tmpTestFilePath;
		EmbeddedResourceRetriever resourceRetriever;
		HarleyDavidsonDataImporterTestClass Importer;
		class HarleyDavidsonDataImporterTestClass : HarleyDavidsonDataImporter
		{
			public HarleyDavidsonDataImporterTestClass(JobDeclaration jobDec) : base(jobDec)
			{
			}

			public new IFlatFileConverter CreateConverter(INotifications notifications)
			{
				return base.CreateConverter(notifications);
			}

			public new IValueObject CreateXsd()
			{
				return base.CreateXsd();
			}

			public new IFlatFileFormat FlatFileFormat
			{
				get
				{
					return base.FlatFileFormat;
				}
			}
		}
		#endregion
	}
}
