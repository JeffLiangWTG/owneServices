using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class BillingSystemTest : TestCaseWithFactory
	{
		public void TestSystemCode()
		{
			AssertEquals("System code", "DUM", dummyBillingSystem.SystemCode);
		}

		public void TestEarliestUsageToAccumulate()
		{
			AssertEquals(new ZDateTime(2010, 11, 1), dummyBillingSystem.EarliestUsageToAccumulate_Exposed);

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, new ZDateTime(2018, 01, 31));
			dummyBillingSystem.LoadSystemBills(context);
			AssertEquals(new ZDateTime(2017, 01, 1), dummyBillingSystem.EarliestUsageToAccumulate_Exposed);
		}

		public void TestGetStartDate()
		{
			AssertEquals(new ZDateTime(2010, 8, 1), dummyBillingSystem.GetStartDate_Exposed(new ZDateTime(2010, 8, 31)));
			AssertEquals(new ZDateTime(2010, 9, 1), dummyBillingSystem.GetStartDate_Exposed(new ZDateTime(2010, 9, 30)));
			AssertEquals(new ZDateTime(2010, 10, 1), dummyBillingSystem.GetStartDate_Exposed(new ZDateTime(2010, 10, 31)));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestLoadSystemBills_NullArgument()
		{
			DummyBillingSystem dummyBillingSystem = new DummyBillingSystem();
			dummyBillingSystem.LoadSystemBills(null);
		}

		public void TestLoadSystemBills()
		{
			var periodStart = new ZDateTime(2010, 08, 01);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, parentOrganisation1.LicCompany.PK, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, childOrganisation11.LicCompany.PK, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, childOrganisation12.LicCompany.PK, 12);

			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, parentOrganisation2.LicCompany.PK, 20);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, childOrganisation21.LicCompany.PK, 21);

			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, childOrganisation31.LicCompany.PK, 31);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, childOrganisation32.LicCompany.PK, 32);

			Factory.Save();

			AssertNull("Precondition: Context", dummyBillingSystem.Context_Exposed);
			AssertEquals("Precondition: call stack empty", true, dummyBillingSystem.TextNotifications.IsEmpty);

			SystemBill[] systemBills = dummyBillingSystem.LoadSystemBills(context);
			AssertEquals("Context", context, dummyBillingSystem.Context_Exposed);

			AssertEquals("Method was called", true, dummyBillingSystem.MethodWasCalled("AddPeriodStartFilter"));
			AssertEquals("Method was called", true, dummyBillingSystem.MethodWasCalled("AddAccumulateUnbilledMonthsFilter"));
			AssertEquals("Method was called", true, dummyBillingSystem.MethodWasCalled("AddAdditionalFilter"));
			AssertEquals("Method was called", true, dummyBillingSystem.MethodWasCalled("LoadChargeableUsages"));
			AssertEquals("Method was called", true, dummyBillingSystem.MethodWasCalled("CreateSystemUsages"));
			AssertEquals("Method was called", true, dummyBillingSystem.MethodWasCalled("CreateSystemBill"));

			foreach (DummyUsage dummyUsage in systemBills[0].SystemUsages)
			{
				AssertEquals("Method was called", true, dummyUsage.MethodWasCalled("CalculateAmount"));
			}

			AssertEquals("3 billing groups", 3, systemBills.Length);
			AssertBillingGroup(systemBills.First(x => x.OrganisationPK == parentOrganisation1.PK), new ZGuid[] { parentOrganisation1.PK, childOrganisation11.PK, childOrganisation12.PK });
			AssertBillingGroup(systemBills.First(x => x.OrganisationPK == parentOrganisation2.PK), new ZGuid[] { parentOrganisation2.PK, childOrganisation21.PK });
			AssertBillingGroup(systemBills.First(x => x.OrganisationPK == parentOrganisation3.PK), new ZGuid[] { childOrganisation31.PK, childOrganisation32.PK });

			context = new BillingRunContext(new BusinessObjectFactory(), new ZDateTime(2010, 8, 31), new ZDateTime(2010, 08, 31), parentOrganisation1.PK);
			systemBills = dummyBillingSystem.LoadSystemBills(context);
			AssertEquals("Billing group only", 1, systemBills.Length);
			AssertBillingGroup(systemBills[0], new ZGuid[] { parentOrganisation1.PK, childOrganisation11.PK, childOrganisation12.PK });
		}

		public void TestLoadSystemBills_IgnoreStl()
		{
			ZDateTime periodStart = new ZDateTime(2010, 08, 01);
			var stlLicence = BillingTestHelper.CreateLicence(Factory, "STL");
			var odmLicence = BillingTestHelper.CreateLicence(Factory, "ODM");
			stlLicence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.SeatTransaction;
			odmLicence.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.OnDemand;
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, stlLicence, 10);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, odmLicence, 10);
			Factory.Save();

			context.IncludeStl = false;
			var odmBills = dummyBillingSystem.LoadSystemBills(context);

			context.IncludeStl = true;
			context.IncludeOdpl = false;
			var stlBills = dummyBillingSystem.LoadSystemBills(context);

			AssertEquals(1, odmBills.Length);
			AssertEquals(1, stlBills.Length);
			AssertEquals(odmBills[0].OrganisationPK, odmLicence.Company.LC_OH);
			AssertEquals(stlBills[0].OrganisationPK, stlLicence.Company.LC_OH);
		}

		void AssertBillingGroup(SystemBill systemBill, ZGuid[] groupOrganisationPK)
		{
			AssertContainsExactElementsInAnyOrder(groupOrganisationPK, Array.ConvertAll(systemBill.SystemUsages.ToArray(), x => x.OrganisationPK));
		}

		public void TestLoadSystemBills_SortOrder()
		{
			ZDateTime periodStart = new ZDateTime(2010, 08, 01);
			var productionLic = parentOrganisation1.LicCompany.LicHeadersForAllDatabases[0];
			var testLic = BillingTestHelper.CreateAnotherDatabase(productionLic, "TST");
			testLic.Database.LD_LicenceType = DatabaseTypes.Codes.Test;

			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, testLic, 11);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, productionLic, 13);

			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart.AddMonths(1), productionLic, 15);
			BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart.AddMonths(1), testLic, 17);

			Factory.Save();
			{
				context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, new ZDateTime(2010, 08, 31));
				var bills = dummyBillingSystem.LoadSystemBills(context);
				AssertEquals(1, bills.Length);
				var bill = bills[0];
				AssertEquals(2, bill.SystemUsages.Count);
				AssertEquals("production first", productionLic.LA_LD, bill.SystemUsages[0].User.DatabasePK);
				AssertEquals("test last", testLic.LA_LD, bill.SystemUsages[1].User.DatabasePK);
			}

			{
				context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
				var bills = dummyBillingSystem.LoadSystemBills(context);
				AssertEquals(1, bills.Length);
				var bill = bills[0];
				AssertEquals(2, bill.SystemUsages.Count);
				AssertEquals("production first", productionLic.LA_LD, bill.SystemUsages[0].User.DatabasePK);
				AssertEquals("test last", testLic.LA_LD, bill.SystemUsages[1].User.DatabasePK);
			}
		}

		// Disables critical validation "Missing Reversing Transaction for this canceled transaction".
		[SuspendCriticalValidation]
		public void TestLoadChargeableUsages_Filtering()
		{
			var today = ZDateTime.Today;
			var currentPeriod = new ZDateTime(today.Year, today.Month, 1);
			var earliestPeriod = currentPeriod.AddMonths(-12);
			var periodStart = earliestPeriod.AddMonths(6);

			ClientChargeableUsage earlierUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart.AddMonths(-1), parentOrganisation1.LicCompany.PK, 11);
			ClientChargeableUsage earlierUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart.AddMonths(-6), childOrganisation12.LicCompany.PK, 10);
			ClientChargeableUsage beforeEarliestUsage = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", dummyBillingSystem.EarliestUsageToAccumulate_Exposed.AddMonths(-1), childOrganisation12.LicCompany.PK, 99);

			ClientChargeableUsage laterUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart.AddMonths(1), parentOrganisation1.LicCompany.PK, 10);
			ClientChargeableUsage laterUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart.AddMonths(6), childOrganisation11.LicCompany.PK, 10);
			ClientChargeableUsage laterUsage3 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart.AddMonths(6), parentOrganisation2.LicCompany.PK, 10);

			ClientChargeableUsage anotherSystemUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "AAA", periodStart, parentOrganisation1.LicCompany.PK, 10);
			ClientChargeableUsage anotherSystemUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, "BBB", periodStart, childOrganisation12.LicCompany.PK, 10);
			ClientChargeableUsage anotherSystemUsage3 = BillingTestHelper.CreateChargeableUsage(Factory, "XXX", periodStart, parentOrganisation2.LicCompany.PK, 10);

			ClientChargeableUsage earlierPartnerClientUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart.AddMonths(-1), parentOrganisation2.LicCompany.PK, 21);
			ClientChargeableUsage earlierPartnerClientUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart.AddMonths(-1), childOrganisation21.LicCompany.PK, 22);
			ClientChargeableUsage earlierPartnerClientUsage3 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart.AddMonths(-1), parentOrganisation3.LicCompany.PK, 23);
			ClientChargeableUsage earlierPartnerClientUsage4 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart.AddMonths(-1), childOrganisation31.LicCompany.PK, 24);

			ClientChargeableUsage validUsage = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", periodStart, childOrganisation12.LicCompany.PK, 17);

			ARInvoice invoiceCancelled = Factory.NewWithValidTestData<ARInvoice>();
			invoiceCancelled.AH_OH = childOrganisation12.PK;
			invoiceCancelled.AH_IsCancelled = true;
			earlierUsage1.U1_AH_Invoice = invoiceCancelled.PK;

			ARInvoice earlierInvoice = Factory.NewWithValidTestData<ARInvoice>();
			earlierInvoice.AH_OH = parentOrganisation1.PK;
			earlierUsage2.U1_AH_Invoice = earlierInvoice.PK;

			ARInvoice currentInvoice = Factory.NewWithValidTestData<ARInvoice>();
			currentInvoice.AH_OH = childOrganisation12.PK;
			validUsage.U1_AH_Invoice = currentInvoice.PK;

			Factory.Save();

			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1), parentOrganisation1.PK);
			SystemBill[] systemBills = dummyBillingSystem.LoadSystemBills(context);
			AssertEquals(1, systemBills.Length);
			AssertEquals("Only the valid usage was loaded", 1, systemBills[0].SystemUsages.Count);
			AssertEquals("The only valid usage from billing group. All others filtered out due to another system code or date.", childOrganisation12.PK, systemBills[0].SystemUsages[0].OrganisationPK);

			dummyBillingSystem = new DummyBillingSystem();
			dummyBillingSystem.AccumulateUnbilledMonths_Exposed = true;
			systemBills = dummyBillingSystem.LoadSystemBills(context);
			AssertEquals(1, systemBills.Length);
			AssertEquals("old unbilled usage was loaded", 2, systemBills[0].SystemUsages.Count);
			var usage1 = systemBills[0].SystemUsages.First(s => s.OrganisationPK == parentOrganisation1.PK);
			var usage2 = systemBills[0].SystemUsages.First(s => s.OrganisationPK == childOrganisation12.PK);
			AssertEquals("usage with cancelled invoice", 11m, usage1.Amount);
			AssertEquals("current usage with invoice", 17m, usage2.Amount);

			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			dummyBillingSystem = new DummyBillingSystem();
			dummyBillingSystem.AccumulateUnbilledMonths_Exposed = true;
			systemBills = dummyBillingSystem.LoadSystemBills(context);
			AssertEquals("old partner usage was loaded", 3, systemBills.Length);
		}

		public void TestLoadChargeableUsages_IgnoreInactiveDatabasesMaxAccumulatedUsage12Mths()
		{
			var now = ZDateTime.Now;
			var periodStart = new ZDateTime(now.Year, now.Month, 1);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "LA1");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "LA2");
			lic2.Database.LD_IsActive = false;
			var lic3 = BillingTestHelper.CreateLicence(Factory, "LA3");
			var usages = new List<ClientChargeableUsage>();

			for (var idx = 1; idx < 15; idx++)
			{
				usages.Add(BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "COR", periodStart.AddMonths(-idx), lic1, idx));

				//U1_LD is inactive
				usages.Add(BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "COR", periodStart.AddMonths(-idx), lic2, idx));

				//U1_LD is null
				usages.Add(BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "COR", periodStart.AddMonths(-idx), lic3.Company.PK, idx));
			}

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1), ZGuid.Empty);
			dummyBillingSystem.AccumulateUnbilledMonths_Exposed = true;
			var systemBills = dummyBillingSystem.LoadSystemBills(context);

			var loadedUsages = systemBills.SelectMany(x => x.SystemUsages).ToArray();

			var loadedData = loadedUsages.Select(x => $"{x.OrganisationPK}-{x.PeriodStart}-{x.Amount.ToZInt()}").ToArray();
			AssertEquals(24, loadedData.Length);

			var expectedData = usages.Where(x => x.Database == null || x.Database.LD_IsActive).Where(x => x.U1_PeriodStart >= periodStart.AddMonths(-12))
				.Select(x => $"{x.OrganisationPK}-{x.U1_PeriodStart}-{x.U1_UnitCount}").ToArray();

			AssertContainsExactElementsInAnyOrder(expectedData, loadedData);
		}

		// Disables critical validation "Missing Reversing Transaction for this canceled transaction".
		[SuspendCriticalValidation]
		public void TestLoadChargeableUsages_AccumulateWhenClientCompanyOnly()
		{
			var today = ZDateTime.Today;
			var currentPeriod = new ZDateTime(today.Year, today.Month, 1);
			var earliestPeriod = currentPeriod.AddMonths(-12);
			var periodStart = earliestPeriod.AddMonths(6);

			var db1 = parentOrganisation1.LicCompany.LicDatabases.AddNew();
			db1.LD_ServerCode = "DB1";
			var db2 = parentOrganisation2.LicCompany.LicDatabases.AddNew();
			db2.LD_ServerCode = "DB1";

			var clientCompany1 = Factory.New<ClientCompany>();
			clientCompany1.LCC_LD = db1.PK;
			clientCompany1.LCC_Name = "Some Co 1";
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			clientCompany1.LCC_Code = "CC1";

			var clientCompany2 = Factory.New<ClientCompany>();
			clientCompany2.LCC_LD = db2.PK;
			clientCompany2.LCC_Name = "Some Co 2";
			clientCompany2.LCC_RN_NKCountryCode = "AU";
			clientCompany2.LCC_Code = "CC2";

			ClientChargeableUsage earlierUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart.AddMonths(-1), clientCompany1, 11);
			ClientChargeableUsage earlierUsage2Invoiced = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart.AddMonths(-6), clientCompany1, 10);
			ClientChargeableUsage beforeEarliestUsage = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", dummyBillingSystem.EarliestUsageToAccumulate_Exposed.AddMonths(-1), clientCompany1, 99);

			ClientChargeableUsage laterUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart.AddMonths(1), clientCompany1, 10);

			ClientChargeableUsage anotherSystemUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "AAA", "", periodStart, clientCompany1, 3333);

			ClientChargeableUsage earlierPartnerClientUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart.AddMonths(-1), clientCompany2, 21);
			earlierPartnerClientUsage1.U1_ManuallyProcessed = true;

			ClientChargeableUsage validUsage = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "", periodStart, clientCompany1, 17);

			ARInvoice invoiceCancelled = Factory.NewWithValidTestData<ARInvoice>();
			invoiceCancelled.AH_OH = childOrganisation12.PK;
			invoiceCancelled.AH_IsCancelled = true;
			earlierUsage1.U1_AH_Invoice = invoiceCancelled.PK;

			ARInvoice earlierInvoice = Factory.NewWithValidTestData<ARInvoice>();
			earlierInvoice.AH_OH = parentOrganisation1.PK;
			earlierUsage2Invoiced.U1_AH_Invoice = earlierInvoice.PK;

			ARInvoice currentInvoice = Factory.NewWithValidTestData<ARInvoice>();
			currentInvoice.AH_OH = childOrganisation12.PK;
			validUsage.U1_AH_Invoice = currentInvoice.PK;

			Factory.Save();

			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1), parentOrganisation1.PK);
			SystemBill[] systemBills = dummyBillingSystem.LoadSystemBills(context);
			AssertEquals(1, systemBills.Length);
			AssertEquals("Only the valid usage was loaded", 1, systemBills[0].SystemUsages.Count);
			AssertEquals("The only valid usage from billing group. All others filtered out due to another system code or date.", clientCompany1.PK, systemBills[0].SystemUsages[0].User.ClientCompanyPK);

			dummyBillingSystem = new DummyBillingSystem();
			dummyBillingSystem.AccumulateUnbilledMonths_Exposed = true;
			systemBills = dummyBillingSystem.LoadSystemBills(context);
			AssertEquals(1, systemBills.Length);
			AssertEquals("old unbilled usage was loaded", 2, systemBills[0].SystemUsages.Count);
			var usage1 = systemBills[0].SystemUsages.First(s => s.User.ClientCompanyPK == clientCompany1.PK && s.PeriodStart == periodStart.AddMonths(-1));
			var usage2 = systemBills[0].SystemUsages.First(s => s.User.ClientCompanyPK == clientCompany1.PK && s.PeriodStart == periodStart);
			AssertEquals("usage with cancelled invoice", 11m, usage1.Amount);
			AssertEquals("current usage with invoice", 17m, usage2.Amount);

			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			dummyBillingSystem = new DummyBillingSystem();
			dummyBillingSystem.AccumulateUnbilledMonths_Exposed = true;
			systemBills = dummyBillingSystem.LoadSystemBills(context);
			AssertEquals("old, manually processed partner usage was not loaded", 1, systemBills.Length);
			AssertEquals("old unbilled usage was loaded", 2, systemBills[0].SystemUsages.Count);
			usage1 = systemBills[0].SystemUsages.First(s => s.User.ClientCompanyPK == clientCompany1.PK && s.PeriodStart == periodStart.AddMonths(-1));
			usage2 = systemBills[0].SystemUsages.First(s => s.User.ClientCompanyPK == clientCompany1.PK && s.PeriodStart == periodStart);
			AssertEquals("usage with cancelled invoice", 11m, usage1.Amount);
			AssertEquals("current usage with invoice", 17m, usage2.Amount);
		}

		public void TestShouldLoadInvoicedUsagesForInactiveDatabases()
		{
			var now = ZDateTime.Now;
			var periodStart = new ZDateTime(now.Year, now.Month, 1);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "LA1");
			lic1.Database.LD_IsActive = false;
			var usage = BillingTestHelper.CreateChargeableUsage(Factory, "DUM", "COR", periodStart, lic1, 100);

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_OH = lic1.Company.Header.PK;
			usage.U1_AH_Invoice = invoice.PK;

			Factory.Save();

			var context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1), ZGuid.Empty);
			dummyBillingSystem.ShouldLoadInvoicedUsagesForInactiveDatabases_Exposed = false;
			var systemBills = dummyBillingSystem.LoadSystemBills(context);
			AssertEquals(0, systemBills.Length);

			dummyBillingSystem.ShouldLoadInvoicedUsagesForInactiveDatabases_Exposed = true;
			systemBills = dummyBillingSystem.LoadSystemBills(context);
			AssertEquals(1, systemBills.Length);
			AssertEquals(100m, systemBills.Single().Amount);
		}

		#region Implementation

		DummyBillingSystem dummyBillingSystem;
		BillingRunContext context;

		EDIOrgHeader parentOrganisation1;
		EDIOrgHeader childOrganisation11;
		EDIOrgHeader childOrganisation12;

		EDIOrgHeader parentOrganisation2;
		EDIOrgHeader childOrganisation21;

		EDIOrgHeader parentOrganisation3;
		EDIOrgHeader childOrganisation31;
		EDIOrgHeader childOrganisation32;

		EDIOrgHeader partnerOrganisation1;
		EDIOrgHeader partnerOrganisation2;

		protected override void SetUp()
		{
			base.SetUp();
			dummyBillingSystem = new DummyBillingSystem();
			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, new ZDateTime(2010, 08, 31));

			partnerOrganisation1 = BillingTestHelper.CreateOrganisation(Factory, "PA1");
			partnerOrganisation2 = BillingTestHelper.CreateOrganisation(Factory, "PA2");
			partnerOrganisation1.LicCompany.SelfBilling.L4_IsPartner = true;
			partnerOrganisation2.LicCompany.SelfBilling.L4_IsPartner = true;

			parentOrganisation1 = BillingTestHelper.CreateOrganisation(Factory, "AAA");
			parentOrganisation1.LicCompany.InvoiceDeliveries.AddNew();

			childOrganisation11 = BillingTestHelper.CreateDependentOrganisation(parentOrganisation1, "AA1");
			childOrganisation12 = BillingTestHelper.CreateDependentOrganisation(parentOrganisation1, "AA2");

			parentOrganisation2 = BillingTestHelper.CreateDependentOrganisation(partnerOrganisation1, "BBB");
			childOrganisation21 = BillingTestHelper.CreateDependentOrganisation(parentOrganisation2, "BB1");

			parentOrganisation3 = BillingTestHelper.CreateDependentOrganisation(partnerOrganisation2, "CCC");
			childOrganisation31 = BillingTestHelper.CreateDependentOrganisation(parentOrganisation3, "CC1");
			childOrganisation32 = BillingTestHelper.CreateDependentOrganisation(parentOrganisation3, "CC2");

			Factory.Save();
		}

		#endregion
	}

	#region DummyBillingSystem

	internal class DummyBillingSystem : BillingSystem
	{
		public DummyBillingSystem(string systemCode)
		{
			this.systemCode = systemCode;
			TextNotifications = new ZStringBuilder();
		}

		public DummyBillingSystem()
			: this("DUM")
		{
		}

		public override string SystemCode
		{
			get { return systemCode; }
		}
		readonly string systemCode;

		public ZDateTime GetStartDate_Exposed(ZDateTime dateToInclusive)
		{
			return GetStartDate(dateToInclusive);
		}

		public BillingRunContext Context_Exposed
		{
			get { return Context; }
		}

		public override SystemRawUsage LoadOdplRawUsage(BillingLoadRawUsageContext context)
		{
			TextNotifications.Append("LoadRawUsageCore");
			return new DummyRawUsage(context);
		}

		public override StlRawUsage LoadStlRawUsage(BillingLoadRawUsageContext context)
		{
			throw new NotImplementedException();
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, Action<string> action)
		{
		}

		protected override void AddPeriodStartFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			TextNotifications.Append("AddPeriodStartFilter");
			base.AddPeriodStartFilter(chargeableUsageQuery);
		}

		protected override void AddAccumulateUnbilledMonthsFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			TextNotifications.Append("AddAccumulateUnbilledMonthsFilter");
			base.AddAccumulateUnbilledMonthsFilter(chargeableUsageQuery);
		}

		protected override void AddAdditionalFilter(ZDBOnlyQuery chargeableUsageQuery)
		{
			TextNotifications.Append("AddAdditionalFilter");
			base.AddAdditionalFilter(chargeableUsageQuery);
		}

		protected override ClientChargeableUsage[] LoadChargeableUsages()
		{
			TextNotifications.Append("LoadChargeableUsages");
			return base.LoadChargeableUsages();
		}

		protected override SystemUsage[] CreateSystemUsages(ClientChargeableUsage[] chargeableUsages)
		{
			TextNotifications.Append("CreateSystemUsages");

			List<SystemUsage> result = new List<SystemUsage>();
			foreach (ClientChargeableUsage chargeableUsage in chargeableUsages)
			{
				result.Add(new DummyUsage(Context.Factory, new UsingParty(chargeableUsage), chargeableUsage.U1_PeriodStart, new ZDecimal(chargeableUsage.U1_UnitCount), SystemCode));
			}

			return result.ToArray();
		}

		protected override SystemBill CreateSystemBill()
		{
			TextNotifications.Append("CreateSystemBill");
			return new SystemBill(Context.Factory);
		}

		public ZDateTime EarliestUsageToAccumulate_Exposed
		{
			get { return EarliestUsageToAccumulate; }
		}

		protected override bool AccumulateUnbilledMonths
		{
			get { return AccumulateUnbilledMonths_Exposed; }
		}
		public bool AccumulateUnbilledMonths_Exposed;

		protected override bool ShouldLoadInvoicedUsagesForInactiveDatabases
		{
			get { return ShouldLoadInvoicedUsagesForInactiveDatabases_Exposed; }
		}
		public bool ShouldLoadInvoicedUsagesForInactiveDatabases_Exposed;

		#region Implementation

		public ZStringBuilder TextNotifications;

		public bool MethodWasCalled(string methodName)
		{
			return TextNotifications.ToStringWithNewLineBetweenAppends().Contains(methodName);
		}

		public void ClearTextNotifications()
		{
			TextNotifications = new ZStringBuilder();
		}

		public override void LoadRawUsageInCsv(BillingLoadRawUsageContext context, bool isStlBilling, ICsvUsageReportWriter writer)
		{
		}

		#endregion
	}

	#endregion
}
