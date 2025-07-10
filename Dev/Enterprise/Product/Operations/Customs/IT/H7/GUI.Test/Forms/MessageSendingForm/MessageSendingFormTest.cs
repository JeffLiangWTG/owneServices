using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.H7.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.GUI.Testing;

[TestedType(typeof(MessageSendingForm))]
sealed class MessageSendingFormTest : EU.H7.GUI.Testing.MessageSendingFormTest
{
	protected override Form GetFormToBashCore()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var testingParent = new EU.H7.Business.MessageSendingObjectParent<MessageSendingObject>(header);
		return new MessageSendingForm(testingParent);
	}

	public override Type FormToBashType => typeof(MessageSendingForm);

	protected override Type MessageSendingGridColumnLayoutType => typeof(MessageSendingGridColumnLayout);

	protected override bool ExpectedAllowOverrideAmendmentReason => true;

	protected override bool ExpectedAllowOverrideCancellationReason => true;

	protected override IReadOnlyList<string> ExpectedGridColumnNames => new string[12] { "ShouldSend", "BillNumber", "Action", "SubStyle", "LocalReferenceNumber", "MRN", "MessageStatus", "CustomsStatus", "AmendmentReasonCode", "LegislativeReference", "DutyAmount", "Currency" };

	protected override Form GetTestingForm(BaseMessageSendingObjectParent testingParent)
	{
		return new MessageSendingForm(testingParent);
	}
}
