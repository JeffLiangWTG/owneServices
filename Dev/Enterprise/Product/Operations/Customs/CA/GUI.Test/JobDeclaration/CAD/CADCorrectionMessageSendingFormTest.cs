using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CADCorrectionMessageSendingForm))]
	sealed class CADCorrectionMessageSendingFormTest : ZFormBasherTest
	{
		public void TestRefreshControls()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_CommoditySequence = 3;
			entryLine.CL_GoodsShipmentSequence = 4;
			var wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			wrapper.ForceSend = true;

			using (var form = new CADCorrectionMessageSendingForm(wrapper))
			{
				form.Show();
				AssertEquals("Form caption.", "Send Cor/Adj Message", form.FormCaption);

				var notificationLabel = form.Controls.Find("notificationLabel", true)[0];
				var sendOption = form.Controls.Find("sendOption", true)[0];
				var discardOption = form.Controls.Find("discardOption", true)[0];
				var sendingActionsGroupBox = form.Controls.Find("SendingActionsGroupBox", true)[0];
				AssertEquals("notificationLabel visibility", false, notificationLabel.Visible);
				AssertEquals("sendOption visibility.", false, sendOption.Visible);
				AssertEquals("discardOption visibility.", false, discardOption.Visible);
				AssertEquals("SendingActionsGroupBox visibility.", true, sendingActionsGroupBox.Visible);
			}

			wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			using (var form = new CADCorrectionMessageSendingForm(wrapper))
			{
				form.Show();
				AssertEquals("Form caption.", "Saving Options", form.FormCaption);

				var notificationLabel = form.Controls.Find("notificationLabel", true)[0];
				var sendOption = form.Controls.Find("sendOption", true)[0];
				var discardOption = form.Controls.Find("discardOption", true)[0];
				var sendingActionsGroupBox = form.Controls.Find("SendingActionsGroupBox", true)[0];
				AssertEquals("notificationLabel visibility", true, notificationLabel.Visible);
				AssertEquals("sendOption visibility.", true, sendOption.Visible);
				AssertEquals("discardOption visibility.", true, discardOption.Visible);
				AssertEquals("SendingActionsGroupBox visibility.", true, sendingActionsGroupBox.Visible);
			}
		}

		protected override System.Windows.Forms.Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var cadEntry = declaration.CustomsEntryHeaders.AddNew();
			cadEntry.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			var entryLine = cadEntry.MergedLines.AddNew();
			entryLine.CL_CommoditySequence = 3;
			entryLine.CL_GoodsShipmentSequence = 4;
			var wrapper = new CADCorrectionMessageSendingActionWrapper(cadEntry);
			return new CADCorrectionMessageSendingForm(wrapper);
		}
	}
}
