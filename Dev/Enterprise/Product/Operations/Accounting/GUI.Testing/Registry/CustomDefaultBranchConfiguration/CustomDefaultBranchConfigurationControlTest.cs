using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(CustomDefaultBranchConfigurationControl))]
	class CustomDefaultBranchConfigurationControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CustomDefaultBranchConfiguration(null, Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CustomDefaultBranchConfigurationControl)control).ReadOnly;
		}

		public void TestConfigTextIsReadonlyForNonSupportUsers()
		{
			using (ZForm form = new ZForm())
			using (RegistryZUserControl control = GetNewControl())
			{
				Assert("Precondition: IsSupportUser", Env.CurrentUser.IsSupportUser);
				form.Controls.Add(control);
				form.Show();

				control.ReadOnly = true;
				IBusiness businessEntity = GetNewBusinessEntity();
				control.SetDataBinding(businessEntity, null);
				var configTextBoxControl = control.Controls.Find("ConfigTextBox", true).FirstOrDefault();
				AssertNotNull(configTextBoxControl);
				var configTextBox = (ZRichTextBox)configTextBoxControl;
				AssertEquals("ConfigTextBox.IsReadOnly should be false for support user", false, configTextBox.ReadOnly);

				var setRulesEngineLink = control.Controls.Find("SetBranchDefaultingRulesLabel", true).FirstOrDefault();
				AssertNotNull(setRulesEngineLink);
				Assert("Rules Engine Link is enabled", setRulesEngineLink.Enabled);
			}

			var nonSupportStaff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.NotEqual, User.SupportUserName));
			using (Env.SetTemporaryUserContext(nonSupportStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			using (ZForm form = new ZForm())
			using (RegistryZUserControl control = GetNewControl())
			{
				Assert("Precondition: not IsSupportUser", !Env.CurrentUser.IsSupportUser);
				form.Controls.Add(control);
				form.Show();

				control.ReadOnly = true;
				IBusiness businessEntity = GetNewBusinessEntity();
				control.SetDataBinding(businessEntity, null);
				var configTextBoxControl = control.Controls.Find("ConfigTextBox", true).FirstOrDefault();
				AssertNotNull(configTextBoxControl);
				var configTextBox = (ZRichTextBox)configTextBoxControl;
				AssertEquals("ConfigTextBox.IsReadOnly should be true for non support user", true, configTextBox.ReadOnly);

				var setRulesEngineLink = (ZLinkLabel)control.Controls.Find("SetBranchDefaultingRulesLabel", true).FirstOrDefault();
				AssertNotNull(setRulesEngineLink);
				Assert("Rules Engine Link is enabled", setRulesEngineLink.Enabled);

				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
				setRulesEngineLink.OnLinkClicked_Exposed(new System.Windows.Forms.LinkLabelLinkClickedEventArgs(null));
				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);

				AssertEquals("Launched uri is correct.", "https", uri.Scheme);
				AssertEquals("Launched uri is correct.", "address", uri.Host);
				AssertEquals("Launched uri is correct.", "/goto/AccountingBranchSelection", uri.AbsolutePath);
			}
		}

		public void TestCustomDefaultBranchConfigurationControlIsINotifications()
		{
			using (var customDefaultBranchConfigurationControl = new CustomDefaultBranchConfigurationControl())
			{
				Assert("CustomDefaultBranchConfigurationControl implements INotifications.", customDefaultBranchConfigurationControl is INotifications);
			}
		}

		public void TestCustomDefaultBranchConfigurationControlNotifications_Error()
		{
			using (var customDefaultBranchConfigurationControl = new CustomDefaultBranchConfigurationControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)customDefaultBranchConfigurationControl;
				notify.AddError("This is error.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Notification message is correct.", "This is error.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCustomDefaultBranchConfigurationControlNotifications_Info()
		{
			using (var customDefaultBranchConfigurationControl = new CustomDefaultBranchConfigurationControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)customDefaultBranchConfigurationControl;
				notify.AddInformation("This is info.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Notification message is correct.", "This is info.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCustomDefaultBranchConfigurationControlNotifications_Warning()
		{
			using (var customDefaultBranchConfigurationControl = new CustomDefaultBranchConfigurationControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var notify = (INotifications)customDefaultBranchConfigurationControl;
				notify.AddWarning("This is warning.");

				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Notification message is correct.", "This is warning.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
