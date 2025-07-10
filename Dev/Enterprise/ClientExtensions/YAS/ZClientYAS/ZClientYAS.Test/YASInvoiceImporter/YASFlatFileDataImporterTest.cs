using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.YAS.YASInvoiceImporter.Testing
{
	public class YASFlatFileDataImporterTest : FlatFileDataImporterTestCase
	{
		#region Implementation
		public void TestFlatFileFormat()
		{
			AssertEquals("Should have YAS flat file format", typeof(YASInvoiceFlatFileFormat), Importer.FlatFileFormatForTest.GetType());
		}

		public void TestObject()
		{
			AssertEquals("Object is InvoiceHeaderCollection", typeof(Xsd.InvoiceHeaderCollection), Importer.ObjectForTest.GetType());
		}

		public void TestConverter()
		{
			AssertEquals("Converter is YASInvoiceConverter", typeof(YASInvoiceConverter), Importer.ConverterForTest.GetType());
		}

		protected override FlatFileDataImporter GetDataImporter()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			return new YASFlatFileDataImporter(declaration);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		protected override string PathToTestFile
		{
			get
			{
				return BaseSourcePath + @"Enterprise\ClientExtensions\YAS\ZClientYAS\ZClientYAS.Test\YASInvoiceImporter\Test\9026931.dat";
			}
		}

		YASFlatFileDataImporterForTest Importer;
		protected override void SetUp()
		{
			base.SetUp();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Importer = new YASFlatFileDataImporterForTest(declaration);
		}

		public class YASFlatFileDataImporterForTest : YASFlatFileDataImporter
		{
			public YASFlatFileDataImporterForTest(BaseJobDeclaration declaration) : base(declaration)
			{
			}

			public IFlatFileFormat FlatFileFormatForTest
			{
				get
				{
					return base.FlatFileFormat;
				}
			}

			public IValueObject ObjectForTest
			{
				get
				{
					return base.CreateXsd();
				}
			}

			public IFlatFileConverter ConverterForTest
			{
				get
				{
					return base.CreateConverter(new NotificationBuffer());
				}
			}
		}
		#endregion
	}
}
