using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Business.Testing
{
	public class ServiceTaskRequirementsCheckerTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			task = Factory.New<ServiceTaskSchedule>();
			task.S5_ScheduleType = "###";
		}

		public void TestDefaults()
		{
			// Arrange
			// Act
			var (result, branch) = GetBranchFromScheduleAndSettings(task);

			// Assert
			AssertEquals(true, result);
			Assert("Some random branch is retrieved.", string.IsNullOrEmpty(branch));
		}

		public void TestDefaultBranch()
		{
			// Arrange
			var serviceTasksProvider = new ServiceTaskScheduleCollectionProvider();
			var serviceTaskScheduleCollection = serviceTasksProvider.Load(Factory);
			var task1a = serviceTaskScheduleCollection.Tasks.AddNew();
			task1a.S5_ScheduleType = "~TA";
			var prevBranch = task.BranchName;

			var (result, branch) = GetBranchFromScheduleAndSettings(task1a);
			AssertEquals(true, result);
			Assert("Some random branch is retrieved.", string.IsNullOrEmpty(branch));

			(result, branch) = GetBranchFromScheduleAndSettings(task1a, "RequiresCompanyInCountry:" + Core.Constants.CountryCodes.Barbados);
			AssertEquals(false, result);
			Assert("Requirements no longer satisfied.", string.IsNullOrEmpty(branch));

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "~TC";
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Barbados;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "~TB";
			newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
			Factory.Save();

			// Act
			(result, branch) = GetBranchFromScheduleAndSettings(task1a, "RequiresCompanyInCountry:" + Core.Constants.CountryCodes.Barbados);

			// Assert
			AssertEquals(true, result);
			AssertEquals(newBranch.GB_Code, branch);
		}

		(bool result, string branchCode) GetBranchFromScheduleAndSettings(ServiceTaskSchedule taskBO, params string[] settings)
		{
			var config = new HostedServiceAttribute(taskBO.S5_ScheduleType, "", "", typeof(object));
			foreach (var setting in settings)
			{
				var colon = setting.IndexOf(':');
				if (colon >= 0)
				{
					config.GetType().GetProperty(setting.Substring(0, colon)).SetValue(config, setting.Substring(colon + 1), Array.Empty<object>());
				}
				else
				{
					config.GetType().GetProperty(setting).SetValue(config, true, Array.Empty<object>());
				}
			}

			var serviceTaskRequirementsChecker = new ServiceTaskRequirementsChecker();

			var result = serviceTaskRequirementsChecker.AttributeSatisfiesRequirements(config, out var branchCode);
			return (result, branchCode);
		}

		ServiceTaskSchedule task;
	}
}
