using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ReportItemsAdditionalDocumentGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ReportItemAdditionalDocumentGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(AdditionalInfo.Schema.CSI_SubType, typeof(ZDropEditColumnStyleInfo), 80),
			(AdditionalInfo.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo), 80),
			(AdditionalInfo.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 131),
		};

		protected override Type GridBoundEntityType => typeof(AdditionalInfo);
	}
}
