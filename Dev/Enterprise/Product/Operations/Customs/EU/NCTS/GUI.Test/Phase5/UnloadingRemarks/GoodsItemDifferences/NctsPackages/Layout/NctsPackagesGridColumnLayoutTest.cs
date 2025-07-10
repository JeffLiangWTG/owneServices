using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class NctsPackagesGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<NctsPackagesGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new (string, Type, int)[]
		{
			(NctsPackage.Schema.B5_SequenceNumber, typeof(ZCalcEditColumnStyleInfo), 80),
			(NctsPackage.Schema.B5_TypeOfDifference, typeof(ZDropEditColumnStyleInfo), 80),
			(NctsPackage.Schema.B5_UnitCount, typeof(ZCalcEditColumnStyleInfo), 80),
			(NctsPackage.Schema.B5_UnitType, typeof(ZDropEditColumnStyleInfo), 80),
			(NctsPackage.Schema.B5_MarksAndNumbers, typeof(ZTextBoxColumnStyleInfo), 80),
		};

		protected override Type GridBoundEntityType => typeof(NctsPackage);
	}
}
