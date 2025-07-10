using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class AutoJobClosureConfigurationFinderTest : TestCaseWithFactory
	{
		public void TestFindBestMatchingJobClosureConfiguration_JobType()
		{
			var configLine1ForClose = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, fromJobStatus: "JFC");
			var configLine1ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD");
			var configLine2ForClose = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "ARV", 20, true, false, fromJobStatus: "JFC");
			var configLine2ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "ARV", 20, true, false, configurationType: "UPD");
			var regValue = TestObjectCreator.CreateJobClosureConfiguration(configLine1ForClose, configLine1ForUpdate, configLine2ForClose, configLine2ForUpdate);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			var configurationForClose = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "ALL", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
			AssertMatchedConfigurationLine(configurationForClose, configLine2ForClose);

			var configurationForUpdate = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "ALL", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			AssertMatchedConfigurationLine(configurationForUpdate, configLine2ForUpdate);

			configurationForClose = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "WKI", "ALL", "ALL", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
			AssertMatchedConfigurationLine(configurationForClose, configLine1ForClose);

			configurationForUpdate = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "WKI", "ALL", "ALL", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			AssertMatchedConfigurationLine(configurationForUpdate, configLine1ForUpdate);
		}

		public void TestFindBestMatchingJobClosureConfiguration_Direction()
		{
			var configLine1ForClose = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, fromJobStatus: "JFC");
			var configLine1ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD");
			var configLine2ForClose = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "JOP", 20, false, false, fromJobStatus: "JFC");
			var configLine2ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "JOP", 20, false, false, configurationType: "UPD");
			var configLine3ForClose = TestObjectCreator.CreateJobClosureConfigLine("SHP", "EXP", "ALL", "ARV", 30, true, true, fromJobStatus: "JFC");
			var configLine3ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("SHP", "EXP", "ALL", "ARV", 30, true, true, configurationType: "UPD");
			var regValue = TestObjectCreator.CreateJobClosureConfiguration(configLine1ForClose, configLine1ForUpdate, configLine2ForClose, configLine2ForUpdate, configLine3ForClose, configLine3ForUpdate);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			var configurationForClose = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "ALL", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
			AssertMatchedConfigurationLine(configurationForClose, configLine3ForClose);

			var configurationForUpdate = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "ALL", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			AssertMatchedConfigurationLine(configurationForUpdate, configLine3ForUpdate);

			configurationForClose = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "IMP", "ALL", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
			AssertMatchedConfigurationLine(configurationForClose, configLine2ForClose);

			configurationForUpdate = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "IMP", "ALL", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			AssertMatchedConfigurationLine(configurationForUpdate, configLine2ForUpdate);
		}

		public void TestFindBestMatchingJobClosureConfiguration_Mode()
		{
			var configLine1ForClose = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, fromJobStatus: "JFC");
			var configLine1ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD");
			var configLine2ForClose = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "JOP", 20, false, false, fromJobStatus: "JFC");
			var configLine2ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "JOP", 20, false, false, configurationType: "UPD");
			var configLine3ForClose = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "SEA", "JOP", 20, false, false, fromJobStatus: "JFC");
			var configLine3ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "SEA", "JOP", 20, false, false, configurationType: "UPD");
			var regValue = TestObjectCreator.CreateJobClosureConfiguration(configLine1ForClose, configLine1ForUpdate, configLine2ForClose, configLine2ForUpdate, configLine3ForClose, configLine3ForUpdate);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			var configurationForClose = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "SEA", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
			AssertMatchedConfigurationLine(configurationForClose, configLine3ForClose);
			var configurationForUpdate = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "SEA", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			AssertMatchedConfigurationLine(configurationForUpdate, configLine3ForUpdate);

			configurationForClose = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "ALL", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
			AssertMatchedConfigurationLine(configurationForClose, configLine2ForClose);
			configurationForUpdate = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "ALL", ZGuid.Empty, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			AssertMatchedConfigurationLine(configurationForUpdate, configLine2ForUpdate);
		}

		public void TestFindBestMatchingJobClosureConfiguration_Department()
		{
			var configLine1ForClose = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, fromJobStatus: "JFC");
			var configLine1ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD");
			var configLine2ForClose = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "PIC", 20, false, false, fromJobStatus: "JFC");
			var configLine2ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "PIC", 20, false, false, configurationType: "UPD");
			var configLine3ForClose = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "ARV", 20, false, false, 0, TestObjectCreator.FEADepartment.PK, fromJobStatus: "JFC");
			var configLine3ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "ALL", "ARV", 20, false, false, 0, TestObjectCreator.FEADepartment.PK, configurationType: "UPD");
			var regValue = TestObjectCreator.CreateJobClosureConfiguration(configLine1ForClose, configLine1ForUpdate, configLine2ForClose, configLine2ForUpdate, configLine3ForClose, configLine3ForUpdate);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			var configurationForClose = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "ALL", TestObjectCreator.FEADepartment.PK, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
			AssertMatchedConfigurationLine(configurationForClose, configLine3ForClose);
			var configurationForUpdate = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "ALL", TestObjectCreator.FEADepartment.PK, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			AssertMatchedConfigurationLine(configurationForUpdate, configLine3ForUpdate);

			configurationForClose = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "ALL", TestObjectCreator.FISDepartment.PK, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
			AssertMatchedConfigurationLine(configurationForClose, configLine2ForClose);
			configurationForUpdate = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "ALL", TestObjectCreator.FISDepartment.PK, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			AssertMatchedConfigurationLine(configurationForUpdate, configLine2ForUpdate);
		}

		public void TestFindBestMatchingJobClosureConfiguration_DirectionAndMode()
		{
			var configLine1ForClose = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, fromJobStatus: "JFC");
			var configLine1ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, configurationType: "UPD");
			var configLine2ForClose = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "SEA", "FAR", 20, false, false, fromJobStatus: "JFC");
			var configLine2ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("SHP", "ALL", "SEA", "FAR", 20, false, false, configurationType: "UPD");
			var configLine3ForClose = TestObjectCreator.CreateJobClosureConfigLine("SHP", "IMP", "ALL", "ARV", 20, false, false, fromJobStatus: "JFC");
			var configLine3ForUpdate = TestObjectCreator.CreateJobClosureConfigLine("SHP", "IMP", "ALL", "ARV", 20, false, false, configurationType: "UPD");
			var regValue = TestObjectCreator.CreateJobClosureConfiguration(configLine1ForClose, configLine1ForUpdate, configLine2ForClose, configLine2ForUpdate, configLine3ForClose, configLine3ForUpdate);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			var configurationForClose = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "IMP", "SEA", TestObjectCreator.FEADepartment.PK, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
			AssertMatchedConfigurationLine(configurationForClose, configLine3ForClose);

			var configurationForUpdate = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "IMP", "SEA", TestObjectCreator.FEADepartment.PK, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			AssertMatchedConfigurationLine(configurationForUpdate, configLine3ForUpdate);

			configurationForClose = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "SEA", TestObjectCreator.FISDepartment.PK, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Close);
			AssertMatchedConfigurationLine(configurationForClose, configLine2ForClose);

			configurationForUpdate = AutoJobClosureConfigurationFinder.FindBestMatchingJobClosureConfiguration(Env.CurrentCompanyPK, "SHP", "EXP", "SEA", TestObjectCreator.FISDepartment.PK, JobConfigurationSelectorHelper.ConfigurationTypeCodes.Update);
			AssertMatchedConfigurationLine(configurationForUpdate, configLine2ForUpdate);
		}

		public void TestGetCompanyPKsThatHaveAutoJoClosureConfiguration()
		{
			var configLine1 = TestObjectCreator.CreateJobClosureConfigLine("ALL", "", "", "JOP", 10, true, true, 0, TestObjectCreator.FESDepartment.PK);
			var regValue = TestObjectCreator.CreateJobClosureConfiguration(configLine1);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			var configs = AutoJobClosureConfigurationFinder.GetCompanyPKsThatHaveAutoJoClosureConfiguration();
			AssertContainsExactElementsInAnyOrder(new ZGuid[] { GlbCompany.CurrentCompany.PK }, configs);
		}

		void AssertMatchedConfigurationLine(JobClosureConfiguration config, JobClosureConfiguration expectedConfig)
		{
			AssertMatchedConfigurationLine(config, expectedConfig.JobType, expectedConfig.DirectionCode, expectedConfig.Mode, expectedConfig.DepartmentPK,
				expectedConfig.ConfigurationType, expectedConfig.CloseJobsWithOpenWip, expectedConfig.CloseJobsWithOpenAcr, expectedConfig.ToJobStatus,
				expectedConfig.FromJobStatus, expectedConfig.JobChargeRecognitionFilter, expectedConfig.Offset, expectedConfig.OffsetType,
				expectedConfig.ReopenRestrictionOffset, expectedConfig.ReopenRestrictionOffsetType);
		}

		void AssertMatchedConfigurationLine(JobClosureConfiguration config, ZString jobType, ZString direction, ZString mode, ZGuid departmentPK,
			ZString configurationType, ZBool openWip, ZBool openAccrual, ZString toJobStatus,
			ZString fromJobStatus, ZString chargeFilter, ZInt closeOffset, ZString closeOffsetType,
			ZInt reopenOffset, ZString reopenOffsetType)
		{
			AssertNotNull(config);
			AssertEquals("Job Type", jobType, config.JobType);
			AssertEquals("Direction", direction, config.DirectionCode);
			AssertEquals("Mode", mode, config.Mode);
			AssertEquals("Department PK", departmentPK, config.DepartmentPK);
			AssertEquals("Configuration Type", configurationType, config.ConfigurationType);
			AssertEquals("Open Wip", openWip, config.CloseJobsWithOpenWip);
			AssertEquals("Open Accrual", openAccrual, config.CloseJobsWithOpenAcr);
			AssertEquals("From Job Status", fromJobStatus, config.FromJobStatus);
			AssertEquals("To Job Status", toJobStatus, config.ToJobStatus);
			AssertEquals("Charge Filter", chargeFilter, config.JobChargeRecognitionFilter);
			AssertEquals("Close Offset", closeOffset, config.Offset);
			AssertEquals("Close Offset type", closeOffsetType, config.OffsetType);
			AssertEquals("ReopenRestrictionOffset", reopenOffset, config.ReopenRestrictionOffset);
			AssertEquals("ReopenRestrictionOffsetType", reopenOffsetType, config.ReopenRestrictionOffsetType);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
