using System;
using System.Data;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.EConversation.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.EConversation.Testing.GUI
{
	sealed class EConversationPluginTest : TestCaseWithFactory
	{
		public void TestShouldPlugInGUIAndBusinessEntityBeCreatedCore_ReturnsFalseWhenParentIsNotInDb()
		{
			var host = Factory.NewWithValidTestData<DummyConversationProvider>();
			using (var plugin = new EConversationPlugin(host.ParentModule, host))
			{
				Assert("Shouldnt be allowed to create stuff - parent is not saved", !ShouldPlugInGUIAndBusinessEntityBeCreated(plugin));
				AssertEquals("Should tell the user why", "The form must be saved and this tab reloaded before an eConversation can be started.", plugin.PlugInNotDisplayedMessage);

				Factory.Save();
				Assert("Should be allowed to create stuff since parent is now saved", ShouldPlugInGUIAndBusinessEntityBeCreated(plugin));
			}
		}

		public void TestAddInternalLog_WhenNewWorkItemUnsaved()
		{
			var host = Factory.NewWithValidTestData<DummyConversationProviderForTest>();
			using (var plugin = new EConversationPlugin(host.ParentModule, host))
			{
				UnitTestUserNotification.Instance.ClearMessages();
				var method = plugin.GetType().GetMethod("AddInternalLog_Click", BindingFlags.NonPublic | BindingFlags.Instance);
				method.Invoke(plugin, new object[] { null, null });

				UnitTestUserNotification.Instance.ClearMessages();
				Factory.Save();
				method.Invoke(plugin, new object[] { null, null });

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddInternalLog_NullConversationProvide()
		{
			var host = Factory.NewWithValidTestData<DummyNullConversationProviderForTest>();
			Factory.Save();
			using (var plugin = new EConversationPlugin(host.ParentModule, host))
			{
				ErrorReporter.Instance.Clear();
				UnitTestUserNotification.Instance.ClearMessages();
				var method = plugin.GetType().GetMethod("AddInternalLog_Click", BindingFlags.NonPublic | BindingFlags.Instance);
				method.Invoke(plugin, new object[] { null, null });
				AssertEquals("The eConversation is unavailable.",UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("The conversation provider does not have a conversation object.",ErrorReporter.LastMessageReported);

				UnitTestUserNotification.Instance.ClearMessages();
				host.ShouldCreateConversation = true;
				method.Invoke(plugin, new object[] { null, null });
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				ErrorReporter.Instance.Clear();
			}
		}

		bool ShouldPlugInGUIAndBusinessEntityBeCreated(ZPlugIn plugin)
		{
			var method = plugin.GetType().GetMethod("ShouldPlugInGUIAndBusinessEntityBeCreated", BindingFlags.NonPublic | BindingFlags.Instance);

			return (bool)method.Invoke(plugin, null);
		}

		public void TestUnsentMessagesWarningOnSave()
		{
			var bizo = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			using (var form = new ZFormForPlugInTest(bizo))
			{
				form.Show();
				var plugin = (EConversationPlugin)form.PlugIns.Instances[0];
				var eConvMessageTextBox = ((IConversationView)plugin.UserControl).MessageTextBox as ZAutoCompleteTextBox;

				AssertEquals("", eConvMessageTextBox.Text);
				AssertEquals("Next message should be empty", ZBlob.Empty, bizo.eConversation.NextMessage);

				plugin.ShowPreSaveDialogsCore();
				AssertNotEquals("Unsent message warning should not be triggered", "You have unsent eConversation messages. Continue with save?", UnitTestUserNotification.Instance.LastMessage.Text);

				eConvMessageTextBox.Focus();
				eConvMessageTextBox.Text = "some text";
				var button1 = ((IConversationView)plugin.UserControl).SendButton;
				button1.Focus();
				AssertEquals("some text", eConvMessageTextBox.Text);
				AssertNotEquals("RTF blob is not empty.", ZBlob.Empty, bizo.eConversation.NextMessage);
				plugin.ShowPreSaveDialogsCore();
				AssertEquals("You have unsent eConversation messages. Continue with save?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				eConvMessageTextBox.Focus();
				eConvMessageTextBox.Text = "";
				var button2 = ((IConversationView)plugin.UserControl).BroadcastButton;
				button2.Focus();
				AssertEquals("there is no text.", "", eConvMessageTextBox.Text);
				AssertNotEquals("it's not empty for rtf blob any more if any text has been inputted even removed.", ZBlob.Empty, bizo.eConversation.NextMessage);
				plugin.ShowPreSaveDialogsCore();
				AssertNotEquals("You have unsent eConversation messages. Continue with save?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				form.SuppressDefaultPreSaveMessageNotification = true;
				eConvMessageTextBox.Focus();
				eConvMessageTextBox.Text = "some text";
				var sendButton = ((IConversationView)plugin.UserControl).SendButton;
				sendButton.Focus();
				AssertEquals("some text", eConvMessageTextBox.Text);
				AssertNotEquals("RTF blob is not empty.", ZBlob.Empty, bizo.eConversation.NextMessage);
				plugin.ShowPreSaveDialogsCore();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		/*

		This test relies on having a controller that uses the plugin, of which there is currently none. Once an implementation is completed we can uncomment this test.

		public void TestAddsToActionMenu()
		{
			var controllerIdWhoHasImplementedPlugin = ControllerIDs.JobShipment;
			var controller = ZControllerFactory.Create(controllerIdWhoHasImplementedPlugin);

			var bizo = Factory.NewWithValidTestData(controller.TypeOfTopLevelBusinessObject);
			Factory.Save();
			
			using (var form = controller.ShowEditForm(bizo))
			{
				Application.DoEvents();

				var actionMenuItems = ((IFileMenuItemsProvider)form).ActionsMenuItem.MenuItems;

				AssertNotNull("Should have the 'Add Internal Message' menu item", actionMenuItems.FindByText("Add Internal Message"));
			}
		}

		*/

		class DummyConversationProviderForTest : DummyConversationProvider
		{
			public DummyConversationProviderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			public override JobConversation eConversation
			{
				get
				{
					if (IsInDatabase)
					{
						return base.eConversation;
					}
					return null;
				}
			}
		}

		class DummyNullConversationProviderForTest : DummyConversationProvider
		{
			public bool ShouldCreateConversation;

			public DummyNullConversationProviderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
			public override JobConversation eConversation => ShouldCreateConversation ? base.eConversation : null;
		}

		[ZArchitecture.GUI.Testing.TestExcludeZWinFormsAllHaveFormBashers]
		[ZArchitecture.GUI.Testing.TestExcludeZWinFormHasTypedConstructor]
		public class ZFormForPlugInTest : ZForm
		{
			public ZFormForPlugInTest(IBusiness businessEntity)
				: base(businessEntity)
			{
				PlugIns.Add(ControllerIDs.eConversationPlugIn);
			}

			public void ExposeAllTabPages()
			{
				ExposeAllTabPages(this);
			}

			FunctionalitySuspender PlugInSuspender
			{
				get
				{
					var plugin = PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn) as EConversationPlugin;
					var suspender = typeof(EConversationPlugin).GetField("DefaultPreSaveMessageNotificationSuspender", BindingFlags.NonPublic | BindingFlags.Instance);
					return suspender.GetValue(plugin) as FunctionalitySuspender;
				}
			}

			public bool SuppressDefaultPreSaveMessageNotification
			{
				get
				{
					return PlugInSuspender.IsSuspended;
				}
				set
				{
					if (value)
					{
						if (!PlugInSuspender.IsSuspended)
						{
							currentSuspender = PlugInSuspender.GetSuspender();
						}
					}
					else
					{
						currentSuspender.Dispose();
					}
				}
			}

			IDisposable currentSuspender;

			void ExposeAllTabPages(Control ctrl)
			{
				foreach (Control nextCtrl in ctrl.Controls)
				{
					ZTabControl tabControl = nextCtrl as ZTabControl;
					if (tabControl != null)
					{
						foreach (ZTabPage page in tabControl.TabPages)
						{
							tabControl.SelectedTab = page;
							Application.DoEvents();
						}
					}

					ExposeAllTabPages(nextCtrl);
				}
			}

			protected override ZTabControl TopLevelTabControl
			{
				get { return TabControl; }
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.TabControl = new ZTabControl();
				this.TabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
				this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 12, true);
				this.TabControl.Name = "TabControl";
				this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(524, 584, true);
				this.TabControl.TabIndex = 7;

				this.Controls.Add(this.TabControl);
			}

			public ZTabControl TabControl;

			public override ContinueWithSave FireSaveButton(object sender = null)
			{
				return ForceSaveToFail ? ContinueWithSave.No : base.FireSaveButton(sender);
			}

			public bool ForceSaveToFail { get; set; }
		}
	}
}
