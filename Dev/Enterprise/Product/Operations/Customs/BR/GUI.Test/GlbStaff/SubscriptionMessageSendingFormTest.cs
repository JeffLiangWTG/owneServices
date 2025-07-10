using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(SubscriptionMessageSendingForm))]
	public class SubscriptionMessageSendingFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XX";
			staff.GS_LoginName = "test";

			var wrapper = BRGlbStaffWrapper.Get(staff);
			var subscriptionSendingObjectParent = new SubscriptionMessageSendingObjectParent(wrapper);
			return new SubscriptionMessageSendingForm(subscriptionSendingObjectParent);
		}

		public void TestSubscriptionMessageSendingGrid()
		{
			using (var subscriptionTestForm = (SubscriptionMessageSendingForm)GetFormToBashCore())
			{
				subscriptionTestForm.Show();
				var grid = subscriptionTestForm.FindSingle<ZGrid>("MessageSendingObjectsGrid");

				CombineAssertions(() =>
				{
					AssertEquals("Count", 5, grid.ColumnStyles.Count);
					AssertEquals("Send?", "ShouldSend", ((ZCheckBoxColumnStyleInfo)grid.ColumnStyles[0]).ColumnName);
					AssertEquals("MessageType", SubscriptionMessageSendingObject.Schema.MessageType, ((ZDropEditColumnStyleInfo)grid.ColumnStyles[1]).ColumnName);
					AssertEquals("EventId", SubscriptionMessageSendingObject.Schema.EventId, ((ZTextBoxColumnStyleInfo)grid.ColumnStyles[2]).ColumnName);
					AssertEquals("SubmittedDate", SubscriptionMessageSendingObject.Schema.SubmittedDate, ((ZDateEditColumnStyleInfo)grid.ColumnStyles[3]).ColumnName);
					AssertEquals("Status", SubscriptionMessageSendingObject.Schema.Status, ((ZDropEditColumnStyleInfo)grid.ColumnStyles[4]).ColumnName);
				});
			}
		}

		public void TestSendSubscription_ClickCheckIsOKToSendXtTest()
		{
			var staff = Factory.New<GlbStaff>();
			var staffWrapper = BRGlbStaffWrapper.Get(staff);
			staffWrapper.EventSubscriptions.AddNew();
			staffWrapper.CCTPassword.GP_ExpiryDate = ZDateTime.Now.AddDays(100);
			var password = staffWrapper.CCTPassword;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var messageSendingObjectParent = new SubscriptionMessageSendingObjectParent(staffWrapper);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			using (var form = new SubscriptionMessageSendingForm(messageSendingObjectParent))
			{
				form.Show();
				form.MessageSendingObjectParent.SendingObjectsCollection.Cast<SubscriptionMessageSendingObject>().ForEach(x => x.ShouldSend = true);

				var sendButton = form.FindSingle<ZButton>("SendButton");
				sendButton.PerformClick();

				AssertEquals("The selected messages will be sent to a test environment!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendSubscription_ClickCheckIsOKToSendXtXtProduction()
		{
			var staff = Factory.New<GlbStaff>();
			var staffWrapper = BRGlbStaffWrapper.Get(staff);
			staffWrapper.EventSubscriptions.AddNew();
			staffWrapper.CCTPassword.GP_ExpiryDate = ZDateTime.Now.AddDays(100);
			var password = staffWrapper.CCTPassword;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			var messageSendingObjectParent = new SubscriptionMessageSendingObjectParent(staffWrapper);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			using (var form = new SubscriptionMessageSendingForm(messageSendingObjectParent))
			{
				form.Show();
				form.MessageSendingObjectParent.SendingObjectsCollection.Cast<SubscriptionMessageSendingObject>().ForEach(x => x.ShouldSend = true);

				var sendButton = form.FindSingle<ZButton>("SendButton");
				sendButton.PerformClick();

				Assert(!UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).Contains("The selected messages will be sent to a test environment!"));
			}
		}
	}
}
