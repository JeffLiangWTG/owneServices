using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class StlReportingBusinessObjectGPCTest : TestCaseWithFactory
	{
		public void TestGPC()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("CN", (NoResString)"CN");
			list.Add("HK", (NoResString)"CN");
			list.Add("MO", (NoResString)"CN");
			list.Add("TW", (NoResString)"CN");
			list.Add("AE", (NoResString)"AE", false);
			list.Add("BH", (NoResString)"AE", false);
			list.Add("KW", (NoResString)"AE", false);
			list.Add("OM", (NoResString)"AE", false);
			list.Add("QA", (NoResString)"AE", false);
			list.Add("SA", (NoResString)"AE", false);
			EDIDataRegistry.Instance.BillingCountryGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", false);
			var db = lic1.Database;
			var priceItem = lic1.Company.PriceHeaders.AddNew().Items.AddNew();
			priceItem.L7_Code = "GPC";

			CreateCompanyAndUsage(db, "AU1", "AU", 189, true);
			CreateCompanyAndUsage(db, "AU2", "AU", 3, false);
			CreateCompanyAndUsage(db, "GB1", "GB", 6, false);
			CreateCompanyAndUsage(db, "GB2", "GB", 9, false);
			CreateCompanyAndUsage(db, "GB3", "GB", 47, true);
			CreateCompanyAndUsage(db, "CN1", "CN", 40, true);
			CreateCompanyAndUsage(db, "CN2", "HK", 3, false);
			CreateCompanyAndUsage(db, "CN3", "MO", 2, false);
			CreateCompanyAndUsage(db, "NZ1", "NZ", 0, true);
			CreateCompanyAndUsage(db, "AE1", "AE", 20, true);
			CreateCompanyAndUsage(db, "OM1", "OM", 10, true);
			CreateCompanyAndUsage(db, "QA1", "QA", 2, true);

			Factory.Save();

			var builder = new ZStringBuilder();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2016, 3, 1), db.PK, priceItem.PK, ZGuid.Empty, BillingConstants.BillingSystem.ODM);
			report.GetCsvUsageReport((csv) => { builder.AppendLine(csv); });

			string expectedCsvResult =
@"""Country"",""Registered Users""
""AE"",""20""
""AU"",""192""
""CN"",""45""
""GB"",""62""
""NZ"",""0""
""OM"",""10""
""QA"",""2""
";
			AssertEquals(expectedCsvResult, builder.ToString());

			builder = new ZStringBuilder();
			var writer = new ReportWriter(builder);
			report.GetCsvUsageReport(writer);
			expectedCsvResult =
@"29-Feb-16 -  -  -  - AE 20 - GPC -  - 20
29-Feb-16 -  -  -  - AU 192 - GPC -  - 192
29-Feb-16 -  -  -  - CN 45 - GPC -  - 45
29-Feb-16 -  -  -  - GB 62 - GPC -  - 62
29-Feb-16 -  -  -  - NZ 0 - GPC -  - 0
29-Feb-16 -  -  -  - OM 10 - GPC -  - 10
29-Feb-16 -  -  -  - QA 2 - GPC -  - 2
";
			AssertEquals(expectedCsvResult, builder.ToString());

			var gpc = StlReportingBusinessObjectFactory.CreateReportingBusinessObject(new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), db.PK, priceItem.PK, ZGuid.Empty), "ODM");
			var rawUsage = gpc.LoadStlRawUsage();
			var summary = rawUsage.SummarySections[0];
			AssertEquals(",Country,Registered Users,,,,,,,,Usage Summary,,,,", summary.Header.Code);
			AssertEquals(@",AE,20,,,,,,,,,,,,
,AU,192,,,,,,,,,,,,
,CN,45,,,,,,,,,,,,
,GB,62,,,,,,,,,,,,
,NZ,0,,,,,,,,,,,,
,OM,10,,,,,,,,,,,,
,QA,2,,,,,,,,,,,,", string.Join("\r\n", summary.Lines.OfType<SummaryLine>().Select(x => x.Code)));
		}

		public void TestGPC_TW()
		{
			var list = new CodeDescriptionBoolCollection();
			list.Add("CN", (NoResString)"CN");
			list.Add("HK", (NoResString)"CN");
			list.Add("MO", (NoResString)"CN");
			list.Add("TW", (NoResString)"CN");
			EDIDataRegistry.Instance.BillingCountryGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD", false);
			var db = lic1.Database;
			var priceItem = lic1.Company.PriceHeaders.AddNew().Items.AddNew();
			priceItem.L7_Code = "GPC";

			CreateCompanyAndUsage(db, "AU1", "AU", 189, true);
			CreateCompanyAndUsage(db, "AU2", "AU", 3, false);
			CreateCompanyAndUsage(db, "GB1", "GB", 6, false);
			CreateCompanyAndUsage(db, "GB2", "GB", 9, false);
			CreateCompanyAndUsage(db, "GB3", "GB", 47, true);
			CreateCompanyAndUsage(db, "CN1", "CN", 40, true);
			CreateCompanyAndUsage(db, "CN2", "HK", 3, false);
			CreateCompanyAndUsage(db, "CN3", "MO", 2, false);
			CreateCompanyAndUsage(db, "CN4", "TW", 21, false);

			Factory.Save();

			var builder = new ZStringBuilder();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2016, 3, 1), db.PK, priceItem.PK, ZGuid.Empty, BillingConstants.BillingSystem.ODM);
			report.GetCsvUsageReport((csv) => { builder.AppendLine(csv); });

			string expectedCsvResult =
@"""Country"",""Registered Users""
""AU"",""192""
""CN"",""66""
""GB"",""62""
";
			AssertEquals(expectedCsvResult, builder.ToString());

			builder = new ZStringBuilder();
			var writer = new ReportWriter(builder);
			report.GetCsvUsageReport(writer);
			expectedCsvResult =
