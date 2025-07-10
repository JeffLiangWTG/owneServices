using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing;

sealed class UCC6TemporaryStorageBillGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<UCC6TemporaryStorageBillGridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		("ABL_Calc_IsMaster", typeof(ZCheckBoxColumnStyleInfo), 80),
		("TypeOfBillDocument", typeof(ZDropEditColumnStyleInfo), 80),
		("ABL_BillNumber", typeof(ZTextBoxColumnStyleInfo), 80),
		("ABL_UCRNumber", typeof(ZTextBoxColumnStyleInfo), 80),
		("ABL_GrossWeight", typeof(ZCalcEditColumnStyleInfo), 80),
		("ABL_GrossWeightUQ", typeof(ZDropEditColumnStyleInfo), 80),
		("ConsignorOrgPK", typeof(ZOrganisationFindBoxColumnStyleInfo), 96),
		("ABL_OA_Shipper", typeof(ZAddressDropEditColumnStyleInfo), 109),
		("ConsigneeOrgPK", typeof(ZOrganisationFindBoxColumnStyleInfo), 97),
		("ABL_OA_Consignee", typeof(ZAddressDropEditColumnStyleInfo), 111)
	};
	protected override Type GridBoundEntityType => typeof(TemporaryStorageBill);
}
