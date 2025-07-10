using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class AdditionalSealsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<AdditionalSealsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new (string, Type, int)[]
		{
			(CusSeal.Schema.BK_SealNumber, typeof(ZTextBoxColumnStyleInfo), 135),
		};

		protected override Type GridBoundEntityType => typeof(CusSeal);
	}
}
