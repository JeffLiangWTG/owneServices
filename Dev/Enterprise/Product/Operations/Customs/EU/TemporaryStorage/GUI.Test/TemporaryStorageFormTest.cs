using System.Linq;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TemporaryStorageForm))]
	sealed class TemporaryStorageFormTest : ZTemplateFormTest
	{
		public void TestMainDynamicLayoutPanel()
		{
			using (var form = new TemporaryStorageForm(Factory.New<TemporaryStorageHeader>()))
			{
				var mainDynamicLayoutPanel = form.MainDynamicLayoutPanel;
				AssertEquals("Name", nameof(TemporaryStorageForm.MainDynamicLayoutPanel), mainDynamicLayoutPanel.Name);
				AssertEquals("Dock", DockStyle.Fill, mainDynamicLayoutPanel.Dock);
				AssertEquals("AutoScroll", true, mainDynamicLayoutPanel.AutoScroll);
			}
		}

		public void TestMessagesControls()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
			using (var form = new TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.Controls.Find("MainTabControl", true).OfType<ZTemplateTabControl>().Single();
				var messagesTabPage = mainTabControl.FindSingle<ZTabPage>("MessagesTabPage");
				var messagesUserControl = messagesTabPage.FindSingle<ZDynamicControlCreationUserControl>("MessagesUserControl");
				CombineAssertions("MessagesTabPage properties", () =>
				{
					AssertEquals("MessagesTabPage should be visible.", true, messagesTabPage.TabVisible);
					AssertEquals("MessagesTabPage caption should be 'Message's.", "Messages", messagesTabPage.CaptionResourceString.Caption);
					AssertNotNull("MessagesUserControl should contain an user control of type MessagesUserControl.", messagesUserControl);
					AssertEquals("MessagesUserControl is correct type", typeof(MessagesTabUserControl), messagesUserControl.UserControlType);
				});
			}
		}

		public void TestBillsTabPage()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
			using (var form = new TemporaryStorageForm(tempStorage))
			{
				var billsTabPage = form.FindSingle<ZTabPage>("BillsTabPage");
				AssertEquals("Bills tab label should be Bills", "Bills", billsTabPage.CaptionResourceString.Caption);

				var billsLayoutPanel = billsTabPage.FindSingleOrDefault<DynamicLayoutPanel>("UCC6TemporaryStorageBillsLayoutPanel");
				AssertNotNull("BillsLayoutPanel should be child of BillsTabPage", billsLayoutPanel);

				var billControl = billsLayoutPanel.FindSingleOrDefault<UCC6TemporaryStorageBillControl>("UCC6TemporaryStorageBillControl");
				AssertNotNull("BillControl should be child of BillsLayoutPanel", billControl);
			}
		}

		public void TestMessagingMenus()
		{
			using (var form = GetFormToBash())
			{
				var menuTypes = form.Menu.MenuItems.Cast<ZMenuItem>().Select(x => x.GetType());
				Assert(menuTypes.Contains(typeof(TemporaryStorageMessagesMenu)));
			}
		}

		public void TestControls()
		{
			using (var form = GetFormToBash())
			{
				AssertNotNull(form.Controls.Find("UCC6TemporaryStorageLayoutPanel", true));
				AssertNotNull(form.Controls.Find("MessagesTabPage", true));
			}
		}

		public void TestFormCaption()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
			using var form = new TemporaryStorageForm(tempStorage);
			AssertEquals("Temporary Storage - UCC - JobReference", form.FormCaption);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockChs = Res.UseMockData())
			{
				var key = "C7892EF1-7792-422E-AF65-E7E7160F56BE";
				mockChs.Put(key, new ResourceStringData(key, "Temporary Storage - UCC (Chinese - Simplified)"));
				AssertEquals("Temporary Storage - UCC (Chinese - Simplified) - JobReference", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			var bill = tempStorage.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.PackedItems.AddNew().FillWithValidTestData();
			var presentationCustomsOfficeCode = tempStorage.PresentationCustomsOfficeCode;
			Factory.Save();
			var form = new TemporaryStorageForm(tempStorage);
			form.ControllerID = ControllerIDs.Customs.EU.UCC6TemporaryStorage;
			return form;
		}

		public void TestContainerControls()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
			using (var form = new TemporaryStorageForm(tempStorage))
			{
				form.Show();
				var mainTabControl = form.Controls.Find("MainTabControl", true).OfType<ZTemplateTabControl>().Single();
				var containerTabPage = mainTabControl.FindSingle<ZTabPage>("ContainerTabPage");
				var containerUserControl = containerTabPage.FindSingle<UCC6TemporaryStorageContainerControl>("UCC6TemporaryStorageContainerControl");
				CombineAssertions("ContainerTabPage properties", () =>
				{
					AssertEquals("ContainerTabPage should be visible.", true, containerTabPage.TabVisible);
					AssertEquals("ContainerTabPage caption should be 'Message's.", "Containers", containerTabPage.CaptionResourceString.Caption);
					AssertNotNull("ContainerUserControl should contain an user control of type ContainerUserControl.", containerUserControl);

					tempStorage.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
					AssertEquals("ContainerTabPage should not be visible when MessageType is 'TF'", false, containerTabPage.TabVisible);
				});
			}
		}

		[RequiresSTA]
		public void TestWorkFlowTabPage_IsVisible()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
			using (var form = new TemporaryStorageForm(tempStorage))
			{
				var mainTabControl = form.Controls.Find("MainTabControl", true).OfType<ZTemplateTabControl>().Single();
				var workflowTabPage = mainTabControl.FindSingle<ZTabPage>("WorkflowTabPage");
				AssertEquals("WorkflowTabPage should be visible.", true, workflowTabPage.TabVisible);
			}
		}

		[RequiresSTA]
		public void TestBillingTabPage()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";
			using (var form = new TemporaryStorageForm(tempStorage))
			{
				var billingTab = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
				AssertNotNull("Form should support invoicing and we should find the plugin", billingTab);
				billingTab.SelectTabPage();
				AssertEquals("Invoicing (billing) tab page enabled", true, billingTab.Enabled);
			}
		}

		public void TestNotifyUnableToCompleteActionAsHouseBillExists()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			var expectedMessage = "Some Message";

			using (var form = new TemporaryStorageForm(tempStorage))
			{
				tempStorage.NotifyUnableToCompleteActionAsHouseBillExists(expectedMessage);

				AssertEquals("The form shows the expected message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPlugIns()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			using var form = new TemporaryStorageForm(header);

			CombineAssertions(() =>
			{
				AssertNotNull(nameof(ControllerIDs.DocDataPlugIn), form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull(nameof(ControllerIDs.DocumentVisualizer), form.PlugIns.GetPlugIn(ControllerIDs.DocumentVisualizer));
			});
		}
	}
}
