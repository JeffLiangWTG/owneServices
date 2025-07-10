using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Mail.GUI;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Client.EDI.Mail.Module
{
	abstract class EDIWorkTaskMailModuleTestCase<T> : ZModuleBasherTest
		where T : BusinessObject, IAllowAttachEmailsToEDocs
	{
		public void TestFilterControl()
		{
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			using (Control filterControl = (Control)module.InternalGetNewFilterControl())
			{
				Assert(typeof(EDIMailItemFilterControl).IsInstanceOfType(filterControl));
			}
		}

		public void TestGridCollection()
		{
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Assert(ExpectedGridCollectionType.IsInstanceOfType(module.GridCollection));
			}
		}

		public void TestFilterBusinessObject()
		{
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				Assert(typeof(EDIWorkTaskMailItemFilterBusinessObject).IsInstanceOfType(module.InternalGetNewFilterBusinessObject()));
			}
		}

		public void TestAllowDelete()
		{
			using (ZModule module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(true, module.AllowDelete);
			}
		}

		public void TestViewInOutlookExpress()
		{
			MailItem item = Factory.NewWithValidTestData<MailItem>();
			item.MI_Application = EDIMailApplication.CustomerService;
			item.MI_Subject = "Email 1";
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_From = "test@cargowise.com";
			Factory.Save();

			switch (GetTypeToMock().Name)
			{
				case "ImplementationMailItemModule":
					TestViewInOutlookExpress_ImplementationMailItemModule(item);
					break;
				case "CustomerServiceMailItemModule":
					TestViewInOutlookExpress_CustomerServiceMailItemModule(item);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		protected void TestViewInOutlookExpress_ImplementationMailItemModule(MailItem item)
		{
			var mockModule = new Mock<ImplementationMailItemModule>();
			mockModule.CallBase = true;
			bool mailShownInOutlook = false;
			mockModule.Setup(m => m.ShowMailInOutlookExpress(item))
				.Callback(() => mailShownInOutlook = true);
			using (var module = mockModule.Object)
			{
				IZForm result = module.InternalShowViewForm(item);
				AssertEquals(true, mailShownInOutlook);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(result);
				mailShownInOutlook = false;
				mockModule.Setup(m => m.ShowMailInOutlookExpress(item))
					.Throws(new InvalidOperationException("Some test exception"));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				result = module.InternalShowViewForm(item);
				AssertEquals(false, mailShownInOutlook);
				AssertEquals("There was a problem showing this email in Microsoft Outlook Express.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(result);
				AssertNoExceptionThrown(() => mockModule.Verify(m => m.ShowMailInOutlookExpress(item)));
				result.Dispose();
			}
		}

		protected void TestViewInOutlookExpress_CustomerServiceMailItemModule(MailItem item)
		{
			var mockModule = new Mock<CustomerServiceMailItemModule>();
			mockModule.CallBase = true;
			bool mailShownInOutlook = false;
			mockModule.Setup(m => m.ShowMailInOutlookExpress(item))
				.Callback(() => mailShownInOutlook = true);
			using (var module = mockModule.Object)
			{
				IZForm result = module.InternalShowViewForm(item);
				AssertEquals(true, mailShownInOutlook);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(result);
				mailShownInOutlook = false;
				mockModule.Setup(m => m.ShowMailInOutlookExpress(item))
					.Throws(new InvalidOperationException("Some test exception"));
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				result = module.InternalShowViewForm(item);
				AssertEquals(false, mailShownInOutlook);
				AssertEquals("There was a problem showing this email in Microsoft Outlook Express.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(result);
				AssertNoExceptionThrown(() => mockModule.Verify(m => m.ShowMailInOutlookExpress(item)));
				result.Dispose();
			}
		}

		public void TestCreateWorkTask()
		{
			MailItem item = Factory.NewWithValidTestData<MailItem>();
			item.MI_Application = ExpectedMailApplicationCode;
			item.MI_Subject = "Email 1";
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_From = "test@cargowise.com";
			MailItem item2 = Factory.NewWithValidTestData<MailItem>();
			item2.MI_Application = ExpectedMailApplicationCode;
			item2.MI_Subject = "Email 2";
			item2.MI_Direction = MailDirection.Receive;
			item2.MI_Status = MailStatus.Unprocessed;
			item2.MI_From = "test@cargowise.com";
			Factory.Save();
			using (ZForm form = new ZForm())
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				AssertNotNull(module.ToolBarButtons.FindByText("Create " + ((ZString)ExpectedWorkTaskTypeName).ToTitleCase()));
				module.CreateWorkTaskClick(this, EventArgs.Empty);
				AssertEquals(string.Format("Please select 1 or more mail items to attach to the new {0}.", ((ZString)ExpectedWorkTaskTypeName).ToLower()), UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.InternalPerformSearch();
				AssertEquals(2, module.InternalGrid.List.Count);
				module.InternalGrid.Select(0);
				module.InternalGrid.Select(1);
				module.CreateWorkTaskClick(this, EventArgs.Empty);
				AssertEquals(ExpectedWorkTaskFormType, module.LastShownFormForTest.GetType());
				T workTask = (T)module.LastShownFormForTest.BusinessEntity;
				AssertEquals(false, workTask.IsInDatabase);
				SaveCreatedWorkTask(workTask);
				AssertEquals(true, workTask.IsInDatabase);
				List<IeDoc> files = new List<IeDoc>();
				foreach (IeDoc eDoc in workTask.DocManagerInfo.Files)
				{
					files.Add(eDoc);
				}

				AssertEquals(2, workTask.DocManagerInfo.Files.Count);
				Assert("'Email 1.eml' should exist", files.Exists(d => ((BusinessObject)d)["SC_DescriptionForWeb"].ToString() == "Email 1.eml"));
				Assert("'Email 2.eml' should exist", files.Exists(d => ((BusinessObject)d)["SC_DescriptionForWeb"].ToString() == "Email 2.eml"));
				AssertEquals("no emails should be generated re no assigned staff etc", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				workTask.Factory.Save();
				AssertEquals("Should not be attaching more emails after the first Factory.Saved", 2, workTask.DocManagerInfo.Files.Count);
				module.LastShownFormForTest.Dispose();
			}
		}

		protected virtual void SaveCreatedWorkTask(T workTask)
		{
			workTask.Factory.Save();
		}

		public void TestAttachWorkTask()
		{
			MailItem mailItem = Factory.NewWithValidTestData<MailItem>();
			mailItem.MI_Application = ExpectedMailApplicationCode;
			mailItem.MI_Subject = "Email 1";
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_Status = MailStatus.Unprocessed;
			mailItem.MI_From = "test@cargowise.com";
			MailItem mailItem2 = Factory.NewWithValidTestData<MailItem>();
			mailItem2.MI_Application = ExpectedMailApplicationCode;
			mailItem2.MI_Subject = "Email 2";
			mailItem2.MI_Direction = MailDirection.Receive;
			mailItem2.MI_Status = MailStatus.Unprocessed;
			mailItem2.MI_From = "test@cargowise.com";
			T workTask = Factory.NewWithValidTestData<T>();
			Factory.Save();
			using (ZForm form = new ZForm())
			using (EDIWorkTaskMailModule<T> module1 = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module1.EmbeddedControl);
				form.Show();
				ZToolBarButton button = (ZToolBarButton)module1.ToolBarButtons.FindByText("Attach to " + ((ZString)ExpectedWorkTaskTypeName).ToTitleCase());
				button.PerformClick();
				AssertEquals(string.Format("Please select 1 or more mail items to attach to the {0}.", ((ZString)ExpectedWorkTaskTypeName).ToLower()), UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module1.InternalPerformSearch();
				AssertEquals(2, module1.InternalGrid.List.Count);
				module1.InternalGrid.Select(0);
				module1.InternalGrid.Select(1);
				button.PerformClick();
				EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm;
				ZFilterModule module = popup.Module_ForTest;
				((BusinessObjectCollection)module.GridCollection).Load(new ZQuery(WorkTaskPKColumn, workTask.PK));
				object lazyLoad = module.GridCollection.Count;
				((ZDisplayGrid)module.DisplayGrid).Select(0);
				popup.ExposedOKButtonForTesting.PerformClick();
				workTask = new BusinessObjectFactory().Load<T>(workTask.PK);
				AssertEquals("Emails should be attached", 2, workTask.DocManagerInfo.Files.Count);
				List<IeDoc> files = new List<IeDoc>();
				foreach (IeDoc eDoc in workTask.DocManagerInfo.Files)
				{
					files.Add(eDoc);
				}

				Assert("'Email 1.eml' should exist", files.Exists(d => ((BusinessObject)d)["SC_DescriptionForWeb"].ToString() == "Email 1.eml"));
				Assert("'Email 2.eml' should exist", files.Exists(d => ((BusinessObject)d)["SC_DescriptionForWeb"].ToString() == "Email 2.eml"));
				AssertEquals("no emails should be generated re no assigned staff etc", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		protected abstract SchemaPKColumn WorkTaskPKColumn { get; }

		public void TestAttachWorkTask_ConcurrentSave()
		{
			MailItem mailItem = Factory.NewWithValidTestData<MailItem>();
			mailItem.MI_Application = ExpectedMailApplicationCode;
			mailItem.MI_Subject = "Email";
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_Status = MailStatus.Unprocessed;
			mailItem.MI_From = "test@cargowise.com";
			T workTask = Factory.NewWithValidTestData<T>();
			Factory.Save();
			using (var form = new ZForm())
			using (var module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.InternalPerformSearch();
				module.InternalGrid.Select(0);
				var anotherFactory = new BusinessObjectFactory();
				anotherFactory.RefreshEnabled = false;
				var loadedMailItem = anotherFactory.Load<MailItem>(mailItem.PK);
				loadedMailItem.MI_Status = MailStatus.Processed;
				anotherFactory.Save();
				ZToolBarButton button = (ZToolBarButton)module.ToolBarButtons.FindByText("Attach to " + ((ZString)ExpectedWorkTaskTypeName).ToTitleCase());
				button.PerformClick();
				UnitTestUserNotification.Instance.AddOKAnswer();
				EmbeddedModulePopup popup = (EmbeddedModulePopup)ZFormModaliser.ActiveForm;
				ZFilterModule filterModule = popup.Module_ForTest;
				((BusinessObjectCollection)filterModule.GridCollection).Load(new ZQuery(WorkTaskPKColumn, workTask.PK));
				object lazyLoad = filterModule.GridCollection.Count;
				((ZDisplayGrid)filterModule.DisplayGrid).Select(0);
				popup.ExposedOKButtonForTesting.PerformClick();
				workTask = new BusinessObjectFactory().Load<T>(workTask.PK);
				AssertEquals("No save concurrency exception", 1, workTask.DocManagerInfo.Files.Count);
			}
		}

		public void TestReplyAllLongSubject()
		{
			MailItem item = Factory.NewWithValidTestData<MailItem>();
			item.MI_Application = ExpectedMailApplicationCode;
			item.MI_Subject = "".PadRight(254, 'A');
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_From = "Zubin Appoo <zubin.appoo@cargowise.com>";
			item.AddRecipientForUserCommunication("Tubolets Pavlo <pavlo.tubolets@edi.com.au>", MailRecipient.RecipientTypes.TO);
			item.AddRecipientForUserCommunication("test@cargowise.com", MailRecipient.RecipientTypes.CC);
			item.AddRecipientForUserCommunication("<test@edi.com.au>", MailRecipient.RecipientTypes.CC);
			foreach (string emailAddress in ExpectedEmailAddressesToIgnore)
			{
				item.AddRecipientForUserCommunication(emailAddress, MailRecipient.RecipientTypes.TO);
				item.AddRecipientForUserCommunication(emailAddress, MailRecipient.RecipientTypes.CC);
			}

			Factory.Save();
			using (ZForm form = new ZForm())
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				AssertNotNull(module.ToolBarButtons.FindByText("Reply"));
				AssertNotNull(module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[0]);
				AssertEquals("Reply", module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[0].Text);
				AssertNotNull(module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[1]);
				AssertEquals("Reply All", module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[1].Text);
				module.ReplyAllClick(this, EventArgs.Empty);
				AssertEquals("Please select a mail item to reply to.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.InternalPerformSearch();
				AssertEquals(1, module.InternalGrid.List.Count);
				module.InternalGrid.Select(0);
				AssertNoExceptionThrown(delegate
				{
					module.ReplyAllClick(this, EventArgs.Empty);
				});
				AssertEquals(typeof(CustomerServiceEmailForm), module.LastShownFormForTest.GetType());
				CustomerServiceEmail email = (CustomerServiceEmail)module.LastShownFormForTest.BusinessEntity;
				AssertEquals("Zubin Appoo;Tubolets Pavlo;test@cargowise.com;test@edi.com.au;", email.ToDisplayName);
				AssertEquals("zubin.appoo@cargowise.com;pavlo.tubolets@edi.com.au;test@cargowise.com;test@edi.com.au;", email.ToEmailAddress);
				AssertEquals("RE: ".PadRight(256, 'A'), email.Subject);
				module.LastShownFormForTest.Close();
				AssertEquals(MailStatus.Unprocessed, item.MI_Status);
				module.LastShownFormForTest.Dispose();
			}
		}

		public void TestReplyLongSubject()
		{
			MailItem item = Factory.NewWithValidTestData<MailItem>();
			item.MI_Application = ExpectedMailApplicationCode;
			item.MI_Subject = "".PadRight(254, 'A');
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_From = "Zubin Appoo <zubin.appoo@cargowise.com>";
			item.AddRecipientForUserCommunication("Tubolets Pavlo <pavlo.tubolets@edi.com.au>", MailRecipient.RecipientTypes.TO);
			item.AddRecipientForUserCommunication("test@cargowise.com", MailRecipient.RecipientTypes.CC);
			item.AddRecipientForUserCommunication("<test@edi.com.au>", MailRecipient.RecipientTypes.CC);
			foreach (string emailAddress in ExpectedEmailAddressesToIgnore)
			{
				item.AddRecipientForUserCommunication(emailAddress, MailRecipient.RecipientTypes.TO);
				item.AddRecipientForUserCommunication(emailAddress, MailRecipient.RecipientTypes.CC);
			}

			Factory.Save();
			using (ZForm form = new ZForm())
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				AssertNotNull(module.ToolBarButtons.FindByText("Reply"));
				AssertNotNull(module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[0]);
				AssertEquals("Reply", module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[0].Text);
				AssertNotNull(module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[1]);
				AssertEquals("Reply All", module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[1].Text);
				module.ReplyAllClick(this, EventArgs.Empty);
				AssertEquals("Please select a mail item to reply to.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.InternalPerformSearch();
				AssertEquals(1, module.InternalGrid.List.Count);
				module.InternalGrid.Select(0);
				AssertNoExceptionThrown(delegate
				{
					module.ReplyClick(this, EventArgs.Empty);
				});
				AssertEquals(typeof(CustomerServiceEmailForm), module.LastShownFormForTest.GetType());
				CustomerServiceEmail email = (CustomerServiceEmail)module.LastShownFormForTest.BusinessEntity;
				AssertEquals("RE: ".PadRight(256, 'A'), email.Subject);
				module.LastShownFormForTest.Close();
				AssertEquals(MailStatus.Unprocessed, item.MI_Status);
				module.LastShownFormForTest.Dispose();
			}
		}

		public void TestReplyToAllButton()
		{
			MailItem item = Factory.NewWithValidTestData<MailItem>();
			item.MI_Application = ExpectedMailApplicationCode;
			item.MI_Subject = "Test Email";
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_From = "Zubin Appoo <zubin.appoo@cargowise.com>";
			item.AddRecipientForUserCommunication("Tubolets Pavlo <pavlo.tubolets@edi.com.au>", MailRecipient.RecipientTypes.TO);
			item.AddRecipientForUserCommunication("test@cargowise.com", MailRecipient.RecipientTypes.CC);
			item.AddRecipientForUserCommunication("<test@edi.com.au>", MailRecipient.RecipientTypes.CC);
			foreach (string emailAddress in ExpectedEmailAddressesToIgnore)
			{
				item.AddRecipientForUserCommunication(emailAddress, MailRecipient.RecipientTypes.TO);
				item.AddRecipientForUserCommunication(emailAddress, MailRecipient.RecipientTypes.CC);
			}

			Factory.Save();
			using (ZForm form = new ZForm())
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				AssertNotNull(module.ToolBarButtons.FindByText("Reply"));
				AssertNotNull(module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[0]);
				AssertEquals("Reply", module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[0].Text);
				AssertNotNull(module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[1]);
				AssertEquals("Reply All", module.ToolBarButtons.FindByText("Reply").DropDownMenu.MenuItems[1].Text);
				module.ReplyAllClick(this, EventArgs.Empty);
				AssertEquals("Please select a mail item to reply to.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.InternalPerformSearch();
				AssertEquals(1, module.InternalGrid.List.Count);
				module.InternalGrid.Select(0);
				module.ReplyAllClick(this, EventArgs.Empty);
				AssertEquals(typeof(CustomerServiceEmailForm), module.LastShownFormForTest.GetType());
				CustomerServiceEmail email = (CustomerServiceEmail)module.LastShownFormForTest.BusinessEntity;
				AssertEquals("Zubin Appoo;Tubolets Pavlo;test@cargowise.com;test@edi.com.au;", email.ToDisplayName);
				AssertEquals("zubin.appoo@cargowise.com;pavlo.tubolets@edi.com.au;test@cargowise.com;test@edi.com.au;", email.ToEmailAddress);
				AssertEquals("RE: Test Email", email.Subject);
				module.LastShownFormForTest.Close();
				AssertEquals(MailStatus.Unprocessed, item.MI_Status);
				module.LastShownFormForTest.Dispose();
			}

			item.MI_From = "<nodisplayname@cargowise.com> ";
			AssertEquals("<nodisplayname@cargowise.com>", item.MI_From);
			Factory.Save();
			using (ZForm form = new ZForm())
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.InternalPerformSearch();
				module.InternalGrid.Select(0);
				module.ReplyAllClick(this, EventArgs.Empty);
				AssertEquals(typeof(CustomerServiceEmailForm), module.LastShownFormForTest.GetType());
				CustomerServiceEmail email = (CustomerServiceEmail)module.LastShownFormForTest.BusinessEntity;
				AssertEquals("nodisplayname@cargowise.com;Tubolets Pavlo;test@cargowise.com;test@edi.com.au;", email.ToDisplayName);
				AssertEquals("nodisplayname@cargowise.com;pavlo.tubolets@edi.com.au;test@cargowise.com;test@edi.com.au;", email.ToEmailAddress);
				AssertEquals("RE: Test Email", email.Subject);
				module.LastShownFormForTest.Close();
				module.LastShownFormForTest.Dispose();
			}

			item.MI_From = "nodisplayname@cargowise.com";
			AssertEquals("nodisplayname@cargowise.com", item.MI_From);
			Factory.Save();
			using (ZForm form = new ZForm())
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.InternalPerformSearch();
				module.InternalGrid.Select(0);
				module.ReplyAllClick(this, EventArgs.Empty);
				AssertEquals(typeof(CustomerServiceEmailForm), module.LastShownFormForTest.GetType());
				CustomerServiceEmail email = (CustomerServiceEmail)module.LastShownFormForTest.BusinessEntity;
				AssertEquals("nodisplayname@cargowise.com;Tubolets Pavlo;test@cargowise.com;test@edi.com.au;", email.ToDisplayName);
				AssertEquals("nodisplayname@cargowise.com;pavlo.tubolets@edi.com.au;test@cargowise.com;test@edi.com.au;", email.ToEmailAddress);
				AssertEquals("RE: Test Email", email.Subject);
				module.LastShownFormForTest.Close();
				module.LastShownFormForTest.Dispose();
			}

			item.MI_From = "undisclosed-recipients:";
			Factory.Save();
			using (ZForm form = new ZForm())
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.InternalPerformSearch();
				module.InternalGrid.Select(0);
				module.ReplyAllClick(this, EventArgs.Empty);
				AssertEquals(typeof(CustomerServiceEmailForm), module.LastShownFormForTest.GetType());
				CustomerServiceEmail email = (CustomerServiceEmail)module.LastShownFormForTest.BusinessEntity;
				AssertEquals("undisclosed-recipients:;Tubolets Pavlo;test@cargowise.com;test@edi.com.au;", email.ToDisplayName);
				AssertEquals(";pavlo.tubolets@edi.com.au;test@cargowise.com;test@edi.com.au;", email.ToEmailAddress);
				AssertEquals("RE: Test Email", email.Subject);
				module.LastShownFormForTest.Close();
				module.LastShownFormForTest.Dispose();
			}
		}

		public void TestForwardButton()
		{
			MailItem item = Factory.NewWithValidTestData<MailItem>();
			item.MI_Application = ExpectedMailApplicationCode;
			item.MI_Subject = "Email 1";
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_From = "Zubin Appoo <zubin.appoo@cargowise.com>";
			MailItem item2 = Factory.NewWithValidTestData<MailItem>();
			item2.MI_Application = ExpectedMailApplicationCode;
			item2.MI_Subject = "Fw: Email 5";
			item2.MI_Direction = MailDirection.Receive;
			item2.MI_Status = MailStatus.Unprocessed;
			item2.MI_From = "test@cargowise.com";
			Factory.Save();

			switch (GetTypeToMock().Name)
			{
				case "ImplementationMailItemModule":
					TestForwardButton_ImplementationMailItemModule();
					break;
				case "CustomerServiceMailItemModule":
					TestForwardButton_CustomerServiceMailItemModule();
					break;
				default:
					throw new NotImplementedException();
			}
		}

		protected void TestForwardButton_ImplementationMailItemModule()
		{
			var mockModule = new Mock<ImplementationMailItemModule>();
			mockModule.CallBase = true;
			bool mailShownInOutlook = false;
			mockModule.Setup(m => m.ShowMailInOutlookForForward(It.IsAny<MailItem>()))
				.Callback(() => mailShownInOutlook = true);
			using (ZForm form = new ZForm())
			using (var module = mockModule.Object)
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				AssertNotNull(module.ToolBarButtons.FindByText("Forward"));
				module.ForwardClick(this, EventArgs.Empty);
				AssertEquals("Please select a mail item to forward.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.InternalPerformSearch();
				AssertEquals(2, module.InternalGrid.List.Count);
				module.InternalGrid.Select(1);
				mailShownInOutlook = false;
				module.ForwardClick(this, EventArgs.Empty);
				Assert(mailShownInOutlook);
			}
		}

		protected void TestForwardButton_CustomerServiceMailItemModule()
		{
			var mockModule = new Mock<CustomerServiceMailItemModule>();
			mockModule.CallBase = true;
			bool mailShownInOutlook = false;
			mockModule.Setup(m => m.ShowMailInOutlookForForward(It.IsAny<MailItem>()))
				.Callback(() => mailShownInOutlook = true);
			using (ZForm form = new ZForm())
			using (var module = mockModule.Object)
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				AssertNotNull(module.ToolBarButtons.FindByText("Forward"));
				module.ForwardClick(this, EventArgs.Empty);
				AssertEquals("Please select a mail item to forward.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.InternalPerformSearch();
				AssertEquals(2, module.InternalGrid.List.Count);
				module.InternalGrid.Select(1);
				mailShownInOutlook = false;
				module.ForwardClick(this, EventArgs.Empty);
				Assert(mailShownInOutlook);
			}
		}

		public void TestForwardEmailSetAsProcessed()
		{
			MailItem item = Factory.NewWithValidTestData<MailItem>();
			item.MI_Application = ExpectedMailApplicationCode;
			item.MI_Subject = "Email DDD";
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_From = "Samuel Wang <samuel@cargowise.com>";
			Factory.Save();

			switch (GetTypeToMock().Name)
			{
				case "ImplementationMailItemModule":
					TestForwardEmailSetAsProcessed_ImplementationMailItemModule(item);
					break;
				case "CustomerServiceMailItemModule":
					TestForwardEmailSetAsProcessed_CustomerServiceMailItemModule(item);
					break;
				default:
					throw new NotImplementedException();
			}
		}

		protected void TestForwardEmailSetAsProcessed_ImplementationMailItemModule(MailItem item)
		{
			var mockModule = new Mock<ImplementationMailItemModule>();
			mockModule.CallBase = true;
			bool mailShownInOutlook = false;
			mockModule.Setup(m => m.ShowMailInOutlookExpress(item))
				.Callback(() => mailShownInOutlook = true);
			using (ZForm form = new ZForm())
			using (var module = mockModule.Object)
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.InternalPerformSearch();
				module.InternalGrid.Select(0);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				module.ShowMailInOutlookForForward(item);
				AssertEquals(true, mailShownInOutlook);
				AssertEquals("Should be marked as Processed", MailStatus.Processed, item.MI_Status);
			}
		}

		protected void TestForwardEmailSetAsProcessed_CustomerServiceMailItemModule(MailItem item)
		{
			var mockModule = new Mock<CustomerServiceMailItemModule>();
			mockModule.CallBase = true;
			bool mailShownInOutlook = false;
			mockModule.Setup(m => m.ShowMailInOutlookExpress(item))
				.Callback(() => mailShownInOutlook = true);
			using (ZForm form = new ZForm())
			using (var module = mockModule.Object)
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.InternalPerformSearch();
				module.InternalGrid.Select(0);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				module.ShowMailInOutlookForForward(item);
				AssertEquals(true, mailShownInOutlook);
				AssertEquals("Should be marked as Processed", MailStatus.Processed, item.MI_Status);
			}
		}

		public void TestReplyButton()
		{
			MailItem item = Factory.NewWithValidTestData<MailItem>();
			item.MI_Application = ExpectedMailApplicationCode;
			item.MI_Subject = "Email 1";
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_From = "Zubin Appoo <zubin.appoo@cargowise.com>";
			MailItem item2 = Factory.NewWithValidTestData<MailItem>();
			item2.MI_Application = ExpectedMailApplicationCode;
			item2.MI_Subject = "Re: Email 5";
			item2.MI_Direction = MailDirection.Receive;
			item2.MI_Status = MailStatus.Unprocessed;
			item2.MI_From = "test@cargowise.com";
			Factory.Save();
			using (ZForm form = new ZForm())
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				AssertNotNull(module.ToolBarButtons.FindByText("Reply"));
				module.ReplyClick(this, EventArgs.Empty);
				AssertEquals("Please select a mail item to reply to.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.InternalPerformSearch();
				AssertEquals(2, module.InternalGrid.List.Count);
				((IBusinessObjectCollection)module.InternalGrid.List).ApplySort(new SortInfo(MailDBItemsSchema.Constants.MI_From, System.ComponentModel.ListSortDirection.Descending));
				module.InternalGrid.Select(0);
				module.ReplyClick(this, EventArgs.Empty);
				AssertEquals(typeof(CustomerServiceEmailForm), module.LastShownFormForTest.GetType());
				CustomerServiceEmail email = (CustomerServiceEmail)module.LastShownFormForTest.BusinessEntity;
				AssertEquals("Zubin Appoo;", email.ToDisplayName);
				AssertEquals("zubin.appoo@cargowise.com;", email.ToEmailAddress);
				AssertEquals("RE: Email 1", email.Subject);
				((CustomerServiceEmailForm)module.LastShownFormForTest).Close(); // Don't send email
				AssertEquals(MailStatus.Unprocessed, item.MI_Status);
				module.LastShownFormForTest.Dispose();
				module.InternalPerformSearch();
				module.InternalGrid.Select(1);
				module.ReplyClick(this, EventArgs.Empty);
				AssertEquals(typeof(CustomerServiceEmailForm), module.LastShownFormForTest.GetType());
				email = (CustomerServiceEmail)module.LastShownFormForTest.BusinessEntity;
				AssertEquals("test@cargowise.com;", email.ToDisplayName);
				AssertEquals("test@cargowise.com;", email.ToEmailAddress);
				AssertEquals("Re: Email 5", email.Subject);
				email.SendEmail();
				AssertEquals(MailStatus.Processed, item2.MI_Status);
				AssertEquals("Saved", false, item2.MI_StatusInfo.HasChanges);
				module.LastShownFormForTest.Dispose();
			}
		}

		public void TestShowOnlyRelevantMailItems()
		{
			MailItem item1 = GetNewMailItem();
			item1.MI_Application = "STD";
			item1.MI_Subject = "STD Item";
			MailItem item2 = GetNewMailItem();
			item2.MI_Application = ExpectedMailApplicationCode;
			item2.MI_Subject = "Work Task Item";
			Factory.Save();
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				module.FilterBusinessObject.ResetToDefaultValues();
				module.InternalPerformSearch();
				ZQuery query = new ZQuery(MailDBItemsSchema.MI_Application, "STD");
				query.AddToFilter(MailDBItemsSchema.MI_Subject, "STD Item");
				Assert("Shouldn't find records with MI_Application='STD'", ((BusinessObjectCollection)module.GridCollection).Find(query).Length == 0);
				query = new ZQuery(MailDBItemsSchema.MI_Application, ExpectedMailApplicationCode);
				query.AddToFilter(MailDBItemsSchema.MI_Subject, "Work Task Item");
				Assert(string.Format("Should find records with MI_Application='{0}'", ExpectedMailApplicationCode), ((BusinessObjectCollection)module.GridCollection).Find(query).Length != 0);
			}
		}

		public void TestMarkAsProcessed()
		{
			MailItem item = Factory.NewWithValidTestData<MailItem>();
			item.MI_Application = ExpectedMailApplicationCode;
			item.MI_ReceivedDateTime = ZDateTime.Now;
			item.MI_SendDateTime = ZDateTime.Now;
			item.MI_Subject = "Email 1";
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_From = "Zubin Appoo <zubin.appoo@cargowise.com>";
			Factory.Save();
			using (ZForm form = new ZForm())
			using (EDIWorkTaskMailModule<T> module = (EDIWorkTaskMailModule<T>)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.InternalPerformSearch();
				module.InternalGrid.Select(0);
				module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Mark as PRS").PerformClick();
				AssertEquals("Should be marked as Processed", MailStatus.Processed, item.MI_Status);
			}
		}

		#region Implementation
		protected abstract string ExpectedWorkTaskTypeName { get; }

		protected abstract string ExpectedMailApplicationCode { get; }

		protected abstract string[] ExpectedEmailAddressesToIgnore { get; }

		protected abstract Type ExpectedWorkTaskFormType { get; }

		protected abstract Type ExpectedGridCollectionType { get; }

		protected Type GetTypeToMock()
		{
			using (ZModule module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				return module.GetType();
			}
		}

		protected MailItem GetNewMailItem()
		{
			MailItem item = Factory.New<MailItem>();
			item.MI_Direction = MailDirection.Receive;
			item.MI_Status = MailStatus.Unprocessed;
			item.MI_SendDateTime = ZDateTime.Now;
			item.MI_ReceivedDateTime = ZDateTime.Now;
			return item;
		}

		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			return GetNewMailItem();
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => true;
		#endregion
	}
}
