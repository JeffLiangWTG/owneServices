using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common.Enumeration;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.SpellCheck.GUI;
using CargoWise.Tools.SpellCheck.TestFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.Test;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class IncidentConversationViewControllerTest : TestCaseWithFactory
	{
		public void TestClearingMessage()
		{
			var helper = new SupportIncidentTestHelper(Factory);
			var incident = helper.CreateIncidentWithLicencedContact("ENT");
			AssertNotNull(incident.Request);
			Factory.Save();
			var conversation = incident.EConversation.Conversation;
			using (var form = new SupportIncidentForm(incident))
			{
				var controller = new IncidentConversationViewController();
				controller.Initialize(form);
				form.Show();
				SelectEConversation(form);
				Application.DoEvents();
				var tabControl = (IncidentConversationTabControl)form.TopLevelTabControl_Exposed.SelectedTab.Controls.Find("EConversationFullControl", true).Single();
				var textbox = (TextBoxBase)tabControl.Controls.Find("econversationMessageTextBox", true).Single();
				textbox.Focus();
				textbox.Text = "hello world";
				Application.DoEvents();
				PushBinding(textbox);
				Application.DoEvents();
				AssertContains("PRE: Binding is updated", "hello world", Encoding.UTF8.GetString(conversation.NextMessage));
				UnitTestUserNotification.Instance.AddYesAnswer();
				ClickSend(tabControl);
				Application.DoEvents();
				form.FireSaveButton();
				Application.DoEvents();
				CombineAssertions("When a message is sent the textbox and bound property should be cleared", () =>
				{
					AssertNotContains("The bound property is cleared", "hello world", Encoding.UTF8.GetString(conversation.NextMessage));
					AssertEquals("The textbox is cleared", string.Empty, textbox.Text);
				});
			}
		}

		void ClickSend(IncidentConversationTabControl tab) => ((ZButton)tab.Controls.Find("SendButton", true).Single()).PerformClick();
		void SelectEConversation(SupportIncidentForm form)
		{
			var tabControl = form.TopLevelTabControl_Exposed;
			var tabPage = tabControl.TabPages.Cast<ZTabPage>().First(t => t.Name == "eConversationTabPage");
			tabControl.SelectedTab = tabPage;
		}

		void PushBinding(Control control)
		{
			var bindings = ZEnumerable.Iterate(control, c => c.Parent, null).SelectMany(c => c.DataBindings.Cast<Binding>()).Where(b => b.Control == control);
			foreach (var binding in bindings)
			{
				binding.BindingManagerBase.EndCurrentEdit();
			}
		}

		public void TestSendMessage_DontWarn()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var helper = new SupportIncidentTestHelper(Factory);
			var incident = helper.CreateIncidentWithLicencedContact("ENT");
			var staffParticipant1 = incident.EConversation.ExistingConversation.Participants.AddNew();
			staffParticipant1.JCP_ParticipantTableCode = staff1.TablePrefix;
			staffParticipant1.JCP_ParticipantID = staff1.PK;
			staffParticipant1.JCP_IsSubscribed = true;
			var staffParticipant2 = incident.EConversation.ExistingConversation.Participants.AddNew();
			staffParticipant2.JCP_ParticipantTableCode = staff2.TablePrefix;
			staffParticipant2.JCP_ParticipantID = staff2.PK;
			staffParticipant2.JCP_IsSubscribed = false;
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Application.DoEvents();

				var controller = new IncidentConversationViewController(false);
				controller.Initialize(form);
				int callsToCountChanged = 0;
				string textDuringFirstCountChanged = null;
				incident.EConversation.MessageCountChanged += (sender, e) =>
				{
					++callsToCountChanged;
					if (textDuringFirstCountChanged == null)
					{
						textDuringFirstCountChanged = controller.TextBoxForTest.Text;
					}
				};
				controller.TextBoxForTest.Text = "12345";
				controller.SendMessage();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("12345", incident.EConversation.GetNewLocalPublishedMessages().First().Body);
				AssertEquals("PRE", true, callsToCountChanged > 0);
				AssertEquals("text box is cleared before message added to incident to prevent recursion", "", textDuringFirstCountChanged);
			}
		}

		public void TestSendMessage_Yes()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var helper = new SupportIncidentTestHelper(Factory);
			var incident = helper.CreateIncidentWithLicencedContact("ENT");
			var staffParticipant1 = incident.EConversation.ExistingConversation.Participants.AddNew();
			staffParticipant1.JCP_ParticipantTableCode = staff1.TablePrefix;
			staffParticipant1.JCP_ParticipantID = staff1.PK;
			staffParticipant1.JCP_IsSubscribed = true;
			var staffParticipant2 = incident.EConversation.ExistingConversation.Participants.AddNew();
			staffParticipant2.JCP_ParticipantTableCode = staff2.TablePrefix;
			staffParticipant2.JCP_ParticipantID = staff2.PK;
			staffParticipant2.JCP_IsSubscribed = false;
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Application.DoEvents();

				var controller = new IncidentConversationViewController(true);
				controller.Initialize(form);
				int callsToCountChanged = 0;
				string textDuringFirstCountChanged = null;
				incident.EConversation.MessageCountChanged += (sender, e) =>
				{
					++callsToCountChanged;
					if (textDuringFirstCountChanged == null)
					{
						textDuringFirstCountChanged = controller.TextBoxForTest.Text;
					}
				};
				controller.TextBoxForTest.Text = "12345";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				controller.SendMessage();
				AssertEquals("You are about to send this message to all subscribed recipients and the customer. If you want to send the message to internal staff only, please click No and add an Internal Log from the Action Menu. Do you want to continue to send this message?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("12345", incident.EConversation.GetNewLocalPublishedMessages().First().Body);
				AssertEquals("PRE", true, callsToCountChanged > 0);
				AssertEquals("text box is cleared before message added to incident to prevent recursion", "", textDuringFirstCountChanged);
			}
		}

		public void TestSendMessage_No()
		{
			var helper = new SupportIncidentTestHelper(Factory);
			var incident = helper.CreateIncidentWithLicencedContact("ENT");
			using (var form = new SupportIncidentForm(incident))
			{
				var controller = new IncidentConversationViewController(true);
				controller.Initialize(form);
				int callsToCountChanged = 0;
				string textDuringFirstCountChanged = null;
				incident.EConversation.MessageCountChanged += (sender, e) =>
				{
					++callsToCountChanged;
					if (textDuringFirstCountChanged == null)
					{
						textDuringFirstCountChanged = controller.TextBoxForTest.Text;
					}
				};
				controller.TextBoxForTest.Text = "12345";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				controller.SendMessage();
				AssertEquals("message not sent", true, callsToCountChanged == 0);
			}
		}

		public void TestTruncateMessage()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var helper = new SupportIncidentTestHelper(Factory);
			var incident = helper.CreateIncidentWithLicencedContact("ENT");
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				form.ConversationMessageTextBox.Text = new string('0', 10000 + 100);
				AssertEquals("The message has been truncated to 10000 characters.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(10000, form.ConversationMessageTextBox.Text.Length);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestCheckSpelling()
		{
			var incident = Factory.New<SupportIncident>();
			Factory.Save();
			var controller = new IncidentConversationViewController(false);
			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Application.DoEvents();

				controller.Initialize(form);
				controller.TextBoxForTest.Text = "helloooo world";
				AssertEquals("helloooo world", controller.TextBoxForTest.Text);
				using (var spellCheckTester = new SpellCheckFormTestHelper((x) => new SpellCheckFormAction(SpellCheckerFormResult.ChangeAll, null)))
				{
					controller.AwaitingResponse();
				}
				AssertEquals("hello world", incident.EConversation.GetNewLocalPublishedMessages().First().Body);
			}
		}

		public void TestCheckSpellingIgnoreKnownNames()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Brruce";

			var incident = Factory.New<SupportIncident>();
			incident.EConversation.Conversation.Staff.AddNewParticipant(staff);

			Factory.Save();

			var text = $"hello {staff.GS_FullName}";

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				Application.DoEvents();

				var controller = new IncidentConversationViewController();
				controller.Initialize(form);
				controller.TextBoxForTest.Text = text;

				using (var spellCheckTester = new SpellCheckFormTestHelper((x) => new SpellCheckFormAction(SpellCheckerFormResult.ChangeAll, null)))
				{
					controller.AwaitingResponse();
					AssertEquals((ZString)text, incident.EConversation.GetNewLocalPublishedMessages().First().Body);
				}
			}
		}

		public void TestAwaitingResponse_NoCheckSpelling()
		{
			var incident = Factory.New<SupportIncident>();
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				var controller = new IncidentConversationViewController(false);
				controller.Initialize(form);
				var eConvPlugin = form.PlugIns.Instances.OfType<IncidentConversationPlugin>().Single();
				controller.Initialize(eConvPlugin.UserControl as IncidentConversationTabControl);
				form.Controls.Add(eConvPlugin.UserControl);
				controller.TextBoxForTest.Text = "helloooo world";

				AssertNoExceptionThrown("Should not throw exception if no spellchecker specified", () => controller.AwaitingResponse());
				AssertEquals("helloooo world", incident.EConversation.GetNewLocalPublishedMessages().First().Body);
			}
		}

		public void TestAwaitingResponse_WhenCR7()
		{
			var incident = Factory.New<SupportIncident>(); 
			incident.IM_Priority = "CR7";
			Factory.Save();

			using (var form = new SupportIncidentForm(incident))
			{
				var controller = new IncidentConversationViewController();
				controller.Initialize(form);

				var eConvPlugin = form.PlugIns.Instances.OfType<IncidentConversationPlugin>().Single();
				form.Controls.Add(eConvPlugin.UserControl);
				form.Show();
				controller.TextBoxForTest.Text = "hello world";

				AssertEquals(3, controller.AwaitingCustomerResponseMenuStripForTest.Items.Count);
				controller.AwaitingCustomerResponseMenuStripForTest.Items[0].PerformClick();
				AssertEquals("Should send system message", true, incident.EConversation.GetNewLocalPublishedMessages().Any(x => x.Body == "Development Estimate Provided - Awaiting Customer"));
			}
		}

		public void TestShouldSentAwaitingClientResponseSystemMessage_WhenAwaitingCustomerResponseButtonIsClicked()
		{
			var incident = Factory.New<SupportIncident>();
			incident.IM_Priority = "CR7";
			incident.IM_ResolutionCode = SupportIncidentLookups.DispositionList.Constants.AwaitingDevelopmentEstimate;
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				var controller = new IncidentConversationViewController(false);
				controller.Initialize(form);
				var eConvPlugin = form.PlugIns.Instances.OfType<IncidentConversationPlugin>().Single();
				controller.Initialize(eConvPlugin.UserControl as IncidentConversationTabControl);
				form.Controls.Add(eConvPlugin.UserControl);
				controller.TextBoxForTest.Text = "hello world";
				controller.AwaitingResponse();
				AssertEquals("Awaiting Client Response", incident.EConversation.GetNewLocalPublishedMessages().Last().Body);
			}
		}

		public void TestAwaitingResponse_MessageOverride()
		{
			var incident = Factory.New<SupportIncident>();
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				var controller = new IncidentConversationViewController(false);
				controller.Initialize(form);
				var eConvPlugin = form.PlugIns.Instances.OfType<IncidentConversationPlugin>().Single();
				controller.Initialize(eConvPlugin.UserControl as IncidentConversationTabControl);
				form.Controls.Add(eConvPlugin.UserControl);
				controller.TextBoxForTest.Text = "hello world";

				controller.AwaitingResponse("override");
				AssertEquals("Should send overridden message", "override", incident.EConversation.GetNewLocalPublishedMessages().First().Body);
				controller.TextBoxForTest.Text = "hello world";
				controller.AwaitingResponse();
				AssertEquals("Should send original message", true, incident.EConversation.GetNewLocalPublishedMessages().Any(x => x.Body == "hello world"));
			}
		}

		public void TestAwaitingResponseShouldMakeSuspendTriggerCloseIncidentToTrue()
		{
			var incident = Factory.New<SupportIncident>();
			Factory.Save();
			using (var form = new SupportIncidentForm(incident))
			{
				var controller = new IncidentConversationViewController(false);
				controller.Initialize(form);
				var eConvPlugin = form.PlugIns.Instances.OfType<IncidentConversationPlugin>().Single();
				controller.Initialize(eConvPlugin.UserControl as IncidentConversationTabControl);
				form.Controls.Add(eConvPlugin.UserControl);
				controller.TextBoxForTest.Text = "hello world";

				AssertEquals("SuspendTriggerCloseIncident shoule be false before AwaitingResponse", false, incident.SuspendTriggerCloseIncident);
				controller.AwaitingResponse();
				AssertEquals("Should set SuspendTriggerCloseIncident to true after AwaitingResponse", true, incident.SuspendTriggerCloseIncident);
			}
		}

		public void TestSendMessage_MessageOverride()
		{
			var incident = Factory.New<SupportIncident>();
			Factory.Save();
			var controller = new IncidentConversationViewController(false);
			using (var form = new SupportIncidentForm(incident))
			{
				var eConvPlugin = form.PlugIns.Instances.OfType<IncidentConversationPlugin>().Single();
				controller.Initialize(eConvPlugin.UserControl as IncidentConversationTabControl);
				form.Controls.Add(eConvPlugin.UserControl);
				controller.TextBoxForTest.Text = "hello world";

				controller.SendMessage("override");
				AssertEquals("Should send overridden message", "override", incident.EConversation.GetNewLocalPublishedMessages().First().Body);
				controller.TextBoxForTest.Text = "hello world";
				controller.SendMessage();
				AssertEquals("Should send original message", true, incident.EConversation.GetNewLocalPublishedMessages().Any(x => x.Body == "hello world"));
			}
		}

		public void TestSetAwaitingClientResponse_WhenWithoutSendingMessage()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_Email = "132@123.com";
			var incident = Factory.New<SupportIncident>();
			incident.IM_OH_Client = contact.OC_OH;
			incident.IM_OC_Contact = contact.PK;
			incident.IM_Priority = "CR4";

			var task1 = incident.WorkflowItems.AddNew();
			task1.P9_Description = "Task 1";
			task1.P9_Sequence = 1;
			task1.P9_Type = "UDF";
			task1.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (var form = new SupportIncidentForm(incident))
			{
				form.Show();
				var controller = new IncidentConversationViewController();
				controller.Initialize(form);

				AssertEquals("The default value of AwaitingResponseButton should be true", true, form.AwaitingResponseButton.Enabled);
				var eConvPlugin = form.PlugIns.Instances.OfType<IncidentConversationPlugin>().Single();
				controller.Initialize(eConvPlugin.UserControl as IncidentConversationTabControl);
				form.Controls.Add(eConvPlugin.UserControl);
				controller.AwaitingResponse();
				AssertEquals("IM_ResolutionCode should be CWR", SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse, incident.IM_ResolutionCode);
				AssertEquals("Awaiting Client Response", incident.EConversation.GetNewLocalPublishedMessages().Last().Body);

				Factory.Save();
				AssertEquals("After clicking the AwaitingResponseButton, the visible property should be false", false, form.AwaitingResponseButton.Enabled);
			}
		}
	}
}
