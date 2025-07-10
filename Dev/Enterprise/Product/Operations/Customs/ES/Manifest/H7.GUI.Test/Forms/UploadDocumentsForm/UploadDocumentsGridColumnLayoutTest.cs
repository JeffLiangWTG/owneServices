using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(UploadDocumentsGridColumnLayout))]
	sealed class UploadDocumentsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<UploadDocumentsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(nameof(AutoMessageSendingObject.ShouldSend), typeof(ZCheckBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.BillNumber, typeof(ZTextBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.Action, typeof(ZDropEditColumnStyleInfo), 140),
			(nameof(Business.UploadDocumentsSendingAction.G3LocalReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
			(nameof(Business.UploadDocumentsSendingAction.G3MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
			(nameof(Business.UploadDocumentsSendingAction.H7MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.CustomsStatus, typeof(ZTextBoxColumnStyleInfo), 100),
			(Business.UploadDocumentsSendingAction.Schema.ClearanceRequested, typeof(ZCheckBoxColumnStyleInfo), 100),
		};

		protected override Type GridBoundEntityType => typeof(Business.UploadDocumentsSendingAction);
	}
}
