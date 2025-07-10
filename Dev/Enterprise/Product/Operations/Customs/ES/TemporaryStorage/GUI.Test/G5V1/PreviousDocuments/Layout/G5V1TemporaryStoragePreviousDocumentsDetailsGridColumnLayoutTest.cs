using System;
using System.Collections.Generic;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI.Testing;

sealed class G5V1TemporaryStoragePreviousDocumentsDetailsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<G5V1TemporaryStoragePreviousDocumentsDetailsGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		("CSI_Code", typeof(ZCodeFindBoxColumnStyleInfo), 40),
		("CSI_ReferenceNumber", typeof(ZTextBoxColumnStyleInfo), 100),
		("CSI_LineNo", typeof(ZCalcEditColumnStyleInfo), 110),
		("CSI_ReferenceNumber2", typeof(ZTextBoxColumnStyleInfo), 80)
	};

	protected override Type GridBoundEntityType => typeof(TemporaryStoragePreviousDocument);
}
