using System;
using System.Collections.Generic;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class ContainersOrEquipmentsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ContainersOrEquipmentsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(CusExitContainer.Schema.CXN_ContainerNumber, typeof(ZTextBoxColumnStyleInfo), 120),
			(CusExitContainer.Schema.CXN_IsEquipment, typeof(ZCheckBoxColumnStyleInfo), 87),
		};

		protected override Type GridBoundEntityType => typeof(CusExitContainer);
	}
}
