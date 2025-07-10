using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.MX.Business.Testing
{
	class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCusEntryInstructionCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var instruction1 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Description = "Inst-1";
			var instruction2 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction2.CEI_Description = "Inst-2";
			var instruction3 = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction3.CEI_Description = "Inst-3";
			var invLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var lookups = invLine.Lookups;

			AssertEquals("Test Count", 3, lookups.CustomsProcedureCodes.Count);
			CombineAssertions("TestCPC", () =>
			{
				AssertEquals("Test 1", true, lookups.CustomsProcedureCodes.ContainsCode("Inst-1"));
				AssertEquals("Test 2", true, lookups.CustomsProcedureCodes.ContainsCode("Inst-2"));
				AssertEquals("Test 3", true, lookups.CustomsProcedureCodes.ContainsCode("Inst-3"));
			});
		}

		public void TestVehicleMileageUQList()
		{
			var parent = Factory.New<CusVehicle>();
			var invLine = Factory.New<JobComInvoiceLine>();
			var lookups = invLine.Lookups;
			var list = lookups.VehicleMileageUQList;

			AssertEquals("KM, MI", lookups.VehicleMileageUQList.CodesAsString);
			AssertSame("cached", list, lookups.VehicleMileageUQList);
		}
	}
}
