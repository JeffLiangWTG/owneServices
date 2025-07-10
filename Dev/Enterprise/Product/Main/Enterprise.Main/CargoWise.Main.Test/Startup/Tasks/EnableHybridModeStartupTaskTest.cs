using System;
using CargoWise.Application;
using Enterprise.BlazorWinFormsInterop;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[RequiresSoftware(RequiredSoftware.IsVM)]
	sealed class EnableHybridModeStartupTaskTest : RemoteDesktopServicesTest
	{
		Mock<IWinFormsListener> winFormsListener;
		GlbGroup userGroup;
		GlbStaff staff;

		protected override void SetUp()
		{
			winFormsListener = new Mock<IWinFormsListener>();
			userGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.Groups.Add(userGroup);
			Factory.Save();

			base.SetUp();
		}

		void CreateWinzorFeature(GlbGroup group, bool isActive)
		{
			CreateFeature(group, StmFeatureTest.WinzorFeatureCode, isActive);
		}

		void CreateFeature(GlbGroup group, string featureName, bool isActive)
		{
			var featureTest = Factory.NewWithValidTestData<StmFeatureTest>();
			featureTest.SFT_FeatureName = featureName;
			featureTest.SFT_IsActive = isActive;
			featureTest.SFT_GG_Group = group.PK;
			Factory.Save();
		}

		public void TestShouldExecuteTrueWhenWinzorEnabledForUserAndGroupsConfiguredAndActiveFeature()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			TemporaryUserContext userContext = new TemporaryUserContext
			{
				StaffLoginName = staff.GS_LoginName
			};
			using (userContext.Set())
			{
				CreateWinzorFeature(userGroup, true);
				AssertEquals(true, new EnableHybridModeStartupTask().ShouldExecute());
			}
		}

		public void TestShouldExecuteFalseWhenWinzorDisabledAndGroupsNotConfigured()
		{
			var groupUserIsNotIn = Factory.NewWithValidTestData<GlbGroup>();
			CreateWinzorFeature(groupUserIsNotIn, false);
			AssertEquals(false, new EnableHybridModeStartupTask().ShouldExecute());
		}

		public void TestShouldExecuteFalseWithoutActiveFeature()
		{
			TemporaryUserContext userContext = new TemporaryUserContext
			{
				StaffLoginName = staff.GS_LoginName
			};
			using (userContext.Set())
			{
				CreateWinzorFeature(userGroup, false);
				Assert(!new EnableHybridModeStartupTask().ShouldExecute());
			}
		}

		public void TestShouldExecuteFalseWhenGroupsNotConfigured()
		{
			var groupUserIsNotIn = Factory.NewWithValidTestData<GlbGroup>();
			CreateWinzorFeature(groupUserIsNotIn, true);
			Assert(!new EnableHybridModeStartupTask().ShouldExecute());
		}

		public void TestShouldExecuteFalseWhenBlazorHybridModeNotEnabled()
		{
			CreateWinzorFeature(userGroup, false);
			Assert(!new EnableHybridModeStartupTask().ShouldExecute());
		}

		[ExpectNoExceptions]
		public void TestWinzorMainFormLaunchedWhenMainFormFeatureEnable()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			var mockBlazorClientLauncher = new Mock<IBlazorClientAppLauncher>();
			var launchUri = new Uri("https://localhost:5001");
			mockBlazorClientLauncher.Setup(m => m.Launch(launchUri, default));
			ObjectFactory.Substitute(mockBlazorClientLauncher.Object);

			TemporaryUserContext userContext = new TemporaryUserContext
			{
				StaffLoginName = staff.GS_LoginName
			};

			using (userContext.Set())
			{
				CreateWinzorFeature(userGroup, true);
				CreateFeature(userGroup, StmFeatureTest.WinzorMainFormFeatureCode, true);
				ExecuteHybridStartupTask();
			}

			mockBlazorClientLauncher.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestWinformsListenerNotInitializeWhenOpenWinzorAndWebVersionFeature()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			var blazorClientLauncher = new Mock<IBlazorClientAppLauncher>();
			var launchUri = new Uri("https://localhost:5001");
			blazorClientLauncher.Setup(m => m.Launch(launchUri, default));

			ObjectFactory.Substitute(blazorClientLauncher.Object);

			TemporaryUserContext userContext = new TemporaryUserContext
			{
				StaffLoginName = staff.GS_LoginName
			};

			using (userContext.Set())
			{
				CreateWinzorFeature(userGroup, true);
				CreateFeature(userGroup, StmFeatureTest.WebVersion, true);
				ExecuteHybridStartupTask();
			}
			winFormsListener.Verify(l => l.Initialise(), Times.Never);
			blazorClientLauncher.Verify(b => b.Launch(It.IsAny<Uri>(), null), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestProgramLauncherCalledIfNotRemoteSession()
		{
			ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest(isRemoteAppSession: false, isWTSSession: false));
			winFormsListener.Setup(p => p.Initialise());
			ExecuteHybridStartupTask();
			winFormsListener.VerifyAll();
		}

		public void TestProgramLauncherNotCalledOnFailure()
		{
			ObjectFactory.Substitute<TerminalService>(new BrokenTerminalService());
			AssertExceptionThrown<AggregateException>(() => ExecuteHybridStartupTask());
			winFormsListener.VerifyNoOtherCalls();
		}

		class BrokenTerminalService : TerminalService
		{
			public override bool IsRemoteAppSession => throw new Exception();
			public override bool IsCitrixICA => throw new Exception();
			public override bool IsWTSSession => throw new Exception();
		}

		void ExecuteHybridStartupTask()
		{
			var task = new EnableHybridModeStartupTask(winFormsListener.Object);
			task.Execute();
			task.Task?.Wait();
		}

		protected override void TearDown()
		{
			base.TearDown();
		}
	}
}
