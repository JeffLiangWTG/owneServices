using System;
using System.Collections.Generic;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	sealed class G3MessageSendingColumnLayoutTest : GridColumnLayoutProviderAbstractTest<G3MessageSendingColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(AutoMessageSendingObject.Schema.BillNumber, typeof(ZTextBoxColumnStyleInfo), 140),
			(nameof(AutoMessageSendingObject.ShouldSend), typeof(ZCheckBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.Action, typeof(ZDropEditColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.MessageStatus, typeof(ZTextBoxColumnStyleInfo), 140),
			(nameof(G3MessageSendingObject.G3LocalReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
			(nameof(G3MessageSendingObject.G3MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
			(nameof(G3MessageSendingObject.H7MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.EntryStatus, typeof(ZTextBoxColumnStyleInfo), 100),
			(AutoMessageSendingObject.Schema.RevokeReason, typeof(ZDropEditColumnStyleInfo), 100),
			(AutoMessageSendingObject.Schema.RevokeReasonDescription, typeof(ZTextBoxColumnStyleInfo), 140),
		};

		protected override Type GridBoundEntityType => typeof(G3MessageSendingObject);
	}
}
