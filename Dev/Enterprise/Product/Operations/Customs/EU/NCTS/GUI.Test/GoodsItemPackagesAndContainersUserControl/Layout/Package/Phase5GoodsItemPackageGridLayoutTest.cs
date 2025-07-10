using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemPackageGridLayoutTest : GridColumnLayoutProviderAbstractTest<Phase5GoodsItemPackageGridLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new (string, Type, int)[]
		{
			(NctsPackage.Schema.B5_UnitType, typeof(ZDropEditColumnStyleInfo), 60),
			(NctsPackage.Schema.B5_UnitCount, typeof(ZCalcEditColumnStyleInfo), 80),
			(NctsPackage.Schema.B5_MarksAndNumbers, typeof(ZTextBoxColumnStyleInfo), 200),
		};

		protected override Type GridBoundEntityType => typeof(NctsPackage);
	}
}
