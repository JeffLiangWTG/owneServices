using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing;

[TestedType(typeof(DocumentRequestGridColumnLayout))]
sealed class DocumentRequestGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<DocumentRequestGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(nameof(AutoMessageSendingObject.ShouldSend), typeof(ZCheckBoxColumnStyleInfo), 140),
		(AutoMessageSendingObject.Schema.BillNumber, typeof(ZTextBoxColumnStyleInfo), 140),
		(nameof(Business.DocumentRequestSendingAction.G3LocalReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
		(nameof(Business.DocumentRequestSendingAction.G3MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
		(nameof(Business.DocumentRequestSendingAction.H7MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 140),
		(AutoMessageSendingObject.Schema.MessageStatus, typeof(ZTextBoxColumnStyleInfo), 140),
		(AutoMessageSendingObject.Schema.CustomsStatus, typeof(ZTextBoxColumnStyleInfo), 140),
	};

	protected override Type GridBoundEntityType => typeof(Business.DocumentRequestSendingAction);
}
