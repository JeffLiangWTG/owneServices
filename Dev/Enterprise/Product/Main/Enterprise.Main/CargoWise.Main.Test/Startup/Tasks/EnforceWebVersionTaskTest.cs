using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	sealed class EnforceWebVersionTaskTest : TestCaseWithFactory
	{
		void CreateWebVersionFeature(GlbGroup group, bool isActive)
		{
			var featureTest = Factory.NewWithValidTestData<StmFeatureTest>();
			featureTest.SFT_FeatureName = StmFeatureTest.WebVersion;
			featureTest.SFT_IsActive = isActive;
			featureTest.SFT_GG_Group = group.PK;
			Factory.Save();
		}

		void AssertShouldExecute(bool enableFeatureTest, bool isInGourp, bool isActive, bool expected, string message)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			if (isInGourp)
			{
				staff.Groups.Add(group);
			}
			Factory.Save();

			DataRegistry.Instance.FeatureTestModeEnabled = enableFeatureTest;
			TemporaryUserContext userContext = new TemporaryUserContext
			{
				StaffLoginName = staff.GS_LoginName
			};
			using (userContext.Set())
			{
				CreateWebVersionFeature(group, isActive);
				AssertEquals(message, expected, new EnforceWebVersionTask().ShouldExecute());
			}
		}

		public void TestShouldExecuteIsTrue()
		{
			AssertShouldExecute(enableFeatureTest: true, isInGourp: true, isActive: true, expected: true, "ShouldExecute is true when all configure is correct.");
		}

		public void TestShouldExecuteIsFalse()
		{
			CombineAssertions(() =>
			{
				AssertShouldExecute(enableFeatureTest: false, isInGourp: true, isActive: true, expected: false, "ShouldExecute is false when feature test mode is not enable.");
				AssertShouldExecute(enableFeatureTest: true, isInGourp: false, isActive: true, expected: false, "ShouldExecute is false when user is not in the group.");
				AssertShouldExecute(enableFeatureTest: true, isInGourp: true, isActive: false, expected: false, "ShouldExecute is false when WebVersion is not active.");
				AssertShouldExecute(enableFeatureTest: false, isInGourp: false, isActive: true, expected: false, "ShouldExecute is false when feature test mode is not enable and user is not in the group.");
				AssertShouldExecute(enableFeatureTest: false, isInGourp: true, isActive: false, expected: false, "ShouldExecute is false when feature test mode is not enable and WebVersion is not active.");
				AssertShouldExecute(enableFeatureTest: true, isInGourp: false, isActive: false, expected: false, "ShouldExecute is false when user is not in the group and WebVersion is not active.");
				AssertShouldExecute(enableFeatureTest: false, isInGourp: false, isActive: false, expected: false, "ShouldExecute is false when feature test mode is not enable, user is not in the group and WebVersion is not active.");
			});
		}

		public void TestExecuteReceiveShutdownMessageWhenMainFormIsNull()
		{
			const string caption = "Enforce Web Version";
			const string message = "You have been added into a Web Version trial. Please use WiseCloud Client to launch the CargoWise Web Version. Look for the CXW icon.";

			var actualMainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				StartupOpenMainFormTask.MainFormInstance = null;
				new EnforceWebVersionTask().Execute();
				AssertEquals(caption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance = actualMainForm;
			}
		}

		[RequiresSTA]
		public void TestExecuteReceiveShutdownMessageWhenMainFormIsVisable()
		{
			const string caption = "Enforce Web Version";
			const string message = "You have been added into a Web Version trial. Please use WiseCloud Client to launch the CargoWise Web Version. Look for the CXW icon.";

			var actualMainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				using (var mainForm = new MainForm())
				{
					StartupOpenMainFormTask.MainFormInstance = mainForm;
					StartupOpenMainFormTask.MainFormInstance.Visible = true;
					new EnforceWebVersionTask().Execute();
					AssertEquals(caption, UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance = actualMainForm;
			}
		}

		public void TestExecuteReceiveShutdownMessageAfterMainFormShown()
		{
			const string caption = "Enforce Web Version";
			const string message = "You have been added into a Web Version trial. Please use WiseCloud Client to launch the CargoWise Web Version. Look for the CXW icon.";

			var actualMainForm = StartupOpenMainFormTask.MainFormInstance;
			try
			{
				using (var mainForm = new MainForm())
				{
					StartupOpenMainFormTask.MainFormInstance = mainForm;
					new EnforceWebVersionTask().Execute();
					Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

					StartupOpenMainFormTask.MainFormInstance.Show();
					Application.DoEvents();
					AssertEquals(caption, UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				StartupOpenMainFormTask.MainFormInstance = actualMainForm;
			}
		}
	}
}
