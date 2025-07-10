using System;
using System.Collections.Generic;
using Enterprise.Customs.IE.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.GUI.Testing
{
	[TestedType(typeof(RF415MessageSendingGridColumnLayout))]
	sealed class RF415MessageSendingGridColumnLayoutProviderTest : GridColumnLayoutProviderAbstractTest<RF415MessageSendingGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new (string, Type, int)[]
		{
			(RF415MessageSendingObject.SchemaShouldSend, typeof(ZCheckBoxColumnStyleInfo), 40),
			(RF415MessageSendingObject.Schema.MovementReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 160),
			(RF415MessageSendingObject.Schema.BillNumber, typeof(ZTextBoxColumnStyleInfo), 160),
			(RF415MessageSendingObject.Schema.RefundType, typeof(ZDropEditColumnStyleInfo), 50),
			(RF415MessageSendingObject.Schema.OfficeOfDebt, typeof(ZCodeFindBoxColumnStyleInfo), 160),
			(RF415MessageSendingObject.Schema.OfficeOfResponsibility, typeof(ZCodeFindBoxColumnStyleInfo), 160),
			(RF415MessageSendingObject.Schema.LegalBasis, typeof(ZDropEditColumnStyleInfo), 160),
			(RF415MessageSendingObject.Schema.DescriptionOfGrounds, typeof(ZTextBoxColumnStyleInfo), 160),
			(RF415MessageSendingObject.Schema.BankDetails, typeof(ZTextBoxColumnStyleInfo), 160),
			(RF415MessageSendingObject.Schema.Amount, typeof(ZCalcEditColumnStyleInfo), 100),
			(RF415MessageSendingObject.Schema.AdditionalInformation, typeof(ZTextBoxColumnStyleInfo), 160),
		};

		protected override Type GridBoundEntityType => typeof(RF415MessageSendingObject);
	}
}
