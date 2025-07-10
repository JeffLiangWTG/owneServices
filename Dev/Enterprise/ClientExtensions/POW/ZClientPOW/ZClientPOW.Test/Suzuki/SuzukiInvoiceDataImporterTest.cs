using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;

namespace Enterprise.Client.ZClientPOW.Suzuki.Testing
{
	sealed class SuzukiInvoiceDataImporterTest : FlatFileInvoiceDataImporterAbstractTest
	{
		public void TestDataReader()
		{
			AssertEquals(typeof(SuzukiInvoiceDataFileReader), Importer.DataReader.GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var toJobDec = Factory.New<BaseJobDeclaration>();
			Importer = new SuzukiInvoiceDataImporterTestClass("", toJobDec);
		}

		SuzukiInvoiceDataImporterTestClass Importer;
		class SuzukiInvoiceDataImporterTestClass : SuzukiInvoiceDataImporter
		{
			public SuzukiInvoiceDataImporterTestClass(string fileName, BaseJobDeclaration toJobDec) : base(fileName, toJobDec)
			{
			}

			internal FileDataReader DataReader
			{
				get
				{
					SetDataReader();
					return dataReader;
				}
			}
		}
	}
}
