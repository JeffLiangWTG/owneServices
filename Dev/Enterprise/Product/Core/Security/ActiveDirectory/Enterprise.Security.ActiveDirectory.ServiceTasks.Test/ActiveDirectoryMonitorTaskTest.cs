using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory.ServiceTasks;
using Enterprise.Security.ActiveDirectory.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Security.ActiveDirectory.ServiceTask.Test
{
	public class ActiveDirectoryMonitorTaskTest : TestCaseWithFactoryAndMocks
	{
		public void TestHostedServiceAttribute()
		{
			var serviceTaskType = typeof(ActiveDirectoryMonitorTask);
			var hostedServiceAttributes = (from HostedServiceAttribute x in serviceTaskType.Assembly.GetCustomAttributes(typeof(HostedServiceAttribute), inherit: false)
										   where x.TypeName.Equals(serviceTaskType.FullName, StringComparison.Ordinal)
										   select x).ToArray();

			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ADN", hostedServiceAttribute.Code);
				AssertEquals("Description", "Active Directory Monitor", hostedServiceAttribute.Description);
				AssertEquals("Category", "SYS", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", false, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1day", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("DefaultScheduleRunEvery", "1day", hostedServiceAttribute.DefaultScheduleRunEvery);
				AssertEquals("DefaultScheduleStartAtLocal", "3hours", hostedServiceAttribute.DefaultScheduleStartAtLocal);
				AssertEquals("DefaultScheduleRandomStartOffset", "60minutes", hostedServiceAttribute.DefaultScheduleRandomStartOffset);
				Assert("ActiveByDefault", !hostedServiceAttribute.ActiveByDefault);
			});
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(ActiveDirectoryMonitorTask).GetMethod(nameof(ActiveDirectoryMonitorTask.CheckIsHostedWithCargoWise));
			Assert("HostedServiceRequirement for ediProd is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			methodInfo = typeof(ActiveDirectoryMonitorTask).GetMethod(nameof(ActiveDirectoryMonitorTask.CheckActiveDirectoryIsDisabled));
			Assert("HostedServiceRequirement for ActiveDirectory is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			methodInfo = typeof(ActiveDirectoryMonitorTask).GetMethod(nameof(ActiveDirectoryMonitorTask.CheckAttributeGS_IsActiveNotSynced));
			Assert("HostedServiceRequirement for GS_IsActive is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		public void TestRunTask_NotCW1Hosted_ShouldNotRun()
		{
			EnvProxy.SetHostedLocationForTest("");
			Assert(!EnvProxy.IsHostedWithCargowise);

			AssertEquals("This service task is not required to run for self-hosted system.", ActiveDirectoryMonitorTask.CheckIsHostedWithCargoWise());
		}

		public void TestRunTask_CW1Hosted()
		{
			EnvProxy.SetHostedLocationForTest("hosted-cw1.test");
			Assert(EnvProxy.IsHostedWithCargowise);

			AssertEquals("", ActiveDirectoryMonitorTask.CheckIsHostedWithCargoWise());
		}

		public void TestRunTask_NotEDIClient_ShouldNotRun()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.ECU))
			{
				AssertEquals("This service task is not required to run for self-hosted system.", ActiveDirectoryMonitorTask.CheckIsHostedWithCargoWise());
			}
		}

		public void TestRunTask_EDIClient()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				AssertEquals("", ActiveDirectoryMonitorTask.CheckIsHostedWithCargoWise());
			}
		}

		public void TestRunTask_ADDisabled_ShouldNotRun()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;

				AssertEquals("This service task is not required to run for non-AD Integrated system.", ActiveDirectoryMonitorTask.CheckActiveDirectoryIsDisabled());
			}
		}

		public void TestRunTask_GS_IsActive_NotMapped_ShouldNotRun()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var map = new AttributeMap();
				map.MapItems.Add(new AttributeMapItem("GS_IsActive", "isActive", false));
				ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

				AssertEquals("This service task is not required to run because GS_IsActive attribute is not synced.", ActiveDirectoryMonitorTask.CheckAttributeGS_IsActiveNotSynced());
			}
		}

		IDisposable SetUpValidADEnv()
		{
			var client = ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI);

			EnvProxy.SetHostedLocationForTest("hosted-cw1.test");
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var map = new AttributeMap();
			map.MapItems.Add(new AttributeMapItem("GS_IsActive", "isActive", true));
			ActiveDirectoryRegistry.Instance.AttributeMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, map);

			return client;
		}

		GlbStaff TestADStaffWithStatus(bool staffIsActive, bool adIsActive)
		{
			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron", active: adIsActive);

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "sauron";
			staff.GS_Code = "SAU";
			staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
			staff.GS_IsActive = staffIsActive;
			staff.GS_FullName = "Lord Sauron";
			Factory.Save();

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, TestConstants.ValidOU)).Returns(directoryEntry);

			var logger = new SimpleLogger();
			var task = new ActiveDirectoryMonitorTask { ServiceLogger = logger };
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;
			AssertNoExceptionThrown(() => task.RunTask());

			return staff;
		}

		public void TestRunTask_Staff_NoMismatch(bool active)
		{
			using (SetUpValidADEnv())
			{
				_ = TestADStaffWithStatus(active, active);

				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestRunTask_Staff_NoMismatch_BothActive()
		{
			TestRunTask_Staff_NoMismatch(true);
		}

		public void TestRunTask_Staff_NoMismatch_BothInActive()
		{
			TestRunTask_Staff_NoMismatch(false);
		}

		public void TestRunTask_Staff_Mismatch_ActiveStatus_Inactive()
		{
			using (SetUpValidADEnv())
			{
				var staff = TestADStaffWithStatus(false, true);

				AssertEquals($@"Staff '{staff.GS_Code}' is inactive in CW1, but it is still active in AD.

Please assign this Issue to ADI or IS to investigate immediately. Thanks", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestRunTask_Staff_Mismatch_ActiveStatus_Active()
		{
			using (SetUpValidADEnv())
			{
				var staff = TestADStaffWithStatus(true, false);

				AssertEquals($@"Staff '{staff.GS_Code}' is active in CW1, but it is still inactive in AD.

Please assign this Issue to ADI or IS to investigate immediately. Thanks", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		GlbGroup TestADGroupWithStatus(bool groupIsActive)
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "TestGroup";
			group.GG_Code = "GroupCode";
			group.GG_DomainName = TestConstants.Domain;
			group.GG_IsActive = groupIsActive;

			var directoryEntry = DummyDirectoryEntryWrapper.CreateGroup(group.GG_Desc);
			group.GG_ActiveDirectoryObjectGuid = directoryEntry.Guid;
			Factory.Save();

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(It.IsAny<string>(), TestConstants.ValidOU)).Returns(directoryEntry);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindGroup(It.IsAny<Guid>(), TestConstants.ValidOU)).Returns(directoryEntry);

			var logger = new SimpleLogger();
			var task = new ActiveDirectoryMonitorTask { ServiceLogger = logger };
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;
			AssertNoExceptionThrown(() => task.RunTask());

			return group;
		}

		public void TestRunTask_Group_NoMismatch(bool active)
		{
			using (SetUpValidADEnv())
			{
				_ = TestADGroupWithStatus(active);

				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
			}
		}

		public void TestRunTask_Group_NoMismatch_BothActive()
		{
			TestRunTask_Group_NoMismatch(true);
		}

		public void TestRunTask_Group_Mismatch_ActiveStatus_Inactive()
		{
			using (SetUpValidADEnv())
			{
				var group = TestADGroupWithStatus(false);

				AssertEquals($@"Group '{group.GG_Code}' is inactive in CW1, but it is still active in AD.

Please assign this Issue to ADI or IS to investigate immediately. Thanks", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
		}

		readonly Guid MinGuid = new Guid("00000000-0000-0000-0000-000000000000");
		readonly Guid MaxGuid = new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff");
	}

	[TestedType(typeof(ActiveDirectoryMonitorTask))]
	class ActiveDirectoryMonitorTaskTestTaskTestCase : ServiceTaskTestCase<ActiveDirectoryMonitorTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
