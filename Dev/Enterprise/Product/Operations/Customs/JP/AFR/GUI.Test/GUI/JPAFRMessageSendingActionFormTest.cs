using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	[TestedType(typeof(JPAFRMessageSendingActionForm))]
	class JPAFRMessageSendingActionFormTest : ZFormBasherTest
	{
		protected override void TearDown()
		{
			base.TearDown();
			MainForm.Dispose();
		}

		public void TestJPH_VesselDetailsChangedCheckBoxVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			header.JPH_IsShippingLineEntry = true;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				form.Show();
				var jPH_VesselDetailsChangedCheckBox = form.Controls.Find("JPH_VesselDetailsChangedCheckBox", true).FirstOrDefault() as ZCheckBox;
				AssertEquals(false, jPH_VesselDetailsChangedCheckBox?.Visible);
			}
			header.JPH_IsShippingLineEntry = false;
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				form.Show();
				var jPH_VesselDetailsChangedCheckBox = form.Controls.Find("JPH_VesselDetailsChangedCheckBox", true).FirstOrDefault() as ZCheckBox;
				AssertEquals(true, jPH_VesselDetailsChangedCheckBox?.Visible);
			}
		}

		public void TestFormCaption()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			foreach (var data in new Tuple<ActionCode, string>[] {
				new Tuple<ActionCode, string>(ActionCode.AmendingAdd, "Manifest Amendments"),
				new Tuple<ActionCode, string>(ActionCode.AmendingDelete, "Manifest Amendments"),
				new Tuple<ActionCode, string>(ActionCode.AmendingUpdate, "Manifest Amendments"),
				})
			{
				using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, data.Item1), MainForm))
				{
					AssertEquals("Form Caption for " + data.Item1.ToString(), data.Item2, form.FormCaption);
				}
			}
		}

		public void TestNotificationOnlyOnBillMarkedForSending()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				form.Show();
				var sendingAction = form.BusinessEntity;
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				AssertEquals(2, sendingAction.ObjectsToSend.Count);
				sendingAction.ObjectsToSend.FirstOrDefault(a => a.JPM_BillOfLadingNumber == "MB1").JPM_Send = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendButton.PerformClick();
				AssertEquals("Please fix these errors before sending any messages:\r\n\r\nBill Of Lading: Bill Of Lading is missing\nMaster Bill Of Lading: Master Bill Of Lading is missing", UnitTestUserNotification.Instance.LastMessage.Text);

				var messageSendingObjects = sendingAction.MessageSendingObjects.Cast<MessageSendingObject>();
				messageSendingObjects.FirstOrDefault(a => a.JPM_BillOfLadingNumber != "MB1").JPM_Send = false;
				messageSendingObjects.FirstOrDefault(a => a.JPM_BillOfLadingNumber == "MB1").JPM_Send = true;
				sendButton.PerformClick();
				AssertNotContains("Bill Of Lading: Bill Of Lading is missing", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Please fix these errors before sending any messages:\r\n\r\nMaster Bill Of Lading: Master Bill Of Lading is missing", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCannotSendWithErrorOnDeleteReasonText()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			header.JPH_MasterBillNumber = "MB1";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				form.Show();
				var sendingAction = form.BusinessEntity;
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				var sendingObject = sendingAction.ObjectsToSend.First();
				sendingObject.JPM_Send = true;
				sendingObject.JPM_DeleteReasonText = "CHANGE CONSIGNEE &　NOTIFY COMPANY NAME AND ADDRESS";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendButton.PerformClick();
				AssertEquals("Please fix these errors before sending any messages:\r\n\r\nDelete Reason Text: Delete Reason Text only accepts Western European languages characters.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSelectTickAndUntickMenu()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "MB2";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "MB3";
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				var messageAction = form.BusinessEntity;
				AssertEquals(3, messageAction.MessageSendingObjects.Count);
				form.Show();

				var mainControlPanel = (ZPanel)form.Controls["MainControlPanel"];
				var billsGroupBox = (ZGroupBox)mainControlPanel.Controls["BillsGroupBox"];
				var billGrid = (ZGrid)billsGroupBox.Controls["BillsGrid"];
				billGrid.Focus();
				billGrid.SelectAllElements();
				MenuItem tickSendAllMenuItem = billGrid.ContextMenu.MenuItems.FindByText("Tick 'Send' for Selected");
				AssertNotNull("Precondition: Tick 'Send' for Selected", tickSendAllMenuItem);
				tickSendAllMenuItem.PerformClick();
				foreach (MessageSendingObject bill in messageAction.MessageSendingObjects)
				{
					AssertEquals(true, bill.JPM_Send);
				}

				billGrid.SelectAllElements();
				MenuItem untickSendAllMenuItem = billGrid.ContextMenu.MenuItems.FindByText("Untick 'Send' for Selected");
				AssertNotNull("Precondition: Untick 'Send' for Selected", untickSendAllMenuItem);
				untickSendAllMenuItem.PerformClick();
				foreach (MessageSendingObject bill in messageAction.MessageSendingObjects)
				{
					AssertEquals(false, bill.JPM_Send);
				}

				billGrid.UnSelect(1);
				AssertEquals("Precondition", 2, billGrid.SelectedElements.Length);

				tickSendAllMenuItem.PerformClick();
				AssertEquals(true, messageAction.MessageSendingObjects[0].JPM_Send);
				AssertEquals(false, messageAction.MessageSendingObjects[1].JPM_Send);
				AssertEquals(true, messageAction.MessageSendingObjects[2].JPM_Send);
			}
		}

		public void TestFormChangeBaseOnContent_NVOCC()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MasterBill";
			header.JPH_Voyage = "oldVOY";
			header.JPH_RL_NKLoading = "AUSYD";
			header.JPH_RL_NKDischarge = "JPTKY";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "MB2";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "MB3";
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Factory.Save();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				var messageAction = form.BusinessEntity;
				AssertEquals(3, messageAction.MessageSendingObjects.Count);
				var testSndObject1 = messageAction.MessageSendingObjects.OfType<MessageSendingObject>().FirstOrDefault(obj => obj.JPM_BillOfLadingNumber == "MB1");
				testSndObject1.JPM_ActionCode = AFRSendingActionCodeList.Codes.Update;
				testSndObject1.JPM_Send = false;
				var testSndObject2 = messageAction.MessageSendingObjects.OfType<MessageSendingObject>().FirstOrDefault(obj => obj.JPM_BillOfLadingNumber == "MB2");
				testSndObject2.JPM_ActionCode = AFRSendingActionCodeList.Codes.Delete;
				var testSndObject3 = messageAction.MessageSendingObjects.OfType<MessageSendingObject>().FirstOrDefault(obj => obj.JPM_BillOfLadingNumber == "MB3");
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("1st isSending", false, testSndObject1.JPM_Send);
					AssertEquals("2nd isSending", true, testSndObject2.JPM_Send);
					AssertEquals("3rd isSending", true, testSndObject3.JPM_Send);
					AssertEquals("3rd isSending ReadOnly", false, testSndObject3.JPM_SendInfo.ReadOnly);
					AssertEquals("1st action", AFRSendingActionCodeList.Codes.Update, testSndObject1.JPM_ActionCode);
					AssertEquals("2nd action", AFRSendingActionCodeList.Codes.Delete, testSndObject2.JPM_ActionCode);
					AssertEquals("3rd action", AFRSendingActionCodeList.Codes.Update, testSndObject3.JPM_ActionCode);
					AssertEquals("3rd action ReadOnly", false, testSndObject3.JPM_ActionCodeInfo.ReadOnly);
				});
				CombineAssertions(() =>
				{
					header.JPH_Voyage = "newVOY";
					AssertEquals("1st isSending", false, testSndObject1.JPM_Send);
					AssertEquals("2nd isSending", true, testSndObject2.JPM_Send);
					AssertEquals("3rd isSending", true, testSndObject3.JPM_Send);
					AssertEquals("3rd isSending ReadOnly", true, testSndObject3.JPM_SendInfo.ReadOnly);
					AssertEquals("1st action", AFRSendingActionCodeList.Codes.Register, testSndObject1.JPM_ActionCode);
					AssertEquals("2nd action", AFRSendingActionCodeList.Codes.Register, testSndObject2.JPM_ActionCode);
					AssertEquals("3rd action", AFRSendingActionCodeList.Codes.Register, testSndObject3.JPM_ActionCode);
					AssertEquals("3rd action ReadOnly", true, testSndObject3.JPM_ActionCodeInfo.ReadOnly);
				});
				CombineAssertions(() =>
				{
					messageAction.HasATDBeenSent = true;
					header.JPH_Voyage = "newVOY";
					AssertEquals("1st isSending", false, testSndObject1.JPM_Send);
					AssertEquals("2nd isSending", true, testSndObject2.JPM_Send);
					AssertEquals("3rd isSending", true, testSndObject3.JPM_Send);
					AssertEquals("3rd isSending ReadOnly", true, testSndObject3.JPM_SendInfo.ReadOnly);
					AssertEquals("1st action", AFRSendingActionCodeList.Codes.Add, testSndObject1.JPM_ActionCode);
					AssertEquals("2nd action", AFRSendingActionCodeList.Codes.Add, testSndObject2.JPM_ActionCode);
					AssertEquals("3rd action", AFRSendingActionCodeList.Codes.Add, testSndObject3.JPM_ActionCode);
					AssertEquals("3rd action ReadOnly", true, testSndObject3.JPM_ActionCodeInfo.ReadOnly);
				});
				CombineAssertions(() =>
				{
					messageAction.HasATDBeenSent = true;
					header.JPH_Voyage = "newVOY";
					header.JPH_RL_NKDischarge = "JPABA";
					AssertEquals("1st isSending", false, testSndObject1.JPM_Send);
					AssertEquals("2nd isSending", true, testSndObject2.JPM_Send);
					AssertEquals("3rd isSending", true, testSndObject3.JPM_Send);
					AssertEquals("3rd isSending ReadOnly", true, testSndObject3.JPM_SendInfo.ReadOnly);
					AssertEquals("1st action", AFRSendingActionCodeList.Codes.Add, testSndObject1.JPM_ActionCode);
					AssertEquals("2nd action", AFRSendingActionCodeList.Codes.Add, testSndObject2.JPM_ActionCode);
					AssertEquals("3rd action", AFRSendingActionCodeList.Codes.Add, testSndObject3.JPM_ActionCode);
					AssertEquals("3rd action ReadOnly", true, testSndObject3.JPM_ActionCodeInfo.ReadOnly);
				});
				CombineAssertions(() =>
				{
					messageAction.HasATDBeenSent = true;
					header.JPH_Voyage = "oldVOY";
					header.JPH_RL_NKDischarge = "JPABA";
					AssertEquals("1st isSending", false, testSndObject1.JPM_Send);
					AssertEquals("2nd isSending", true, testSndObject2.JPM_Send);
					AssertEquals("3rd isSending", true, testSndObject3.JPM_Send);
					AssertEquals("3rd isSending ReadOnly", true, testSndObject3.JPM_SendInfo.ReadOnly);
					AssertEquals("1st action", AFRSendingActionCodeList.Codes.Add, testSndObject1.JPM_ActionCode);
					AssertEquals("2nd action", AFRSendingActionCodeList.Codes.Add, testSndObject2.JPM_ActionCode);
					AssertEquals("3rd action", AFRSendingActionCodeList.Codes.Update, testSndObject3.JPM_ActionCode);
					AssertEquals("3rd action ReadOnly", false, testSndObject3.JPM_ActionCodeInfo.ReadOnly);
				});
				CombineAssertions(() =>
				{
					messageAction.HasATDBeenSent = false;
					header.JPH_Voyage = "oldVOY";
					header.JPH_RL_NKDischarge = "JPABA";
					AssertEquals("1st isSending", false, testSndObject1.JPM_Send);
					AssertEquals("2nd isSending", true, testSndObject2.JPM_Send);
					AssertEquals("3rd isSending", true, testSndObject3.JPM_Send);
					AssertEquals("3rd isSending ReadOnly", true, testSndObject3.JPM_SendInfo.ReadOnly);
					AssertEquals("1st action", AFRSendingActionCodeList.Codes.Register, testSndObject1.JPM_ActionCode);
					AssertEquals("2nd action", AFRSendingActionCodeList.Codes.Register, testSndObject2.JPM_ActionCode);
					AssertEquals("3rd action", AFRSendingActionCodeList.Codes.Update, testSndObject3.JPM_ActionCode);
					AssertEquals("3rd action ReadOnly", false, testSndObject3.JPM_ActionCodeInfo.ReadOnly);
				});
				CombineAssertions(() =>
				{
					messageAction.HasATDBeenSent = false;
					header.JPH_Voyage = "oldVOY";
					header.JPH_RL_NKDischarge = "JPTKY";
					AssertEquals("1st isSending", false, testSndObject1.JPM_Send);
					AssertEquals("2nd isSending", true, testSndObject2.JPM_Send);
					AssertEquals("3rd isSending", true, testSndObject3.JPM_Send);
					AssertEquals("3rd isSending ReadOnly", false, testSndObject3.JPM_SendInfo.ReadOnly);
					AssertEquals("1st action", AFRSendingActionCodeList.Codes.Register, testSndObject1.JPM_ActionCode);
					AssertEquals("2nd action", AFRSendingActionCodeList.Codes.Register, testSndObject2.JPM_ActionCode);
					AssertEquals("3rd action", AFRSendingActionCodeList.Codes.Update, testSndObject3.JPM_ActionCode);
					AssertEquals("3rd action ReadOnly", false, testSndObject3.JPM_ActionCodeInfo.ReadOnly);
				});
			}
		}

		public void TestFormChangeBaseOnContent_VOCC_BeforeATD()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			header.JPH_Voyage = "oldVoy";
			header.JPH_RL_NKLoading = "AUSYD";
			header.JPH_RL_NKDischarge = "JPTKY";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MBOL1";
			bill2.JPB_BillNumber = "MBOL2";
			bill3.JPB_BillNumber = "MBOL3";
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Factory.Save();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				var descriptionLabel = form.Controls.Find("DescriptionLabel", true).FirstOrDefault() as ZLabel;
				var testSndAction = form.BusinessEntity;
				AssertEquals(3, testSndAction.MessageSendingObjects.Count);
				var testSndObject1 = testSndAction.MessageSendingObjects.OfType<MessageSendingObject>().FirstOrDefault(obj => obj.JPM_BillOfLadingNumber == "MBOL1");
				var testSndObject2 = testSndAction.MessageSendingObjects.OfType<MessageSendingObject>().FirstOrDefault(obj => obj.JPM_BillOfLadingNumber == "MBOL2");
				var testSndObject3 = testSndAction.MessageSendingObjects.OfType<MessageSendingObject>().FirstOrDefault(obj => obj.JPM_BillOfLadingNumber == "MBOL3");
				UnitTestUserNotification.Instance.ClearMessages();
				form.Show();
				CombineAssertions("InitialStage", () =>
				{
					AssertEquals("SndAction ActionCode", ActionCode.AmendingAdd, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					AssertEquals("Checking descriptionLabel", DefaultDescriptionForVOCC, descriptionLabel.CaptionResourceString.Caption);
				});

				CombineAssertions("ChangeMasterInformation", () =>
				{
					header.JPH_RelaxedAppId = !header.JPH_RelaxedAppId;
					AssertEquals("SndAction ActionCode", ActionCode.CorrectMasterInformation, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.CorrectMasterInformation, false, AFRSendingActionCodeList.Codes.Update, true, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					AssertEquals("Checking descriptionLabel", MasterInformationChangedDescription, descriptionLabel.CaptionResourceString.Caption);
				});
				CombineAssertions("ChangeMasterInformation_Revert", () =>
				{
					header.JPH_RelaxedAppId = !header.JPH_RelaxedAppId;
					AssertEquals("SndAction ActionCode", ActionCode.AmendingAdd, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					AssertEquals("Checking descriptionLabel", DefaultDescriptionForVOCC, descriptionLabel.CaptionResourceString.Caption);
				});
				CombineAssertions("ChangeVesselInformation", () =>
				{
					header.JPH_RL_NKLoading = "AUBNE";
					AssertEquals("SndAction ActionCode", ActionCode.ReRegisterMasterBeforeATD, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.ReRegisterMasterBeforeATD, false, AFRSendingActionCodeList.Codes.Register, true, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					AssertEquals("Checking descriptionLabel", VesselInformationChangedBeforeATDDescriptionForVOCC, descriptionLabel.CaptionResourceString.Caption);
				});
				CombineAssertions("ChangeVesselInformation after ChangeVesselInformation", () =>
				{
					header.JPH_Voyage = "newVoy";
					AssertEquals("SndAction ActionCode", ActionCode.ReRegisterMasterBeforeATD, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.ReRegisterMasterBeforeATD, false, AFRSendingActionCodeList.Codes.Register, true, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					AssertEquals("Checking descriptionLabel", VesselInformationChangedBeforeATDDescriptionForVOCC, descriptionLabel.CaptionResourceString.Caption);
				});
				CombineAssertions("ChangeVesselInformation_Revert", () =>
				{
					header.JPH_RL_NKLoading = "AUSYD";
					header.JPH_Voyage = "oldVoy";
					AssertEquals("SndAction ActionCode", ActionCode.AmendingAdd, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					AssertEquals("Checking descriptionLabel", DefaultDescriptionForVOCC, descriptionLabel.CaptionResourceString.Caption);
				});
				CombineAssertions("ChangeMasterInformation Then ChangeVesselInformation", () =>
				{
					header.JPH_RelaxedAppId = !header.JPH_RelaxedAppId;
					AssertEquals("SndAction ActionCode", ActionCode.CorrectMasterInformation, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.CorrectMasterInformation, false, AFRSendingActionCodeList.Codes.Update, true, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					AssertEquals("Checking descriptionLabel", MasterInformationChangedDescription, descriptionLabel.CaptionResourceString.Caption);
					header.JPH_RL_NKLoading = "AUBNE";
					AssertEquals("SndAction ActionCode", ActionCode.ReRegisterMasterBeforeATD, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.ReRegisterMasterBeforeATD, false, AFRSendingActionCodeList.Codes.Register, true, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					AssertEquals("Checking descriptionLabel", VesselInformationChangedBeforeATDDescriptionForVOCC, descriptionLabel.CaptionResourceString.Caption);
				});
				CombineAssertions("Revert to ChangeMasterInformation", () =>
				{
					header.JPH_RL_NKLoading = "AUSYD";
					AssertEquals("SndAction ActionCode", ActionCode.CorrectMasterInformation, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.CorrectMasterInformation, false, AFRSendingActionCodeList.Codes.Update, true, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					AssertEquals("Checking descriptionLabel", MasterInformationChangedDescription, descriptionLabel.CaptionResourceString.Caption);
				});
			}
		}

		public void TestFormChangeBaseOnContent_VOCC_AfterATD()
		{
			var header = Factory.New<JPAFRHeader>();
			header.LogDepartureTimeRegistration();
			header.JPH_IsShippingLineEntry = true;
			header.JPH_Voyage = "oldVoy";
			header.JPH_RL_NKLoading = "AUSYD";
			header.JPH_RL_NKDischarge = "JPTKY";
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var bill3 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MBOL1";
			bill2.JPB_BillNumber = "MBOL2";
			bill3.JPB_BillNumber = "MBOL3";
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.HLD;
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Factory.Save();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				var testSndAction = form.BusinessEntity;
				AssertEquals(3, testSndAction.MessageSendingObjects.Count);
				var testSndObject1 = testSndAction.MessageSendingObjects.OfType<MessageSendingObject>().FirstOrDefault(obj => obj.JPM_BillOfLadingNumber == "MBOL1");
				var testSndObject2 = testSndAction.MessageSendingObjects.OfType<MessageSendingObject>().FirstOrDefault(obj => obj.JPM_BillOfLadingNumber == "MBOL2");
				var testSndObject3 = testSndAction.MessageSendingObjects.OfType<MessageSendingObject>().FirstOrDefault(obj => obj.JPM_BillOfLadingNumber == "MBOL3");
				UnitTestUserNotification.Instance.ClearMessages();
				form.Show();
				CombineAssertions("InitialStage", () =>
				{
					AssertEquals("SndAction ActionCode", ActionCode.AmendingAdd, testSndAction.ActionCode);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					Assert("hasATDBeenSend should be unchanged", testSndAction.HasATDBeenSent);
				});
				CombineAssertions("ChangeATDInformation", () =>
				{
					header.JPH_RelaxedAppId = !header.JPH_RelaxedAppId;
					AssertEquals("SndAction ActionCode", ActionCode.ChangeDepartureTimeAfterATD, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Add, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.ChangeDepartureTimeAfterATD, false, AFRSendingActionCodeList.Codes.Update, false, false);
					AssertTestSndObject("3", testSndObject3, ActionCode.ChangeDepartureTimeAfterATD, false, AFRSendingActionCodeList.Codes.Update, false, false);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					Assert("hasATDBeenSend should be unchanged", testSndAction.HasATDBeenSent);
				});
				CombineAssertions("ChangeATDInformation_Revert", () =>
				{
					header.JPH_RelaxedAppId = !header.JPH_RelaxedAppId;
					AssertEquals("SndAction ActionCode", ActionCode.AmendingAdd, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Add, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					Assert("hasATDBeenSend should be unchanged", testSndAction.HasATDBeenSent);
				});
				CombineAssertions("ChangeMasterInformation", () =>
				{
					header.JPH_RL_NKDischarge = "JPABA";
					AssertEquals("SndAction ActionCode", ActionCode.CorrectMasterInformation, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Add, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.CorrectMasterInformation, false, AFRSendingActionCodeList.Codes.Update, true, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.CorrectMasterInformation, false, AFRSendingActionCodeList.Codes.Update, true, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					Assert("hasATDBeenSend should be unchanged", testSndAction.HasATDBeenSent);
				});
				CombineAssertions("ChangeMasterInformation_Revert", () =>
				{
					header.JPH_RL_NKDischarge = "JPTKY";
					AssertEquals("SndAction ActionCode", ActionCode.AmendingAdd, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Add, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					Assert("hasATDBeenSend should be unchanged", testSndAction.HasATDBeenSent);
				});
				CombineAssertions("ChangeVesselInformation_WithNo", () =>
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					header.JPH_RL_NKLoading = "AUBNE";
					AssertEquals("SndAction ActionCode", ActionCode.AmendingAdd, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Add, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertEquals("NotificationMessage", @"Are you sure you want to change to another Vessel Information?

1. If 'No', the changes you have made on the Vessel Information will be revert back to their original value.

2. If 'Yes', the current 'ATD Registration Status' will be canceled in your system when the message is generated successfully.

!! Please remember that you need to register Departure Time again when the bills are successfully registered with JP Customs.
!! But already registered Bill/Departure Time on the old Vessel Information will not be allowed to be removed from JP Customs systems. You may experience some message errors when reusing the information in other jobs.
", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Port should be Unchanged", "AUSYD", header.JPH_RL_NKLoading);
					Assert("hasATDBeenSend should be unchanged", testSndAction.HasATDBeenSent);
				});
				CombineAssertions("ChangeVesselInformation_WithYes", () =>
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					header.JPH_RL_NKLoading = "AUBNE";
					AssertEquals("SndAction ActionCode", ActionCode.ReRegisterMasterAfterATD, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.ReRegisterMasterAfterATD, false, AFRSendingActionCodeList.Codes.Register, true, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.ReRegisterMasterAfterATD, false, AFRSendingActionCodeList.Codes.Register, true, true);
					AssertEquals("NotificationMessage", @"Are you sure you want to change to another Vessel Information?

1. If 'No', the changes you have made on the Vessel Information will be revert back to their original value.

2. If 'Yes', the current 'ATD Registration Status' will be canceled in your system when the message is generated successfully.

!! Please remember that you need to register Departure Time again when the bills are successfully registered with JP Customs.
!! But already registered Bill/Departure Time on the old Vessel Information will not be allowed to be removed from JP Customs systems. You may experience some message errors when reusing the information in other jobs.
", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Port should be changed", "AUBNE", header.JPH_RL_NKLoading);
					Assert("hasATDBeenSend should be changed", !testSndAction.HasATDBeenSent);
				});
				CombineAssertions("ChangeVesselInformation after ChangeVesselInformation", () =>
				{
					header.JPH_Voyage = "newVoy";
					AssertEquals("SndAction ActionCode", ActionCode.ReRegisterMasterAfterATD, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.ReRegisterMasterAfterATD, false, AFRSendingActionCodeList.Codes.Register, true, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.ReRegisterMasterAfterATD, false, AFRSendingActionCodeList.Codes.Register, true, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
				});
				CombineAssertions("ChangeVesselInformation_Revert", () =>
				{
					header.JPH_RL_NKLoading = "AUSYD";
					header.JPH_Voyage = "oldVoy";
					AssertEquals("SndAction ActionCode", ActionCode.AmendingAdd, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Add, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
				});
				CombineAssertions("ChangeVesselInformation_WithNo_AgainToMakeSureNotificationWillPop", () =>
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					header.JPH_RL_NKLoading = "AUBNE";
					AssertEquals("SndAction ActionCode", ActionCode.AmendingAdd, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Add, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.AmendingAdd, false, AFRSendingActionCodeList.Codes.Update, false, true);
					AssertEquals("NotificationMessage", @"Are you sure you want to change to another Vessel Information?

1. If 'No', the changes you have made on the Vessel Information will be revert back to their original value.

2. If 'Yes', the current 'ATD Registration Status' will be canceled in your system when the message is generated successfully.

!! Please remember that you need to register Departure Time again when the bills are successfully registered with JP Customs.
!! But already registered Bill/Departure Time on the old Vessel Information will not be allowed to be removed from JP Customs systems. You may experience some message errors when reusing the information in other jobs.
", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Port should be Unchanged", "AUSYD", header.JPH_RL_NKLoading);
					Assert("hasATDBeenSend should be unchanged", testSndAction.HasATDBeenSent);
				});
				CombineAssertions("ChangeATDInformation Then ChangeVesselInformation", () =>
				{
					header.JPH_RelaxedAppId = !header.JPH_RelaxedAppId;
					AssertEquals("SndAction ActionCode", ActionCode.ChangeDepartureTimeAfterATD, testSndAction.ActionCode);
					AssertTestSndObject("1-1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Add, false, true);
					AssertTestSndObject("1-2", testSndObject2, ActionCode.ChangeDepartureTimeAfterATD, false, AFRSendingActionCodeList.Codes.Update, false, false);
					AssertTestSndObject("1-3", testSndObject3, ActionCode.ChangeDepartureTimeAfterATD, false, AFRSendingActionCodeList.Codes.Update, false, false);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					Assert("hasATDBeenSend should be unchanged", testSndAction.HasATDBeenSent);
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					header.JPH_RL_NKLoading = "AUBNE";
					AssertEquals("SndAction ActionCode", ActionCode.ReRegisterMasterAfterATD, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Register, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.ReRegisterMasterAfterATD, false, AFRSendingActionCodeList.Codes.Register, true, true);
					AssertTestSndObject("3", testSndObject3, ActionCode.ReRegisterMasterAfterATD, false, AFRSendingActionCodeList.Codes.Register, true, true);
					AssertEquals("NotificationMessage", @"Are you sure you want to change to another Vessel Information?

1. If 'No', the changes you have made on the Vessel Information will be revert back to their original value.

2. If 'Yes', the current 'ATD Registration Status' will be canceled in your system when the message is generated successfully.

!! Please remember that you need to register Departure Time again when the bills are successfully registered with JP Customs.
!! But already registered Bill/Departure Time on the old Vessel Information will not be allowed to be removed from JP Customs systems. You may experience some message errors when reusing the information in other jobs.
", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertEquals("Port should be changed", "AUBNE", header.JPH_RL_NKLoading);
					Assert("hasATDBeenSend should be changed", !testSndAction.HasATDBeenSent);
				});
				CombineAssertions("Revert to ChangeATDInformation", () =>
				{
					header.JPH_RL_NKLoading = "AUSYD";
					AssertEquals("SndAction ActionCode", ActionCode.ChangeDepartureTimeAfterATD, testSndAction.ActionCode);
					AssertTestSndObject("1", testSndObject1, ActionCode.NewBill, false, AFRSendingActionCodeList.Codes.Add, false, true);
					AssertTestSndObject("2", testSndObject2, ActionCode.ChangeDepartureTimeAfterATD, false, AFRSendingActionCodeList.Codes.Update, false, false);
					AssertTestSndObject("3", testSndObject3, ActionCode.ChangeDepartureTimeAfterATD, false, AFRSendingActionCodeList.Codes.Update, false, false);
					AssertEquals("NotificationMessage", null, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessages();
					Assert("hasATDBeenSend should be unchanged", testSndAction.HasATDBeenSent);
				});
			}
		}

		public void TestSuppressValidationForMasterInformation()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MASTERBILL";
			header.JPH_Voyage = "oldVOY";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "MB2";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "MB3";
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Factory.Save();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				var messageAction = form.BusinessEntity;
				AssertEquals(3, messageAction.MessageSendingObjects.Count);
				form.Show();

				header.JPH_Voyage = "newVOY";
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				sendButton.PerformClick();
				AssertNotContains("Cannot change Voyage Number", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendButton.PerformClick();
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendButton.PerformClick();
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}

			var reloadedHeader = (new BusinessObjectFactory()).Load<JPAFRHeader>(header.PK);
			AssertEquals("newVOY", reloadedHeader.JPH_Voyage);

			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingHouseBillAdd;
			Factory.Save();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				var messageAction = form.BusinessEntity;
				AssertEquals(3, messageAction.MessageSendingObjects.Count);
				form.Show();

				header.JPH_Voyage = "newVOY2";
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				sendButton.PerformClick();
				AssertContains("Voyage Number: Cannot change Voyage Number from 'newVOY' to 'newVOY2' when the previous value 'newVOY' is being reported to Customs.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendButton.PerformClick();
				AssertContains("Voyage Number: Cannot change Voyage Number from 'newVOY' to 'newVOY2' when the previous value 'newVOY' is being reported to Customs.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(DialogResult.None, form.DialogResult);

				var cancelButton = (ZButton)form.Controls.Find("cancelButton", true).FirstOrDefault();
				cancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}

			reloadedHeader = (new BusinessObjectFactory()).Load<JPAFRHeader>(header.PK);
			AssertEquals("newVOY", reloadedHeader.JPH_Voyage);
		}

		public void TestETAOnMessageSendingActionUpdated()
		{
			var currentJPDate = DateTime.UtcNow.AddHours(9).Date;
			var oldDate = currentJPDate.AddMonths(-1);
			var newDate = currentJPDate.AddMonths(1);

			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MASTERBILL";
			header.JPH_Voyage = "oldVOY";
			header.JPH_ETA = oldDate;

			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Factory.Save();
			CombineAssertions(() =>
			{
				using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
				{
					var messageAction = form.BusinessEntity;
					AssertEquals("Pre: messageAction has 1 sub object", 1, messageAction.MessageSendingObjects.Count);
					form.Show();
					AssertEquals("Pre: Form should be showing", true, form.Visible);
					var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();

					sendButton.PerformClick();
					AssertContains("TST: Validation should be against the old Date", ValidationConstants.Header.PastDateNotAllowedForETA, UnitTestUserNotification.Instance.LastMessage.Text);

					header.JPH_ETA = newDate;
					AssertEquals("TST: Message sending Action should be reflacting the new Date", newDate, form.BusinessEntity.JPM_ETA);
					sendButton.PerformClick();
					AssertNotContains("TST: Validation should be against the new Date _ 1", ValidationConstants.Header.PastDateNotAllowedForETA, UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					sendButton.PerformClick();
					AssertNotContains("TST: Validation should be against the new Date _ 2", ValidationConstants.Header.PastDateNotAllowedForETA, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("TST: Form Result should be OK", DialogResult.OK, form.DialogResult);
					AssertEquals("TST: Form Should be closed", false, form.Visible);
				}

				var testReloadQuery = new ZDBOnlyQuery(typeof(JPAFRHeader));
				testReloadQuery.AddToFilter(ZArchitecture.Schema.JPAFRHeaderSchema.PK, header.PK);
				var reloadedHeader = (new BusinessObjectFactory()).Load<JPAFRHeader>(testReloadQuery).FirstOrDefault();
				AssertEquals("TST: new Date should be in DB", newDate, reloadedHeader.JPH_ETA);
			});
		}

		public void TestValidationForMasterBillShowsInDialogue()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = " MASTERBILL";
			header.JPH_Voyage = "oldVOY";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			Factory.Save();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				var messageAction = form.BusinessEntity;
				AssertEquals(1, messageAction.MessageSendingObjects.Count);
				form.Show();

				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				sendButton.PerformClick();
				AssertContains("The Master Bill Number cannot start with spaces.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendButton.PerformClick();
				AssertContains("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("The Master Bill Number cannot start with spaces.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAutoActionChangesStoppedWhenFormClosed()
		{
			var currentJPDate = DateTime.UtcNow.AddHours(9).Date;
			var oldDate = currentJPDate.AddMonths(-1);
			var newDate = currentJPDate.AddMonths(1);

			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MASTERBILL";
			header.JPH_Voyage = "oldVOY";

			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "MB2";
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			Factory.Save();
			CombineAssertions(() =>
			{
				using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
				{
					var messageAction = form.BusinessEntity;
					AssertEquals("Pre: messageAction has 2 sub object", 2, messageAction.MessageSendingObjects.Count);
					form.Shown += delegate(object sender, EventArgs e)
					{
						AssertEquals("Pre: Form should be showing", true, form.Visible);
						foreach (var item in messageAction.MessageSendingObjects.OfType<MessageSendingObject>())
						{
							AssertEquals(string.Format("{0} current Action Code Should Be Update", item.JPM_BillOfLadingNumber), AFRSendingActionCodeList.Codes.Update, item.JPM_ActionCode);
						}
						var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();

						header.JPH_Voyage = "newVoy";
						foreach (var item in messageAction.MessageSendingObjects.OfType<MessageSendingObject>())
						{
							AssertEquals(string.Format("{0} current Action Code Should Be automatically changed to Register", item.JPM_BillOfLadingNumber), AFRSendingActionCodeList.Codes.Register, item.JPM_ActionCode);
							AssertEquals(string.Format("{0} current Action Code Should Be automatically ticked to Send", item.JPM_BillOfLadingNumber), true, item.JPM_Send);
						}
						sendButton.PerformClick();

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						sendButton.PerformClick();
					};

					ZFormModaliser.ShowDialogsInTest = true;
					var dialogResult = ZFormModaliser.ShowDialogWithoutDispose(form);
					AssertEquals("TST: Form Should be closed", false, form.Visible);
					var objectsToSend = form.BusinessEntity.ObjectsToSend;
					var sendOK = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance).SendDataToCustoms(MessagingTypeList.Codes.UpdateAdvanceCargoInformationRegistrationHouse, (ZGuid billPK, ref string functionType) =>
					{
						var shouldContinue = false;
						functionType = null;
						var objectToSend = objectsToSend.FirstOrDefault(x => x.PK == billPK);
						if (objectToSend != null)
						{
							functionType = GetFunctionType(objectToSend.JPM_ActionCode);
							shouldContinue = true;
						}
						return shouldContinue;
					});
					Assert("SendOK", sendOK);
					var generatedMessages = Factory.Load<Messaging.Business.EDIMessage>(new ZQuery());
					AssertEquals("TST: Generated EDIMessage Count", 1, generatedMessages.Length);
					var generatedText = generatedMessages[0].EM_MessageText;
					AssertEquals("TST: REG Action Code Occurrance", 2, System.Text.RegularExpressions.Regex.Matches(generatedText, "<Code>REG</Code>").Count);
				}

				var testReloadQuery = new ZDBOnlyQuery(typeof(JPAFRHeader));
				testReloadQuery.AddToFilter(Enterprise.ZArchitecture.Schema.JPAFRHeaderSchema.PK, header.PK);
				var reloadedHeader = (new BusinessObjectFactory()).Load<JPAFRHeader>(testReloadQuery).FirstOrDefault();
				AssertEquals("TST: new VoyageInfomation Saved", "newVoy", reloadedHeader.JPH_Voyage);
			});
		}

		public void TestEventUnhookedUponDisposing()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MASTERBILL";
			header.JPH_Voyage = "oldVOY";
			Assert("Header should not be in DB initially", !header.IsInDatabase);
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
			}
			try
			{
				Factory.Save();
			}
			catch
			{
				Fail("Oops, Unexpected Exception");
			}
			Assert("Header should be in DB", header.IsInDatabase);
		}

		public void TestUpdateGridStatusForBillWithInbondDetails()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MASTERBILL";
			header.JPH_Voyage = "oldVOY";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MB1";
			header.InBondDetailInitiator = (MainForm as JPAFRForm);
			bill1.JPB_Calc_ArrivalBondedAreaCode = "test1";
			Factory.Save();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				try
				{
					int i = 0;
					var targetMessageSendingAction = form.BusinessEntity;
					form.InjectHeaderHasChangesChangedEventHandlerForTest(new EventHandler<HasChangesChangedEventArgs>((a, b) =>
					{
						if (i++ > 50)
						{
							throw new Exception("Over 50 hits of HasChangesChanged, heading to infinite");
						}
					}));
					targetMessageSendingAction.HasATDBeenSent = true;
				}
				catch (Exception e)
				{
					Fail("Oops, Unexpected Exception:" + e.Message);
				}
			}
			Assert("Header should be in DB", header.IsInDatabase);
		}

		public void TestLayoutChange_HasATDBeenSentCheckBox()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MasterBill";
			header.JPH_IsShippingLineEntry = true;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "MBOL1";
			bill2.JPB_BillNumber = "MBOL2";
			Factory.Save();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				form.Show();
				var aTDCheckBox = form.Controls.Find("HasATDBeenSentCheckBox", true).FirstOrDefault() as ZCheckBox;
				var descriptonLabel = form.Controls.Find("DescriptionLabel", true).FirstOrDefault() as ZLabel;
				CombineAssertions("Checking VOCC without ATD", () =>
				{
					AssertNotNull("Checking Box Existance", aTDCheckBox);
					Assert("Checking Box visibility", !aTDCheckBox.Visible);
					AssertEquals("Checking sendingAction Flag", false, form.BusinessEntity.HasATDBeenSent);
					AssertEquals("Checking DescriptionLabel Position", aTDCheckBox.Location.Y + 1, descriptonLabel.Location.Y);
					AssertEquals("Checking DescriptionLabel Size", ControlDpiScalingHelper.ScaleToCurrentDpiY(69 + 26), descriptonLabel.Size.Height);
					AssertEquals("TestDefault Description", "In order to change the Bill information you have made in the Bill tab, you just need to select the corresponding action from below grid leaving the above sections unchanged. If you want to correct the Vessel Information or change the Master level information which will affect all the bills, please make the modification above and the grid below will be updated to corresponding message action.", descriptonLabel.CaptionResourceString.Caption);
				});
			}
			header.LogDepartureTimeRegistration();
			Factory.Save();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				form.Show();
				var aTDCheckBox = form.Controls.Find("HasATDBeenSentCheckBox", true).FirstOrDefault() as ZCheckBox;
				var descriptonLabel = form.Controls.Find("DescriptionLabel", true).FirstOrDefault() as ZLabel;
				CombineAssertions("Checking VOCC without ATD", () =>
				{
					AssertNotNull("Checking Box Existance", aTDCheckBox);
					Assert("Checking Box visibility", !aTDCheckBox.Visible);
					AssertEquals("Checking sendingAction Flag", true, form.BusinessEntity.HasATDBeenSent);
					AssertEquals("Checking DescriptionLabel Position", aTDCheckBox.Location.Y + 1, descriptonLabel.Location.Y);
					AssertEquals("Checking DescriptionLabel Size", ControlDpiScalingHelper.ScaleToCurrentDpiY(69 + 26), descriptonLabel.Size.Height);
					AssertEquals("TestDefault Description", "In order to change the Bill information you have made in the Bill tab, you just need to select the corresponding action from below grid leaving the above sections unchanged. If you want to correct the Vessel Information or change the Master level information which will affect all the bills, please make the modification above and the grid below will be updated to corresponding message action.", descriptonLabel.CaptionResourceString.Caption);
				});
			}
			header.JPH_IsShippingLineEntry = false;
			Factory.Save();
			using (var form = new JPAFRMessageSendingActionForm(new MessageSendingAction(header, ActionCode.AmendingAdd), MainForm))
			{
				form.Show();
				var aTDCheckBox = form.Controls.Find("HasATDBeenSentCheckBox", true).FirstOrDefault() as ZCheckBox;
				var descriptonLabel = form.Controls.Find("DescriptionLabel", true).FirstOrDefault() as ZLabel;
				CombineAssertions("Checking NVOCC with ATD", () =>
				{
					AssertNotNull("Checking Box Existance", aTDCheckBox);
					Assert("Checking Box visibility", aTDCheckBox.Visible);
					AssertEquals("Checking sendingAction Flag", false, form.BusinessEntity.HasATDBeenSent);
					AssertEquals("Checking DescriptionLabel Position", ControlDpiScalingHelper.ScaleToCurrentDpiY(ControlDpiScalingHelper.UnscaleFromCurrentDpiY(aTDCheckBox.Location.Y) + 27), descriptonLabel.Location.Y);
					AssertEquals("Checking DescriptionLabel Size", ControlDpiScalingHelper.ScaleToCurrentDpiY(69), descriptonLabel.Size.Height);
					AssertEquals("TestDefault Description", "In order to change the House Bill level information you have made in the Bill tab, you just need to select the corresponding action from below grid leaving the above sections unchanged. If you want to correct the Vessel Information or change the Master level information which will affect all the bills, please make the modification above and the grid below will be updated to corresponding message action.", descriptonLabel.CaptionResourceString.Caption);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new JPAFRMessageSendingActionForm(new MessageSendingAction(Factory.New<JPAFRHeader>(), ActionCode.AmendingAdd), MainForm);
		}

		void AssertTestSndObject(string prefix, MessageSendingObject testSndObject, ActionCode actionCode, bool isActionReadOnly, string action, bool isSendingReadOnly, bool isSending)
		{
			AssertEquals(prefix + " ActionCode", actionCode, testSndObject.ActionCode);
			AssertEquals(prefix + " isSending", isSending, testSndObject.JPM_Send);
			AssertEquals(prefix + " isSending ReadOnly", isSendingReadOnly, testSndObject.JPM_SendInfo.ReadOnly);
			AssertEquals(prefix + " action", action, testSndObject.JPM_ActionCode);
			AssertEquals(prefix + " action ReadOnly", isActionReadOnly, testSndObject.JPM_ActionCodeInfo.ReadOnly);
		}

		string GetFunctionType(ZString actionCode)
		{
			string result = null;
			switch (actionCode)
			{
				case AFRSendingActionCodeList.Codes.Add:
					result = FunctionTypeList.Codes.Add;
					break;
				case AFRSendingActionCodeList.Codes.Delete:
					result = FunctionTypeList.Codes.Delete;
					break;
				case AFRSendingActionCodeList.Codes.Update:
					result = FunctionTypeList.Codes.Update;
					break;
				case AFRSendingActionCodeList.Codes.Register:
					result = FunctionTypeList.Codes.Registration;
					break;
			}
			return result;
		}

		ZForm MainForm
		{
			get
			{
				if (mainForm == null)
				{
					var header = Factory.New<JPAFRHeader>();
					mainForm = new JPAFRForm(header);
				}
				return mainForm;
			}
		}
		ZForm mainForm;

		#region Description String For Test

		const string MasterInformationChangedDescription = "Since the corresponding Master Level Information has been changed, all the registered bills should be updated to keep the consistency between the information recorded in Japan Customs and the records in your system. You can also choose to delete the unwanted bill that has already been registered.";
		const string DefaultDescriptionForVOCC = "In order to change the Bill information you have made in the Bill tab, you just need to select the corresponding action from below grid leaving the above sections unchanged. If you want to correct the Vessel Information or change the Master level information which will affect all the bills, please make the modification above and the grid below will be updated to corresponding message action.";
		const string VesselInformationChangedBeforeATDDescriptionForVOCC = "To change the Vessel Information (Carrier Code, Vessel Code. Voyage Number, Port of Loading and Port of Loading Suffix) before sending the ATD message, a ‘Registration’ message is used to re-manifest all the registered bills. Any other changes on the Bill level will also be submitted through the re-file.";

		#endregion

	}
}
