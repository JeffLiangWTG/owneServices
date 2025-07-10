using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class UnloadingDifferencesSupportingDocumentsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<UnloadingDifferencesSupportingDocumentsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new (string, Type, int)[]
		{
			(NctsSupportingDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyleInfo), 80),
			(NctsSupportingDocument.Schema.CSI_Status, typeof(ZDropEditColumnStyleInfo), 95),
			(NctsSupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo), 80),
			(NctsSupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 150),
			(NctsSupportingDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 200),
		};

		protected override Type GridBoundEntityType => typeof(NctsSupportingDocument);
	}
}
