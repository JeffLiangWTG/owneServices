using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.ComplianceReport.Testing
{
	public class ComplianceReportComplianceDocumentFinaliserTest : TestCaseWithFactory
	{
		public void TestIsInFinalisedDateOfComplianceSequence()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				Report.ACR_Status = AccComplianceReport.Status.ReportFinalised;

				var branch = Factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = GlbCompany.CurrentCompany.PK;
				branch.GB_Code = "AAA";

				Factory.Save();

				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_SequenceClass = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;
				sequence.XD_Code = "LM1";
				sequence.XD_StartNumber = 1;
				sequence.XD_EndNumber = 100;
				sequence.XD_NextNumber = 1;
				sequence.XD_MaximumNumberDigits = 9;
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				sequence.XD_GB_BranchOwner = branch.PK;

				Assert(!sequence.IsInFinalisedDate);

				sequence.XD_SequenceClass = "TCR";
				sequence.XD_StartDate = new ZDate(2018, 3, 1);
				sequence.XD_ExpiryDate = new ZDate(2018, 3, 31);

				Assert(sequence.IsInFinalisedDate);
			}
		}

		public void TestFindNoSuitableSequenceBookWithDateFlallsInFinalisedDate()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceSequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				complianceSequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				complianceSequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				complianceSequence.XD_Code = "AAA";
				complianceSequence.XD_SequenceClass = "TCR";
				complianceSequence.XD_Prefix = "abc-001";
				complianceSequence.XD_StartNumber = 1;
				complianceSequence.XD_EndNumber = 99;
				complianceSequence.XD_MaximumNumberDigits = 4;
				complianceSequence.XD_NextNumber = 25;
				complianceSequence.XD_AllocationLevel = ComplianceBookAllocationLevel.Branch;
				complianceSequence.XD_IsActive = true;
				complianceSequence.XD_StartDate = new ZDate(2018, 3, 1);
				complianceSequence.XD_ExpiryDate = new ZDateTime(2018, 3, 31);
				Factory.Save();

				var sequenceBooks = AccComplianceSequence.FindSuitableSequenceBook("TCR", ComplianceBookAllocationLevel.Branch, new ZDate(2018, 3, 1), new ZDateTime(2018, 3, 31), GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
				AssertEquals(1, sequenceBooks.Length);
				AssertEquals(complianceSequence.PK, sequenceBooks[0].PK);

				Report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
				Factory.Save();
				sequenceBooks = AccComplianceSequence.FindSuitableSequenceBook("TCR", ComplianceBookAllocationLevel.Branch, new ZDate(2018, 3, 1), new ZDateTime(2018, 3, 31), GlbCompany.CurrentCompany.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
				AssertEquals(0, sequenceBooks.Length);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupReport();
		}

		AccComplianceReport Report;

		void SetupReport()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = "TST";
				reportConfig.ReportTitle = "Test Tax Report";
				reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
				reportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				reportConfig.TaxRegistrationType = "APC";
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;

				var setting1 = reportConfig.Settings.AddNew();
				setting1.ComplianceSubType = "TCR";
				setting1.LedgerType = LedgerTypes.AccountsReceivable;

				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				Report = Factory.New<AccComplianceReport>();
				Report.ACR_ReportType = "TST";
				Report.ACR_DateFrom = new ZDate(2018, 3, 1);
				Report.ACR_DateTo = new ZDate(2018, 3, 31);

				Factory.Save();
			}
		}
	}
}
