using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.GUI.Testing;

[TestedType(typeof(MessageSendingGridColumnLayout))]
sealed class MessageSendingGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<MessageSendingGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(nameof(AutoMessageSendingObject.ShouldSend), typeof(ZCheckBoxColumnStyleInfo), 140),
		(AutoMessageSendingObject.Schema.BillNumber, typeof(ZTextBoxColumnStyleInfo), 140),
		(AutoMessageSendingObject.Schema.Action, typeof(ZDropEditColumnStyleInfo), 140),
		(AutoMessageSendingObject.Schema.SubStyle, typeof(ZDropEditColumnStyleInfo), 100),
		(AutoMessageSendingObject.Schema.LocalReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 140),
		(AutoMessageSendingObject.Schema.MRN, typeof(ZTextBoxColumnStyleInfo), 150),
		(AutoMessageSendingObject.Schema.MessageStatus, typeof(ZTextBoxColumnStyleInfo), 100),
		(AutoMessageSendingObject.Schema.CustomsStatus, typeof(ZTextBoxColumnStyleInfo), 100),
		(AutoMessageSendingObject.Schema.AmendmentReasonCode, typeof(ZDropEditColumnStyleInfo), 160),
		(Business.MessageSendingObject.Schema.LegislativeReference, typeof(ZDropEditColumnStyleInfo), 140),
		(Business.MessageSendingObject.Schema.DutyAmount, typeof(ZCalcEditColumnStyleInfo), 100),
		(Business.MessageSendingObject.Schema.Currency, typeof(ZTextBoxColumnStyleInfo), 100),
	};

	protected override Type GridBoundEntityType => typeof(Business.MessageSendingObject);
}
