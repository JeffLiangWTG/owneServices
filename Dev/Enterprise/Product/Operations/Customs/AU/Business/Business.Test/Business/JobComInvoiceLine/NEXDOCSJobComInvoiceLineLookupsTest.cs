using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class NEXDOCSJobComInvoiceLineLookupsTest : JobComInvoiceLineLookupsTest
	{
		public void TestEXDOCPermitAuthorityListIsCached()
		{
			Assert("EXDOCPermitAuthorityList should be cached in same factory", ReferenceEquals(Enterprise.Customs.Universal.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Australia, "NPERT", ZDate.Today), invoiceLine.Lookups.EXDOCPermitAuthorityList));
		}

		[TestDate(2019, 3, 6)]
		public void TestEXDOCPermitAuthorityList()
		{
			AssertEquals("AQ, BP, CS", invoiceLine.Lookups.EXDOCPermitAuthorityList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("NPERT", "NEXDOCS Related Export Permit Authority");
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPERT", "AQ", "Aquaculture", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPERT", "BP", "Animal By-Product", date1, date2);
			helper.CreateNewOrGetExistingCusCodeList("AU", "NPERT", "CS", "Carcass-Side", date1, date2);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoice = declaration.Invoices.AddNew();
			invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
			invoiceLine = invoice.InvoiceLines.AddNew() as JobComInvoiceLine;
		}
		JobComInvoiceLine invoiceLine;
	}
}
