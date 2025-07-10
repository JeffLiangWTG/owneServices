using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStoragePackedItemGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<UCC6TemporaryStoragePackedItemGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns =>
	[
		("API_LineNo", typeof(ZCalcEditColumnStyleInfo), 80),
		("API_FormattedTariff", typeof(Universal.GUI.TariffColumnStyleInfo), 80),
		("API_GoodsDescription", typeof(ZTextBoxColumnStyleInfo), 120),
		("API_GrossWeight", typeof(ZCalcEditColumnStyleInfo), 80),
		("API_GrossWeightUQ", typeof(ZDropEditColumnStyleInfo), 80),
		("API_ChemicalSubstanceCode", typeof(ZCodeFindBoxColumnStyleInfo), 40),
	];

	protected override Type GridBoundEntityType => typeof(TemporaryStoragePackedItem);
}
