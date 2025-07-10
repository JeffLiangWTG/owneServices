using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class JobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestInvoiceLine()
	{
		var parent = Factory.New<JobComInvoiceLine>();
		AssertEquals(parent.Lookups.InvoiceLine, parent);
	}

	public void TestRegionOfDispatchList()
	{
		var list = lookups.RegionOfDispatchList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", "1, 2, 3", list.CodesAsString);
			AssertSame("Cached", lookups.RegionOfDispatchList, list);
		});
	}

	public void TestPrimaryPreferenceList()
	{
		var expectedCodeAsString = "100, 110, 115, 118, 119, 120, 123, 125, 128, 140, 150, 200, 210, 215, 218, 219, 220, 223, 225, 228, 240, 250, 300, 310, 315, 318, 319, 320, 323, 325, 328, 340, 350, 400, 410, 415, 418, 419, 420, 423, 425, 428, 440, 450, 500, 510, 518, 519, 520, 525, 528, 550";
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine.JI_JZ = invoiceHeader.PK;
		var list = (CodeDescriptionPairList)lookups.PrimaryPreferenceList;
		CombineAssertions(() =>
		{
			AssertNotEquals("CodesAsString not expected for export", expectedCodeAsString, list.CodesAsString);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			list = (CodeDescriptionPairList)lookups.PrimaryPreferenceList;
			AssertEquals("CodesAsString expected for import", expectedCodeAsString, list.CodesAsString);
			AssertSame("Cached", lookups.PrimaryPreferenceList, list);
		});
	}

	public void TestExporterList()
	{
		AssertType<OrgHeaderCollection>(lookups.ExporterList);
	}

	protected override void SetUp()
	{
		base.SetUp();
		invoiceLine = Factory.New<JobComInvoiceLine>();
		lookups = invoiceLine.Lookups;
	}

	JobComInvoiceLineLookups lookups;
	JobComInvoiceLine invoiceLine;
}
