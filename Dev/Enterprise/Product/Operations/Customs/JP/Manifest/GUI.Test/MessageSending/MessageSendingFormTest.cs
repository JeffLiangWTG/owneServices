using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.Customs.JP.Manifest.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Manifest.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	public class MessageSendingFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDF01 };
			Header.SetCurrentMessageSendingContext(messageSendingContext);
			var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
			return new MessageSendingForm(sendingObjectParent);
		}

		public void TestHAWBGroupBoxAndENDCheckBox()
		{
			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01 };
			using (Header.SetCurrentMessageSendingContext(messageSendingContext))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
				using (var form = new MessageSendingForm(sendingObjectParent))
				{
					form.Show();

					var hawbGroupBox = form.FindSingle<ZGroupBox>("HAWBGroupBox");
					Assert(hawbGroupBox.Visible);

					var endCheckBox = form.FindSingle<ZCheckBox>("ENDCheckBox");
					Assert(endCheckBox.Visible);
				}
			}

			messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.HDF01;
			using (Header.SetCurrentMessageSendingContext(messageSendingContext))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
				using (var form = new MessageSendingForm(sendingObjectParent))
				{
					form.Show();

					var hawbGroupBox = form.FindSingle<ZGroupBox>("HAWBGroupBox");
					Assert(!hawbGroupBox.Visible);

					var endCheckBox = form.FindSingle<ZCheckBox>("ENDCheckBox");
					Assert(!endCheckBox.Visible);
				}
			}
		}

		public void TestCheckIsOkToSend_PopupForErrorOnForm()
		{
			SetCredentials();
			SetCompanyWithMailboxCredential();

			for (var i = 0; i < 21; i++)
			{
				Header.Bills.AddNew();
			}

			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01 };
			using (Header.SetCurrentMessageSendingContext(messageSendingContext))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
				sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ForEach(x => x.ShouldSend = true);
				using (var form = new MessageSendingForm(sendingObjectParent))
				{
					form.Show();
					sendingObjectParent.AllowSendWithError = true;

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					form.FindSingle<ZButton>("SendButton").PerformClick();
					AssertEquals("Please fix these errors before sending any messages:\r\n\r\nSend?: A maximum of 20 HAWBs can be included in the message.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public virtual void TestHDF01_RemovePreviousSortAndSortByBillStatus()
		{
			SetCredentials();
			SetCompanyWithMailboxCredential();

			header.AMA_Nature = JPJobMessageTypeList.Codes.Export;
			header.AMA_TransportMode = "AIR";

			var bill0 = Header.Bills.AddNew();
			bill0.ABL_BillStatus = "";
			var bill1 = Header.Bills.AddNew();
			bill1.ABL_BillStatus = JPCustomsStatusList.Codes.CAN;
			var bill2 = Header.Bills.AddNew();
			bill2.ABL_BillStatus = JPCustomsStatusList.Codes.AWD;
			var bill3 = Header.Bills.AddNew();
			bill3.ABL_BillStatus = JPCustomsStatusList.Codes.AWC;
			var bill4 = Header.Bills.AddNew();
			bill4.ABL_BillStatus = JPCustomsStatusList.Codes.AWR;
			var bill5 = Header.Bills.AddNew();
			bill5.ABL_BillStatus = JPCustomsStatusList.Codes.REG;
			var bill6 = Header.Bills.AddNew();
			bill6.ABL_BillStatus = JPCustomsStatusList.Codes.Deleted;

			var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HDF01 };
			using (header.SetCurrentMessageSendingContext(messageSendingContext))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
				sendingObjectParent.AllowSendWithError = true;
				var sendingObjects = sendingObjectParent.SendingObjectsCollection;
				sendingObjects[1].Action = JPMessageActionList.Codes.C;
				sendingObjects[3].Action = JPMessageActionList.Codes.X;

				using (var form = GetMessageSendingFormForTest(sendingObjectParent))
				{
					form.Show();
					var listManager = form.FindSingle<ZGrid>("MessageSendingObjectsGrid").ListManager;
					var list = listManager.List;
					var status = list.ToList<ManifestMessageSendingObject>().Select(x => x.Bill.ABL_BillStatus);
					AssertOrderByBillStatus(status.ToArray(), "When first open the form, grid should order by ABL_BillStatus.");

					var descriptor = listManager.GetItemProperties()["Action"];
					(listManager.List as IBindingList).ApplySort(descriptor, ListSortDirection.Ascending);
					status = list.ToList<ManifestMessageSendingObject>().Select(x => x.Bill.ABL_BillStatus);
					AssertOrderByAction(status.ToArray(), "After manually select order by 'Action', grid should order by Action.");
				}

				messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.HDE;
				using (var form = GetMessageSendingFormForTest(sendingObjectParent))
				{
					form.Show();
					var list = form.FindSingle<ZGrid>("MessageSendingObjectsGrid").ListManager.List;
					var numbers = list.ToList<ManifestMessageSendingObject>().Select(x => x.Bill.ABL_BillNumber);
					var status = list.ToList<ManifestMessageSendingObject>().Select(x => x.Bill.ABL_BillStatus);
					AssertOrderByAction(status.ToArray(), "The form(not HDF01) should remember what it is order by, it should still order by Action even if we close form and open a new form.");
				}

				messageSendingContext.ProcedureCode = JPProcedureCodeList.Codes.HDF01;
				using (var form = GetMessageSendingFormForTest(sendingObjectParent))
				{
					form.Show();
					var list = form.FindSingle<ZGrid>("MessageSendingObjectsGrid").ListManager.List;
					var numbers = list.ToList<ManifestMessageSendingObject>().Select(x => x.Bill.ABL_BillNumber);
					var status = list.ToList<ManifestMessageSendingObject>().Select(x => x.Bill.ABL_BillStatus);

					AssertOrderByBillStatus(status.ToArray(), "For message sending object form for HDF01, it should not remember previous column it is order by, it should always order by ABL_BillStatus when it is opened.");
				}

				void AssertOrderByBillStatus(ZString[] actualStatus, string message = "")
				{
					AssertArrayEqualsByElements(message, ["", JPCustomsStatusList.Codes.REG, JPCustomsStatusList.Codes.CAN, JPCustomsStatusList.Codes.AWR, JPCustomsStatusList.Codes.AWC, JPCustomsStatusList.Codes.AWD, JPCustomsStatusList.Codes.Deleted], actualStatus);
				}

				void AssertOrderByAction(ZString[] actualStatus, string message = "")
				{
					AssertArrayEqualsByElements(message, ["", JPCustomsStatusList.Codes.REG, JPCustomsStatusList.Codes.Deleted, JPCustomsStatusList.Codes.CAN, JPCustomsStatusList.Codes.AWR, JPCustomsStatusList.Codes.AWC, JPCustomsStatusList.Codes.AWD], actualStatus);
				}
			}
		}

		public virtual void TestSortByBillStatus_NV01()
		{
			var header = Factory.New<AsycudaManifestHeaderForTest>();
			header.AMA_TransportMode = TransportTypeList.Codes.Sea;
			var sendingContext = new MessageSendingContext();
			sendingContext.ProcedureCode = JPProcedureCodeList.Codes.NVC01;

			var awrBill = header.Bills.AddNew();
			var delBill = header.Bills.AddNew();
			var regBill = header.Bills.AddNew();
			var emptyStatusBill = header.Bills.AddNew();
			var amdBill = header.Bills.AddNew();
			var awdBill = header.Bills.AddNew();
			var awaBill = header.Bills.AddNew();
			regBill.ABL_BillStatus = JPCustomsStatusList.Codes.REG;
			amdBill.ABL_BillStatus = JPCustomsStatusList.Codes.AMD;
			delBill.ABL_BillStatus = JPCustomsStatusList.Codes.DEL;
			awrBill.ABL_BillStatus = JPCustomsStatusList.Codes.AWR;
			awdBill.ABL_BillStatus = JPCustomsStatusList.Codes.AWD;
			awaBill.ABL_BillStatus = JPCustomsStatusList.Codes.AWA;

			using (header.SetCurrentMessageSendingContext(sendingContext))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				using var form = GetMessageSendingFormForTest(sendingObjectParent);
				form.Show();

				var sendingObjectCollection = sendingObjectParent.SendingObjectsCollection;
				CombineAssertions(() =>
				{
					AssertEquals(emptyStatusBill, sendingObjectCollection[0].Bill);
					AssertEquals(regBill, sendingObjectCollection[1].Bill);
					AssertEquals(amdBill, sendingObjectCollection[2].Bill);
					AssertEquals(delBill, sendingObjectCollection[3].Bill);
					AssertEquals(awaBill, sendingObjectCollection[4].Bill);
					AssertEquals(awdBill, sendingObjectCollection[5].Bill);
					AssertEquals(awrBill, sendingObjectCollection[6].Bill);
				});
			}
		}

		public void TestRunPreSendValidation_OnlyValidateSelectedMessageSendingObjects()
		{
			SetCredentials();
			SetCompanyWithMailboxCredential();

			var bill1 = Header.Bills.AddNew();
			var bill2 = Header.Bills.AddNew();
			bill1.ABL_GrossWeightUQ = Core.Constants.Weight.Pounds;
			bill1.ABL_GrossWeight = 123456789m;

			var sendingObjectParent = new ManifestMessageSendingObjectParent(Header);
			sendingObjectParent.AllowSendWithError = true;
			var sendingObjects = sendingObjectParent.SendingObjectsCollection;
			var sendingObject1 = sendingObjects[0];
			var sendingObject2 = sendingObjects[1];

			sendingObject1.ShouldSend = true;
			sendingObject2.ShouldSend = false;
			using (var form = GetMessageSendingForm(sendingObjectParent))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FindSingle<ZButton>("SendButton").PerformClick();

				Assert(UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text != null && x.Text.Contains("Gross Weight: The number 123,456,789 is too large, the maximum value allowed for Gross Weight is 99,999,999.999.")));
			}

			sendingObject1.ShouldSend = false;
			sendingObject2.ShouldSend = true;
			using (var form = GetMessageSendingForm(sendingObjectParent))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FindSingle<ZButton>("SendButton").PerformClick();

				Assert(!UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text != null && x.Text.Contains("Gross Weight: The number 123,456,789 is too large, the maximum value allowed for Gross Weight is 99,999,999.999.")));
			}
		}

		public void TestSendMessageWithVisualData()
		{
			SetCredentials();
			SetCompanyWithMailboxCredential();
			var header = Header;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "TEST001";
			bill.MarkLightValidationAsValidForTesting();

			header.Factory.Save();

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01, EnableMessageVisual = true }))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObjectParent.AllowSendWithError = true;
				sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ForEach(c => c.ShouldSend = true);

				Assert("Pre-Condition", sendingObjectParent.Context.EnableMessageVisual);

				var displayForms = new List<Type>();

				using var form = GetMessageSendingFormForTest(sendingObjectParent);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FindSingle<ZButton>("SendButton").PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals(SendTarget.Normal, sendingObjectParent.Context.SendTarget);
					AssertEquals("Caption", "Preview and modify before sending?", lastMessage.Caption);
					AssertEquals("Caption", "There will 1 message in total to be sent. Would you like to preview them and modify before sending to customs?", lastMessage.Text);

					var lastFormShownDialog = ZFormModaliser.LastFormShownDialogForTest;
					AssertContains($"Type:{lastFormShownDialog.GetType()} Name: {lastFormShownDialog.Name} Text: {lastFormShownDialog.Text}", "EditableMessageSendingForm", lastFormShownDialog.GetType().Name);
				});
			}
		}

		public void TestExportMessageWithVisualData()
		{
			var header = Header;

			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = "TEST001";
			bill.MarkLightValidationAsValidForTesting();

			header.Factory.Save();

			using (header.SetCurrentMessageSendingContext(new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.HCH01, EnableMessageVisual = true }))
			{
				var sendingObjectParent = new ManifestMessageSendingObjectParent(header);
				sendingObjectParent.AllowSendWithError = true;
				sendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().ForEach(c => c.ShouldSend = true);

				Assert("Pre-Condition", sendingObjectParent.Context.EnableMessageVisual);

				var displayForms = new List<Type>();

				using var dir = new TestTemporaryDirectory();
				sendingObjectParent.ExportPath = dir.Directory.FullName;
				using var form = GetMessageSendingFormForTest(sendingObjectParent);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddYesAnswer();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FindSingle<ZButton>("ExportButton").PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals(SendTarget.FlatFile, sendingObjectParent.Context.SendTarget);
					AssertEquals("Caption", "Preview and modify before exporting?", lastMessage.Caption);
					AssertEquals("Caption", "There will 1 message in total to be exported. Would you like to preview them and modify before exporting?", lastMessage.Text);

					var lastFormShownDialog = ZFormModaliser.LastFormShownDialogForTest;
					AssertContains($"Type:{lastFormShownDialog.GetType()} Name: {lastFormShownDialog.Name} Text: {lastFormShownDialog.Text}", "EditableMessageSendingForm", lastFormShownDialog.GetType().Name);
				});
			}
		}

		public void TestExportMessageControls()
		{
			var header = Header;
			var context = new MessageSendingContext();
			header.SetCurrentMessageSendingContext(context);

			using (var form = GetMessageSendingForm(new ManifestMessageSendingObjectParent(header)))
			{
				form.Show();
				var exportPathTextBox = form.FindSingle<ZTextBox>("ExportPathTextBox");

				AssertEquals("Send or Export Messages", form.FormHeading);
				AssertEquals("&Send", form.FindSingle<ZButton>("SendButton").CaptionResourceString.Caption);
				AssertEquals("Export", form.FindSingle<ZButton>("ExportButton").CaptionResourceString.Caption);
				Assert("Should always be visible", exportPathTextBox.Visible);
				AssertEquals("ExportPathTextBox Anchor", AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom, exportPathTextBox.Anchor);
				Assert("Should always be visible", form.FindSingleOrDefault<ZButton>("ExportPathButton").Visible);
			}
		}

		public void TestNewColumns()
		{
			var header = Header;
			var sendingObjectParent = new ManifestMessageSendingObjectParent(header);

			var sendingContext = new MessageSendingContext();
			header.SetCurrentMessageSendingContext(sendingContext);

			using (var form = GetMessageSendingForm(sendingObjectParent))
			{
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				AssertNull("Action", grid.GetColumnStyle("Action"));
				AssertNull("Action", grid.GetColumnStyle("ActionDescription"));
				Assert(grid.GetColumnStyle("MessageType").IsReadOnly);
				AssertNotNull(grid.GetColumnStyle("Bill+ABL_BillNumber"));
				Assert(!grid.GetColumnStyle("Bill+ABL_MessageStatus").IsVisible);
			}

			sendingContext.ProcedureCode = JPProcedureCodeList.Codes.HDF01;
			sendingObjectParent = new ManifestMessageSendingObjectParent(header);

			using (var form = GetMessageSendingForm(sendingObjectParent))
			{
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				AssertEquals("Action", grid.GetColumnStyle("Action").GroupName.Caption);
				AssertEquals("Action", grid.GetColumnStyle("ActionDescription").GroupName.Caption);
			}

			sendingContext.ProcedureCode = JPProcedureCodeList.Codes.NVC01;
			sendingObjectParent = new ManifestMessageSendingObjectParent(header);

			using (var form = GetMessageSendingForm(sendingObjectParent))
			{
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				AssertEquals("Action", grid.GetColumnStyle("Action").GroupName.Caption);
				AssertEquals("Action", grid.GetColumnStyle("ActionDescription").GroupName.Caption);
			}

			sendingContext.ProcedureCode = JPProcedureCodeList.Codes.CHA;
			sendingObjectParent = new ManifestMessageSendingObjectParent(header);

			using (var form = GetMessageSendingForm(sendingObjectParent))
			{
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				Assert(grid.GetColumnStyle(nameof(ManifestMessageSendingObject.Reason)).IsVisible);
			}
		}

		public void TestSendButtonEnabled()
		{
			var header = Header;
			header.Bills.AddNew();
			header.CreateMessageErrorForTest = true;
			header.Validation.ValidateAll();
			var messageSendingParent = new ManifestMessageSendingObjectParent(header);

			using (var form = GetMessageSendingForm(messageSendingParent))
			{
				form.Show();
				var sendButton = form.FindSingle<ZButton>("SendButton");
				Assert(!sendButton.Enabled);
				var messageSendingObject = messageSendingParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;
				messageSendingParent.AllowSendWithError = true;
				Assert(!sendButton.Enabled);
			}

			SetCredentials();
			SetCompanyWithMailboxCredential();
			messageSendingParent = new ManifestMessageSendingObjectParent(header);
			using (var form = GetMessageSendingForm(messageSendingParent))
			{
				form.Show();
				var sendButton = form.FindSingle<ZButton>("SendButton");
				Assert(!sendButton.Enabled);
				var messageSendingObject = messageSendingParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;
				messageSendingParent.AllowSendWithError = true;
				Assert(sendButton.Enabled);
			}
		}

		public void TestContinueToSendCheckBoxReadOnly()
		{
			var header = Header;
			header.Bills.AddNew();
			header.CreateMessageErrorForTest = true;
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);

			using (var form = GetMessageSendingForm(messageSendingObjectParent))
			{
				form.Show();
				var continueToSendCheckBox = form.Controls.Find("ContinueToSendCheckBox", true)[0] as ZCheckBox;
				var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.FirstOrDefault() as ManifestMessageSendingObject;
				messageSendingObject.ShouldSend = false;
				Assert(continueToSendCheckBox.ReadOnly);

				messageSendingObject.ShouldSend = true;
				Assert(!continueToSendCheckBox.ReadOnly);
			}
		}

		public void TestAdditionalWarningsTextBox()
		{
			var header = Header;
			header.Bills.AddNew();
			header.CreateWarningForTest = true;
			header.Validation.ValidateAll();
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault();

			using (var form = GetMessageSendingForm(messageSendingObjectParent))
			{
				form.Show();
				var additionalWarningsTextBox = form.Controls.Find("AdditionalWarningsTextBox", true)[0] as ZTextBox;

				messageSendingObject.ShouldSend = false;
				AssertNullOrEmpty(additionalWarningsTextBox.Text);
				messageSendingObject.ShouldSend = true;
				AssertContains("TEST WARNING ON AMA_MANIFESTTYPE.", additionalWarningsTextBox.Text);
			}
		}

		public void TestRunPreSendValidation_TextBox()
		{
			var header = Header;
			header.Bills.AddNew();
			header.CreateMessageErrorForTest = true;
			header.Validation.ValidateAll();
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault();

			using (var form = GetMessageSendingForm(messageSendingObjectParent))
			{
				form.Show();
				var validationErrorsTextBox = form.FindSingle<ZTextBox>("ValidationErrorsTextBox");

				messageSendingObject.ShouldSend = false;
				AssertNullOrEmpty(validationErrorsTextBox.Text);
				messageSendingObject.ShouldSend = true;
				AssertContains("TEST MESSAGE ERROR ON AMA_MANIFESTTYPE.", validationErrorsTextBox.Text);
			}
		}

		public void TestRunPreSaveValidation_Popup()
		{
			Env.Security.GlobalManifestSendWithMessageErrors.IsAllowed = false;
			Env.Security.AllowMessageErrors.IsAllowed = false;
			GlbStaff.CurrentUser.GS_IsController = false;
			var staff = Factory.New<GlbStaff>();
			staff.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff.GS_IsController = true;
			Factory.Save();

			SetCredentials();
			SetCompanyWithMailboxCredential();
			var header = Header;
			header.Bills.AddNew();
			header.CreateErrorForTest = true;
			header.Validation.ValidateAll();
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);

			using (var form = GetMessageSendingForm(messageSendingObjectParent))
			{
				form.Show();
				var sendButton = form.FindSingle<ZButton>("SendButton");
				messageSendingObjectParent.AllowSendWithError = true;
				var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault();
				messageSendingObject.ShouldSend = true;

				sendButton.PerformClick();
				AssertContains("Should contain errors from header", "Test error on AMA_ManifestType.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				header.CreateErrorForTest = false;
				header.CreateMessageErrorForTest = true;
				header.Validation.ValidateAll();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendButton.PerformClick();
				AssertContains("Should contain message errors from header", "Test message error on AMA_ManifestType.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				sendButton.PerformClick();
				AssertContains("Should contain message from supervisor override", "The supervisor must have its Security Rights > Operate > Customs > Supervisor Overrides > Allow Message Errors set to Is Allowed.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void SetCredentials()
		{
			var settings = new MailboxAndRemoteWebPrintClientCredentials { LocalComputerAlias = "dummy", DomainName = "dummy", Status = XtCredentialStatusList.Codes.Unregistered };
			JPRegistry.Instance.MailboxAndRemoteWebPrintClientCredentials.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}

		void SetCompanyWithMailboxCredential()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Japan;

			var wrapper = GlbCompanyWrapper.GetWrapper<JPGlbCompanyWrapper>(company);

			var mailboxCredential = wrapper.MailboxCredential;
			mailboxCredential.GP_MailBoxID = "TEST";
			mailboxCredential.CurrentDecryptedPassword = "TEST";

			Header.AMA_GB = branch.PK;
		}

		public void TestMessageSendingObjectsGroupBoxCaption()
		{
			var header = Header;
			header.Bills.AddNew();
			header.CreateMessageErrorForTest = true;
			header.Validation.ValidateAll();
			var messageSendingObjectParent = new ManifestMessageSendingObjectParent(header);
			var messageSendingObject = messageSendingObjectParent.SendingObjectsCollection.Cast<ManifestMessageSendingObject>().FirstOrDefault();

			using (var form = new MessageSendingForm(messageSendingObjectParent))
			{
				form.Show();
				var messageSendingObjectsGroupBox = form.FindSingle<ZGroupBox>("messageSendingObjectsGroupBox");
				AssertEquals("House bills to be sent", messageSendingObjectsGroupBox.CaptionResourceString.Caption);
			}
		}

		protected AsycudaManifestHeaderForTest Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.NewWithValidTestData<AsycudaManifestHeaderForTest>();
					header.AMA_RL_NKPortOfLoading = "JPTKX";
					header.AMA_RL_NKPortOfDischarge = "JPTYK";
				}
				return header;
			}
		}

		AsycudaManifestHeaderForTest header;

		protected virtual MessageSendingForm GetMessageSendingForm(ManifestMessageSendingObjectParent sendingObjectParent)
		{
			return new MessageSendingForm(sendingObjectParent);
		}

		protected virtual MessageSendingForm GetMessageSendingFormForTest(ManifestMessageSendingObjectParent sendingObjectParent)
		{
			return new MessageSendingFormForTest(sendingObjectParent);
		}

		public class MessageSendingFormForTest : MessageSendingForm
		{
			public MessageSendingFormForTest(ManifestMessageSendingObjectParent sendingObjectParent)
				: base(sendingObjectParent)
			{
			}

			protected override bool CheckIsOKToSend() => true;
		}
	}
}
