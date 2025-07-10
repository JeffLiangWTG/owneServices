using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		("CSI_Code", typeof(ZCodeFindBoxColumnStyleInfo), 40),
		("CSI_ReferenceNumber", typeof(ZTextBoxColumnStyleInfo), 100),
		("CSI_LineNo", typeof(ZCalcEditColumnStyleInfo), 110)
	};

	protected override Type GridBoundEntityType => typeof(TemporaryStoragePreviousDocument);
}
