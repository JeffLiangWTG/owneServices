using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	class MessageSendingGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<MessageSendingGridColumnLayout>
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
			(AutoMessageSendingObject.Schema.QueryType, typeof(ZDropEditColumnStyleInfo), 180),
			(AutoMessageSendingObject.Schema.AmendmentReasonCode, typeof(ZDropEditColumnStyleInfo), 100),
			(AutoMessageSendingObject.Schema.AmendmentInvalidationReason, typeof(ZTextBoxColumnStyleInfo), 180),
			(AutoMessageSendingObject.Schema.EntryType, typeof(ZTextBoxColumnStyleInfo), 150),
			(AutoMessageSendingObject.Schema.ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 150),
		};

		protected override Type GridBoundEntityType => typeof(MessageSendingObject);
	}
}
