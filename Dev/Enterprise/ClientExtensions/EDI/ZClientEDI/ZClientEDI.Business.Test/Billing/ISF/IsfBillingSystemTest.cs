using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class IsfBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			IsfBillingSystem isfBilling = new IsfBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.ImporterSecurityFiling, isfBilling.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ImporterSecurityFiling, new ZDateTime(2010, 10, 01), ZGuid.Empty, 10);
			Factory.Save();

			IsfBillingSystem isfBilling = new IsfBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			TransactionalSystemBill bill = isfBilling.LoadSystemBills(context).First() as TransactionalSystemBill;
			AssertEquals(BillingConstants.BillingSystem.ImporterSecurityFiling, bill.SystemCode);
		}

		public void TestLoadOdplRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "ISF", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "D51-18238477972", "4701", "d2978459-02fd-4088-9858-d39d969969e2", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "ISF", new ZDateTime(2016, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "AN7-81669348021", "2704", "084d9e6a-f687-4eed-87bf-e6d5b3a31a06", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "ISF", new ZDateTime(2016, 5, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "DSV-58488215651", "4701", "8fd5ae74-ff87-4c39-aebd-08f9ddabf8b2", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "ISF", new ZDateTime(2016, 5, 9, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new IsfBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 5, 1), org.PK, clientCompany.PK, licence.Company.PK, licence.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "DDDABCSYD", "2704", "AN7-81669348021");
				AssertOdplRawUsage(rawUsage.Summary.Lines[1], "DDDABCSYD", "4701", "DSV-58488215651");
				AssertOdplRawUsage(rawUsage.Summary.Lines[2], "DDDABCSYD", "", "");
			});

			string expectedCsvResult =
