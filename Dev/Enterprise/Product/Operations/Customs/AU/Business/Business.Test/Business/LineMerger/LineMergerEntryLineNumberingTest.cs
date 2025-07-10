using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class LineMergerEntryLineNumberingTest : Customs.Business.Testing.LineMergerEntryLineNumberingTest
	{
		public void TestAssignLineNumberToPackingGroups()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_MessageType = "IMP";
			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JobComInvoiceLines.AddNew();
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JobComInvoiceLines.AddNew();

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "1";
			Customs.Business.Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_HouseBill = "2";

			var pack1 = declaration.Packages.AddNew();
			pack1.CW_HouseBill = houseBill1.CU_BillUniqueCode;

			var pack2 = declaration.Packages.AddNew();
			pack2.CW_HouseBill = houseBill2.CU_BillUniqueCode;

			declaration.DoMerge();

			var entryHeader = declaration.CustomsEntryHeaders.Single();
			entryHeader.HighHouseContPivotNoManager.AssignLineNumbers();

			AssertEquals("Count for HouseBillContainer", 2, declaration.PackingGroups.Count);
			Assert("Line Number", declaration.PackingGroups[0].CR_HouseContainerNumber > 0);
			Assert("Line Number", declaration.PackingGroups[1].CR_HouseContainerNumber > 0);
			AssertEquals("Should not update HighHouseContPivotNo", ZShort.Zero, entryHeader.HighHouseContPivotNo);
		}

		protected override string GetClearStatus()
		{
			return CustomsEntryStatus.ClearFormalLodge.Code;
		}

		protected override Customs.Business.LineMerger GetNewLineMerger(BaseJobDeclaration declaration)
		{
			return new LineMerger((JobDeclaration)declaration);
		}

		public override void TestLineNumbersDoNotGetReusedWhenLineIsDeletedAfterSendingMessageToCustoms()
		{
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			base.TestExistingLinesKeepNumbersAfterSubmissionToCustoms();
		}

		public override void TestExistingLinesKeepNumbersAfterSubmissionToCustoms()
		{
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			base.TestExistingLinesKeepNumbersAfterSubmissionToCustoms();
		}
	}
}
