using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(NonUCC6PreviousDocumentGridColumnLayout))]
	sealed class NonUCC6PreviousDocumentGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<NonUCC6PreviousDocumentGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_SubType, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),
			(PreviousDocument.Schema.CSI_DateOfIssue, typeof(ZDateEditColumnStyleInfo), 120),
			(PreviousDocument.Schema.CSI_LineNo, typeof(ZCalcEditColumnStyleInfo), 80),
		};

		protected override Type GridBoundEntityType => typeof(PreviousDocument);
	}
}
