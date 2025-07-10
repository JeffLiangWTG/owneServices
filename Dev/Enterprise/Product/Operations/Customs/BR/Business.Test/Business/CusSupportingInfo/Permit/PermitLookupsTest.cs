using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class PermitLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPermitUQList()
		{
			var permit = CreateNewPermit();
			AssertSame((permit.Parent as JobComInvoiceLine).Lookups.InvoiceUQList, permit.Lookups.UQList);
		}

		public void TestLPCOHeaders()
		{
			var permit = CreateNewPermit();
			permit.CSI_ReferenceNumber = "123";
			AssertType<CusLPCOHeaderCollection>(permit.Lookups.LPCOHeaders);

			var permitsFilterObj = permit.Lookups.LPCOHeaders.FilterBusinessObjectDefaults;
			AssertEquals("Start Date", "In the Past", permitsFilterObj[CusLPCOHeaderCollection.FilterConstants.StartDate + ":PropertySearch"].Value);
			AssertEquals("End Date", "In the Future", permitsFilterObj[CusLPCOHeaderCollection.FilterConstants.EndDate + ":PropertySearch"].Value);
		}

		Permit CreateNewPermit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			return invoiceLine.Permits.AddNew();
		}
	}
}
