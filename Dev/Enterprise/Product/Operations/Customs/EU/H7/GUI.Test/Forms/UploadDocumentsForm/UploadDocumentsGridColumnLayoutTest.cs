using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	[TestedType(typeof(UploadDocumentsGridColumnLayout))]
	class UploadDocumentsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<UploadDocumentsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(nameof(AutoMessageSendingObject.ShouldSend), typeof(ZCheckBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.BillNumber, typeof(ZTextBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.Action, typeof(ZDropEditColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.LocalReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 140),
			(AutoMessageSendingObject.Schema.MRN, typeof(ZTextBoxColumnStyleInfo), 150),
			(AutoMessageSendingObject.Schema.CustomsStatus, typeof(ZTextBoxColumnStyleInfo), 100),
		};

		protected override Type GridBoundEntityType => typeof(UploadDocumentsSendingAction);
	}
}
