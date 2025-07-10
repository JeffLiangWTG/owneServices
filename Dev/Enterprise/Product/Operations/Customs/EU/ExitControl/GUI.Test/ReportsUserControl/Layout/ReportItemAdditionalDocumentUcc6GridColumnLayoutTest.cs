using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

sealed class ReportItemAdditionalDocumentUcc6GridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ReportItemAdditionalDocumentUcc6GridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(AdditionalInfo.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyleInfo), 75),
		(AdditionalInfo.Schema.CSI_SubType, typeof(ZDropEditColumnStyleInfo), 80),
		(AdditionalInfo.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo), 80),
		(AdditionalInfo.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 131),
		(AdditionalInfo.Schema.CSI_Status, typeof(ZDropEditColumnStyleInfo), 75),
	};

	protected override Type GridBoundEntityType => typeof(AdditionalInfo);
}
