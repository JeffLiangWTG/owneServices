using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.Ccsuk.ServiceTasks.Testing
{
	public class CcsukMaintenanceServiceTaskRunner_ArchiverTest : TestCaseWithFactory
	{
		[TestDate(2023, 8, 3)]
		public void TestArchiveOldRecordsAndSendReportsWithMultipleCompanies()
		{
			var processor = new CcsukMaintenanceServiceTaskRunner_ArchiverForTest(log);

			var (company1, branch1, group1, staff1) = SetupCompanyData("Company1", "GB1");
			var (company2, branch2, group2, staff2) = SetupCompanyData("Company2", "GB2");

			GBCustomsDataRegistry.Instance.NotificationCcsukArchive.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, group1.PK.ToGuid());
			GBCustomsDataRegistry.Instance.NotificationCcsukArchive.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, group2.PK.ToGuid());

			CreateAWB(branch1.PK);
			CreateAWB(branch2.PK);
			Factory.Save();

			processor.ArchiveOldRecordsAndSendReportsExposed();
			AssertEquals(expected: false, log.Count > 0);

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			CombineAssertions(() =>
			{
				AssertEquals(2, emails.Count);
				var firstEmail = emails.First(c => c.Recipients.Contains(staff1.GS_EmailAddress));
				AssertContains(company1.CompanyName, firstEmail.Body);
				AssertNotContains(company2.CompanyName, firstEmail.Body);

				var secondEmail = emails.First(c => c.Recipients.Contains(staff2.GS_EmailAddress));
				AssertNotContains(company1.CompanyName, secondEmail.Body);
				AssertContains(company2.CompanyName, secondEmail.Body);
			});
		}

		public void TestArchiveOldRecordsAndSendReportsFailWithWarning()
		{
			var processor = new CcsukMaintenanceServiceTaskRunner_ArchiverForTest(log);
			var (company1, _, group1, _) = SetupCompanyData("Company1", "GB1", activeBranch: false);
			Factory.Save();

			processor.ArchiveOldRecordsAndSendReportsExposed();
			CombineAssertions(() =>
			{
				AssertEquals(expected: true, log.Count > 0);
				AssertContains($"Warning|Cannot execute, there are no active UK branches under company {company1.GC_Code}", log.ToString());
			});
		}

		void CreateAWB(ZGuid branchPK)
		{
			var awb = Factory.New<CusMAWB>();
			awb.CM_GB = branchPK;
			awb.ShipmentDescriptionCode = "C";
			awb.Status1Date = ZDateTime.Now.AddDays(-8);
		}

		(GlbCompany company, GlbBranch branch, GlbGroup group, GlbStaff staff) SetupCompanyData(string companyName, string companyCode, bool activeBranch = true)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_Name = companyName;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var branch = company.Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_IsActive = activeBranch;
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = $"staff@{companyName}.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff);
			return (company, branch, group, staff);
		}

		protected override void SetUp()
		{
			base.SetUp();
			log = new TestServiceLogger();
		}

		protected TestServiceLogger log;

		protected class CcsukMaintenanceServiceTaskRunner_ArchiverForTest : CcsukMaintenanceServiceTaskRunner_Archiver
		{
			public CcsukMaintenanceServiceTaskRunner_ArchiverForTest(ILogger log) : base(log) { }
			public void ArchiveOldRecordsAndSendReportsExposed() => ArchiveOldRecordsAndSendReports(new CancellationToken());
		}
	}
}
