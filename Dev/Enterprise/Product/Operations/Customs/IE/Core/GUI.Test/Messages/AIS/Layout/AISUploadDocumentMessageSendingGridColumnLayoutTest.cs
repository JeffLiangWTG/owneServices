using System;
using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class AISUploadDocumentMessageSendingGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<AISUploadDocumentMessageSendingGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(UploadDocumentsSendingAction.SchemaShouldSend, typeof(ZCheckBoxColumnStyleInfo), 40),
			(nameof(UploadDocumentsSendingAction.MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 160),
			(nameof(UploadDocumentsSendingAction.LocalReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 160),
			(nameof(UploadDocumentsSendingAction.MessageTypeForDisplay), typeof(ZDropEditColumnStyleInfo), 80),
			(nameof(UploadDocumentsSendingAction.MessageTypeDescription), typeof(ZDropEditColumnStyleInfo), 160),
			(nameof(UploadDocumentsSendingAction.EntryStatus), typeof(ZTextBoxColumnStyleInfo), 160),
		};

		protected override Type GridBoundEntityType => typeof(UploadDocumentsSendingAction);
	}
}
