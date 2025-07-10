using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.Startup.Testing
{
	sealed class ForceCW1HomeScreenTaskTest : TestCaseWithFactory
	{
		void CreateForceCW1HomeScreenFeature(GlbGroup group, bool isActive)
		{
			var featureTest = Factory.NewWithValidTestData<StmFeatureTest>();
			featureTest.SFT_FeatureName = StmFeatureTest.ForceCW1HomeScreen;
			featureTest.SFT_IsActive = isActive;
			featureTest.SFT_GG_Group = group.PK;
			Factory.Save();
		}

		void AssertShouldExecute(bool enableFeatureTest, bool isInGroup, bool isActive, bool isCWNextEnabled, bool expected, string message)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			if (isInGroup)
			{
				staff.Groups.Add(group);
			}
			Factory.Save();

			TemporaryUserContext userContext = new TemporaryUserContext
			{
				StaffLoginName = staff.GS_LoginName
			};

			using (RawDataRegistry.Instance.FeatureTestModeEnabled.SetTemporaryValue(default, default, default, enableFeatureTest))
			using (userContext.Set())
			using (CWNextFeatureTestHelper.SetIsCWNextEnabled(isCWNextEnabled))
			{
				CreateForceCW1HomeScreenFeature(group, isActive);
				AssertEquals(message, expected, new ForceCW1HomeScreenTask().ShouldExecute());
			}
		}

		public void TestShouldExecuteIsTrue()
		{
			AssertShouldExecute(enableFeatureTest: true, isInGroup: true, isActive: true, isCWNextEnabled: true, expected: true, "ShouldExecute is true when all configure is correct.");
		}

		public void TestShouldExecuteIsFalse()
		{
			CombineAssertions(() =>
			{
				AssertShouldExecute(enableFeatureTest: false, isInGroup: true, isActive: true, isCWNextEnabled: true, expected: false, "ShouldExecute is false when feature test mode is not enabled.");
				AssertShouldExecute(enableFeatureTest: true, isInGroup: false, isActive: true, isCWNextEnabled: true, expected: false, "ShouldExecute is false when user is not in the group.");
				AssertShouldExecute(enableFeatureTest: true, isInGroup: true, isActive: false, isCWNextEnabled: true, expected: false, "ShouldExecute is false when ForceCW1HomeScreen is not active.");
				AssertShouldExecute(enableFeatureTest: false, isInGroup: false, isActive: true, isCWNextEnabled: true, expected: false, "ShouldExecute is false when feature test mode is not enabled and user is not in the group.");
				AssertShouldExecute(enableFeatureTest: false, isInGroup: true, isActive: false, isCWNextEnabled: true, expected: false, "ShouldExecute is false when feature test mode is not enabled and ForceCW1HomeScreen is not active.");
				AssertShouldExecute(enableFeatureTest: true, isInGroup: false, isActive: false, isCWNextEnabled: true, expected: false, "ShouldExecute is false when user is not in the group and ForceCW1HomeScreen is not active.");
				AssertShouldExecute(enableFeatureTest: false, isInGroup: false, isActive: false, isCWNextEnabled: true, expected: false, "ShouldExecute is false when feature test mode is not enabled, user is not in the group, and ForceCW1HomeScreen is not active.");
				AssertShouldExecute(enableFeatureTest: true, isInGroup: true, isActive: true, isCWNextEnabled: false, expected: false, "ShouldExecute is false when CWNext is disabled.");
				AssertShouldExecute(enableFeatureTest: false, isInGroup: true, isActive: true, isCWNextEnabled: false, expected: false, "ShouldExecute is false when CWNext is disabled, and feature test mode is not enabled.");
				AssertShouldExecute(enableFeatureTest: true, isInGroup: false, isActive: true, isCWNextEnabled: false, expected: false, "ShouldExecute is false when CWNext is disabled, and user is not in the group.");
				AssertShouldExecute(enableFeatureTest: true, isInGroup: true, isActive: false, isCWNextEnabled: false, expected: false, "ShouldExecute is false when CWNext is disabled, and ForceCW1HomeScreen is not active.");
				AssertShouldExecute(enableFeatureTest: false, isInGroup: false, isActive: false, isCWNextEnabled: false, expected: false, "ShouldExecute is false when CWNext is disabled, feature test mode is not enabled, user is not in the group, and ForceCW1HomeScreen is not active.");
			});
		}

		public void TestExecuteShouldAddForceCW1HomeScreenArgumentAndCallRestart()
		{
			using (ApplicationArgumentsTestHelper.TemporaryApplicationArguments([]))
			{
				var mockProgramRestarter = new Mock<IProgramRestarter>();
				var task = new ForceCW1HomeScreenTask(mockProgramRestarter.Object);
				AssertNoExceptionThrown(task.Execute);
				mockProgramRestarter.Verify(pr => pr.Restart(It.IsAny<string>(), It.Is<CommandLineArguments>(args => (bool)args[ApplicationArguments.OptionForceCW1HomeScreen])), Times.Once);
			}
		}
	}
}