@"""Client ID"",""Postcode"",""Transaction Number"",""Message Time (UTC)""
""DDDABCSYD"",""2704"",""AN7-81669348021"",""01-May-16 10:00""
""DDDABCSYD"",""4701"",""DSV-58488215651"",""02-May-16 10:00""
""DDDABCSYD"","""","""",""09-May-16 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-May-16 10:00"",""DDDABCSYD"",""B11"",""S11"",""2704 AN7-81669348021"","""","""",""1""
""02-May-16 10:00"",""DDDABCSYD"",""B12"",""S12"",""4701 DSV-58488215651"","""","""",""1""
""09-May-16 00:00"",""DDDABCSYD"",""B13"",""S13"","""","""","""",""1""
", writer.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string portCode, string transactionNumber)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Port Code", portCode, summaryLine.Column2);
			AssertEquals("Transaction Number", transactionNumber, summaryLine.Column3);
		}

		public void TestLoadStlRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "ISF", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "D51-18238477972", "4701", "d2978459-02fd-4088-9858-d39d969969e2", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "ISF", new ZDateTime(2016, 5, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "AN7-81669348021", "2704", "084d9e6a-f687-4eed-87bf-e6d5b3a31a06", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "ISF", new ZDateTime(2016, 5, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "DSV-58488215651", "4701", "8fd5ae74-ff87-4c39-aebd-08f9ddabf8b2", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("USC", "ISF", new ZDateTime(2016, 5, 9, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new IsfBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 5, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(3, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertStlRawUsage(rawUsage.Summary.Lines[0], "ABC", "2704", "AN7-81669348021");
				AssertStlRawUsage(rawUsage.Summary.Lines[1], "ABC", "4701", "DSV-58488215651");
				AssertStlRawUsage(rawUsage.Summary.Lines[2], "ABC", "", "");
			});

			string expectedCsvResult =
@"""Company Code"",""Postcode"",""Transaction Number"",""Message Time (UTC)""
""ABC"",""2704"",""AN7-81669348021"",""01-May-16 10:00""
""ABC"",""4701"",""DSV-58488215651"",""02-May-16 10:00""
""ABC"","""","""",""09-May-16 00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-May-16 10:00"",""ABC"",""B11"",""S11"",""2704 AN7-81669348021"","""","""",""1""
""02-May-16 10:00"",""ABC"",""B12"",""S12"",""4701 DSV-58488215651"","""","""",""1""
""09-May-16 00:00"",""ABC"",""B13"",""S13"","""","""","""",""1""
", writer.ToString());
		}

		void AssertStlRawUsage(SummaryLine summaryLine, string companyCode, string portCode, string transactionNumber)
		{
			AssertEquals("Company Code", companyCode, summaryLine.Column1);
			AssertEquals("Port Code", portCode, summaryLine.Column2);
			AssertEquals("Transaction Number", transactionNumber, summaryLine.Column3);
		}

		[TestDate(2017, 7, 20)]
		public void TestLoadOdplRawUsage_FromChargeableUsage()
		{
			var periodStart = new ZDateTime(2012, 11, 1);
			CreateChargeableUsage(Organisation, periodStart, "1001", 10);
			CreateChargeableUsage(Organisation, periodStart, "1002", 24);
			CreateChargeableUsage(Organisation, periodStart, "1003", 35);
			CreateChargeableUsage(Organisation, periodStart.AddMonths(-1), "1003", 93);
			CreateChargeableUsage(Organisation, periodStart.AddMonths(-1), "1004", 94);

			// Creating this ones to ensure that they will not be included in raw usage
			CreateChargeableUsage(ChildOrganisation, periodStart, "1004", 44);
			CreateChargeableUsage(ChildOrganisation, periodStart, "1005", 55);
			var otherSystem = CreateChargeableUsage(Organisation, periodStart, "1006", 61);
			otherSystem.U1_Code = "DUM";
			Factory.Save();

			var billing = new IsfBillingSystem();

			var clientCompany = Organisation.LicCompany.LicHeadersForAllDatabases[0].ClientCompany;
			var context = new BillingLoadRawUsageContext(Factory, periodStart, Organisation.PK, clientCompany.PK, ZGuid.Empty, clientCompany.Database.PK);
			var rawUsage = (SystemCodeRawUsage)billing.LoadOdplRawUsage(context);
			AssertEquals(new ZDateTime(2012, 11, 1), rawUsage.PeriodStart);
			AssertEquals("ISF", rawUsage.SystemCode);
			AssertEquals(Organisation.OH_FullName, rawUsage.OrgName);
			AssertEquals(clientCompany.LCC_Code, rawUsage.CompanyCode);
			AssertEquals(clientCompany.Database.LD_ServerCode, rawUsage.ServerCode);
			AssertEquals("Importer Security Filing Usage Summary", rawUsage.SummaryHeaderDescription);

			AssertEquals("Port Code", rawUsage.Summary.Header.Column1);
			AssertEquals("Usage Count", rawUsage.Summary.Header.Column2);

			AssertEquals(3, rawUsage.Summary.Lines.Count);
			AssertEquals("1001", rawUsage.Summary.Lines[0].Column1);
			AssertEquals("10", rawUsage.Summary.Lines[0].Column2);
			AssertEquals("1002", rawUsage.Summary.Lines[1].Column1);
			AssertEquals("24", rawUsage.Summary.Lines[1].Column2);
			AssertEquals("1003", rawUsage.Summary.Lines[2].Column1);
			AssertEquals("35", rawUsage.Summary.Lines[2].Column2);

			string expectedCsvResult =
@"""Port Code"",""Usage Count""
""1001"",""10""
""1002"",""24""
""1003"",""35""
";

			var builder = new ZStringBuilder();
			billing.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billing.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""20-Jul-17 00:00"","""","""","""",""1001"","""","""",""10""
""20-Jul-17 00:00"","""","""","""",""1002"","""","""",""24""
""20-Jul-17 00:00"","""","""","""",""1003"","""","""",""35""
", writer.ToString());
		}

		[TestDate(2017, 7, 20)]
		public void TestLoadStlRawUsage_FromChargeableUsage()
		{
			var periodStart = new ZDateTime(2012, 11, 1);
			CreateChargeableUsage(Organisation, periodStart, "1001", 10);
			CreateChargeableUsage(Organisation, periodStart, "1002", 24);
			CreateChargeableUsage(Organisation, periodStart, "1003", 35);
			CreateChargeableUsage(Organisation, periodStart.AddMonths(-1), "1003", 93);
			CreateChargeableUsage(Organisation, periodStart.AddMonths(-1), "1004", 94);

			// Creating this ones to ensure that they will not be included in raw usage
			CreateChargeableUsage(ChildOrganisation, periodStart, "1004", 44);
			CreateChargeableUsage(ChildOrganisation, periodStart, "1005", 55);
			var otherSystem = CreateChargeableUsage(Organisation, periodStart, "1006", 61);
			otherSystem.U1_Code = "DUM";
			Factory.Save();

			var billing = new IsfBillingSystem();

			var clientCompany = Organisation.LicCompany.LicHeadersForAllDatabases[0].ClientCompany;
			var context = new BillingLoadRawUsageContext(Factory, periodStart, clientCompany.Database.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billing.LoadStlRawUsage(context);
			AssertEquals(new ZDateTime(2012, 11, 1), rawUsage.PeriodStart);
			AssertEquals(clientCompany.Database.LD_ServerCode, rawUsage.ServerCode);

			AssertEquals("Company Code", rawUsage.Summary.Header.Column1);
			AssertEquals("Port Code", rawUsage.Summary.Header.Column2);
			AssertEquals("Usage Count", rawUsage.Summary.Header.Column9);

			AssertEquals(3, rawUsage.Summary.Lines.Count);

			AssertEquals(clientCompany.LCC_Code, rawUsage.Summary.Lines[0].Column1);
			AssertEquals("1001", rawUsage.Summary.Lines[0].Column2);
			AssertEquals("10", rawUsage.Summary.Lines[0].Column9);

			AssertEquals(clientCompany.LCC_Code, rawUsage.Summary.Lines[1].Column1);
			AssertEquals("1002", rawUsage.Summary.Lines[1].Column2);
			AssertEquals("24", rawUsage.Summary.Lines[1].Column9);

			AssertEquals(clientCompany.LCC_Code, rawUsage.Summary.Lines[2].Column1);
			AssertEquals("1003", rawUsage.Summary.Lines[2].Column2);
			AssertEquals("35", rawUsage.Summary.Lines[2].Column9);

			string expectedCsvResult =
@"""Company Code"",""Port Code"",""Usage Count""
""SYD"",""1001"",""10""
""SYD"",""1002"",""24""
""SYD"",""1003"",""35""
";

			var builder = new ZStringBuilder();
			billing.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billing.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""20-Jul-17 00:00"",""SYD"","""","""",""1001"","""","""",""10""
""20-Jul-17 00:00"",""SYD"","""","""",""1002"","""","""",""24""
""20-Jul-17 00:00"",""SYD"","""","""",""1003"","""","""",""35""
", writer.ToString());
		}

		ClientChargeableUsage CreateChargeableUsage(EDIOrgHeader organisation, ZDateTime periodStart, ZString subCode, ZInt unitCount)
		{
			var licHeader = organisation.LicCompany.LicHeadersForAllDatabases[0];
			var usage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ImporterSecurityFiling, subCode, periodStart, licHeader, unitCount);
			usage.U1_LC = ZGuid.Empty;
			return usage;
		}

		#region Implementation

		EDIOrgHeader Organisation;
		EDIOrgHeader ChildOrganisation;

		protected override void SetUp()
		{
			base.SetUp();

			Organisation = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			ChildOrganisation = BillingTestHelper.CreateDependentOrganisation(Organisation, "BBB");

			Factory.Save();
		}

		#endregion
	}
}