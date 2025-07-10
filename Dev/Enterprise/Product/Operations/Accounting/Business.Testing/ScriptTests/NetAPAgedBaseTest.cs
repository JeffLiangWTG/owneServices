using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class NetAPAgedBaseTest : ScriptTest
	{
		[TestDate(2020, 12, 30)]
		public void TestGetAdditionalCompanyNameWhenCompanyOrgProxyIsNull()
		{
			var companyWithoutOrgProxy = TestObjectCreator.CreateNewCompany("ABC");
			companyWithoutOrgProxy.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			companyWithoutOrgProxy.GC_OH_OrgProxy = ZGuid.Empty;

			var branchWithoutOrgProxy = TestObjectCreator.CreateNewBranch(companyWithoutOrgProxy, "BB1");
			branchWithoutOrgProxy.GB_OH_OrgProxy = ZGuid.Empty;

			var branchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGBB2", false, true);
			branchOrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
			var branchWithOrgProxy = TestObjectCreator.CreateNewBranch(companyWithoutOrgProxy, "BB2");
			branchWithOrgProxy.GB_OH_OrgProxy = branchOrgProxy.PK;
			Factory.Save();

			new AccountingPeriodTestHelper(Factory).SetupSinglePeriod(202012, new ZDateTime(2020, 12, 01), new ZDateTime(2020, 12, 31), companyWithoutOrgProxy.PK);
			var expectedAdditionalCompanyName = "人生得意须尽欢";
			var glAccount = TestObjectCreator.GetGLAccountFromDB();
			var orgHeader = TestObjectCreator.CreateOrgHeader("ABC", true, false);
			TestObjectCreator.CreateAddress(orgHeader, Constants.CountryCodes.China, SharedConstants.Languages.ChineseSimplified, "莫使金樽空对月", expectedAdditionalCompanyName, OrgAddressType.Receivables, OrgAddressType.Receivables);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchWithOrgProxy.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader, glAccount.PK);
				Factory.Save();

				var resultForOrg = RunScript(202012);
				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals("The value of AdditionalCompanyName should be expectedAdditionalCompanyName", expectedAdditionalCompanyName, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchWithoutOrgProxy.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var resultForOrg = RunScript(202012);

				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals("The value of AdditionalCompanyName should be null", DBNull.Value, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}
		}

		[TestDate(2020, 12, 30)]
		public void TestGetAdditionalCompanyName()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				GlbCompany.CurrentCompany.OrgProxy.OH_Language = SharedConstants.Languages.ChineseSimplified;
				GlbCompany.CurrentCompany.Factory.Save();

				var period = new AccountingPeriodTestHelper(Factory);
				period.SetupSinglePeriod(202012, new ZDateTime(2020, 12, 01), new ZDateTime(2020, 12, 31));

				var expectedAdditionalCompanyName = "人生得意须尽欢";

				AccGLHeader glAccount = TestObjectCreator.GetGLAccountFromDB();

				var orgHeader = TestObjectCreator.CreateOrgHeader("ABC", true, false);
				var orgAddress = TestObjectCreator.CreateAddress(orgHeader, Constants.CountryCodes.China, SharedConstants.Languages.ChineseSimplified, "莫使金樽空对月", expectedAdditionalCompanyName, OrgAddressType.Receivables, OrgAddressType.Receivables);

				Factory.Save();

				TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M, orgHeader, glAccount.PK);

				Factory.Save();

				DataTable resultForOrg = RunScript(202012);

				AssertEquals("Result count should be 1.", 1, resultForOrg.Rows.Count);
				AssertEquals("The value of AdditionalCompanyName should be expectedAdditionalCompanyName", expectedAdditionalCompanyName, resultForOrg.Rows[0]["AdditionalCompanyName"]);
			}
		}

		DataTable RunScript(int period)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format($@"
SELECT * FROM [dbo].[NetAPAgedBase]
(
'{period}',										--Period
'{GlbCompany.CurrentCompany.PK.ToGuid()}',		--Company
'{GlbBranch.CurrentBranch.PK.ToGuid()}',		--Branch
'',												--CountryList
''												--ExCountryList
)"));
		}
	}
}
