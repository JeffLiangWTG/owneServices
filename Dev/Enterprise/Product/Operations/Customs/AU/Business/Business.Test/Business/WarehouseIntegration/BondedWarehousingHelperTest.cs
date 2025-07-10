using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BondedWarehousingHelperTest : TestCaseWithFactory
	{
		public void TestIsMarkedForBondedWarehousing()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var helper = new BondedWarehousingHelper(declaration);
			AssertEquals(false, helper.IsMarkedForBondedWarehousing(invoiceLine));
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			AssertEquals(true, helper.IsMarkedForBondedWarehousing(invoiceLine));
			invoiceLine.SetIsGoingIntoBondedWarehouseCoreForTesting(false);
			AssertEquals(false, helper.IsMarkedForBondedWarehousing(invoiceLine));
			invoiceLine.SetUseBondedWarehouseAutomationForTesting(true);
			AssertEquals(false, helper.IsMarkedForBondedWarehousing(invoiceLine));
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(true, helper.IsMarkedForBondedWarehousing(invoiceLine));
		}

		public void TestHasBondedWarehouseEntryDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var helper = new BondedWarehousingHelper(declaration);
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			invoiceLine.AddInfo.ZA_WRN = "SD23";
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			invoiceLine.AddInfo.ZA_WRL = 1;
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			declaration.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			invoiceLine.AddInfo.ZA_WRN = "";
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			invoiceLine.AddInfo.ZA_WRN = "SD23";
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
		}
	}
}
