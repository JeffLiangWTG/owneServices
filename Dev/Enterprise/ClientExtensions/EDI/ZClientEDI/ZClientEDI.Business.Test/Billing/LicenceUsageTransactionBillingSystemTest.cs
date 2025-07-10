using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class LicenceUsageTransactionBillingSystemTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			LicenceUsageTransactionBillingSystem billingSystem = new LicenceUsageTransactionBillingSystem("AAA");
			AssertEquals("AAA", billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			var organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			CreateChargeableUsage(organisation1, "AAA", new ZDateTime(2010, 10, 01), 10);

			var u11 = CreateCptLicenceUsage(organisation1, organisation1.Contacts[0], "AAA", new ZDateTime(2010, 10, 1));
			var staff = Factory.New<ClientStaff>();
			staff.LS_LD = u11.Company.LCC_LD;
			staff.LS_Code = "U01";
			staff.LS_FullName = "U01 A";
			staff.LS_Email = "a@test.com";
			u11.LX_LS = staff.PK;

			Factory.Save();

			LicenceUsageTransactionBillingSystem billingSystem = new LicenceUsageTransactionBillingSystem("AAA", "AAA Message");
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			TransactionalSystemBill bill = billingSystem.LoadSystemBills(context).First() as TransactionalSystemBill;
			AssertEquals("AAA", bill.SystemCode);
			AssertEquals(LicenceUsageTransactionBillingSystem.ClientChargeableUsageCode, bill.UsageCodeForBilledUsage);
			AssertEquals("AAA", bill.SystemUsages[0].SubCode);

			var usageContext = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 01), u11.Company.LCC_LD, PriceItem.PK, u11.Company.PK);

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(usageContext, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(@"""Company Code"",""User"",""Usage Description"",""Price Code"",""Price Item Description"",""Date""
""SYD"",""U01 A"",""AAA Message"",""#PD"",""DESC - #PD"",""01-Oct-2010 00:00:00""
", builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(usageContext, true, writer);
			AssertEquals(@"""01-Oct-10 00:00"",""SYD"","""",""U01"",""AAA Message"",""#PD"",""DESC - #PD"",""1""
", writer.ToString());

			var builder2 = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(usageContext, false, (csv) => { builder2.AppendLine(csv); });
			AssertEquals(@"""Company Code"",""User"",""Usage Description"",""Price Code"",""Price Item Description"",""Date""
""SYD"",""U01 A"",""AAA Message"",""#PD"",""DESC - #PD"",""01-Oct-2010 00:00:00""
", builder2.ToString());

			var writer2 = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(usageContext, false, writer2);
			AssertEquals(@"""01-Oct-10 00:00"",""SYD"","""",""U01"",""AAA Message"",""#PD"",""DESC - #PD"",""1""
", writer2.ToString());
		}

		public void TestAddCodeFilter()
		{
			CodeDescriptionPairList cptUsage = new CodeDescriptionPairList();
			cptUsage.AddPair("ACP", "ediACIReporting"); // Env.Licence.ACIReportingPerTransaction
			cptUsage.AddPair("AMS", "ediAMSReporting"); // Env.Licence.AMSReporting
			EDIDataRegistry.Instance.LicenceUsageBilledPerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cptUsage);

			var organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			CreateChargeableUsage(organisation1, "ACP", new ZDateTime(2011, 6, 1), 11);
			CreateChargeableUsage(organisation2, "ACP", new ZDateTime(2011, 6, 1), 3);

			CreateChargeableUsage(organisation1, "ACP", new ZDateTime(2011, 7, 1), 22);

			CreateChargeableUsage(organisation2, "AMS", new ZDateTime(2011, 6, 1), 7);
			CreateChargeableUsage(organisation2, "AMS", new ZDateTime(2011, 7, 1), 13);

			CreateChargeableUsage(organisation1, "ZZZ", new ZDateTime(2011, 6, 1), 17);
			CreateChargeableUsage(organisation1, "ZZZ", new ZDateTime(2011, 5, 1), 19);
			Factory.Save();

			LicenceUsageTransactionBillingSystem billingSystem = new LicenceUsageTransactionBillingSystem("ACP");
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2011, 6, 30));
			SystemBill[] bills = billingSystem.LoadSystemBills(context);
			AssertEquals("bill count", 2, bills.Length);
			TransactionalSystemBill bill1 = (TransactionalSystemBill)bills.First(s => s.OrganisationPK == organisation1.PK);
			TransactionalSystemBill bill2 = (TransactionalSystemBill)bills.First(s => s.OrganisationPK == organisation2.PK);
			AssertEquals("SystemUsages.Count", 1, bill1.SystemUsages.Count);
			AssertEquals("UnitCount", 11, bill1.SystemUsages[0].UnitCount);
			AssertEquals("SystemUsages.Count", 1, bill2.SystemUsages.Count);
			AssertEquals("UnitCount", 3, bill2.SystemUsages[0].UnitCount);

			billingSystem = new LicenceUsageTransactionBillingSystem("AMS");
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2011, 6, 30));
			bills = billingSystem.LoadSystemBills(context);
			AssertEquals("bill count", 1, bills.Length);
			bill1 = (TransactionalSystemBill)bills.First(s => s.OrganisationPK == organisation2.PK);
			AssertEquals("SystemUsages.Count", 1, bill1.SystemUsages.Count);
			AssertEquals("UnitCount", 7, bill1.SystemUsages[0].UnitCount);
		}

		ClientChargeableUsage CreateChargeableUsage(EDIOrgHeader organisation, ZString systemCode, ZDateTime periodStart, ZInt unitCount)
		{
			ClientChargeableUsage result = BillingTestHelper.CreateChargeableUsage(Factory, "CPT", periodStart, organisation.LicCompany.PK, unitCount);
			result.U1_SubCode = systemCode;
			result.U1_LD = organisation.LicCompany.LicHeadersForAllDatabases[0].LA_LD;

			return result;
		}

		public void TestLoadOdplRawUsage()
		{
			var organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var childOrganisation11 = BillingTestHelper.CreateDependentOrganisation(organisation1, "AA1");
			var organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			var organisation3 = BillingTestHelper.CreateOrganisation(Factory, "CCC");

			Factory.Save();

			organisation1.Contacts.AddNew().OC_ContactName = "John Doe";
			var u11 = CreateCptLicenceUsage(organisation1, organisation1.Contacts[0], "AMS", new ZDateTime(2010, 10, 1));
			var u12 = CreateCptLicenceUsage(organisation1, organisation1.Contacts[0], "AMS", new ZDateTime(2010, 10, 2));
			var u13 = CreateCptLicenceUsage(organisation1, organisation1.Contacts[1], "AMS", new ZDateTime(2010, 10, 3));
			var u14 = CreateCptLicenceUsage(organisation1, organisation1.Contacts[0], "AMS", new ZDateTime(2010, 10, 4));

			var u111 = CreateCptLicenceUsage(childOrganisation11, childOrganisation11.Contacts[0], "AMS", new ZDateTime(2010, 10, 1));

			var u21 = CreateCptLicenceUsage(organisation2, organisation2.Contacts[0], "AMS", new ZDateTime(2010, 10, 1));
			var u22 = CreateCptLicenceUsage(organisation2, organisation2.Contacts[0], "AMS", new ZDateTime(2010, 10, 2));

			organisation3.LicCompany.LicHeadersForAllDatabases[0].LA_AgreedLiveDate = new ZDateTime(2010, 10, 20);
			var u32 = CreateCptLicenceUsage(organisation3, organisation3.Contacts[0], "AMS", new ZDateTime(2010, 11, 1));

			Factory.Save();

			LoadAndAssertOdplRawUsage(organisation1, new ClientLicenceUsage[] { u11, u12, u13, u14 });
			LoadAndAssertOdplRawUsage(childOrganisation11, new ClientLicenceUsage[] { u111 });
			LoadAndAssertOdplRawUsage(organisation2, new ClientLicenceUsage[] { u21, u22 });

			// Raw usage for organisation3 is NOT loaded because of the date or licence type
			LoadAndAssertOdplRawUsage(organisation3, Array.Empty<ClientLicenceUsage>());
		}

		void LoadAndAssertOdplRawUsage(EDIOrgHeader org, ClientLicenceUsage[] moduleUsages)
		{
			LicenceUsageTransactionBillingSystem billingSystem = new LicenceUsageTransactionBillingSystem("AMS");
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 1), org.PK, licence.ClientCompany.PK, ZGuid.Empty, licence.Database.PK);
			SystemCodeRawUsage rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;

			for (int i = 0; i < moduleUsages.Length; ++i)
			{
				ClientLicenceUsage moduleUsage = moduleUsages[i];
				var line = rawUsage.Summary.Lines[i];
				AssertEquals("datetime", moduleUsage.LX_UsageTime.ToString("dd-MMM-yyyy HH:mm:ss"), line.Column5);
				AssertEquals("user", moduleUsage.Staff.LS_FullName, line.Column2);
				AssertEquals("company", licence.ClientCompany.LCC_Code, line.Column1);
			}
		}

		public void TestLoadStlRawUsage()
		{
			var organisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			var childOrganisation11 = BillingTestHelper.CreateDependentOrganisation(organisation1, "AA1");
			var organisation2 = BillingTestHelper.CreateOrganisation(Factory, "BBB");
			var organisation3 = BillingTestHelper.CreateOrganisation(Factory, "CCC");

			Factory.Save();

			var db1 = organisation1.LicCompany.LicHeadersForAllDatabases[0];
			var db2 = childOrganisation11.LicCompany.LicHeadersForAllDatabases[0];
			var db3 = organisation2.LicCompany.LicHeadersForAllDatabases[0];
			var db4 = organisation3.LicCompany.LicHeadersForAllDatabases[0];

			organisation1.Contacts.AddNew().OC_ContactName = "John Doe";
			var u11 = CreateCptLicenceUsage(organisation1, organisation1.Contacts[0], "AMS", new ZDateTime(2010, 10, 1));
			var u12 = CreateCptLicenceUsage(organisation1, organisation1.Contacts[0], "AMS", new ZDateTime(2010, 10, 2));
			var u13 = CreateCptLicenceUsage(organisation1, organisation1.Contacts[1], "AMS", new ZDateTime(2010, 10, 3));
			var u14 = CreateCptLicenceUsage(organisation1, organisation1.Contacts[0], "AMS", new ZDateTime(2010, 10, 4));

			var u111 = CreateCptLicenceUsage(childOrganisation11, childOrganisation11.Contacts[0], "AMS", new ZDateTime(2010, 10, 1));

			var u21 = CreateCptLicenceUsage(organisation2, organisation2.Contacts[0], "AMS", new ZDateTime(2010, 10, 1));
			var u22 = CreateCptLicenceUsage(organisation2, organisation2.Contacts[0], "AMS", new ZDateTime(2010, 10, 2));

			organisation3.LicCompany.LicHeadersForAllDatabases[0].LA_AgreedLiveDate = new ZDateTime(2010, 10, 20);
			var u32 = CreateCptLicenceUsage(organisation3, organisation3.Contacts[0], "AMS", new ZDateTime(2010, 11, 1));

			Factory.Save();

			LoadAndAssertStlRawUsage(db1, new ClientLicenceUsage[] { u11, u12, u13, u14 });
			LoadAndAssertStlRawUsage(db2, new ClientLicenceUsage[] { u111 });
			LoadAndAssertStlRawUsage(db3, new ClientLicenceUsage[] { u21, u22 });

			// Raw usage for organisation3 is NOT loaded because of the date or licence type
			LoadAndAssertStlRawUsage(db4, Array.Empty<ClientLicenceUsage>());
		}

		void LoadAndAssertStlRawUsage(LicenceHeader lic, ClientLicenceUsage[] moduleUsages)
		{
			LicenceUsageTransactionBillingSystem billingSystem = new LicenceUsageTransactionBillingSystem("AMS");
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 1), lic.LA_LD, PriceItem.PK, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);

			for (int i = 0; i < moduleUsages.Length; ++i)
			{
				ClientLicenceUsage moduleUsage = moduleUsages[i];
				var line = rawUsage.Summary.Lines[i];
				AssertEquals("user", moduleUsage.Staff.LS_FullName, line.Column2);
				AssertEquals("#PD", line.Column4);
				AssertEquals("DESC - #PD", line.Column5);
				AssertEquals("datetime", moduleUsage.LX_UsageTime.ToString("dd-MMM-yyyy HH:mm:ss"), line.Column9);
			}
		}

		public void TestQuery_Raw_Usage()
		{
			string expected_sql_Raw_Usage = @"
SELECT 
	" + ClientLicenceUsageSchema.Constants.LX_UsageTime + @",
	" + ClientStaffSchema.Constants.LS_FullName + @",
	" + ClientCompanySchema.Constants.LCC_Code + @",
	" + ClientStaffSchema.Constants.LS_Code + @"
FROM 
	dbo.ClientLicenceUsage
	join dbo.ClientCompany ON LX_LCC = LCC_PK
	join dbo.ClientStaff ON LX_LS = LS_PK AND LS_LD = LCC_LD
WHERE
	LX_ModuleCode = 'AAA' 
	AND LX_UsageTime >= @DateFrom AND LX_UsageTime < @DateTo
	AND (LCC_PK = @ClientCompanyPk OR LCC_LD = @DatabasePk)
ORDER By
	" + ClientLicenceUsageSchema.Constants.LX_UsageTime + @"
";
			LicenceUsageTransactionBillingSystemForTest billingSystem = new LicenceUsageTransactionBillingSystemForTest("AAA");
			AssertEquals(expected_sql_Raw_Usage, billingSystem.Query_Raw_Usage_Exposed);
		}

		public static ClientLicenceUsage CreateCptLicenceUsage(EDIOrgHeader organisation, OrgContact staff, string moduleCode, ZDateTime usageDate)
		{
			var licenceHeader = organisation.LicCompany.LicHeadersForAllDatabases[0];
			var databaseStaff = BillingTestHelper.FindOrCreateDatabaseStaffByName(licenceHeader.Database, staff.OC_ContactName);
			var company = BillingTestHelper.FindOrCreateClientCompany(licenceHeader);

			return BillingTestHelper.CreateCptLicenceUsage(company, databaseStaff, moduleCode, usageDate);
		}

		protected override void SetUp()
		{
			base.SetUp();

			PriceItem = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			PriceItem.L7_Code = "#PD";
			PriceItem.L7_Description = "DESC - #PD";
			Factory.Save();
		}

		ClientLicencePriceItem PriceItem;

		#region Implementation

		class LicenceUsageTransactionBillingSystemForTest : LicenceUsageTransactionBillingSystem
		{
			public LicenceUsageTransactionBillingSystemForTest(ZString systemCode)
				: base(systemCode)
			{
			}

			public string Query_Raw_Usage_Exposed
			{
				get { return Query_Raw_Usage; }
			}
		}

		#endregion
	}
}
