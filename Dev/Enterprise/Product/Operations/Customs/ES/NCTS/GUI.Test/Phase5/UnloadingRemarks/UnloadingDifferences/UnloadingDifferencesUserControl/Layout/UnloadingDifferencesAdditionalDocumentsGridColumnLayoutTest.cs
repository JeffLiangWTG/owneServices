using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class UnloadingDifferencesAdditionalDocumentsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<UnloadingDifferencesAdditionalDocumentsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new (string, Type, int)[]
		{
			(NctsAdditionalInfo.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 80),
			(NctsAdditionalInfo.Schema.CSI_Status, typeof(ZDropEditColumnStyleInfo), 95),
			(NctsAdditionalInfo.Schema.CSI_SubType, typeof(ZDropEditColumnStyleInfo), 80),
			(NctsAdditionalInfo.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo), 80),
			(NctsAdditionalInfo.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 150),
			(NctsAdditionalInfo.Schema.CSI_Description, typeof(ZTextBoxColumnStyleInfo), 200),
		};

		protected override Type GridBoundEntityType => typeof(NctsAdditionalInfo);
	}
}
