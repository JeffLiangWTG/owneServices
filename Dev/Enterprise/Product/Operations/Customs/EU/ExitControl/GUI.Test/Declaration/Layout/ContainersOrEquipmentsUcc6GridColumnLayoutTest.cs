using System;
using System.Collections.Generic;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;
sealed class ContainersOrEquipmentsUcc6GridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ContainersOrEquipmentsUcc6GridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(CusExitContainer.Schema.CXN_Sequence, typeof(ZCalcEditColumnStyleInfo), 75),
		(CusExitContainer.Schema.CXN_ContainerNumber, typeof(ZTextBoxColumnStyleInfo), 120),
		(CusExitContainer.Schema.CXN_IsEquipment, typeof(ZCheckBoxColumnStyleInfo), 87),
		(CusExitContainer.Schema.CXN_Status, typeof(ZDropEditColumnStyleInfo), 75),
		(CusExitContainer.Schema.CXN_SealCount, typeof(ZCalcEditColumnStyleInfo), 75),
	};

	protected override Type GridBoundEntityType => typeof(CusExitContainer);
}
