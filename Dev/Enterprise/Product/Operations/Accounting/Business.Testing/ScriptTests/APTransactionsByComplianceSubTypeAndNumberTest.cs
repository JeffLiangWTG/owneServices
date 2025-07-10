using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class APTransactionsByComplianceSubTypeAndNumberTest : TransactionsByComplianceSubTypeCoreTest
	{
		protected override Type TransactionType => typeof(APInvoice);

		public void TestTransactionLocalAmountWithOtherTaxes()
		{
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.AUD, 1.0m, 1000m, 100m, 0m, 1000m, 100m, 0m, TestObjectCreator.Creditor1);
			TestObjectCreator.CreateJobCharge(apInvoice.Lines[0], TestObjectCreator.Job1, TestObjectCreator.CC1);
			apInvoice.AH_LocalTaxAmountOtherTaxes = apInvoice.AH_OSTaxAmountOtherTaxes = -200M;

			Factory.Save();

			var result = RunScript(ZDateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), ZDateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), "", "ALL", "", "BTH");
			AssertDataTableAllRows("AccountsPayable", result, new[] { "TransactionLocalAmount" }, new object[][] { new object[] { -1300m } });
		}

		public void TestSimpleRun()
		{
			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "001", TestObjectCreator.AUD);
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice1.AH_PostDate = ZDateTime.Today;

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "002", TestObjectCreator.AUD);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_PostDate = ZDateTime.Today;

			Factory.Save();

			var headers = new[]
			{
				"TransactionLedger", "TransactionType", "TransactionNum", "TransactionInternalReference",
				"TransactionChequeOrReferenceNum", "TransactionDescription", "TransactionOrgCode", "TransactionCurrency",
				"TransactionOrgName", "TransactionUNLOCO", "AllocationLevel"
			};

			var result = RunScript(ZDateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), ZDateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), "", "ALL", "", "BTH");

			var lines = new[]
			{
				new object[] { "AP", "INV", "002", "00001000",
					"", "Test Invoice",  "ABIGAS      ", "AUD",
					"ABI GAS & TOOLS", "AUBNE", "COM" }
			};

			AssertDataTableAllRowsByKeyColumns("AccountsPayable", result, headers, lines);
		}

		protected override DataTable RunScript(string fromDate, string toDate, string transactionType, string status, string subtype, string allocationLevel, string orgCusCode = null)
		{
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var currentCountryTaxRegistrationOrgCusCode = string.IsNullOrEmpty(orgCusCode) ? Country.GetConsumptionTaxRegistrationOrgCusCode(currentCountry) : orgCusCode;

			var sql = string.Format(@"
SELECT * FROM APTransactionsByComplianceSubTypeAndNumber(
	'{0}',				-- CurrentCountry
	'{1}',				-- Company
	'{2}',				-- CurrentCountryTaxRegistrationOrgCusCode
	'{3}',				-- FromDate
	'{4}',				-- ToDate
	'{5}',				-- TransactionType
	'{6}',				-- Status
	'{7}',				-- Subtype
	'',					-- TransactionBranchList
	'{8}',				-- AllocationLevel
	''					-- BookBranchList
)",
							GlbCompany.CurrentCompany.GC_RN_NKCountryCode,	//@CurrentCountry
							GlbCompany.CurrentCompany.PK,					//@Company
							currentCountryTaxRegistrationOrgCusCode,		//@CurrentCountryTaxRegistrationOrgCusCode
							fromDate,										//@@FromDate
							toDate,											//@ToDate
							transactionType,								//@TransactionType
							status,											//@Status
							subtype,										//@Subtype
							allocationLevel									//@AllocationLevel
	);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
