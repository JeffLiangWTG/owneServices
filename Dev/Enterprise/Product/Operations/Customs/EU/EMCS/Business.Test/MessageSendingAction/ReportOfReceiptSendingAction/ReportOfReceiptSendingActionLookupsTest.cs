using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class ReportOfReceiptSendingActionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReceiptResultList()
		{
			var receiptResultList = lookups.ReceiptResultList;

			CombineAssertions(() =>
			{
				AssertSame(receiptResultList, receiptResultList);
				AssertEquals("1, 2, 3, 4, 21, 22, 23", receiptResultList.CodesAsString);
			});
		}

		public void TestReceiptResultList_RefusedQuantityGreater0()
		{
			var line = jobDeclaration.InvoiceLines.AddNew();
			line.Outturn.C5_RejectedQuantity = 1;

			var receiptResultList = lookups.ReceiptResultList;

			CombineAssertions(() =>
			{
				AssertSame(receiptResultList, receiptResultList);
				AssertEquals("4", receiptResultList.CodesAsString);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobDeclaration = Factory.New<EMCSJobDeclaration>();
			lookups = new ReportOfReceiptSendingActionLookups(new ReportOfReceiptSendingAction(jobDeclaration));
		}
		EMCSJobDeclaration jobDeclaration;
		ReportOfReceiptSendingActionLookups lookups;
	}
}
