using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Moq;

namespace Enterprise.eHubMessaging.Tests
{
	public class MessageActionTest : TestCaseWithFactory
	{
		public void TestNewMessageAction()
		{
			var factoryProvider = new BusinessObjectFactoryProvider();

			AssertNull(MessageAction.New(factoryProvider, "BLAH", "BLAH"));

			var messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.AgencyBillsOfLading);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.Freight.Agency.DataTransfer.AgencyBillOfLadingMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.Consols);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.ServiceManager.Tasks.XMLAutomation.ForwardingConsolMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.ContainerMovements);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.Freight.Agency.DataTransfer.ContainerMovementMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.Events);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.ServiceManager.Tasks.XMLAutomation.EventMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.FinancialTransactions);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.Accounting.DataTransfer.FinancialInvoiceMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.Orders);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.ServiceManager.Tasks.XMLAutomation.OrderMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.Products);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.DataTransfer.ProductMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.Shipments);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.ServiceManager.Tasks.XMLAutomation.ForwardingShipmentMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.WhsDockets);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.Warehouse.Transactions.DataTransfer.WhsDocketMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.Brokerage);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.Customs.DataTransfer.DeclarationMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.Invoices);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.Customs.DataTransfer.InvoiceMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.LocalCartageBooking);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.Freight.LocalCartage.DataTransfer.CartageBookingMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.LocalCartageStatus);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.Freight.LocalCartage.DataTransfer.CartageStatusMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.CFSLoadList);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.Freight.CFS.Business.CFSLoadListMessageAction", messageAction.GetType().ToString());

			messageAction = MessageAction.New(factoryProvider, EDIMessageTypeList.Codes.XMS, EDIMessageSubTypeList.Codes.Organizations);
			AssertNotNull(messageAction);
			AssertEquals("Enterprise.ServiceManager.Tasks.XMLAutomation.OrganizationMessageAction", messageAction.GetType().ToString());
		}

		public void TestExecuteAction()
		{
			var message1 = EDIMessageTestFactory.New(Factory);
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.AgencyBillsOfLading;
			var message2 = EDIMessageTestFactory.New(Factory);
			message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.Consols;
			var message3 = EDIMessageTestFactory.New(Factory);
			message3.EM_MessageSubType = EDIMessageSubTypeList.Codes.Orders;

			var notifications = new Mock<INotifications>(MockBehavior.Strict);
			notifications.Setup(m => m.Add(It.IsAny<INotification>()))
				.Callback(new NotificationDelegate((INotification notification) =>
			{
				Assert(notification.GetType().IsAssignableFrom(typeof(ErrorNotification)));
				Assert(notification.Message.StartsWith("Error: Could not execute Message Action"));
			}));

			var factoryProvider = new BusinessObjectFactoryProvider(Factory);
			var messageAction = new Mock<MessageAction>(factoryProvider) { CallBase = true };
			var participants = new List<ITransactionParticipant>();
			messageAction.SetupSequence(m => m.ExecuteActionCore(It.IsAny<EDIMessage>(), It.IsAny<INotifications>(), out participants))
				.Returns(true).Returns(false).Throws(new Exception());

			Assert(messageAction.Object.ExecuteAction(message1, notifications.Object, out participants));
			Assert(!messageAction.Object.ExecuteAction(message2, notifications.Object, out participants));
			Assert(!messageAction.Object.ExecuteAction(message3, notifications.Object, out participants));

			notifications.VerifyAll();
			messageAction.VerifyAll();
		}

		public void TestSendNotificationEmail()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "GRP";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "~TT";
			staff.GS_EmailAddress = "blah@blah.com";
			Factory.Save();

			var notifications = new Mock<INotifications>();
			var messageAction = new Mock<MessageAction>(factoryProvider) { CallBase = true };
			messageAction.Setup(m => m.NotificationGroup).Returns(group);

			string subject = "Test email subject";
			string body = "Test email body";

			bool onSuccess = false;
			messageAction.Object.SendNotificationEmail(subject, body, notifications.Object, onSuccess);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(subject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals(body, Env.OutgoingMailManager.EmailsCreated[0].Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			onSuccess = true;
			messageAction.Object.SendNotificationEmail(subject, body, notifications.Object, onSuccess);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(subject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals(body, Env.OutgoingMailManager.EmailsCreated[0].Body);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			onSuccess = true;
			messageAction.Object.SendNotificationEmail(subject, body, notifications.Object, onSuccess);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			notifications.VerifyAll();
			messageAction.VerifyAll();
		}

		public void TestSendNotificationEmailFailDueToNoValidGroup()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);

			var messageAction = new Mock<MessageAction>(factoryProvider) { CallBase = true };
			messageAction.Setup(m => m.NotificationGroup).Returns((Integration.IGlbGroup)null);
			messageAction.Setup(m => m.NotificationGroupRegistryPath).Returns(ZString.Empty);

			var notifications = new Mock<INotifications>();
			notifications.Setup(m => m.Add(It.IsAny<INotification>()))
				.Callback(new NotificationDelegate((INotification notification) =>
			{
				Assert(notification.GetType().IsAssignableFrom(typeof(WarningNotification)));
				Assert(notification.Message.Contains("Notification group is not specified or invalid. Please check"));
			}));

			messageAction.Object.SendNotificationEmail("", "", notifications.Object, false);

			messageAction.VerifyAll();
			notifications.VerifyAll();
		}

		public void TestSendNotificationEmailFailDueToCreateMailException()
		{
			string sendEmailFailString = "Send email fail.";
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "GRP";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "~TT";
			staff.GS_EmailAddress = "blah@blah.com";
			Factory.Save();

			var messageAction = new Mock<MessageAction>(factoryProvider) { CallBase = true };
			messageAction.Setup(m => m.NotificationGroup).Returns(group);
			messageAction.Setup(m => m.CreateEmail(It.IsAny<EmailDef>())).Throws(new EmailSendFailedException(sendEmailFailString));

			var notifications = new Mock<INotifications>();
			notifications.Setup(m => m.Add(It.IsAny<INotification>()))
				.Callback(new NotificationDelegate((INotification notification) =>
			{
				Assert(notification.GetType().IsAssignableFrom(typeof(WarningNotification)));
				Assert(notification.Message.Contains("Notification could not be sent: " + sendEmailFailString));
			}));

			messageAction.Object.SendNotificationEmail("", "", notifications.Object, false);

			messageAction.VerifyAll();
			notifications.VerifyAll();
		}

		#region Implementation

		protected delegate void NotificationDelegate(INotification notification);

		#endregion
	}
}
