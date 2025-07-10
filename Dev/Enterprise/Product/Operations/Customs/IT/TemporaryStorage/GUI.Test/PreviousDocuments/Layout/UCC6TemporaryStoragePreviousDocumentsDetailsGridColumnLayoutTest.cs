using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<UCC6TemporaryStoragePreviousDocumentsDetailsGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		("CSI_Code", typeof(ZCodeFindBoxColumnStyleInfo), 40),
		("CSI_ReferenceNumber", typeof(ZTextBoxColumnStyleInfo), 100),
		("CSI_LineNo", typeof(ZCalcEditColumnStyleInfo), 110),
		("CSI_PackQty", typeof(ZCalcEditColumnStyleInfo), 80),
		("CSI_PackType", typeof(ZDropEditColumnStyleInfo), 80),
		("CSI_Quantity", typeof(ZCalcEditColumnStyleInfo), 80),
		("CSI_UnitOfQuantity", typeof(ZDropEditColumnStyleInfo), 80)
	};

	protected override Type GridBoundEntityType => typeof(TemporaryStoragePreviousDocument);
}
