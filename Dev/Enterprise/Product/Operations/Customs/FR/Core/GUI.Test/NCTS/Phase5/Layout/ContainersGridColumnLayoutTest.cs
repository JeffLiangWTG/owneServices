using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	sealed class ContainersGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<ContainersGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new (string, Type, int)[]
		{
			(NctsDepartureHeaderContainer.Schema.BC_RC, typeof(ZGuidFindBoxColumnStyleInfo), 100),
			(NctsDepartureHeaderContainer.Schema.BC_Mode, typeof(ZDropEditColumnStyleInfo), 100),
			(NctsDepartureHeaderContainer.Schema.BC_ContainerNum, typeof(ZTextBoxColumnStyleInfo), 175),
			(nameof(NctsDepartureHeaderContainer.Seal1), typeof(ZTextBoxColumnStyleInfo), 110),
			(nameof(NctsDepartureHeaderContainer.Seal2), typeof(ZTextBoxColumnStyleInfo), 110),
			(nameof(NctsDepartureHeaderContainer.TotalSealCount), typeof(ZCalcEditColumnStyleInfo), 95)
		};

		protected override Type GridBoundEntityType => typeof(NctsDepartureHeaderContainer);
	}
}
