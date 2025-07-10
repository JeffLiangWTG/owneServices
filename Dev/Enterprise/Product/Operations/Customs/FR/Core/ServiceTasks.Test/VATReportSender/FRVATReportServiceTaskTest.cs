using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.FR.ServiceTasks.Testing
{
	[TestedType(typeof(FRVATReportServiceTask))]
	public class FRVATReportServiceTaskTest : ServiceTaskTestCase<FRVATReportServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHostedServiceAttribute()
		{
			var attr = typeof(FRVATReportServiceTask).Assembly.GetCustomAttributes<HostedServiceAttribute>().Single(x => x.Code == "FRV");
			AssertEquals("FR Customs VAT Report Sender", attr.Description);
			AssertEquals(typeof(FRVATReportServiceTask).FullName, attr.TypeName);
			AssertEquals("FR,GF,GP,MQ,YT,RE,MF,BL", attr.RequiresCompanyInCountry);
			AssertEquals(true, attr.CanRunInAnyBranch);
			AssertEquals("1Day", attr.MinimumPeriod);
			AssertEquals("1Day", attr.DefaultScheduleRunEvery);
			AssertEquals("3Hours", attr.DefaultScheduleStartAtLocal);
		}

		[TestDate(2022, 05, 04)]
		public void TestRunTask()
		{
			AssertEquals(0, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var importer1 = Factory.NewWithValidTestData<OrgHeader>();
			importer1.OH_Code = "IMP1";
			importer1.OH_FullName = "Importer 1;TST";
			importer1.CustomsCodes.AddNew("EOR", "001234", "FR");
			importer1.CustomsCodes.AddNew("TVA", "004477", "FR");
			var importer1PK = importer1.PK.ToGuid();
			var contact = importer1.Contacts.AddNew();
			contact.OC_Email = "marcus.jiang@wisetechglobal.com";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = OrgConstants.ContactAllocationType.VAT;

			var supplier1 = Factory.NewWithValidTestData<OrgHeader>();
			supplier1.OH_Code = "SUP1";
			supplier1.OH_FullName = "Supplier 1";
			var supplier1PK = supplier1.PK.ToGuid();

			CreateDeclarationAtMinimumRequirement("0000001", GlbCompany.CurrentCompany, importer1PK, supplier1PK);
			Factory.Save();

			var logger = new TestServiceLogger();
			var task = new FRVATReportServiceTask();
			task.ServiceLogger = logger;
			task.RunTask();
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.FirstOrDefault();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "marcus.jiang@wisetechglobal.com" }, email.Recipients.ToStringCollection());
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		}

		[TestDate(2022, 05, 04)]
		public void TestRunForFranceAndTerritories()
		{
			using (FRCustomsDataRegistry.Instance.FRVATReportStaggeringFactor.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 100))
			{
				var territories = Core.Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories.Except(Core.Constants.CountryCodes.France);
				foreach (var territory in territories)
				{
					var company = Factory.New<GlbCompany>();
					company.GC_Code = territory + "1";
					company.GC_RN_NKCountryCode = territory;
					var branch = company.Branches.AddNew();
					branch.GB_Code = territory + "1";
				}
				Factory.Save();

				var task = new FRVATReportServiceTaskForTest();
				task.RunTask();
				AssertEquals(13, task.Reports.Count);
			}
		}

		[TestDate(2022, 05, 04)]
		public void TestRunTask_WhenLastRunTimeIsBlank()
		{
			var task = new FRVATReportServiceTaskForTest();
			task.RunTask();
			AssertEquals(1, task.Reports.Count);
		}

		[TestDate(2022, 05, 04)]
		public void TestRunTask_WhenLastRunTimeIsNotInTheCurrentMonth()
		{
			using (FRCustomsDataRegistry.Instance.FRVATReportLastRunTime.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2022, 04, 30)))
			{
				var task = new FRVATReportServiceTaskForTest();
				task.RunTask();
				AssertEquals(1, task.Reports.Count);
			}
		}

		[TestDate(2022, 05, 04)]
		public void TestDoNotRunTask_WhenLastRunTimeIsInTheCurrentMonth()
		{
			using (FRCustomsDataRegistry.Instance.FRVATReportLastRunTime.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2022, 05, 02)))
			{
				var task = new FRVATReportServiceTaskForTest();
				task.RunTask();
				AssertEquals(0, task.Reports.Count);
			}
		}

		public void TestDoNotRunTaskIntheStartingOfTheMonth()
		{
			SetupFrenchCompaniesUpToAmount(10);

			var task = new FRVATReportServiceTaskForTest();
			using (FRCustomsDataRegistry.Instance.FRVATReportDeadlineDayOfTheMonth.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				task.SetNow(new ZDate(2022, 08, 01));
				task.RunTask();
				AssertEquals(0, task.Reports.Count);

				task.SetNow(new ZDate(2022, 08, 02));
				task.RunTask();
				AssertEquals(0, task.Reports.Count);

				task.SetNow(new ZDate(2022, 08, 03));
				task.RunTask();
				AssertEquals(10, task.Reports.Count);
			}
		}

		public void TestDoNotRunTaskAfterDeadline()
		{
			SetupFrenchCompaniesUpToAmount(10);

			var task = new FRVATReportServiceTaskForTest();
			using (FRCustomsDataRegistry.Instance.FRVATReportDeadlineDayOfTheMonth.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				task.SetNow(new ZDate(2022, 08, 03));
				task.RunTask();
				AssertEquals(4, task.Reports.Count);

				task.SetNow(new ZDate(2022, 08, 04));
				task.RunTask();
				AssertEquals(8, task.Reports.Count);
			}

			using (FRCustomsDataRegistry.Instance.FRVATReportDeadlineDayOfTheMonth.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4))
			{
				task.SetNow(new ZDate(2022, 08, 05));
				task.RunTask();
				AssertEquals("No new report is created because it is after deadline.", 8, task.Reports.Count);
			}
		}

		public void TestProcessAllCompaniesInTheLastDay()
		{
			SetupFrenchCompaniesUpToAmount(10);

			var task = new FRVATReportServiceTaskForTest();

			using (FRCustomsDataRegistry.Instance.FRVATReportDeadlineDayOfTheMonth.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				task.SetNow(new ZDate(2022, 08, 03));
				task.RunTask();
				AssertEquals(4, task.Reports.Count);

				task.SetNow(new ZDate(2022, 08, 04));
				task.RunTask();
				AssertEquals(8, task.Reports.Count);

				SetupFrenchCompaniesUpToAmount(20); // Assume that user adds a few new companies during the month.
				task.SetNow(new ZDate(2022, 08, 05));
				task.RunTask();
				AssertEquals("All companies should be processed in the last day.", 20, task.Reports.Count);
			}
		}

		public void TestWhenThereIsBusinessDay_BeforeDeadline()
		{
			SetupFrenchCompaniesUpToAmount(12);

			var task = new FRVATReportServiceTaskForTest();

			using (FRCustomsDataRegistry.Instance.FRVATReportDeadlineDayOfTheMonth.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				task.SetNow(new ZDate(2022, 10, 03));
				task.RunTask();
				AssertEquals("3rd Oct is Monday, so we have 3 days to process all companies, we will process 12/3=4 companies per day.", 4, task.Reports.Count);

				task.SetNow(new ZDate(2022, 10, 04));
				task.RunTask();
				AssertEquals(8, task.Reports.Count);

				task.SetNow(new ZDate(2022, 10, 05));
				task.RunTask();
				AssertEquals(12, task.Reports.Count);
			}
		}

		public void TestStaggeringFactorWith0Factor()
		{
			AssertReportsInTheNextDays("Total companies: 24, factor: 0, without limit per day.", 0, new[] { 24, 0, 0, 0, 0 });
		}

		public void TestStaggeringFactorWith1Factor()
		{
			AssertReportsInTheNextDays("Total companies: 24, factor: 1, up to 24/4*1=6 per day.", 1, new[] { 6, 6, 6, 6, 0 });
		}

		public void TestStaggeringFactorWith2Factor()
		{
			AssertReportsInTheNextDays("Total companies: 24, factor: 2, up to 24/4*2=12 per day.", 2, new[] { 12, 12, 0, 0, 0 });
		}

		public void TestStaggeringFactorWith3Factor()
		{
			AssertReportsInTheNextDays("Total companies: 24, factor: 3, up to 24/4*3=18 per day.", 3, new[] { 18, 6, 0, 0, 0 });
		}

		public void TestStaggeringFactorWith4Factor()
		{
			AssertReportsInTheNextDays("Total companies: 24, factor: 4, up to 24/4*3=24 per day.", 4, new[] { 24, 0, 0, 0, 0 });
		}

		public void TestStaggeringFactorWith5Factor()
		{
			AssertReportsInTheNextDays("Total companies: 24, factor: 5, up to 24/4*5=30 per day.", 5, new[] { 24, 0, 0, 0, 0 });
		}

		void AssertReportsInTheNextDays(string message, int staggeringFactor, params int[] reportsCount)
		{
			SetupFrenchCompaniesUpToAmount(24);

			var today = new ZDate(2022, 08, 03);
			var totalReports = 0;
			var task = new FRVATReportServiceTaskForTest();

			using (FRCustomsDataRegistry.Instance.FRVATReportDeadlineDayOfTheMonth.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 6))
			using (FRCustomsDataRegistry.Instance.FRVATReportStaggeringFactor.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, staggeringFactor))
			{
				CombineAssertions(message, () =>
				{
					for (var index = 0; index < reportsCount.Length; index++)
					{
						var r = reportsCount[index];
						task.SetNow(today);
						task.RunTask();
						totalReports += r;
						AssertEquals($"day {index + 1}", totalReports, task.Reports.Count);
						today = today.AddDays(1);
					}
				});
			}
		}

		[TestDate(2022, 05, 04, 10, 30, 00)]
		public void TestUpdateLastRunTime_AfterRunTask()
		{
			var task = new FRVATReportServiceTask();
			task.RunTask();
			var lastRunTime = task.GetLastRunTime(GlbCompany.CurrentCompany);
			AssertEquals(new ZDateTime(2022, 05, 04, 10, 30, 00), lastRunTime);
		}

		void SetupFrenchCompaniesUpToAmount(int companiesCount)
		{
			var frCountryCodes = Core.Constants.CountryCodes.FranceAndOverseasDepartmentsAndTerritories.ToList();
			var availableCodes = frCountryCodes.SelectMany(x => Enumerable.Range(0, 10).Select(y => x + y)).ToList();
			var existingFrCompanies = GlbCompany.GetActiveCompanies(x => frCountryCodes.Contains(x.GC_RN_NKCountryCode)).Select(x => x.GC_Code).ToList();
			var companiesToAdd = companiesCount - existingFrCompanies.Count;
			if (companiesToAdd > 0)
			{
				for (var i = companiesToAdd; i > 0; i--)
				{
					var code = availableCodes.First(x => !existingFrCompanies.Contains(x));
					availableCodes.Remove(code);
					var company = Factory.New<GlbCompany>();
					company.GC_Code = code;
					company.GC_RN_NKCountryCode = code.Substring(0, 2);
					var branch = company.Branches.AddNew();
					branch.GB_Code = code;
				}
			}
			Factory.Save();

			AssertEquals($"There should be exact {companiesCount} FR companies.", companiesCount, GlbCompany.GetActiveCompanies(x => frCountryCodes.Contains(x.GC_RN_NKCountryCode)).Length);
		}

		(JobDeclaration declaration, JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine, CusEntryHeader entryHeader, CusEntryLine entryLine) CreateDeclarationAtMinimumRequirement(string reference, GlbCompany company, Guid importer1PK, Guid supplier1PK)
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_OH_Importer = importer1PK;
			declaration.JE_OH_Supplier = supplier1PK;
			declaration.JE_MessageType = "IMP";
			declaration.JE_MessageSubType = "IM";
			declaration.JE_DeclarationReference = "B" + reference;
			declaration.JE_UCR = "UCR" + reference;
			declaration.JE_GC = company.PK;
			declaration.ZG_VATDeferType = "2";

			var vatNumberDocument = declaration.SupportingDocuments.AddNew("1008", "VAT NO.");

			var invoiceHeader = declaration.Invoices.AddNew();

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.ZG_CountryOfSupply = "US";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryReleaseDate = ZDateTimeOffset.Today.AddMonths(-1).ToZDateTime();
			entryHeader.CH_BGMReference = "BGM" + reference;
			entryHeader.EntryNumber = "EntryNum" + reference;

			var entryLine = entryHeader.MergedLines.AddNew();
			var b00 = entryLine.ConfirmedFees.AddNew();
			b00.CF_ChargeType = "B00";
			b00.CF_BaseValue = 40m;
			b00.CF_ChargeAmount = 4m;
			b00.CF_MethodOfPayment = "6";

			invoiceLine.JI_CL = entryLine.PK;

			return (declaration, invoiceHeader, invoiceLine, entryHeader, entryLine);
		}

		[TestDate(2022, 05, 04)]
		public void TestRunTask_InProductionenv()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (FRCustomsDataRegistry.Instance.RunFRVATReportInUAT.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var task = new FRVATReportServiceTaskForTest();
				task.RunTask();
				AssertEquals(1, task.Reports.Count);
			}

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (FRCustomsDataRegistry.Instance.RunFRVATReportInUAT.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var task = new FRVATReportServiceTaskForTest();
				task.RunTask();
				AssertEquals(0, task.Reports.Count);
			}
		}

		[TestDate(2022, 05, 04)]
		public void TestRunTask_NotInUATenv()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			using (FRCustomsDataRegistry.Instance.RunFRVATReportInUAT.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var task = new FRVATReportServiceTaskForTest();
				task.RunTask();
				AssertEquals(0, task.Reports.Count);
			}
		}

		protected override void SetUpCore()
		{
			setupRunFRVATReportInUAT = FRCustomsDataRegistry.Instance.RunFRVATReportInUAT.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		IDisposable setupRunFRVATReportInUAT;

		protected override void TearDownCore()
		{
			setupRunFRVATReportInUAT.Dispose();
		}
	}

	public class FRVATReportServiceTaskForTest : FRVATReportServiceTask
	{
		public FRVATReportServiceTaskForTest()
		{
		}

		public List<VATReport> Reports = new List<VATReport>();
		public Dictionary<ZString, ZDateTime> LastRunTimes = new Dictionary<ZString, ZDateTime>();
		ZDateTime now;

		protected override void ExecuteReport(GlbCompany company)
		{
			base.ExecuteReport(company);
			Reports.Add(new VATReport
			{
				CompanyCode = company.GC_Code
			});
		}

		public void SetNow(ZDateTime dateTime)
		{
			now = dateTime;
		}

		protected override ZDateTime Now => now.IsEmpty ? ZDateTime.Now : now;
	}

	public class VATReport
	{
		public ZString CompanyCode { get; set; }
	}
}
