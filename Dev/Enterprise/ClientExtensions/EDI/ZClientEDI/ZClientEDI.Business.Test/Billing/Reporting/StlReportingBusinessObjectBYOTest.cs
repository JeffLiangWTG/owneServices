using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class StlReportingBusinessObjectBYOTest : TestCaseWithFactory
	{
		public void TestBYO()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "LE1", "LC1", "LD1");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "LE2", "LC2", "LD2");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "LE3", "LC3", "LD3");

			var db1 = lic1.Database;
			var db2 = lic2.Database;
			var db3 = lic3.Database;

			var priceItem = lic1.Company.PriceHeaders.AddNew().Items.AddNew();
			priceItem.L7_Code = "BYO";
			priceItem.L7_Description = "Un-registered customer supplied device";
			Factory.Save();

			var lc1CC1 = ClientCompany.FindOrCreate(Factory, "CC1", db1.PK, ZGuid.Empty, "", "");
			var lc1CC1Num = db1.DatabaseId + ".CC1";
			var lc1CC2 = ClientCompany.FindOrCreate(Factory, "CC2", db1.PK, ZGuid.Empty, "", "");
			var lc1CC2Num = db1.DatabaseId + ".CC2";

			var lc2CC1 = ClientCompany.FindOrCreate(Factory, "CC1", db2.PK, ZGuid.Empty, "", "");
			var lc2CC1Num = db2.DatabaseId + ".CC1";
			var lc2CC2 = ClientCompany.FindOrCreate(Factory, "CC2", db2.PK, ZGuid.Empty, "", "");
			var lc2CC2Num = db2.DatabaseId + ".CC2";

			var lc3CC1 = ClientCompany.FindOrCreate(Factory, "CC1", db3.PK, ZGuid.Empty, "", "");
			var lc3CC1Num = db3.DatabaseId + ".CC1";
			var lc3CC2 = ClientCompany.FindOrCreate(Factory, "CC2", db3.PK, ZGuid.Empty, "", "");
			var lc3CC2Num = db3.DatabaseId + ".CC2";
			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 1, 0, 0, 0), "EN2CC1LD2", lc2CC1Num, db2.DatabaseId, lc2CC1.PK, "e545d9c0-bcac-499a-b7f4-f1f21886e829", "668f66f6-e0c5-423a-a947-589d32b5c7e3", "BR1", "5BBBDAFFF0CBFB4001FEA9B3C39E33C2EB626B59F311720629", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 2, 0, 0, 0), "EN2CC1LD2", lc2CC2Num, db2.DatabaseId, lc2CC2.PK, "96596887-14cc-44eb-8032-d972bf885b38", "c85d3c14-15e3-4d3f-ae7c-07a177a18cea", "BR2", "74F4302544EEE8BAC1D27F709B2628DD73FE9BF78D71D34568", null, "ENT", 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 3, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "fcfbd202-407b-4100-b5ea-7da0b37fdc5f", "9746192c-9e59-4924-8fc5-40c0046079d5", "BR3", "8CD24C038EE7F38C7A44035A899040FC1F2B5EF1E892F136C0", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 4, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "fb890957-9f5c-4719-b456-724d36e6c44e", "bc6750c5-7a3f-426a-b424-b2129bc81a8c", "BR4", "63FB9D822F5CB9FBDDA855C4FAD20F297DAE4137F080DE78E1", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 5, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "c980e066-99a2-470b-a3d4-02e659eadc1d", "9fface89-6cc4-4341-aee2-d8f8fe838f60", "140,210", "ECF8FF2FB34D513CCE722D9CF5FDB2DC5F724DBC94BE60B60D", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 6, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "00a6dede-9ec2-49b4-8496-b3536a864aeb", "cde7dbe8-b8d0-4707-a1d5-baba581ebe5f", "BR6", "B5AEB38DB3F2C56089BC05465C91957CA1D42FA74F06FEDBCF", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 7, 0, 0, 0), "EN3CC1LD3", lc3CC1Num, db3.DatabaseId, lc3CC1.PK, "3545be51-6fc3-4660-b3aa-67215c733bab", "a5aa44e0-33c1-43ff-a378-47bf037f4897", "BR7", "DE43113301320C1FDC8D57398841AF1D0054A11F8219C53CCF", null, "ENT", 1));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 8, 0, 0, 0), "EN3CC2LD3", lc3CC2Num, db3.DatabaseId, lc3CC2.PK, "ea456521-90e3-46e5-a3b2-e064d83097c3", "2bcbe51f-1347-4d19-9870-4f7e3b5e0f65", "BR8", "D2FF038CD3F31D329FC54741B6A2A3154CC25C00A71A0986FC", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 9, 0, 0, 0), "EN3CC2LD3", lc3CC2Num, db3.DatabaseId, lc3CC2.PK, "97150401-f2c9-4423-9312-9ced28ab5af3", "9ea05816-9645-4285-880c-2d4df5a04a90", "130,140,240", "EA4FD0D8B88C8BB2F50C277EB314D7A5D32152233CC842F818", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 10, 0, 0, 0), "EN3CC2LD3", lc3CC2Num, db3.DatabaseId, lc3CC2.PK, "0305a2fd-3515-4080-a598-510cc7e37f06", "7868f9e7-7ab2-44c4-9ee7-9031897e156a", "BRX", "3D4C0B6CC061B5FBC3A0D9ACC380E7D7F33C52579029B14E83", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 11, 0, 0, 0), "EN3CC2LD3", lc3CC2Num, db3.DatabaseId, lc3CC2.PK, "93bd41a8-f5db-4aed-aad4-c8a520219dbe", "c17a0cdf-068a-40a6-b78f-7714e000da40", "BRY", "ECF8FF2FB34D513CCE722D9CF5FDB2DC5F724DBC94BE60B60D", null, "ENT", 1));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "RF2", new ZDateTime(2022, 4, 12, 0, 0, 0), "EN3CC2LD3", lc3CC2Num, db3.DatabaseId, lc3CC2.PK, "8927b9ef-3bb2-4434-ba76-937b7d330974", "96853ecd-531f-415a-b588-26ab4b2e2fd7", "BRZ", "8CD24C038EE7F38C7A44035A899040FC1F2B5EF1E892F136C0", null, "ENT", 1));

			EServicesBillingTestHelper.AddTransactions(infoList);

			BillingTestHelper.CreateChargeableUsage(db1.Factory, "STL", "BYO", new ZDateTime(2022, 4, 1), lc1CC1, 1);
			BillingTestHelper.CreateChargeableUsage(db1.Factory, "STL", "BYO", new ZDateTime(2022, 4, 1), lc2CC2, 1);
			BillingTestHelper.CreateChargeableUsage(db1.Factory, "STL", "BYO", new ZDateTime(2022, 4, 1), lc3CC1, 1);

			Factory.Save();

			var builder = new ZStringBuilder();
			var report = new StlReportingBusinessObject(Factory, new ZDateTime(2022, 4, 1), db2.PK, priceItem.PK, ZGuid.Empty, BillingConstants.BillingSystem.STL);
			report.GetCsvUsageReport((csv) => { builder.AppendLine(csv); });

			string expectedCsvResult =
