using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.CH.GUI;
using Enterprise.Customs.CH.NCTS.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing.NCTS.MessageSending;

abstract class BaseMessageSendingFormTest<TForm> : MessageSendingFormWithValidationDetailsAbstractTest
		where TForm : MessageSendingFormWithValidationDetails, BaseMessageSendingFormTest<TForm>.IMessageSendingFormForTesting
{
	protected override bool AllowHasChangesOnFormOpen => true;
	protected abstract string MovementType { get; }

	protected override Form GetFormToBashCore() => CreateMessageSendingForm();

	public void TestSendToCustoms_WhenNotYetSent() => CombineAssertions(() =>
	{
		NctsHeader.EffectiveMessageStatus = ZString.Empty;
		using (var form = CreateMessageSendingForm())
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			form.Show();
			form.SendButtonExposed.Enabled = true;
			form.SendButtonExposed.PerformClick();
			AssertNull("ConfirmSendForm not shown", ZFormModaliser.LastFormShownDialogForTest);
			EventsTestHelper.AssertEventNotAdded(nctsHeader, Events.Authorised, withReference: ResendEventReference + "%");
			AssertEquals("SendForm closed", false, form.Visible);
			AssertEquals("Sendform.DialogResult", DialogResult.OK, form.DialogResult);
		}
	});

	public void TestSendToCustoms_WhenAlreadySent() => CombineAssertions(() =>
	{
		ZFormModaliser.ShowDialogsInTest = true;
		ZFormModaliser.SetDelegateToCallOnFormClosing(frm =>
		{
			if (frm is ResendReasonSendForm confirmSendForm)
			{
				confirmSendForm.Reason = "some reason";
			}
		});

		NctsHeader.EffectiveMessageStatus = CHLogicalStatusList.Codes.Sent;
		using (var form = CreateMessageSendingForm())
		{
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			form.Show();
			form.SendButtonExposed.Enabled = true;
			form.SendButtonExposed.PerformClick();
			AssertType<ResendReasonSendForm>("Cancel: ResendReasonSendForm shown", ZFormModaliser.LastFormShownDialogForTest);
			EventsTestHelper.AssertEventNotAdded(nctsHeader, Events.Authorised, withReference: ResendEventReference + "%", message: "Cancelled");
			AssertEquals("Cancelled: SendForm visible after confirmation canceled", true, form.Visible);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			form.Show();
			form.SendButtonExposed.Enabled = true;
			form.SendButtonExposed.PerformClick();
			AssertType<ResendReasonSendForm>("Confirm: ResendReasonSendForm shown", ZFormModaliser.LastFormShownDialogForTest);
			EventsTestHelper.AssertEventAdded(nctsHeader, Events.Authorised, withReference: ResendEventReference + "%", expectedReference: ResendEventReference + "|RES=some reason", message: "Confirmed");
			AssertEquals("Confirmed: SendForm closed after confirmation", false, form.Visible);
			AssertEquals("Confirmed: Sendform.DialogResult after confirmation", DialogResult.OK, form.DialogResult);
		}
	});

	protected abstract TForm CreateMessageSendingForm();

	protected NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNtcsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNtcsHeader()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = MovementType;
		return nctsHeader;
	}

	internal interface IMessageSendingFormForTesting
	{
		ZButton SendButtonExposed { get; }
		public ZCheckBox SendWithValidationErrorsCheckBoxExposed { get; }
	}

	const string ResendEventReference = "Submitting when Message status is sent";
}
