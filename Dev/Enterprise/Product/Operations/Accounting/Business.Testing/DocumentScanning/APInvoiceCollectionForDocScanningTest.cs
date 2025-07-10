namespace Enterprise.Accounting.Business.Testing
{
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using NUnit.Framework;

	[TestedType(typeof(APInvoiceCollectionForDocScanning))]
	public class APInvoiceCollectionForDocScanningTest : BaseAPInvoiceCollectionForDocScanningTest
	{
		public void TestTypeOfElements()
		{
			BaseAPInvoiceCollectionForDocScanning collection = GetNewCollection();
			AssertEquals("Should be AP Invoice", typeof(APInvoice), collection.TypeOfElements);
		}

		protected override BaseAPInvoiceCollectionForDocScanning GetNewCollection()
		{
			return new APInvoiceCollectionForDocScanning(Factory, new ZQuery());
		}
	}
}
