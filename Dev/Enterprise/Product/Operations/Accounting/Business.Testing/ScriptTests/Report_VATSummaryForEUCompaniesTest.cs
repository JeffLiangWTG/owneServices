using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_VATSummaryForEUCompaniesTest : ScriptTest
	{
		public void TestShowRowsHaveGSTVATButLineAmountZero()
		{
			var payment = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 100M, 10M, 0M, 0M);
			var receipt = TestObjectCreator.CreateDirectReceipt(ZDateTime.Now, 0M, 50M, 0M, 200M);

			Factory.Save();

			DataTable result = RunScript(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany, LedgerTypes.CashBook, Country.GetConsumptionTaxRegistrationOrgCusCode(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			AssertEquals("Should have 2 Rows", 2, result.Rows.Count);
			AssertEquals("Should Find Row With Zero Line Amount And Have GSTVAT Value", 1, result.Select("TaxBaseAmount = 0").Length);
		}

		[TestDate(2015, 11, 10)]	
		public void TestTaxMsgIsReturned()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV001001", TestObjectCreator.AUD, 1M, 100M, 10M, 100M, 10M, TestObjectCreator.ABIGAS, TestObjectCreator.CC1.PK, ZDateTime.Today, ZDateTime.Today, ZDateTime.Today, false);
			invoice.Lines[0].AL_A9_VATClass = TestObjectCreator.TaxMsg1.PK;

			Factory.Save();

			var result = RunScript(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, GlbCompany.CurrentCompany, LedgerTypes.AccountsReceivable, null);
			AssertEquals("Should have 1 Row", 1, result.Rows.Count);
			AssertEquals("Tax msg should be retuned by the function", TestObjectCreator.TaxMsg1.A9_Code, result.Rows[0]["TaxMsgCode"]);
		}

		DataTable RunScript(string currentCountry, GlbCompany loginCompany, string ledgerType, string currentCountryTaxRegistrationOrgCusCode)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_VATSummaryForEUCompanies(	
'{0}',	--@CurrentCountry
'{1}',	--@CurrentCountryName
'{2}',	--@CompanyPK
NULL,	--@Period
'{3}',	--@StartDate
'{4}',	--@EndDate
'{5}',	--@TransactionLedger
'',		--@TaxID
'',		--@BranchList
'{6}',	--@CurrentCountryTaxRegistrationOrgCusCode
null	--@BranchPK
) 
",
			currentCountry,
			loginCompany.GC_Name,
			loginCompany.PK,
			ZDateTime.Today.AddDays(-1).ToISO8601String(),
			ZDateTime.Today.AddDays(1).ToISO8601String(),
			ledgerType,
			currentCountryTaxRegistrationOrgCusCode
			));
		}
	}
}

