using System;
using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IE.GUI.Testing
{
	sealed class RefundApplicationSendingMessageGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<RefundApplicationSendingGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(RefundApplicationMessageSendingAction.SchemaShouldSend, typeof(ZCheckBoxColumnStyleInfo), 40),
			(nameof(RefundApplicationMessageSendingAction.MovementReferenceNumber), typeof(ZTextBoxColumnStyleInfo), 160),
			(nameof(RefundApplicationMessageSendingAction.RefundType), typeof(ZDropEditColumnStyleInfo), 50),
			(nameof(RefundApplicationMessageSendingAction.OfficeOfDebt), typeof(ZCodeFindBoxColumnStyleInfo ), 160),
			(nameof(RefundApplicationMessageSendingAction.OfficeOfResponsibility), typeof(ZCodeFindBoxColumnStyleInfo), 160),
			(nameof(RefundApplicationMessageSendingAction.LegalBasis), typeof(ZDropEditColumnStyleInfo), 160),
			(nameof(RefundApplicationMessageSendingAction.DescriptionOfGrounds), typeof(ZTextBoxColumnStyleInfo), 160),
			(nameof(RefundApplicationMessageSendingAction.BankDetails), typeof(ZTextBoxColumnStyleInfo), 160),
			(nameof(RefundApplicationMessageSendingAction.Amount), typeof(ZCalcEditColumnStyleInfo), 100),
			(nameof(RefundApplicationMessageSendingAction.AdditionalInformation), typeof(ZTextBoxColumnStyleInfo), 160),
		};

		protected override Type GridBoundEntityType => typeof(RefundApplicationMessageSendingAction);
	}
}
