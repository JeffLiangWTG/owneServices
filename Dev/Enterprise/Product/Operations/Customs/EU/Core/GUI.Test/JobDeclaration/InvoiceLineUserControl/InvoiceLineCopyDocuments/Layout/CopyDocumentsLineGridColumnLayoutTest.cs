using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.GUI.Testing
{
	internal class CopyDocumentsLineGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<CopyDocumentsLineGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new []
		{
			(AutoCopyDocumentsSelectionLine.Schema.CSI_Type, typeof(ZTextBoxColumnStyleInfo), 80),
			(AutoCopyDocumentsSelectionLine.Schema.CSI_SubType, typeof(ZTextBoxColumnStyleInfo), 80),
			(AutoCopyDocumentsSelectionLine.Schema.CSI_Code, typeof(ZTextBoxColumnStyleInfo), 80),
			(AutoCopyDocumentsSelectionLine.Schema.CSI_ReferenceNumber, typeof(ZTextBoxColumnStyleInfo), 120),
			(AutoCopyDocumentsSelectionLine.Schema.CSI_ReferenceNumber2, typeof(ZTextBoxColumnStyleInfo), 120),
			(AutoCopyDocumentsSelectionLine.Schema.CSI_Description, typeof(ZTextBoxColumnStyleInfo), 120),
			(AutoCopyDocumentsSelectionLine.Schema.IsSelected, typeof(ZCheckBoxColumnStyleInfo), 40),
		};

		protected override Type GridBoundEntityType => typeof(CopyDocumentsSelectionLine);
	}
}
