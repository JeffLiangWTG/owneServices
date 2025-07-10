using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.ComplianceReport;
using Enterprise.Accounting.DataTransfer.ComplianceReport.JPKV7M;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.DataTransfer.Testing.ComplianceReport.JPKV7M
{
	public class JPKAdditionalDataCollectorTest : JPKTestBase
	{
		[TestDate(2023, 9, 26)]
		public void TestConstructor()
		{
			var dataCollector = new JPKAdditionalDataCollector(Report);

			AssertNotNull("JPKAdditionalDataCollector", dataCollector);
			AssertEquals("DataCollectionMode", ComplianceReportDataCollectionMode.JPKV7M, dataCollector.DataCollectionMode);
			AssertEquals("DateFrom", Report.ACR_DateFrom, dataCollector.DateFrom);
			AssertEquals("DateTo", Report.ACR_DateTo, dataCollector.DateTo);

			AssertNotNull("HeaderData", dataCollector.HeaderData);
			AssertNotNull("AdditionalDataProvider", dataCollector.AdditionalDataProvider);
			Assert("Type of AdditionalDataProvider", dataCollector.AdditionalDataProvider is ComplianceReportAdditionalDataProviderNotSAFT);
		}

		[TestDate(2023, 9, 26, 9, 54, 35)]
		public void TestHeaderData()
		{
			SetupTaxRates();
			SetupDebtorsAndCreditors();
			SetupARTransactions();

			var dataCollector = new JPKAdditionalDataCollector(Report);
			AssertNotNull("HeaderData", dataCollector.HeaderData);
			AssertEquals("HeaderData.Count", 6, dataCollector.HeaderData.Count);
			assertHeaderData(dataCollector.GetTransactionHeader(ARInv1.PK) as TransactionHeaderDetailsJPK, 1, 1, Creator.AALSHI.PK, true, ZDate.Today.AddDays(-1), "AR", "First AR Invoice");
			assertHeaderData(dataCollector.GetTransactionHeader(ARInv2.PK) as TransactionHeaderDetailsJPK, 2, 4, Creator.DebtorDE.PK, true, ZDate.Today.AddDays(-5), "AR", "Second AR Invoice");
			assertHeaderData(dataCollector.GetTransactionHeader(ARCrd.PK) as TransactionHeaderDetailsJPK, 5, 5, Creator.DebtorDE.PK, true, ZDate.Today.AddDays(-1), "AR", "AR Credit Note");
			assertHeaderData(dataCollector.GetTransactionHeader(ARInv3.PK) as TransactionHeaderDetailsJPK, 6, 8, DebtorCreditorAT.PK, false, ZDate.Today.AddDays(-3), "AR", "Third AR Invoice");
			assertHeaderData(dataCollector.GetTransactionHeader(ARInv4.PK) as TransactionHeaderDetailsJPK, 9, 9, DebtorCreditorPL.PK, false, ZDate.Today.AddDays(-1), "AR", "Fourth AR Invoice");
			assertHeaderData(dataCollector.GetTransactionHeader(ARInv5.PK) as TransactionHeaderDetailsJPK, 10, 10, OtherCompanyOrgProxy.PK, false, ZDate.Today, "AR", "Fifth AR Invoice");

			void assertHeaderData(TransactionHeaderDetailsJPK headerData, ZInt firstSequence, ZInt lastSequence, ZGuid orgPK, ZBool isGTU_13, ZDate earliestTaxDate, ZString ledger, ZString description)
			{
				AssertNotNull(headerData);
				AssertEquals("HeaderSequence", firstSequence, headerData.HeaderSequence);
				AssertEquals("LastSequence", lastSequence, headerData.LastSequence);
				AssertEquals("OrgPK", orgPK, headerData.OrgPK);
				AssertEquals("IsGTU_13", isGTU_13, headerData.IsGTU_13);
				AssertEquals("EarliestTaxDate", earliestTaxDate, headerData.EarliestTaxDate);

				AssertEquals("InvoiceDate", ZDate.Today, headerData.InvoiceDate);
				AssertEquals("Ledger", ledger, headerData.Ledger);
				AssertEquals("Description", description, headerData.Description);
				AssertEquals("CreateUserCode", "E", headerData.CreateUserCode);
				AssertEquals("CreateUserName", "CargoWise Support", headerData.CreateUserName);
				AssertEquals("CreateTime is SMALLDATETIME", ZDateTime.Now.AddSeconds(-ZDateTime.Now.Second), headerData.CreateTime);
			}
		}

		public void TestOrgHeaderAndTaxRegistrationNumberDetails()
		{
			SetupTaxRates();
			SetupDebtorsAndCreditors();
			Creator.CreateCustomsCodes(Creator.AALSHI, CountryCodes.Australia, "ABN", "6140102321");

			SetupARTransactions();

			var dataCollector = new JPKAdditionalDataCollector(Report);
			AssertNotNull("OrgHeaderDetails", dataCollector.OrgHeaderDetails);
			AssertEquals("OrgHeaderDetails.Count", 5, dataCollector.OrgHeaderDetails.Count);

			assertOrgHeaderDetails("OTHORG_WW");
			assertOrgHeaderDetails("GERDEBBER");
			assertOrgHeaderDetails("AALSHI", "NON");
			assertOrgHeaderDetails("AUSDEBVIE", consolidatedCategory: "MIR");
			assertOrgHeaderDetails("POLDEBWAR", consolidatedCategory: "UNR");

			AssertNotNull("OrgTaxRegistrationNumberDetails", dataCollector.OrgTaxRegistrationNumberDetails);
			AssertEquals("OrgTaxRegistrationNumberDetails.Count", 6, dataCollector.OrgTaxRegistrationNumberDetails.Count);

			assertTaxRegNumberDetails("OTHORG_WW", "PL", "0098765432");
			assertTaxRegNumberDetails("GERDEBBER", "DE", "0147154321");
			assertTaxRegNumberDetails("AALSHI", "AU", "6140102321");
			assertTaxRegNumberDetails("AUSDEBVIE", "AT", "987654321");
			assertTaxRegNumberDetails("POLDEBWAR", "PL", "001 471-543.2");
			assertTaxRegNumberDetails("EDICUS", "AU", ""); // Current Company Org Proxy in AU without ABN

			void assertOrgHeaderDetails(string orgCode, string apVatConfig = "DEF", string consolidatedCategory = "")
			{
				Assert(orgCode, dataCollector.OrgHeaderDetails.TryGetValue(orgCode, out var details));
				AssertEquals(orgCode + " APVATConfig", apVatConfig, details.APVATConfig);
				AssertEquals(orgCode + " APCostsSelfBilled", false, details.APCostsSelfBilled);
				AssertEquals(orgCode + " ARCustomerSelfBillsRevenue", false, details.ARCustomerSelfBillsRevenue);
				AssertEquals(orgCode + " ARConsolidatedAccountingCategory", consolidatedCategory, details.ARConsolidatedAccountingCategory);
			}

			void assertTaxRegNumberDetails(string orgCode, string country, string regNum)
			{
				Assert(orgCode, dataCollector.OrgTaxRegistrationNumberDetails.TryGetValue(orgCode, out var details));
				AssertEquals(orgCode + " Item1", country, details.Item1);
				AssertEquals(orgCode + " Item2", regNum, details.Item2);
			}
		}
	}
}
