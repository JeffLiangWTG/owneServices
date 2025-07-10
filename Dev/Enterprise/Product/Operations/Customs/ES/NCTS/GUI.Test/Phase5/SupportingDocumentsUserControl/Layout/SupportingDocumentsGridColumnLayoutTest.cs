using System;
using System.Collections.Generic;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class SupportingDocumentsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<SupportingDocumentsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new (string, Type, int)[]
		{
			(NctsSupportingDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 80),
			(NctsSupportingDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo), 80),
			(NctsSupportingDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 200),
			(NctsSupportingDocument.Schema.CSI_ItemNumber, typeof(ZCalcEditColumnStyleInfo), 80),
			(NctsSupportingDocument.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 200),
		};

		protected override Type GridBoundEntityType => typeof(NctsSupportingDocument);
	}
}
