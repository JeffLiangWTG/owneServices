using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class ABMCustomsBillingSystemTest : TestCaseWithFactory
	{
		public void TestSystemCode()
		{
			var billingSystem = new ABMCustomsBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.ABMCustoms, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ABMCustoms, "CTM", new ZDateTime(2015, 2, 1), ZGuid.Empty, 10);
			Factory.Save();

			var billingSystem = new ABMCustomsBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 2, 28));

			var bill = billingSystem.LoadSystemBills(context).First() as ABMCustomsBill;
			AssertEquals(BillingConstants.BillingSystem.ABMCustoms, bill.SystemCode);
		}

		public void TestCreateSystemBill_UsingParty()
		{
			var org1 = Factory.New<EDIOrgHeader>();
			org1.OH_Code = "ORG1";
			org1.OH_FullName = org1.OH_Code;

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org1.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "PRD";
			database.LD_LE = enterprise.PK;

			var company1 = Factory.New<LicenceCompany>();
			company1.LC_CompanyCode = "ABC";
			company1.LC_LE = enterprise.PK;
			company1.LC_OH = org1.PK;

			var licenceHeader1 = Factory.New<LicenceHeader>();
			licenceHeader1.LA_LC = company1.PK;
			licenceHeader1.LA_LD = database.PK;

			var org2 = Factory.New<EDIOrgHeader>();
			org2.OH_Code = "ORG2";
			org2.OH_FullName = org1.OH_Code;

			var company2 = Factory.New<LicenceCompany>();
			company2.LC_CompanyCode = "DEF";
			company2.LC_LE = enterprise.PK;
			company2.LC_OH = org2.PK;

			var licenceHeader2 = Factory.New<LicenceHeader>();
			licenceHeader2.LA_LC = company2.PK;
			licenceHeader2.LA_LD = database.PK;

			var chargeableUsage = BillingTestHelper.CreateChargeableUsage(Factory, "ABM", new ZDateTime(2015, 5, 1), company2.PK, 10);
			chargeableUsage.U1_LD = database.PK;

			Factory.Save();

			var billingSystem = new ABMCustomsBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 5, 31));
			var systemBills = billingSystem.LoadSystemBills(context);
			AssertEquals(org2.PK, systemBills[0].OrganisationPK);
		}

		public void TestLoadOdplRawUsage()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EEE", "LOL", "MEL");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "FFF", "III", "BRN");
			var org1 = lic1.Company.Header;
			var org2 = lic2.Company.Header;
			var org3 = lic3.Company.Header;

			Factory.Save();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			// org1
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "EBA", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "B00003001", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CAR", new ZDateTime(2014, 2, 2, 10, 0, 0), lic1, "S00003001", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "RES", new ZDateTime(2014, 2, 3, 10, 0, 0), lic1, "E00003001", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "AGSIMP", "", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "NCTSARR", "", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "FRP", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "AGSIMP", "14/321", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "FRP", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "SAGIMP", "14/8755", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "POC", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "PORTBASE", "20140901093115632-4342", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "POC", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "PORTBASE", "20140901093115643-4342", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic2, "PLDAIMP", "", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic2, "ATLEXT", "", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "POC", new ZDateTime(2014, 2, 1, 10, 0, 0), lic2, "CARGONAUT", "7124203675", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 1, 1, 10, 0, 0), lic3, "NCTSDEP", "", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "POC", new ZDateTime(2014, 2, 1, 10, 0, 0), lic3, "PORTBASE", "20140904110625600", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic3, "NCTSDEP", "", "", "ABM", "EXPORT"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic3, "SAGEXP", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "POC", new ZDateTime(2014, 2, 1, 10, 0, 0), lic3, "PORTBASE", "20140905142513500", "", "ABM"));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new ABMCustomsBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), org1.PK, ZGuid.Empty, lic1.Company.PK, lic1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals("org1: rawUsage.SummarySections[0]: Lines", 2, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals("org1: rawUsage.SummarySections[0]: Top Level Description", " - CustomsWare Messaging", rawUsage.SummarySections[0].Header.TopLevelDescription);
			AssertEquals("org1: rawUsage.SummarySections[1]: Lines", 2, rawUsage.SummarySections[1].Lines.Count);
			AssertEquals("org1: rawUsage.SummarySections[1]: Top Level Description", " - Movement Messaging", rawUsage.SummarySections[1].Header.TopLevelDescription);
			AssertEquals("org1: rawUsage.SummarySections[2]: Lines", 2, rawUsage.SummarySections[2].Lines.Count);
			AssertEquals("org1: rawUsage.SummarySections[2]: Top Level Description", " - Fiscal Rep Invoice", rawUsage.SummarySections[2].Header.TopLevelDescription);

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), org2.PK, ZGuid.Empty, lic2.Company.PK, lic2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals("org2: rawUsage.SummarySections[0]: Lines", 2, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals("org2: rawUsage.SummarySections[0]: Top Level Description", " - CustomsWare Messaging", rawUsage.SummarySections[0].Header.TopLevelDescription);
			AssertEquals("org2: rawUsage.SummarySections[1]: Lines", 1, rawUsage.SummarySections[1].Lines.Count);
			AssertEquals("org2: rawUsage.SummarySections[1]: Top Level Description", " - Movement Messaging", rawUsage.SummarySections[1].Header.TopLevelDescription);

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), org3.PK, ZGuid.Empty, lic3.Company.PK, lic3.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals("org3: rawUsage.SummarySections[0]: Lines", 2, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals("org3: rawUsage.SummarySections[0]: Department", "EXPORT", rawUsage.SummarySections[0].Lines[0].Column3);
			AssertEquals("org3: rawUsage.SummarySections[0]: Top Level Description", " - CustomsWare Messaging", rawUsage.SummarySections[0].Header.TopLevelDescription);
			AssertEquals("org3: rawUsage.SummarySections[1]: Lines", 2, rawUsage.SummarySections[1].Lines.Count);
			AssertEquals("org3: rawUsage.SummarySections[1]: Top Level Description", " - Movement Messaging", rawUsage.SummarySections[1].Header.TopLevelDescription);

			string expectedCsvResult =
@"""Type"",""Client ID"",""Jurisdiction"",""Department"",""Provider"",""Reference"",""Transactions""
""CustomsWare Messaging"",""FFFIIIBRN"",""NCTSDEP"",""EXPORT"","""","""",""1""
""CustomsWare Messaging"",""FFFIIIBRN"",""SAGEXP"","""","""","""",""1""
""Movement Messaging"",""FFFIIIBRN"",""PORTBASE"","""",""20140905142513500"","""",""1""
""Movement Messaging"",""FFFIIIBRN"",""PORTBASE"","""",""20140904110625600"","""",""1""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Feb-14 10:00"",""FFFIIIBRN"",""B24"",""S24"",""CustomsWare Messaging NCTSDEP EXPORT"",""ABM"",""ABM Customs"",""1""
""01-Feb-14 10:00"",""FFFIIIBRN"",""B25"",""S25"",""CustomsWare Messaging SAGEXP"",""ABM"",""ABM Customs"",""1""
""01-Feb-14 10:00"",""FFFIIIBRN"",""B26"",""S26"",""Movement Messaging PORTBASE  20140905142513500"",""ABM"",""ABM Customs"",""1""
""01-Feb-14 10:00"",""FFFIIIBRN"",""B23"",""S23"",""Movement Messaging PORTBASE  20140904110625600"",""ABM"",""ABM Customs"",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EEE", "LOL", "MEL");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "FFF", "III", "BRN");

			var db1 = lic1.Database;
			var db2 = lic2.Database;
			var db3 = lic3.Database;

			Factory.Save();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "EBA", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "B00003001", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CAR", new ZDateTime(2014, 2, 2, 10, 0, 0), lic1, "S00003001", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "RES", new ZDateTime(2014, 2, 3, 10, 0, 0), lic1, "E00003001", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "AGSIMP", "", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "NCTSARR", "", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "FRP", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "AGSIMP", "14/321", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "FRP", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "SAGIMP", "14/8755", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "POC", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "PORTBASE", "20140901093115632-4342", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "POC", new ZDateTime(2014, 2, 1, 10, 0, 0), lic1, "PORTBASE", "20140901093115643-4342", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic2, "PLDAIMP", "", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic2, "ATLEXT", "", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "POC", new ZDateTime(2014, 2, 1, 10, 0, 0), lic2, "CARGONAUT", "7124203675", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 1, 1, 10, 0, 0), lic3, "NCTSDEP", "", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "POC", new ZDateTime(2014, 2, 1, 10, 0, 0), lic3, "PORTBASE", "20140904110625600", "", "ABM"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic3, "NCTSDEP", "", "", "ABM", "EXPORT"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "CTM", new ZDateTime(2014, 2, 1, 10, 0, 0), lic3, "SAGEXP", "", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ABM", "POC", new ZDateTime(2014, 2, 1, 10, 0, 0), lic3, "PORTBASE", "20140905142513500", "", "ABM"));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new ABMCustomsBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), db1.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals("db1: rawUsage.SummarySections[0]: Lines", 2, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals("db1: rawUsage.SummarySections[0]: Top Level Description", " - CustomsWare Messaging", rawUsage.SummarySections[0].Header.TopLevelDescription);
			AssertEquals("db1: rawUsage.SummarySections[1]: Lines", 2, rawUsage.SummarySections[1].Lines.Count);
			AssertEquals("db1: rawUsage.SummarySections[1]: Top Level Description", " - Movement Messaging", rawUsage.SummarySections[1].Header.TopLevelDescription);
			AssertEquals("db1: rawUsage.SummarySections[2]: Lines", 2, rawUsage.SummarySections[2].Lines.Count);
			AssertEquals("db1: rawUsage.SummarySections[2]: Top Level Description", " - Fiscal Rep Invoice", rawUsage.SummarySections[2].Header.TopLevelDescription);

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), db2.PK, ZGuid.Empty, ZGuid.Empty);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals("db2: rawUsage.SummarySections[0]: Lines", 2, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals("db2: rawUsage.SummarySections[0]: Top Level Description", " - CustomsWare Messaging", rawUsage.SummarySections[0].Header.TopLevelDescription);
			AssertEquals("db2: rawUsage.SummarySections[1]: Lines", 1, rawUsage.SummarySections[1].Lines.Count);
			AssertEquals("db2: rawUsage.SummarySections[1]: Top Level Description", " - Movement Messaging", rawUsage.SummarySections[1].Header.TopLevelDescription);

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), db3.PK, ZGuid.Empty, ZGuid.Empty);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertEquals("db3: rawUsage.SummarySections[0]: Lines", 2, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals("db3: rawUsage.SummarySections[0]: Department", "EXPORT", rawUsage.SummarySections[0].Lines[0].Column3);
			AssertEquals("db3: rawUsage.SummarySections[0]: Top Level Description", " - CustomsWare Messaging", rawUsage.SummarySections[0].Header.TopLevelDescription);
			AssertEquals("db3: rawUsage.SummarySections[1]: Lines", 2, rawUsage.SummarySections[1].Lines.Count);
			AssertEquals("db3: rawUsage.SummarySections[1]: Top Level Description", " - Movement Messaging", rawUsage.SummarySections[1].Header.TopLevelDescription);

			string expectedCsvResult =
@"""Type"",""Company Code"",""Jurisdiction"",""Department"",""Provider"",""Reference"",""Transactions""
""CustomsWare Messaging"",""III"",""NCTSDEP"",""EXPORT"","""","""",""1""
""CustomsWare Messaging"",""III"",""SAGEXP"","""","""","""",""1""
""Movement Messaging"",""III"",""PORTBASE"","""",""20140905142513500"","""",""1""
""Movement Messaging"",""III"",""PORTBASE"","""",""20140904110625600"","""",""1""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Feb-14 10:00"",""III"",""B24"",""S24"",""CustomsWare Messaging NCTSDEP EXPORT"",""ABM"",""ABM Customs"",""1""
""01-Feb-14 10:00"",""III"",""B25"",""S25"",""CustomsWare Messaging SAGEXP"",""ABM"",""ABM Customs"",""1""
""01-Feb-14 10:00"",""III"",""B26"",""S26"",""Movement Messaging PORTBASE  20140905142513500"",""ABM"",""ABM Customs"",""1""
""01-Feb-14 10:00"",""III"",""B23"",""S23"",""Movement Messaging PORTBASE  20140904110625600"",""ABM"",""ABM Customs"",""1""
", writer.ToString());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			EServicesBillingTestHelper.CreateTable();
		}

		public override void RunBare()
		{
			base.RunBare();
			EServicesBillingTestHelper.DropTable();
		}

		#endregion
	}
}