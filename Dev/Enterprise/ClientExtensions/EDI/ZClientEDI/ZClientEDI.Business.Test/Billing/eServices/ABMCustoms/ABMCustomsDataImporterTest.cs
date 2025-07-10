using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class ABMCustomsDataImporterTest : TestCaseWithFactory
	{
		public void TestImportAndSave_OtherProduct()
		{
			var orgs = new ImportOrgs(Factory, "CSP");
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var path = resourceRetriever.SaveResourceToFile(GoodFileResourceName);
			var importer = new ABMCustomsDataImporter(Factory, new ZDateTime(2015, 5, 1), path, new NotificationBuffer());
			importer.Import();
			resourceRetriever.Dispose();

			AssertEquals("transaction count", 36, importer.Transactions.Count);
		}

		public void TestImportAndSave()
		{
			var orgs = new ImportOrgs(Factory);

			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var path = resourceRetriever.SaveResourceToFile(GoodFileResourceName);
			AssertEquals(path + " file exist", true, File.Exists(path));

			var importer = new ABMCustomsDataImporter(Factory, new ZDateTime(2015, 2, 1), path, new NotificationBuffer());
			importer.Import();
			AssertEquals("Data not imported because filename does not contain period start 'Feb 2015'", 0, importer.Transactions.Count);

			importer = new ABMCustomsDataImporter(Factory, new ZDateTime(2015, 5, 1), path, new NotificationBuffer());
			importer.Import();

			AssertEquals("transaction count", 43, importer.Transactions.Count);

			var transactionData = importer.Transactions[0];
			CombineAssertions(() =>
				{
					AssertEquals("TransactionType:", ABMCustomsTransactionTypes.Codes.Customs, transactionData.TransactionType);
					AssertEquals("ClientID:", "AAASYDAAA", transactionData.ClientID);
					AssertEquals("ClientNumber:", "STW.SYD", transactionData.ClientNumber);
					AssertEquals("PeriodStart:", new DateTime(2015, 5, 1), transactionData.PeriodStart);
					AssertEquals("ABMCompanyCode:", "AAATST", transactionData.ABMCompanyCode);
					AssertEquals("JurisdictionCode:", "DE", transactionData.JurisdictionCode);
					AssertEquals("ProcedureCode:", "ATLEXP", transactionData.ProcedureCode);
					AssertEquals("Department:", "", transactionData.Department);
					AssertEquals("DocumentReference", "", transactionData.DocumentReference);
					AssertEquals("TransactionCount", 122, transactionData.TransactionCount);
				});

			transactionData = importer.Transactions[17];
			CombineAssertions(() =>
			{
				AssertEquals("TransactionType:", ABMCustomsTransactionTypes.Codes.PortCommunity, transactionData.TransactionType);
				AssertEquals("ClientID:", "BBBSYDBBB", transactionData.ClientID);
				AssertEquals("ClientNumber:", "RY.SYD", transactionData.ClientNumber);
				AssertEquals("PeriodStart:", new DateTime(2015, 5, 1), transactionData.PeriodStart);
				AssertEquals("ABMCompanyCode:", "BBBNET", transactionData.ABMCompanyCode);
				AssertEquals("JurisdictionCode:", "NL", transactionData.JurisdictionCode);
				AssertEquals("ProcedureCode:", "PORTBASE", transactionData.ProcedureCode);
				AssertEquals("Department:", "", transactionData.Department);
				AssertEquals("DocumentReference", "20141201092338614-5413", transactionData.DocumentReference);
				AssertEquals("TransactionCount", 1, transactionData.TransactionCount);
			});

			transactionData = importer.Transactions[20];
			CombineAssertions(() =>
			{
				AssertEquals("TransactionType:", ABMCustomsTransactionTypes.Codes.PortCommunity, transactionData.TransactionType);
				AssertEquals("ClientID:", "BBBSYDBBB", transactionData.ClientID);
				AssertEquals("ClientNumber:", "RY.SYD", transactionData.ClientNumber);
				AssertEquals("PeriodStart:", new DateTime(2015, 5, 1), transactionData.PeriodStart);
				AssertEquals("ABMCompanyCode:", "BBBBEL", transactionData.ABMCompanyCode);
				AssertEquals("JurisdictionCode:", "NL", transactionData.JurisdictionCode);
				AssertEquals("ProcedureCode:", "APCS", transactionData.ProcedureCode);
				AssertEquals("Department:", "IMPORT SEA", transactionData.Department);
				AssertEquals("DocumentReference", "20141202110112506", transactionData.DocumentReference);
				AssertEquals("TransactionCount", 1, transactionData.TransactionCount);
			});

			transactionData = importer.Transactions[42];
			CombineAssertions(() =>
			{
				AssertEquals("TransactionType:", ABMCustomsTransactionTypes.Codes.FiscalRep, transactionData.TransactionType);
				AssertEquals("ClientID:", "BBBSYDBBB", transactionData.ClientID);
				AssertEquals("ClientNumber:", "RY.SYD", transactionData.ClientNumber);
				AssertEquals("PeriodStart:", new DateTime(2015, 5, 1), transactionData.PeriodStart);
				AssertEquals("ABMCompanyCode:", "BBBBEL", transactionData.ABMCompanyCode);
				AssertEquals("JurisdictionCode:", "BE", transactionData.JurisdictionCode);
				AssertEquals("ProcedureCode:", "PLDAIMP", transactionData.ProcedureCode);
				AssertEquals("Department:", "", transactionData.Department);
				AssertEquals("DocumentReference", "14/2248", transactionData.DocumentReference);
				AssertEquals("TransactionCount", 1, transactionData.TransactionCount);
			});

			importer.Save();

			var processSql =
@"INSERT INTO BillingTransaction
(
	TX_Period,
	TX_Category,
	TX_PriceItemCode,
	TX_BillableCount,
	TX_ReportingSource,
	TX_ServiceOccuredUTC,
	TX_ClientID,
	TX_ClientNumber,
	TX_ClientStaffCode,
	TX_Reference1,
	TX_Reference2,
	TX_Reference3,
	TX_Reference4,
	TX_Reference5,
	TX_Version,
	TX_Branch,
	TX_SystemCreateUTC,
	TX_CapturedUTC,
	TX_SystemID,
	TX_LCC
)
SELECT
	YEAR(TX_ServiceOccuredUTC) * 100 + MONTH(TX_ServiceOccuredUTC),
	TX_Category,
	TX_PriceItemCode,
	TX_BillableCount,
	TX_ReportingSource,
	TX_ServiceOccuredUTC,
	TX_ClientID,
	TX_ClientNumber,
	TX_ClientStaffCode,
	TX_Reference1,
	TX_Reference2,
	TX_Reference3,
	TX_Reference4,
	TX_Reference5,
	TX_Version,
	TX_Branch,
	sysutcdatetime(),
	sysutcdatetime(),
	1,
	NEWID()
FROM 
	BillingTransactionStaging";
			using (var command = Db.Connection.Command(processSql))
			{
				command.ExecuteNonQuery();
			}

			using (var command = Db.Connection.Command("SELECT COUNT(*) FROM BillingViewChargeable"))
			{
				int dataLineCount = (int)command.ExecuteScalar();
				AssertEquals(43, dataLineCount);
			}

			using (var command = Db.Connection.Command("SELECT TOP 1 TX_BillableCount FROM BillingViewChargeable WHERE TX_PriceItemCode = 'CTM' AND TX_Reference4 = 'AAATST' AND TX_Reference1 = 'DE' AND TX_Reference2 = 'ATLEXP'"))
			{
				int transactionCount = (int)command.ExecuteScalar();
				AssertEquals(122, transactionCount);
			}

			using (var command = Db.Connection.Command("SELECT TOP 1 TX_BillableCount FROM BillingViewChargeable WHERE TX_PriceItemCode = 'POC' AND TX_Reference4 = 'BBBBEL' AND TX_Reference1 = 'NL' AND TX_Reference2 = 'APCS' AND TX_Reference3 = '20141202110112506' AND TX_Reference5 = 'IMPORT SEA'"))
			{
				int transactionCount = (int)command.ExecuteScalar();
				AssertEquals(1, transactionCount);
			}

			using (var command = Db.Connection.Command("SELECT TOP 1 TX_BillableCount FROM BillingViewChargeable WHERE TX_PriceItemCode = 'FRP' AND TX_Reference4 = 'BBBBEL' AND TX_Reference1 = 'BE' AND TX_Reference2 = 'PLDAIMP' AND TX_Reference3 = '14/2247'"))
			{
				int transactionCount = (int)command.ExecuteScalar();
				AssertEquals(1, transactionCount);
			}

			var factory2 = new BusinessObjectFactory();
			var query = new ZQuery(ClientChargeableUsageSchema.U1_Code, BillingConstants.BillingSystem.ABMCustoms);
			query.AddToFilter(ClientChargeableUsageSchema.U1_PeriodStart, new ZDateTime(2015, 5, 1));
			var usage = Factory.Load<ClientChargeableUsage>(query);
			AssertEquals("Usage count", 13, usage.Length);

			var aaaUsage = usage.Where(u => u.U1_LC == orgs.ClientOrgAAA.LicCompany.PK);
			var bbbUsage = usage.Where(u => u.U1_LC == orgs.ClientOrgBBB.LicCompany.PK);
			var cccUsage = usage.Where(u => u.U1_LC == orgs.ClientOrgCCC.LicCompany.PK);

			AssertEquals(2, aaaUsage.Count());
			AssertContainsChargeableUsage(aaaUsage, ABMCustomsTransactionTypes.Codes.Customs, "DE", "", "", 1303);
			AssertContainsChargeableUsage(aaaUsage, ABMCustomsTransactionTypes.Codes.PortCommunity, "DE", "TUL", "AMS", 65);

			AssertEquals(8, bbbUsage.Count());
			AssertContainsChargeableUsage(bbbUsage, ABMCustomsTransactionTypes.Codes.Customs, "NL", "", "EXPORT AIR", 92);
			AssertContainsChargeableUsage(bbbUsage, ABMCustomsTransactionTypes.Codes.Customs, "NL", "", "IMPORT AIR", 571);
			AssertContainsChargeableUsage(bbbUsage, ABMCustomsTransactionTypes.Codes.Customs, "BE", "", "", 472);
			AssertContainsChargeableUsage(bbbUsage, ABMCustomsTransactionTypes.Codes.PortCommunity, "NL", "PORTBASE", "", 3);
			AssertContainsChargeableUsage(bbbUsage, ABMCustomsTransactionTypes.Codes.PortCommunity, "NL", "APCS", "IMPORT SEA", 3);
			AssertContainsChargeableUsage(bbbUsage, ABMCustomsTransactionTypes.Codes.FiscalRep, "NL", "", "AMSIMP", 2);
			AssertContainsChargeableUsage(bbbUsage, ABMCustomsTransactionTypes.Codes.FiscalRep, "NL", "", "RIDCUS", 4);
			AssertContainsChargeableUsage(bbbUsage, ABMCustomsTransactionTypes.Codes.FiscalRep, "BE", "", "", 3);

			AssertEquals(3, cccUsage.Count());
			AssertContainsChargeableUsage(cccUsage, ABMCustomsTransactionTypes.Codes.Customs, "NL", "", "", 12);
			AssertContainsChargeableUsage(cccUsage, ABMCustomsTransactionTypes.Codes.PortCommunity, "NL", "CARGONAUT", "AMSIMP", 5);
			AssertContainsChargeableUsage(cccUsage, ABMCustomsTransactionTypes.Codes.PortCommunity, "NL", "CARGONAUT", "AMSEXP", 6);

			var notification = new NotificationBuffer();
			importer = new ABMCustomsDataImporter(Factory, new ZDateTime(2015, 5, 1), path, notification);
			Assert("Usage for May 2015 already exist", importer.HasExistingChargeableUsage());
			resourceRetriever.Dispose();
		}

		void AssertContainsChargeableUsage(IEnumerable<ClientChargeableUsage> usages, ZString subCode, ZString reference1, ZString reference2, ZString reference3, int unitCount)
		{
			Assert(usages.Any(u => u.U1_SubCode == subCode
								&& u.U1_Reference1 == reference1
								&& u.U1_Reference2 == reference2
								&& u.U1_Reference3 == reference3
								&& u.U1_UnitCount == unitCount));
		}

		// Disables critical validation "Missing Reversing Transaction for this canceled transaction".
		[SuspendCriticalValidation]
		public void TestHasExistingInvoicedChargeableUsage()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var org1 = lic1.Company.Header;

			ARInvoice invoiceCancelled = Factory.NewWithValidTestData<ARInvoice>();
			invoiceCancelled.AH_OH = org1.PK;
			invoiceCancelled.AH_IsCancelled = true;

			ARInvoice currentInvoice = Factory.NewWithValidTestData<ARInvoice>();
			currentInvoice.AH_OH = org1.PK;

			var period1 = BillingTestHelper.MonthToday.AddMonths(-3);
			var period2 = period1.AddMonths(1);
			var period3 = period1.AddMonths(2);

			var usageNotCancelled = BillingTestHelper.CreateChargeableUsage(Factory, "ABM", period1, org1.LicCompany.PK, 1);
			usageNotCancelled.U1_AH_Invoice = currentInvoice.PK;

			var usageCancelled = BillingTestHelper.CreateChargeableUsage(Factory, "ABM", period2, org1.LicCompany.PK, 2);
			usageCancelled.U1_AH_Invoice = invoiceCancelled.PK;

			var usageNoInvoice = BillingTestHelper.CreateChargeableUsage(Factory, "ABM", period3, org1.LicCompany.PK, 2);

			Factory.Save();

			AssertEquals(true, ABMCustomsDataImporter.HasExistingInvoicedChargeableUsage(Factory, period1));
			AssertEquals(false, ABMCustomsDataImporter.HasExistingInvoicedChargeableUsage(Factory, period2));

			AssertEquals("PRE", true, ABMCustomsDataImporter.HasExistingChargeableUsage(Factory, period3));
			AssertEquals(false, ABMCustomsDataImporter.HasExistingInvoicedChargeableUsage(Factory, period3));
		}

		public void TestDeleteChargeableUsages()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var org1 = lic1.Company.Header;

			var period1 = BillingTestHelper.MonthToday.AddMonths(-3);

			var usage = BillingTestHelper.CreateChargeableUsage(Factory, "ABM", period1, org1.LicCompany.PK, 2);
			var usageInOtherMonth = BillingTestHelper.CreateChargeableUsage(Factory, "ABM", period1.AddMonths(1), org1.LicCompany.PK, 2);
			var usageOther = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", period1, org1.LicCompany.PK, 2);

			Factory.Save();

			AssertEquals("PRE", true, ABMCustomsDataImporter.HasExistingChargeableUsage(Factory, period1));

			var importer = new ABMCustomsDataImporter(Factory, period1, string.Empty, new NotificationBuffer());
			importer.DeleteChargeableUsages();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertEquals(false, ABMCustomsDataImporter.HasExistingChargeableUsage(factory2, period1));

			AssertNotNull("non ABM usage is not deleted", factory2.Load<ClientChargeableUsage>(usageOther.PK));
			AssertNotNull("ABM usage in another month is not deleted", factory2.Load<ClientChargeableUsage>(usageInOtherMonth.PK));
		}

		public void TestHasExistingBillingTransactions()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			Factory.Save();

			var period = BillingTestHelper.MonthToday;
			AssertEquals(false, ABMCustomsDataImporter.HasExistingBillingTransactions(period));

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "", period, lic1, "", "", "", ""));
			EServicesBillingTestHelper.AddTransactions(infoList);

			AssertEquals(true, ABMCustomsDataImporter.HasExistingBillingTransactions(period));
			AssertEquals(false, ABMCustomsDataImporter.HasExistingBillingTransactions(period.AddMonths(1)));
			AssertEquals(false, ABMCustomsDataImporter.HasExistingBillingTransactions(period.AddMonths(-1)));
		}

		const string GoodFileResourceName = "ZClientEDI.Business.Test.Billing.eServices.ABMCustoms.Test_ABMCustoms_Data May 2015.xlsx";

		internal class ImportOrgs
		{
			internal readonly EDIOrgHeader ClientOrgAAA;
			internal readonly EDIOrgHeader ClientOrgBBB;
			internal readonly EDIOrgHeader ClientOrgCCC;

			internal ImportOrgs(BusinessObjectFactory factory, string product = Licencing.Business.ProductTypes.Codes.CargoWiseOne)
			{
				ClientOrgAAA = BillingTestHelper.CreateOrganisation(factory, "AAA");
				ClientOrgAAA.CustomsCodes.AddNew("ABM", "AAATST");
				var db1 = ClientOrgAAA.LicCompany.ActiveOrAllLicDatabases[0];
				db1.LD_DatabaseNumber = 9871;
				db1.LD_Product = product;

				ClientOrgBBB = BillingTestHelper.CreateOrganisation(factory, "BBB");
				ClientOrgBBB.CustomsCodes.AddNew("ABM", "BBBNET", "NL");
				ClientOrgBBB.CustomsCodes.AddNew("ABM", "BBBBEL", "BE");
				var db2 = ClientOrgBBB.LicCompany.ActiveOrAllLicDatabases[0];
				db2.LD_DatabaseNumber = 342;

				ClientOrgCCC = BillingTestHelper.CreateOrganisation(factory, "CCC");
				ClientOrgCCC.CustomsCodes.AddNew("ABM", "CCCTST");
				var db3 = ClientOrgCCC.LicCompany.ActiveOrAllLicDatabases[0];
				db3.LD_DatabaseNumber = 5555;

				factory.Save();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			EServicesBillingTestHelper.CreateTable();
			EServicesBillingTestHelper.CreateTableBillingTransactionStaging();
		}

		protected override void TearDown()
		{
			base.TearDown();
			EServicesBillingTestHelper.DropTable();
		}
	}
}
