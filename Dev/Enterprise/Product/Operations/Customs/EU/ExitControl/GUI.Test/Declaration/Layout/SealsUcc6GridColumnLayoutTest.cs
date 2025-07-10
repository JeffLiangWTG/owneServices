using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;
sealed class SealsUcc6GridColumnLayoutTest : GridColumnLayoutProviderAbstractTest<SealsUcc6GridColumnLayout>
{
	protected override IReadOnlyCollection<(string, Type, int)> ExpectedColumns => new[]
	{
		(CusExitSeal.Schema.BK_SequenceNumber, typeof(ZCalcEditColumnStyleInfo), 75),
		(CusExitSeal.Schema.BK_SealNumber, typeof(ZTextBoxColumnStyleInfo), 120),
		(CusExitSeal.Schema.BK_UnloadingState, typeof(ZDropEditColumnStyleInfo), 75)
	};

	protected override Type GridBoundEntityType => typeof(CusExitSeal);
}
