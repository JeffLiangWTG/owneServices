

using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_PaymentsToIRS1099FormEligibleOrganisationsSummaryTest : ScriptTest
	{
		GlbCompany loginCompany;
		int postDate;

		public void TestLoginCompanyEINIsReturned()
		{
			loginCompany = TestObjectCreator.CreateNewCompany("AAA", "US");
			GlbBranch branch1 = TestObjectCreator.CreateNewBranch(loginCompany, "AB1");
			branch1.GB_GC = loginCompany.PK;
			OrgHeader loginCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROAAA", true, true);
			loginCompanyOrgProxy.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-345678901");
			loginCompany.GC_OH_OrgProxy = loginCompanyOrgProxy.PK;
			OrgHeader creditor = TestObjectCreator.AALSHI;
			creditor.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "76-54321ABCD");
			AccTransactionHeader apInvoice = TestObjectCreator.InsertTransaction("PAY", "AP");
			apInvoice.AH_GB = branch1.PK;
			apInvoice.AH_OH = creditor.PK;
			postDate = apInvoice.AH_PostDate.Year;
			Factory.Save();

			DataTable result = RunScript();

			AssertEquals("Result Rows.Count", 1, result.Rows.Count);
			AssertEquals("OK_LoginCompanyEIN", "12-3456789", result.Rows[0]["OK_LoginCompanyEIN"]);
			AssertEquals("OK_CustomsRegNoEIN", "76-54321AB", result.Rows[0]["OK_CustomsRegNoEIN"]);
			AssertEquals("SSNorEIN", "76-54321AB", result.Rows[0]["SSNorEIN"]);
		}

		DataTable RunScript()
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_PaymentsToIRS1099FormEligibleOrganisationsSummary(
'{0}',	--@CurrentCompany uniqueidentifier
'{1}',	--@Year int
'{2}'	--@OrganisationType char(3)
)  
",
			loginCompany.PK,
			postDate,
			"ALL"
			));
		}
	}
}


