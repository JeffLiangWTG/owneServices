using System;
using System.Collections.Generic;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class PreviousDocumentsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<PreviousDocumentsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(PreviousDocument.Schema.CSI_Code, typeof(ZCodeFindBoxColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),
		};

		protected override Type GridBoundEntityType => typeof(PreviousDocument);
	}
}
