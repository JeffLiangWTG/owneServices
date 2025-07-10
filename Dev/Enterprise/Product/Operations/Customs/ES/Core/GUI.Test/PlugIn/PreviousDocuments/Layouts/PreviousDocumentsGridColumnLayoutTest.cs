using System;
using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ES.GUI.Testing;

public class PreviousDocumentsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<PreviousDocumentsGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns
	{
		get
		{
			return new[]
			{
				(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), 120),
				(PreviousDocument.Schema.CSI_SubType, typeof(ZCodeFindBoxColumnStyleInfo), 120),
				(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),
				(PreviousDocument.Schema.CSI_RN_NKCountryCode, typeof(ZCodeFindBoxColumnStyleInfo), 120),
			};
		}
	}

	protected override Type GridBoundEntityType => typeof(PreviousDocument);
}
