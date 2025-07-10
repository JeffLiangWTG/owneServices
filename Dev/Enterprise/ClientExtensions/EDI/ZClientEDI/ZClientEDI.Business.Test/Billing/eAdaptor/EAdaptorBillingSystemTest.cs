using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.eAdaptor.Test
{
	sealed class EAdaptorBillingSystemTest : TestCaseWithFactory
	{
		public void TestSystemCode()
		{
			var billingSystem = new EAdaptorBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.eAdaptor, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill_Fee()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			var clientCompany1 = BillingTestHelper.FindOrCreateClientCompany(lic1);
			var clientCompany2 = BillingTestHelper.FindOrCreateClientCompany(lic2);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");
			BillingTestHelper.SetInvoicingTo(lic2, lic1);

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_LicenceUnitRate = 0.5;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			var priceItem1 = BillingTestHelper.AddPriceItem(priceHeader, "IC1", BillingConstants.FeeType.Transactional, "EAM", 0.10m);
			var priceItem2 = BillingTestHelper.AddPriceItem(priceHeader, "IC2", BillingConstants.FeeType.Transactional, "EAM", 0.30m);
			var priceItem3 = BillingTestHelper.AddPriceItem(priceHeader, "IC3", BillingConstants.FeeType.Included, "EAM", 0.0m);
			var priceFee1 = BillingTestHelper.AddPriceItem(priceHeader, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 250m);
			var priceFee2 = BillingTestHelper.AddPriceItem(priceHeader, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 500m);
			var priceFee3 = BillingTestHelper.AddPriceItem(priceHeader, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 1000m);
			priceFee2.L7_UnitBreak = 1000;
			priceFee3.L7_UnitBreak = 10000;

			Factory.Save();

			var chargeable1a = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.eAdaptor, "IC1", new ZDateTime(2015, 1, 1), lic1, 77);
			var chargeable1b = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.eAdaptor, "IC2", new ZDateTime(2015, 1, 1), lic1, 55);
			var chargeable1c = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.eAdaptor, "IC3", new ZDateTime(2015, 1, 1), lic1, 1000);

			var chargeable2a = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.eAdaptor, "IC1", new ZDateTime(2015, 1, 1), lic2, 3);
			var chargeable2b = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.eAdaptor, "IC2", new ZDateTime(2015, 1, 1), lic2, 13);
			var chargeable2c = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.eAdaptor, "IC3", new ZDateTime(2015, 1, 1), lic2, 17);
			Factory.Save();

			var billing = new EAdaptorBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 1, 31));
			var bills = billing.LoadSystemBills(context);
			var bill1 = bills[0];
			AssertEquals(BillingConstants.BillingSystem.eAdaptor, bill1.SystemCode);
			var usages = bill1.SystemUsages;
			var usage1 = (EAdaptorUsage)usages.FirstOrDefault(x => x.OrganisationPK == lic1.Company.LC_OH);
			var usage2 = (EAdaptorUsage)usages.FirstOrDefault(x => x.OrganisationPK == lic2.Company.LC_OH);
			AssertEquals("TransactionalAmount1", 77 * 0.1m + 55 * 0.3m, usage1.TransactionalAmount);
			AssertEquals("TransactionalAmount2", 3 * 0.1m + 13 * 0.3m, usage2.TransactionalAmount);
			AssertEquals("Fee is per database - not company1", 0m, usage1.Amount - usage1.TransactionalAmount);
			AssertEquals("Fee is per database - not company2", 0m, usage2.Amount - usage2.TransactionalAmount);
			AssertEquals("bill amount includes fee", usage1.Amount + usage2.Amount + 500m, bill1.Amount);

			AssertEquals(2, usages.Count);
			AssertEquals(1, bills.Length);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateSystemBill_FeeNotBilledTwiceWhenAccumulating()
		{
			BillingTestHelper.LoadClientSpecificDocuments();

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);
			var chargeCodes = new CodeDescriptionPairList();
			chargeCodes.AddPair(BillingConstants.BillingSystem.eAdaptor, EDIDataRegistry.Instance.OdplUsageChargeCode.Value);
			EDIDataRegistry.Instance.TransactionChargeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, chargeCodes);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var clientCompany1 = BillingTestHelper.FindOrCreateClientCompany(lic1);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK, "AUD");

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_LicenceUnitRate = 0.5;
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			var priceItem1 = BillingTestHelper.AddPriceItem(priceHeader, "IC1", BillingConstants.FeeType.Transactional, "EAM", 0.10m);
			var priceItem2 = BillingTestHelper.AddPriceItem(priceHeader, "IC2", BillingConstants.FeeType.Transactional, "EAM", 0.30m);
			var priceItem3 = BillingTestHelper.AddPriceItem(priceHeader, "IC3", BillingConstants.FeeType.Included, "EAM", 0.0m);
			var priceFee1 = BillingTestHelper.AddPriceItem(priceHeader, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 250m);
			var priceFee2 = BillingTestHelper.AddPriceItem(priceHeader, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 500m);
			var priceFee3 = BillingTestHelper.AddPriceItem(priceHeader, "EAM", BillingConstants.FeeType.VolumeDatabaseFee, "", 1000m);
			priceFee2.L7_UnitBreak = 1000;
			priceFee3.L7_UnitBreak = 10000;

			Factory.Save();

			var chargeable1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.eAdaptor, "IC1", new ZDateTime(2015, 1, 1), lic1, 2000);
			var chargeable2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.eAdaptor, "IC1", new ZDateTime(2015, 2, 1), lic1, 15000);

			Factory.Save();

			var billing = new EAdaptorBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 1, 31));
			var bills = billing.LoadSystemBills(context);
			var bill1 = bills[0];

			var orgBill = new OrganisationBill(Factory, Env.CurrentBranch.PK, lic1.Company.LC_OH, "AUD", ZDateTime.Now);
			orgBill.AddSystemBill(bill1);
			orgBill.DateTo = new ZDateTime(2015, 1, 31);
			orgBill.SetEnabledSystemCodes(new string[] { BillingConstants.BillingSystem.eAdaptor });
			orgBill.CalculateAll(20);
			var inv = orgBill.CreateInvoice(ZDateTime.Empty);

			// some late usage from last month
			var chargeable1b = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.eAdaptor, "IC2", new ZDateTime(2015, 1, 1), lic1, 1000);
			Factory.Save();

			billing = new EAdaptorBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 2, 28));
			bills = billing.LoadSystemBills(context);
			AssertEquals(1, bills.Length);
			bill1 = bills[0];

			var usages = bill1.SystemUsages;
			AssertEquals(2, usages.Count);
			var newUsage = (EAdaptorUsage)usages.FirstOrDefault(x => x.OrganisationPK == lic1.Company.LC_OH && x.PeriodStart == context.PeriodStart);
			var oldUsage = (EAdaptorUsage)usages.FirstOrDefault(x => x.OrganisationPK == lic1.Company.LC_OH && x.PeriodStart == context.PeriodStart.AddMonths(-1));
			AssertEquals(15000 * 0.1m, newUsage.Amount);
			AssertEquals(1000 * 0.3m, oldUsage.Amount);
			AssertEquals(1000m, newUsage.NonTransactionalAmount);
			AssertEquals("no fee for accumulated usage", 0m, oldUsage.NonTransactionalAmount);
		}

		public void TestCreateSystemBill_TransactionalModule()
		{
			Assert(true);
		}

		public void TestLoadRawUsage()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");
			Factory.Save();

			EServicesBillingTestHelper.CreateTable();

			var billingSystem = new EAdaptorBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), lic.LA_LD, ZGuid.NewZGuid(), lic.ClientCompany.PK);
			billingSystem.LoadRawUsageInCsv(context, false, x => { });

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"", writer.ToString());

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "III", "SYD");

			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 10301;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "III", db.PK, org2.PK, "", "");

			Factory.Save();

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("EAD", "IC1", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", db.DatabaseId + ".ABC", db.DatabaseId, clientCompany1.PK, "B00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("EAD", "IC1", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDIIISYD", db.DatabaseId + ".III", db.DatabaseId, clientCompany2.PK, "S00003001", "", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("EAD", "IC1", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDIIISYD", null, db.DatabaseId, clientCompany2.PK, "E00003001", "", "", "", null));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals("Company ABC", 1, rawUsage.SummarySections[0].Lines.Count);
			AssertEquals("Compnay III", 2, rawUsage.SummarySections[1].Lines.Count);

			string expectedCsvResult =
@"""Message Type"",""Company Code"",""eHub Tracking ID"",""Transaction No."","""",""Message Time (UTC)""
""Acc Transaction (AR/AP Invoice, Journals) - create"",""YD"","""",""B00003001"","""",""01-Mar-16 10:00:00""
""Acc Transaction (AR/AP Invoice, Journals) - create"",""YD"","""",""S00003001"","""",""02-Mar-16 10:00:00""
""Acc Transaction (AR/AP Invoice, Journals) - create"",""YD"","""",""E00003001"","""",""03-Mar-16 10:00:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer2 = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer2);
			AssertEquals(@"""01-Mar-16 10:00"",""YD"","""","""",""B00003001"",""IC1"","""",""1""
""02-Mar-16 10:00"",""YD"","""","""",""S00003001"",""IC1"","""",""1""
""03-Mar-16 10:00"",""YD"","""","""",""E00003001"",""IC1"","""",""1""
", writer2.ToString());
		}
	}
}
