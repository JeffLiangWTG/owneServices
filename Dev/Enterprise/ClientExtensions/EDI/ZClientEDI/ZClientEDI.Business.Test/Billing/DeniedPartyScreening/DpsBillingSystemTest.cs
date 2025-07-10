using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.DeniedPartyScreening.Test
{
	public class DpsBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			DpsBillingSystem billing = new DpsBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.DeniedPartyScreening, billing.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.DeniedPartyScreening, new ZDateTime(2010, 10, 01), ZGuid.Empty, 10);
			Factory.Save();

			DpsBillingSystem dpsBilling = new DpsBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			TransactionalSystemBill bill = dpsBilling.LoadSystemBills(context).First() as TransactionalSystemBill;
			AssertEquals(BillingConstants.BillingSystem.DeniedPartyScreening, bill.SystemCode);
		}

		#region Raw Usage

		public void TestLoadOdplRawUsage()
		{
			var lic1 = CreateLicenceAndUsage();

			var billingSystem = new DpsBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 3, 1), lic1.Company.LC_OH, lic1.ClientCompany.PK, lic1.LA_LC, lic1.LA_LD);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;

			var line = rawUsage.Summary.Lines[0];
			AssertEquals("ABC", line.Column1);
			AssertEquals("bill", line.Column2);
			AssertEquals("2", line.Column3);

			line = rawUsage.Summary.Lines[1];
			AssertEquals("ABC", line.Column1);
			AssertEquals("joe", line.Column2);
			AssertEquals("1", line.Column3);

			AssertEquals(2, rawUsage.Summary.Lines.Count);

			string expectedCsvResult =
@"""Company Code"",""User"",""Screen Count""
""ABC"",""bill"",""2""
""ABC"",""joe"",""1""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""05-Mar-15 00:00"",""ABC"","""","""",""DDD-ABC-SYD bill"","""","""",""2""
""03-Mar-15 00:00"",""ABC"","""","""",""DDD-ABC-SYD joe"","""","""",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage()
		{
			var lic1 = CreateLicenceAndUsage();

			var billingSystem = new DpsBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 3, 1), lic1.LA_LD, ZGuid.Empty, lic1.ClientCompany.PK);
			var rawUsage = billingSystem.LoadStlRawUsage(context);

			var line = rawUsage.Summary.Lines[0];
			AssertEquals("ABC", line.Column1);
			AssertEquals("bill", line.Column2);
			AssertEquals("2", line.Column9);

			line = rawUsage.Summary.Lines[1];
			AssertEquals("ABC", line.Column1);
			AssertEquals("joe", line.Column2);
			AssertEquals("1", line.Column9);

			AssertEquals(2, rawUsage.Summary.Lines.Count);

			string expectedCsvResult =
@"""Company Code"",""User"",""Screen Count""
""ABC"",""bill"",""2""
""ABC"",""joe"",""1""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""05-Mar-15 00:00"",""ABC"","""","""",""DDD-ABC-SYD bill"","""","""",""2""
""03-Mar-15 00:00"",""ABC"","""","""",""DDD-ABC-SYD joe"","""","""",""1""
", writer.ToString());
		}

		LicenceHeader CreateLicenceAndUsage()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var lic2 = BillingTestHelper.CreateDependentLicence(lic1, "DEF");

			var db = lic1.Database;
			db.LD_DatabaseNumber = 8201;

			Factory.Save();

			var clientNumber = db.DatabaseId;

			AddEHubTransaction(lic1.ClientCompany, "BEFORE", new DateTime(2015, 1, 1));

			AddEHubTransaction(lic1.ClientCompany, "joe", new DateTime(2015, 3, 3));
			AddEHubTransaction(lic1.ClientCompany, "bill", new DateTime(2015, 3, 4));
			AddEHubTransaction(lic1.ClientCompany, "bill", new DateTime(2015, 3, 5));

			AddEHubTransaction(lic2.ClientCompany, "ned", new DateTime(2015, 3, 5));

			AddEHubTransaction(lic1.ClientCompany, "AFTER", new DateTime(2015, 4, 1));

			return lic1;
		}

		static void AddEHubTransaction(ClientCompany clientCompany, string userName, DateTime requestUtc)
		{
			var clientNumber = clientCompany.Database.DatabaseId + '.' + clientCompany.LCC_Code;
			var record = new EServicesBillingTestHelper.RawUsageInfo(
				category: "DPS",
				priceItemCode: "DPS",
				messageTimeUTC: requestUtc,
				clientID: clientCompany.LicenceCode,
				clientNumber: clientNumber,
				systemId: clientCompany.Database.DatabaseId,
				companyPk: clientCompany.PK,
				reference1: "SCR",
				reference2: "",
				reference3: "",
				reference4: userName);

			EServicesBillingTestHelper.InsertTransaction(record);
		}

		#endregion

	}
}