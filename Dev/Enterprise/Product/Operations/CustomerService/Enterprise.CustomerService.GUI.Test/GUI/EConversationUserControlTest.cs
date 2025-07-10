using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.EConversation.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CustomerService.GUI.Testing
{
	class EConversationUserControlTest : TestCaseWithFactory
	{
		public void TestLimitMessages()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var bizo = Factory.New<DummyEnterpriseBusinessObject>();
			var eConversation = new Business.EConversation(bizo, 10);

			using (var form = new ZForm(eConversation))
			using (var control = new EConversationMessageListUserControl())
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				var controls = ((Control)form).SelectRecursive(f => f.Controls.Cast<Control>()).OfType<ConversationMessageUserControl>().ToArray();
				AssertEquals(0, controls.Length);

				for (var i = 0; i < 15; i++)
				{
					eConversation.AddMessageFromLocalUser("Bork bork bork bork. I'm a chicken.");
					control.RefreshMessages();
					Application.DoEvents();
				}

				controls = ((Control)form).SelectRecursive(f => f.Controls.Cast<Control>()).OfType<ConversationMessageUserControl>().ToArray();
				AssertEquals("There shouldn't be 15 controls because they number should be limited", 10, controls.Length);
			}
		}

		public void TestMessageWithInvalidZDate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var bizo = Factory.New<DummyEnterpriseBusinessObject>();
			var eConversation = new Business.EConversation(bizo, 10);

			using (var form = new ZForm(eConversation))
			{
				using (var control = new EConversationMessageListUserControl())
				{
					using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
					{
						form.Controls.Add(control);
						form.Show();
						Application.DoEvents();

						var controls = ((Control)form).SelectRecursive(f => f.Controls.Cast<Control>()).OfType<ConversationMessageUserControl>().ToArray();
						AssertEquals(0, controls.Length);

						eConversation.AddMessage(Guid.NewGuid(), ZDateTime.Empty, Env.CurrentUser.Initials, Env.CurrentUser.FullName, "I'm still a chicken.", "");
						control.RefreshMessages();
						Application.DoEvents();

						controls = ((Control)form).SelectRecursive(f => f.Controls.Cast<Control>()).OfType<ConversationMessageUserControl>().ToArray();
						AssertEquals("There should still be 1 control even though the message has an invalid date", 1, controls.Length);
					}
				}
			}
		}
	}
}
