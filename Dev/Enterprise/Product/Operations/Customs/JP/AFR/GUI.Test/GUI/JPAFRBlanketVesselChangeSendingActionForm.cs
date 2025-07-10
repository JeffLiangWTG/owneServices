using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	[TestedType(typeof(JPAFRBlanketVesselChangeSendingActionForm))]
	class JPAFRBlanketVesselChangeSendingActionFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var header = Factory.New<JPAFRHeader>();

			var blanketVesselChange = new BlanketVesselChange(header);
			using (var form = new JPAFRBlanketVesselChangeSendingActionForm(blanketVesselChange))
			{
				form.Show();
				AssertEquals("Blanket Vessel Change", form.FormCaption);
			}
		}

		public void TestSendButton()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			var blanketVesselChange = new BlanketVesselChange(header);
			using (var form = new JPAFRBlanketVesselChangeSendingActionForm(blanketVesselChange))
			{
				form.Show();
				AssertEquals(blanketVesselChange, form.BusinessEntity);
				AssertEquals(2, form.BusinessEntity.BlanketVesselChangeBills.Count);
				var msgObj1 = form.BusinessEntity.BlanketVesselChangeBills[0];
				var msgObj2 = form.BusinessEntity.BlanketVesselChangeBills[1];
				msgObj1.JPM_Send = ZBool.False;
				msgObj2.JPM_Send = ZBool.False;
				form.BusinessEntity.JPM_BlanketChange = false;
				AssertEquals(false, form.BusinessEntity.JPM_BlanketChange);
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendButton?.PerformClick();
				AssertContains("No bills have been selected for AFR reporting", UnitTestUserNotification.Instance.LastMessage.Text);

				msgObj1.JPM_Send = ZBool.True;
				msgObj2.JPM_Send = ZBool.True;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				sendButton?.PerformClick();
				AssertNotContains("No bills have been selected for AFR reporting", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No Vessel Details have changed from the Original AFR Manifest.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendButton?.PerformClick();
				AssertNotContains("No bills have been selected for AFR reporting", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:\r\n\r\nCarrier Code New: You have not entered a value.\nCarrier Code New: Please enter a valid Japan Customs Carrier Code, which should be 3-4 characters long with only alphanumeric characters.\nCarrier Code New: No Vessel Details have changed from the Original AFR Manifest.\nVessel Name New: You have not entered a value.\nVessel Name New: No Vessel Details have changed from the Original AFR Manifest.\nRadio Call Sign New: You have not entered a value.\nRadio Call Sign New: No Vessel Details have changed from the Original AFR Manifest.\nCountry Of Reg New: You have not entered a value.\nCountry Of Reg New: No Vessel Details have changed from the Original AFR Manifest.\nVoyage Number New: You have not entered a value.\nVoyage Number New: No Vessel Details have changed from the Original AFR Manifest.\nOperator Voyage New: No Vessel Details have changed from the Original AFR Manifest.\nPort Of Loading Code New: You have not entered a value.\nPort Of Loading Code New: No Vessel Details have changed from the Original AFR Manifest.\nPort Of Loading Suffix New: No Vessel Details have changed from the Original AFR Manifest.\nIs Departure From Relaxed Area New: No Vessel Details have changed from the Original AFR Manifest.\nETD New: You have not entered a value.\nETD New: No Vessel Details have changed from the Original AFR Manifest.\r\n\r\nDo you want to send the message(s) despite these errors?\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var form = new JPAFRBlanketVesselChangeSendingActionForm(blanketVesselChange))
			{
				form.Show();
				AssertEquals(blanketVesselChange, form.BusinessEntity);
				AssertEquals(2, form.BusinessEntity.BlanketVesselChangeBills.Count);
				form.BusinessEntity.JPM_BlanketChange = ZBool.True;
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendButton?.PerformClick();
				AssertNotContains("No bills have been selected for AFR reporting", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("It is likely that your message(s) will be rejected by Customs, as they have the following message errors:\r\n\r\nCarrier Code New: You have not entered a value.\nCarrier Code New: Please enter a valid Japan Customs Carrier Code, which should be 3-4 characters long with only alphanumeric characters.\nCarrier Code New: No Vessel Details have changed from the Original AFR Manifest.\nVessel Name New: You have not entered a value.\nVessel Name New: No Vessel Details have changed from the Original AFR Manifest.\nRadio Call Sign New: You have not entered a value.\nRadio Call Sign New: No Vessel Details have changed from the Original AFR Manifest.\nCountry Of Reg New: You have not entered a value.\nCountry Of Reg New: No Vessel Details have changed from the Original AFR Manifest.\nVoyage Number New: You have not entered a value.\nVoyage Number New: No Vessel Details have changed from the Original AFR Manifest.\nOperator Voyage New: No Vessel Details have changed from the Original AFR Manifest.\nPort Of Loading Code New: You have not entered a value.\nPort Of Loading Code New: No Vessel Details have changed from the Original AFR Manifest.\nPort Of Loading Suffix New: No Vessel Details have changed from the Original AFR Manifest.\nIs Departure From Relaxed Area New: No Vessel Details have changed from the Original AFR Manifest.\nETD New: You have not entered a value.\nETD New: No Vessel Details have changed from the Original AFR Manifest.\r\n\r\nDo you want to send the message(s) despite these errors?\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			header.JPH_MasterBillNumber = "MASTERBILL";
			bill1.JPB_BillNumber = "MB1";
			bill2.JPB_BillNumber = "MB2";
			blanketVesselChange = new BlanketVesselChange(header);
			using (var form = new JPAFRBlanketVesselChangeSendingActionForm(blanketVesselChange))
			{
				form.Show();

				blanketVesselChange.JPM_CarrierCodeNew = "test";
				var sendButton = (ZButton)form.Controls.Find("SendButton", true).FirstOrDefault();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendButton?.PerformClick();
			}

			AssertEquals("test", header.NewVesselVoyage.JP_CarrierCode);
		}

		public void TestBusinessEntity()
		{
			var header = Factory.New<JPAFRHeader>();
			var blanketVesselChange = new BlanketVesselChange(header);
			using (var form = new JPAFRBlanketVesselChangeSendingActionForm(blanketVesselChange))
			{
				form.Show();
				AssertEquals(blanketVesselChange, form.BusinessEntity);
			}
		}

		public void TestControlVisibilityForDifferentJobtype()
		{
			var header = Factory.New<JPAFRHeader>();
			using (var form = new JPAFRBlanketVesselChangeSendingActionForm(new BlanketVesselChange(header)))
			{
				form.Show();
				var newOperatorVoyageTextBox = form.Controls.Find("NewOperatorVoyageTextBox", true).FirstOrDefault() as ZTextBox;
				AssertEquals(false, newOperatorVoyageTextBox?.Visible);
			}
			header.JPH_IsShippingLineEntry = ZBool.True;
			using (var form = new JPAFRBlanketVesselChangeSendingActionForm(new BlanketVesselChange(header)))
			{
				form.Show();
				var newOperatorVoyageTextBox = form.Controls.Find("NewOperatorVoyageTextBox", true).FirstOrDefault() as ZTextBox;
				AssertEquals(true, newOperatorVoyageTextBox?.Visible);
			}
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		protected override Form GetFormToBashCore() => new JPAFRBlanketVesselChangeSendingActionForm(new BlanketVesselChange(Factory.New<JPAFRHeader>()));
	}
}
