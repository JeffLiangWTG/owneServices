using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	sealed class SealsGridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<SealsGridColumnLayout>
	{
		protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
		{
			(CusExitSeal.Schema.BK_SealNumber, typeof(ZTextBoxColumnStyleInfo), 120)
		};

		protected override Type GridBoundEntityType => typeof(CusExitSeal);
	}
}
