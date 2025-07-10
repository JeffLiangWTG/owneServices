using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCPCList()
	{
		var currentCountry = GlbCompany.CurrentCompany.Country.Code;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateRefCusProcedure(currentCountry, "A", "10", "11", "111", "One", "IMP", group: "IFD");
		helper.CreateRefCusProcedure(currentCountry, "B", "10", "22", "222", "Two", "IMP", group: "IFD");
		helper.CreateRefCusProcedure(currentCountry, "C", "10", "33", "333", "Three", "EXP", group: "ICR");
		helper.CreateRefCusProcedure(currentCountry, "D", "20", "44", "444", "Four", "IMP", group: "IFD");

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		CombineAssertions(() =>
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
			var cpcList = invoiceLine.Lookups.CPCList;
			AssertEquals("CPC List should have procedure codes filtered by shipmentType & declarationType", 3, cpcList.Count);
			AssertEquals("CPC List does not contain default filter for CPC", false, cpcList.FilterBusinessObjectDefaults.ContainsDefaultFor("CPC:Property"));

			entryInstruction.CEI_Procedure = "10";
			cpcList = invoiceLine.Lookups.CPCList;
			var filterProperty = cpcList.FilterBusinessObjectDefaults["CPC:Property"];
			AssertEquals("Filter value for CPC", "10", filterProperty.Value);
			AssertEquals("Property Not removable", false, filterProperty.IsRemovable);
		});
	}

	public void TestConsigneeList()
	{
		AssertType(typeof(ConsigneeCollection), lookups.Consignees);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		lookups = new JobComInvoiceLineLookups(invoiceLine);
	}
	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLineLookups lookups;
}
