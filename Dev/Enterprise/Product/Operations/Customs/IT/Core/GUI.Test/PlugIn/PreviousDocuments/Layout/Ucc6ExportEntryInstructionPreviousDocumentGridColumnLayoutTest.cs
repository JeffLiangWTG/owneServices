using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(Ucc6ExportEntryInstructionPreviousDocumentGridColumnLayout))]
sealed class Ucc6ExportEntryInstructionPreviousDocumentGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<Ucc6ExportEntryInstructionPreviousDocumentGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
			(PreviousDocument.Schema.CSI_Code, typeof(ZDropEditColumnStyleInfo), 80),
			(PreviousDocument.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),
	};

	protected override Type GridBoundEntityType => typeof(PreviousDocument);
}