@"29-Feb-16 -  -  -  - AU 192 - GPC -  - 192
29-Feb-16 -  -  -  - CN 66 - GPC -  - 66
29-Feb-16 -  -  -  - GB 62 - GPC -  - 62
";
			AssertEquals(expectedCsvResult, builder.ToString());

			var gpc = StlReportingBusinessObjectFactory.CreateReportingBusinessObject(new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), db.PK, priceItem.PK, ZGuid.Empty), "ODM");
			var rawUsage = gpc.LoadStlRawUsage();
			var summary = rawUsage.SummarySections[0];
			AssertEquals(",Country,Registered Users,,,,,,,,Usage Summary,,,,", summary.Header.Code);
			AssertEquals(@",AU,192,,,,,,,,,,,,
,CN,66,,,,,,,,,,,,
,GB,62,,,,,,,,,,,,", string.Join("\r\n", summary.Lines.OfType<SummaryLine>().Select(x => x.Code)));
		}

		class ReportWriter : ICsvUsageReportWriter
		{
			readonly ZStringBuilder Builder;

			public ReportWriter(ZStringBuilder builder)
			{
				Builder = builder;
			}

			public void WriteCsvUsageReport(ZDateTime usageTime, ZString company, ZString branch, ZString staff, ZString reference, ZString priceCode, ZString priceItemDescription, ZInt unitCount, ZDecimal? adjustedUnitCount = null, ZString? tenantID = null)
			{
				Builder.AppendLine(string.Join(" - ", usageTime.ToShortDateString(), company, branch, staff, reference, priceCode, priceItemDescription, unitCount));
			}
		}

		#region Implementation

		static void CreateCompanyAndUsage(LicenceDatabase db, string code, string countryCode, int userCount, bool isBiggestCompanyForCountry)
		{
			var clientCompany = BillingTestHelper.CreateClientCompany(db, code);
			clientCompany.LCC_RN_NKCountryCode = countryCode;

			if (userCount > 0)
			{
				BillingTestHelper.CreateChargeableUsage(db.Factory, "STL", "USR", new ZDateTime(2016, 3, 1), clientCompany, userCount);
			}
			BillingTestHelper.CreateChargeableUsage(db.Factory, "STL", "USR", new ZDateTime(2016, 2, 1), clientCompany, 2);

			if (isBiggestCompanyForCountry)
			{
				BillingTestHelper.CreateChargeableUsage(db.Factory, "ODM", "GPC", new ZDateTime(2016, 3, 1), clientCompany, 1);
				BillingTestHelper.CreateChargeableUsage(db.Factory, "ODM", "GPC", new ZDateTime(2016, 2, 1), clientCompany, 1);
			}
		}

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

		#endregion Implementation
	}
}
