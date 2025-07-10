using System;
using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class DocumentMessageSendingGridColumnLayoutTest	: GridColumnLayoutProviderAbstractTest<DocumentMessageSendingGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new []
		{
			(DocumentsSendingAction.SchemaShouldSend, typeof(ZCheckBoxColumnStyleInfo), 40),
			(nameof(DocumentsSendingAction.LocalReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 250),
			(nameof(DocumentsSendingAction.MovementReference), typeof(ZTextBoxColumnStyleInfo), 250),
		};

		protected override Type GridBoundEntityType => typeof(DocumentsSendingAction);
	}
}
