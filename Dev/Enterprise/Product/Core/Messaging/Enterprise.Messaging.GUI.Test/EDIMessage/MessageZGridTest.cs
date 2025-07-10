using System.Data;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI.Testing
{
	sealed class MessageZGridTest : TestCaseWithDummy
	{
		public void TestInerchangeMenu()
		{
			using (MessageZGrid grid = new MessageZGrid())
			{
				MenuItem found = new ZMenuItem();
				found.Text = "Not found yet";
				foreach (MenuItem candidate in grid.ContextMenu.MenuItems)
				{
					if (candidate.Text == "Resend Interchange")
					{
						found = candidate;
					}
				}
				AssertNotEquals("Not found yet", found.Text);
				found.PerformClick();
				AssertEquals("Select an interchange to resend", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestResendInterchangeClick()
		{
			DummyBizoWithEDIMessageCollection dummyBizo = Factory.New<DummyBizoWithEDIMessageCollection>();
			DummyEDIMessage message1 = Factory.New<DummyEDIMessage>();
			dummyBizo.Messages.Add(message1);
			message1.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			AssertEquals("Messages count", 1, dummyBizo.Messages.Count);

			using (ZForm form = new ZForm(dummyBizo))
			{
				EDIMessageUserControl control = new EDIMessageUserControl();
				form.Controls.Add(control);
				control.MessagesGrid.BindTo = "Messages";
				control.SetDataBinding(dummyBizo, "");

				form.Show();

				MenuItem resendInterchangeMenuItem = null;
				foreach (MenuItem item in control.MessagesGrid.ContextMenu.MenuItems)
				{
					if (item.Text == "Resend Interchange")
					{
						resendInterchangeMenuItem = item;
						break;
					}
				}
				AssertNotNull("Context menu contains Resend Interchange Menu Item", resendInterchangeMenuItem);
				Factory.Save();
				resendInterchangeMenuItem.PerformClick();
				AssertEquals("No message selected", "Select an interchange to resend", UnitTestUserNotification.Instance.LastMessage.Text);
				control.MessagesGrid.Select(0);
				Factory.Save();
				resendInterchangeMenuItem.PerformClick();
				AssertEquals("No interchange attached to selected message", "Message 111 has never been sent.  Please verify the service tasks are running", UnitTestUserNotification.Instance.LastMessage.Text);
				EDIInterchange interchange1 = Factory.New<EDIInterchange>();
				interchange1.EI_InterchangeNum = "222";
				message1.EM_EI = interchange1.PK;
				message1.EM_ReceiveTransmit = "";
				Factory.Save();
				resendInterchangeMenuItem.PerformClick();
				AssertEquals("Invalide direction", "Message 111 in Interchange 222 is not an outbound message.  You may only resend outbound messages", UnitTestUserNotification.Instance.LastMessage.Text);
				message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				Factory.Save();
				resendInterchangeMenuItem.PerformClick();
				AssertEquals("Inbound message", "Message 111 in Interchange 222 is not an outbound message.  You may only resend outbound messages", UnitTestUserNotification.Instance.LastMessage.Text);
				message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				interchange1.EI_Status = EDIInterchange.Status.Queued;
				Factory.Save();
				resendInterchangeMenuItem.PerformClick();
				AssertEquals("Queued message", "Message 111 in Interchange 222 is waiting to be sent or has just been sent.  Please verify the service tasks are running", UnitTestUserNotification.Instance.LastMessage.Text);
				interchange1.EI_Status = EDIInterchange.Status.Acknowledged;
				Factory.Save();
				resendInterchangeMenuItem.PerformClick();
				AssertEquals("Acknowledged message", "Message 111 in Interchange 222 has been acknowledged.  Are you sure you want to resend it?", UnitTestUserNotification.Instance.LastMessage.Text);
				interchange1.EI_Status = EDIInterchange.Status.Sent;
				Factory.Save();
				resendInterchangeMenuItem.PerformClick();
				AssertEquals("Sent message", "Message 111 in Interchange 222 has not been acknowledged.  Do you wish to resend it?", UnitTestUserNotification.Instance.LastMessage.Text);
				interchange1.EI_Status = EDIInterchange.Status.Failed;
				Factory.Save();
				resendInterchangeMenuItem.PerformClick();
				AssertEquals("Failed message", "Message 111 in Interchange 222 has not been acknowledged.  Do you wish to resend it?", UnitTestUserNotification.Instance.LastMessage.Text);
				interchange1.EI_Status = EDIInterchange.Status.Error;
				Factory.Save();
				resendInterchangeMenuItem.PerformClick();
				AssertEquals("Error message", "Message 111 in Interchange 222 has not been acknowledged.  Do you wish to resend it?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region TestHelper
		class DummyBizoWithEDIMessageCollection : DummyBusinessObject, IEDIMessageCollectionProvider
		{
			public DummyBizoWithEDIMessageCollection(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			EDIMessageCollection messages;
			public EDIMessageCollection Messages
			{
				get
				{
					if (messages == null)
					{
						messages = new EDIMessageCollection(this, Factory);
						messages.Load();
					}
					return messages;
				}
			}
		}
		class DummyEDIMessage : EDIMessage
		{
			public DummyEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
			protected override string GetMessageReferenceNumber()
			{
				return "111";
			}
		}
		#endregion
	}
}
