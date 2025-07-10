using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.Business.ESConstants;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	class RequestInboxNotificationsMenuItemCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			using (var provider = new EU.ExitControl.GUI.Testing.ReportsGridUserControlProviderForTesting())
			{
				var requestInboxNotificationsMenuItem = new RequestInboxNotificationsMenuItemCreator(provider);
				var menuItem = requestInboxNotificationsMenuItem.Create();
				AssertEquals("Check for Inbox Notifications", menuItem.Text);
			}
		}

		public void TestRequestInboxNotifications_Click_Validations()
		{
			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = ZString.Empty;
			exitHeader.CXH_CustomsProfile = ZString.Empty;

			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			consignment1.CXC_MovementReference = "AAA";
			var report1 = exitHeader.CusExitReports.AddNew();
			report1.CER_CXC_Consignment = consignment1.PK;
			var consignment2 = exitHeader.CusExitConsignments.AddNew();
			consignment2.CXC_MovementReference = "BBB";
			var report2 = exitHeader.CusExitReports.AddNew();
			report2.CER_CXC_Consignment = consignment2.PK;

			Factory.Save();

			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				var reportsTabUserControl = GetReportsTabUserControl(form);
				var grid = reportsTabUserControl.FindSingle<ZArchitecture.ZGrid>("ReportsGrid");

				var requestInboxNotifEntryMenuItem = grid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications");

				CombineAssertions(() =>
				{
					grid.Select();
					grid.Focus();

					requestInboxNotifEntryMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					reportsTabUserControl.Refresh();
					grid.SelectAllElements();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertEquals("Message informing declarations need broker and certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

					exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
					exitHeader.CXH_CustomsProfile = ZString.Empty;
					reportsTabUserControl.Refresh();
					grid.SelectAllElements();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertEquals("Message informing declarations need broker and certificate declared when broker is declared but certificate is empty", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

					exitHeader.CXH_CustomsProfile = "INVALID";
					reportsTabUserControl.Refresh();
					grid.SelectAllElements();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertEquals("Message informing broker and certificate are needed when broker is declared but certificate declared is invalid", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

					exitHeader.CXH_CustomsProfile = CertificateName;
					reportsTabUserControl.Refresh();
					grid.SelectAllElements();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("User Notification Last Message was cleared", UnitTestUserNotification.Instance.LastMessage.Text);
					exitHeader.Reload();
					reportsTabUserControl.Refresh();
					grid.SelectAllElements();
					requestInboxNotifEntryMenuItem.PerformClick();
					AssertEquals("Message informing broker and certificate are needed when broker is declared but current user has no authorisation for certificate declared, even if we haven't done a new save& validation", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate in the details tab.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestRequestInboxNotifications_Click_OneConsignment_EHub()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CXH_CustomsProfile = CertificateName;

				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_MovementReference = "AAA";
				var report1 = exitHeader.CusExitReports.AddNew();
				report1.CER_CXC_Consignment = consignment1.PK;

				Factory.Save();

				using (var form = new ExitControlForm(exitHeader))
				{
					form.Show();

					ZFormModaliser.ShowDialogsInTest = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var reportsTabUserControl = GetReportsTabUserControl(form);
					var grid = reportsTabUserControl.FindSingle<ZArchitecture.ZGrid>("ReportsGrid");

					var requestInboxNotifEntryMenuItem = grid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications");

					exitHeader.HasChanges = true;
					reportsTabUserControl.Refresh();
					grid.SelectAllElements();

					CombineAssertions(() =>
					{
						CheckFactoryHasNoPendingChanges("Before sending inbox notification request");
						requestInboxNotifEntryMenuItem.PerformClick();
						CheckFactoryHasNoPendingChanges("After sending inbox notification request");

						AssertEquals("Should have message asking to save the declaration before sending", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

						AssertEquals("Inbox Notification Requests were sent correctly", "2 In-box Notification requests created", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertNewRequestEDIMessagesCreated(report1, inboxTypes);
					});
				}
			}
		}

		public void TestRequestInboxNotifications_Click_OneConsignment_xT()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var mrnCode = "AAA";

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CXH_CustomsProfile = CertificateName;

				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_FullName = DeclarantName;
				carrier.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_Code = "MYADDRESS";
				address.OA_OH = carrier.PK;
				exitHeader.CXH_OA_Carrier = address.PK;

				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_MovementReference = mrnCode;
				var report1 = exitHeader.CusExitReports.AddNew();
				report1.CER_CXC_Consignment = consignment1.PK;

				Factory.Save();

				using (var form = new ExitControlForm(exitHeader))
				{
					form.Show();

					ZFormModaliser.ShowDialogsInTest = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var reportsTabUserControl = GetReportsTabUserControl(form);
					var grid = reportsTabUserControl.FindSingle<ZArchitecture.ZGrid>("ReportsGrid");

					var requestInboxNotifEntryMenuItem = grid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications");

					exitHeader.HasChanges = true;
					reportsTabUserControl.Refresh();
					grid.SelectAllElements();

					CombineAssertions(() =>
					{
						CheckFactoryHasNoPendingChanges("Before sending inbox notification request");
						requestInboxNotifEntryMenuItem.PerformClick();
						CheckFactoryHasNoPendingChanges("After sending inbox notification request");

						AssertEquals("Should have message asking to save the declaration before sending", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

						AssertEquals("Inbox Notification Requests were sent correctly", "2 In-box Notification requests created", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertNewCusPollingTransaction(report1.PK, report1.TablePrefix, inboxTypes, mrnCode);

						AssertNewInboxListMessages(inboxURLs);
					});
				}
			}
		}

		public void TestRequestInboxNotifications_Click_MultipleConsignments_EHub()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(false))
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CXH_CustomsProfile = CertificateName;

				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_MovementReference = "AAA";
				var report1 = exitHeader.CusExitReports.AddNew();
				report1.CER_CXC_Consignment = consignment1.PK;
				var consignment2 = exitHeader.CusExitConsignments.AddNew();
				consignment2.CXC_MovementReference = "BBB";
				var report2 = exitHeader.CusExitReports.AddNew();
				report2.CER_CXC_Consignment = consignment2.PK;

				Factory.Save();

				using (var form = new ExitControlForm(exitHeader))
				{
					form.Show();

					ZFormModaliser.ShowDialogsInTest = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var reportsTabUserControl = GetReportsTabUserControl(form);
					var grid = reportsTabUserControl.FindSingle<ZArchitecture.ZGrid>("ReportsGrid");

					var requestInboxNotifEntryMenuItem = grid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications");

					exitHeader.HasChanges = true;
					reportsTabUserControl.Refresh();
					grid.SelectAllElements();

					CombineAssertions(() =>
					{
						CheckFactoryHasNoPendingChanges("Before sending inbox notification request");
						requestInboxNotifEntryMenuItem.PerformClick();
						CheckFactoryHasNoPendingChanges("After sending inbox notification request");

						AssertEquals("Should have message asking to save the declaration before sending", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

						AssertEquals("Inbox Notification Requests were sent correctly", "4 In-box Notification requests created", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertNewRequestEDIMessagesCreated(report1, inboxTypes);
						AssertNewRequestEDIMessagesCreated(report2, inboxTypes);
					});
				}
			}
		}

		public void TestRequestInboxNotifications_Click_MultipleConsignments_xT()
		{
			using (RegistryTemporarySetterHelper.SetEnableESInboxMessagesThroughDirectxTInterface(true))
			using (Env.SetTemporaryUserContext(new UserContext(Staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var mrnCode1 = "AAA";
				var mrnCode2 = "BBB";

				var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
				exitHeader.CXH_GS_NKCustomsAgent = Staff.GS_Code;
				exitHeader.CXH_CustomsProfile = CertificateName;

				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				carrier.OH_FullName = DeclarantName;
				carrier.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
				var address = Factory.NewWithValidTestData<OrgAddress>();
				address.OA_Code = "MYADDRESS";
				address.OA_OH = carrier.PK;
				exitHeader.CXH_OA_Carrier = address.PK;

				var consignment1 = exitHeader.CusExitConsignments.AddNew();
				consignment1.CXC_MovementReference = mrnCode1;
				var report1 = exitHeader.CusExitReports.AddNew();
				report1.CER_CXC_Consignment = consignment1.PK;
				var consignment2 = exitHeader.CusExitConsignments.AddNew();
				consignment2.CXC_MovementReference = mrnCode2;
				var report2 = exitHeader.CusExitReports.AddNew();
				report2.CER_CXC_Consignment = consignment2.PK;

				Factory.Save();

				using (var form = new ExitControlForm(exitHeader))
				{
					form.Show();

					ZFormModaliser.ShowDialogsInTest = false;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var reportsTabUserControl = GetReportsTabUserControl(form);
					var grid = reportsTabUserControl.FindSingle<ZArchitecture.ZGrid>("ReportsGrid");

					var requestInboxNotifEntryMenuItem = grid.ContextMenu.MenuItems.FindByText("Check for Inbox Notifications");

					exitHeader.HasChanges = true;
					reportsTabUserControl.Refresh();
					grid.SelectAllElements();

					CombineAssertions(() =>
					{
						CheckFactoryHasNoPendingChanges("Before sending inbox notification request");
						requestInboxNotifEntryMenuItem.PerformClick();
						CheckFactoryHasNoPendingChanges("After sending inbox notification request");

						AssertEquals("Should have message asking to save the declaration before sending", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

						AssertEquals("Inbox Notification Requests were sent correctly", "4 In-box Notification requests created", UnitTestUserNotification.Instance.LastMessage.Text);

						AssertNewCusPollingTransaction(report1.PK, report1.TablePrefix, inboxTypes, mrnCode1);
						AssertNewCusPollingTransaction(report2.PK, report2.TablePrefix, inboxTypes, mrnCode2);

						AssertNewInboxListMessages(inboxURLs);
					});
				}
			}
		}

		void AssertNewRequestEDIMessagesCreated(CusExitReport report, ZString[] messageTypes, string messageSubType = "")
		{
			var newRequestMessages = report.Messages;

			AssertEquals("messages count is correct", messageTypes.Length, newRequestMessages.Count);

			foreach (EDIMessage message in newRequestMessages)
			{
				AssertEquals("message.EM_MessageType", true, messageTypes.Contains(message.EM_MessageType));
				AssertRequestEDIMessageCreated(message, messageSubType);
			}
		}

		void AssertRequestEDIMessageCreated(EDIMessage message, string messageSubType = "")
		{
			AssertEquals("message.EM_ApplicationCode", Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage, message.EM_ApplicationCode);
			AssertEquals("message.EM_MessageSubType", messageSubType, message.EM_MessageSubType);
			AssertEquals("message.EM_IsTestMessage", true, message.EM_IsTestMessage);
			AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("message.EM_ApplicationReference", CertificateName, message.EM_ApplicationReference);
			AssertNull("message doesn't have interchange", message.Interchange);
		}

		void AssertNewCusPollingTransaction(ZGuid parentID, ZString parentTablePrefix, ZString[] messageTypes, ZString mrn)
		{
			var query = new ZQuery(CusPollingTransactionSchema.CPT_ApplicationCode, Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage);
			query.AddToFilter(CusPollingTransactionSchema.CPT_TransactionID, mrn);
			var newTransactions = Factory.Load<CusPollingTransaction>(query);

			AssertEquals("There should only be 1 ESC transaction for each messageType", messageTypes.Length, newTransactions.Length);

			foreach (var transaction in newTransactions)
			{
				AssertEquals(transaction.CPT_Type + " transaction.CPT_ApplicationCode", Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ESCustomsMessage, transaction.CPT_ApplicationCode);
				AssertEquals(transaction.CPT_Type + " transaction.CPT_Status", Core.Constants.Customs.CusPollingTransactionStatus.Codes.PND, transaction.CPT_Status);
				AssertEquals(transaction.CPT_Type + " transaction.CPT_Type is in list", true, messageTypes.Contains(transaction.CPT_Type));
				AssertEquals(transaction.CPT_Type + " transaction.CPT_TransactionID", mrn, transaction.CPT_TransactionID);
				AssertEquals(transaction.CPT_Type + " transaction.CPT_ParentID", parentID, transaction.CPT_ParentID);
				AssertEquals(transaction.CPT_Type + " transaction.CPT_ParentTableCode", parentTablePrefix, transaction.CPT_ParentTableCode);
			}
			AssertContainsExactElementsInAnyOrder("All transaction.CPT_Type are correct", messageTypes, newTransactions.Select(x => x.CPT_Type));
		}

		void AssertNewInboxListMessages(ZString[] urls)
		{
			var messagesQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.ESCustomsMessage);
			messagesQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			var newRequestMessages = NewFactory().Load<EDIMessage>(messagesQuery);

			AssertEquals("Messages count is correct", urls.Length, newRequestMessages.Length);

			var newRequestMessagesText = new List<ZString>();
			foreach (EDIMessage message in newRequestMessages)
			{
				AssertRequestEDIMessageCreated(message, DeclarationMessageSubTypeList.Codes.OriginalDeclaration);
				AssertEquals(message.EM_MessageType + " message.EM_MessageType", DeclarationMessageTypeList.Codes.InboxPendingList, message.EM_MessageType);

				var url = urls.FirstOrDefault(x => message.EM_MessageText.Contains(x));
				AssertMultilineASCIIEquals(message.EM_MessageType + " message.EM_MessageText", GetExpectedNewInboxMessageBodyText(url), message.EM_MessageText);
			}
		}

		ZString GetExpectedNewInboxMessageBodyText(ZString url) => ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soapenv:Header />
  <soapenv:Body>
<ListaDecV4Ent tipoRespuesta=""{0}"" xmlns=""https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adht/band/ws/li/ListaDecV4Ent.xsd"">
  <declarante>
    <NifDeclarante>{1}</NifDeclarante>
    <NombreDeclarante>{2}</NombreDeclarante>
  </declarante>
</ListaDecV4Ent>
  </soapenv:Body>
</soapenv:Envelope>", url, DeclarantId, DeclarantName);

		void CheckFactoryHasNoPendingChanges(ZString messagePrefix)
		{
			var mainFactoryChangeSet = Factory.GetChanges();

			bool mainFactoryHasChanges =
				mainFactoryChangeSet.GetChangedObjects().Any()
				|| mainFactoryChangeSet.GetAddedObjects().Any();

			AssertEquals(messagePrefix + " [All changes should be made in the sending factory] Does Main Factory have changes?", false, mainFactoryHasChanges);
		}

		ReportsTabUserControl GetReportsTabUserControl(ExitControlForm form)
		{
			var exitControlUserControl = form.FindSingle<ZUserControl>("ExitControlUserControl");
			var exitControlTabControl = exitControlUserControl.FindSingle<ZTabControl>("ExitControlTabControl");
			exitControlTabControl.SelectTab("ReportsTabPage");

			return (ReportsTabUserControl)exitControlUserControl.FindSingle<ZUserControl>("ReportsTabUserControl");
		}

		public GlbStaff Staff
		{
			get
			{
				if (staff == null)
				{
					staff = Factory.New<GlbStaff>();
					staff.GS_Code = "AH";
					staff.GS_LoginName = "ahtest";
					var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
					var cert = wrapper.ESBPasswordCollection.AddNew();
					cert.GP_Name = CertificateName;
					cert.GP_MailBoxID = "Test";
					cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
					cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				}

				return staff;
			}
		}
		GlbStaff staff;

		const string CertificateName = "TestCert1";
		const string DeclarantId = "NIF22222222";
		const string DeclarantName = "Declarant Full Name";
		readonly ZString[] inboxTypes = new ZString[] { DeclarationMessageTypeList.Codes.ExportExitClearanceNotification, DeclarationMessageTypeList.Codes.ExportExitNonConformityNotification };
		readonly ZString[] inboxURLs = new ZString[] { InboxNotificationResponseTypes.AESExitClearance, InboxNotificationResponseTypes.AESExitNonConformity };
	}
}
