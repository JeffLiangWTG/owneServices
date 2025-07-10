using System;
using System.Collections.Generic;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStoragePackedItemGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<UCC6TemporaryStoragePackedItemGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		("API_LineNo", typeof(ZCalcEditColumnStyleInfo), 80),
		("API_FormattedTariff", typeof(Universal.GUI.TariffColumnStyleInfo), 80),
		("API_GoodsDescription", typeof(ZTextBoxColumnStyleInfo), 120),
		("API_GrossWeight", typeof(ZCalcEditColumnStyleInfo), 80),
		("API_GrossWeightUQ", typeof(ZDropEditColumnStyleInfo), 80),
		("API_NetWeight", typeof(ZCalcEditColumnStyleInfo), 80),
		("API_NetWeightUQ", typeof(ZDropEditColumnStyleInfo), 80),
		("API_CustomsQty2", typeof(ZCalcEditColumnStyleInfo), 80),
		("API_CustomsUQ2", typeof(ZDropEditColumnStyleInfo), 80),
		("API_ChemicalSubstanceCode", typeof(ZCodeFindBoxColumnStyleInfo), 40),
		("RegistrationNo", typeof(ZTextBoxColumnStyleInfo), 120),
		("ReleaseDate", typeof(ZDateEditColumnStyleInfo), 120)
	};

	protected override Type GridBoundEntityType => typeof(TemporaryStoragePackedItem);
}
