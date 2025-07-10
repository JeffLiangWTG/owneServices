using System;
using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class DocumentSendingSupportingDocumentGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<DocumentSendingSupportingDocumentGridColumnLayout>
	{
		 protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new []
		 {
			 (DocumentSendingObject.Schema.EDoc, typeof(ZGuidDropEditColumnStyleInfo), 200),
			 (nameof(DocumentSendingObject.FileName), typeof(ZTextBoxColumnStyleInfo), 100),
			 (nameof(DocumentSendingObject.FileSizeInKB), typeof(ZTextBoxColumnStyleInfo), 80),
			 (nameof(DocumentSendingObject.FileDescription), typeof(ZTextBoxColumnStyleInfo), 200),
		 };

		 protected override Type GridBoundEntityType => typeof(DocumentSendingObject);
	}
}
