using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRBillCollectionForEntry))]
	public class AUBillCollectionForEntryTest : BillCollectionForEntryTest
	{
		public void TestBillsCollectionWhenMasterWithChildrenIsUsed()
		{
			Customs.Business.Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.CU_MasterBill = "MBL";

			Customs.Business.Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill2.CU_MasterBill = "MBL";
			bill2.CU_HouseBill = "HBL1";

			Customs.Business.Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill3.CU_MasterBill = "MBL";
			bill3.CU_HouseBill = "HBL2";

			Customs.Business.CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_CU_RelatedHouseBill = bill1.PK;
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals(2, entryHeader.Bills.Count);
			AssertCollectionContains(bill2, entryHeader.Bills);
			AssertCollectionContains(bill3, entryHeader.Bills);
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			declaration = SetDeclaration();
			entryHeader = declaration.CustomsEntryHeaders[0];
			return new CMRBillCollectionForEntry(entryHeader);
		}
		#endregion
	}
}
