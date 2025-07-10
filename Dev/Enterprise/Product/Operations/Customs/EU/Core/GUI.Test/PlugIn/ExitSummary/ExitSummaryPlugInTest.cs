using System;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	public class ExitSummaryPlugInTest : ZPlugInGenericTest
	{
		public void TestPluginEnabledOnlyForExport()
		{
			CombineAssertions(() =>
			{
				using (ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>().EnableExitControlPlugin.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					exitSummaryHost.JE_MessageType = MessageTypeList.Codes.Import;
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Plugin disabled", false, plugInWrapper.EnabledExposed);

					exitSummaryHost.JE_MessageType = MessageTypeList.Codes.Export;
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Plugin enabled", true, plugInWrapper.EnabledExposed);
				}
			});
		}

		public void TestRegistryEnablesPlugin()
		{
			var registry = ObjectFactory.Get<Integration.Customs.EUExitControl.IExitControlCustomsDataRegistry>();
			exitSummaryHost.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				using (registry.EnableExitControlPlugin.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
				{
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Plugin enabled", true, plugInWrapper.EnabledExposed);
				}

				using (registry.EnableExitControlPlugin.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
				{
					plugInWrapper.ChangeTheVisibilityExposed();
					AssertEquals("Plugin disabled", false, plugInWrapper.EnabledExposed);
				}
			});
		}

		public void TestName()
		{
			AssertEquals("Name", "EU Exit Summary", plugIn.Name);
		}

		public void TestUserControl()
		{
			AssertNotNull("User Control", plugIn.UserControl);
			AssertEquals("Type", GetUserControlType(), plugIn.UserControl.GetType());
		}

		public void TestOnSelectWhenUsersDontWantToCreateExitSummaryNow()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			plugInWrapper.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();

			CombineAssertions(() =>
			{
				AssertNull("User dont want to create", plugIn.ExitHeader);
				AssertEquals("The Exit Declaration has not been created.", plugIn.PlugInNotDisplayedMessage);
			});
		}

		public void TestExitSummaryMenu()
		{
			AssertType<EcsMessagingMenu>("Exit Summary Menu", plugIn.TopLevelMenu);

			plugIn.Enabled = false;
			plugIn.Enabled = true;
			AssertNull("Precondition: ExitHeader not created", plugIn.ExitHeader);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			plugIn.TopLevelMenu.PerformClick();

			AssertEquals("Are you sure that you want to create the Exit Declaration?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNull("User dont want to create", plugIn.ExitHeader);
			AssertNull("EcsMessagingMenu.ExitHeader", (plugIn.TopLevelMenu as EcsMessagingMenu).ExitHeader);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			plugIn.TopLevelMenu.PerformClick();

			AssertEquals("Are you sure that you want to create the Exit Declaration?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotNull("Exit Summary created", plugIn.ExitHeader);
			AssertNotNull("EcsMessagingMenu.ExitHeader", (plugIn.TopLevelMenu as EcsMessagingMenu).ExitHeader);
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var dec = Factory.New<JobDeclaration>();
			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_ReferenceNumber = "Test";
			exitHeader.CEH_ParentID = dec.PK;
			exitHeader.CEH_ParentTableCode = dec.TablePrefix;
			Factory.Save();
			return GetTestableExitSummaryPlugInWrapper(dec).PlugIn;
		}

		protected virtual Type GetUserControlType() => typeof(ExitSummaryUserControl);

		protected override void SetUp()
		{
			base.SetUp();
			exitSummaryHost = Factory.New<JobDeclaration>();
			plugInWrapper = GetTestableExitSummaryPlugInWrapper(exitSummaryHost);
			plugIn = plugInWrapper.PlugIn;
		}

		protected override void TearDown()
		{
			base.TearDown();
			plugIn?.Dispose();
		}
		ITestableExitSummaryPlugIn plugInWrapper;
		ExitSummaryPlugIn plugIn;
		JobDeclaration exitSummaryHost;

		protected virtual ITestableExitSummaryPlugIn GetTestableExitSummaryPlugInWrapper(JobDeclaration declaration) => new TestExitSummaryPlugIn(declaration);

		class TestExitSummaryPlugIn : ExitSummaryPlugIn, ITestableExitSummaryPlugIn
		{
			public TestExitSummaryPlugIn(IBusiness hostEntity)
				: base(hostEntity)
			{
			}

			public bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed() => base.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();

			public bool EnabledExposed => Enabled;

			ExitSummaryPlugIn ITestableExitSummaryPlugIn.PlugIn => this;

			public void ChangeTheVisibilityExposed() => ChangeTheVisibility();
		}
	}

	public interface ITestableExitSummaryPlugIn
	{
		bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedExposed();
		bool EnabledExposed { get; }
		void ChangeTheVisibilityExposed();
		ExitSummaryPlugIn PlugIn { get; }
	}
}
