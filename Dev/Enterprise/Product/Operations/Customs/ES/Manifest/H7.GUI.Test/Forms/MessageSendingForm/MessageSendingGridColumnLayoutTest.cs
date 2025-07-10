using System;
using System.Collections.Generic;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	internal class MessageSendingColumnLayoutTest : GridColumnLayoutProviderAbstractTest<MessageSendingGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(nameof(AutoMessageSendingObject.ShouldSend), typeof(ZCheckBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.BillNumber, typeof(ZTextBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.Action, typeof(ZDropEditColumnStyleInfo), 140),
			(nameof(H7MessageSendingObject.G3LocalReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
			(nameof(H7MessageSendingObject.G3MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
			(nameof(H7MessageSendingObject.H7MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.MessageStatus, typeof(ZTextBoxColumnStyleInfo), 100),
			(AutoMessageSendingObject.Schema.CustomsStatus, typeof(ZTextBoxColumnStyleInfo), 100),
			(AutoMessageSendingObject.Schema.OperationCode, typeof(ZDropEditColumnStyleInfo), 140),
		};

		protected override Type GridBoundEntityType => typeof(H7MessageSendingObject);
	}
}
