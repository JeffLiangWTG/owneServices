using System;
using System.Collections.Generic;
using Enterprise.Customs.FR.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.FR.H7.GUI.Testing
{
	sealed class MessageSendingGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<MessageSendingGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(nameof(MessageSendingObject.ShouldSend), typeof(ZCheckBoxColumnStyleInfo), 140),
			(MessageSendingObject.Schema.BillNumber, typeof(ZTextBoxColumnStyleInfo), 140),
			(MessageSendingObject.Schema.Action, typeof(ZDropEditColumnStyleInfo), 140),
			(MessageSendingObject.Schema.SubStyle, typeof(ZDropEditColumnStyleInfo), 100),
			(MessageSendingObject.Schema.LocalReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 140),
			(MessageSendingObject.Schema.MRN, typeof(ZTextBoxColumnStyleInfo), 150),
			(MessageSendingObject.Schema.MessageStatus, typeof(ZTextBoxColumnStyleInfo), 100),
			(MessageSendingObject.Schema.CustomsStatus, typeof(ZTextBoxColumnStyleInfo), 100),
			(MessageSendingObject.Schema.AmendmentInvalidationReason, typeof(ZMultiLineTextBoxColumnInfo), 200),
			(MessageSendingObject.Schema.Motivation, typeof(ZDropEditColumnStyleInfo), 100),
			(MessageSendingObject.Schema.ManifestLodgementDateTime, typeof(ZDateEditColumnStyleInfo), 150),
			(MessageSendingObject.Schema.FallbackReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 150),
		};

		protected override Type GridBoundEntityType => typeof(MessageSendingObject);
	}
}