@"""Device ID"",""ID Hash"",""Branch"",""Count""
""668f66f6-e0c5-423a-a947-589d32b5c7e3"",""5BBBDAFFF0CBFB4001FEA9B3C39E33C2EB626B59F311720629"",""BR1"",""1""
""c85d3c14-15e3-4d3f-ae7c-07a177a18cea"",""74F4302544EEE8BAC1D27F709B2628DD73FE9BF78D71D34568"",""BR2"",""1""
""Total Device Count"","""","""",""2""
""Registered Devices"","""","""",""-1""
""Handheld Device Licenses"","""","""",""1""
";
			AssertEquals(expectedCsvResult, builder.ToString());

			builder = new ZStringBuilder();
			var writer = new ReportWriter(builder);
			report.GetCsvUsageReport(writer);
			expectedCsvResult =
@"01-Apr-22 -  - BR1 -  - 668f66f6-e0c5-423a-a947-589d32b5c7e3 5BBBDAFFF0CBFB4001FEA9B3C39E33C2EB626B59F311720629 - BYO - Un-registered customer supplied device - 1
02-Apr-22 -  - BR2 -  - c85d3c14-15e3-4d3f-ae7c-07a177a18cea 74F4302544EEE8BAC1D27F709B2628DD73FE9BF78D71D34568 - BYO - Un-registered customer supplied device - 1
30-Apr-22 -  -  -  - Total Device Count - BYO - Total count of all active devices - 2
30-Apr-22 -  -  -  - Registered Devices - BYO - Total count of all registered devices - -1
30-Apr-22 -  -  -  - Handheld Device Licenses - BYO - Numbers of active handheld device licenses - 1
";
			AssertEquals(expectedCsvResult, builder.ToString());

			var byo = StlReportingBusinessObjectFactory.CreateReportingBusinessObject(new BillingLoadRawUsageContext(Factory, new ZDateTime(2022, 4, 1), db2.PK, priceItem.PK, ZGuid.Empty), "STL");
			var rawUsage = byo.LoadStlRawUsage();
			var summary = rawUsage.SummarySections[0];
			AssertEquals(",Device ID,ID Hash,,Branch,,Count,,,,,,,,", summary.Header.Code);
			AssertEquals(@",668f66f6-e0c5-423a-a947-589d32b5c7e3,5BBBDAFFF0CBFB4001FEA9B3C39E33C2EB626B59F311720629,,BR1,,1,,,,,,,,
,c85d3c14-15e3-4d3f-ae7c-07a177a18cea,74F4302544EEE8BAC1D27F709B2628DD73FE9BF78D71D34568,,BR2,,1,,,,,,,,
,─────────────────,─────────────────,,─────,,─────,,,,,,,,
,Total Device Count, ,, ,,2,,,,,,,,
,Registered Devices, ,, ,,-1,,,,,,,,
,Handheld Device Licenses, ,, ,,1,,,,,,,,", string.Join("\r\n", summary.Lines.OfType<SummaryLine>().Select(x => x.Code)));
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
	}
}
