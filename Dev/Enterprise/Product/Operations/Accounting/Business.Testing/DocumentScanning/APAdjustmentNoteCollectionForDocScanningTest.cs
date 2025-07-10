namespace Enterprise.Accounting.Business.Testing
{
	using CargoWise.EntityFramework;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using NUnit.Framework;

	[TestedType(typeof(APAdjustmentNoteCollectionForDocScanning))]
	public class APAdjustmentNoteCollectionForDocScanningTest : BaseAPInvoiceCollectionForDocScanningTest
	{
		public void TestTypeOfElements()
		{
			BaseAPInvoiceCollectionForDocScanning collection = GetNewCollection();
			AssertEquals("Should be AP Invoice", typeof(APAdjustmentNote), collection.TypeOfElements);
		}

		protected override BaseAPInvoiceCollectionForDocScanning GetNewCollection()
		{
			return new APAdjustmentNoteCollectionForDocScanning(Factory, new ZQuery());
		}
	}
}
