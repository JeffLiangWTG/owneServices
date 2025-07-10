using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDocsBarcode))]
	public class StorageDocsBarcodeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return MasterFactory.NewWithValidTestData<StorageDocsBarcode>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return MasterFactory.NewWithValidTestData<StorageDocsBarcode>();
		}

		protected DocumentFactory MasterFactory
		{
			get { return masterFactory ?? (masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory())); }
		}
		DocumentFactory masterFactory;

		protected override BusinessObjectFactory NewFactory()
		{
			return MasterFactory;
		}

		[ExpectNoExceptions]
		public void TestBarcodeLength()
		{
			var barcode = MasterFactory.New<StorageDocsBarcode>();
			string barcodeString = "".PadLeft(80, 'z');
			barcode.SCB_Barcode = barcodeString;

			AssertEquals(barcodeString, barcode.SCB_Barcode);
		}
	}
}
