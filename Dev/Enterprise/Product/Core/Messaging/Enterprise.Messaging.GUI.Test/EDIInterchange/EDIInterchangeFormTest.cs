using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Testing
{
	[TestedType(typeof(EDIInterchangeForm))]
	sealed class EDIInterchangeFormTest : ZFormBasherTest
	{
		public void TestInterchangeModificationMenu()
		{
			SystemDataRegistry.Instance.AllowMessageModificationBeforeSending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = "USC";
			interchange.EI_To = "DUMMY";
			GlbStaff staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_IsDeveloper = false;
			GlbGroup group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff.Add(staff);
			Factory.Save();
			SystemDataRegistry.Instance.MessageModificationBeforeSendingAuthorisationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			using (EDIInterchangeForm form = new EDIInterchangeForm(interchange))
			{
				form.Show();
				AssertNull("Is Not Transmit", form.Menu.MenuItems.FindByText("Modify Interchange"));
			}

			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			Factory.Save();
			using (EDIInterchangeForm form = new EDIInterchangeForm(interchange))
			{
				form.Show();
				AssertNotNull(form.Menu.MenuItems.FindByText("Modify Interchange"));
			}

			group.Staff.Remove(staff);
			Factory.Save();
			using (EDIInterchangeForm form = new EDIInterchangeForm(interchange))
			{
				form.Show();
				AssertNull(form.Menu.MenuItems.FindByText("Modify Interchange"));
			}

			interchange.EI_Status = EDIInterchange.Status.Sent;
			Factory.Save();
			using (EDIInterchangeForm form = new EDIInterchangeForm(interchange))
			{
				form.Show();
				AssertNull("Is Not Queued", form.Menu.MenuItems.FindByText("Modify Interchange"));
			}

			interchange.EI_Status = EDIInterchange.Status.Queued;
			Factory.Save();

			SystemDataRegistry.Instance.AllowMessageModificationBeforeSending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (EDIInterchangeForm form = new EDIInterchangeForm(interchange))
			{
				form.Show();
				AssertNull(form.Menu.MenuItems.FindByText("Modify Interchange"));
			}

			staff.GS_IsDeveloper = true;
			Factory.Save();
			using (EDIInterchangeForm form = new EDIInterchangeForm(interchange))
			{
				form.Show();
				AssertNotNull("Is a developer", form.Menu.MenuItems.FindByText("Modify Interchange"));
			}
		}

		public void TestInterchangeModification_Click()
		{
			SystemDataRegistry.Instance.AllowMessageModificationBeforeSending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = "USC";
			interchange.EI_To = "DUMMY";
			GlbStaff staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			GlbGroup group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff.Add(staff);
			Factory.Save();
			SystemDataRegistry.Instance.MessageModificationBeforeSendingAuthorisationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			Factory.Save();
			using (EDIInterchangeForm form = new EDIInterchangeForm(interchange))
			{
				form.Show();
				MenuItem item = form.Menu.MenuItems.FindByText("Modify Interchange");
				AssertNotNull(item);
				item.PerformClick();
				AssertEquals(typeof(EDIInterchangeModificationForm), ZFormModaliser.LastFormShownForTest.GetType());
				ZFormModaliser.LastFormShownForTest.Dispose();
			}
		}

		public void TestInterchangeLargeUI()
		{
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			string testFilePath = LargeMessageTestHelper.CreateTestFile(LargeMessageTestHelper.FileType.Text, 100 * 1024 * 1024);
			try
			{
				LargeFileHolder tester = new LargeFileHolder(testFilePath);
				interchange.SetEI_BodyTextSource(tester);

				interchange.EI_HeaderText = "HEADER";
				interchange.EI_FooterText = "FOOTER";

				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_From = "USC";
				interchange.EI_To = "DUMMY";

				using (EDIInterchangeForm form = new EDIInterchangeForm(interchange))
				{
					form.Show();
					CheckControl(form, "zLabelTruncateNotification", isVisible: true);
					CheckControl(form, "zButtonSaveContentToDisk", isVisible: true);
					CheckControl(form, "zButtonSaveBodyTextToDisk", isVisible: true);
					CheckControl(form, "zTextBoxInterchangeText", isVisible: true);
					AssertEquals("Interchange content size should be 1Mb ", LargeMessageHelper.DetailTextSizeLimit, ((TextBox)form.Controls.Find("zTextBoxInterchangeText", true)[0]).Text.Length);
				}

				interchange.EI_BodyText = "blah";

				using (EDIInterchangeForm form = new EDIInterchangeForm(interchange))
				{
					form.Show();
					CheckControl(form, "zLabelTruncateNotification", isVisible: false);
					CheckControl(form, "zButtonSaveContentToDisk", isVisible: true);
					CheckControl(form, "zButtonSaveBodyTextToDisk", isVisible: true);
					CheckControl(form, "zTextBoxInterchangeText", isVisible: true);
				}
			}
			finally
			{
				File.Delete(testFilePath);
			}
		}

		public void TestVisibilityOfControls()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = "USC";
			interchange.EI_To = "DUMMY";
			interchange.EI_BodyText = "blah";
			interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange.EI_XTInternalMsgID = 1;
			using (var form = new EDIInterchangeForm(interchange))
			{
				form.Show();
				CheckControl(form, "zTextBoxEdiClient", true);

				var tabControl = (ZTemplateTabControl)form.Controls.Find("MainTabControl", true)[0];
				tabControl.SelectedIndex = 1;

				CheckControl(form, "interchangeEventUserControl", true);
				CheckControl(form, "xtMsgIdTextBox", true);
				CheckControl(form, "EventsGrid", true);
			}
		}

		public void TestVisibilityOfEventTabPage()
		{
			AssertVisibilityOfTabPage(1, EDIInterchange.TransportType.xT, true, true);
			AssertVisibilityOfTabPage(1, "", false, false);
			AssertVisibilityOfTabPage(0, EDIInterchange.TransportType.xT, false, false);
		}

		void AssertVisibilityOfTabPage(int xtMsgId, string transportType, bool expectedVisibility, bool expectedExists)
		{
			EDIInterchange interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = "USC";
			interchange.EI_To = "DUMMY";
			interchange.EI_BodyText = "blah";
			interchange.EI_TransportType = transportType;
			interchange.EI_XTInternalMsgID = xtMsgId;
			using (var form = new EDIInterchangeForm(interchange))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("MainTabControl", true)[0];
				tabControl.SelectedIndex = 1;
				CheckControl(form, "EventTabPage", expectedVisibility, expectedExists);
			}
		}

		static void CheckControl(Control form, string controlName, bool isVisible, bool isExists = true)
		{
			var controls = form.Controls.Find(controlName, true);

			if (isExists)
			{
				if (!(controls != null && controls.Length == 1))
				{
					Assert(controlName + " control should exists", false);
				}

				AssertEquals(controlName + "control should be visible", isVisible, controls[0].Visible);
			}
			else
			{
				AssertEquals(controlName + " control should not exists", false, controls.Any());
			}
		}

		static void CheckControl(Form form, string controlName, bool isVisible)
		{
			Control[] controls = form.Controls.Find(controlName, true);

			if (!(controls != null && controls.Length == 1))
			{
				Assert(controlName + " control should exists", false);
			}

			AssertEquals(controlName + "control should be visible", isVisible, controls[0].Visible);
		}

		protected override Form GetFormToBashCore()
		{
			EDIInterchange bizO = Factory.New<EDIInterchange>();
			bizO.EI_To = "TO";
			bizO.EI_From = "FROM";
			bizO.HasChanges = false;
			return new EDIInterchangeForm(bizO);
		}

		public void TestVisibilityOfEDIMessageControls()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = "USC";
			interchange.EI_To = "DUMMY";
			interchange.EI_BodyText = "blah";
			interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange.EI_XTInternalMsgID = 1;

			using (var form = new EDIInterchangeForm(interchange))
			{
				form.Show();
				var tabControl = (ZTemplateTabControl)form.Controls.Find("MainTabControl", true)[0];
				tabControl.SelectedIndex = 2;
				CheckControl(form, "EDIMessageTabPage", true);
				CheckControl(form, "EDIMessagesGrid", true);
			}
		}

		public void TestShowEDIMessageRecordsInGrid()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_SessionGUID = Guid.NewGuid();
			interchange.EI_HeaderText = "HEADER";
			interchange.EI_FooterText = "FOOTER";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_From = "USC";
			interchange.EI_To = "DUMMY";
			interchange.EI_BodyText = "blah";
			interchange.EI_TransportType = EDIInterchange.TransportType.xT;
			interchange.EI_XTInternalMsgID = 1;

			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			message.EM_EI = interchange.PK;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = "{ \"generatedLrn\": \"LRN123456\", \"result\": \"SUCCESS\", \"messages\": null }";
			message.MessageNumberStrategy = new GenericMessageNumberStrategy(Factory, interchange.EI_InterchangeNum);

			message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.eHub;
			message.EM_EI = interchange.PK;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Cancelled;
			message.EM_MessageText = "{ \"generatedLrn\": \"LRN1234567\", \"result\": \"Cancelled\", \"messages\": null }";
			message.MessageNumberStrategy = new GenericMessageNumberStrategy(Factory, interchange.EI_InterchangeNum);
			Factory.Save();

			using (var form = new EDIInterchangeForm(interchange))
			{
				form.Show();

				var tabControl = (ZTemplateTabControl)form.Controls.Find("MainTabControl", true)[0];
				tabControl.SelectedIndex = 2;

				var grid = (ZGrid)form.Controls.Find("EDIMessagesGrid", true).First();

				AssertEquals("Number of EDI messages should be equal",2, grid.List.Count);
				AssertType(typeof(EDIMessage), (IBindingList)grid.List[0]);

				var first_message = ((EDIMessage)(IBindingList)grid.List[0]);
				var second_message = ((EDIMessage)(IBindingList)grid.List[1]);
				AssertEquals("EM_ApplicationCode should be equal", EDIMessage.ApplicationCodes.COLS, first_message.EM_ApplicationCode);
				AssertEquals("EM_ReceiveTransmit should be equal", EDIMessage.Direction.Transmit, second_message.EM_ReceiveTransmit);

				grid.Select(1);

				var doubleClick = typeof(EDIInterchangeForm).GetMethod("GridEDIMessages_DoubleClick", BindingFlags.NonPublic | BindingFlags.Instance);
				AssertNotNull(doubleClick);
				doubleClick.Invoke(form, new object[] { null, EventArgs.Empty });

				var selectedMessage = (EDIMessage)grid.SelectedElements[0];
				using (var shownForm = OpenedFormCache.GetInstance().GetForm(selectedMessage.PK.ToGuid(), ControllerIDs.Messaging.EDIMessage.ToString()))
				{
					AssertNotNull(shownForm);
					AssertType(typeof(EDIMessageForm), shownForm);

					var datasource = ((EDIMessageForm)shownForm).LastDataSourceForTest;
					AssertType(typeof(EDIMessage), datasource);

					var popupMessage = (EDIMessage)datasource;
					AssertEquals("EM_ApplicationCode should be equal", EDIMessage.ApplicationCodes.eHub, popupMessage.EM_ApplicationCode);
					AssertEquals("EM_ReceiveTransmit should be equal", EDIMessage.Direction.Transmit, popupMessage.EM_ReceiveTransmit);

					var initialOpenFormsCount = Application.OpenForms.Count;
					doubleClick.Invoke(form, new object[] { null, EventArgs.Empty });
					AssertEquals(initialOpenFormsCount, Application.OpenForms.Count);
				}
			}
		}
	}
}
