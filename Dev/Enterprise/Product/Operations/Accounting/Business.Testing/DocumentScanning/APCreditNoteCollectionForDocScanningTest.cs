namespace Enterprise.Accounting.Business.Testing
{
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using NUnit.Framework;

	[TestedType(typeof(APCreditNoteCollectionForDocScanning))]
	public class APCreditNoteCollectionForDocScanningTest : BaseAPInvoiceCollectionForDocScanningTest
	{
		public void TestTypeOfElements()
		{
			BaseAPInvoiceCollectionForDocScanning collection = GetNewCollection();
			AssertEquals("Should be AP Invoice", typeof(APCreditNote), collection.TypeOfElements);
		}

		protected override BaseAPInvoiceCollectionForDocScanning GetNewCollection()
		{
			return new APCreditNoteCollectionForDocScanning(Factory, new ZQuery());
		}
	}
}
